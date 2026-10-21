using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Interfaces;
using TechExchangeApp.Localization;

namespace TechExchangeApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IAuthorizationService _authz;

        /// <summary>Loại tệp cho phép đính kèm trong chat: ảnh, PDF, văn bản Office.</summary>
        private static readonly Dictionary<string, string> AllowedAttachments = new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".png"] = "image/png", [".gif"] = "image/gif", [".webp"] = "image/webp",
            [".pdf"] = "application/pdf",
            [".doc"] = "application/msword", [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            [".xls"] = "application/vnd.ms-excel", [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            [".ppt"] = "application/vnd.ms-powerpoint", [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        };
        private const long MaxAttachmentBytes = 10 * 1024 * 1024;

        public ChatController(IChatService chatService, UserManager<ApplicationUser> userManager,
            AppDbContext context, IWebHostEnvironment env, IAuthorizationService authz)
        {
            _chatService = chatService;
            _userManager = userManager;
            _context = context;
            _env = env;
            _authz = authz;
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id ?? 0;
        }

        // ── POST /api/chat/start ────────────────────────────────────────────
        [HttpPost("/api/chat/start")]
        public async Task<IActionResult> StartConversation([FromBody] ChatStartRequest request)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return Unauthorized(new { success = false, error = I18n.T(HttpContext, "chat.err.loginRequired") });

            var result = await _chatService.StartConversationAsync(request.ProductId, userId);

            if (!result.Success)
                return BadRequest(new { success = false, error = result.Error });

            return Ok(new
            {
                success = true,
                conversationId = result.ConversationId,
                isNew = result.IsNew,
                redirectUrl = $"/chat/{result.ConversationId}"
            });
        }

        // ── GET /chat ───────────────────────────────────────────────────────
        [HttpGet("/chat")]
        public async Task<IActionResult> Index()
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return Redirect("/dang-nhap");

            var conversations = await _chatService.GetConversationsAsync(userId);
            return View(conversations);
        }

        // ── GET /chat/{id} ──────────────────────────────────────────────────
        [HttpGet("/chat/{id:long}")]
        public async Task<IActionResult> Conversation(long id)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return Redirect("/dang-nhap");

            // Mark messages as read
            await _chatService.MarkAsReadAsync(id, userId);

            var detail = await _chatService.GetConversationAsync(id, userId);
            if (detail == null) return NotFound();

            return View(detail);
        }

        // ── POST /api/chat/{id}/send ────────────────────────────────────────
        [HttpPost("/api/chat/{id:long}/send")]
        public async Task<IActionResult> SendMessage(long id, [FromBody] ChatSendRequest request)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return Unauthorized(new { success = false });

            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { success = false, error = I18n.T(HttpContext, "chat.err.emptyMessage") });

            var ok = await _chatService.SendMessageAsync(id, userId, request.Message.Trim());
            if (!ok) return BadRequest(new { success = false, error = I18n.T(HttpContext, "chat.err.conversationNotFound") });

            return Ok(new { success = true });
        }

        // ── POST /api/chat/{id}/upload — gửi hình ảnh, tệp (HSMT A.5.3, C.7) ──
        [HttpPost("/api/chat/{id:long}/upload")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxAttachmentBytes + 1024 * 1024)]
        public async Task<IActionResult> UploadAttachment(long id, IFormFile? file, [FromForm] string? message)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return Unauthorized(new { success = false });
            if (file == null || file.Length == 0) return BadRequest(new { success = false, error = "Chưa chọn tệp." });
            if (file.Length > MaxAttachmentBytes) return BadRequest(new { success = false, error = "Tệp vượt quá 10 MB." });

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || !AllowedAttachments.TryGetValue(ext, out var contentType))
                return BadRequest(new { success = false, error = "Chỉ gửi được ảnh, PDF hoặc văn bản Word, Excel, PowerPoint." });

            // Chỉ thành viên cuộc trò chuyện được gửi
            var uid = userId.ToString();
            var isMember = await _context.ChatConversations.AsNoTracking()
                .AnyAsync(c => c.Id == id && (c.BuyerUserId == uid || c.SupplierUserId == uid));
            if (!isMember) return BadRequest(new { success = false, error = I18n.T(HttpContext, "chat.err.conversationNotFound") });

            // Lưu ngoài wwwroot, tên ngẫu nhiên
            var relDir = Path.Combine("App_Data", "chat", id.ToString());
            var absDir = Path.Combine(_env.ContentRootPath, relDir);
            Directory.CreateDirectory(absDir);
            var stored = Guid.NewGuid().ToString("N") + ext.ToLowerInvariant();
            await using (var fs = System.IO.File.Create(Path.Combine(absDir, stored)))
                await file.CopyToAsync(fs);

            var name = Path.GetFileName(file.FileName);
            if (name.Length > 255) name = name[^255..];
            var text = string.IsNullOrWhiteSpace(message) ? "📎 " + name : message.Trim();
            var ok = await _chatService.SendMessageAsync(id, userId, text, new ChatAttachmentInfo
            {
                Path = Path.Combine(relDir, stored).Replace('\\', '/'),
                Name = name,
                ContentType = contentType,
                Size = file.Length
            });
            if (!ok) return BadRequest(new { success = false, error = I18n.T(HttpContext, "chat.err.conversationNotFound") });
            return Ok(new { success = true });
        }

        // ── GET /chat/tep/{messageId} — tải tệp đính kèm, kiểm tra quyền ─────
        [HttpGet("/chat/tep/{messageId:long}")]
        public async Task<IActionResult> DownloadAttachment(long messageId)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return Redirect("/dang-nhap");

            var msg = await _context.ChatMessages.AsNoTracking()
                .Where(m => m.Id == messageId && m.AttachmentPath != null)
                .Select(m => new { m.ConversationId, m.AttachmentPath, m.AttachmentName, m.AttachmentType })
                .FirstOrDefaultAsync();
            if (msg == null) return NotFound();

            // Thành viên cuộc trò chuyện, cán bộ được giao hỗ trợ, hoặc người có quyền quản trị (xem lịch sử chat)
            var uid = userId.ToString();
            var allowed = await _context.ChatConversations.AsNoTracking()
                .AnyAsync(c => c.Id == msg.ConversationId && (c.BuyerUserId == uid || c.SupplierUserId == uid || c.AssignedStaffUserId == userId));
            if (!allowed)
                allowed = (await _authz.AuthorizeAsync(User, "CmsAccess")).Succeeded;
            if (!allowed) return Forbid();

            var abs = Path.GetFullPath(Path.Combine(_env.ContentRootPath, msg.AttachmentPath!));
            var root = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "App_Data", "chat"));
            if (!abs.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(abs)) return NotFound();

            _context.Logs.Add(new Log
            {
                FunctionID = 0, ActTime = DateTime.Now, EventID = TechExchangeApp.Infrastructure.DownloadAuditMiddleware.EventDownload,
                Content = $"Tải tệp chat: {msg.AttachmentName} (hội thoại #{msg.ConversationId})",
                ClientIP = HttpContext.Connection.RemoteIpAddress?.ToString(), UserName = User.Identity?.Name, Domain = Request.Host.Value
            });
            await _context.SaveChangesAsync();

            Response.Headers["Cache-Control"] = "private, no-store";
            var type = msg.AttachmentType ?? "application/octet-stream";
            return type.StartsWith("image/")
                ? PhysicalFile(abs, type)
                : PhysicalFile(abs, type, msg.AttachmentName ?? Path.GetFileName(abs));
        }

        // ── GET /api/chat/unread-count ──────────────────────────────────────
        [HttpGet("/api/chat/unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return Ok(new { count = 0 });

            var count = await _chatService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        // ── GET /api/chat/{id}/messages?afterId=0 ───────────────────────────
        [HttpGet("/api/chat/{id:long}/messages")]
        public async Task<IActionResult> GetMessages(long id, [FromQuery] long afterId = 0)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return Unauthorized();

            var detail = await _chatService.GetConversationAsync(id, userId);
            if (detail == null) return NotFound();

            // Mark as read
            await _chatService.MarkAsReadAsync(id, userId);

            var newMessages = detail.Messages
                .Where(m => m.Id > afterId)
                .Select(m => new
                {
                    m.Id,
                    m.SenderName,
                    m.IsMe,
                    m.IsSystem,
                    m.Message,
                    created = m.Created.ToString("dd/MM HH:mm"),
                    attachmentName = m.AttachmentName,
                    attachmentUrl = m.HasAttachment ? m.AttachmentUrl : null,
                    isImage = m.IsImage
                })
                .ToList();

            return Ok(new { messages = newMessages });
        }
    }

    // ── Request DTOs ────────────────────────────────────────────────────────
    public class ChatStartRequest
    {
        public int ProductId { get; set; }
    }

    public class ChatSendRequest
    {
        public string Message { get; set; } = "";
    }
}
