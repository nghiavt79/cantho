SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(600), en NVARCHAR(600));
INSERT INTO @s VALUES
 (N'product.congnghe.title',    N'Công nghệ & Giải pháp ứng dụng', N'Technologies & Application Solutions'),
 (N'product.congnghe.subtitle', N'Khám phá các công nghệ, thiết bị và quy trình sẵn sàng chuyển giao, phục vụ doanh nghiệp, hợp tác xã và tổ chức tại Cần Thơ.', N'Explore technologies, equipment and processes ready for transfer, serving businesses, cooperatives and organizations in Can Tho.'),
 (N'product.congnghe.searchPh', N'Tìm kiếm công nghệ, thiết bị, giải pháp...', N'Search technologies, equipment, solutions...'),
 (N'product.congnghe.searchAria', N'Tìm kiếm công nghệ', N'Search technologies'),
 (N'product.congnghe.statsAria',  N'Thống kê công nghệ', N'Technology statistics'),
 (N'product.congnghe.statReady',  N'Công nghệ sẵn sàng chuyển giao', N'Technologies ready for transfer'),
 (N'product.congnghe.statFields', N'Lĩnh vực ứng dụng', N'Application fields'),
 (N'product.congnghe.statOrgs',   N'Đơn vị nghiên cứu & doanh nghiệp', N'Research units & businesses'),
 (N'product.congnghe.descFallback', N'Giải pháp công nghệ phục vụ sản xuất, vận hành và ứng dụng thực tế cho doanh nghiệp.', N'Technology solutions for production, operation and practical application for businesses.'),
 (N'product.congnghe.ctaKicker',  N'Kết nối chuyển giao', N'Transfer connection'),
 (N'product.congnghe.ctaTitle',   N'Bạn có công nghệ cần giới thiệu hoặc chuyển giao?', N'Do you have a technology to promote or transfer?'),
 (N'product.congnghe.ctaDesc',    N'Gửi thông tin để trung tâm hỗ trợ đánh giá, kết nối doanh nghiệp và đề xuất lộ trình thương mại hóa phù hợp.', N'Send us the details and the center will help assess, connect with businesses and propose a suitable commercialization roadmap.'),
 (N'product.congnghe.contactCenter', N'Liên hệ trung tâm', N'Contact the center');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted;
