using Microsoft.AspNetCore.Mvc;

namespace TechExchangeApp.Controllers
{
    public class LanguageController : Controller
    {
        [HttpGet]
        public IActionResult SetLanguage(string lang, string? returnUrl)
        {
            var value = lang == "en" ? "en" : "vi";
            Response.Cookies.Append("site_lang", value, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });

            return LocalRedirect(string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl) ? "/" : returnUrl);
        }
    }
}
