using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    [Table("AiKnowledgeDocuments")]
    public class AiKnowledgeDocument
    {
        [Key]
        public long Id { get; set; }

        [MaxLength(100)]
        public string SourceType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string SourceId { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? SourceSlug { get; set; }

        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Url { get; set; }

        public string ContentText { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ContentHash { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(100)]
        public string? DatasetVersion { get; set; }

        public DateTime LastSyncedAt { get; set; }
    }
}
