using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    [Route("cms")]
    public class HuongDanCmsController : Controller
    {
        [HttpGet("huog-dan-cms")]
        [HttpGet("huong-dan-cms")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
