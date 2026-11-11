using TechExchangeApp.Entities;

namespace TechExchangeApp.Helpers
{
    /// <summary>
    /// Link sang gian hàng ở Triển lãm trực tuyến 3D (/trienlam, ứng dụng riêng).
    /// Gian hàng dùng chung mã với NhaCungUng.CungUngId; triển lãm chỉ dựng gian hàng cho
    /// NCC đang kích hoạt, đã duyệt (StatusId = 3) và có lĩnh vực — NCC khác trả 404.
    /// </summary>
    public static class ExhibitionHelper
    {
        public static string? BoothUrl(NhaCungUng? ncc, string? mainDomain)
        {
            if (ncc == null || ncc.IsActivated != true || ncc.StatusId != 3) return null;
            if (string.IsNullOrWhiteSpace(ncc.LinhVucId?.Trim(';', ' '))) return null;
            return $"{mainDomain}trienlam/Exhibition/Booth/{ncc.CungUngId}";
        }
    }
}
