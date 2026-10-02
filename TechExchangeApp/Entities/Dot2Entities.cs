using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    /// <summary>Một phiên bản nội dung sản phẩm (HSMT A.2.7, A.3.2, A.8.3). Ghi tự động khi lưu SanPhamCNTB.</summary>
    [Table("SanPhamCNTBVersions")]
    public class SanPhamCNTBVersion
    {
        [Key]
        public int Id { get; set; }
        public int SanPhamId { get; set; }
        public int VersionNo { get; set; }
        /// <summary>0 = trạng thái trước khi bắt đầu theo dõi, 1 = tạo mới, 2 = chỉnh sửa</summary>
        public byte Action { get; set; }
        public string SnapshotJson { get; set; } = "{}";
        /// <summary>Tên các trường thay đổi so với phiên bản trước, phân cách dấu phẩy.</summary>
        [StringLength(2000)]
        public string? ChangedFields { get; set; }
        [StringLength(256)]
        public string? ChangedBy { get; set; }
        public int? ChangedByUserId { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }

    public static class ProductVersionActions
    {
        public const byte Baseline = 0;
        public const byte Create = 1;
        public const byte Update = 2;
    }

    /// <summary>Cảnh báo sản phẩm trùng lặp / quá hạn cập nhật (HSMT A.8.2).</summary>
    [Table("ProductWarnings")]
    public class ProductWarning
    {
        [Key]
        public int Id { get; set; }
        public int SanPhamId { get; set; }
        /// <summary>1 = trùng tên, 2 = trùng mô tả, 3 = quá hạn cập nhật</summary>
        public byte WarningType { get; set; }
        [StringLength(450)]
        public string? GroupKey { get; set; }
        [StringLength(1000)]
        public string? Detail { get; set; }
        public DateTime DetectedAt { get; set; } = DateTime.Now;
        public DateTime LastSeenAt { get; set; } = DateTime.Now;
        /// <summary>0 = đang mở, 1 = quản trị đã xử lý, 2 = tự đóng (lần quét sau không còn)</summary>
        public byte Status { get; set; }
        public DateTime? ResolvedAt { get; set; }
        [StringLength(256)]
        public string? ResolvedBy { get; set; }
        [StringLength(500)]
        public string? Note { get; set; }
    }

    public static class ProductWarningTypes
    {
        public const byte DuplicateName = 1;
        public const byte DuplicateDescription = 2;
        public const byte Stale = 3;
    }

    public static class ProductWarningStatuses
    {
        public const byte Open = 0;
        public const byte Resolved = 1;
        public const byte AutoClosed = 2;
    }

    /// <summary>Bình luận của người dùng (HSMT B.3.5, A.7.1, A.5.4). Bảng UserComments.</summary>
    [Table("UserComments")]
    public class UserComment
    {
        [Key]
        public int Id { get; set; }
        /// <summary>1 = sản phẩm, 2 = gian hàng/nhà cung ứng, 3 = bài viết (xem CommentTargetTypes)</summary>
        public byte TargetType { get; set; }
        public int TargetId { get; set; }
        public int UserId { get; set; }
        public int? ParentId { get; set; }
        [Required, StringLength(2000)]
        public string Content { get; set; } = "";
        /// <summary>1 = hiển thị, 0 = đã ẩn</summary>
        public byte StatusId { get; set; } = 1;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? Modified { get; set; }
        [StringLength(256)]
        public string? ModifiedBy { get; set; }
        [StringLength(64)]
        public string? IpAddress { get; set; }
    }

    public static class CommentTargetTypes
    {
        public const byte Product = 1;
        public const byte Store = 2;
        public const byte Post = 3;
    }
}
