-- Seed i18n cho trang chi tiết sản phẩm (Product/Detail + partials). Idempotent.
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(400), en NVARCHAR(400));
INSERT INTO @s VALUES
 (N'common.category',                 N'Danh mục',                          N'Category'),
 (N'common.status',                   N'Trạng thái',                        N'Status'),
 (N'product.detail.heading',          N'Chi tiết sản phẩm',                 N'Product details'),
 (N'product.detail.reviews',          N'đánh giá',                          N'reviews'),
 (N'product.detail.views',            N'lượt xem',                          N'views'),
 (N'product.detail.supplier',         N'Nhà cung cấp',                      N'Supplier'),
 (N'product.detail.origin',           N'Xuất xứ',                           N'Origin'),
 (N'product.detail.price',            N'Giá tham khảo',                     N'Reference price'),
 (N'product.detail.priceContact',     N'Liên hệ báo giá',                   N'Contact for quote'),
 (N'product.detail.inquiryBtn',       N'Gửi yêu cầu đặt mua / quan tâm',    N'Send purchase / interest request'),
 (N'product.detail.unknownType',      N'Không xác định loại sản phẩm.',     N'Unknown product type.'),
 (N'product.detail.quickInfo',        N'Thông tin nhanh',                   N'Quick info'),
 (N'product.detail.code',             N'Mã sản phẩm',                       N'Product code'),
 (N'product.detail.updated',          N'Cập nhật',                          N'Updated'),
 (N'product.detail.statusIntroducing',N'Đang giới thiệu',                   N'Introducing'),
 (N'product.detail.relatedCategories',N'Danh mục liên quan',                N'Related categories');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
