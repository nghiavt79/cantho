using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Interfaces;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>
    /// Admin Analytics Dashboard Controller.
    /// Executes exactly 1 snapshot read per request (scalable to 1M+ rows).
    /// All heavy analytics run in background jobs.
    /// </summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class AdminAnalyticsController : Controller
    {
        private readonly IAdminDashboardService _dashSvc;
        private readonly ILogger<AdminAnalyticsController> _logger;

        public AdminAnalyticsController(
            IAdminDashboardService dashSvc,
            ILogger<AdminAnalyticsController> logger)
        {
            _dashSvc = dashSvc;
            _logger  = logger;
        }

        // GET: /cms/AdminAnalytics
        // GET: /cms/AdminAnalytics?year=2025&month=6
        [HttpGet]
        public async Task<IActionResult> Index(int? year, int? month,
            [FromServices] TechExchangeApp.Data.AppDbContext db)
        {
            await LoadSiteVisitStatsAsync(db, year, month);
            try
            {
                var vm = await _dashSvc.GetAdminDashboardAsync(year, month);
                ViewData["Title"] = "Analytics Dashboard";
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdminAnalytics] Dashboard failed to load. Message: {Msg}", ex.Message);

                // In Development: surface the real error so you can debug quickly
                var env = HttpContext.RequestServices.GetService<IWebHostEnvironment>();
                if (env?.IsDevelopment() == true)
                {
                    TempData["Error"] = $"[DEV] {ex.GetType().Name}: {ex.Message}";
                    if (ex.InnerException != null)
                        TempData["Error"] += $" → {ex.InnerException.Message}";
                }
                else
                {
                    TempData["Error"] = "Không thể tải dữ liệu dashboard. Vui lòng thử lại sau.";
                }

                ViewData["Title"] = "Analytics Dashboard";
                return View(new TechExchangeApp.ViewModel.AdminDashboardVm());
            }
        }

        /// <summary>
        /// Lượt truy cập toàn Sàn (bảng SiteVisitDaily): hôm nay, tháng đang lọc (mặc định tháng hiện tại), năm, tổng cộng
        /// và số liệu từng ngày của tháng — dùng cho Báo cáo kết quả cung cấp dịch vụ (Mẫu số 2).
        /// </summary>
        private async Task LoadSiteVisitStatsAsync(TechExchangeApp.Data.AppDbContext db, int? year, int? month)
        {
            try
            {
                var today = DateTime.Now.Date;
                var y = year ?? today.Year;
                var m = month is >= 1 and <= 12 ? month.Value : (year.HasValue ? 0 : today.Month);
                var from = m > 0 ? new DateTime(y, m, 1) : new DateTime(y, 1, 1);
                var to = m > 0 ? from.AddMonths(1) : from.AddYears(1);

                var visits = db.SiteVisitDaily.AsNoTracking();
                var todayRow = await visits.FirstOrDefaultAsync(v => v.VisitDate == today);
                var periodRows = await visits.Where(v => v.VisitDate >= from && v.VisitDate < to)
                    .OrderBy(v => v.VisitDate).ToListAsync();
                var yearFrom = new DateTime(y, 1, 1);
                var yearTo = yearFrom.AddYears(1);

                ViewBag.VisitToday = todayRow;
                ViewBag.VisitPeriodLabel = m > 0 ? $"Tháng {m}/{y}" : $"Năm {y}";
                ViewBag.VisitPeriodPageViews = periodRows.Sum(v => (long)v.PageViews);
                ViewBag.VisitPeriodVisitors = periodRows.Sum(v => (long)v.Visitors);
                ViewBag.VisitYear = y;
                ViewBag.VisitYearPageViews = await visits.Where(v => v.VisitDate >= yearFrom && v.VisitDate < yearTo).SumAsync(v => (long)v.PageViews);
                ViewBag.VisitTotalPageViews = await visits.SumAsync(v => (long)v.PageViews);
                ViewBag.VisitTotalVisitors = await visits.SumAsync(v => (long)v.Visitors);
                ViewBag.VisitDays = periodRows;
            }
            catch (Exception ex)
            {
                // Chưa chạy Scripts/add-site-visit-daily.sql → ẩn khối lượt truy cập, không làm hỏng trang
                _logger.LogWarning(ex, "[AdminAnalytics] Không đọc được bảng SiteVisitDaily");
                ViewBag.VisitDays = null;
            }
        }

        // POST: /cms/AdminAnalytics/RefreshSnapshot
        // Manual trigger for admins
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefreshSnapshot()
        {
            await _dashSvc.RebuildSnapshotAsync();
            await _dashSvc.RebuildMonthlyStatsAsync();
            TempData["Success"] = "Snapshot đã được cập nhật thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
