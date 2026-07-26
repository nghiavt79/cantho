using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Areas.Cms.Models;
using TechExchangeApp.Data;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class LogAdminController : Controller
    {
        private readonly AppDbContext _context;

        public LogAdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? keyword, int? functionId, int? eventId, string? userName, string? clientIP,
            int? siteId, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 20)
        {
            if (!new[] { 10, 20, 50, 100 }.Contains(pageSize))
                pageSize = 20;

            var query =
                from log in _context.Logs.AsNoTracking()
                join function in _context.SysFunctions.AsNoTracking()
                    on log.FunctionID equals function.FunctionId into functionJoin
                from function in functionJoin.DefaultIfEmpty()
                select new LogListItemVm
                {
                    LogID = log.LogID,
                    FunctionID = log.FunctionID,
                    FunctionName = function != null ? function.FunctionName : null,
                    ActTime = log.ActTime,
                    EventID = log.EventID,
                    Content = log.Content,
                    ClientIP = log.ClientIP,
                    UserName = log.UserName,
                    Domain = log.Domain,
                    LanguageId = log.LanguageId,
                    SiteId = log.SiteId
                };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(l =>
                    l.LogID.ToString().Contains(kw) ||
                    (l.Content != null && l.Content.ToLower().Contains(kw)) ||
                    (l.FunctionName != null && l.FunctionName.ToLower().Contains(kw)) ||
                    (l.Domain != null && l.Domain.ToLower().Contains(kw)));
            }

            if (functionId.HasValue)
                query = query.Where(l => l.FunctionID == functionId.Value);
            if (eventId.HasValue)
                query = query.Where(l => l.EventID == eventId.Value);
            if (!string.IsNullOrWhiteSpace(userName))
            {
                var user = userName.Trim().ToLower();
                query = query.Where(l => l.UserName != null && l.UserName.ToLower().Contains(user));
            }
            if (!string.IsNullOrWhiteSpace(clientIP))
            {
                var ip = clientIP.Trim();
                query = query.Where(l => l.ClientIP != null && l.ClientIP.Contains(ip));
            }
            if (siteId.HasValue)
                query = query.Where(l => l.SiteId == siteId.Value);
            if (fromDate.HasValue)
                query = query.Where(l => l.ActTime >= fromDate.Value.Date);
            if (toDate.HasValue)
            {
                var endDate = toDate.Value.Date.AddDays(1);
                query = query.Where(l => l.ActTime < endDate);
            }

            query = query.OrderByDescending(l => l.ActTime).ThenByDescending(l => l.LogID);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            foreach (var item in items)
                item.EventName = GetEventName(item.EventID);

            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Keyword = keyword;
            ViewBag.FunctionId = functionId;
            ViewBag.EventId = eventId;
            ViewBag.UserName = userName;
            ViewBag.ClientIP = clientIP;
            ViewBag.SiteId = siteId;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Functions = await _context.SysFunctions.AsNoTracking()
                .OrderBy(f => f.FunctionName)
                .Select(f => new { f.FunctionId, f.FunctionName })
                .ToListAsync();
            ViewBag.Sites = await _context.RootSites.AsNoTracking().OrderBy(s => s.SiteId).ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ListPartial", items);

            return View(items);
        }

        private static string GetEventName(int? eventId)
        {
            return eventId switch
            {
                1 => "Thêm",
                2 => "Sửa",
                3 => "Xóa",
                4 => "Đăng nhập",
                5 => "Đăng xuất",
                _ => eventId?.ToString() ?? ""
            };
        }
    }
}
