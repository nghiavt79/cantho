-- Seed i18n cho _DetailThietBi (chi tiết thiết bị). Idempotent.
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(600), en NVARCHAR(600));
INSERT INTO @s VALUES
 (N'product.equip.tab.info',           N'Thông tin',                             N'Information'),
 (N'product.equip.tab.advantagePrice', N'Ưu điểm & Giá',                         N'Advantages & Price'),
 (N'product.equip.tab.supplier',       N'Nhà cung ứng',                          N'Supplier'),
 (N'product.equip.deviceInfo',         N'Thông tin thiết bị',                    N'Equipment information'),
 (N'product.equip.deviceName',         N'Tên thiết bị / máy móc',                N'Equipment / machine name'),
 (N'product.equip.applicationField',   N'Lĩnh vực áp dụng',                      N'Application field'),
 (N'product.equip.developmentLevel',   N'Mức độ phát triển',                     N'Development level'),
 (N'product.equip.levelPilot',         N'Đã ứng dụng thử nghiệm (Pilot)',        N'Pilot tested'),
 (N'product.equip.levelIndustrial',    N'Đã ứng dụng công nghiệp',               N'Industrial application'),
 (N'product.equip.levelCommercial',    N'Đã thương mại hóa',                     N'Commercialized'),
 (N'product.equip.sold',               N'Đã bán:',                               N'Sold:'),
 (N'product.equip.units',              N'đơn vị',                                N'units'),
 (N'product.equip.stockStatus',        N'Tình trạng hàng:',                      N'Stock status:'),
 (N'product.equip.inStock',            N'Còn hàng',                              N'In stock'),
 (N'product.equip.outStock',           N'Hết hàng',                              N'Out of stock'),
 (N'product.equip.keywords',           N'Từ khoá',                               N'Keywords'),
 (N'product.equip.specOperation',      N'Thông số kỹ thuật & Vận hành',          N'Technical specifications & Operation'),
 (N'product.equip.mainSpec',           N'Thông số kỹ thuật chính',               N'Main specifications'),
 (N'product.equip.principle',          N'Mô tả nguyên lý hoạt động',             N'Operating principle'),
 (N'product.equip.operationVideo',     N'Video vận hành',                        N'Operation video'),
 (N'product.equip.advantagePriceTitle',N'Ưu điểm & Giá bán',                     N'Advantages & Price'),
 (N'product.equip.refPriceVnd',        N'Giá tham khảo (VNĐ)',                   N'Reference price (VND)'),
 (N'product.equip.warrantySupport',    N'Chế độ bảo hành & Hỗ trợ kỹ thuật',     N'Warranty & Technical support'),
 (N'product.equip.awards',             N'Giải thưởng / Chứng nhận',              N'Awards / Certifications'),
 (N'product.equip.supplierInfo',       N'Thông tin nhà cung ứng',                N'Supplier information'),
 (N'product.equip.supplierName',       N'Tên đơn vị sở hữu / cung ứng',          N'Owner / supplier name'),
 (N'product.equip.contactInfo',        N'Thông tin liên hệ',                     N'Contact information'),
 (N'product.equip.address',            N'Địa chỉ:',                              N'Address:'),
 (N'product.equip.representative',     N'Người đại diện:',                       N'Representative:'),
 (N'product.equip.phone',              N'Điện thoại:',                           N'Phone:'),
 (N'product.equip.viewSupplierProfile',N'Xem hồ sơ nhà cung ứng',                N'View supplier profile');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
