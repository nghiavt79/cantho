using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    [Table("ProjectContracts")]
    public class ProjectContract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public int VersionNumber { get; set; } = 1;

        /// <summary>1=AutoGenerate, 2=Upload</summary>
        public int SourceType { get; set; } = 1;

        [StringLength(100)]
        public string? TemplateCode { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; } = null!;

        /// <summary>See ContractStatus enum</summary>
        public int StatusId { get; set; } = 0; // Draft

        public string? HtmlContent { get; set; }

        [StringLength(500)]
        public string? OriginalFilePath { get; set; }

        [StringLength(260)]
        public string? OriginalFileName { get; set; }

        [StringLength(500)]
        public string? SignedFilePath { get; set; }

        [StringLength(260)]
        public string? SignedFileName { get; set; }

        [StringLength(128)]
        public string? Sha256Original { get; set; }

        [StringLength(128)]
        public string? Sha256Signed { get; set; }

        public DateTime? ReadyToSignAt { get; set; }

        public bool IsActive { get; set; } = false;

        public int? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public DateTime? ArchivedAt { get; set; }

        [StringLength(1000)]
        public string? Note { get; set; }

        // eContract MobiFone (null = hợp đồng theo luồng ký cũ)
        [StringLength(30)]
        public string? SigningProvider { get; set; }

        [StringLength(20)]
        public string? EContractEnv { get; set; }

        public long? EContractId { get; set; }

        [StringLength(100)]
        public string? EContractNo { get; set; }

        /// <summary>Trạng thái phía eContract, xem EContractConstants</summary>
        public int? EContractStatus { get; set; }

        /// <summary>See EContractIntegrationState enum</summary>
        public int IntegrationState { get; set; } = 0;

        [StringLength(500)]
        public string? FrozenPdfPath { get; set; }

        [StringLength(128)]
        public string? FrozenPdfSha256 { get; set; }

        public DateTime? EContractIssuedAt { get; set; }

        public DateTime? EContractSyncedAt { get; set; }

        public DateTime? EContractCompletedAt { get; set; }
    }
}
