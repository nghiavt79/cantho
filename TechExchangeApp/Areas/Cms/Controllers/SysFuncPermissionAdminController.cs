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
    public class SysFuncPermissionAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "SysFuncRolesStatusPermission";
        private static readonly string[] LogFunctionAliases = { "Phân quyền", "Permission", "Roles" };
        // AuditNumber không được app dùng để check quyền; đặt mặc định "đủ quyền" khi thêm dòng mới.
        private const int DefaultAuditNumber = 63;

        public SysFuncPermissionAdminController(AppDbContext context, IConfiguration configuration)
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

        public async Task<IActionResult> Index(int? roleId)
        {
            var roleList = await _context.CmsRoles.AsNoTracking()
                .OrderBy(r => r.RoleId)
                .Select(r => new { r.RoleId, r.Title })
                .ToListAsync();

            var vm = new SysFuncPermissionMatrixVm
            {
                Roles = roleList.Select(r => (r.RoleId, r.Title ?? $"Role #{r.RoleId}")).ToList()
            };

            // Cột trạng thái: 0 = Chung (function không theo status) + các Status thực tế.
            vm.StatusColumns.Add(new PermissionStatusColumn { StatusId = 0, Title = "Chung" });
            var statuses = await _context.Statuses.AsNoTracking()
                .OrderBy(s => s.StatusId)
                .Select(s => new PermissionStatusColumn { StatusId = s.StatusId, Title = s.Title ?? $"#{s.StatusId}" })
                .ToListAsync();
            vm.StatusColumns.AddRange(statuses);

            vm.SelectedRoleId = roleId;
            if (roleId.HasValue)
            {
                vm.SelectedRoleTitle = vm.Roles.FirstOrDefault(r => r.RoleId == roleId.Value).Title;

                var granted = (await _context.SysFuncRolePermissions.AsNoTracking()
                        .Where(p => p.RoleId == roleId.Value)
                        .Select(p => new { p.FunctionId, p.StatusId })
                        .ToListAsync())
                    .GroupBy(p => p.FunctionId)
                    .ToDictionary(g => g.Key, g => g.Select(x => (int)x.StatusId).ToHashSet());

                vm.Rows = await _context.SysFunctions.AsNoTracking()
                    .Where(f => f.IsShow)
                    .OrderBy(f => f.ParentId).ThenBy(f => f.Sort).ThenBy(f => f.FunctionId)
                    .Select(f => new SysFuncPermissionRowVm
                    {
                        FunctionId = f.FunctionId,
                        FunctionName = f.FunctionName,
                        URL = f.URL,
                        IsStatusBased = f.IsStatus == true,
                        ParentId = f.ParentId
                    })
                    .ToListAsync();

                foreach (var row in vm.Rows)
                {
                    if (granted.TryGetValue(row.FunctionId, out var set))
                        row.GrantedStatusIds = set;
                }
            }

            return View(vm);
        }

        // granted: mỗi phần tử "functionId:statusId" ứng với 1 ô được tick.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int roleId, List<string>? granted)
        {
            var role = await _context.CmsRoles.AsNoTracking().FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (role == null)
            {
                TempData["Error"] = "Không tìm thấy nhóm quyền.";
                return RedirectToAction(nameof(Index));
            }

            // Tập ô mong muốn (functionId, statusId) sau khi lưu.
            var desired = new HashSet<(int fid, byte sid)>();
            foreach (var key in granted ?? new List<string>())
            {
                var parts = key.Split(':');
                if (parts.Length == 2
                    && int.TryParse(parts[0], out var fid)
                    && byte.TryParse(parts[1], out var sid))
                {
                    desired.Add((fid, sid));
                }
            }

            // Chỉ xử lý trong phạm vi các function đang hiển thị (IsShow=true) — giữ nguyên
            // quyền của các function bị ẩn, không đụng tới khi lưu.
            var visibleFuncIds = await _context.SysFunctions.AsNoTracking()
                .Where(f => f.IsShow)
                .Select(f => f.FunctionId)
                .ToListAsync();
            var visibleSet = visibleFuncIds.ToHashSet();

            var existing = await _context.SysFuncRolePermissions
                .Where(p => p.RoleId == roleId && visibleFuncIds.Contains(p.FunctionId))
                .ToListAsync();
            var existingKeys = existing.Select(p => (p.FunctionId, p.StatusId)).ToHashSet();

            // Bỏ qua ô của function không hiển thị (phòng dữ liệu post bất thường).
            desired.RemoveWhere(d => !visibleSet.Contains(d.fid));

            // Xóa ô bị bỏ tick.
            var toRemove = existing.Where(p => !desired.Contains((p.FunctionId, p.StatusId))).ToList();
            _context.SysFuncRolePermissions.RemoveRange(toRemove);

            // Thêm ô mới tick (giữ nguyên AuditNumber cho ô đã có).
            var added = 0;
            foreach (var (fid, sid) in desired)
            {
                if (existingKeys.Contains((fid, sid)))
                    continue;
                _context.SysFuncRolePermissions.Add(new SysFuncRolePermission
                {
                    FunctionId = fid,
                    RoleId = roleId,
                    StatusId = sid,
                    AuditNumber = DefaultAuditNumber,
                    Domain = role.Domain,
                    LanguageId = role.LanguageId ?? 1,
                    ParentId = role.ParentId ?? 0,
                    SiteId = role.SiteId
                });
                added++;
            }

            await _context.SaveChangesAsync();

            await WriteLog(2, $"Update Permission: RoleId={roleId} ('{role.Title}'); +{added} / -{toRemove.Count}; tổng {desired.Count} quyền.");

            TempData["Success"] = $"Đã lưu phân quyền cho nhóm '{role.Title}' (thêm {added}, bỏ {toRemove.Count}).";
            return RedirectToAction(nameof(Index), new { roleId });
        }
    }
}
