-- Triển khai mekongx.vn 25/10/2026 (áp dụng cho máy chủ đang chạy bản 22/10/2026)
-- Gộp: (1) tham số SMS Brandname South Telecom (gói 24/10, chưa chạy)
--       (2) bảng SiteVisitDaily – thống kê lượt truy cập toàn Sàn (add-site-visit-daily.sql)
-- Chạy: sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -i deploy-server-2026-10-25.sql
-- Idempotent: chạy lại nhiều lần an toàn; không ghi đè giá trị tham số đã nhập.
SET NOCOUNT ON;

/* ===== (1) THAM SỐ SMS SOUTH TELECOM ===== */

DECLARE @p TABLE (Name nvarchar(100), Val nvarchar(1000), Type int, Description nvarchar(400));
INSERT INTO @p VALUES
 (N'SMS_API_URL',         N'https://api-04.worldsms.vn/apidebit/sendSMS', NULL, N'Địa chỉ API gửi SMS của South Telecom'),
 (N'SMS_API_KEY',         N'',             5,    N'Khoá kết nối (AUTHORIZATION_KEY) South Telecom cấp, nhập nguyên chuỗi, không kèm chữ Basic'),
 (N'SMS_BRANDNAME',       N'',             NULL, N'Tên thương hiệu (brandname) đã đăng ký với South Telecom, phân biệt HOA/thường'),
 (N'SMS_DLR_ENABLED',     N'1',            3,    N'Yêu cầu South Telecom báo kết quả giao tin về /api/sms/dlr (1 = bật, 0 = tắt)'),
 (N'SMS_DLR_ALLOWED_IPS', N'123.30.142.10,123.30.142.15,123.30.142.16,123.30.142.20,123.30.142.22,125.212.193.103,125.212.193.104,125.212.193.109,210.211.110.83,210.211.110.84,210.211.110.85,113.161.113.129,113.161.113.131,113.161.113.132,113.161.113.133,113.161.38.201,123.30.245.27,123.30.245.40,123.30.146.10,222.255.42.14',
                                           NULL, N'Danh sách IP South Telecom được gọi báo kết quả giao tin, cách nhau dấu phẩy (* = không kiểm tra)');

INSERT INTO SYS_PARAMETERS (Name, Val, Val2, Type, Description, IsSystem, Activated, Domain, LanguageId, ParentId, SiteId)
SELECT p.Name, p.Val, NULL, p.Type, p.Description, NULL, 1, N'SMS', 1, 0, NULL
FROM @p p
WHERE NOT EXISTS (SELECT 1 FROM SYS_PARAMETERS s WHERE s.Name = p.Name);

UPDATE SYS_PARAMETERS
SET Description = N'Nhà cung cấp SMS: SOUTHTELECOM = gửi thật; STUB = chỉ ghi nhật ký, không gửi'
WHERE Name = N'SMS_PROVIDER';

UPDATE SYS_PARAMETERS SET Val = N'STUB' WHERE Name = N'SMS_PROVIDER' AND Val = N'TWILIO';

SELECT Name, CASE WHEN Name = N'SMS_API_KEY' THEN N'(ẩn)' ELSE LEFT(Val, 60) END AS Val, Activated
FROM SYS_PARAMETERS WHERE Domain = N'SMS' ORDER BY Name;

/* ===== (2) BẢNG THỐNG KÊ LƯỢT TRUY CẬP ===== */
IF OBJECT_ID('dbo.SiteVisitDaily', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SiteVisitDaily (
        VisitDate date NOT NULL CONSTRAINT PK_SiteVisitDaily PRIMARY KEY,
        PageViews int  NOT NULL CONSTRAINT DF_SiteVisitDaily_PageViews DEFAULT 0,
        Visitors  int  NOT NULL CONSTRAINT DF_SiteVisitDaily_Visitors DEFAULT 0
    );
    PRINT 'SiteVisitDaily created';
END
ELSE
    PRINT 'SiteVisitDaily already exists';
