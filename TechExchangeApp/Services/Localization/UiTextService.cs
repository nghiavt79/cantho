using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TechExchangeApp.Data;

namespace TechExchangeApp.Services.Localization
{
    public class UiTextService : IUiTextService
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<UiTextService> _logger;

        private const string CacheKey = "ui_translations_map_v1";

        // Log mỗi key thiếu đúng 1 lần/tiến trình (net bắt sót lúc chạy, kể cả key động).
        private static readonly ConcurrentDictionary<string, byte> _loggedMissing = new();

        public UiTextService(AppDbContext db, IMemoryCache cache, IHttpContextAccessor http, ILogger<UiTextService> logger)
        {
            _db = db;
            _cache = cache;
            _http = http;
            _logger = logger;
        }

        public IReadOnlyDictionary<string, UiTextEntry> GetAll()
        {
            return _cache.GetOrCreate(CacheKey, entry =>
            {
                // Giữ đến khi Invalidate() (chạy 1 instance nên đủ).
                entry.Priority = CacheItemPriority.NeverRemove;
                return _db.UiTranslations
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .Select(x => new UiTextEntry(x.Key, x.Vi, x.En, x.AllowHtml))
                    .ToDictionary(x => x.Key, StringComparer.Ordinal);
            })!;
        }

        public string Text(string key)
        {
            if (GetAll().TryGetValue(key, out var e))
            {
                var v = Localized(e);
                if (!string.IsNullOrEmpty(v)) return v;
            }
            LogMissing(key);
            return key; // giữ hành vi cũ: thiếu key -> trả về key
        }

        // Ghi log 1 lần cho mỗi key thiếu bản dịch (bắt sót khi migrate, kể cả key động).
        private void LogMissing(string key)
        {
            if (_loggedMissing.TryAdd(key, 0))
                _logger.LogWarning("[i18n] Missing translation key: {Key}", key);
        }

        public IHtmlContent TrHtml(string key)
        {
            if (GetAll().TryGetValue(key, out var e))
            {
                var v = Localized(e) ?? key;
                // Chỉ nhả raw khi cờ bật; ngược lại luôn encode (an toàn mặc định).
                return e.AllowHtml ? new HtmlString(v) : new HtmlString(WebUtility.HtmlEncode(v));
            }
            LogMissing(key);
            return new HtmlString(WebUtility.HtmlEncode(key));
        }

        public void Invalidate() => _cache.Remove(CacheKey);

        private bool IsEnglish()
            => (_http.HttpContext?.Items["Lang"] as string) == "en";

        // EN: ưu tiên En, thiếu thì fallback Vi. VI: ưu tiên Vi, thiếu thì fallback En.
        private string? Localized(UiTextEntry e)
            => IsEnglish()
                ? (!string.IsNullOrWhiteSpace(e.En) ? e.En : e.Vi)
                : (!string.IsNullOrWhiteSpace(e.Vi) ? e.Vi : e.En);
    }
}
