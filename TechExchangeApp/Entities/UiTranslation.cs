using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    /// <summary>Chuỗi giao diện (UI text) song ngữ. Nguồn cho I18n thay cho ui.json.</summary>
    [Table("UiTranslations")]
    public class UiTranslation
    {
        public int Id { get; set; }

        [Column("Key")]
        [MaxLength(250)]
        public string Key { get; set; } = "";

        public string? Vi { get; set; }
        public string? En { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        /// <summary>Cho phép render raw HTML (chỉ khi = true, TrHtml mới nhả raw).</summary>
        public bool AllowHtml { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>Optimistic concurrency.</summary>
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public DateTime Created { get; set; }
        public string? Creator { get; set; }
        public DateTime? Modified { get; set; }
        public string? Modifier { get; set; }
    }
}
