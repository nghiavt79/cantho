-- Seed i18n cho Đợt 5: đồng ý xử lý dữ liệu cá nhân, gọi video trong chat. Idempotent.
-- Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -I -i seed-i18n-dot5-hsmt.sql
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'auth.consent.prefix', N'Tôi đồng ý với',                                  N'I agree to the'),
 (N'auth.consent.policy', N'điều khoản xử lý dữ liệu cá nhân',                N'personal data processing terms'),
 (N'auth.consent.suffix', N'của Sàn giao dịch công nghệ.',                    N'of the Technology Exchange.'),
 (N'chat.video.button',   N'Gọi video',                                       N'Video call'),
 (N'chat.video.confirm',  N'Tạo phòng gọi video và gửi lời mời vào cuộc trò chuyện này?', N'Create a video call room and send the invitation to this conversation?');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-dot5-hsmt' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
