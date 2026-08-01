using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechExchangeApp.Helpers;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>Đổi ngôn ngữ biên tập chung của CMS (dropdown góc phải).</summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class CmsLanguageController : Controller
    {
        [HttpGet]
        public IActionResult Set(int lang, string? returnUrl)
        {
            CmsLangHelper.Set(HttpContext, lang);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Dashboard", new { area = "Cms" });
        }
    }
}
