using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    /// <summary>Câu hỏi thường gặp (HSMT A.7.3). LanguageId: 1 = vi, 2 = en.</summary>
    [Table("Faqs")]
    public class Faq
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string GroupName { get; set; } = "";

        [Required, StringLength(1000)]
        public string Question { get; set; } = "";

        [Required]
        public string Answer { get; set; } = "";

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public int LanguageId { get; set; } = 1;

        public DateTime Created { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? Creator { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(100)]
        public string? Modifier { get; set; }
    }
}
