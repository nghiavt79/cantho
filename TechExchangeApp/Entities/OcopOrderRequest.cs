using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    [Table("OcopOrderRequests")]
    public class OcopOrderRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int? SupplierId { get; set; } // NhaCungUng.CungUngId

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập điện thoại")]
        [StringLength(20)]
        public string DienThoai { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(300)]
        public string DiaChiGiao { get; set; } = null!;

        [Range(1, 100000, ErrorMessage = "Số lượng không hợp lệ")]
        public int SoLuong { get; set; } = 1;

        public string? GhiChu { get; set; }

        /// <summary>1=Mới đặt, 2=Đã xác nhận, 3=Hoàn tất</summary>
        public int StatusId { get; set; } = 1;

        public int? NguoiTao { get; set; } // Users.UserId

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? NguoiSua { get; set; }

        public DateTime? NgaySua { get; set; }
    }
}
