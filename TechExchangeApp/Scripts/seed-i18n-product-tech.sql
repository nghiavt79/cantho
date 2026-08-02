-- Seed i18n cho _DetailCongNghe (chi tiết công nghệ). Idempotent.
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(600), en NVARCHAR(600));
INSERT INTO @s VALUES
 (N'product.tech.heading',          N'Chi tiết công nghệ',                          N'Technology details'),
 (N'product.tech.techInfo',         N'Thông tin công nghệ',                         N'Technology information'),
 (N'product.tech.name',             N'Tên công nghệ',                               N'Technology name'),
 (N'product.tech.code',             N'Mã công nghệ',                                N'Technology code'),
 (N'product.tech.tab.overview',     N'Tổng quan',                                   N'Overview'),
 (N'product.tech.tab.spec',         N'Thông số kỹ thuật',                           N'Technical specifications'),
 (N'product.tech.tab.application',  N'Ứng dụng',                                    N'Application'),
 (N'product.tech.tab.transfer',     N'Chuyển giao',                                 N'Transfer'),
 (N'product.tech.tab.docs',         N'Tài liệu',                                    N'Documents'),
 (N'product.tech.tab.review',       N'Đánh giá',                                    N'Reviews'),
 (N'product.tech.overviewEmpty',    N'Thông tin tổng quan đang được cập nhật.',     N'Overview information is being updated.'),
 (N'product.tech.specEmpty',        N'Thông số kỹ thuật đang được cập nhật.',       N'Technical specifications are being updated.'),
 (N'product.tech.advantages',       N'Ưu điểm nổi bật',                             N'Key advantages'),
 (N'product.tech.applicationTitle', N'Ứng dụng thực tế',                            N'Practical applications'),
 (N'product.tech.applicationEmpty', N'Thông tin ứng dụng đang được cập nhật.',      N'Application information is being updated.'),
 (N'product.tech.transferTitle',    N'Phương thức chuyển giao',                     N'Transfer method'),
 (N'product.tech.ndaLabel',         N'Thỏa thuận bảo mật',                          N'Confidentiality agreement (NDA)'),
 (N'product.tech.ndaMaybe',         N'Có thể cần thực hiện NDA nếu một trong các bên yêu cầu', N'An NDA may be required if either party requests it'),
 (N'product.tech.ndaContentTitle',  N'Nội dung thỏa thuận bảo mật',                 N'NDA content'),
 (N'product.tech.method',           N'Hình thức:',                                  N'Method:'),
 (N'product.tech.other',            N'Khác:',                                       N'Other:'),
 (N'product.tech.expectedPrice',    N'Giá bán dự kiến:',                            N'Expected price:'),
 (N'product.tech.extraCost',        N'Chi phí phát sinh:',                          N'Additional costs:'),
 (N'product.tech.warranty',         N'Bảo hành và hỗ trợ:',                         N'Warranty and support:'),
 (N'product.tech.transferEmpty',    N'Thông tin chuyển giao đang được cập nhật.',   N'Transfer information is being updated.'),
 (N'product.tech.docsTitle',        N'Tài liệu / chứng nhận',                       N'Documents / certifications'),
 (N'product.tech.watchVideo',       N'Xem video',                                  N'Watch video'),
 (N'product.tech.downloadDoc',      N'Tải tài liệu',                               N'Download document'),
 (N'product.tech.quatest',          N'Phiếu kiểm nghiệm Quatest',                   N'Quatest test report'),
 (N'product.tech.otherCert',        N'Chứng nhận khác',                             N'Other certification'),
 (N'product.tech.docsEmpty',        N'Tài liệu và chứng nhận đang được cập nhật.',  N'Documents and certifications are being updated.'),
 (N'product.tech.noReviews',        N'Chưa có đánh giá nào cho công nghệ này.',     N'No reviews yet for this technology.'),
 (N'product.tech.ndaWarning',       N'Các bên có thể yêu cầu thực hiện thỏa thuận bảo mật (NDA) trước khi trao đổi thông tin chi tiết.', N'The parties may request a non-disclosure agreement (NDA) before exchanging detailed information.');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
