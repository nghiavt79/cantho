using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    /// <summary>
    /// Token đặt lại mật khẩu (HSMT A.1.1). Chỉ lưu SHA-256 của token, hết hạn sau
    /// PasswordResetService.TokenLifetime và dùng được một lần.
    /// </summary>
    [Table("PasswordResetTokens")]
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [StringLength(64)]
        public string TokenHash { get; set; } = "";

        public DateTime ExpiresAt { get; set; }

        public DateTime? UsedAt { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;

        [StringLength(64)]
        public string? RequestIp { get; set; }
    }
}
