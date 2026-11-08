using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Services;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class ProjectsAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly INotificationQueueService _notificationService;
        private const string LogFunctionName = "Projects";
        private static readonly string[] LogFunctionAliases = { "Dự án" };

        public ProjectsAdminController(
            AppDbContext context,
            IConfiguration configuration,
            INotificationQueueService notificationService)
        {
            _context = context;
            _configuration = configuration;
            _notificationService = notificationService;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private async Task WriteLog(int eventId, string content)
        {
            await CmsLogHelper.WriteLogAsync(_context, HttpContext, GetSiteId(),
                LogFunctionName, LogFunctionAliases, eventId, content);
        }

        // ─── INDEX ───
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 15)
        {
            // Base query — just projects
            var query = _context.Projects.AsNoTracking().AsQueryable();

            // Text search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();

                // Khớp mã, tên hồ sơ hoặc tên thành viên, bên bán được mời
                query = query.Where(p =>
                    p.ProjectCode.ToLower().Contains(s) ||
                    p.ProjectName.ToLower().Contains(s) ||
                    _context.ProjectMembers.Any(m => m.ProjectId == p.Id && m.IsActive &&
                        _context.Users.Any(u => u.Id == m.UserId && u.FullName != null && u.FullName.ToLower().Contains(s))) ||
                    _context.RFQInvitations.Any(i => i.ProjectId == p.Id && i.IsActive &&
                        _context.Users.Any(u => u.Id == i.SellerId && u.FullName != null && u.FullName.ToLower().Contains(s))));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Sort
            bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy?.ToLower()) switch
            {
                "code" => asc ? query.OrderBy(p => p.ProjectCode) : query.OrderByDescending(p => p.ProjectCode),
                "name" => asc ? query.OrderBy(p => p.ProjectName) : query.OrderByDescending(p => p.ProjectName),
                "created" => asc ? query.OrderBy(p => p.CreatedDate) : query.OrderByDescending(p => p.CreatedDate),
                _ => query.OrderByDescending(p => p.CreatedDate)
            };

            var rawList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Chỉ nạp bước, thành viên, lời mời của các hồ sơ trên trang
            var pageIds = rawList.Select(p => p.Id).ToList();
            var allSteps = await _context.ProjectSteps.AsNoTracking()
                .Where(ps => pageIds.Contains(ps.ProjectId))
                .OrderBy(ps => ps.ProjectId).ThenBy(ps => ps.StepNumber)
                .Select(ps => new { ps.ProjectId, ps.StepNumber, ps.StepName, ps.StatusId })
                .ToListAsync();
            var stepsByProject = allSteps.GroupBy(s => s.ProjectId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Buyer=1, Seller=2, Consultant=3
            var allMembers = await _context.ProjectMembers.AsNoTracking()
                .Where(pm => pm.IsActive && pageIds.Contains(pm.ProjectId))
                .Join(_context.Users.AsNoTracking(),
                    pm => pm.UserId, u => u.Id,
                    (pm, u) => new { pm.Id, pm.ProjectId, pm.Role, u.FullName, UserId = u.Id })
                .ToListAsync();

            // Bên bán được mời chưa là thành viên hồ sơ nhưng vẫn phải hiện ở cột Bên bán
            var allInvited = await _context.RFQInvitations.AsNoTracking()
                .Where(i => i.IsActive && pageIds.Contains(i.ProjectId))
                .Join(_context.Users.AsNoTracking(), i => i.SellerId, u => u.Id,
                    (i, u) => new { i.ProjectId, u.FullName })
                .ToListAsync();

            // Build view models
            var items = rawList.Select(p =>
            {
                // Determine current step using same logic as DashboardService
                var steps = stepsByProject.ContainsKey(p.Id)
                    ? stepsByProject[p.Id] : new();

                int currentStep = 0;
                string stepName = "";

                if (steps.Any())
                {
                    var lastCompleted = steps
                        .Where(s => s.StatusId == 2) // 2 = Completed
                        .OrderByDescending(s => s.StepNumber)
                        .FirstOrDefault();

                    if (lastCompleted == null)
                    {
                        // Nothing completed → show step 1
                        var first = steps.OrderBy(s => s.StepNumber).First();
                        currentStep = first.StepNumber;
                        stepName = first.StepName;
                    }
                    else
                    {
                        var next = steps.FirstOrDefault(s => s.StepNumber > lastCompleted.StepNumber);
                        if (next != null)
                        {
                            currentStep = next.StepNumber;
                            stepName = next.StepName;
                        }
                        else
                        {
                            // All steps completed
                            currentStep = lastCompleted.StepNumber;
                            stepName = lastCompleted.StepName;
                        }
                    }
                }

                var buyers = allMembers
                    .Where(m => m.ProjectId == p.Id && m.Role == 1)
                    .Select(m => m.FullName ?? "").ToList();

                var sellers = allMembers
                    .Where(m => m.ProjectId == p.Id && m.Role == 2)
                    .Select(m => m.FullName ?? "")
                    .Concat(allInvited.Where(i => i.ProjectId == p.Id).Select(i => i.FullName ?? ""))
                    .Distinct().ToList();

                var consultants = allMembers
                    .Where(m => m.ProjectId == p.Id && m.Role == 3)
                    .Select(m => new ConsultantInfo { Id = m.Id, Name = m.FullName ?? "" })
                    .ToList();

                return new CmsProjectListItem
                {
                    ProjectId = p.Id,
                    ProjectCode = p.ProjectCode,
                    ProjectName = p.ProjectName,
                    CurrentStep = currentStep,
                    CurrentStepName = stepName,
                    BuyerNames = buyers,
                    SellerNames = sellers,
                    Consultants = consultants,
                    CreatedDate = p.CreatedDate
                };
            }).ToList();

            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ProjectListPartial", items);

            return View(items);
        }

        // ─── DELETE ───
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return Json(new { success = false, message = "Không tìm thấy dự án." });

            // Remove related data
            _context.ProjectMembers.RemoveRange(
                _context.ProjectMembers.Where(pm => pm.ProjectId == id));
            _context.ProjectSteps.RemoveRange(
                _context.ProjectSteps.Where(ps => ps.ProjectId == id));
            _context.ProjectWorkflowStates.RemoveRange(
                _context.ProjectWorkflowStates.Where(ws => ws.ProjectId == id));
            _context.ProjectStepStates.RemoveRange(
                _context.ProjectStepStates.Where(ss => ss.ProjectId == id));
            _context.ProjectAccessLogs.RemoveRange(
                _context.ProjectAccessLogs.Where(l => l.ProjectId == id));

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            await WriteLog(3, $"Delete Project: {project.ProjectCode} (ID={id})");
            return Json(new { success = true });
        }

        // ─── NHẬT KÝ HỒ SƠ (HSMT A.8.5): chuyển bước, truy cập, tải tệp ───
        [HttpGet]
        // Tham số lọc không được đặt tên "action": trùng giá trị route nên luôn nhận "Logs" và trả danh sách rỗng
        public async Task<IActionResult> Logs(int id, string? logAction = null)
        {
            var project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (project == null) return NotFound();

            var transitions = await (from l in _context.WorkflowTransitionLogs.AsNoTracking()
                                     join u in _context.Users.AsNoTracking() on l.ActorUserId equals u.Id into uj
                                     from u in uj.DefaultIfEmpty()
                                     where l.ProjectId == id
                                     select new ProjectLogRow
                                     {
                                         Time = l.CreatedAt.ToLocalTime(), Kind = "Chuyển bước", Action = l.Action,
                                         UserId = l.ActorUserId,
                                         UserName = u != null ? (u.FullName ?? u.UserName) : "#" + l.ActorUserId,
                                         Detail = l.Message, Ip = l.IpAddress
                                     }).ToListAsync();
            var accessQ = from l in _context.ProjectAccessLogs.AsNoTracking()
                          join u in _context.Users.AsNoTracking() on l.UserId equals u.Id into uj
                          from u in uj.DefaultIfEmpty()
                          where l.ProjectId == id
                          select new { l, u };
            if (!string.IsNullOrWhiteSpace(logAction)) accessQ = accessQ.Where(x => x.l.Action == logAction);
            var access = await accessQ.OrderByDescending(x => x.l.Timestamp).Take(1000)
                .Select(x => new ProjectLogRow
                {
                    Time = x.l.Timestamp, Kind = x.l.Action == "DownloadFile" ? "Tải tệp" : "Truy cập", Action = x.l.Action,
                    UserId = x.l.UserId,
                    UserName = x.u != null ? (x.u.FullName ?? x.u.UserName) : "#" + x.l.UserId,
                    Detail = x.l.AdditionalData, Ip = x.l.IpAddress
                }).ToListAsync();

            var rows = (string.IsNullOrWhiteSpace(logAction) ? transitions : new List<ProjectLogRow>())
                .Concat(access).OrderByDescending(r => r.Time).ToList();

            // Vai trò theo hồ sơ: thành viên, bên được mời hoặc bên bán đã chọn; còn lại là cán bộ Sàn
            var roles = await _context.ProjectMembers.AsNoTracking().Where(m => m.ProjectId == id)
                .Select(m => new { m.UserId, m.Role }).ToListAsync();
            var invitedSellers = await _context.RFQInvitations.AsNoTracking().Where(i => i.ProjectId == id)
                .Select(i => i.SellerId).ToListAsync();
            foreach (var r in rows)
            {
                var role = roles.FirstOrDefault(m => m.UserId == r.UserId)?.Role
                    ?? (invitedSellers.Contains(r.UserId) || project.SelectedSellerId == r.UserId ? 2 : 0);
                r.Role = role switch { 1 => "Bên mua", 2 => "Bên bán", 3 => "Nhà tư vấn", _ => "Cán bộ Sàn" };
            }
            ViewBag.Project = project;
            ViewBag.ActionFilter = logAction;
            ViewBag.Actions = await _context.ProjectAccessLogs.AsNoTracking().Where(l => l.ProjectId == id)
                .Select(l => l.Action).Distinct().OrderBy(a => a).ToListAsync();
            return View(rows);
        }

        // ─── ADD CONSULTANT (GET) ───
        [HttpGet]
        public async Task<IActionResult> AddConsultant(int projectId)
        {
            var project = await _context.Projects.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound();

            // Get already assigned consultant user IDs (Role=3)
            var assignedUserIds = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId && pm.Role == 3 && pm.IsActive)
                .Select(pm => pm.UserId)
                .ToListAsync();

            // Get UserId of all active NhaTuVan
            var nhaTuVanUserIds = await _context.NhaTuVans.AsNoTracking()
                .Where(n => n.UserId != null && n.IsActivated == true)
                .Select(n => n.UserId!.Value)
                .ToListAsync();

            // Get Users: IsAdmin=true OR is NhaTuVan, exclude already assigned, same SiteId
            var siteId = GetSiteId();
            var available = await _context.Users.AsNoTracking()
                .Where(u => u.SiteId == siteId
                    && (u.IsAdmin == true || nhaTuVanUserIds.Contains(u.Id))
                    && !assignedUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName })
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = project.ProjectName;
            ViewBag.AvailableConsultants = available
                .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.FullName
                }).ToList();

            return PartialView("_AddConsultantPartial");
        }

        // ─── ADD CONSULTANT (POST) ───
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConsultant(int projectId, int consultantUserId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return Json(new { success = false, message = "Không tìm thấy dự án." });

            // Check if user already exists in this project (any role)
            var existingMember = await _context.ProjectMembers.FirstOrDefaultAsync(pm =>
                pm.ProjectId == projectId && pm.UserId == consultantUserId && pm.IsActive);

            if (existingMember != null)
            {
                var roleName = existingMember.Role switch { 1 => "Buyer", 2 => "Seller", 3 => "Nhà tư vấn", _ => "thành viên" };
                return Json(new { success = false, message = $"Người dùng này đã là {roleName} của dự án." });
            }

            _context.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = projectId,
                UserId = consultantUserId,
                Role = 3, // Consultant
                JoinedDate = DateTime.Now,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var userName = await _context.Users.AsNoTracking()
                .Where(u => u.Id == consultantUserId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync();

            await WriteLog(1, $"Add Consultant '{userName}' to Project '{project.ProjectCode}' (ProjectId={projectId})");

            // Notify the consultant
            await _notificationService.QueueAsync(
                consultantUserId,
                projectId,
                "Bạn đã được thêm vào dự án",
                $"Bạn đã được thêm vào dự án <strong>{project.ProjectName}</strong> ({project.ProjectCode}) với vai trò Nhà tư vấn.");

            return Json(new { success = true, message = $"Đã thêm nhà tư vấn '{userName}' vào dự án." });
        }

        // ─── GÁN BÊN BÁN (GET): nhà cung ứng đã xuất bản, kích hoạt và có tài khoản ───
        [HttpGet]
        public async Task<IActionResult> AddSeller(int projectId)
        {
            var project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound();

            var invited = await _context.RFQInvitations.AsNoTracking()
                .Where(i => i.ProjectId == projectId && i.IsActive)
                .Join(_context.Users.AsNoTracking(), i => i.SellerId, u => u.Id,
                    (i, u) => new { i.Id, i.SellerId, u.FullName, u.UserName })
                .ToListAsync();
            var invitedIds = invited.Select(i => i.SellerId).ToList();

            var available = await _context.NhaCungUngs.AsNoTracking()
                .Where(n => n.UserId != null && n.IsActivated == true && n.StatusId == 3
                    && n.LanguageId == 1 && !invitedIds.Contains(n.UserId!.Value))
                .OrderBy(n => n.FullName)
                .Select(n => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = n.UserId!.Value.ToString(),
                    Text = n.FullName
                })
                .ToListAsync();

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = project.ProjectName;
            ViewBag.Available = available;
            ViewBag.Invited = invited
                .Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = i.Id.ToString(),
                    Text = $"{i.FullName} ({i.UserName})"
                }).ToList();
            ViewBag.HasRfq = await _context.RFQRequests.AnyAsync(r => r.ProjectId == projectId);
            return PartialView("_AddSellerPartial");
        }

        // ─── GÁN BÊN BÁN (POST): tạo lời mời như khi bên mua mời ở bước 3 ───
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSeller(int projectId, int sellerUserId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return Json(new { success = false, message = "Không tìm thấy dự án." });

            var rfq = await _context.RFQRequests.FirstOrDefaultAsync(r => r.ProjectId == projectId);
            if (rfq == null)
                return Json(new { success = false, message = "Hồ sơ chưa có yêu cầu báo giá (bước 3) nên chưa gán được bên bán." });

            var isValidSeller = await _context.NhaCungUngs.AnyAsync(n =>
                n.UserId == sellerUserId && n.IsActivated == true && n.StatusId == 3);
            if (!isValidSeller)
                return Json(new { success = false, message = "Hồ sơ nhà cung ứng chưa ở trạng thái Xuất bản và Kích hoạt." });

            var invitation = await _context.RFQInvitations.FirstOrDefaultAsync(i =>
                i.ProjectId == projectId && i.RFQId == rfq.Id && i.SellerId == sellerUserId);
            if (invitation == null)
            {
                _context.RFQInvitations.Add(new RFQInvitation
                {
                    ProjectId = projectId,
                    RFQId = rfq.Id,
                    SellerId = sellerUserId,
                    InvitedDate = DateTime.Now,
                    StatusId = 0,
                    IsActive = true
                });
            }
            else
            {
                invitation.IsActive = true;
                invitation.InvitedDate = DateTime.Now;
                invitation.StatusId = 0;
            }
            rfq.StatusId = 2; // Sent
            await _context.SaveChangesAsync();

            await WriteLog(1, $"Add Seller #{sellerUserId} to Project '{project.ProjectCode}' (ProjectId={projectId})");
            await _notificationService.QueueAsync(sellerUserId, null,
                "📨 Bạn được mời nộp hồ sơ báo giá",
                "Bạn được mời nộp hồ sơ báo giá. Hãy vào mục 'Lời mời tham gia' để xem chi tiết và nộp hồ sơ trước hạn.");

            return Json(new { success = true, message = "Đã gán bên bán vào hồ sơ." });
        }

        // ─── THU HỒI BÊN BÁN: chỉ khi chưa nộp hồ sơ đề xuất ───
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSeller(int invitationId)
        {
            var invitation = await _context.RFQInvitations.FindAsync(invitationId);
            if (invitation == null)
                return Json(new { success = false, message = "Không tìm thấy lời mời." });

            var hasProposal = await _context.ProposalSubmissions.AnyAsync(p =>
                p.ProjectId == invitation.ProjectId && p.NguoiTao == invitation.SellerId);
            if (hasProposal)
                return Json(new { success = false, message = "Bên bán đã nộp hồ sơ đề xuất, không thu hồi được." });

            invitation.IsActive = false;
            await _context.SaveChangesAsync();
            await WriteLog(3, $"Remove Seller #{invitation.SellerId} from ProjectId={invitation.ProjectId}");
            return Json(new { success = true, message = "Đã thu hồi lời mời." });
        }
        // ─── REMOVE CONSULTANT ───
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveConsultant(int id)
        {
            var pm = await _context.ProjectMembers.FindAsync(id);
            if (pm == null || pm.Role != 3)
                return Json(new { success = false, message = "Không tìm thấy bản ghi." });

            var projectCode = await _context.Projects.AsNoTracking()
                .Where(p => p.Id == pm.ProjectId)
                .Select(p => p.ProjectCode)
                .FirstOrDefaultAsync();

            _context.ProjectMembers.Remove(pm);
            await _context.SaveChangesAsync();

            await WriteLog(3, $"Remove Consultant (MemberId={id}) from Project '{projectCode}'");
            return Json(new { success = true });
        }
    }

    // ─── View Models ───
    public class CmsProjectListItem
    {
        public int ProjectId { get; set; }
        public string ProjectCode { get; set; } = "";
        public string ProjectName { get; set; } = "";
        public int CurrentStep { get; set; }
        public string CurrentStepName { get; set; } = "";
        public List<string> BuyerNames { get; set; } = new();
        public List<string> SellerNames { get; set; } = new();
        public List<ConsultantInfo> Consultants { get; set; } = new();
        public DateTime CreatedDate { get; set; }
    }

    public class ConsultantInfo
    {
        public int Id { get; set; } // ProjectMember.Id
        public string Name { get; set; } = "";
    }

    public class ProjectLogRow
    {
        public DateTime Time { get; set; }
        public string Kind { get; set; } = "";
        public string Action { get; set; } = "";
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Role { get; set; }
        public string? Detail { get; set; }
        public string? Ip { get; set; }
    }
}
