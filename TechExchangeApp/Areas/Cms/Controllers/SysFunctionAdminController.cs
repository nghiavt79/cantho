using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Areas.Cms.Models;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class SysFunctionAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "SysFunction";
        private static readonly string[] LogFunctionAliases = { "FunctionName", "Tính năng" };

        public SysFunctionAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private async Task WriteLog(int eventId, string content)
        {
            await CmsLogHelper.WriteLogAsync(_context, HttpContext, GetSiteId(),
                LogFunctionName, LogFunctionAliases, eventId, content);
        }

        public async Task<IActionResult> Index(
            string? keyword, bool? isMenu, bool? isStatus, bool? isShow, int? parentId, int? siteId,
            int page = 1, int pageSize = 20)
        {
            if (!new[] { 10, 20, 50, 100 }.Contains(pageSize))
                pageSize = 20;

            var query = _context.SysFunctions.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(f =>
                    f.FunctionId.ToString().Contains(kw) ||
                    (f.FunctionName != null && f.FunctionName.ToLower().Contains(kw)) ||
                    (f.URL != null && f.URL.ToLower().Contains(kw)) ||
                    (f.HrefName != null && f.HrefName.ToLower().Contains(kw)));
            }

            if (isMenu.HasValue)
                query = query.Where(f => f.IsMenu == isMenu.Value);
            if (isStatus.HasValue)
                query = query.Where(f => f.IsStatus == isStatus.Value);
            if (isShow.HasValue)
                query = query.Where(f => f.IsShow == isShow.Value);
            if (parentId.HasValue)
                query = query.Where(f => f.ParentId == parentId.Value);
            if (siteId.HasValue)
                query = query.Where(f => f.SiteId == siteId.Value);

            query = query.OrderBy(f => f.ParentId).ThenBy(f => f.Sort).ThenBy(f => f.FunctionId);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            var items = await query.Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new SysFunctionListItemVm
                {
                    FunctionId = f.FunctionId,
                    FunctionName = f.FunctionName,
                    URL = f.URL,
                    IsMenu = f.IsMenu,
                    IsStatus = f.IsStatus,
                    IsShow = f.IsShow,
                    HrefName = f.HrefName,
                    ParentId = f.ParentId,
                    Sort = f.Sort,
                    SiteId = f.SiteId,
                    Domain = f.Domain,
                    LanguageId = f.LanguageId
                })
                .ToListAsync();

            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Keyword = keyword;
            ViewBag.IsMenu = isMenu;
            ViewBag.IsStatus = isStatus;
            ViewBag.IsShow = isShow;
            ViewBag.ParentId = parentId;
            ViewBag.SiteId = siteId;
            ViewBag.Sites = await _context.RootSites.AsNoTracking().OrderBy(s => s.SiteId).ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ListPartial", items);

            return View(items);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.SysFunctions.AsNoTracking()
                .FirstOrDefaultAsync(f => f.FunctionId == id);
            if (entity == null)
                return NotFound();

            return View(new SysFunctionEditVm
            {
                FunctionId = entity.FunctionId,
                FunctionName = entity.FunctionName ?? "",
                URL = entity.URL,
                IsMenu = entity.IsMenu,
                IsStatus = entity.IsStatus,
                IsShow = entity.IsShow,
                Domain = entity.Domain,
                HrefName = entity.HrefName,
                ParentId = entity.ParentId,
                Sort = entity.Sort,
                SiteId = entity.SiteId,
                LanguageId = entity.LanguageId
            });
        }

        public async Task<IActionResult> Create()
        {
            var nextId = (await _context.SysFunctions.AsNoTracking().MaxAsync(f => (int?)f.FunctionId) ?? 0) + 1;
            var nextSort = (await _context.SysFunctions.AsNoTracking()
                .Where(f => f.ParentId == 0)
                .MaxAsync(f => (int?)f.Sort) ?? 0) + 1;

            return View(new SysFunctionCreateVm
            {
                FunctionId = nextId,
                IsMenu = true,
                IsStatus = false,
                IsShow = true,
                ParentId = 0,
                Sort = nextSort,
                SiteId = GetSiteId(),
                LanguageId = 1
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SysFunctionCreateVm model)
        {
            if (await _context.SysFunctions.AnyAsync(f => f.FunctionId == model.FunctionId))
                ModelState.AddModelError(nameof(model.FunctionId), "FunctionId đã tồn tại.");

            if (!ModelState.IsValid)
                return View(model);

            var entity = new SysFunction
            {
                FunctionId = model.FunctionId,
                FunctionName = model.FunctionName.Trim(),
                URL = string.IsNullOrWhiteSpace(model.URL) ? null : model.URL.Trim(),
                IsMenu = model.IsMenu,
                IsStatus = model.IsStatus,
                IsShow = model.IsShow,
                Domain = string.IsNullOrWhiteSpace(model.Domain) ? null : model.Domain.Trim(),
                HrefName = string.IsNullOrWhiteSpace(model.HrefName) ? null : model.HrefName.Trim(),
                ParentId = model.ParentId,
                Sort = model.Sort,
                SiteId = model.SiteId,
                LanguageId = model.LanguageId
            };

            _context.SysFunctions.Add(entity);
            await _context.SaveChangesAsync();

            await WriteLog(1, $"Create SysFunction: ID={entity.FunctionId}; {Snapshot(entity.FunctionName, entity.URL, entity.IsMenu, entity.IsStatus, entity.IsShow, entity.HrefName, entity.ParentId, entity.Sort, entity.SiteId)}");

            TempData["Success"] = "Đã thêm menu/tính năng hệ thống.";
            return RedirectToAction(nameof(Edit), new { id = entity.FunctionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SysFunctionEditVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var entity = await _context.SysFunctions.FirstOrDefaultAsync(f => f.FunctionId == model.FunctionId);
            if (entity == null)
                return NotFound();

            var oldSummary = Snapshot(entity.FunctionName, entity.URL, entity.IsMenu, entity.IsStatus,
                entity.IsShow, entity.HrefName, entity.ParentId, entity.Sort, entity.SiteId);

            entity.FunctionName = model.FunctionName.Trim();
            entity.URL = string.IsNullOrWhiteSpace(model.URL) ? null : model.URL.Trim();
            entity.IsMenu = model.IsMenu;
            entity.IsStatus = model.IsStatus;
            entity.IsShow = model.IsShow;
            entity.Domain = string.IsNullOrWhiteSpace(model.Domain) ? null : model.Domain.Trim();
            entity.HrefName = string.IsNullOrWhiteSpace(model.HrefName) ? null : model.HrefName.Trim();
            entity.ParentId = model.ParentId;
            entity.Sort = model.Sort;
            entity.SiteId = model.SiteId;
            entity.LanguageId = model.LanguageId;

            await _context.SaveChangesAsync();

            var newSummary = Snapshot(entity.FunctionName, entity.URL, entity.IsMenu, entity.IsStatus,
                entity.IsShow, entity.HrefName, entity.ParentId, entity.Sort, entity.SiteId);
            await WriteLog(2, $"Update SysFunction: ID={entity.FunctionId}; {oldSummary} -> {newSummary}");

            TempData["Success"] = "Đã cập nhật tính năng hệ thống.";
            return RedirectToAction(nameof(Edit), new { id = entity.FunctionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.SysFunctions.FirstOrDefaultAsync(f => f.FunctionId == id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy SysFunction." });

            var permissionCount = await _context.Database
                .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM dbo.SysFuncRolesStatusPermission WHERE FunctionId = {0}", id)
                .SingleAsync();

            if (permissionCount > 0)
                return Json(new { success = false, message = $"Không thể xóa vì còn {permissionCount} dòng phân quyền." });

            var summary = Snapshot(entity.FunctionName, entity.URL, entity.IsMenu, entity.IsStatus,
                entity.IsShow, entity.HrefName, entity.ParentId, entity.Sort, entity.SiteId);

            _context.SysFunctions.Remove(entity);
            await _context.SaveChangesAsync();

            await WriteLog(3, $"Delete SysFunction: ID={id}; {summary}");

            return Json(new { success = true, message = "Đã xóa SysFunction." });
        }

        private static string Snapshot(string? name, string? url, bool? isMenu, bool? isStatus, bool isShow, string? href, int? parentId, int? sort, int? siteId)
        {
            return $"Name='{Shorten(name)}', URL='{Shorten(url)}', IsMenu={isMenu}, IsStatus={isStatus}, IsShow={isShow}, Href='{Shorten(href)}', ParentId={parentId}, Sort={sort}, SiteId={siteId}";
        }

        private static string Shorten(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            return value.Length <= 80 ? value : value[..80] + "...";
        }
    }
}
