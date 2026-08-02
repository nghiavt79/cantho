using System.Net;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechExchangeApp.Services.Localization;

namespace TechExchangeApp.Localization
{
    /// <summary>
    /// Adapter i18n: giữ nguyên chữ ký cũ (Html.Text / Html.Tr / I18n.T) — nay đọc từ
    /// IUiTextService (bảng UiTranslations + cache) thay cho ui.json.
    /// Thêm Html.TrHtml cho key có AllowHtml=1 (render raw có kiểm soát).
    /// </summary>
    public static class I18n
    {
        /// <summary>Text thuần (dùng trong controller/JSON). Thiếu key -> trả về key.</summary>
        public static string T(HttpContext httpContext, string key)
            => Resolve(httpContext)?.Text(key) ?? key;

        /// <summary>Html.Text(key): trả string, Razor tự encode (an toàn).</summary>
        public static string Text(this IHtmlHelper html, string key)
            => Resolve(html.ViewContext.HttpContext)?.Text(key) ?? key;

        /// <summary>Html.Tr(key): IHtmlContent đã encode (giữ hành vi cũ, luôn an toàn).</summary>
        public static IHtmlContent Tr(this IHtmlHelper html, string key)
            => new HtmlString(WebUtility.HtmlEncode(Resolve(html.ViewContext.HttpContext)?.Text(key) ?? key));

        /// <summary>Html.TrHtml(key): raw CHỈ KHI key có AllowHtml=1, ngược lại vẫn encode.</summary>
        public static IHtmlContent TrHtml(this IHtmlHelper html, string key)
        {
            var svc = Resolve(html.ViewContext.HttpContext);
            return svc != null ? svc.TrHtml(key) : new HtmlString(WebUtility.HtmlEncode(key));
        }

        private static IUiTextService? Resolve(HttpContext? ctx)
            => ctx?.RequestServices.GetService(typeof(IUiTextService)) as IUiTextService;
    }
}
