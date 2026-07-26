using System.ComponentModel.DataAnnotations;

namespace TechExchangeApp.Areas.Cms.Models
{
    public class ContentsYeuCauListItemVm
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public string? QueryString { get; set; }
        public string? LinhVucText { get; set; }
        public string? DiaPhuong { get; set; }
        public string? Author { get; set; }
        public int? StatusId { get; set; }
        public string? StatusTitle { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public DateTime? PublishedDate { get; set; }
        public int? PhieuYeuCauCNTBId { get; set; }
        public string SourceLabel { get; set; } = "";
        public string PublicUrl => $"/tim-mua-cong-nghe/{QueryString}-{Id}";
    }

    public class ContentsYeuCauEditVm
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(500)]
        public string Title { get; set; } = "";

        [StringLength(500)]
        public string? QueryString { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public string? Contents { get; set; }

        public string? LinhVucId { get; set; }

        [StringLength(500)]
        public string? Keyword { get; set; }

        [StringLength(300)]
        public string? Image { get; set; }

        public int? StatusId { get; set; }
        public DateTime? PublishedDate { get; set; }

        [StringLength(200)]
        public string? Author { get; set; }

        [StringLength(500)]
        public string? URL { get; set; }

        public int? TrangThaiNhuCau { get; set; }

        [StringLength(200)]
        public string? DiaPhuong { get; set; }

        public DateTime? HanTiepNhan { get; set; }

        [StringLength(200)]
        public string? NganSach { get; set; }

        [StringLength(500)]
        public string? HinhThucHopTac { get; set; }

        public string? MucTieu { get; set; }
        public string? HienTrang { get; set; }
        public string? YeuCauKyThuat { get; set; }
        public string? QuyMoTrienKhai { get; set; }
        public string? TieuChiChonDoiTac { get; set; }

        public int? MenuId { get; set; }
        public int? TypeId { get; set; }
        public int LanguageId { get; set; }
        public string Domain { get; set; } = "";
        public int? SiteId { get; set; }
        public DateTime? Created { get; set; }
        public string? Creator { get; set; }
        public DateTime? Modified { get; set; }
        public string? Modifier { get; set; }
        public int? PhieuYeuCauCNTBId { get; set; }
        public string SourceLabel { get; set; } = "";
        public string? SourceDetail { get; set; }
    }
}
