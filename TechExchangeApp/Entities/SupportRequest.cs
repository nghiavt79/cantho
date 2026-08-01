using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    [Table("SupportRequests")]
    public class SupportRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public int ProjectId { get; set; }

        public int RequestedByUserId { get; set; }

        public int RequestType { get; set; }

        public int? ServiceType { get; set; }

        [StringLength(100)]
        public string? SupportContextCode { get; set; }

        public int? DisplayStepNumber { get; set; }

        public int? InternalStepNumber { get; set; }

        [StringLength(300)]
        public string? Subject { get; set; }

        public string? Description { get; set; }

        public int Status { get; set; }

        public int? AssignedStaffUserId { get; set; }

        public long? ConversationId { get; set; }

        public bool IsPrivateToRequester { get; set; } = true;

        public bool? IsChargeable { get; set; }

        [StringLength(200)]
        public string? FeePolicy { get; set; }

        public DateTime? AssignedAt { get; set; }

        public DateTime? FirstRespondedAt { get; set; }

        public DateTime? DueAt { get; set; }

        public int? LastStatusChangedByUserId { get; set; }

        public DateTime? LastStatusChangedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ClosedAt { get; set; }
    }
}
