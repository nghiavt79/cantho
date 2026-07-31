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

        /// <summary>URL bài tin tiếng Anh: /en/news-event/{slug}-{id}</summary>
        public static string EnNewsUrl(string? slug, long id)
            => $"/en/news-event/{slug}-{id}";
    }
}
