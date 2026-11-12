using System.Collections.Generic;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Helpers
{
    /// <summary>
    /// Link sang gian hàng ở Triển lãm trực tuyến 3D (/trienlam, ứng dụng riêng).
    /// Gian hàng dùng chung mã với NhaCungUng.CungUngId; triển lãm chỉ dựng gian hàng cho
    /// NCC đang kích hoạt, đã duyệt (StatusId = 3) và có lĩnh vực — NCC khác trả 404.
    /// Đợt này chỉ hiện nút cho các NCC trong danh sách (mặc định 9 NCC nhập từ phiếu mẫu
    /// khách cung cấp); đổi danh sách bằng appsettings "Exhibition:BoothSupplierIds" (mã bản tiếng Việt).
    /// </summary>
    public static class ExhibitionHelper
    {
        // MISA, Machinex, China Ecotek, KATEC, TPT, Aquadelta, Kim Ngưu, Phúc Trường Hải, Cát Sạch MeKong
        private static HashSet<int> _supplierIds = new() { 8096, 7225, 8887, 8888, 8889, 8890, 8891, 8892, 8904 };

        public static void Configure(int[]? supplierIds)
        {
            if (supplierIds != null && supplierIds.Length > 0) _supplierIds = new HashSet<int>(supplierIds);
        }

        public static string? BoothUrl(NhaCungUng? ncc, string? mainDomain)
        {
            if (ncc == null || ncc.IsActivated != true || ncc.StatusId != 3) return null;
            if (string.IsNullOrWhiteSpace(ncc.LinhVucId?.Trim(';', ' '))) return null;
            // Bản tiếng Anh xét theo mã bản tiếng Việt gốc
            if (!_supplierIds.Contains(ncc.OriginalId ?? ncc.CungUngId)) return null;
            return $"{mainDomain}trienlam/Exhibition/Booth/{ncc.CungUngId}";
        }
    }
}
