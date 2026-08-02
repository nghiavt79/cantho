SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(500), en NVARCHAR(500));
INSERT INTO @s VALUES
 (N'common.noInfo',             N'Chưa có thông tin.',                        N'No information yet.'),
 (N'expert.otherExperts',       N'Chuyên gia khác',                          N'Other experts'),
 (N'expert.citations',          N'trích dẫn',                                N'citations'),
 (N'expert.phoneShort',         N'ĐT',                                       N'Phone'),
 (N'expert.address',            N'Địa chỉ',                                  N'Address'),
 (N'expert.tab.metrics',        N'Chỉ số KH',                                N'Science metrics'),
 (N'expert.tab.training',       N'Đào tạo & CT',                             N'Training & Work'),
 (N'expert.tab.publications',   N'Công bố & SHTT',                           N'Publications & IP'),
 (N'expert.tab.consulting',     N'Tư vấn & DA',                              N'Consulting & Projects'),
 (N'expert.tab.evidence',       N'Minh chứng',                              N'Evidence'),
 (N'expert.name',               N'Họ và tên',                                N'Full name'),
 (N'expert.degree',             N'Học hàm / Học vị',                         N'Academic title / Degree'),
 (N'expert.organization',       N'Cơ quan công tác',                         N'Organization'),
 (N'expert.position',           N'Chức vụ',                                  N'Position'),
 (N'expert.phone',              N'Điện thoại',                               N'Phone'),
 (N'expert.birthDate',          N'Ngày sinh',                                N'Date of birth'),
 (N'expert.intlId',             N'Mã định danh quốc tế',                     N'International identifier'),
 (N'expert.totalCitations',     N'Tổng số trích dẫn',                        N'Total citations'),
 (N'expert.researchFields',     N'Lĩnh vực nghiên cứu trọng tâm',            N'Key research fields'),
 (N'expert.trainingProcess',    N'Quá trình đào tạo',                        N'Education'),
 (N'expert.workProcess',        N'Quá trình công tác',                       N'Work history'),
 (N'expert.publications',       N'Bài báo khoa học / Báo cáo Hội nghị',      N'Scientific papers / Conference reports'),
 (N'expert.patents',            N'Bằng độc quyền Sáng chế / Tác quyền',      N'Patents / Copyrights'),
 (N'expert.researchProjects',   N'Dự án nghiên cứu đã chủ trì',              N'Research projects led'),
 (N'expert.deepConsulting',     N'Dịch vụ tư vấn chuyên sâu',                N'In-depth consulting services'),
 (N'expert.businessExperience', N'Kinh nghiệm thực chiến tại Doanh nghiệp',  N'Hands-on business experience'),
 (N'expert.attachments',        N'Hồ sơ đính kèm',                           N'Attachments'),
 (N'expert.associations',       N'Thành viên hiệp hội khoa học',             N'Scientific association memberships'),
 (N'expert.booksChapters',      N'Sách & Chương sách đã xuất bản',           N'Published books & book chapters');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
