using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Services;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>
    /// Quản lý thu chi (HSMT A.6.1): tỷ lệ hoa hồng theo loại giao dịch, ghi nhận hoa hồng theo giá trị hợp đồng,
    /// sổ thu chi (khoản tự động từ gói gian hàng, gian hàng sự kiện + khoản nhập tay), báo cáo doanh thu, xuất Excel.
    /// </summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class FinanceAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "FinanceAdmin";
        private static readonly string[] LogFunctionAliases = { "Finance" };

        public FinanceAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() => int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;
        private Task WriteLog(int eventId, string content) =>
            CmsLogHelper.WriteLogAsync(_context, HttpContext, GetSiteId(), LogFunctionName, LogFunctionAliases, eventId, content);

        private IQueryable<FinanceEntry> Filter(DateTime from, DateTime to, string? category, byte? direction)
        {
            var q = _context.FinanceEntries.AsNoTracking().Where(e => e.EntryDate >= from && e.EntryDate <= to);
            if (!string.IsNullOrEmpty(category)) q = q.Where(e => e.Category == category);
            if (direction.HasValue) q = q.Where(e => e.Direction == direction);
            return q;
        }

        private static (DateTime From, DateTime To) Range(DateTime? from, DateTime? to)
        {
            var f = (from ?? new DateTime(DateTime.Today.Year, 1, 1)).Date;
            var t = (to ?? DateTime.Today).Date;
            return t < f ? (t, f) : (f, t);
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? from, DateTime? to, string? category, byte? direction)
        {
            var (f, t) = Range(from, to);
            var entries = await Filter(f, t, category, direction).OrderByDescending(e => e.EntryDate).ThenByDescending(e => e.Id).Take(1000).ToListAsync();
            var supplierIds = entries.Where(e => e.CungUngId.HasValue).Select(e => e.CungUngId!.Value).Distinct().ToList();
            ViewBag.SupplierNames = await _context.NhaCungUngs.AsNoTracking().Where(n => supplierIds.Contains(n.CungUngId))
                .ToDictionaryAsync(n => n.CungUngId, n => n.FullName ?? "");

            var all = await Filter(f, t, null, null).Select(e => new { e.EntryDate, e.Category, e.Direction, e.Amount }).ToListAsync();
            ViewBag.ByCategory = all.GroupBy(e => (e.Category, e.Direction))
                .Select(g => new FinanceSummaryRow { Key = FinanceCategories.Label(g.Key.Category), Direction = g.Key.Direction, Amount = g.Sum(x => x.Amount), Count = g.Count() })
                .OrderBy(r => r.Direction).ThenByDescending(r => r.Amount).ToList();
            ViewBag.ByMonth = all.GroupBy(e => new DateTime(e.EntryDate.Year, e.EntryDate.Month, 1))
                .Select(g => new FinanceMonthRow
                {
                    Month = g.Key,
                    Income = g.Where(x => x.Direction == FinanceDirections.Income).Sum(x => x.Amount),
                    Expense = g.Where(x => x.Direction == FinanceDirections.Expense).Sum(x => x.Amount)
                }).OrderBy(r => r.Month).ToList();
            ViewBag.TotalIncome = all.Where(x => x.Direction == FinanceDirections.Income).Sum(x => x.Amount);
            ViewBag.TotalExpense = all.Where(x => x.Direction == FinanceDirections.Expense).Sum(x => x.Amount);

            ViewBag.Rates = await _context.CommissionRates.AsNoTracking().OrderBy(r => r.Id).ToListAsync();
            ViewBag.Suppliers = await _context.NhaCungUngs.AsNoTracking().Where(n => n.LanguageId == 1).OrderBy(n => n.FullName)
                .Select(n => new { n.CungUngId, n.FullName }).ToListAsync();
            ViewBag.From = f; ViewBag.To = t; ViewBag.Category = category; ViewBag.Direction = direction;
            return View(entries);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRates(List<int> ids, List<decimal> rates, List<bool>? actives)
        {
            if (ids.Count != rates.Count) return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            if (rates.Any(r => r < 0 || r > 100)) return Json(new { success = false, message = "Tỷ lệ phải từ 0 đến 100%." });
            var rows = await _context.CommissionRates.Where(r => ids.Contains(r.Id)).ToListAsync();
            foreach (var r in rows) r.RatePercent = rates[ids.IndexOf(r.Id)];
            await _context.SaveChangesAsync();
            await WriteLog(2, "Cập nhật tỷ lệ hoa hồng: " + string.Join(", ", rows.Select(r => $"{r.Code}={r.RatePercent}%")));
            return Json(new { success = true });
        }

        /// <summary>Ghi nhận hoa hồng: số tiền = giá trị giao dịch × tỷ lệ của loại giao dịch.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCommission(int rateId, decimal baseAmount, int? cungUngId, DateTime? entryDate, string? description)
        {
            var rate = await _context.CommissionRates.FindAsync(rateId);
            if (rate == null || !rate.IsActive) return Json(new { success = false, message = "Loại giao dịch không hợp lệ." });
            if (baseAmount <= 0) return Json(new { success = false, message = "Nhập giá trị giao dịch." });
            if (rate.RatePercent <= 0) return Json(new { success = false, message = "Tỷ lệ hoa hồng của loại giao dịch này đang là 0%." });

            var amount = Math.Round(baseAmount * rate.RatePercent / 100m, 0);
            _context.FinanceEntries.Add(new FinanceEntry
            {
                EntryDate = (entryDate ?? DateTime.Today).Date, Direction = FinanceDirections.Income, Category = FinanceCategories.Commission,
                Amount = amount, CungUngId = cungUngId, RefType = FinanceRefTypes.Commission, BaseAmount = baseAmount, RatePercent = rate.RatePercent,
                Description = Trim($"{rate.Name}: {description}", 500), CreatedBy = User.Identity?.Name
            });
            await _context.SaveChangesAsync();
            await WriteLog(1, $"Ghi nhận hoa hồng {amount:N0} đ ({rate.Code} {rate.RatePercent}% × {baseAmount:N0})");
            return Json(new { success = true, message = $"Đã ghi nhận hoa hồng {amount:N0} đ." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEntry(byte direction, string category, decimal amount, int? cungUngId, DateTime? entryDate, string? description)
        {
            if (direction is not (FinanceDirections.Income or FinanceDirections.Expense)) return Json(new { success = false, message = "Chọn thu hoặc chi." });
            if (amount <= 0) return Json(new { success = false, message = "Số tiền phải lớn hơn 0." });
            if (category is not (FinanceCategories.StoreFee or FinanceCategories.EventBoothFee or FinanceCategories.Commission or FinanceCategories.Other))
                category = FinanceCategories.Other;
            if (string.IsNullOrWhiteSpace(description)) return Json(new { success = false, message = "Nhập nội dung khoản thu/chi." });

            _context.FinanceEntries.Add(new FinanceEntry
            {
                EntryDate = (entryDate ?? DateTime.Today).Date, Direction = direction, Category = category, Amount = amount,
                CungUngId = cungUngId, Description = Trim(description, 500), CreatedBy = User.Identity?.Name
            });
            await _context.SaveChangesAsync();
            await WriteLog(1, $"Nhập {(direction == 1 ? "thu" : "chi")} {amount:N0} đ – {category}");
            return Json(new { success = true });
        }

        /// <summary>Chỉ xóa được khoản nhập tay; khoản tự động gỡ bằng cách hủy gói / từ chối gian hàng ở nguồn.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            var e = await _context.FinanceEntries.FindAsync(id);
            if (e == null) return Json(new { success = false, message = "Không tìm thấy." });
            if (e.RefType is FinanceRefTypes.Subscription or FinanceRefTypes.BoothBooking)
                return Json(new { success = false, message = "Khoản này phát sinh tự động; hủy gói gian hàng hoặc đăng ký gian hàng sự kiện tương ứng để gỡ." });
            _context.FinanceEntries.Remove(e);
            await _context.SaveChangesAsync();
            await WriteLog(3, $"Xóa khoản thu chi #{id}: {e.Amount:N0} đ – {e.Description}");
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Export(DateTime? from, DateTime? to, string? category, byte? direction, [FromServices] IExcelExportService excel)
        {
            var (f, t) = Range(from, to);
            var rows = await Filter(f, t, category, direction).OrderBy(e => e.EntryDate).ThenBy(e => e.Id).ToListAsync();
            var ids = rows.Where(e => e.CungUngId.HasValue).Select(e => e.CungUngId!.Value).Distinct().ToList();
            var names = await _context.NhaCungUngs.AsNoTracking().Where(n => ids.Contains(n.CungUngId)).ToDictionaryAsync(n => n.CungUngId, n => n.FullName ?? "");
            var data = rows.Select((e, i) => new FinanceExportRow
            {
                Stt = i + 1, Ngay = e.EntryDate.ToString("dd/MM/yyyy"), Loai = e.Direction == FinanceDirections.Income ? "Thu" : "Chi",
                Nhom = FinanceCategories.Label(e.Category), SoTien = e.Amount, GiaTriGiaoDich = e.BaseAmount, TyLe = e.RatePercent,
                NhaCungCap = e.CungUngId.HasValue && names.TryGetValue(e.CungUngId.Value, out var n) ? n : null,
                NoiDung = e.Description, NguoiNhap = e.CreatedBy
            });
            await WriteLog(4, $"Xuất Excel sổ thu chi {f:dd/MM/yyyy}–{t:dd/MM/yyyy} ({rows.Count} dòng)");
            return excel.Export(data, $"SoThuChi_{f:yyyyMMdd}_{t:yyyyMMdd}.xlsx");
        }

        // ─── Xuất PDF sổ thu chi (HSMT A.6.1, nhóm 12) ───
        [HttpGet]
        public async Task<IActionResult> ExportPdf(DateTime? from, DateTime? to, string? category, byte? direction, [FromServices] ReportPdfService pdf)
        {
            var (f, t) = Range(from, to);
            var rows = await Filter(f, t, category, direction).OrderBy(e => e.EntryDate).ThenBy(e => e.Id).ToListAsync();
            var ids = rows.Where(e => e.CungUngId.HasValue).Select(e => e.CungUngId!.Value).Distinct().ToList();
            var names = await _context.NhaCungUngs.AsNoTracking().Where(n => ids.Contains(n.CungUngId)).ToDictionaryAsync(n => n.CungUngId, n => n.FullName ?? "");
            var data = rows.Select((e, i) => new FinanceExportRow
            {
                Stt = i + 1, Ngay = e.EntryDate.ToString("dd/MM/yyyy"), Loai = e.Direction == FinanceDirections.Income ? "Thu" : "Chi",
                Nhom = FinanceCategories.Label(e.Category), SoTien = e.Amount, GiaTriGiaoDich = e.BaseAmount, TyLe = e.RatePercent,
                NhaCungCap = e.CungUngId.HasValue && names.TryGetValue(e.CungUngId.Value, out var n) ? n : null,
                NoiDung = e.Description, NguoiNhap = e.CreatedBy
            }).ToList();
            var income = rows.Where(e => e.Direction == FinanceDirections.Income).Sum(e => e.Amount);
            var expense = rows.Where(e => e.Direction != FinanceDirections.Income).Sum(e => e.Amount);
            var summary = $"<p>Tổng thu: <b>{income:N0} đồng</b> · Tổng chi: <b>{expense:N0} đồng</b> · Chênh lệch: <b>{income - expense:N0} đồng</b> · Số khoản: {rows.Count}</p>";
            await WriteLog(4, $"Xuất PDF sổ thu chi {f:dd/MM/yyyy}–{t:dd/MM/yyyy} ({rows.Count} dòng)");
            return pdf.Build("SỔ THU CHI", $"Từ ngày {f:dd/MM/yyyy} đến ngày {t:dd/MM/yyyy}",
                new[] { new ReportPdfService.Section("", summary), new ReportPdfService.Section("", ReportPdfService.Table(data)) },
                $"SoThuChi_{f:yyyyMMdd}_{t:yyyyMMdd}.pdf");
        }

        // ─── Báo cáo giao dịch (HSMT A.8.1): hồ sơ chuyển giao công nghệ, đơn đặt mua, doanh thu ───
        [HttpGet]
        public async Task<IActionResult> Transactions(DateTime? from, DateTime? to, string? format, [FromServices] ReportPdfService pdf, [FromServices] IExcelExportService excel)
        {
            var (f, t) = Range(from, to);
            var tEnd = t.AddDays(1);

            var projects = await (from p in _context.Projects.AsNoTracking()
                                  join b in _context.Users.AsNoTracking() on p.CreatedBy equals b.Id into bj
                                  from b in bj.DefaultIfEmpty()
                                  join s in _context.Users.AsNoTracking() on p.SelectedSellerId equals s.Id into sj
                                  from s in sj.DefaultIfEmpty()
                                  where p.CreatedDate >= f && p.CreatedDate < tEnd
                                  orderby p.CreatedDate
                                  select new
                                  {
                                      p.Id, p.ProjectCode, p.ProjectName, p.StatusId, p.CreatedDate,
                                      Buyer = b != null ? (b.FullName ?? b.UserName) : null,
                                      Seller = s != null ? (s.FullName ?? s.UserName) : null,
                                      Done = _context.ProjectSteps.Count(x => x.ProjectId == p.Id && x.StatusId == 2),
                                      Current = _context.ProjectSteps.Where(x => x.ProjectId == p.Id && x.StatusId == 1).Select(x => (int?)x.StepNumber).Min()
                                  }).ToListAsync();
            var projRows = projects.Select((p, i) => new TransactionProjectRow
            {
                Stt = i + 1, Ma = p.ProjectCode, Ten = p.ProjectName, BenMua = p.Buyer, BenBan = p.Seller,
                Ngay = p.CreatedDate.ToString("dd/MM/yyyy"),
                Buoc = p.StatusId == 3 || p.Done >= 14 ? "Hoàn thành 14/14" : $"Đang ở bước {p.Current ?? p.Done + 1}/14",
                TrangThai = p.StatusId == 3 || p.Done >= 14 ? "Thành công" : p.Done == 0 ? "Mới tạo" : "Đang thực hiện"
            }).ToList();

            var orders = await (from o in _context.OcopOrderRequests.AsNoTracking()
                                join sp in _context.SanPhamCNTBs.AsNoTracking() on o.ProductId equals sp.ID into spj
                                from sp in spj.DefaultIfEmpty()
                                join n in _context.NhaCungUngs.AsNoTracking() on o.SupplierId equals n.CungUngId into nj
                                from n in nj.DefaultIfEmpty()
                                where o.NgayTao >= f && o.NgayTao < tEnd
                                orderby o.NgayTao
                                select new { o.Id, o.NgayTao, o.HoTen, o.SoLuong, o.StatusId, o.GiaTriDon, o.TrangThaiThanhToan, Sp = sp != null ? sp.Name : null, Ncc = n != null ? n.FullName : null })
                               .ToListAsync();
            var orderRows = orders.Select((o, i) => new TransactionOrderRow
            {
                Stt = i + 1, MaDon = OcopOrderRequest.FormatMaDon(o.Id, o.NgayTao), Ngay = o.NgayTao.ToString("dd/MM/yyyy"),
                SanPham = o.Sp, NhaCungCap = o.Ncc, NguoiMua = o.HoTen, SoLuong = o.SoLuong, GiaTri = o.GiaTriDon,
                ThanhToan = o.TrangThaiThanhToan == 1 ? "Đã thanh toán" : "Đang chờ",
                TrangThai = o.StatusId switch { 1 => "Mới gửi", 2 => "Đã xác nhận", 3 => "Hoàn tất", 4 => "Đã hủy", _ => "" }
            }).ToList();

            var vm = new TransactionReportVm
            {
                From = f, To = t, Projects = projRows, Orders = orderRows,
                ProjectsSucceeded = projRows.Count(p => p.TrangThai == "Thành công"),
                OrdersSucceeded = orders.Count(o => o.StatusId == 3),
                Revenue = orders.Where(o => o.StatusId != 4 && (o.StatusId == 3 || o.TrangThaiThanhToan == 1)).Sum(o => o.GiaTriDon ?? 0),
                Commission = await _context.FinanceEntries.AsNoTracking()
                    .Where(e => e.Direction == FinanceDirections.Income && e.EntryDate >= f && e.EntryDate <= t).SumAsync(e => (decimal?)e.Amount) ?? 0
            };

            if (format == "pdf")
            {
                var summary = $"<p>Hồ sơ chuyển giao công nghệ: {projRows.Count} (thành công {vm.ProjectsSucceeded}) · Đơn đặt mua: {orderRows.Count} (hoàn tất {vm.OrdersSucceeded}) · "
                            + $"Giá trị giao dịch thành công: <b>{vm.Revenue:N0} đồng</b> · Thu của Sàn (hoa hồng, gian hàng): <b>{vm.Commission:N0} đồng</b></p>";
                await WriteLog(4, $"Xuất PDF báo cáo giao dịch {f:dd/MM/yyyy}–{t:dd/MM/yyyy}");
                return pdf.Build("BÁO CÁO GIAO DỊCH", $"Từ ngày {f:dd/MM/yyyy} đến ngày {t:dd/MM/yyyy}", new[]
                {
                    new ReportPdfService.Section("", summary),
                    new ReportPdfService.Section("1. Hồ sơ chuyển giao công nghệ", ReportPdfService.Table(projRows)),
                    new ReportPdfService.Section("2. Đơn đặt mua sản phẩm", ReportPdfService.Table(orderRows))
                }, $"BaoCaoGiaoDich_{f:yyyyMMdd}_{t:yyyyMMdd}.pdf");
            }
            if (format == "excel")
            {
                await WriteLog(4, $"Xuất Excel báo cáo giao dịch {f:dd/MM/yyyy}–{t:dd/MM/yyyy}");
                return excel.Export(orderRows, $"DonDatMua_{f:yyyyMMdd}_{t:yyyyMMdd}.xlsx");
            }
            return View(vm);
        }

        private static string? Trim(string? s, int max) => s == null ? null : (s.Length > max ? s[..max] : s);
    }

    public class FinanceSummaryRow
    {
        public string Key { get; set; } = "";
        public byte Direction { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class FinanceMonthRow
    {
        public DateTime Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
    }

    public class TransactionReportVm
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<TransactionProjectRow> Projects { get; set; } = new();
        public List<TransactionOrderRow> Orders { get; set; } = new();
        public int ProjectsSucceeded { get; set; }
        public int OrdersSucceeded { get; set; }
        public decimal Revenue { get; set; }
        public decimal Commission { get; set; }
    }

    public class TransactionProjectRow
    {
        [Display(Name = "STT")] public int Stt { get; set; }
        [Display(Name = "Mã hồ sơ")] public string Ma { get; set; } = "";
        [Display(Name = "Tên hồ sơ")] public string Ten { get; set; } = "";
        [Display(Name = "Bên mua")] public string? BenMua { get; set; }
        [Display(Name = "Bên bán")] public string? BenBan { get; set; }
        [Display(Name = "Ngày tạo")] public string Ngay { get; set; } = "";
        [Display(Name = "Tiến độ")] public string Buoc { get; set; } = "";
        [Display(Name = "Trạng thái")] public string TrangThai { get; set; } = "";
    }

    public class TransactionOrderRow
    {
        [Display(Name = "STT")] public int Stt { get; set; }
        [Display(Name = "Mã đơn")] public string MaDon { get; set; } = "";
        [Display(Name = "Ngày đặt")] public string Ngay { get; set; } = "";
        [Display(Name = "Sản phẩm")] public string? SanPham { get; set; }
        [Display(Name = "Nhà cung cấp")] public string? NhaCungCap { get; set; }
        [Display(Name = "Người mua")] public string? NguoiMua { get; set; }
        [Display(Name = "SL")] public int SoLuong { get; set; }
        [Display(Name = "Giá trị (đ)")] public decimal? GiaTri { get; set; }
        [Display(Name = "Thanh toán")] public string ThanhToan { get; set; } = "";
        [Display(Name = "Trạng thái")] public string TrangThai { get; set; } = "";
    }

    public class FinanceExportRow
    {
        [Display(Name = "STT")] public int Stt { get; set; }
        [Display(Name = "Ngày")] public string Ngay { get; set; } = "";
        [Display(Name = "Thu/Chi")] public string Loai { get; set; } = "";
        [Display(Name = "Nhóm")] public string Nhom { get; set; } = "";
        [Display(Name = "Số tiền (đ)")] public decimal SoTien { get; set; }
        [Display(Name = "Giá trị giao dịch (đ)")] public decimal? GiaTriGiaoDich { get; set; }
        [Display(Name = "Tỷ lệ hoa hồng (%)")] public decimal? TyLe { get; set; }
        [Display(Name = "Nhà cung cấp")] public string? NhaCungCap { get; set; }
        [Display(Name = "Nội dung")] public string? NoiDung { get; set; }
        [Display(Name = "Người nhập")] public string? NguoiNhap { get; set; }
    }
}
