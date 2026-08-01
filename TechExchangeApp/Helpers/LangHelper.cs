using Microsoft.AspNetCore.Http;

namespace TechExchangeApp.Helpers
{
    /// <summary>
    /// Ngôn ngữ hiện tại suy ra từ cờ do middleware "/en" gắn vào HttpContext.Items["Lang"].
    /// Không dùng Session. LanguageId: 1 = tiếng Việt, 2 = tiếng Anh.
    /// </summary>
    public static class LangHelper
    {
        public const int Vi = 1;
        public const int En = 2;

        public static bool IsEnglish(HttpContext? ctx)
            => (ctx?.Items["Lang"] as string) == "en";

        public static int CurrentLangId(HttpContext? ctx)
            => IsEnglish(ctx) ? En : Vi;

        /// <summary>URL trang chủ theo ngôn ngữ: "/en" khi đang tiếng Anh, ngược lại "/".</summary>
        public static string HomeUrl(HttpContext? ctx)
            => IsEnglish(ctx) ? "/en" : "/";

        /// <summary>URL trang tìm kiếm theo ngôn ngữ: "/en/search" khi tiếng Anh, ngược lại "/Search".</summary>
        public static string SearchUrl(HttpContext? ctx)
            => IsEnglish(ctx) ? "/en/search" : "/Search";

        /// <summary>
        /// URL trang nội dung menu (động) theo ngôn ngữ:
        /// tiếng Anh "/en/page/{slug}-{menuId}", tiếng Việt "/trang/{slug}-{menuId}".
        /// slug chỉ để đẹp URL; menuId mới quyết định trang.
        /// </summary>
        public static string MenuPath(bool isEnglish, string? slug, int menuId)
        {
            var s = string.IsNullOrWhiteSpace(slug) ? "muc" : slug.Trim();
            return (isEnglish ? "/en/page/" : "/trang/") + s + "-" + menuId;
        }

        /// <summary>URL bài tin tiếng Anh: /en/news-event/{slug}-{id}</summary>
        public static string EnNewsUrl(string? slug, long id)
            => $"/en/news-event/{slug}-{id}";
    }
}
