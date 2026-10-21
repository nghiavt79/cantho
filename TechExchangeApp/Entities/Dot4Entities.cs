using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    // Bổ sung chức năng HSMT — Đợt 4.

    /// <summary>Hạng nhà cung cấp và tiêu chí gợi ý xếp hạng (A.2.2, A.2.4).</summary>
    [Table("SupplierTiers")]
    public class SupplierTier
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(100)] public string Name { get; set; } = "";
        /// <summary>Hạng cao hơn có Rank lớn hơn.</summary>
        public int Rank { get; set; }
        [StringLength(20)] public string BadgeColor { get; set; } = "#6c757d";
        public int MinProducts { get; set; }
        public int MinContacts { get; set; }
        public int MinViews { get; set; }
        /// <summary>Số lượt giao dịch tối thiểu (đơn đặt mua chưa hủy) – HSMT A.2.4.</summary>
        public int MinTransactions { get; set; }
        /// <summary>Doanh thu tối thiểu (đồng): tổng giá trị đơn đã hoàn tất hoặc đã thanh toán – HSMT A.2.4.</summary>
        [Column(TypeName = "decimal(18,0)")] public decimal MinRevenue { get; set; }
        /// <summary>Điểm đánh giá trung bình sản phẩm tối thiểu (0–5) – chất lượng sản phẩm, HSMT A.2.4.</summary>
        [Column(TypeName = "decimal(3,1)")] public decimal MinRating { get; set; }
        public bool RequireVerified { get; set; }
        [StringLength(500)] public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Gói gian hàng: số sản phẩm tối đa, giá, thời hạn (A.2.3).</summary>
    [Table("StorePackages")]
    public class StorePackage
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(200)] public string Name { get; set; } = "";
        public int MaxProducts { get; set; }
        [Column(TypeName = "decimal(18,0)")] public decimal Price { get; set; }
        public int DurationDays { get; set; } = 365;
        [StringLength(500)] public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Nhà cung cấp mua / được cấp gói gian hàng (A.2.3).</summary>
    [Table("SupplierSubscriptions")]
    public class SupplierSubscription
    {
        [Key] public int Id { get; set; }
        public int CungUngId { get; set; }
        public int PackageId { get; set; }
        [Column(TypeName = "date")] public DateTime StartDate { get; set; }
        [Column(TypeName = "date")] public DateTime EndDate { get; set; }
        public int MaxProducts { get; set; }
        [Column(TypeName = "decimal(18,0)")] public decimal Price { get; set; }
        /// <summary>1 = hiệu lực, 0 = đã hủy</summary>
        public byte Status { get; set; } = 1;
        [StringLength(500)] public string? Note { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        [StringLength(256)] public string? CreatedBy { get; set; }
    }

    /// <summary>Tỷ lệ hoa hồng theo loại giao dịch (A.6.1).</summary>
    [Table("CommissionRates")]
    public class CommissionRate
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(50)] public string Code { get; set; } = "";
        [Required, StringLength(200)] public string Name { get; set; } = "";
        [Column(TypeName = "decimal(5,2)")] public decimal RatePercent { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Một khoản thu / chi trong sổ thu chi của Sàn (A.6.1).</summary>
    [Table("FinanceEntries")]
    public class FinanceEntry
    {
        [Key] public int Id { get; set; }
        [Column(TypeName = "date")] public DateTime EntryDate { get; set; }
        /// <summary>1 = thu, 2 = chi</summary>
        public byte Direction { get; set; } = FinanceDirections.Income;
        [Required, StringLength(50)] public string Category { get; set; } = FinanceCategories.Other;
        [Column(TypeName = "decimal(18,0)")] public decimal Amount { get; set; }
        public int? CungUngId { get; set; }
        [StringLength(50)] public string? RefType { get; set; }
        public int? RefId { get; set; }
        [Column(TypeName = "decimal(18,0)")] public decimal? BaseAmount { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal? RatePercent { get; set; }
        [StringLength(500)] public string? Description { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        [StringLength(256)] public string? CreatedBy { get; set; }
    }

    public static class FinanceDirections
    {
        public const byte Income = 1;
        public const byte Expense = 2;
    }

    public static class FinanceCategories
    {
        public const string StoreFee = "PHI_GIAN_HANG";
        public const string EventBoothFee = "PHI_GIAN_HANG_SU_KIEN";
        public const string Commission = "HOA_HONG";
        public const string Other = "KHAC";

        public static string Label(string c) => c switch
        {
            StoreFee => "Phí gian hàng",
            EventBoothFee => "Phí gian hàng sự kiện",
            Commission => "Hoa hồng giao dịch",
            _ => "Khác"
        };
    }

    public static class FinanceRefTypes
    {
        public const string Subscription = "SUBSCRIPTION";
        public const string BoothBooking = "BOOTH_BOOKING";
        public const string Commission = "COMMISSION";
    }

    /// <summary>Phòng trưng bày (A.3.3).</summary>
    [Table("Showrooms")]
    public class Showroom
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(200)] public string Name { get; set; } = "";
        [StringLength(300)] public string? Location { get; set; }
        [StringLength(500)] public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Vị trí trưng bày trong phòng; SanPhamId null = đang trống (A.3.4).</summary>
    [Table("ShowroomSlots")]
    public class ShowroomSlot
    {
        [Key] public int Id { get; set; }
        public int ShowroomId { get; set; }
        [Required, StringLength(50)] public string Code { get; set; } = "";
        [StringLength(300)] public string? Description { get; set; }
        public int? SanPhamId { get; set; }
        public DateTime? AssignedAt { get; set; }
        [StringLength(256)] public string? AssignedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Biểu mẫu động do quản trị định nghĩa (A.7.2, B.1.6).</summary>
    [Table("DynamicForms")]
    public class DynamicForm
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(300)] public string Title { get; set; } = "";
        [Required, StringLength(200)] public string Slug { get; set; } = "";
        [StringLength(1000)] public string? Description { get; set; }
        public string FieldsJson { get; set; } = "[]";
        [StringLength(100)] public string? SubmitText { get; set; }
        [StringLength(500)] public string? SuccessMessage { get; set; }
        [StringLength(500)] public string? RedirectUrl { get; set; }
        [StringLength(500)] public string? NotifyEmails { get; set; }
        public bool SendConfirmation { get; set; }
        public int LanguageId { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? Modified { get; set; }
        [StringLength(256)] public string? Modifier { get; set; }
    }

    /// <summary>Một lần gửi biểu mẫu.</summary>
    [Table("DynamicFormSubmissions")]
    public class DynamicFormSubmission
    {
        [Key] public int Id { get; set; }
        public int FormId { get; set; }
        public string DataJson { get; set; } = "{}";
        [StringLength(256)] public string? Email { get; set; }
        public int? UserId { get; set; }
        [StringLength(64)] public string? IpAddress { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }

    /// <summary>Định nghĩa một trường của biểu mẫu động (lưu trong DynamicForms.FieldsJson).</summary>
    public class DynamicFormField
    {
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        /// <summary>text | email | tel | number | date | textarea | select | checkbox</summary>
        public string Type { get; set; } = "text";
        public bool Required { get; set; }
        public List<string>? Options { get; set; }
        public string? Placeholder { get; set; }

        public static readonly string[] AllowedTypes = { "text", "email", "tel", "number", "date", "textarea", "select", "checkbox" };
    }
}
