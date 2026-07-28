using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Enums;
using TechExchangeApp.Interfaces;

namespace TechExchangeApp.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;
        private readonly INotificationQueueService _notifQueue;

        public ChatService(AppDbContext context, INotificationQueueService notifQueue)
        {
            _context = context;
            _notifQueue = notifQueue;
        }

        // ── Start or Resume Conversation ────────────────────────────────────────
        public async Task<ChatStartResult> StartConversationAsync(int productId, int buyerUserId)
        {
            // 1) Validate product
            var product = await _context.SanPhamCNTBs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID == productId && p.StatusId == 3);

            if (product == null)
                return new ChatStartResult { Success = false, Error = "Sản phẩm không tồn tại." };

            // 2) Determine supplier UserId
            int? supplierUserId = await ResolveSupplierUserIdAsync(product);

            if (supplierUserId == null || supplierUserId == 0)
                return new ChatStartResult { Success = false, Error = "Không tìm thấy nhà cung ứng." };

            // 3) Cannot chat with self
            if (supplierUserId == buyerUserId)
                return new ChatStartResult { Success = false, Error = "Bạn không thể liên hệ chính mình." };

            string buyerIdStr = buyerUserId.ToString();
            string supplierIdStr = supplierUserId.Value.ToString();

            // 4) Check existing conversation
            var existing = await _context.ChatConversations
                .FirstOrDefaultAsync(c =>
                    c.ConversationType == 1 &&
                    c.ProductId == productId &&
                    c.BuyerUserId == buyerIdStr &&
                    c.SupplierUserId == supplierIdStr);

            if (existing != null)
            {
                return new ChatStartResult
                {
                    Success = true,
                    ConversationId = existing.Id,
                    IsNew = false
                };
            }

            // 5) Create new conversation + first message
            var conversation = new ChatConversation
            {
                ConversationType = (int)ChatConversationType.Product,
                ProductId = productId,
                ProductType = product.ProductType,
                BuyerUserId = buyerIdStr,
                SupplierUserId = supplierIdStr,
                ProductName = product.Name,
                IsFromProductDetail = true,
                Created = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };
            _context.ChatConversations.Add(conversation);
            await _context.SaveChangesAsync();

            // Auto-generated first message with product link
            var productUrl = $"/san-pham/chi-tiet/{TechExchangeApp.Controllers.ProductController.MakeURLFriendly(product.Name)}-{product.ID}";
            var firstMessage = new ChatMessage
            {
                ConversationId = conversation.Id,
                SenderUserId = buyerIdStr,
                Message = $"Xin chào, tôi quan tâm đến sản phẩm: <a href=\"{productUrl}\" target=\"_blank\">{product.Name}</a>.\nMong được tư vấn thêm.",
                IsSystem = true,
                IsRead = false,
                Created = DateTime.UtcNow
            };
            _context.ChatMessages.Add(firstMessage);
            await _context.SaveChangesAsync();

            // 6) Trigger notification to supplier
            try
            {
                await _notifQueue.QueueAsync(
                    supplierUserId.Value,
                    projectId: null,
                    title: "Có yêu cầu liên hệ mới",
                    content: $"Bạn có tin nhắn mới về sản phẩm: {product.Name}",
                    channel: NotificationChannel.Email,
                    url: $"/chat/{conversation.Id}");
            }
            catch
            {
                // Non-critical — don't fail the chat start
            }

            return new ChatStartResult
            {
                Success = true,
                ConversationId = conversation.Id,
                IsNew = true
            };
        }

        // ── Get Conversations List ──────────────────────────────────────────────
        public async Task<List<ChatConversationVm>> GetConversationsAsync(int userId)
        {
            string uid = userId.ToString();

            var conversations = await _context.ChatConversations
                .AsNoTracking()
                .Where(c => c.BuyerUserId == uid || c.SupplierUserId == uid)
                .OrderByDescending(c => c.LastMessageAt ?? c.Created)
                .ToListAsync();

            var result = new List<ChatConversationVm>();

            foreach (var c in conversations)
            {
                var lastMsg = await _context.ChatMessages
                    .AsNoTracking()
                    .Where(m => m.ConversationId == c.Id)
                    .OrderByDescending(m => m.Created)
                    .Select(m => m.Message)
                    .FirstOrDefaultAsync();

                int unread = await _context.ChatMessages
                    .CountAsync(m =>
                        m.ConversationId == c.Id &&
                        m.SenderUserId != uid &&
                        !m.IsRead);

                // Resolve other user name (an toàn cho support chưa gán)
                var otherUser = await ResolveOtherDisplayNameAsync(c, uid);

                result.Add(new ChatConversationVm
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.ProductName,
                    OtherUserName = otherUser,
                    LastMessage = lastMsg,
                    LastMessageAt = c.LastMessageAt,
                    UnreadCount = unread,
                    ConversationType = c.ConversationType,
                    ProjectId = c.ProjectId,
                    StepNumber = c.StepNumber,
                    SupportStatus = c.SupportStatus,
                    AssignedStaffUserId = c.AssignedStaffUserId
                });
            }

            return result;
        }

        // ── Get Conversation Detail ─────────────────────────────────────────────
        public async Task<ChatConversationDetailVm?> GetConversationAsync(long conversationId, int userId)
        {
            string uid = userId.ToString();

            var conv = await _context.ChatConversations
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    (c.BuyerUserId == uid || c.SupplierUserId == uid));

            if (conv == null) return null;

            var otherUser = await ResolveOtherDisplayNameAsync(conv, uid);

            // Tên dự án cho hội thoại hỗ trợ (hiển thị badge dự án + bước)
            string? projectName = null;
            if (conv.ConversationType == (int)ChatConversationType.PlatformSupport && conv.ProjectId.HasValue)
            {
                projectName = await _context.Projects.AsNoTracking()
                    .Where(p => p.Id == conv.ProjectId.Value)
                    .Select(p => p.ProjectName)
                    .FirstOrDefaultAsync();
            }

            var messages = await _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.Created)
                .ToListAsync();

            // Batch-resolve sender names
            var senderIds = messages.Select(m => m.SenderUserId).Distinct().ToList();
            var senderNames = new Dictionary<string, string>();
            foreach (var sid in senderIds)
            {
                if (int.TryParse(sid, out int sidInt))
                {
                    var name = await _context.Users
                        .AsNoTracking()
                        .Where(u => u.Id == sidInt)
                        .Select(u => u.UserName)
                        .FirstOrDefaultAsync();
                    senderNames[sid] = name ?? "Người dùng";
                }
            }

            return new ChatConversationDetailVm
            {
                ConversationId = conv.Id,
                ProductId = conv.ProductId,
                ProductName = conv.ProductName,
                OtherUserName = otherUser,
                ConversationType = conv.ConversationType,
                ProjectId = conv.ProjectId,
                ProjectName = projectName,
                StepNumber = conv.StepNumber,
                SupportStatus = conv.SupportStatus,
                AssignedStaffUserId = conv.AssignedStaffUserId,
                Messages = messages.Select(m => new ChatMessageVm
                {
                    Id = m.Id,
                    SenderName = senderNames.GetValueOrDefault(m.SenderUserId, "Người dùng"),
                    IsMe = m.SenderUserId == uid,
                    IsSystem = m.IsSystem,
                    Message = m.Message,
                    Created = m.Created
                }).ToList()
            };
        }

        // ── Send Message ────────────────────────────────────────────────────────
        public async Task<bool> SendMessageAsync(long conversationId, int senderUserId, string message)
        {
            string uid = senderUserId.ToString();

            var conv = await _context.ChatConversations
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    (c.BuyerUserId == uid || c.SupplierUserId == uid));

            if (conv == null) return false;

            var msg = new ChatMessage
            {
                ConversationId = conversationId,
                SenderUserId = uid,
                Message = message,
                IsSystem = false,
                IsRead = false,
                Created = DateTime.UtcNow
            };
            _context.ChatMessages.Add(msg);

            conv.LastMessageAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Notify the other party (phân nhánh support / product)
            try
            {
                int recipientInt = 0;
                string title, content, url;

                if (conv.ConversationType == (int)ChatConversationType.PlatformSupport)
                {
                    if (conv.BuyerUserId == uid)
                    {
                        // Người gửi yêu cầu nhắn tiếp → báo đầu mối (nếu đã gán)
                        recipientInt = conv.AssignedStaffUserId ?? 0;
                        title = "Tin nhắn hỗ trợ mới";
                        content = "Có tin nhắn mới trong yêu cầu hỗ trợ dự án.";
                        url = $"/cms/SupportRequestsAdmin/Thread/{conversationId}";
                    }
                    else
                    {
                        // Đầu mối nhắn qua giao diện member → báo người gửi
                        int.TryParse(conv.BuyerUserId, out recipientInt);
                        title = "Phản hồi hỗ trợ từ Sàn";
                        content = "Sàn vừa phản hồi yêu cầu hỗ trợ của bạn.";
                        url = $"/chat/{conversationId}";
                    }
                }
                else
                {
                    string recipientId = conv.BuyerUserId == uid ? conv.SupplierUserId : conv.BuyerUserId;
                    int.TryParse(recipientId, out recipientInt);
                    title = "Tin nhắn mới";
                    content = $"Bạn có tin nhắn mới về sản phẩm: {conv.ProductName}";
                    url = $"/chat/{conversationId}";
                }

                if (recipientInt > 0)
                {
                    await _notifQueue.QueueAsync(
                        recipientInt, conv.ProjectId, title, content,
                        NotificationChannel.Email, url);
                }
            }
            catch { /* non-critical */ }

            return true;
        }

        // ── Mark As Read ────────────────────────────────────────────────────────
        public async Task MarkAsReadAsync(long conversationId, int userId)
        {
            string uid = userId.ToString();

            var unread = await _context.ChatMessages
                .Where(m =>
                    m.ConversationId == conversationId &&
                    m.SenderUserId != uid &&
                    !m.IsRead)
                .ToListAsync();

            foreach (var m in unread) m.IsRead = true;
            await _context.SaveChangesAsync();
        }

        // ── Unread Count ────────────────────────────────────────────────────────
        public async Task<int> GetUnreadCountAsync(int userId)
        {
            string uid = userId.ToString();

            // Get all conversation IDs where this user is a participant
            var convIds = await _context.ChatConversations
                .AsNoTracking()
                .Where(c => c.BuyerUserId == uid || c.SupplierUserId == uid)
                .Select(c => c.Id)
                .ToListAsync();

            return await _context.ChatMessages
                .CountAsync(m =>
                    convIds.Contains(m.ConversationId) &&
                    m.SenderUserId != uid &&
                    !m.IsRead);
        }

        // ── Resolve Supplier UserId from product ────────────────────────────────
        private async Task<int?> ResolveSupplierUserIdAsync(SanPhamCNTB product)
        {
            // Path 1: NCUId → NhaCungUng.UserId
            if (product.NCUId.HasValue && product.NCUId > 0)
            {
                var ncuUserId = await _context.NhaCungUngs
                    .AsNoTracking()
                    .Where(n => n.CungUngId == product.NCUId)
                    .Select(n => n.UserId)
                    .FirstOrDefaultAsync();

                if (ncuUserId.HasValue && ncuUserId > 0)
                    return ncuUserId;
            }

            // Path 2: Creator username → resolve to UserId
            if (!string.IsNullOrEmpty(product.Creator))
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserName == product.Creator);

                if (user != null)
                    return user.Id;
            }

            // Path 3: StoreId → Store.UserId (if Store entity has UserId)
            if (product.StoreId.HasValue && product.StoreId > 0)
            {
                var storeUserId = await _context.Stores
                    .AsNoTracking()
                    .Where(s => s.StoreId == product.StoreId)
                    .Select(s => s.UserId)
                    .FirstOrDefaultAsync();

                if (storeUserId > 0)
                    return storeUserId;
            }

            return null;
        }

        // ── Hỗ trợ Sàn ──────────────────────────────────────────────────────────

        /// <summary>Tên hiển thị an toàn cho một user id (chuỗi). Sentinel "0"/rỗng → "Người dùng".</summary>
        private async Task<string> ResolveUserNameAsync(string? userIdStr)
        {
            if (int.TryParse(userIdStr, out int uidInt) && uidInt > 0)
            {
                var name = await _context.Users.AsNoTracking()
                    .Where(u => u.Id == uidInt)
                    .Select(u => u.UserName)
                    .FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(name)) return name!;
            }
            return "Người dùng";
        }

        /// <summary>Tên đối tác hiển thị, an toàn cho hội thoại hỗ trợ chưa gán.</summary>
        private async Task<string> ResolveOtherDisplayNameAsync(ChatConversation c, string uid)
        {
            if (c.ConversationType == (int)ChatConversationType.PlatformSupport)
            {
                // Người gửi yêu cầu nhìn vào → đối tác là Sàn/đầu mối
                if (c.BuyerUserId == uid)
                {
                    if (c.AssignedStaffUserId is int staffId && staffId > 0)
                        return await ResolveUserNameAsync(staffId.ToString());
                    return "Sàn hỗ trợ";
                }
                // Nhân viên nhìn vào → đối tác là người gửi
                return await ResolveUserNameAsync(c.BuyerUserId);
            }

            string otherId = c.BuyerUserId == uid ? c.SupplierUserId : c.BuyerUserId;
            return await ResolveUserNameAsync(otherId);
        }

        public async Task<long> StartSupportConversationAsync(int projectId, int requesterUserId, int stepNumber, string message)
        {
            string requesterStr = requesterUserId.ToString();

            // 1) Tái dùng hội thoại hỗ trợ đang mở của (dự án, người gửi)
            var existing = await _context.ChatConversations.FirstOrDefaultAsync(c =>
                c.ConversationType == (int)ChatConversationType.PlatformSupport &&
                c.ProjectId == projectId &&
                c.BuyerUserId == requesterStr &&
                (c.SupportStatus == (int)SupportConversationStatus.Unassigned ||
                 c.SupportStatus == (int)SupportConversationStatus.InProgress));

            if (existing != null)
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    _context.ChatMessages.Add(new ChatMessage
                    {
                        ConversationId = existing.Id,
                        SenderUserId = requesterStr,
                        Message = message.Trim(),
                        IsSystem = false,
                        IsRead = false,
                        Created = DateTime.UtcNow
                    });
                    existing.LastMessageAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    if (existing.AssignedStaffUserId is int sId && sId > 0)
                    {
                        try
                        {
                            await _notifQueue.QueueAsync(sId, projectId, "Tin nhắn hỗ trợ mới",
                                "Có tin nhắn mới trong yêu cầu hỗ trợ dự án.",
                                NotificationChannel.Email, $"/cms/SupportRequestsAdmin/Thread/{existing.Id}");
                        }
                        catch { /* non-critical */ }
                    }
                }
                return existing.Id;
            }

            // 2) Tạo mới — xác định đầu mối: ProjectMembers Role=3 (Consultant) active
            var projectName = await _context.Projects.AsNoTracking()
                .Where(p => p.Id == projectId)
                .Select(p => p.ProjectName)
                .FirstOrDefaultAsync() ?? $"Dự án #{projectId}";

            int? staffUserId = await _context.ProjectMembers.AsNoTracking()
                .Where(m => m.ProjectId == projectId && m.Role == (int)ProjectRole.Consultant && m.IsActive)
                .OrderBy(m => m.JoinedDate)
                .Select(m => (int?)m.UserId)
                .FirstOrDefaultAsync();

            bool hasStaff = staffUserId.HasValue && staffUserId.Value > 0;

            var conv = new ChatConversation
            {
                ConversationType = (int)ChatConversationType.PlatformSupport,
                ProductId = null,
                ProductType = null,
                BuyerUserId = requesterStr,
                SupplierUserId = hasStaff ? staffUserId!.Value.ToString() : "0", // sentinel "0" = chưa gán
                ProductName = "Hỗ trợ: " + projectName,
                IsFromProductDetail = false,
                ProjectId = projectId,
                StepNumber = stepNumber,
                AssignedStaffUserId = hasStaff ? staffUserId : null,
                SupportStatus = hasStaff
                    ? (int)SupportConversationStatus.InProgress
                    : (int)SupportConversationStatus.Unassigned,
                Created = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };
            _context.ChatConversations.Add(conv);
            await _context.SaveChangesAsync();

            // 3) System message: context server-side đã encode (KHÔNG chứa input thô của user)
            string safeProject = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(projectName);
            _context.ChatMessages.Add(new ChatMessage
            {
                ConversationId = conv.Id,
                SenderUserId = requesterStr,
                Message = $"<i class=\"fa fa-headset\"></i> Yêu cầu hỗ trợ — dự án <strong>{safeProject}</strong>, bước {stepNumber}.",
                IsSystem = true,
                IsRead = false,
                Created = DateTime.UtcNow
            });

            // 4) User message thật — luôn IsSystem=false (view sẽ tự encode)
            if (!string.IsNullOrWhiteSpace(message))
            {
                _context.ChatMessages.Add(new ChatMessage
                {
                    ConversationId = conv.Id,
                    SenderUserId = requesterStr,
                    Message = message.Trim(),
                    IsSystem = false,
                    IsRead = false,
                    Created = DateTime.UtcNow
                });
            }
            conv.LastMessageAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 5) Notify đầu mối nếu đã gán
            if (hasStaff)
            {
                try
                {
                    await _notifQueue.QueueAsync(staffUserId!.Value, projectId, "Yêu cầu hỗ trợ mới",
                        $"Có yêu cầu hỗ trợ mới cho dự án: {projectName}",
                        NotificationChannel.Email, $"/cms/SupportRequestsAdmin/Thread/{conv.Id}");
                }
                catch { /* non-critical */ }
            }

            return conv.Id;
        }

        public async Task<SupportReplyResult> ReplySupportAsync(long conversationId, int staffUserId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return new SupportReplyResult { Outcome = SupportReplyOutcome.EmptyMessage };

            var conv = await _context.ChatConversations.FirstOrDefaultAsync(c =>
                c.Id == conversationId &&
                c.ConversationType == (int)ChatConversationType.PlatformSupport);
            if (conv == null)
                return new SupportReplyResult { Outcome = SupportReplyOutcome.NotFound };

            using var tx = await _context.Database.BeginTransactionAsync();

            // Nếu đã gán cho người khác → chặn mềm
            if (conv.AssignedStaffUserId is int owner0 && owner0 > 0 && owner0 != staffUserId)
            {
                await tx.RollbackAsync();
                return new SupportReplyResult
                {
                    Outcome = SupportReplyOutcome.AssignedToOther,
                    AssignedStaffUserId = owner0,
                    AssignedStaffName = await ResolveUserNameAsync(owner0.ToString())
                };
            }

            // Chưa gán → nhận xử lý ATOMIC (conditional UPDATE, chống 2 staff cùng nhận)
            if (!conv.AssignedStaffUserId.HasValue || conv.AssignedStaffUserId <= 0)
            {
                string staffStr = staffUserId.ToString();
                int inProgress = (int)SupportConversationStatus.InProgress;
                int affected = await _context.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE ChatConversations
SET AssignedStaffUserId = {staffUserId}, SupplierUserId = {staffStr}, SupportStatus = {inProgress}
WHERE Id = {conversationId} AND ConversationType = 2 AND SupportStatus = 1 AND AssignedStaffUserId IS NULL");

                if (affected == 0)
                {
                    // Người khác vừa nhận (hoặc chính mình từ request song song)
                    var ownerNow = await _context.ChatConversations.AsNoTracking()
                        .Where(c => c.Id == conversationId)
                        .Select(c => c.AssignedStaffUserId)
                        .FirstOrDefaultAsync();
                    if (ownerNow.HasValue && ownerNow.Value != staffUserId)
                    {
                        await tx.RollbackAsync();
                        return new SupportReplyResult
                        {
                            Outcome = SupportReplyOutcome.AssignedToOther,
                            AssignedStaffUserId = ownerNow,
                            AssignedStaffName = await ResolveUserNameAsync(ownerNow.Value.ToString())
                        };
                    }
                    // ownerNow == me → tiếp tục
                }
            }

            // Thêm tin trả lời (IsSystem=false), cập nhật LastMessageAt
            _context.ChatMessages.Add(new ChatMessage
            {
                ConversationId = conversationId,
                SenderUserId = staffUserId.ToString(),
                Message = message.Trim(),
                IsSystem = false,
                IsRead = false,
                Created = DateTime.UtcNow
            });
            conv.LastMessageAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            // Notify người gửi SAU commit
            if (int.TryParse(conv.BuyerUserId, out int requesterInt) && requesterInt > 0)
            {
                try
                {
                    await _notifQueue.QueueAsync(requesterInt, conv.ProjectId, "Phản hồi hỗ trợ từ Sàn",
                        "Sàn vừa phản hồi yêu cầu hỗ trợ của bạn.",
                        NotificationChannel.Email, $"/chat/{conversationId}");
                }
                catch { /* non-critical */ }
            }

            return new SupportReplyResult { Outcome = SupportReplyOutcome.Ok, AssignedStaffUserId = staffUserId };
        }
    }
}
