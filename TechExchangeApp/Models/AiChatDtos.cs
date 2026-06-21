using System.ComponentModel.DataAnnotations;

namespace TechExchangeApp.Models
{
    public class AiChatMessageRequest
    {
        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SessionKey { get; set; }
    }

    public class AiChatMessageResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string SessionKey { get; set; } = string.Empty;
        public IReadOnlyList<AiKnowledgeItem> Sources { get; set; } = Array.Empty<AiKnowledgeItem>();
        public bool NeedsContactInfo { get; set; }
    }

    public class AiChatFeedbackRequest
    {
        [MaxLength(255)]
        public string? FullName { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(2000)]
        public string? Message { get; set; }

        [MaxLength(100)]
        public string? SessionKey { get; set; }
    }

    public class AiChatFeedbackResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class AiKnowledgeItem
    {
        public string SourceType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Url { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
