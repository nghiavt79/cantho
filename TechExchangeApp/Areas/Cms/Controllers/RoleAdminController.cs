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
    public class RoleAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "Roles";
        private static readonly string[] LogFunctionAliases = { "Role", "Nhóm quyền", "Phân quyền" };

        public RoleAdminController(AppDbContext context, IConfiguration configuration)
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
            string? keyword, int? siteId, int page = 1, int pageSize = 20)
        {
            if (!new[] { 10, 20, 50, 100 }.Contains(pageSize))
                pageSize = 20;

            var query = _context.CmsRoles.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(r =>
                    r.RoleId.ToString().Contains(kw) ||
                    (r.Title != null && r.Title.ToLower().Contains(kw)) ||
                    (r.Description != null && r.Description.ToLower().Contains(kw)));
            }

            if (siteId.HasValue)
                query = query.Where(r => r.SiteId == siteId.Value);

            query = query.OrderBy(r => r.RoleId);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            var items = await query.Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RoleListItemVm
                {
                    RoleId = r.RoleId,
                    Title = r.Title,
                    Description = r.Description,
                    SiteId = r.SiteId,
                    LanguageId = r.LanguageId,
                    ParentId = r.ParentId,
                    Domain = r.Domain,
                    PermissionCount = _context.SysFuncRolePermissions.Count(p => p.RoleId == r.RoleId)
                })
                .ToListAsync();

            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Keyword = keyword;
            ViewBag.SiteId = siteId;
            ViewBag.Sites = await _context.RootSites.AsNoTracking().OrderBy(s => s.SiteId).ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ListPartial", items);

            return View(items);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.CmsRoles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoleId == id);
            if (entity == null)
                return NotFound();

            return View(new RoleEditVm
            {
                RoleId = entity.RoleId,
                Title = entity.Title ?? "",
                Description = entity.Description,
                Domain = entity.Domain,
                LanguageId = entity.LanguageId,
                ParentId = entity.ParentId,
                SiteId = entity.SiteId
            });
        }

        public IActionResult Create()
        {
            return View(new RoleCreateVm
            {
                LanguageId = 1,
                ParentId = 0,
                SiteId = GetSiteId()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleCreateVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var entity = new CmsRole
            {
                Title = model.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                Domain = string.IsNullOrWhiteSpace(model.Domain) ? null : model.Domain.Trim(),
                LanguageId = model.LanguageId,
                ParentId = model.ParentId,
                SiteId = model.SiteId
            };

            _context.CmsRoles.Add(entity);
            await _context.SaveChangesAsync();

            await WriteLog(1, $"Create Role: ID={entity.RoleId}; {Snapshot(entity.Title, entity.Description, entity.ParentId, entity.SiteId)}");

            TempData["Success"] = "Đã thêm nhóm quyền.";
            return RedirectToAction(nameof(Edit), new { id = entity.RoleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleEditVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var entity = await _context.CmsRoles.FirstOrDefaultAsync(r => r.RoleId == model.RoleId);
            if (entity == null)
                return NotFound();

            var oldSummary = Snapshot(entity.Title, entity.Description, entity.ParentId, entity.SiteId);

            entity.Title = model.Title.Trim();
            entity.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            entity.Domain = string.IsNullOrWhiteSpace(model.Domain) ? null : model.Domain.Trim();
            entity.LanguageId = model.LanguageId;
            entity.ParentId = model.ParentId;
            entity.SiteId = model.SiteId;

            await _context.SaveChangesAsync();

            var newSummary = Snapshot(entity.Title, entity.Description, entity.ParentId, entity.SiteId);
            await WriteLog(2, $"Update Role: ID={entity.RoleId}; {oldSummary} -> {newSummary}");

            TempData["Success"] = "Đã cập nhật nhóm quyền.";
            return RedirectToAction(nameof(Edit), new { id = entity.RoleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.CmsRoles.FirstOrDefaultAsync(r => r.RoleId == id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy nhóm quyền." });

            var permissionCount = await _context.SysFuncRolePermissions.CountAsync(p => p.RoleId == id);
            if (permissionCount > 0)
                return Json(new { success = false, message = $"Không thể xóa vì còn {permissionCount} dòng phân quyền. Hãy bỏ phân quyền của nhóm này trước." });

            var userCount = await _context.CmsUserRoles.CountAsync(u => u.RoleId == id);
            if (userCount > 0)
                return Json(new { success = false, message = $"Không thể xóa vì còn {userCount} người dùng thuộc nhóm quyền này." });

            var summary = Snapshot(entity.Title, entity.Description, entity.ParentId, entity.SiteId);

            _context.CmsRoles.Remove(entity);
            await _context.SaveChangesAsync();

            await WriteLog(3, $"Delete Role: ID={id}; {summary}");

            return Json(new { success = true, message = "Đã xóa nhóm quyền." });
        }

        private static string Snapshot(string? title, string? description, int? parentId, int? siteId)
        {
            return $"Title='{Shorten(title)}', Desc='{Shorten(description)}', ParentId={parentId}, SiteId={siteId}";
        }

        private static string Shorten(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            return value.Length <= 80 ? value : value[..80] + "...";
        }
    }
}
