-- Đợt 5 bổ sung theo HSMT: an toàn thông tin, bảo vệ dữ liệu cá nhân, gọi video.
-- Idempotent. Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -I -i dot5-attt-hsmt.sql
SET NOCOUNT ON;

-- ── 2.11: Đồng ý xử lý dữ liệu cá nhân khi đăng ký ─────────────────────────
IF COL_LENGTH('dbo.Users', 'ConsentAt') IS NULL
    ALTER TABLE dbo.Users ADD ConsentAt DATETIME NULL, ConsentVersion VARCHAR(20) NULL;
GO

-- ── Tham số ────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.SYS_PARAMETERS WHERE Name = 'CMS_MFA_ENABLED')
    INSERT INTO dbo.SYS_PARAMETERS(Name, Val, Description, IsSystem, Activated, Domain, LanguageId, ParentId)
    VALUES ('CMS_MFA_ENABLED', '0', N'1 = tài khoản quản trị phải nhập mã OTP gửi qua email khi vào CMS (cần cấu hình email trước khi bật)', 0, 1, 'BAO MAT', 1, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.SYS_PARAMETERS WHERE Name = 'PRIVACY_POLICY_URL')
    INSERT INTO dbo.SYS_PARAMETERS(Name, Val, Description, IsSystem, Activated, Domain, LanguageId, ParentId)
    VALUES ('PRIVACY_POLICY_URL', '', N'Đường dẫn trang chính sách bảo vệ dữ liệu cá nhân (vd /trang/chinh-sach-...-123); để trống thì ô đồng ý không có liên kết', 0, 1, 'BAO MAT', 1, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.SYS_PARAMETERS WHERE Name = 'PRIVACY_CONSENT_VERSION')
    INSERT INTO dbo.SYS_PARAMETERS(Name, Val, Description, IsSystem, Activated, Domain, LanguageId, ParentId)
    VALUES ('PRIVACY_CONSENT_VERSION', '2026-10', N'Phiên bản điều khoản xử lý dữ liệu cá nhân người dùng đồng ý khi đăng ký', 0, 1, 'BAO MAT', 1, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.SYS_PARAMETERS WHERE Name = 'VIDEO_CALL_BASE_URL')
    INSERT INTO dbo.SYS_PARAMETERS(Name, Val, Description, IsSystem, Activated, Domain, LanguageId, ParentId)
    VALUES ('VIDEO_CALL_BASE_URL', 'https://meet.jit.si/', N'Địa chỉ máy chủ họp trực tuyến (Jitsi) dùng cho nút Gọi video trong chat; nên thay bằng máy chủ Jitsi tự dựng', 0, 1, 'TUONG TAC', 1, 0);
GO

SELECT COL_LENGTH('dbo.Users','ConsentAt') ConsentCol,
       (SELECT COUNT(*) FROM dbo.SYS_PARAMETERS WHERE Name IN ('CMS_MFA_ENABLED','PRIVACY_POLICY_URL','PRIVACY_CONSENT_VERSION','VIDEO_CALL_BASE_URL')) Params;
