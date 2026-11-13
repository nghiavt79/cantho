/*
Thêm cột ghi kết quả gửi mã OTP vào bảng UserOtps (xác thực số điện thoại, 11/2026).
  SendStatus       : NULL = chưa gửi, 1 = gửi thành công, 2 = gửi lỗi
  SendError        : nội dung lỗi khi gửi SMS (ví dụ "South Telecom lỗi 51: IP máy chủ chưa được cho phép")
  ProviderResponse : phản hồi của nhà cung cấp SMS khi gửi thành công
Chạy TRƯỚC khi deploy code mới. An toàn, chạy lại được (idempotent).

Xem lỗi gửi OTP gần nhất:
  SELECT TOP 20 o.Id, o.UserId, u.UserName, u.Mobile, o.CreatedAt, o.SendStatus, o.SendError, o.ProviderResponse
  FROM UserOtps o JOIN Users u ON u.UserId = o.UserId
  WHERE o.OtpType = 2 ORDER BY o.CreatedAt DESC;
*/
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.UserOtps') AND name = 'SendStatus')
BEGIN
    ALTER TABLE dbo.UserOtps ADD SendStatus int NULL;
    PRINT 'UserOtps.SendStatus added';
END
ELSE
    PRINT 'UserOtps.SendStatus already exists';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.UserOtps') AND name = 'SendError')
BEGIN
    ALTER TABLE dbo.UserOtps ADD SendError nvarchar(1000) NULL;
    PRINT 'UserOtps.SendError added';
END
ELSE
    PRINT 'UserOtps.SendError already exists';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.UserOtps') AND name = 'ProviderResponse')
BEGIN
    ALTER TABLE dbo.UserOtps ADD ProviderResponse nvarchar(1000) NULL;
    PRINT 'UserOtps.ProviderResponse added';
END
ELSE
    PRINT 'UserOtps.ProviderResponse already exists';
