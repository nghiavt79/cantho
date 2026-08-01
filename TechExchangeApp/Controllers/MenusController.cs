using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Helpers;
using TechExchangeApp.ViewModel;

namespace TechExchangeApp.Controllers
{
    public class MenuController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly string _mainDomain;

        public MenuController(AppDbContext context, IConfiguration config, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _config = config;
            _mainDomain = appSettings.Value.MainDomain;
        }

        [HttpGet]

        public IActionResult Detail(int menuId)
        {
            var lang = LangHelper.CurrentLangId(HttpContext); // /en → 2, còn lại → 1 (VI giữ nguyên)

            var model = new MenuDetailViewModel
            {
                MenuId = menuId
            };

            // ===== BindInfo logic =====
            // Ưu tiên đúng ngôn ngữ; thiếu bản dịch thì fallback bản bất kỳ; không có → 404.
            var current = _context.Menus
                .FirstOrDefault(x => x.MenuId == menuId && x.LanguageId == lang)
                ?? _context.Menus.FirstOrDefault(x => x.MenuId == menuId);

            if (current == null)
                return NotFound();

            model.Title = current.Title;
            model.Description = current.Description;

            // Xác định "gốc nhóm" cho sidebar (động theo cây menu):
            //  - current là gốc  (ParentId 0/null) -> chính nó
            //  - current là con  (ParentId <> 0)   -> lấy cha của nó
            int sectionRootId = (current?.ParentId is int pid && pid != 0)
                ? pid
                : menuId;

            var sectionRoot = _context.Menus
                .FirstOrDefault(x => x.MenuId == sectionRootId && x.LanguageId == lang)
                ?? _context.Menus.FirstOrDefault(x => x.MenuId == sectionRootId);

            // Tiêu đề box = Title của gốc nhóm (đúng ngôn ngữ hiện tại).
            model.Header = sectionRoot?.Title;

            // Danh sách = menu con 1 cấp của gốc nhóm (bỏ menu rác StatusId null).
            // Materialize trước rồi mới dựng URL (MakeURLFriendly không dịch được sang SQL).
            var childRows = _context.Menus
                .Where(x =>
                    x.ParentId == sectionRootId &&
                    x.LanguageId == lang &&
                    x.StatusId != null)
                .OrderBy(x => x.Sort)
                .Take(50)
                .Select(x => new { x.MenuId, x.Title, x.QueryString, x.NavigateUrl })
                .ToList();

            bool isEn = lang == LangHelper.En;
            model.Menus = childRows.Select(x => new MenuItemViewModel
            {
                MenuId = x.MenuId,
                Title = x.Title,
                // Link ngoài (http...) giữ nguyên; còn lại trỏ route động theo ngôn ngữ.
                NavigateUrl = (!string.IsNullOrEmpty(x.NavigateUrl) && x.NavigateUrl.Contains("http"))
                    ? x.NavigateUrl
                    : LangHelper.MenuPath(isEn, ProductController.MakeURLFriendly(x.QueryString ?? x.Title), x.MenuId)
            }).ToList();

            return View(model);
        }
    }
}
