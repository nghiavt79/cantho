using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.BackgroundServices;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Services;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>
    /// Quản lý sự kiện (HSMT A.4.1, A.4.3, A.4.4, A.4.5): gắn thông tin sự kiện cho tin bài, danh sách đăng ký,
    /// xuất Excel, điểm danh bằng QR, thống kê lượt xem/quan tâm/nhắc lịch, loại gian hàng và duyệt đăng ký thuê.
    /// </summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class EventsAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "EventsAdmin";
        private static readonly string[] LogFunctionAliases = { "Events" };

        public EventsAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private Task WriteLog(int eventId, string content) =>
            CmsLogHelper.WriteLogAsync(_context, HttpContext, GetSiteId(), LogFunctionName, LogFunctionAliases, eventId, content);

        // ── Danh sách sự kiện + tìm tin bài để gắn sự kiện ──
        [HttpGet]
        public async Task<IActionResult> Index(string? find)
        {
            var events = await (from e in _context.EventInfos.AsNoTracking()
                                join c in _context.Contents.AsNoTracking() on e.ContentId equals c.Id
                                orderby e.StartAt descending
                                select new EventAdminRow
                                {
                                    ContentId = e.ContentId, Title = c.Title ?? "", StartAt = e.StartAt, Location = e.Location,
                                    Registered = _context.EventRegistrations.Count(r => r.ContentId == e.ContentId && r.Status != EventRegistrationStatuses.Cancelled),
                                    CheckedIn = _context.EventRegistrations.Count(r => r.ContentId == e.ContentId && r.Status == EventRegistrationStatuses.CheckedIn),
                                    Interest = _context.EventInteractions.Count(i => i.ContentId == e.ContentId && i.InteractionType == EventInteractionTypes.Interest),
                                    PendingBooths = _context.EventBoothBookings.Count(b => b.ContentId == e.ContentId && b.Status == EventBoothBookingStatuses.Pending)
                                }).ToListAsync();

            if (!string.IsNullOrWhiteSpace(find))
            {
                var eventIds = events.Select(e => e.ContentId).ToList();
                ViewBag.FindResults = await _context.Contents.AsNoTracking()
                    .Where(c => c.LanguageId == 1 && c.Title != null && c.Title.Contains(find) && !eventIds.Contains(c.Id))
                    .OrderByDescending(c => c.PublishedDate ?? c.Created).Take(20)
                    .Select(c => new { c.Id, c.Title, c.PublishedDate, c.StatusId }).ToListAsync();
            }
            ViewBag.Find = find;
            return View(events);
        }

        // ── Thiết lập thông tin sự kiện + loại gian hàng ──
        [HttpGet]
        public async Task<IActionResult> Edit(long contentId)
        {
            var post = await _context.Contents.AsNoTracking().Where(c => c.Id == contentId).Select(c => new { c.Id, c.Title, c.LanguageId }).FirstOrDefaultAsync();
            if (post == null) return NotFound();
            if (post.LanguageId != 1) return BadRequest("Thiết lập sự kiện trên tin bài tiếng Việt; bản tiếng Anh dùng chung.");

            var info = await _context.EventInfos.AsNoTracking().FirstOrDefaultAsync(e => e.ContentId == contentId);
            var vm = info == null
                ? new EventInfoFormVm { ContentId = contentId, StartAt = DateTime.Today.AddDays(7).AddHours(8) }
                : new EventInfoFormVm
                {
                    ContentId = contentId, StartAt = info.StartAt, EndAt = info.EndAt, Location = info.Location,
                    LivestreamUrl = info.LivestreamUrl, RegistrationEnabled = info.RegistrationEnabled,
                    RegistrationDeadline = info.RegistrationDeadline, MaxAttendees = info.MaxAttendees,
                    ReminderOffsetsMinutes = info.ReminderOffsetsMinutes,
                    RemindByEmail = (info.ReminderChannels & EventReminderChannels.Email) != 0,
                    RemindBySms = (info.ReminderChannels & EventReminderChannels.Sms) != 0
                };
            ViewBag.Title2 = post.Title;
            ViewBag.IsNew = info == null;
            ViewBag.Booths = await _context.EventBoothTypes.AsNoTracking().Where(b => b.ContentId == contentId).OrderBy(b => b.SortOrder).ThenBy(b => b.Id).ToListAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EventInfoFormVm vm)
        {
            if (vm.EndAt.HasValue && vm.EndAt < vm.StartAt) ModelState.AddModelError(nameof(vm.EndAt), "Thời gian kết thúc phải sau thời gian bắt đầu.");
            if (!string.IsNullOrWhiteSpace(vm.LivestreamUrl) &&
                !(Uri.TryCreate(vm.LivestreamUrl, UriKind.Absolute, out var u) && (u.Scheme == "https" || u.Scheme == "http")))
                ModelState.AddModelError(nameof(vm.LivestreamUrl), "Link livestream phải bắt đầu bằng http:// hoặc https://");
            var offsets = (vm.ReminderOffsetsMinutes ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (offsets.Any(o => !int.TryParse(o, out var n) || n <= 0))
                ModelState.AddModelError(nameof(vm.ReminderOffsetsMinutes), "Mốc nhắc là các số phút dương, cách nhau dấu phẩy (vd 1440,60).");

            if (!ModelState.IsValid)
            {
                ViewBag.Title2 = await _context.Contents.Where(c => c.Id == vm.ContentId).Select(c => c.Title).FirstOrDefaultAsync();
                ViewBag.IsNew = !await _context.EventInfos.AnyAsync(e => e.ContentId == vm.ContentId);
                ViewBag.Booths = await _context.EventBoothTypes.AsNoTracking().Where(b => b.ContentId == vm.ContentId).OrderBy(b => b.SortOrder).ToListAsync();
                return View(vm);
            }

            var info = await _context.EventInfos.FirstOrDefaultAsync(e => e.ContentId == vm.ContentId);
            var isNew = info == null;
            if (info == null)
            {
                if (!await _context.Contents.AnyAsync(c => c.Id == vm.ContentId && c.LanguageId == 1)) return NotFound();
                info = new EventInfo { ContentId = vm.ContentId, Created = DateTime.Now };
                _context.EventInfos.Add(info);
            }
            info.StartAt = vm.StartAt;
            info.EndAt = vm.EndAt;
            info.Location = vm.Location?.Trim();
            info.LivestreamUrl = string.IsNullOrWhiteSpace(vm.LivestreamUrl) ? null : vm.LivestreamUrl.Trim();
            info.RegistrationEnabled = vm.RegistrationEnabled;
            info.RegistrationDeadline = vm.RegistrationDeadline;
            info.MaxAttendees = vm.MaxAttendees is > 0 ? vm.MaxAttendees : null;
            info.ReminderOffsetsMinutes = string.Join(",", offsets.Select(int.Parse).Distinct().OrderByDescending(n => n));
            if (info.ReminderOffsetsMinutes.Length == 0) info.ReminderOffsetsMinutes = "1440,60";
            info.ReminderChannels = (byte)((vm.RemindByEmail ? EventReminderChannels.Email : 0) | (vm.RemindBySms ? EventReminderChannels.Sms : 0));
            info.Modified = DateTime.Now;
            info.Modifier = User.Identity?.Name;
            await _context.SaveChangesAsync();
            await WriteLog(isNew ? 1 : 2, $"{(isNew ? "Tạo" : "Sửa")} thông tin sự kiện cho tin bài #{vm.ContentId}");

            TempData["Success"] = "Đã lưu thông tin sự kiện.";
            return RedirectToAction(nameof(Edit), new { contentId = vm.ContentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBooth(EventBoothType input)
        {
            if (string.IsNullOrWhiteSpace(input.Name) || input.Quantity < 1 || input.Price < 0)
                return Json(new { success = false, message = "Nhập tên loại gian hàng, số lượng ≥ 1 và giá ≥ 0." });
            if (!await _context.EventInfos.AnyAsync(e => e.ContentId == input.ContentId))
                return Json(new { success = false, message = "Cần lưu thông tin sự kiện trước." });

            EventBoothType b;
            if (input.Id > 0)
            {
                b = await _context.EventBoothTypes.FirstOrDefaultAsync(x => x.Id == input.Id && x.ContentId == input.ContentId)
                    ?? throw new InvalidOperationException();
            }
            else
            {
                b = new EventBoothType { ContentId = input.ContentId };
                _context.EventBoothTypes.Add(b);
            }
            b.Name = input.Name.Trim();
            b.AreaM2 = input.AreaM2;
            b.Position = input.Position?.Trim();
            b.Price = input.Price;
            b.Quantity = input.Quantity;
            b.Services = input.Services?.Trim();
            b.SortOrder = input.SortOrder;
            b.IsActive = input.IsActive;
            await _context.SaveChangesAsync();
            await WriteLog(2, $"Lưu loại gian hàng #{b.Id} '{b.Name}' cho sự kiện #{b.ContentId}");
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBooth(int id)
        {
            var b = await _context.EventBoothTypes.FindAsync(id);
            if (b == null) return Json(new { success = false, message = "Không tìm thấy." });
            if (await _context.EventBoothBookings.AnyAsync(x => x.BoothTypeId == id))
            {
                b.IsActive = false; // đã có đăng ký thuê: chỉ ẩn để giữ lịch sử
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Loại gian hàng đã có đăng ký thuê nên chỉ được ẩn." });
            }
            _context.EventBoothTypes.Remove(b);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ── Quản lý: đăng ký / thống kê / gian hàng ──
        [HttpGet]
        public async Task<IActionResult> Manage(long contentId, string tab = "registrations", string? keyword = null, byte? status = null)
        {
            var info = await _context.EventInfos.AsNoTracking().FirstOrDefaultAsync(e => e.ContentId == contentId);
            if (info == null) return RedirectToAction(nameof(Edit), new { contentId });
            ViewBag.Info = info;
            ViewBag.Title2 = await _context.Contents.Where(c => c.Id == contentId).Select(c => c.Title).FirstOrDefaultAsync();
            ViewBag.Tab = tab;
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;

            var regs = _context.EventRegistrations.AsNoTracking().Where(r => r.ContentId == contentId);
            ViewBag.RegTotal = await regs.CountAsync(r => r.Status != EventRegistrationStatuses.Cancelled);
            ViewBag.RegCheckedIn = await regs.CountAsync(r => r.Status == EventRegistrationStatuses.CheckedIn);

            if (tab == "stats")
            {
                var since = DateTime.Today.AddDays(-29);
                var inter = await _context.EventInteractions.AsNoTracking().Where(i => i.ContentId == contentId)
                    .GroupBy(i => i.InteractionType).Select(g => new { g.Key, N = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.N);
                ViewBag.Views = inter.GetValueOrDefault(EventInteractionTypes.View);
                ViewBag.Interest = inter.GetValueOrDefault(EventInteractionTypes.Interest);
                ViewBag.Remind = inter.GetValueOrDefault(EventInteractionTypes.Remind);
                var daily = await _context.EventInteractions.AsNoTracking().Where(i => i.ContentId == contentId && i.Created >= since)
                    .GroupBy(i => new { i.Created.Date, i.InteractionType }).Select(g => new { g.Key.Date, g.Key.InteractionType, N = g.Count() }).ToListAsync();
                var dailyReg = await regs.Where(r => r.Created >= since).GroupBy(r => r.Created.Date).Select(g => new { Date = g.Key, N = g.Count() }).ToListAsync();
                ViewBag.Daily = Enumerable.Range(0, 30).Select(d => since.AddDays(d)).Select(d => new EventDailyStat
                {
                    Date = d,
                    Views = daily.Where(x => x.Date == d && x.InteractionType == EventInteractionTypes.View).Sum(x => x.N),
                    Interest = daily.Where(x => x.Date == d && x.InteractionType == EventInteractionTypes.Interest).Sum(x => x.N),
                    Remind = daily.Where(x => x.Date == d && x.InteractionType == EventInteractionTypes.Remind).Sum(x => x.N),
                    Registrations = dailyReg.Where(x => x.Date == d).Sum(x => x.N)
                }).ToList();
                ViewBag.RemindersSent = await _context.EventReminderLogs.CountAsync(l => l.ContentId == contentId);
                return View("Manage");
            }

            if (tab == "booths")
            {
                ViewBag.Bookings = await (from b in _context.EventBoothBookings.AsNoTracking()
                                          join t in _context.EventBoothTypes.AsNoTracking() on b.BoothTypeId equals t.Id
                                          where b.ContentId == contentId
                                          orderby b.Status, b.Created descending
                                          select new EventBoothBookingRow { Booking = b, BoothName = t.Name, Price = t.Price }).ToListAsync();
                return View("Manage");
            }

            if (!string.IsNullOrWhiteSpace(keyword))
                regs = regs.Where(r => r.FullName.Contains(keyword) || r.Email.Contains(keyword) || (r.Phone != null && r.Phone.Contains(keyword)) || (r.Organization != null && r.Organization.Contains(keyword)) || r.CheckInCode == keyword);
            if (status.HasValue) regs = regs.Where(r => r.Status == status.Value);
            ViewBag.Registrations = await regs.OrderByDescending(r => r.Created).Take(1000).ToListAsync();
            return View("Manage");
        }

        [HttpGet]
        public async Task<IActionResult> ExportRegistrations(long contentId, [FromServices] IExcelExportService excel)
        {
            var title = await _context.Contents.Where(c => c.Id == contentId).Select(c => c.Title).FirstOrDefaultAsync() ?? "su-kien";
            var rows = await _context.EventRegistrations.AsNoTracking().Where(r => r.ContentId == contentId)
                .OrderBy(r => r.Created).ToListAsync();
            var data = rows.Select((r, i) => new EventRegistrationExport
            {
                Stt = i + 1, FullName = r.FullName, Email = r.Email, Phone = r.Phone, Organization = r.Organization, Note = r.Note,
                Status = r.Status switch { 2 => "Đã điểm danh", 0 => "Đã hủy", _ => "Đã đăng ký" },
                Created = r.Created, CheckedInAt = r.CheckedInAt, CheckInCode = r.CheckInCode
            });
            await WriteLog(4, $"Xuất Excel danh sách đăng ký sự kiện #{contentId} ({rows.Count} dòng)");
            return excel.Export(data, $"DangKy_SuKien_{contentId}_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // ── Điểm danh: QR trên vé mở trang này (cán bộ đã đăng nhập CMS) ──
        [HttpGet]
        public async Task<IActionResult> CheckIn(string? code)
        {
            EventRegistration? reg = null;
            if (!string.IsNullOrWhiteSpace(code))
                reg = await _context.EventRegistrations.AsNoTracking().FirstOrDefaultAsync(r => r.CheckInCode == code.Trim());
            ViewBag.Code = code;
            ViewBag.EventTitle = reg == null ? null : await _context.Contents.Where(c => c.Id == reg.ContentId).Select(c => c.Title).FirstOrDefaultAsync();
            return View(reg);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRegistrationStatus(int id, byte status)
        {
            var r = await _context.EventRegistrations.FindAsync(id);
            if (r == null) return Json(new { success = false, message = "Không tìm thấy đăng ký." });
            if (status is not (EventRegistrationStatuses.Cancelled or EventRegistrationStatuses.Registered or EventRegistrationStatuses.CheckedIn))
                return Json(new { success = false, message = "Trạng thái không hợp lệ." });
            if (status == EventRegistrationStatuses.CheckedIn && r.Status == EventRegistrationStatuses.CheckedIn)
                return Json(new { success = false, message = $"Đã điểm danh lúc {r.CheckedInAt:HH:mm dd/MM}." });
            r.Status = status;
            r.CheckedInAt = status == EventRegistrationStatuses.CheckedIn ? DateTime.Now : null;
            r.CheckedInBy = status == EventRegistrationStatuses.CheckedIn ? User.Identity?.Name : null;
            await _context.SaveChangesAsync();
            await WriteLog(2, $"Đăng ký sự kiện #{id} ({r.Email}): trạng thái {status}");
            return Json(new { success = true, message = status == EventRegistrationStatuses.CheckedIn ? $"Đã điểm danh: {r.FullName}" : "Đã cập nhật." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewBooking(int id, byte status, string? note)
        {
            var b = await _context.EventBoothBookings.FindAsync(id);
            if (b == null) return Json(new { success = false, message = "Không tìm thấy." });
            if (status is not (EventBoothBookingStatuses.Approved or EventBoothBookingStatuses.Rejected or EventBoothBookingStatuses.Cancelled))
                return Json(new { success = false, message = "Trạng thái không hợp lệ." });
            if (status == EventBoothBookingStatuses.Approved)
            {
                var type = await _context.EventBoothTypes.FindAsync(b.BoothTypeId);
                var approved = await _context.EventBoothBookings.Where(x => x.BoothTypeId == b.BoothTypeId && x.Id != id && x.Status == EventBoothBookingStatuses.Approved)
                    .SumAsync(x => (int?)x.Quantity) ?? 0;
                if (type != null && approved + b.Quantity > type.Quantity)
                    return Json(new { success = false, message = "Không đủ gian hàng trống để duyệt." });
            }
            b.Status = status;
            b.ReviewedBy = User.Identity?.Name;
            b.ReviewedAt = DateTime.Now;
            b.ReviewNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim()[..Math.Min(note.Trim().Length, 500)];
            await _context.SaveChangesAsync();

            // Sổ thu chi (HSMT A.6.1): duyệt → ghi khoản thu phí gian hàng sự kiện; từ chối/hủy → gỡ
            var store = HttpContext.RequestServices.GetRequiredService<ISupplierStoreService>();
            if (status == EventBoothBookingStatuses.Approved)
            {
                var boothType = await _context.EventBoothTypes.AsNoTracking().FirstOrDefaultAsync(t => t.Id == b.BoothTypeId);
                var amount = (boothType?.Price ?? 0) * b.Quantity;
                if (amount > 0)
                    await store.UpsertAutoIncomeAsync(FinanceRefTypes.BoothBooking, b.Id, FinanceCategories.EventBoothFee, amount, null,
                        $"Gian hàng sự kiện #{b.ContentId}: {boothType?.Name} x{b.Quantity} – {b.Organization}", User.Identity?.Name);
            }
            else
                await store.RemoveAutoIncomeAsync(FinanceRefTypes.BoothBooking, b.Id);
            await WriteLog(2, $"Duyệt đăng ký gian hàng #{id} ({b.Organization}): trạng thái {status}");
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRemindersNow()
        {
            var n = await EventReminderBackgroundService.RunOnceAsync(HttpContext.RequestServices, DateTime.Now, HttpContext.RequestAborted);
            return Json(new { success = true, message = n == 0 ? "Không có nhắc lịch nào đến hạn." : $"Đã đưa {n} nhắc lịch vào hàng đợi gửi." });
        }
    }

    public class EventAdminRow
    {
        public long ContentId { get; set; }
        public string Title { get; set; } = "";
        public DateTime StartAt { get; set; }
        public string? Location { get; set; }
        public int Registered { get; set; }
        public int CheckedIn { get; set; }
        public int Interest { get; set; }
        public int PendingBooths { get; set; }
    }

    public class EventInfoFormVm
    {
        public long ContentId { get; set; }
        [Required(ErrorMessage = "Nhập thời gian bắt đầu.")] public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        [StringLength(500)] public string? Location { get; set; }
        [StringLength(1000)] public string? LivestreamUrl { get; set; }
        public bool RegistrationEnabled { get; set; } = true;
        public DateTime? RegistrationDeadline { get; set; }
        public int? MaxAttendees { get; set; }
        [StringLength(100)] public string? ReminderOffsetsMinutes { get; set; } = "1440,60";
        public bool RemindByEmail { get; set; } = true;
        public bool RemindBySms { get; set; }
    }

    public class EventDailyStat
    {
        public DateTime Date { get; set; }
        public int Views { get; set; }
        public int Interest { get; set; }
        public int Remind { get; set; }
        public int Registrations { get; set; }
    }

    public class EventBoothBookingRow
    {
        public EventBoothBooking Booking { get; set; } = null!;
        public string BoothName { get; set; } = "";
        public decimal Price { get; set; }
    }

    public class EventRegistrationExport
    {
        [Display(Name = "STT")] public int Stt { get; set; }
        [Display(Name = "Họ tên")] public string FullName { get; set; } = "";
        [Display(Name = "Email")] public string Email { get; set; } = "";
        [Display(Name = "Điện thoại")] public string? Phone { get; set; }
        [Display(Name = "Đơn vị")] public string? Organization { get; set; }
        [Display(Name = "Ghi chú")] public string? Note { get; set; }
        [Display(Name = "Trạng thái")] public string Status { get; set; } = "";
        [Display(Name = "Thời điểm đăng ký")] public DateTime Created { get; set; }
        [Display(Name = "Thời điểm điểm danh")] public DateTime? CheckedInAt { get; set; }
        [Display(Name = "Mã điểm danh")] public string CheckInCode { get; set; } = "";
    }
}
