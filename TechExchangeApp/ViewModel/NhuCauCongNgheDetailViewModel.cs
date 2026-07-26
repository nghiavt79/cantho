using TechExchangeApp.Entities;

namespace TechExchangeApp.ViewModel
{
    public class NhuCauCongNgheDetailViewModel
    {
        public ContentYeucauDetailVm? Detail { get; set; }

        public List<RelationItemVm> Relations { get; set; } = new();

        public List<Album> Images { get; set; } = new();

        public int TargetId { get; set; }

        public int? CommentTypeId { get; set; }
        public PhieuYeuCauCNViewModel PhieuYeuCau { get; set; } = new();

        public PortletYeuCauMoiViewModel YeuCauMoi { get; set; } = new();
    }

    public class ContentYeucauDetailVm
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Contents { get; set; }
        public string? Author { get; set; }
        public string? QueryString { get; set; }
        public int MenuId { get; set; }
        public int? Viewed { get; set; }
        public int? Like { get; set; }
        public DateTime? PublishedDate { get; set; }
        public DateTime? Modified { get; set; }
        public int TypeId { get; set; }

        // Badge lĩnh vực
        public string? LinhVucText { get; set; }

        // Trường cấu trúc
        public int? TrangThaiNhuCau { get; set; }
        public string? DiaPhuong { get; set; }
        public DateTime? HanTiepNhan { get; set; }
        public string? NganSach { get; set; }
        public string? HinhThucHopTac { get; set; }
        public string? MucTieu { get; set; }
        public string? HienTrang { get; set; }
        public string? YeuCauKyThuat { get; set; }
        public string? QuyMoTrienKhai { get; set; }
        public string? TieuChiChonDoiTac { get; set; }

        // Mã nhu cầu hiển thị
        public string MaNhuCau => $"NC-{Id}";
        // true nếu nhu cầu còn tiếp nhận đề xuất (trạng thái != đã kết thúc)
        public bool DangTiepNhan => (TrangThaiNhuCau ?? 1) != 3;
    }
    public class RelationItemVm
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? QueryString { get; set; }
        public int MenuId { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string? Image { get; set; }
        public string? LinhVucText { get; set; }
        public int? TrangThaiNhuCau { get; set; }
        public string DetailUrl { get; set; } = "";
    }

}
