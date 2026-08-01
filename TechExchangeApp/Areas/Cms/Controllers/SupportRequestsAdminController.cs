using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Enums;
using TechExchangeApp.Interfaces;
using TechExchangeApp.Services;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class SupportRequestsAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IChatService _chatService;
        private readonly INotificationQueueService _notifQueue;
        private readonly IExcelExportService _excelExport;
        private readonly IConfiguration _configuration;

        public SupportRequestsAdminController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IChatService chatService,
            INotificationQueueService notifQueue,
            IExcelExportService excelExport,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _chatService = chatService;
            _notifQueue = notifQueue;
            _excelExport = excelExport;
            _configuration = configuration;
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            var u = await _userManager.GetUserAsync(User);
            return u?.Id ?? 0;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        public async Task<IActionResult> Index(int? status, int? requestType, int? serviceType, int? staffUserId, int? projectId, DateTime? dateFrom, DateTime? dateTo, string? search)
        {
            var currentUserId = await GetCurrentUserIdAsync();

            var q = _context.SupportRequests.AsNoTracking();

            if (status.HasValue && status.Value > 0) q = q.Where(r => r.Status == status.Value);
            else if (status == -1) q = q.Where(r => r.AssignedStaffUserId == currentUserId);

            if (requestType.HasValue && requestType.Value > 0) q = q.Where(r => r.RequestType == requestType.Value);
            if (serviceType.HasValue && serviceType.Value > 0) q = q.Where(r => r.ServiceType == serviceType.Value);
            if (staffUserId.HasValue && staffUserId.Value > 0) q = q.Where(r => r.AssignedStaffUserId == staffUserId.Value);
            if (projectId.HasValue && projectId.Value > 0) q = q.Where(r => r.ProjectId == projectId.Value);
            if (dateFrom.HasValue) q = q.Where(r => r.CreatedAt >= dateFrom.Value.Date);
            if (dateTo.HasValue) q = q.Where(r => r.CreatedAt < dateTo.Value.Date.AddDays(1));

            ViewBag.TotalCount = await q.CountAsync();
            ViewBag.NewCount = await q.CountAsync(r => r.Status == (int)SupportRequestStatus.New);
            ViewBag.UnassignedCount = await q.CountAsync(r => !r.AssignedStaffUserId.HasValue || r.AssignedStaffUserId <= 0);
            ViewBag.CompletedCount = await q.CountAsync(r => r.Status == (int)SupportRequestStatus.Completed);

            var tickets = await q
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .Take(300)
                .ToListAsync();

            var userIds = tickets.Select(t => t.RequestedByUserId)
                .Concat(tickets.Where(t => t.AssignedStaffUserId.HasValue).Select(t => t.AssignedStaffUserId!.Value))
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            var userNames = await _context.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.FullName })
                .ToDictionaryAsync(u => u.Id, u => string.IsNullOrEmpty(u.FullName) ? u.UserName : u.FullName);

            var projectIds = tickets.Select(t => t.ProjectId).Distinct().ToList();
            var projectNames = await _context.Projects.AsNoTracking()
                .Where(p => projectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProjectName);

            var convIds = tickets.Where(t => t.ConversationId.HasValue).Select(t => t.ConversationId!.Value).ToList();
            var lastMsgs = convIds.Any()
                ? await _context.ChatMessages.AsNoTracking()
                    .Where(m => convIds.Contains(m.ConversationId) && !m.IsSystem)
                    .GroupBy(m => m.ConversationId)
                    .Select(g => new { ConversationId = g.Key, Message = g.OrderByDescending(x => x.Created).Select(x => x.Message).FirstOrDefault() })
                    .ToDictionaryAsync(x => x.ConversationId, x => x.Message)
                : new Dictionary<long, string?>();

            var items = tickets.Select(t => new SupportInboxItemVm
            {
                Id = t.Id,
                ConversationId = t.ConversationId,
                ProjectId = t.ProjectId,
                ProjectName = projectNames.TryGetValue(t.ProjectId, out var pn) ? pn : $"Hồ sơ #{t.ProjectId}",
                StepNumber = t.DisplayStepNumber ?? t.InternalStepNumber,
                RequesterName = userNames.TryGetValue(t.RequestedByUserId, out var rn) ? rn ?? "" : $"User #{t.RequestedByUserId}",
                AssignedStaffName = t.AssignedStaffUserId.HasValue && userNames.TryGetValue(t.AssignedStaffUserId.Value, out var sn) ? sn : null,
                RequestType = t.RequestType,
                ServiceType = t.ServiceType,
                TicketStatus = t.Status,
                Subject = t.Subject,
                SupportContextCode = t.SupportContextCode,
                LastMessage = t.ConversationId.HasValue && lastMsgs.TryGetValue(t.ConversationId.Value, out var lm) ? lm : t.Description,
                LastMessageAt = t.UpdatedAt ?? t.CreatedAt
            }).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var kw = search.Trim().ToLowerInvariant();
                items = items.Where(i =>
                    (i.ProjectName ?? "").ToLowerInvariant().Contains(kw) ||
                    (i.RequesterName ?? "").ToLowerInvariant().Contains(kw) ||
                    (i.Subject ?? "").ToLowerInvariant().Contains(kw)).ToList();
            }

            ViewBag.Status = status;
            ViewBag.RequestType = requestType;
            ViewBag.ServiceType = serviceType;
            ViewBag.StaffUserId = staffUserId;
            ViewBag.ProjectId = projectId;
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;
            ViewBag.Search = search;
            ViewBag.StaffOptions = await GetStaffOptionsAsync();
            return View(items);
        }

        public async Task<IActionResult> ExportExcel(int? status, int? requestType, int? serviceType, int? staffUserId, int? projectId, DateTime? dateFrom, DateTime? dateTo, string? search)
        {
            var currentUserId = await GetCurrentUserIdAsync();
            var q = _context.SupportRequests.AsNoTracking();

            if (status.HasValue && status.Value > 0) q = q.Where(r => r.Status == status.Value);
            else if (status == -1) q = q.Where(r => r.AssignedStaffUserId == currentUserId);
            if (requestType.HasValue && requestType.Value > 0) q = q.Where(r => r.RequestType == requestType.Value);
            if (serviceType.HasValue && serviceType.Value > 0) q = q.Where(r => r.ServiceType == serviceType.Value);
            if (staffUserId.HasValue && staffUserId.Value > 0) q = q.Where(r => r.AssignedStaffUserId == staffUserId.Value);
            if (projectId.HasValue && projectId.Value > 0) q = q.Where(r => r.ProjectId == projectId.Value);
            if (dateFrom.HasValue) q = q.Where(r => r.CreatedAt >= dateFrom.Value.Date);
            if (dateTo.HasValue) q = q.Where(r => r.CreatedAt < dateTo.Value.Date.AddDays(1));

            var tickets = await q.OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt).Take(2000).ToListAsync();
            var rows = await BuildExportRowsAsync(tickets, search);
            return _excelExport.Export(rows, $"SupportRequests_{DateTime.Now:yyyyMMddHHmm}");
        }

        private async Task<List<SupportExportRowVm>> BuildExportRowsAsync(List<SupportRequest> tickets, string? search)
        {
            var userIds = tickets.Select(t => t.RequestedByUserId)
                .Concat(tickets.Where(t => t.AssignedStaffUserId.HasValue).Select(t => t.AssignedStaffUserId!.Value))
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            var userNames = await _context.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.FullName })
                .ToDictionaryAsync(u => u.Id, u => string.IsNullOrEmpty(u.FullName) ? u.UserName : u.FullName);

            var projectIds = tickets.Select(t => t.ProjectId).Distinct().ToList();
            var projectNames = await _context.Projects.AsNoTracking()
                .Where(p => projectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProjectName);

            var rows = tickets.Select(t => new SupportExportRowVm
            {
                Code = $"REQ-{t.Id}",
                ProjectId = t.ProjectId,
                ProjectName = projectNames.TryGetValue(t.ProjectId, out var pn) ? pn : $"Hồ sơ #{t.ProjectId}",
                RequestType = RequestTypeText(t.RequestType),
                ServiceType = ServiceTypeText(t.ServiceType),
                Status = StatusText(t.Status),
                RequesterName = userNames.TryGetValue(t.RequestedByUserId, out var rn) ? rn ?? "" : $"User #{t.RequestedByUserId}",
                AssignedStaffName = t.AssignedStaffUserId.HasValue && userNames.TryGetValue(t.AssignedStaffUserId.Value, out var sn) ? sn ?? "" : "",
                StepNumber = t.DisplayStepNumber ?? t.InternalStepNumber,
                SupportContextCode = t.SupportContextCode,
                Subject = t.Subject,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ClosedAt = t.ClosedAt
            }).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var kw = search.Trim().ToLowerInvariant();
                rows = rows.Where(i =>
                    (i.ProjectName ?? "").ToLowerInvariant().Contains(kw) ||
                    (i.RequesterName ?? "").ToLowerInvariant().Contains(kw) ||
                    (i.Subject ?? "").ToLowerInvariant().Contains(kw)).ToList();
            }

            return rows;
        }

        private static string StatusText(int s) => s switch
        {
            (int)SupportRequestStatus.New => "Mới tiếp nhận",
            (int)SupportRequestStatus.Assigned => "Đã phân công",
            (int)SupportRequestStatus.InProgress => "Đang xử lý",
            (int)SupportRequestStatus.WaitingForUser => "Chờ doanh nghiệp bổ sung",
            (int)SupportRequestStatus.Responded => "Đã phản hồi",
            (int)SupportRequestStatus.Completed => "Đã hoàn thành",
            (int)SupportRequestStatus.Cancelled => "Đã hủy",
            _ => "-"
        };

        private static string RequestTypeText(int t) =>
            t == (int)SupportRequestType.TransactionConsulting ? "Tư vấn giao dịch/chuyển giao" : "Hỗ trợ sử dụng hệ thống";

        private static string ServiceTypeText(int? t) => t switch
        {
            (int)SupportServiceType.SupplierMatching => "Tìm kiếm, lựa chọn nhà cung cấp",
            (int)SupportServiceType.ProposalEvaluation => "Đánh giá hồ sơ đề xuất",
            (int)SupportServiceType.NegotiationAdvice => "Tư vấn đàm phán thương mại",
            (int)SupportServiceType.LegalReview => "Kiểm tra pháp lý hợp đồng",
            (int)SupportServiceType.ElectronicContractSupport => "Hỗ trợ hợp đồng điện tử",
            (int)SupportServiceType.Other => "Khác",
            _ => "-"
        };

        public async Task<IActionResult> Thread(long id)
        {
            var ticket = await _context.SupportRequests.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
            if (ticket == null) return NotFound();

            var conv = ticket.ConversationId.HasValue
                ? await _context.ChatConversations.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == ticket.ConversationId.Value)
                : null;

            var messages = conv != null
                ? await _context.ChatMessages.AsNoTracking()
                    .Where(m => m.ConversationId == conv.Id)
                    .OrderBy(m => m.Created)
                    .ToListAsync()
                : new List<ChatMessage>();

            var senderIds = messages.Select(m => ParseId(m.SenderUserId))
                .Concat(new[] { ticket.RequestedByUserId })
                .Where(x => x > 0)
                .Distinct()
                .ToList();
            if (ticket.AssignedStaffUserId.HasValue) senderIds.Add(ticket.AssignedStaffUserId.Value);

            var names = await _context.Users.AsNoTracking()
                .Where(u => senderIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.FullName })
                .ToDictionaryAsync(u => u.Id, u => string.IsNullOrEmpty(u.FullName) ? u.UserName : u.FullName);

            var projectName = await _context.Projects.AsNoTracking()
                .Where(p => p.Id == ticket.ProjectId)
                .Select(p => p.ProjectName)
                .FirstOrDefaultAsync();

            var vm = new SupportThreadVm
            {
                Id = ticket.Id,
                ConversationId = ticket.ConversationId,
                ProjectId = ticket.ProjectId,
                ProjectName = projectName ?? $"Hồ sơ #{ticket.ProjectId}",
                StepNumber = ticket.DisplayStepNumber ?? ticket.InternalStepNumber,
                RequesterName = names.TryGetValue(ticket.RequestedByUserId, out var rn) ? rn ?? "" : $"User #{ticket.RequestedByUserId}",
                AssignedStaffUserId = ticket.AssignedStaffUserId,
                AssignedStaffName = ticket.AssignedStaffUserId.HasValue && names.TryGetValue(ticket.AssignedStaffUserId.Value, out var sn) ? sn : null,
                RequestType = ticket.RequestType,
                ServiceType = ticket.ServiceType,
                TicketStatus = ticket.Status,
                Subject = ticket.Subject,
                SupportContextCode = ticket.SupportContextCode,
                Description = ticket.Description,
                Messages = messages.Select(m => new SupportThreadMsgVm
                {
                    SenderName = names.TryGetValue(ParseId(m.SenderUserId), out var mn) ? mn ?? "" : "Người dùng",
                    IsSystem = m.IsSystem,
                    Message = m.Message,
                    Created = m.Created
                }).ToList(),
                StaffOptions = await GetStaffOptionsAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(long id, string message)
        {
            var currentUserId = await GetCurrentUserIdAsync();
            if (currentUserId == 0) return Forbid();

            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = "Nội dung trả lời không được để trống.";
                return RedirectToAction(nameof(Thread), new { id });
            }
            if (message.Length > 2000) message = message[..2000];

            var ticket = await _context.SupportRequests.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
            if (ticket == null) return NotFound();
            if (!ticket.ConversationId.HasValue)
            {
                TempData["ErrorMessage"] = "Ticket chưa có hội thoại liên kết.";
                return RedirectToAction(nameof(Thread), new { id });
            }

            var result = await _chatService.ReplySupportAsync(ticket.ConversationId.Value, currentUserId, message.Trim());
            switch (result.Outcome)
            {
                case SupportReplyOutcome.Ok:
                    TempData["SuccessMessage"] = "Đã gửi phản hồi.";
                    break;
                case SupportReplyOutcome.AssignedToOther:
                    TempData["ErrorMessage"] = $"Yêu cầu đang do {result.AssignedStaffName ?? "người khác"} phụ trách.";
                    break;
                case SupportReplyOutcome.NotFound:
                    return NotFound();
                case SupportReplyOutcome.EmptyMessage:
                    TempData["ErrorMessage"] = "Nội dung trả lời không được để trống.";
                    break;
            }
            return RedirectToAction(nameof(Thread), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(long id, int status)
        {
            var currentUserId = await GetCurrentUserIdAsync();
            if (currentUserId == 0) return Forbid();

            var validStatuses = new[]
            {
                (int)SupportRequestStatus.New,
                (int)SupportRequestStatus.Assigned,
                (int)SupportRequestStatus.InProgress,
                (int)SupportRequestStatus.WaitingForUser,
                (int)SupportRequestStatus.Responded,
                (int)SupportRequestStatus.Completed,
                (int)SupportRequestStatus.Cancelled
            };
            if (!validStatuses.Contains(status))
            {
                TempData["ErrorMessage"] = "Trạng thái không hợp lệ.";
                return RedirectToAction(nameof(Thread), new { id });
            }

            var ticket = await _context.SupportRequests
                .FirstOrDefaultAsync(r => r.Id == id);
            if (ticket == null) return NotFound();

            var now = DateTime.UtcNow;
            ticket.Status = status;
            ticket.UpdatedAt = now;
            ticket.LastStatusChangedByUserId = currentUserId;
            ticket.LastStatusChangedAt = now;
            ticket.ClosedAt = status == (int)SupportRequestStatus.Completed || status == (int)SupportRequestStatus.Cancelled
                ? now
                : null;

            if (ticket.ConversationId.HasValue)
            {
                var conv = await _context.ChatConversations.FirstOrDefaultAsync(c => c.Id == ticket.ConversationId.Value);
                if (conv != null) conv.SupportStatus = MapTicketStatusToConversationStatus(status, ticket.AssignedStaffUserId);
            }

            await _context.SaveChangesAsync();

            if (status == (int)SupportRequestStatus.Responded ||
                status == (int)SupportRequestStatus.Completed ||
                status == (int)SupportRequestStatus.Cancelled)
            {
                try
                {
                    await _notifQueue.QueueAsync(ticket.RequestedByUserId, ticket.ProjectId,
                        "Cập nhật yêu cầu hỗ trợ/tư vấn",
                        "Trung tâm vừa cập nhật trạng thái yêu cầu hỗ trợ/tư vấn của bạn.",
                        NotificationChannel.Email,
                        ticket.ConversationId.HasValue ? $"/chat/{ticket.ConversationId.Value}" : "/chat");
                }
                catch { /* non-critical */ }
            }

            TempData["SuccessMessage"] = "Đã cập nhật trạng thái yêu cầu.";
            return RedirectToAction(nameof(Thread), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(long id, int staffUserId)
        {
            var ticket = await _context.SupportRequests
                .FirstOrDefaultAsync(r => r.Id == id);
            if (ticket == null) return NotFound();

            if (staffUserId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một nhân viên/tư vấn hợp lệ.";
                return RedirectToAction(nameof(Thread), new { id });
            }
            var staffUser = await _context.Users.AsNoTracking()
                .Where(u => u.Id == staffUserId
                    && u.IsAdmin == true
                    && u.SiteId == GetSiteId()
                    && (u.IsActivated == null || u.IsActivated == true))
                .Select(u => new { u.Id })
                .FirstOrDefaultAsync();
            if (staffUser == null)
            {
                TempData["ErrorMessage"] = "Vui lÃ²ng chá»n má»™t nhÃ¢n viÃªn sÃ n há»£p lá»‡.";
                return RedirectToAction(nameof(Thread), new { id });
            }

            if (ticket.Status == (int)SupportRequestStatus.Completed || ticket.Status == (int)SupportRequestStatus.Cancelled)
            {
                TempData["ErrorMessage"] = "Yêu cầu đã kết thúc, không thể gán.";
                return RedirectToAction(nameof(Thread), new { id });
            }

            var now = DateTime.UtcNow;
            ticket.AssignedStaffUserId = staffUserId;
            ticket.AssignedAt ??= now;
            ticket.Status = (int)SupportRequestStatus.Assigned;
            ticket.UpdatedAt = now;
            ticket.LastStatusChangedByUserId = await GetCurrentUserIdAsync();
            ticket.LastStatusChangedAt = now;

            if (ticket.ConversationId.HasValue)
            {
                var conv = await _context.ChatConversations.FirstOrDefaultAsync(c => c.Id == ticket.ConversationId.Value);
                if (conv != null)
                {
                    conv.AssignedStaffUserId = staffUserId;
                    conv.SupplierUserId = staffUserId.ToString();
                    conv.SupportStatus = MapTicketStatusToConversationStatus(ticket.Status, ticket.AssignedStaffUserId);
                }
            }

            await EnsureProjectConsultantAsync(ticket.ProjectId, staffUserId);
            await _context.SaveChangesAsync();

            try
            {
                await _notifQueue.QueueAsync(staffUserId, ticket.ProjectId,
                    "Bạn được giao xử lý yêu cầu hỗ trợ/tư vấn",
                    "Bạn vừa được giao phụ trách một yêu cầu hỗ trợ/tư vấn.",
                    NotificationChannel.Email, $"/cms/SupportRequestsAdmin/Thread/{id}");
            }
            catch { /* non-critical */ }

            TempData["SuccessMessage"] = "Đã gán yêu cầu cho nhân viên/tư vấn.";
            return RedirectToAction(nameof(Thread), new { id });
        }

        private async Task EnsureProjectConsultantAsync(int projectId, int staffUserId)
        {
            var existingConsultant = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId
                    && pm.UserId == staffUserId
                    && pm.Role == 3);

            if (existingConsultant != null)
            {
                if (!existingConsultant.IsActive)
                {
                    existingConsultant.IsActive = true;
                    existingConsultant.JoinedDate = DateTime.Now;
                }
                return;
            }

            _context.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = projectId,
                UserId = staffUserId,
                Role = 3,
                JoinedDate = DateTime.Now,
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(long id, int status = (int)SupportRequestStatus.Completed)
        {
            var ticket = await _context.SupportRequests
                .FirstOrDefaultAsync(r => r.Id == id);
            if (ticket == null) return NotFound();

            var closedStatus = status == (int)SupportRequestStatus.Cancelled
                ? (int)SupportRequestStatus.Cancelled
                : (int)SupportRequestStatus.Completed;
            var now = DateTime.UtcNow;
            ticket.Status = closedStatus;
            ticket.ClosedAt = now;
            ticket.UpdatedAt = now;
            ticket.LastStatusChangedByUserId = await GetCurrentUserIdAsync();
            ticket.LastStatusChangedAt = now;

            if (ticket.ConversationId.HasValue)
            {
                var conv = await _context.ChatConversations.FirstOrDefaultAsync(c => c.Id == ticket.ConversationId.Value);
                if (conv != null) conv.SupportStatus = (int)SupportConversationStatus.Closed;
            }

            await _context.SaveChangesAsync();

            try
            {
                await _notifQueue.QueueAsync(ticket.RequestedByUserId, ticket.ProjectId,
                    "Yêu cầu hỗ trợ/tư vấn đã được cập nhật",
                    closedStatus == (int)SupportRequestStatus.Cancelled
                        ? "Trung tâm đã hủy yêu cầu hỗ trợ/tư vấn của bạn."
                        : "Trung tâm đã hoàn thành yêu cầu hỗ trợ/tư vấn của bạn.",
                    NotificationChannel.Email,
                    ticket.ConversationId.HasValue ? $"/chat/{ticket.ConversationId.Value}" : "/chat");
            }
            catch { /* non-critical */ }

            TempData["SuccessMessage"] = "Đã đóng yêu cầu hỗ trợ/tư vấn.";
            return RedirectToAction(nameof(Thread), new { id });
        }

        private static int MapTicketStatusToConversationStatus(int ticketStatus, int? assignedStaffUserId)
        {
            if (ticketStatus == (int)SupportRequestStatus.Completed ||
                ticketStatus == (int)SupportRequestStatus.Cancelled)
                return (int)SupportConversationStatus.Closed;

            if (assignedStaffUserId.HasValue && assignedStaffUserId.Value > 0)
                return (int)SupportConversationStatus.InProgress;

            return (int)SupportConversationStatus.Unassigned;
        }

        private async Task<List<SupportStaffOptionVm>> GetStaffOptionsAsync()
        {
            var siteId = GetSiteId();
            return await _context.Users.AsNoTracking()
                .Where(u => u.SiteId == siteId
                    && u.IsAdmin == true
                    && (u.IsActivated == null || u.IsActivated == true))
                .OrderBy(u => u.FullName ?? u.UserName)
                .Select(u => new SupportStaffOptionVm
                {
                    UserId = u.Id,
                    Name = string.IsNullOrEmpty(u.FullName) ? (u.UserName ?? $"User #{u.Id}") : u.FullName
                })
                .ToListAsync();
        }

        private static int ParseId(string? s) => int.TryParse(s, out var v) ? v : 0;
    }

    public class SupportInboxItemVm
    {
        public long Id { get; set; }
        public long? ConversationId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public int? StepNumber { get; set; }
        public string RequesterName { get; set; } = "";
        public string? AssignedStaffName { get; set; }
        public int RequestType { get; set; }
        public int? ServiceType { get; set; }
        public int TicketStatus { get; set; }
        public string? Subject { get; set; }
        public string? SupportContextCode { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }

    public class SupportThreadVm
    {
        public long Id { get; set; }
        public long? ConversationId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public int? StepNumber { get; set; }
        public string RequesterName { get; set; } = "";
        public int? AssignedStaffUserId { get; set; }
        public string? AssignedStaffName { get; set; }
        public int RequestType { get; set; }
        public int? ServiceType { get; set; }
        public int TicketStatus { get; set; }
        public string? Subject { get; set; }
        public string? SupportContextCode { get; set; }
        public string? Description { get; set; }
        public List<SupportThreadMsgVm> Messages { get; set; } = new();
        public List<SupportStaffOptionVm> StaffOptions { get; set; } = new();
    }

    public class SupportStaffOptionVm
    {
        public int UserId { get; set; }
        public string Name { get; set; } = "";
    }

    public class SupportExportRowVm
    {
        [Display(Name = "Mã yêu cầu")]
        public string Code { get; set; } = "";
        [Display(Name = "Mã hồ sơ")]
        public int ProjectId { get; set; }
        [Display(Name = "Hồ sơ")]
        public string ProjectName { get; set; } = "";
        [Display(Name = "Loại yêu cầu")]
        public string RequestType { get; set; } = "";
        [Display(Name = "Dịch vụ tư vấn")]
        public string ServiceType { get; set; } = "";
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "";
        [Display(Name = "Người gửi")]
        public string RequesterName { get; set; } = "";
        [Display(Name = "Phụ trách")]
        public string AssignedStaffName { get; set; } = "";
        [Display(Name = "Bước")]
        public int? StepNumber { get; set; }
        [Display(Name = "Mã ngữ cảnh")]
        public string? SupportContextCode { get; set; }
        [Display(Name = "Tiêu đề")]
        public string? Subject { get; set; }
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }
        [Display(Name = "Ngày đóng")]
        public DateTime? ClosedAt { get; set; }
    }

    public class SupportThreadMsgVm
    {
        public string SenderName { get; set; } = "";
        public bool IsSystem { get; set; }
        public string Message { get; set; } = "";
        public DateTime Created { get; set; }
    }
}
