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

        public int? RequestProductType { get; set; }

        public int? SupplierId { get; set; } // NhaCungUng.CungUngId

        public string? GiaThamKhao { get; set; }

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

        /// <summary>1=Mới đặt, 2=Đã xác nhận, 3=Hoàn tất, 4=Đã huỷ</summary>
        public int StatusId { get; set; } = 1;

        /// <summary>1=Thanh toán khi nhận hàng (COD), 2=Chuyển khoản</summary>
        public int HinhThucThanhToan { get; set; } = 1;

        public int? NguoiTao { get; set; } // Users.UserId

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? NguoiSua { get; set; }

        public DateTime? NgaySua { get; set; }

        /// <summary>Giá trị đơn hàng (đồng) do nhà cung ứng xác nhận (A.5.2).</summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? GiaTriDon { get; set; }

        /// <summary>0=Chưa thanh toán, 1=Đã thanh toán (A.5.2).</summary>
        public int TrangThaiThanhToan { get; set; }

        public DateTime? NgayThanhToan { get; set; }

        /// <summary>Mã đơn hiển thị: DH + năm tháng ngày tạo + số đơn.</summary>
        [NotMapped]
        public string MaDon => FormatMaDon(Id, NgayTao);

        public static string FormatMaDon(int id, DateTime ngayTao) => $"DH{ngayTao:yyMMdd}-{id:D5}";

        /// <summary>Giá trị tạm tính từ giá tham khảo × số lượng khi giá tham khảo là số.</summary>
        public static decimal? UocTinhGiaTri(string? giaThamKhao, int soLuong)
        {
            if (string.IsNullOrWhiteSpace(giaThamKhao)) return null;
            var digits = new string(giaThamKhao.Where(char.IsDigit).ToArray());
            return decimal.TryParse(digits, out var v) && v > 0 ? v * Math.Max(soLuong, 1) : null;
        }
    }
}
