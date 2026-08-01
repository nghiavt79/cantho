using System.Text.Json;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;

namespace TechExchangeApp.Localization
{
    public static class I18n
    {
        private const string DefaultLang = "vi";
        private static readonly object Sync = new();
        private static Dictionary<string, Dictionary<string, string>>? _strings;

        public static IHtmlContent Tr(this IHtmlHelper html, string key)
            => new HtmlString(System.Net.WebUtility.HtmlEncode(T(html.ViewContext.HttpContext, key)));

        public static string T(HttpContext httpContext, string key)
        {
            var lang = GetLanguage(httpContext);
            var strings = Load(httpContext);

            if (strings.TryGetValue(key, out var values))
            {
                if (values.TryGetValue(lang, out var localized) && !string.IsNullOrWhiteSpace(localized))
                {
                    return localized;
                }

                if (values.TryGetValue(DefaultLang, out var fallback) && !string.IsNullOrWhiteSpace(fallback))
                {
                    return fallback;
                }
            }

            return key;
        }

        public static string Text(this IHtmlHelper html, string key)
            => T(html.ViewContext.HttpContext, key);

        private static string GetLanguage(HttpContext httpContext)
        {
            // Ngôn ngữ do middleware "/en" gắn vào Items["Lang"] (không dùng cookie nữa)
            var lang = httpContext.Items["Lang"] as string;
            return string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase) ? "en" : DefaultLang;
        }

        private static Dictionary<string, Dictionary<string, string>> Load(HttpContext httpContext)
        {
            if (_strings != null) return _strings;

            lock (Sync)
            {
                if (_strings != null) return _strings;

                var env = httpContext.RequestServices.GetService(typeof(IHostEnvironment)) as IHostEnvironment;
                var contentRoot = env?.ContentRootPath ?? AppContext.BaseDirectory;
                var path = Path.Combine(contentRoot, "Localization", "ui.json");

                if (!File.Exists(path))
                {
                    _strings = new Dictionary<string, Dictionary<string, string>>();
                    return _strings;
                }

                using var stream = File.OpenRead(path);
                _strings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(stream)
                    ?? new Dictionary<string, Dictionary<string, string>>();
                return _strings;
            }
        }
    }
}
