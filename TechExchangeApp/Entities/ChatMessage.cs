using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    [Table("ChatMessages")]
    public class ChatMessage
    {
        [Key]
        public long Id { get; set; }

        public long ConversationId { get; set; }

        [MaxLength(450)]
        public string SenderUserId { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        /// <summary>Auto-generated system message vs user-typed</summary>
        public bool IsSystem { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        // ── Tệp đính kèm (HSMT A.5.3, C.7): lưu ngoài thư mục công khai, tải qua kiểm tra quyền ──
        [MaxLength(500)]
        public string? AttachmentPath { get; set; }
        [MaxLength(255)]
        public string? AttachmentName { get; set; }
        [MaxLength(100)]
        public string? AttachmentType { get; set; }
        public long? AttachmentSize { get; set; }

        [ForeignKey(nameof(ConversationId))]
        public ChatConversation? Conversation { get; set; }
    }
}
