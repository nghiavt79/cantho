using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TechExchangeApp.Controllers;
using TechExchangeApp.Services;

namespace TechExchangeApp.ViewComponents
{
    /// <summary>
    /// Khối sự kiện trên trang tin bài: thời gian, địa điểm, livestream, đăng ký, quan tâm, nhắc lịch, gian hàng.
    /// Tin bài chưa được thiết lập sự kiện thì không hiện gì.
    ///   @await Component.InvokeAsync("EventPanel", new { contentId = Model.Id })
    /// </summary>
    public class EventPanelViewComponent : ViewComponent
    {
        private const string ViewedSessionPrefix = "evt-viewed:";
        private readonly IEventService _events;
        public EventPanelViewComponent(IEventService events) => _events = events;

        public async Task<IViewComponentResult> InvokeAsync(long contentId)
        {
            var id = await _events.ResolveContentIdAsync(contentId);
            int? userId = int.TryParse(UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : null;
            var visitorKey = EventController.EnsureVisitorKey(HttpContext);

            var vm = await _events.GetPanelAsync(id, userId, visitorKey);
            if (vm == null) return Content(string.Empty);

            // Đếm lượt xem trang sự kiện (A.4.4) — mỗi phiên tính 1 lần
            var sessionKey = ViewedSessionPrefix + id;
            if (HttpContext.Session.GetString(sessionKey) == null)
            {
                HttpContext.Session.SetString(sessionKey, "1");
                await _events.RecordViewAsync(id, userId, visitorKey);
            }

            ViewBag.PostId = contentId;
            return View(vm);
        }
    }
}
