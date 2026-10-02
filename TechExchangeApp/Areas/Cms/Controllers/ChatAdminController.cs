using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>
    /// Xem lịch sử chat theo người mua / người bán, tìm từ khóa, gắn cờ hội thoại vi phạm
    /// (HSMT A.5.3, A.2.10). Chỉ đọc nội dung — không gửi tin thay người dùng.
    /// </summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class ChatAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "ChatAdmin";
        private static readonly string[] LogFunctionAliases = { "Chat" };

        public ChatAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private Task WriteLog(int eventId, string content) =>
            CmsLogHelper.WriteLogAsync(_context, HttpContext, GetSiteId(), LogFunctionName, LogFunctionAliases, eventId, content);

        /// <param name="role">any | buyer | seller — vai của người dùng cần tìm trong hội thoại</param>
        [HttpGet]
        public async Task<IActionResult> Index(string? user, string role = "any", string? keyword = null,
            bool flagged = false, int? type = null, int page = 1, int pageSize = 30)
        {
            if (page < 1) page = 1;
            var query = _context.ChatConversations.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(user))
            {
                var ids = await _context.Users.AsNoTracking()
                    .Where(u => u.UserName.Contains(user) || (u.FullName != null && u.FullName.Contains(user)) || (u.Email != null && u.Email.Contains(user)))
                    .Select(u => u.Id.ToString()).Take(500).ToListAsync();
                query = role switch
                {
                    "buyer" => query.Where(c => ids.Contains(c.BuyerUserId)),
                    "seller" => query.Where(c => ids.Contains(c.SupplierUserId)),
                    _ => query.Where(c => ids.Contains(c.BuyerUserId) || ids.Contains(c.SupplierUserId))
                };
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var convIds = _context.ChatMessages.Where(m => m.Message.Contains(keyword)).Select(m => m.ConversationId);
                query = query.Where(c => convIds.Contains(c.Id));
            }
            if (flagged) query = query.Where(c => c.IsFlagged);
            if (type.HasValue) query = query.Where(c => c.ConversationType == type.Value);

            var total = await query.CountAsync();
            var convs = await query
                .OrderByDescending(c => c.IsFlagged).ThenByDescending(c => c.LastMessageAt ?? c.Created)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(c => new ChatAdminRow
                {
                    Id = c.Id, ConversationType = c.ConversationType, ProductId = c.ProductId, ProductName = c.ProductName,
                    BuyerUserId = c.BuyerUserId, SupplierUserId = c.SupplierUserId, Created = c.Created,
                    LastMessageAt = c.LastMessageAt, IsFlagged = c.IsFlagged, FlagReason = c.FlagReason,
                    MessageCount = _context.ChatMessages.Count(m => m.ConversationId == c.Id)
                }).ToListAsync();

            await FillUserNamesAsync(convs);

            ViewBag.User = user;
            ViewBag.Role = role;
            ViewBag.Keyword = keyword;
            ViewBag.Flagged = flagged;
            ViewBag.Type = type;
            ViewBag.Page = page;
            ViewBag.Total = total;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.FlaggedCount = await _context.ChatConversations.CountAsync(c => c.IsFlagged);
            return View(convs);
        }

        [HttpGet]
        public async Task<IActionResult> Conversation(long id, string? keyword)
        {
            var c = await _context.ChatConversations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return NotFound();
            var row = new ChatAdminRow
            {
                Id = c.Id, ConversationType = c.ConversationType, ProductId = c.ProductId, ProductName = c.ProductName,
                BuyerUserId = c.BuyerUserId, SupplierUserId = c.SupplierUserId, Created = c.Created,
                LastMessageAt = c.LastMessageAt, IsFlagged = c.IsFlagged, FlagReason = c.FlagReason,
                FlaggedAt = c.FlaggedAt, FlaggedBy = c.FlaggedBy
            };
            await FillUserNamesAsync(new List<ChatAdminRow> { row });

            var messages = await _context.ChatMessages.AsNoTracking()
                .Where(m => m.ConversationId == id).OrderBy(m => m.Created)
                .Select(m => new ChatAdminMessage { Id = m.Id, SenderUserId = m.SenderUserId, Message = m.Message, IsSystem = m.IsSystem, Created = m.Created })
                .ToListAsync();
            var senderIds = messages.Select(m => m.SenderUserId).Distinct().Select(s => int.TryParse(s, out var n) ? n : 0).Where(n => n > 0).ToList();
            var names = await _context.Users.AsNoTracking().Where(u => senderIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id.ToString(), u => u.FullName ?? u.UserName);
            foreach (var m in messages) m.SenderName = names.TryGetValue(m.SenderUserId, out var n) ? n : m.SenderUserId;

            ViewBag.Messages = messages;
            ViewBag.Keyword = keyword;
            return View(row);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Flag(long id, bool flagged, string? reason)
        {
            var c = await _context.ChatConversations.FindAsync(id);
            if (c == null) return Json(new { success = false, message = "Không tìm thấy hội thoại." });
            reason = reason?.Trim();
            if (flagged && string.IsNullOrEmpty(reason))
                return Json(new { success = false, message = "Vui lòng nhập lý do gắn cờ." });

            c.IsFlagged = flagged;
            c.FlagReason = flagged ? (reason!.Length > 500 ? reason[..500] : reason) : null;
            c.FlaggedAt = flagged ? DateTime.Now : null;
            c.FlaggedBy = flagged ? User.Identity?.Name : null;
            await _context.SaveChangesAsync();
            await WriteLog(2, flagged ? $"Gắn cờ hội thoại #{id}: {c.FlagReason}" : $"Bỏ cờ hội thoại #{id}");
            return Json(new { success = true });
        }

        private async Task FillUserNamesAsync(List<ChatAdminRow> rows)
        {
            var ids = rows.SelectMany(r => new[] { r.BuyerUserId, r.SupplierUserId })
                .Select(s => int.TryParse(s, out var n) ? n : 0).Where(n => n > 0).Distinct().ToList();
            var users = await _context.Users.AsNoTracking().Where(u => ids.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.FullName }).ToListAsync();
            var map = users.ToDictionary(u => u.Id.ToString(), u => (u.UserName, u.FullName));
            foreach (var r in rows)
            {
                if (map.TryGetValue(r.BuyerUserId, out var b)) { r.BuyerName = b.FullName ?? b.UserName; r.BuyerUserName = b.UserName; }
                if (map.TryGetValue(r.SupplierUserId, out var s)) { r.SupplierName = s.FullName ?? s.UserName; r.SupplierUserName = s.UserName; }
            }
        }
    }

    public class ChatAdminRow
    {
        public long Id { get; set; }
        public int ConversationType { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string BuyerUserId { get; set; } = "";
        public string SupplierUserId { get; set; } = "";
        public string? BuyerName { get; set; }
        public string? BuyerUserName { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierUserName { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int MessageCount { get; set; }
        public bool IsFlagged { get; set; }
        public string? FlagReason { get; set; }
        public DateTime? FlaggedAt { get; set; }
        public string? FlaggedBy { get; set; }
    }

    public class ChatAdminMessage
    {
        public long Id { get; set; }
        public string SenderUserId { get; set; } = "";
        public string SenderName { get; set; } = "";
        public string Message { get; set; } = "";
        public bool IsSystem { get; set; }
        public DateTime Created { get; set; }
    }
}
