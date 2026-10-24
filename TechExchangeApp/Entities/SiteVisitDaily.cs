using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    /// <summary>
    /// Thống kê lượt truy cập toàn Sàn theo ngày (số liệu Báo cáo kết quả cung cấp dịch vụ – Mẫu số 2 PL VIII TT41).
    /// Bảng tạo bằng Scripts/add-site-visit-daily.sql.
    /// </summary>
    [Table("SiteVisitDaily")]
    public class SiteVisitDaily
    {
        [Key]
        [Column(TypeName = "date")]
        public DateTime VisitDate { get; set; }

        /// <summary>Số lượt xem trang.</summary>
        public int PageViews { get; set; }

        /// <summary>Số người truy cập (khác nhau theo IP + trình duyệt trong ngày).</summary>
        public int Visitors { get; set; }
    }
}
