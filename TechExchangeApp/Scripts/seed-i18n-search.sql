-- Seed i18n cho trang Tìm kiếm (Views/Search/Index.cshtml + _SearchResults.cshtml). Idempotent.
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'search.pageTitleQuery',   N'Tìm kiếm:',                                     N'Search:'),
 (N'search.resultsFor',       N'Kết quả tìm kiếm:',                             N'Search results for:'),
 (N'search.noResults',        N'Chưa tìm thấy nội dung phù hợp với từ khóa này.', N'No matching content found for this keyword.'),
 (N'search.foundPrefix',      N'Tìm thấy',                                      N'Found'),
 (N'search.foundSuffix',      N'kết quả',                                       N'results'),
 (N'search.aiSuggestionsAnd', N'và',                                            N'and'),
 (N'search.aiSuggestionsSuffix', N'gợi ý AI',                                   N'AI suggestions'),
 (N'search.subtitlePrompt',   N'Nhập từ khóa để tìm công nghệ, thiết bị, nhà cung ứng…', N'Enter a keyword to find technologies, equipment, suppliers…'),
 (N'search.mainPlaceholder',  N'Nhập tên công nghệ, thiết bị, chuyên gia hoặc sản phẩm...', N'Enter a technology, equipment, expert or product name...'),
 (N'search.emptyPromptTitle', N'Vui lòng nhập từ khóa tìm kiếm',                N'Please enter a search keyword'),
 (N'search.modeNormal',       N'Tìm kiếm thông thường',                         N'Standard search'),
 (N'search.modeAi',           N'Tìm kiếm thông minh bằng AI',                   N'AI-powered search'),
 (N'search.matchPercent',     N'phù hợp',                                       N'match'),
 (N'search.matchingProducts', N'Sản phẩm phù hợp:',                             N'Matching products:'),
 (N'search.aiNoResults',      N'Không có kết quả AI',                           N'No AI results'),
 (N'search.aiNoResultsDesc',  N'Thử mô tả chi tiết hơn hoặc sử dụng tab "Tìm kiếm thông thường".', N'Try a more detailed description or use the "Standard search" tab.'),
 (N'search.showingPrefix',    N'Hiển thị',                                      N'Showing'),
 (N'search.showingIn',        N'trong',                                         N'of'),
 (N'search.resultsUnit',      N'kết quả',                                       N'results'),
 (N'search.emptyResultsTitle',N'Chưa tìm thấy nội dung phù hợp',                N'No matching content found'),
 (N'search.emptyResultsDesc', N'Thử sử dụng từ khóa ngắn hơn, bỏ bớt bộ lọc hoặc chọn một nhóm nội dung khác.', N'Try a shorter keyword, remove some filters, or choose a different category.'),
 (N'search.suggestionsLabel', N'Gợi ý:',                                        N'Suggestions:'),
 (N'search.suggest.plants',   N'cây trồng',                                     N'plants'),
 (N'search.suggest.agriculture', N'nông nghiệp',                                N'agriculture'),
 (N'search.suggest.biotech',  N'công nghệ sinh học',                            N'biotechnology'),
 (N'search.suggest.variety',  N'giống cây',                                     N'plant variety'),
 (N'search.ctaTitle',         N'Không tìm thấy công nghệ, thiết bị hoặc giải pháp phù hợp?', N'Can''t find the technology, equipment or solution you need?'),
 (N'search.ctaDesc',          N'Gửi nhu cầu tìm mua để Trung tâm hỗ trợ tiếp nhận, tìm kiếm nhà cung cấp và kết nối với đơn vị phù hợp.', N'Send a sourcing request and the Center will help receive it, find suppliers and connect you with a suitable provider.'),
 (N'search.ctaButton',        N'Gửi nhu cầu tìm mua',                           N'Send sourcing request');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
