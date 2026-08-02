-- Seed i18n cho trang danh mục sản phẩm (ProductByCate + CongNghe/ThietBi/TaiSanTriTue). Idempotent.
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(600), en NVARCHAR(600));
INSERT INTO @s VALUES
 (N'product.cate.productsUnit',   N'sản phẩm',                                N'products'),
 (N'product.cate.showing',        N'Hiển thị',                                N'Showing'),
 (N'product.cate.ofTotal',        N'trong tổng số',                           N'of'),
 (N'product.cate.emptyField',     N'Chưa có sản phẩm nào trong lĩnh vực này.',N'No products in this field yet.'),
 (N'product.cate.backToTech',     N'← Quay lại danh sách công nghệ',          N'← Back to technology list'),
 (N'product.cate.supplier',       N'Nhà cung ứng:',                           N'Supplier:'),
 (N'product.cate.refPrice',       N'Giá tham khảo:',                          N'Reference price:'),
 (N'product.cate.techField',      N'Lĩnh vực công nghệ',                      N'Technology field'),
 (N'product.cate.subCategories',  N'Danh mục con',                            N'Subcategories'),
 (N'product.cate.priceRange',     N'Khoảng giá',                              N'Price range'),
 (N'product.cate.priceUnder5',    N'Dưới 5 triệu',                            N'Under 5 million'),
 (N'product.cate.price5to20',     N'5 – 20 triệu',                            N'5 – 20 million'),
 (N'product.cate.priceOver20',    N'Trên 20 triệu',                           N'Over 20 million'),
 (N'product.cate.statusReady',    N'Sẵn sàng chuyển giao',                    N'Ready for transfer'),
 (N'product.cate.statusConsult',  N'Có tư vấn triển khai',                    N'Implementation consulting available'),
 (N'product.cate.apply',          N'Áp dụng',                                 N'Apply'),
 (N'product.cate.clearFilter',    N'Xóa lọc',                                 N'Clear filters'),
 (N'product.cate.searchPlaceholder', N'Tìm theo tên sản phẩm, thiết bị, giải pháp...', N'Search by product, equipment, solution name...'),
 (N'product.cate.sort',           N'Sắp xếp',                                 N'Sort'),
 (N'product.cate.sortNewest',     N'Mới cập nhật',                            N'Recently updated'),
 (N'product.cate.sortNameAsc',    N'Tên A–Z',                                 N'Name A–Z'),
 (N'product.cate.sortNameDesc',   N'Tên Z–A',                                 N'Name Z–A'),
 (N'product.cate.sortPriceAsc',   N'Giá tăng dần',                            N'Price ascending'),
 (N'product.cate.sortPriceDesc',  N'Giá giảm dần',                            N'Price descending'),
 (N'product.cate.perPage',        N'/ trang',                                 N'/ page'),
 (N'product.cate.perPageAria',    N'Số sản phẩm/trang',                       N'Products per page');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
