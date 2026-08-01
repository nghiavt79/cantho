using Microsoft.AspNetCore.Mvc;
using TechExchangeApp.Helpers;
using TechExchangeApp.Models.Navigation;
using TechExchangeApp.Services.Navigation;

namespace TechExchangeApp.ViewComponents
{
    /// <summary>
    /// Render thanh menu header 2 cấp, hoàn toàn data-driven từ bảng Menu (2 ngôn ngữ).
    /// Cấp 1 = MenuPosition "1"; cấp 2 = row Menu có ParentId trỏ về mục cấp 1.
    /// variant = "desktop" (thanh ngang) hoặc "mobile" (offcanvas).
    /// </summary>
    public class SiteHeaderMenuViewComponent : ViewComponent
    {
        private readonly IHeaderMenuService _headerMenu;

        public SiteHeaderMenuViewComponent(IHeaderMenuService headerMenu)
        {
            _headerMenu = headerMenu;
        }

        public IViewComponentResult Invoke(string variant = "desktop")
        {
            int langId = LangHelper.CurrentLangId(HttpContext);
            var items = _headerMenu.GetHeaderMenu(langId);

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
        public IReadOnlyList<HeaderMenuItem> Items { get; set; } = new List<HeaderMenuItem>();
    }
}
