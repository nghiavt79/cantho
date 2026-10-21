using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TechExchangeApp.Helpers;
using TechExchangeApp.Localization;
using TechExchangeApp.Services;

namespace TechExchangeApp.Controllers
{
    /// <summary>
    /// Thao tác sự kiện phía người dùng (HSMT A.4.1, A.4.3, A.4.4, A.4.5): đăng ký, quan tâm, nhắc lịch,
    /// thuê gian hàng, xem vé. Khách chưa đăng nhập vẫn dùng được; nhận diện bằng cookie "evk".
    /// Gọi AJAX qua /Event/... hoặc /en/event/... (để thông báo trả về đúng ngôn ngữ).
    /// </summary>
    public class EventController : Controller
    {
        public const string VisitorCookie = "evk";
        private readonly IEventService _events;
        public EventController(IEventService events) => _events = events;

        private int? CurrentUserId => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

        /// <summary>Lấy hoặc tạo mã khách ẩn danh (cookie HttpOnly 1 năm).</summary>
        public static string EnsureVisitorKey(HttpContext ctx)
        {
            var key = ctx.Request.Cookies[VisitorCookie];
            if (string.IsNullOrEmpty(key) || key.Length > 64)
            {
                key = Guid.NewGuid().ToString("N");
                ctx.Response.Cookies.Append(VisitorCookie, key, new CookieOptions
                {
                    HttpOnly = true, Secure = ctx.Request.IsHttps, SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true
                });
            }
            return key;
        }

        private string T(string key) => I18n.T(HttpContext, key);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(long contentId, EventRegisterInput input, string? website)
        {
            // Bẫy bot: ô ẩn "website" phải để trống
            if (!string.IsNullOrEmpty(website)) return Json(new { success = true });
            var id = await _events.ResolveContentIdAsync(contentId);
            var (ok, err, ticketUrl) = await _events.RegisterAsync(id, input, CurrentUserId,
                HttpContext.Connection.RemoteIpAddress?.ToString(), LangHelper.IsEnglish(HttpContext));
            return Json(new { success = ok, message = T(ok ? "event.reg.success" : err!), ticketUrl });
        }

        /// <summary>Đường dẫn theo dõi lượt nhấp (A.4.4): gắn vào banner, bài đăng, tin nhắn; ghi lượt nhấp rồi chuyển tới trang sự kiện.</summary>
        [HttpGet("/su-kien/nhap/{contentId:long}")]
        public async Task<IActionResult> Click(long contentId)
        {
            var id = await _events.ResolveContentIdAsync(contentId);
            await _events.RecordInteractionAsync(id, TechExchangeApp.Entities.EventInteractionTypes.Click, CurrentUserId, EnsureVisitorKey(HttpContext));
            return Redirect(await _events.EventUrlAsync(id));
        }

        /// <summary>Ghi lượt chia sẻ khi người xem bấm nút chia sẻ mạng xã hội trên trang sự kiện (A.4.4).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share(long contentId)
        {
            var id = await _events.ResolveContentIdAsync(contentId);
            await _events.RecordInteractionAsync(id, TechExchangeApp.Entities.EventInteractionTypes.Share, CurrentUserId, EnsureVisitorKey(HttpContext));
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Interest(long contentId)
        {
            var id = await _events.ResolveContentIdAsync(contentId);
            var (active, count) = await _events.ToggleInterestAsync(id, CurrentUserId, EnsureVisitorKey(HttpContext));
            return Json(new { success = true, active, count });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remind(long contentId, string? email, string? phone)
        {
            var id = await _events.ResolveContentIdAsync(contentId);
            var (ok, err) = await _events.SubscribeReminderAsync(id, CurrentUserId, EnsureVisitorKey(HttpContext), email, phone);
            return Json(new { success = ok, message = T(ok ? "event.remind.success" : err!) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Booth(long contentId, EventBoothInput input, string? website)
        {
            if (!string.IsNullOrEmpty(website)) return Json(new { success = true });
            var id = await _events.ResolveContentIdAsync(contentId);
            var (ok, err) = await _events.BookBoothAsync(id, input, CurrentUserId);
            return Json(new { success = ok, message = T(ok ? "event.booth.success" : err!) });
        }

        // GET /su-kien/ve/{code}, /en/event/ticket/{code}
        [HttpGet]
        public async Task<IActionResult> Ticket(string code)
        {
            var vm = await _events.GetTicketAsync(code);
            if (vm == null) return NotFound();
            Response.Headers["Referrer-Policy"] = "no-referrer";
            Response.Headers["X-Robots-Tag"] = "noindex";
            return View(vm);
        }
    }
}
