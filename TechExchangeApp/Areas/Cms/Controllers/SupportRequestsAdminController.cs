using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Enums;
using TechExchangeApp.Interfaces;
using TechExchangeApp.Services;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>Inbox CMS xử lý "Yêu cầu Sàn hỗ trợ" (ChatConversation type=2).</summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class SupportRequestsAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IChatService _chatService;
        private readonly INotificationQueueService _notifQueue;

        public SupportRequestsAdminController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IChatService chatService,
            INotificationQueueService notifQueue)
        {
            _context = context;
            _userManager = userManager;
            _chatService = chatService;
            _notifQueue = notifQueue;
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            var u = await _userManager.GetUserAsync(User);
            return u?.Id ?? 0;
        }

        // GET /cms/SupportRequestsAdmin/Index?status=&search=
        public async Task<IActionResult> Index(int? status, string? search)
        {
            var currentUserId = await GetCurrentUserIdAsync();

            var q = _context.ChatConversations.AsNoTracking()
                .Where(c => c.ConversationType == (int)ChatConversationType.PlatformSupport);

            // status: 1 = chưa gán, 2 = đang xử lý, -1 = của tôi
            if (status == 1) q = q.Where(c => c.SupportStatus == (int)SupportConversationStatus.Unassigned);
            else if (status == 2) q = q.Where(c => c.SupportStatus == (int)SupportConversationStatus.InProgress);
            else if (status == -1) q = q.Where(c => c.AssignedStaffUserId == currentUserId);

            var convs = await q
                .OrderByDescending(c => c.LastMessageAt ?? c.Created)
                .Take(300)
                .ToListAsync();

            // Resolve names in batch
            var userIds = convs.Select(c => ParseId(c.BuyerUserId))
                .Concat(convs.Where(c => c.AssignedStaffUserId.HasValue).Select(c => c.AssignedStaffUserId!.Value))
                .Where(id => id > 0).Distinct().ToList();
            var userNames = await _context.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.FullName })
                .ToDictionaryAsync(u => u.Id, u => string.IsNullOrEmpty(u.FullName) ? u.UserName : u.FullName);

            var projectIds = convs.Where(c => c.ProjectId.HasValue).Select(c => c.ProjectId!.Value).Distinct().ToList();
            var projectNames = await _context.Projects.AsNoTracking()
                .Where(p => projectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProjectName);

            var convIds = convs.Select(c => c.Id).ToList();
            var lastMsgs = await _context.ChatMessages.AsNoTracking()
                .Where(m => convIds.Contains(m.ConversationId) && !m.IsSystem)
                .GroupBy(m => m.ConversationId)
                .Select(g => new { ConversationId = g.Key, Message = g.OrderByDescending(x => x.Created).Select(x => x.Message).FirstOrDefault() })
                .ToDictionaryAsync(x => x.ConversationId, x => x.Message);

            var items = convs.Select(c => new SupportInboxItemVm
            {
                Id = c.Id,
                ProjectId = c.ProjectId,
                ProjectName = c.ProjectId.HasValue && projectNames.TryGetValue(c.ProjectId.Value, out var pn) ? pn : (c.ProductName ?? "—"),
                StepNumber = c.StepNumber,
                RequesterName = userNames.TryGetValue(ParseId(c.BuyerUserId), out var rn) ? rn : "Người dùng",
                AssignedStaffName = c.AssignedStaffUserId.HasValue && userNames.TryGetValue(c.AssignedStaffUserId.Value, out var sn) ? sn : null,
                SupportStatus = c.SupportStatus,
                LastMessage = lastMsgs.TryGetValue(c.Id, out var lm) ? lm : null,
                LastMessageAt = c.LastMessageAt
            }).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var kw = search.Trim().ToLowerInvariant();
                items = items.Where(i =>
                    (i.ProjectName ?? "").ToLowerInvariant().Contains(kw) ||
                    (i.RequesterName ?? "").ToLowerInvariant().Contains(kw)).ToList();
            }

            ViewBag.Status = status;
            ViewBag.Search = search;
            return View(items);
        }

        // GET /cms/SupportRequestsAdmin/Thread/{id}
        public async Task<IActionResult> Thread(long id)
        {
            var conv = await _context.ChatConversations.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.ConversationType == (int)ChatConversationType.PlatformSupport);
            if (conv == null) return NotFound();

            var messages = await _context.ChatMessages.AsNoTracking()
                .Where(m => m.ConversationId == id)
                .OrderBy(m => m.Created)
                .ToListAsync();

            var senderIds = messages.Select(m => ParseId(m.SenderUserId)).Where(x => x > 0).Distinct().ToList();
            if (conv.AssignedStaffUserId.HasValue) senderIds.Add(conv.AssignedStaffUserId.Value);
            senderIds.Add(ParseId(conv.BuyerUserId));
            var names = await _context.Users.AsNoTracking()
                .Where(u => senderIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.FullName })
                .ToDictionaryAsync(u => u.Id, u => string.IsNullOrEmpty(u.FullName) ? u.UserName : u.FullName);

            string? projectName = null;
            if (conv.ProjectId.HasValue)
            {
                projectName = await _context.Projects.AsNoTracking()
                    .Where(p => p.Id == conv.ProjectId.Value).Select(p => p.ProjectName).FirstOrDefaultAsync();
            }

            var vm = new SupportThreadVm
            {
                Id = conv.Id,
                ProjectId = conv.ProjectId,
                ProjectName = projectName ?? conv.ProductName ?? "—",
                StepNumber = conv.StepNumber,
                RequesterName = names.TryGetValue(ParseId(conv.BuyerUserId), out var rn) ? rn : "Người dùng",
                AssignedStaffUserId = conv.AssignedStaffUserId,
                AssignedStaffName = conv.AssignedStaffUserId.HasValue && names.TryGetValue(conv.AssignedStaffUserId.Value, out var sn) ? sn : null,
                SupportStatus = conv.SupportStatus,
                Messages = messages.Select(m => new SupportThreadMsgVm
                {
                    SenderName = names.TryGetValue(ParseId(m.SenderUserId), out var mn) ? mn : "Người dùng",
                    IsSystem = m.IsSystem,
                    Message = m.Message,
                    Created = m.Created
                }).ToList(),
                StaffOptions = await GetStaffOptionsAsync()
            };

            return View(vm);
        }

        // POST /cms/SupportRequestsAdmin/Reply
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
            if (message.Length > 2000) message = message.Substring(0, 2000);

            var result = await _chatService.ReplySupportAsync(id, currentUserId, message.Trim());
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

        // POST /cms/SupportRequestsAdmin/Assign — điều phối gán yêu cầu cho một tư vấn viên cụ thể
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(long id, int staffUserId)
        {
            var conv = await _context.ChatConversations
                .FirstOrDefaultAsync(c => c.Id == id && c.ConversationType == (int)ChatConversationType.PlatformSupport);
            if (conv == null) return NotFound();

            if (staffUserId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một nhân viên/tư vấn hợp lệ.";
                return RedirectToAction(nameof(Thread), new { id });
            }
            if (conv.SupportStatus == (int)SupportConversationStatus.Closed)
            {
                TempData["ErrorMessage"] = "Yêu cầu đã đóng, không thể gán.";
                return RedirectToAction(nameof(Thread), new { id });
            }

            conv.AssignedStaffUserId = staffUserId;
            conv.SupplierUserId = staffUserId.ToString(); // sync để tư vấn thấy trong /chat của họ
            conv.SupportStatus = (int)SupportConversationStatus.InProgress;
            await _context.SaveChangesAsync();

            try
            {
                await _notifQueue.QueueAsync(staffUserId, conv.ProjectId,
                    "Bạn được giao xử lý yêu cầu hỗ trợ",
                    "Bạn vừa được giao phụ trách một yêu cầu hỗ trợ dự án.",
                    NotificationChannel.Email, $"/cms/SupportRequestsAdmin/Thread/{id}");
            }
            catch { /* non-critical */ }

            TempData["SuccessMessage"] = "Đã gán yêu cầu cho nhân viên/tư vấn.";
            return RedirectToAction(nameof(Thread), new { id });
        }

        // POST /cms/SupportRequestsAdmin/Close — đóng yêu cầu hỗ trợ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(long id)
        {
            var conv = await _context.ChatConversations
                .FirstOrDefaultAsync(c => c.Id == id && c.ConversationType == (int)ChatConversationType.PlatformSupport);
            if (conv == null) return NotFound();

            conv.SupportStatus = (int)SupportConversationStatus.Closed;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã đóng yêu cầu hỗ trợ.";
            return RedirectToAction(nameof(Thread), new { id });
        }

        // Danh sách tư vấn viên (NhaTuVan có tài khoản) cho dropdown gán
        private async Task<List<SupportStaffOptionVm>> GetStaffOptionsAsync()
        {
            return await _context.NhaTuVans.AsNoTracking()
                .Where(n => n.UserId != null && n.UserId > 0)
                .OrderBy(n => n.FullName)
                .Select(n => new SupportStaffOptionVm { UserId = n.UserId!.Value, Name = n.FullName })
                .ToListAsync();
        }

        private static int ParseId(string? s) => int.TryParse(s, out var v) ? v : 0;
    }

    public class SupportInboxItemVm
    {
        public long Id { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public int? StepNumber { get; set; }
        public string RequesterName { get; set; } = "";
        public string? AssignedStaffName { get; set; }
        public int SupportStatus { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }

    public class SupportThreadVm
    {
        public long Id { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public int? StepNumber { get; set; }
        public string RequesterName { get; set; } = "";
        public int? AssignedStaffUserId { get; set; }
        public string? AssignedStaffName { get; set; }
        public int SupportStatus { get; set; }
        public List<SupportThreadMsgVm> Messages { get; set; } = new();
        public List<SupportStaffOptionVm> StaffOptions { get; set; } = new();
    }

    public class SupportStaffOptionVm
    {
        public int UserId { get; set; }
        public string Name { get; set; } = "";
    }

    public class SupportThreadMsgVm
    {
        public string SenderName { get; set; } = "";
        public bool IsSystem { get; set; }
        public string Message { get; set; } = "";
        public DateTime Created { get; set; }
    }
}
