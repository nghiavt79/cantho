using TechExchangeApp.Entities;

namespace TechExchangeApp.Helpers
{
    /// <summary>
    /// Link sang gian hàng ở Triển lãm trực tuyến 3D (/trienlam, ứng dụng riêng).
    /// Gian hàng dùng chung mã với NhaCungUng.CungUngId. NCC đã đăng ký tham gia triển lãm là NCC
    /// được gán số gian hàng (NhaCungUng.BoothNumber, thiết lập ở trang quản trị triển lãm).
    /// Truyền vào bản VI (bản EN không có số gian hàng — controller tự lấy bản gốc theo OriginalId).
    /// </summary>
    public static class ExhibitionHelper
    {
        public static string? BoothUrl(NhaCungUng? ncc, string? mainDomain)
        {
            if (ncc == null || string.IsNullOrWhiteSpace(ncc.BoothNumber)) return null;
            // Triển lãm chỉ mở gian hàng của NCC đang kích hoạt (NCC khác trả 404)
            if (ncc.IsActivated != true || ncc.StatusId != 3) return null;
            return $"{mainDomain}trienlam/Exhibition/Booth/{ncc.CungUngId}";
        }
    }
}
