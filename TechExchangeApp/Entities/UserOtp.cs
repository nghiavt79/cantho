using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    [Table("UserOtps")]
    public class UserOtp
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        /// <summary>1 = Email, 2 = Phone</summary>
        public int OtpType { get; set; }

        [Required, MaxLength(10)]
        public string OtpCode { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Kết quả gửi mã: null = chưa gửi, 1 = thành công, 2 = lỗi (Scripts/add-userotps-send-result.sql)</summary>
        public int? SendStatus { get; set; }

        [MaxLength(1000)]
        public string? SendError { get; set; }

        [MaxLength(1000)]
        public string? ProviderResponse { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
    }
}
