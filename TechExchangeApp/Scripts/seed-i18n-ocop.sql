SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'ocop.pageTitle',        N'Góc trưng bày Sản phẩm OCOP & Truy xuất nguồn gốc', N'OCOP Product Showcase & Traceability'),
 (N'ocop.eyebrow',          N'Góc trưng bày',                                N'Showcase'),
 (N'ocop.heroTitle',        N'Sản phẩm OCOP & Truy xuất nguồn gốc',          N'OCOP Products & Traceability'),
 (N'ocop.heroDesc',         N'Đặc sản, nông sản và sản phẩm thủ công đạt chứng nhận OCOP của Cần Thơ — mỗi sản phẩm gắn một mã truy xuất riêng để xác thực đơn vị sản xuất, vùng nguyên liệu và chứng nhận chất lượng.', N'Specialty foods, agricultural products and handicrafts certified OCOP in Can Tho — each product carries its own traceability code to verify the producer, material region and quality certification.'),
 (N'ocop.statProducts',     N'Sản phẩm OCOP',                                N'OCOP products'),
 (N'ocop.statTraceable',    N'Có mã truy xuất',                              N'With traceability code'),
 (N'ocop.statOrigins',      N'Vùng nguyên liệu',                             N'Material regions'),
 (N'ocop.step1Title',       N'Quét mã QR',                                   N'Scan the QR code'),
 (N'ocop.step1Desc',        N'Quét mã QR trên bao bì hoặc tại quầy trưng bày sản phẩm.', N'Scan the QR code on the packaging or at the product display counter.'),
 (N'ocop.step2Title',       N'Xem hồ sơ nguồn gốc',                          N'View the traceability record'),
 (N'ocop.step2Desc',        N'Xem thông tin đơn vị sản xuất, vùng nguyên liệu, ngày đóng gói.', N'View producer information, material region and packaging date.'),
 (N'ocop.step3Title',       N'Xác thực chứng nhận',                          N'Verify certification'),
 (N'ocop.step3Desc',        N'Đối chiếu chứng nhận OCOP, VietGAP và các tiêu chuẩn liên quan.', N'Cross-check OCOP, VietGAP and related standards certification.'),
 (N'ocop.featuredTitle',    N'Sản phẩm OCOP nổi bật',                        N'Featured OCOP products'),
 (N'ocop.hasTraceCode',     N'Có mã truy xuất',                              N'Has traceability code'),
 (N'ocop.emptyLine1',       N'Chưa có sản phẩm OCOP nào được đăng tải.',     N'No OCOP products have been published yet.'),
 (N'ocop.emptyLine2',       N'Đơn vị sản xuất có thể liên hệ sàn để đăng ký trưng bày sản phẩm.', N'Producers can contact the exchange to register for product display.'),
 (N'ocop.descFallback',     N'Đang cập nhật thông tin sản phẩm.',            N'Product information is being updated.'),
 (N'ocop.sendOrder',        N'Gửi yêu cầu đặt mua',                          N'Send purchase request'),
 (N'ocop.verified',         N'Đã xác thực nguồn gốc',                        N'Traceability verified'),
 (N'ocop.qrAlt',            N'Mã QR truy xuất nguồn gốc',                    N'Traceability QR code'),
 (N'ocop.qrCaption',        N'Quét mã để xem lại hồ sơ truy xuất này',       N'Scan to view this traceability record again'),
 (N'ocop.traceCode',        N'Mã truy xuất',                                 N'Traceability code'),
 (N'ocop.producer',         N'Đơn vị sản xuất',                              N'Producer'),
 (N'ocop.materialRegion',   N'Vùng nguyên liệu',                             N'Material region'),
 (N'ocop.ranking',          N'Xếp hạng',                                     N'Ranking'),
 (N'ocop.stars',            N'sao',                                          N'stars'),
 (N'ocop.otherProducts',    N'Sản phẩm OCOP khác',                          N'Other OCOP products'),
 (N'ocop.backToShowcase',   N'Quay lại Góc trưng bày OCOP',                  N'Back to OCOP Showcase');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
