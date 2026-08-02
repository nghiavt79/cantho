SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(500), en NVARCHAR(500));
INSERT INTO @s VALUES
 (N'supplier.supplyFields',         N'Lĩnh vực cung ứng',                         N'Supply fields'),
 (N'supplier.otherSuppliers',       N'Nhà cung ứng khác',                         N'Other suppliers'),
 (N'supplier.tab.servicesProducts', N'Dịch vụ & Sản phẩm',                        N'Services & Products'),
 (N'supplier.tab.capabilityFields', N'Năng lực & Lĩnh vực',                       N'Capabilities & Fields'),
 (N'supplier.tab.digitalProfile',   N'Hồ sơ năng lực số',                         N'Digital capability profile'),
 (N'supplier.organizationName',     N'Tên đơn vị',                                N'Organization name'),
 (N'supplier.shortName',            N'Tên viết tắt',                              N'Abbreviated name'),
 (N'supplier.organizationType',     N'Loại hình tổ chức',                         N'Organization type'),
 (N'supplier.taxCode',              N'Mã số thuế',                                N'Tax code'),
 (N'supplier.representative',       N'Người đại diện',                            N'Representative'),
 (N'supplier.mainActivityFields',   N'Lĩnh vực hoạt động chính',                  N'Main activity fields'),
 (N'supplier.missionCoreValue',     N'Chức năng nhiệm vụ / Giá trị cốt lõi',       N'Mission / Core values'),
 (N'supplier.scienceTechServices',  N'Dịch vụ khoa học và công nghệ',             N'Science and technology services'),
 (N'supplier.techProductsResults',  N'Sản phẩm công nghệ / Kết quả nghiên cứu',   N'Technology products / Research results'),
 (N'supplier.officeImages',         N'Hình ảnh trụ sở / Hoạt động tiêu biểu',     N'Office images / Featured activities'),
 (N'supplier.viewLargeImage',       N'Nhấn để xem ảnh lớn',                       N'Click to view larger image'),
 (N'supplier.introVideo',           N'Video giới thiệu đơn vị',                   N'Organization introduction video'),
 (N'supplier.videoNotSupported',    N'Trình duyệt của bạn không hỗ trợ hiển thị video.', N'Your browser does not support video playback.'),
 (N'supplier.downloadVideo',        N'Tải video về máy',                          N'Download video'),
 (N'supplier.viewDownloadVideo',    N'Xem video / Tải video',                     N'View video / Download video'),
 (N'supplier.certificates',         N'Chứng nhận năng lực / Giấy phép',           N'Capability certificates / Licenses'),
 (N'supplier.downloadCertificate',  N'Tải file chứng nhận',                       N'Download certificate file');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
