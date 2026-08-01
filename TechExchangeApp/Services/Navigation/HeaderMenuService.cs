using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TechExchangeApp.Controllers;
using TechExchangeApp.Data;
using TechExchangeApp.Helpers;
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
            // Cấp 1: ShowHeader = true, là mục gốc (ParentId 0/null), đang bật, đúng ngôn ngữ, theo Sort.
            var parents = _db.Menus
                .AsNoTracking()
                .Where(m => m.LanguageId == languageId
                            && m.StatusId == (byte)1
                            && m.ShowHeader
                            && (m.ParentId == null || m.ParentId == 0))
                .OrderBy(m => m.Sort ?? int.MaxValue)
                .Select(m => new { m.MenuId, m.Title, m.QueryString, m.NavigateUrl, m.Description })
                .ToList();

            var parentIds = parents.Select(p => p.MenuId).ToList();
            bool isEn = languageId == LangHelper.En;

            // Cấp 2: con của mục cấp 1, cũng phải ShowHeader=1 (tôn trọng toggle từng mục).
            var children = _db.Menus
                .AsNoTracking()
                .Where(m => m.LanguageId == languageId
                            && m.StatusId == (byte)1
                            && m.ShowHeader
                            && m.ParentId != null
                            && parentIds.Contains(m.ParentId.Value))
                .OrderBy(m => m.Sort ?? int.MaxValue)
                .Select(m => new { m.MenuId, ParentId = m.ParentId!.Value, m.Title, m.QueryString, m.NavigateUrl })
                .ToList();

            var childrenByParent = children
                .GroupBy(c => c.ParentId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(c => new HeaderMenuChild
                    {
                        Label = c.Title ?? "",
                        Url = BuildUrl(isEn, c.NavigateUrl, c.QueryString, c.Title, c.MenuId)
                    }).ToList());

            var items = new List<HeaderMenuItem>(parents.Count);
            foreach (var p in parents)
            {
                var meta = ParseMeta(p.Description);
                items.Add(new HeaderMenuItem
                {
                    MenuId = p.MenuId,
                    Label = p.Title ?? "",
                    Url = BuildUrl(isEn, p.NavigateUrl, p.QueryString, p.Title, p.MenuId),
                    Icon = meta.TryGetValue("icon", out var ic) ? ic : null,
                    CssClass = meta.TryGetValue("css", out var cs) ? cs : null,
                    Children = childrenByParent.TryGetValue(p.MenuId, out var kids) ? kids : new List<HeaderMenuChild>()
                });
            }
            return items;
        }

        // NavigateUrl = href thật (nếu có); thiếu thì tự sinh URL trang động /trang/{slug}-{id}
        // (hoặc /en/page/... khi tiếng Anh), slug từ QueryString hoặc Title.
        private static string BuildUrl(bool isEn, string? navigateUrl, string? queryString, string? title, int menuId)
        {
            if (!string.IsNullOrWhiteSpace(navigateUrl))
                return navigateUrl!.Trim();
            var slug = ProductController.MakeURLFriendly(queryString ?? title);
            return LangHelper.MenuPath(isEn, slug, menuId);
        }

        /// <summary>
        /// Phân tích cột Description của row header cấp 1 thành meta "key=value;key=value".
        /// Key hỗ trợ: icon, css (dùng cho mục đặc biệt như OCOP).
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
