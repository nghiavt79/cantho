using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    [Table("AiChatMessages")]
    public class AiChatMessage
    {
        [Key]
        public long Id { get; set; }

        public long SessionId { get; set; }

        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
