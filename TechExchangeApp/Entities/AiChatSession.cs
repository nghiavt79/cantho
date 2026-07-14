using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    [Table("AiChatSessions")]
    public class AiChatSession
    {
        [Key]
        public long Id { get; set; }

        [MaxLength(100)]
        public string SessionKey { get; set; } = string.Empty;

        public int? UserId { get; set; }

        [MaxLength(255)]
        public string? UserName { get; set; }

        [MaxLength(50)]
        public string? UserPhone { get; set; }

        [MaxLength(255)]
        public string? UserEmail { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Open";

        public DateTime CreatedAt { get; set; }

        public DateTime? LastMessageAt { get; set; }
    }
}
