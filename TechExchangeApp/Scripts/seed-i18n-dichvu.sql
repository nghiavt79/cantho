SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'common.keyword',        N'Từ khóa',                                     N'Keyword'),
 (N'dichvu.pageTitle',      N'Dịch vụ tư vấn khoa học công nghệ',           N'Science & technology consulting services'),
 (N'dichvu.breadcrumb',     N'Dịch vụ tư vấn',                              N'Consulting services'),
 (N'dichvu.title',          N'Dịch vụ tư vấn khoa học, công nghệ và đổi mới sáng tạo', N'Science, technology and innovation consulting services'),
 (N'dichvu.subtitle',       N'Kết nối doanh nghiệp, tổ chức, cá nhân với đơn vị tư vấn và chuyên gia hàng đầu trong lĩnh vực khoa học công nghệ tại Thành phố Cần Thơ.', N'Connecting businesses, organizations and individuals with leading consulting units and experts in science and technology in Can Tho City.'),
 (N'dichvu.findUnit',       N'Tìm đơn vị tư vấn',                           N'Find a consulting unit'),
 (N'dichvu.searchPh',       N'Nhập tên đơn vị, chuyên gia...',              N'Enter unit or expert name...'),
 (N'dichvu.field',          N'Lĩnh vực tư vấn',                             N'Consulting field'),
 (N'dichvu.tabUnit',        N'Đơn vị tư vấn',                               N'Consulting units'),
 (N'dichvu.tabConsultant',  N'Nhà tư vấn',                                  N'Consultants'),
 (N'dichvu.needConsult',    N'Bạn cần tư vấn?',                             N'Need consulting?'),
 (N'dichvu.ctaSidebar',     N'Gửi yêu cầu tư vấn để được kết nối với đơn vị và chuyên gia phù hợp nhất.', N'Send a consulting request to connect with the most suitable units and experts.'),
 (N'dichvu.popularFields',  N'Lĩnh vực tư vấn phổ biến',                    N'Popular consulting fields');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
