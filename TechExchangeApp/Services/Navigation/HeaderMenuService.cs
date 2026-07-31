using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TechExchangeApp.Data;
using TechExchangeApp.Models.Navigation;

namespace TechExchangeApp.Services.Navigation
{
    public class HeaderMenuService : IHeaderMenuService
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;

        // Ngôn ngữ hỗ trợ (1 = VI, 2 = EN) — dùng khi Invalidate() xoá cache.
        private static readonly int[] SupportedLangs = { 1, 2 };
        private const string CacheKeyPrefix = "header_menu_lang_";

        public HeaderMenuService(AppDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public IReadOnlyList<HeaderMenuItem> GetHeaderMenu(int languageId)
        {
            return _cache.GetOrCreate(CacheKeyPrefix + languageId, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
                return BuildFromDb(languageId);
            })!;
        }

        public void Invalidate()
        {
            foreach (var lang in SupportedLangs)
                _cache.Remove(CacheKeyPrefix + lang);
        }

        private List<HeaderMenuItem> BuildFromDb(int languageId)
        {
            // Chọn đúng tập header: MenuPosition == "1" (khớp tuyệt đối, không phải Contains),
            // đang bật (StatusId == 1), đúng ngôn ngữ. Sắp theo Sort.
            var rows = _db.Menus
                .AsNoTracking()
                .Where(m => m.LanguageId == languageId
                            && m.StatusId == (byte)1
                            && m.MenuPosition != null
                            && m.MenuPosition.Trim() == "1")
                .OrderBy(m => m.Sort ?? int.MaxValue)
                .Select(m => new
                {
                    m.MenuId,
                    m.Title,
                    m.QueryString,
                    m.NavigateUrl,
                    m.Description
                })
                .ToList();

            var items = new List<HeaderMenuItem>(rows.Count);
            foreach (var r in rows)
            {
                var meta = ParseMeta(r.Description);
                items.Add(new HeaderMenuItem
                {
                    MenuId = r.MenuId,
                    Label = r.Title ?? "",
                    // NavigateUrl = href thật; thiếu thì fallback "/" + QueryString.
                    Url = !string.IsNullOrWhiteSpace(r.NavigateUrl)
                        ? r.NavigateUrl!.Trim()
                        : "/" + (r.QueryString ?? "").Trim().TrimStart('/'),
                    Icon = meta.TryGetValue("icon", out var ic) ? ic : null,
                    CssClass = meta.TryGetValue("css", out var cs) ? cs : null,
                    CategoryGroup = meta.TryGetValue("cat", out var cat) && int.TryParse(cat, out var g) ? g : null
                });
            }
            return items;
        }

        /// <summary>
        /// Phân tích cột Description của row header thành meta "key=value;key=value".
        /// Chỉ áp dụng cho row header (MenuPosition="1"); các key hỗ trợ: cat, icon, css.
        /// </summary>
        private static Dictionary<string, string> ParseMeta(string? description)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(description)) return map;

            foreach (var part in description.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var idx = part.IndexOf('=');
                if (idx <= 0) continue;
                var key = part[..idx].Trim();
                var val = part[(idx + 1)..].Trim();
                if (key.Length > 0) map[key] = val;
            }
            return map;
        }
    }
}
