SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(400), en NVARCHAR(400));
INSERT INTO @s VALUES
 (N'common.prev',              N'Trước',                                   N'Previous'),
 (N'common.next',              N'Sau',                                     N'Next'),
 (N'dichvu.noUnits',           N'Không tìm thấy đơn vị tư vấn nào phù hợp.',N'No matching consulting units found.'),
 (N'dichvu.noConsultants',     N'Không tìm thấy nhà tư vấn nào phù hợp.',   N'No matching consultants found.'),
 (N'dichvu.unitsCount',        N'đơn vị tư vấn',                           N'consulting units'),
 (N'dichvu.consultantsCount',  N'nhà tư vấn',                              N'consultants'),
 (N'dichvu.rating',            N'Đánh giá:',                               N'Rating:'),
 (N'dichvu.sendRequest',       N'Gửi yêu cầu',                             N'Send request'),
 (N'dichvu.viewProfile',       N'Xem hồ sơ',                               N'View profile');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted;
