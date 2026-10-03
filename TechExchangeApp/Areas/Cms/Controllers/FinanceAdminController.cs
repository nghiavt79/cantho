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
