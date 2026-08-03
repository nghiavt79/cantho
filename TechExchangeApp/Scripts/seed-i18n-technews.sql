SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'common.cancel',            N'Hủy',                                          N'Cancel'),
 (N'common.close',             N'Đóng',                                         N'Close'),
 (N'common.sortNewest',        N'Mới nhất',                                     N'Newest'),
 (N'common.sortOldest',        N'Cũ nhất',                                      N'Oldest'),
 (N'common.sort',              N'Sắp xếp:',                                     N'Sort:'),
 (N'technews.pageTitle',       N'Tìm mua công nghệ',                            N'Sourcing Technology'),
 (N'technews.formTitle',       N'TÌM MUA CÔNG NGHỆ',                            N'SOURCING TECHNOLOGY'),
 (N'technews.formIntro',       N'Vui lòng mô tả nhu cầu tìm mua công nghệ, thiết bị hoặc giải pháp để Sàn tiếp nhận và hỗ trợ kết nối.', N'Please describe your need to source technology, equipment or a solution so the Exchange can receive it and help connect you.'),
 (N'technews.breadcrumb',      N'Tìm mua công nghệ',                            N'Sourcing Technology'),
 (N'technews.heroTitleLine1',  N'Tìm mua máy móc, công nghệ',                   N'Source machinery, technology'),
 (N'technews.heroTitleLine2',  N'và dây chuyền sản xuất',                       N'and production lines'),
 (N'technews.heroSubtitle',    N'Kết nối nhu cầu của doanh nghiệp với các đơn vị cung cấp giải pháp công nghệ phù hợp.', N'Connecting business needs with suppliers of suitable technology solutions.'),
 (N'technews.postNeed',        N'Đăng nhu cầu tìm mua',                         N'Post a sourcing request'),
 (N'technews.searchKeyword',   N'Từ khóa tìm kiếm',                             N'Search keyword'),
 (N'technews.searchPh',        N'Nhập tên công nghệ, máy móc, thiết bị hoặc giải pháp cần tìm...', N'Enter the technology, machinery, equipment or solution name...'),
 (N'technews.field',           N'Lĩnh vực công nghệ',                           N'Technology field'),
 (N'technews.chooseField',     N'--- Chọn lĩnh vực ---',                        N'--- Select a field ---'),
 (N'technews.popularKeywords', N'Từ khóa phổ biến:',                            N'Popular keywords:'),
 (N'technews.kw1',             N'Dây chuyền chế biến',                          N'Processing lines'),
 (N'technews.kw2',             N'Máy đóng gói',                                 N'Packaging machines'),
 (N'technews.kw3',             N'Tự động hóa',                                  N'Automation'),
 (N'technews.kw4',             N'Năng lượng',                                   N'Energy'),
 (N'technews.kw5',             N'Công nghệ sinh học',                           N'Biotechnology'),
 (N'technews.resultsPrefix',   N'Có',                                           N'Found'),
 (N'technews.resultsSuffix',   N'nhu cầu phù hợp',                              N'matching requests'),
 (N'technews.empty',           N'Không tìm thấy nhu cầu phù hợp. Vui lòng thử từ khóa hoặc lĩnh vực khác.', N'No matching requests found. Please try a different keyword or field.'),
 (N'technews.justPosted',      N'vừa đăng',                                     N'just posted'),
 (N'technews.minutesAgo',      N'{0} phút trước',                               N'{0} minutes ago'),
 (N'technews.hoursAgo',        N'{0} giờ trước',                                N'{0} hours ago'),
 (N'technews.daysAgo',         N'{0} ngày trước',                               N'{0} days ago'),
 (N'technews.ctaEndTitle',     N'Không tìm thấy công nghệ phù hợp với nhu cầu?', N'Can''t find the technology you need?'),
 (N'technews.ctaEndDesc',      N'Gửi yêu cầu của bạn, chúng tôi sẽ kết nối và hỗ trợ tìm kiếm giải pháp phù hợp nhất.', N'Send us your request and we will help connect you with the most suitable solution.'),
 (N'technews.ctaEndBtn',       N'Gửi nhu cầu tìm mua ngay',                     N'Send sourcing request now'),
 (N'technews.modalTitle',      N'Gửi nhu cầu tìm mua công nghệ',                N'Submit a technology sourcing request'),
 (N'technews.modalSubtitle',   N'Cung cấp thông tin về máy móc, thiết bị hoặc giải pháp bạn đang cần. CASTA sẽ hỗ trợ kết nối với đơn vị phù hợp.', N'Provide details about the machinery, equipment or solution you need. CASTA will help connect you with a suitable provider.'),
 (N'technews.successTitle',    N'Đã tiếp nhận nhu cầu của bạn',                 N'Your request has been received'),
 (N'technews.successDesc',     N'CASTA sẽ kiểm tra thông tin và liên hệ hỗ trợ trong thời gian sớm nhất.', N'CASTA will review your information and contact you as soon as possible.'),
 (N'technews.form.fullName',   N'Họ và tên',                                    N'Full name'),
 (N'technews.form.fullNamePh', N'Nhập họ và tên',                               N'Enter your full name'),
 (N'technews.form.phone',      N'Số điện thoại',                                N'Phone number'),
 (N'technews.form.phonePh',    N'Nhập số điện thoại',                           N'Enter your phone number'),
 (N'technews.form.emailPh',    N'Nhập email',                                   N'Enter your email'),
 (N'technews.form.captcha',    N'Xác thực',                                     N'Verification'),
 (N'technews.form.captchaPh',  N'Kết quả',                                      N'Result'),
 (N'technews.form.desc',       N'Mô tả nhu cầu',                                N'Description of your need'),
 (N'technews.form.descPh',     N'Mô tả máy móc, thiết bị hoặc giải pháp bạn đang cần...', N'Describe the machinery, equipment or solution you need...'),
 (N'technews.form.note',       N'Thông tin chỉ dùng để hỗ trợ kết nối, không chia sẻ cho bên thứ ba.', N'This information is only used to support connections and is not shared with third parties.'),
 (N'technews.form.submit',     N'Gửi yêu cầu',                                  N'Send request');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;

-- Bổ sung: chuỗi JS (loading/error) trong script block của TechNeedsByMenu.cshtml
DECLARE @s2 TABLE(k NVARCHAR(250), vi NVARCHAR(300), en NVARCHAR(300));
INSERT INTO @s2 VALUES
 (N'technews.js.sending',      N'Đang gửi...',                                  N'Sending...'),
 (N'technews.js.errorGeneric', N'Gửi thất bại, vui lòng thử lại.',              N'Submission failed, please try again.'),
 (N'technews.js.errorNetwork', N'Gửi thất bại, vui lòng kiểm tra kết nối và thử lại.', N'Submission failed, please check your connection and try again.');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s2.k,s2.vi,s2.en,'i18n-page-seed' FROM @s2 s2 WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s2.k);

-- Bổ sung: nút "Đọc thêm" trên card tin tức (News/Category.cshtml)
IF NOT EXISTS(SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'news.readMore')
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES(N'news.readMore',N'Đọc thêm',N'Read more','i18n-page-seed');
