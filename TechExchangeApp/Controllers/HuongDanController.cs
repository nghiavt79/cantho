using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TechExchangeApp.Data;
using TechExchangeApp.Helpers;
using TechExchangeApp.ViewModel;

namespace TechExchangeApp.Controllers
{
    // Trang "Hướng dẫn sử dụng" công khai, chia theo vai trò (Người mua hàng, Nhà cung ứng, Nhà tư vấn...).
    // Tái sử dụng bảng Menu/Contents có sẵn (soạn nội dung qua Cms/MenuAdmin + Cms/Posts) — chỉ viết
    // controller/view hiển thị riêng vì giao diện dạng "docs site" khác hẳn NewsController (blog).
    public class HuongDanController : Controller
    {
        private const string RootQueryString = "huong-dan-su-dung";

        private readonly AppDbContext _context;
        private readonly string _mainDomain;

        public HuongDanController(AppDbContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _mainDomain = appSettings.Value.MainDomain;
        }

        // GET: /huong-dan
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var root = await GetRootMenuAsync();
            if (root == null) return View(new HuongDanIndexVm());

            var roles = await BuildSidebarAsync(root.MenuId, activeMenuId: null, activeArticleId: null);

            ViewData["Title"] = "Hướng dẫn sử dụng";
            return View(new HuongDanIndexVm { Roles = roles });
        }

        // GET: /huong-dan/{queryString}-{menuId}
        [HttpGet]
        public async Task<IActionResult> Category(int menuId)
        {
            var root = await GetRootMenuAsync();
            if (root == null) return NotFound();

            var menu = await _context.Menus.AsNoTracking()
                .FirstOrDefaultAsync(m => m.MenuId == menuId && m.ParentId == root.MenuId);
            if (menu == null) return NotFound();

            var sidebar = await BuildSidebarAsync(root.MenuId, activeMenuId: menuId, activeArticleId: null);

            var vm = new HuongDanCategoryVm
            {
                MenuId = menuId,
                CategoryTitle = menu.Title ?? "",
                CategoryDescription = menu.Description,
                Sidebar = sidebar
            };

            ViewData["Title"] = menu.Title;
            return View(vm);
        }

        // GET: /huong-dan/bai-viet/{queryString}-{id}
        [HttpGet]
        public async Task<IActionResult> Detail(long id)
        {
            var article = await _context.Contents.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.StatusId == 3);
            if (article == null) return NotFound();

            var root = await GetRootMenuAsync();
            if (root == null) return NotFound();

            // Bài phải thuộc 1 menu con của cây "Hướng dẫn sử dụng"
            var parentMenu = await _context.Menus.AsNoTracking()
                .FirstOrDefaultAsync(m => m.MenuId == article.MenuId && m.ParentId == root.MenuId);
            if (parentMenu == null) return NotFound();

            var sidebar = await BuildSidebarAsync(root.MenuId, activeMenuId: parentMenu.MenuId, activeArticleId: id);

            var vm = new HuongDanDetailVm
            {
                Id = article.Id,
                Title = article.Title ?? "",
                Description = article.Description,
                Content = article.Contents,
                MenuId = parentMenu.MenuId,
                Sidebar = sidebar
            };

            ViewData["Title"] = article.Title;
            return View(vm);
        }

        private Task<Entities.Menu?> GetRootMenuAsync() =>
            _context.Menus.AsNoTracking()
                .FirstOrDefaultAsync(m => m.QueryString == RootQueryString)!;

        private async Task<List<HuongDanMenuVm>> BuildSidebarAsync(int rootMenuId, int? activeMenuId, long? activeArticleId)
        {
            var roleMenus = await _context.Menus.AsNoTracking()
                .Where(m => m.ParentId == rootMenuId && m.StatusId == 1)
                .OrderBy(m => m.Sort)
                .ToListAsync();

            var roleMenuIds = roleMenus.Select(m => m.MenuId).ToList();

            var articles = await _context.Contents.AsNoTracking()
                .Where(c => c.MenuId != null && roleMenuIds.Contains(c.MenuId.Value) && c.StatusId == 3)
                .OrderBy(c => c.Id)
                .Select(c => new { c.Id, c.Title, c.QueryString, c.MenuId })
                .ToListAsync();

            return roleMenus.Select(m => new HuongDanMenuVm
            {
                MenuId = m.MenuId,
                Title = m.Title ?? "",
                QueryString = m.QueryString ?? "",
                IsActive = m.MenuId == activeMenuId,
                Articles = articles
                    .Where(a => a.MenuId == m.MenuId)
                    .Select(a => new HuongDanArticleSummaryVm
                    {
                        Id = a.Id,
                        Title = a.Title ?? "",
                        QueryString = a.QueryString ?? "",
                        IsActive = a.Id == activeArticleId
                    })
                    .ToList()
            }).ToList();
        }
    }
}
