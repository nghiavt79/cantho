using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechExchangeApp.Services;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class CacheController : Controller
    {
        private readonly IHomeCacheSignal _homeCacheSignal;

        public CacheController(IHomeCacheSignal homeCacheSignal)
        {
            _homeCacheSignal = homeCacheSignal;
        }

        // POST: /cms/Cache/ClearHomeCache
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearHomeCache()
        {
            _homeCacheSignal.Clear();
            return Json(new { success = true, message = "Đã xóa cache trang chủ. Tải lại trang chủ để thấy dữ liệu mới." });
        }
    }
}
