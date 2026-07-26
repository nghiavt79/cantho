using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    [Table("ContentsYeuCau")]
    public class ContentsYeuCau
    {
        [Key]
        public long Id { get; set; }

        public string? Title { get; set; }
        public string? QueryString { get; set; }
        public string? Description { get; set; }
        public string? Contents { get; set; }

        public string? ChuongTrinh { get; set; }
        public string? PhieuDangKy { get; set; }
        public string? STINFO { get; set; }
        public string? HinhSTINFO { get; set; }

        public string? LinhVucId { get; set; }
        public string? Author { get; set; }

        public int? StatusId { get; set; }
        public int? MenuId { get; set; }

        public bool? IsHot { get; set; }

        public DateTime? Created { get; set; }
        public string? Creator { get; set; }

        public DateTime? Modified { get; set; }
        public string? Modifier { get; set; }

        public string? Image { get; set; }

        public DateTime? PublishedDate { get; set; }
        public DateTime? bEffectiveDate { get; set; }
        public DateTime? eEffectiveDate { get; set; }

        public bool? IsNew { get; set; }

        public string? Subject { get; set; }
        public string? Keyword { get; set; }

        public int? Like { get; set; }
        public int? DisLike { get; set; }
        public int? Viewed { get; set; }

        public int? TypeId { get; set; }

        public string? URL { get; set; }

        public bool? IsYoutube { get; set; }
        public string? ImageBig { get; set; }

        public int LanguageId { get; set; }
        public string Domain { get; set; } = string.Empty;

        public string? LinkInvite { get; set; }
        public string? LinkFile { get; set; }

        public DateTime? DemoDate { get; set; }

        public bool? IsReport { get; set; }

        public int? ParentId { get; set; }

        public string? UserLink { get; set; }

        public int? SiteId { get; set; }

        public int? PhieuYeuCauCNTBId { get; set; }

        // === Trường cấu trúc cho hồ sơ nhu cầu (tách từ khối Contents) ===
        public int? TrangThaiNhuCau { get; set; }      // 1 Đang tiếp nhận, 2 Đang xử lý, 3 Đã kết thúc
        public string? DiaPhuong { get; set; }
        public DateTime? HanTiepNhan { get; set; }
        public string? NganSach { get; set; }
        public string? HinhThucHopTac { get; set; }    // danh sách phân tách bằng ';'
        public string? MucTieu { get; set; }
        public string? HienTrang { get; set; }
        public string? YeuCauKyThuat { get; set; }
        public string? QuyMoTrienKhai { get; set; }
        public string? TieuChiChonDoiTac { get; set; }
    }
}
