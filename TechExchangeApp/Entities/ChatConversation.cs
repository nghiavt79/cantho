using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    [Table("ChatConversations")]
    public class ChatConversation
    {
        [Key]
        public long Id { get; set; }

        /// <summary>Link to SanPhamCNTB.ID (nullable for general chats)</summary>
        public int? ProductId { get; set; }

        /// <summary>1=CongNghe, 2=ThietBi, 3=SanPhamTriTue</summary>
        public int? ProductType { get; set; }

        [MaxLength(450)]
        public string BuyerUserId { get; set; } = string.Empty;

        [MaxLength(450)]
        public string SupplierUserId { get; set; } = string.Empty;

        /// <summary>Product name snapshot for display</summary>
        [MaxLength(500)]
        public string? ProductName { get; set; }

        /// <summary>Originated from product detail page</summary>
        public bool IsFromProductDetail { get; set; } = true;

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? LastMessageAt { get; set; }

        // ── Hỗ trợ/tư vấn Trung tâm ──
        /// <summary>1 = chat sản phẩm (mặc định), 2 = hỗ trợ/tư vấn Trung tâm. Xem <see cref="Enums.ChatConversationType"/>.</summary>
        public int ConversationType { get; set; } = 1;

        /// <summary>Dự án gắn với hội thoại hỗ trợ (null với chat sản phẩm).</summary>
        public int? ProjectId { get; set; }

        /// <summary>Bước phát sinh yêu cầu hỗ trợ (1..14).</summary>
        public int? StepNumber { get; set; }

        /// <summary>0 = n/a, 1 = chưa gán, 2 = đang xử lý, 3 = đã đóng. Xem <see cref="Enums.SupportConversationStatus"/>.</summary>
        public int SupportStatus { get; set; } = 0;

        /// <summary>Nhân viên/tư vấn đang phụ trách hỗ trợ (field nghiệp vụ chính; null = chưa gán).</summary>
        public int? AssignedStaffUserId { get; set; }
    }
}
