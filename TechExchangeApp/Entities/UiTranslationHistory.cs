using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    /// <summary>Snapshot giá trị cũ trước mỗi lần ghi đè UiTranslation (audit).</summary>
    [Table("UiTranslationHistory")]
    public class UiTranslationHistory
    {
        public int Id { get; set; }
        public int TranslationId { get; set; }
        public string? Vi { get; set; }
        public string? En { get; set; }
        public bool AllowHtml { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
