using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;

namespace TechExchangeApp.Services
{
    /// <summary>
    /// Đồng bộ dữ liệu đi kèm sản phẩm sau khi tạo, sửa, đổi trạng thái: bảng lĩnh vực và dữ liệu tra cứu.
    /// Lỗi đồng bộ chỉ ghi log, không làm hỏng thao tác chính của người dùng.
    /// </summary>
    public static class ProductPublishSync
    {
        private static ILogger Log(HttpContext http) =>
            http.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(ProductPublishSync));

        /// <summary>
        /// Tách CategoryId vào bảng lĩnh vực (trang lĩnh vực đọc từ bảng này). Không gọi sp_SyncSanPhamCNTB_Normalized
        /// vì thủ tục đó còn ghi lại ảnh, giá. Không có mã hợp lệ thì giữ nguyên các dòng đang có.
        /// </summary>
        public static async Task SyncCategoriesAsync(AppDbContext context, HttpContext http, int productId)
        {
            try
            {
                await context.Database.ExecuteSqlInterpolatedAsync($@"
                    DECLARE @Cat TABLE (CatId INT PRIMARY KEY);
                    INSERT INTO @Cat (CatId)
                    SELECT DISTINCT k.CatId
                    FROM dbo.SanPhamCNTB s
                    CROSS APPLY STRING_SPLIT(REPLACE(ISNULL(s.CategoryId, ''), ',', ';'), ';') v
                    JOIN dbo.Category k ON k.CatId = TRY_CAST(LTRIM(RTRIM(v.value)) AS INT)
                    WHERE s.ID = {productId};

                    IF EXISTS (SELECT 1 FROM @Cat)
                    BEGIN
                        DELETE FROM dbo.SanPhamCNTBCategory WHERE SanPhamCNTBId = {productId};
                        INSERT INTO dbo.SanPhamCNTBCategory (SanPhamCNTBId, CatId)
                        SELECT {productId}, CatId FROM @Cat;
                    END");
            }
            catch (Exception ex)
            {
                Log(http).LogWarning(ex, "Không đồng bộ được lĩnh vực của sản phẩm {Id}", productId);
            }
        }

        /// <summary>
        /// Cập nhật dữ liệu tra cứu của một sản phẩm: có khi đang Xuất bản, không có trong các trạng thái khác.
        /// Cột giống uspReIndexSearchContents, thêm mô tả chi tiết để tìm được theo nội dung.
        /// </summary>
        public static async Task SyncSearchIndexAsync(AppDbContext context, HttpContext http, int productId)
        {
            try
            {
                await context.Database.ExecuteSqlInterpolatedAsync($@"
                    DELETE FROM dbo.SearchIndexContents
                    WHERE RefId = {productId} AND TypeName IN (N'Công nghệ', N'Thiết bị', N'Tài sản trí tuệ', N'OCOP');

                    INSERT INTO dbo.SearchIndexContents
                        (ImgPreview, Title, [Description], Contents, RefId, TypeName, URL, Created, Creator, LanguageId)
                    SELECT s.QuyTrinhHinhAnh, REPLACE(s.Name, '-', ' '), ISNULL(s.MoTaNgan, ''),
                           ISNULL(s.MoTaNgan, '') + ' ' + ISNULL(s.Thongso, '') + ' ' + ISNULL(s.UuDiem, '') + ' ' + ISNULL(CAST(s.MoTa AS NVARCHAR(MAX)), ''),
                           s.ID,
                           CASE s.ProductType WHEN 1 THEN N'Công nghệ' WHEN 2 THEN N'Thiết bị' WHEN 3 THEN N'Tài sản trí tuệ' WHEN 4 THEN N'OCOP' END,
                           CASE WHEN s.LanguageId = 2 THEN N'/en' ELSE N'' END
                             + CASE WHEN s.ProductType = 4 THEN N'/ocop/'
                                    WHEN s.LanguageId = 2 THEN N'/products/'
                                    ELSE N'/san-pham/chi-tiet/' END
                             + s.QueryString + N'-' + CAST(s.ID AS NVARCHAR(50)),
                           GETDATE(), 'sys', CAST(s.LanguageId AS NVARCHAR(10))
                    FROM dbo.SanPhamCNTB s
                    WHERE s.ID = {productId} AND s.StatusId = 3;");
            }
            catch (Exception ex)
            {
                Log(http).LogWarning(ex, "Không cập nhật được dữ liệu tra cứu của sản phẩm {Id}", productId);
            }
        }

        /// <summary>
        /// Sản phẩm của nhà cung ứng chưa xuất bản, chưa kích hoạt thì không xuất bản được:
        /// bên mua sẽ thấy sản phẩm nhưng không mời được bên bán ở bước 3. Trả về lý do, null nếu hợp lệ.
        /// </summary>
        public static async Task<string?> GetPublishBlockReasonAsync(AppDbContext context, int? ncuId)
        {
            if (ncuId is null or <= 0) return null;

            var ncc = await context.NhaCungUngs.AsNoTracking()
                .Where(n => n.CungUngId == ncuId.Value)
                .Select(n => new { n.FullName, n.IsActivated, n.StatusId })
                .FirstOrDefaultAsync();

            if (ncc == null || (ncc.IsActivated == true && ncc.StatusId == 3)) return null;

            return $"Chưa xuất bản được: hồ sơ nhà cung ứng \"{ncc.FullName}\" chưa ở trạng thái Xuất bản và Kích hoạt.";
        }
    }
}
