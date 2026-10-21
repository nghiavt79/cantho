using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    // Sự kiện (HSMT Đợt 3) — gắn theo Contents.Id của tin bài bản tiếng Việt.

    /// <summary>Thông tin sự kiện của một tin bài (A.4.1, C.6).</summary>
    [Table("EventInfos")]
    public class EventInfo
    {
        [Key] public int Id { get; set; }
        public long ContentId { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        [StringLength(500)] public string? Location { get; set; }
        [StringLength(1000)] public string? LivestreamUrl { get; set; }
        public bool RegistrationEnabled { get; set; } = true;
        public DateTime? RegistrationDeadline { get; set; }
        public int? MaxAttendees { get; set; }
        /// <summary>Mốc nhắc lịch, số phút trước giờ bắt đầu, phân cách dấu phẩy.</summary>
        [StringLength(100)] public string ReminderOffsetsMinutes { get; set; } = "1440,60";
        /// <summary>Cờ kênh nhắc: 1 = email (+ chuông web cho người đăng nhập), 2 = SMS.</summary>
        public byte ReminderChannels { get; set; } = EventReminderChannels.Email;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? Modified { get; set; }
        [StringLength(256)] public string? Modifier { get; set; }

        public IEnumerable<int> ReminderOffsets() => (ReminderOffsetsMinutes ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var n) ? n : 0).Where(n => n > 0).Distinct();
    }

    public static class EventReminderChannels
    {
        public const byte Email = 1;
        public const byte Sms = 2;
    }

    /// <summary>Đăng ký tham gia sự kiện (A.4.1, B.1.6).</summary>
    [Table("EventRegistrations")]
    public class EventRegistration
    {
        [Key] public int Id { get; set; }
        public long ContentId { get; set; }
        public int? UserId { get; set; }
        [Required, StringLength(200)] public string FullName { get; set; } = "";
        [Required, StringLength(256)] public string Email { get; set; } = "";
        [StringLength(30)] public string? Phone { get; set; }
        [StringLength(300)] public string? Organization { get; set; }
        [StringLength(1000)] public string? Note { get; set; }
        /// <summary>1 = đã đăng ký, 2 = đã điểm danh, 0 = đã hủy</summary>
        public byte Status { get; set; } = EventRegistrationStatuses.Registered;
        [StringLength(32)] public string CheckInCode { get; set; } = "";
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? CheckedInAt { get; set; }
        [StringLength(256)] public string? CheckedInBy { get; set; }
        [StringLength(64)] public string? IpAddress { get; set; }
    }

    public static class EventRegistrationStatuses
    {
        public const byte Cancelled = 0;
        public const byte Registered = 1;
        public const byte CheckedIn = 2;
    }

    /// <summary>Lượt xem / quan tâm / đăng ký nhắc lịch của sự kiện (A.4.4, A.4.5).</summary>
    [Table("EventInteractions")]
    public class EventInteraction
    {
        [Key] public long Id { get; set; }
        public long ContentId { get; set; }
        /// <summary>1 = xem trang, 2 = quan tâm, 3 = đăng ký nhắc lịch, 4 = nhấp banner/đường dẫn, 5 = chia sẻ</summary>
        public byte InteractionType { get; set; }
        public int? UserId { get; set; }
        [StringLength(64)] public string? VisitorKey { get; set; }
        [StringLength(256)] public string? Email { get; set; }
        [StringLength(30)] public string? Phone { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }

    public static class EventInteractionTypes
    {
        public const byte View = 1;
        public const byte Interest = 2;
        public const byte Remind = 3;
        /// <summary>Nhấp vào banner, đường dẫn trỏ tới sự kiện (A.4.4).</summary>
        public const byte Click = 4;
        /// <summary>Bấm chia sẻ lên mạng xã hội (A.4.4).</summary>
        public const byte Share = 5;
    }

    /// <summary>Đã gửi nhắc lịch cho người nhận ở một mốc — chống gửi trùng (A.4.5).</summary>
    [Table("EventReminderLogs")]
    public class EventReminderLog
    {
        [Key] public int Id { get; set; }
        public long ContentId { get; set; }
        [StringLength(300)] public string RecipientKey { get; set; } = "";
        public int OffsetMinutes { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }

    /// <summary>Loại gian hàng của sự kiện: diện tích, vị trí, giá, dịch vụ đi kèm (A.4.3).</summary>
    [Table("EventBoothTypes")]
    public class EventBoothType
    {
        [Key] public int Id { get; set; }
        public long ContentId { get; set; }
        [Required, StringLength(200)] public string Name { get; set; } = "";
        [Column(TypeName = "decimal(8,2)")] public decimal? AreaM2 { get; set; }
        [StringLength(200)] public string? Position { get; set; }
        [Column(TypeName = "decimal(18,0)")] public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        [StringLength(1000)] public string? Services { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Đăng ký thuê gian hàng sự kiện (A.4.3).</summary>
    [Table("EventBoothBookings")]
    public class EventBoothBooking
    {
        [Key] public int Id { get; set; }
        public int BoothTypeId { get; set; }
        public long ContentId { get; set; }
        public int? UserId { get; set; }
        [Required, StringLength(300)] public string Organization { get; set; } = "";
        [Required, StringLength(200)] public string ContactName { get; set; } = "";
        [Required, StringLength(256)] public string Email { get; set; } = "";
        [Required, StringLength(30)] public string Phone { get; set; } = "";
        public int Quantity { get; set; } = 1;
        [StringLength(1000)] public string? Note { get; set; }
        /// <summary>1 = chờ duyệt, 2 = đã duyệt, 3 = từ chối, 0 = đã hủy</summary>
        public byte Status { get; set; } = EventBoothBookingStatuses.Pending;
        public DateTime Created { get; set; } = DateTime.Now;
        [StringLength(256)] public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        [StringLength(500)] public string? ReviewNote { get; set; }
    }

    public static class EventBoothBookingStatuses
    {
        public const byte Cancelled = 0;
        public const byte Pending = 1;
        public const byte Approved = 2;
        public const byte Rejected = 3;
    }
}
