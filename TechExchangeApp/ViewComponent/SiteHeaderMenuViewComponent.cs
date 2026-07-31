using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Controllers;
using TechExchangeApp.Data;
using TechExchangeApp.Helpers;
using TechExchangeApp.Models.Navigation;
using TechExchangeApp.Services.Navigation;

namespace TechExchangeApp.ViewComponents
{
    /// <summary>
    /// Render thanh menu header data-driven từ bảng Menu (2 ngôn ngữ).
    /// variant = "desktop" (thanh ngang) hoặc "mobile" (offcanvas).
    /// </summary>
    public class SiteHeaderMenuViewComponent : ViewComponent
    {
        private readonly IHeaderMenuService _headerMenu;
        private readonly AppDbContext _db;

        // ParentId danh mục -> tiền tố URL trang danh mục (VI, EN) — khớp route đã khai báo.
        private static readonly Dictionary<int, (string Vi, string En)> CategoryUrlPrefix = new()
        {
            [1] = ("/san-pham", "/en/products"),
            [2] = ("/dich-vu-tu-van", "/en/services")
        };

        public SiteHeaderMenuViewComponent(IHeaderMenuService headerMenu, AppDbContext db)
        {
            _headerMenu = headerMenu;
            _db = db;
        }

        public IViewComponentResult Invoke(string variant = "desktop")
        {
            var httpContext = HttpContext;
            int langId = LangHelper.CurrentLangId(httpContext);
            bool isEn = langId == LangHelper.En;

            // Copy tươi từ cache (không mutate object dùng chung).
            var items = _headerMenu.GetHeaderMenu(langId)
                .Select(src => new HeaderMenuItem
                {
                    MenuId = src.MenuId,
                    Label = src.Label,
                    Url = src.Url,
                    Icon = src.Icon,
                    CssClass = src.CssClass,
                    CategoryGroup = src.CategoryGroup
                })
                .ToList();

            // Bơm danh mục con động cho mục có CategoryGroup (Sản phẩm/Dịch vụ).
            foreach (var item in items)
            {
                if (item.CategoryGroup is not int group) continue;
                if (!CategoryUrlPrefix.TryGetValue(group, out var prefixes)) continue;

                var prefix = isEn ? prefixes.En : prefixes.Vi;

                var categories = _db.Categories
                    .AsNoTracking()
                    .Where(c => c.ParentId == group && c.MainCate == true
                                && c.StatusId == 1 && c.LanguageId == 1)
                    .OrderBy(c => c.Sort ?? int.MaxValue)
                    .ThenBy(c => c.Title)
                    .Select(c => new { c.CatId, c.Title, c.TitleEn, c.QueryString })
                    .ToList();

                item.Children = categories.Select(c =>
                {
                    var hasEn = isEn && !string.IsNullOrWhiteSpace(c.TitleEn);
                    // EN: slug từ TitleEn (vd "Mechanical Engineering" -> "Mechanical-Engineering");
                    // VI: slug từ QueryString (fallback Title) như cũ.
                    var slugSource = hasEn ? c.TitleEn : (c.QueryString ?? c.Title);
                    return new HeaderMenuChild
                    {
                        Label = hasEn ? c.TitleEn! : (c.Title ?? ""),
                        Url = $"{prefix}/{ProductController.MakeURLFriendly(slugSource)}-{c.CatId}"
                    };
                }).ToList();
            }

            var vm = new SiteHeaderMenuViewModel
            {
                Variant = string.Equals(variant, "mobile", StringComparison.OrdinalIgnoreCase) ? "mobile" : "desktop",
                Items = items
            };
            return View(vm);
        }
    }

    public class SiteHeaderMenuViewModel
    {
        public string Variant { get; set; } = "desktop";
        public List<HeaderMenuItem> Items { get; set; } = new();
    }
}
