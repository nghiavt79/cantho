using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    /// <summary>Người ký trên eContract MobiFone, snapshot lúc phát hành.</summary>
    [Table("EContractRecipients")]
    public class EContractRecipient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProjectContractId { get; set; }

        public int? UserId { get; set; }

        /// <summary>1=Buyer (Bên B), 2=Seller (Bên A)</summary>
        public int BusinessRole { get; set; }

        public long? RecipientId { get; set; }

        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = null!;

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(30)]
        public string? Phone { get; set; }

        /// <summary>MST/CCCD</summary>
        [StringLength(50)]
        public string? CardId { get; set; }

        public int Ordering { get; set; } = 1;

        public int? Status { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>Vùng ký đã gửi lên eContract; một người ký có thể có nhiều vùng.</summary>
    [Table("EContractFields")]
    public class EContractField
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int EContractRecipientId { get; set; }

        public long? FieldId { get; set; }

        public long? DocumentId { get; set; }

        public int FieldType { get; set; } = 3;

        public int Page { get; set; }

        public double CoordinateX { get; set; }

        /// <summary>Tọa độ cộng dồn theo trang</summary>
        public double CoordinateY { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }
    }

    /// <summary>Checkpoint từng thao tác gọi eContract, phục vụ chống tạo trùng và đối soát.</summary>
    [Table("EContractOperations")]
    public class EContractOperation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string OperationKey { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = null!;

        public int ProjectContractId { get; set; }

        [StringLength(128)]
        public string? InputSha256 { get; set; }

        /// <summary>See EContractOperationState enum</summary>
        public int State { get; set; }

        [StringLength(64)]
        public string? CorrelationId { get; set; }

        [StringLength(1000)]
        public string? Error { get; set; }

        public DateTime? LeaseUntil { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }
    }
}
