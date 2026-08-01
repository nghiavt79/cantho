namespace TechExchangeApp.Interfaces
{
    public interface IChatService
    {
        /// <summary>
        /// Start or resume a chat conversation between buyer and supplier for a product.
        /// Returns the conversation ID.
        /// </summary>
        Task<ChatStartResult> StartConversationAsync(int productId, int buyerUserId);

        /// <summary>Get all conversations for a user (as buyer or supplier).</summary>
        Task<List<ChatConversationVm>> GetConversationsAsync(int userId);

        /// <summary>Get messages for a conversation (with access check).</summary>
        Task<ChatConversationDetailVm?> GetConversationAsync(long conversationId, int userId);

        /// <summary>Send a message in a conversation.</summary>
        Task<bool> SendMessageAsync(long conversationId, int senderUserId, string message);

        /// <summary>Mark messages as read.</summary>
        Task MarkAsReadAsync(long conversationId, int userId);

        /// <summary>Get unread message count for a user.</summary>
        Task<int> GetUnreadCountAsync(int userId);

        /// <summary>
        /// Tạo ticket hỗ trợ/tư vấn Trung tâm cho một dự án + bước.
        /// Giả định quyền truy cập dự án đã được kiểm tra ở controller. Trả về conversationId.
        /// </summary>
        Task<long> StartSupportConversationAsync(int projectId, int requesterUserId, SupportStartOptions options);

        /// <summary>
        /// Nhân viên CMS trả lời một hội thoại hỗ trợ (atomic nhận xử lý khi chưa gán).
        /// </summary>
        Task<SupportReplyResult> ReplySupportAsync(long conversationId, int staffUserId, string message);
    }

    public enum SupportReplyOutcome { Ok, NotFound, EmptyMessage, AssignedToOther }

    public class SupportReplyResult
    {
        public SupportReplyOutcome Outcome { get; set; }
        public int? AssignedStaffUserId { get; set; }
        public string? AssignedStaffName { get; set; }
    }

    public class ChatStartResult
    {
        public bool Success { get; set; }
        public long ConversationId { get; set; }
        public bool IsNew { get; set; }
        public string? Error { get; set; }
    }

    public class ChatConversationVm
    {
        public long Id { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string OtherUserName { get; set; } = "";
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }

        // ── Hỗ trợ Sàn ──
        public int ConversationType { get; set; } = 1;
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public int? StepNumber { get; set; }
        public int SupportStatus { get; set; }
        public int? AssignedStaffUserId { get; set; }
        public long? SupportRequestId { get; set; }
        public int? RequestType { get; set; }
        public int? ServiceType { get; set; }
        public int? TicketStatus { get; set; }
        public string? Subject { get; set; }
        public string? SupportContextCode { get; set; }
    }

    public class ChatConversationDetailVm
    {
        public long ConversationId { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string OtherUserName { get; set; } = "";
        public List<ChatMessageVm> Messages { get; set; } = new();

        // ── Hỗ trợ Sàn ──
        public int ConversationType { get; set; } = 1;
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public int? StepNumber { get; set; }
        public int SupportStatus { get; set; }
        public int? AssignedStaffUserId { get; set; }
        public long? SupportRequestId { get; set; }
        public int? RequestType { get; set; }
        public int? ServiceType { get; set; }
        public int? TicketStatus { get; set; }
        public string? Subject { get; set; }
        public string? SupportContextCode { get; set; }
    }

    public class ChatMessageVm
    {
        public long Id { get; set; }
        public string SenderName { get; set; } = "";
        public bool IsMe { get; set; }
        public bool IsSystem { get; set; }
        public string Message { get; set; } = "";
        public DateTime Created { get; set; }
    }

    public class SupportStartOptions
    {
        public int StepNumber { get; set; }
        public int RequestType { get; set; }
        public int? ServiceType { get; set; }
        public string? SupportContextCode { get; set; }
        public int? DisplayStepNumber { get; set; }
        public int? InternalStepNumber { get; set; }
        public string? Subject { get; set; }
        public string Description { get; set; } = "";
    }
}
