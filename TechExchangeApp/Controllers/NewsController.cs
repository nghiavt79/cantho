using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.ViewModel;
using System.Globalization;
using TechExchangeApp.Helpers;

namespace TechExchangeApp.Controllers
{
    public class NewsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly string _mainDomain;

        public NewsController(AppDbContext context, IConfiguration config, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _config = config;
            _mainDomain = appSettings.Value.MainDomain;
        }

        [HttpGet]
        public async Task<IActionResult> Detail(
            int menuId,
            string queryString,
            long id)
        {

            var p = await _context.Contents
                .AsNoTracking()
                .Where(x => x.Id == id && x.StatusId == 3)
                .OrderByDescending(x => x.PublishedDate)
                .FirstOrDefaultAsync();

            if (p == null)
                return Redirect($"{_mainDomain}Errors/404.aspx");

            // === CHECK ROUTE (GIỐNG HỆT WEBFORMS) ===
            if (menuId != p.MenuId || queryString != p.QueryString)
                return Redirect($"{_mainDomain}Errors/404.aspx");

            // === META ===
            ViewData["Title"] = p.Title;
            ViewData["MetaDescription"] = p.Description;

            // === DATE FORMAT ===
            string? publishedDate = null;
            if (p.PublishedDate.HasValue)
            {
                publishedDate = p.PublishedDate.Value
                    .ToString("dddd, d/M/yyyy, HH:mm",
                        new CultureInfo("vi-VN")) + " (GMT+7)";
            }

            // === IMAGE (TypeId = 4) ===
            var images = new List<Album>();
            if (p.TypeId == 4)
            {
                images = await _context.Albums
                    .AsNoTracking()
                    .Where(x => x.ContensID == p.Id)
                    .ToListAsync();
            }

            // === UPDATE VIEW ===
            if (p.Viewed == null)
            {
                p.Viewed = 168;
                await _context.SaveChangesAsync();
            }
            else
            {
                var sessionView = HttpContext.Session.GetString("ViewNews");
                var updateTime = _config.GetValue<int>("SettingTimeUpdatePageView");

                if (sessionView == null ||
                    (DateTime.Now - DateTime.Parse(sessionView)).TotalSeconds >= updateTime)
                {
                    p.Viewed += 1;
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("ViewNews", DateTime.Now.ToString());
                }
            }

            // === RELATED NEWS ===
            var langId = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var subMenus = _context
                .UspSelectSubMenu(p.MenuId ?? 0);

            // Include parent menuId itself
            var allMenuIds = subMenus.Append(p.MenuId ?? 0).ToHashSet();

            var related = await _context.Contents
                .AsNoTracking()
                .Where(x =>
                    x.Id != id &&
                    x.StatusId == 3 &&
                    allMenuIds.Contains(x.MenuId ?? 0) &&
                    x.LanguageId == langId)
                .OrderByDescending(x => x.PublishedDate)
                .Take(5)
                .Select(x => new RelatedNewsVm
                {
                    Id = x.Id,
                    Title = x.Title,
                    QueryString = x.QueryString,
                    MenuId = x.MenuId,
                    PublishedDate = x.PublishedDate
                })
                .ToListAsync();

            // === VIEWMODEL ===
            var vm = new NewsDetailVm
            {
                Id = p.Id,
                MenuId = p.MenuId ?? menuId,
                Title = p.Title,
                Description = p.Description,
                Content = p.Contents,
                Author = (p.TypeId == 5 && p.MenuId == 46) ? "" : p.Author,
                PublishedDateText = publishedDate,
                Images = images,
                Related = related
            };

            return View(vm);
        }


        [HttpGet]

        public async Task<IActionResult> Category(int menuId, int page = 1)
        {
            const int pageSize = 10;

            var langId = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var menu = await GetMenuAsync(menuId);
            if (menu == null)
                return Redirect($"{_mainDomain}Errors/404.aspx");

            var subMenuIds = GetSubMenuIds(menuId);
            // Include the root menuId itself so we don't miss top-level articles
            var allMenuIds = subMenuIds.Append(menuId).ToHashSet();

            var (items, total) = await GetNewsByMenuAsync(
                allMenuIds,
                langId,
                page,
                pageSize
            );

            var pager = BuildPager(total, page, pageSize, 10);

            // Featured items: first 4 most recent (only on page 1)
            var featuredItems = new List<NewsItemVm>();
            if (page == 1)
            {
                featuredItems = await _context.Contents
                    .AsNoTracking()
                    .Where(q => allMenuIds.Contains(q.MenuId ?? 0)
                        && q.LanguageId == langId
                        && q.StatusId == 3
                        && q.IsReport != true
                        && q.PublishedDate <= DateTime.Now
                        && (q.eEffectiveDate == null || q.eEffectiveDate >= DateTime.Now))
                    .OrderByDescending(q => q.PublishedDate)
                    .Take(4)
                    .Select(q => new NewsItemVm
                    {
                        Id = q.Id,
                        MenuId = q.MenuId,
                        Title = q.Title,
                        QueryString = q.QueryString,
                        Image = q.Image,
                        Description = q.Description,
                        PublishedDate = q.PublishedDate
                    })
                    .ToListAsync();
            }

            // Latest items for sidebar (5 most recent across all news)
            var latestItems = await _context.Contents
                .AsNoTracking()
                .Where(q => q.LanguageId == langId
                    && q.StatusId == 3
                    && q.IsReport != true
                    && q.PublishedDate <= DateTime.Now)
                .OrderByDescending(q => q.PublishedDate)
                .Take(5)
                .Select(q => new NewsItemVm
                {
                    Id = q.Id,
                    MenuId = q.MenuId,
                    Title = q.Title,
                    QueryString = q.QueryString,
                    Image = q.Image,
                    PublishedDate = q.PublishedDate
                })
                .ToListAsync();

            // Sibling menus for category filter tabs
            var parentMenuId = menu.ParentId ?? 0;
            var siblingMenus = await _context.Menus
                .AsNoTracking()
                .Where(m => m.ParentId == parentMenuId && m.StatusId == 1)
                .OrderBy(m => m.Sort)
                .Select(m => new SiblingMenuVm
                {
                    MenuId = m.MenuId,
                    Title = m.Title,
                    QueryString = m.QueryString,
                    IsActive = m.MenuId == menuId
                })
                .ToListAsync();

            var vm = new NewsCategoryVm
            {
                MenuId = menuId,
                CategoryTitle = menu.Title,
                CategoryDescription = $"Cập nhật các hoạt động, chương trình, hội thảo và thông tin khoa học công nghệ mới nhất thuộc mục {menu.Title}.",
                Items = items,
                FeaturedItems = featuredItems,
                SiblingMenus = siblingMenus,
                LatestItems = latestItems,
                Pager = pager
            };

            ViewData["Title"] = menu.Title;

            return View(vm);
        }


        private async Task<Menu?> GetMenuAsync(int menuId)
        {
            return await _context.Menus
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MenuId == menuId);
        }


        private List<int> GetSubMenuIds(int menuId)
        {
            return _context
                .UspSelectSubMenu(menuId)
                .Select(x => x)
                .ToList();
        }


        private async Task<(List<NewsItemVm> Items, int Total)>
    GetNewsByMenuAsync(
        HashSet<int> allMenuIds,
        int langId,
        int page,
        int pageSize)
        {
            var now = DateTime.Now;

            // Single filtered query — reused for both COUNT and SELECT
            var query = _context.Contents
                .AsNoTracking()
                .Where(q =>
                    allMenuIds.Contains(q.MenuId ?? 0) &&
                    q.LanguageId == langId &&
                    q.StatusId == 3 &&
                    q.IsReport != true &&
                    q.PublishedDate <= now &&
                    (q.eEffectiveDate == null || q.eEffectiveDate >= now)
                );

            // Sequential — EF Core DbContext không thread-safe, không dùng Task.WhenAll trên cùng context
            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(q => q.PublishedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new NewsItemVm
                {
                    Id = q.Id,
                    MenuId = q.MenuId,
                    Title = q.Title,
                    QueryString = q.QueryString,
                    Image = q.Image,
                    Description = q.Description,
                    PublishedDate = q.PublishedDate
                })
                .ToListAsync();

            return (items, total);

        }


        private PagerVm BuildPager(
            int totalRecord,
            int currentPage,
            int pageSize,
            int pageToShow)
        {
            var totalPage = (int)Math.Ceiling(totalRecord / (double)pageSize);

            var pages = new HashSet<int>();

            for (int i = Math.Max(1, currentPage - pageToShow);
                 i <= Math.Min(totalPage, currentPage + pageToShow);
                 i++)
            {
                pages.Add(i);
            }

            return new PagerVm
            {
                TotalRecord = totalRecord,
                TotalPage = totalPage,
                CurrentPage = currentPage,
                Pages = pages.OrderBy(x => x).ToList()
            };
        }

    }
}
