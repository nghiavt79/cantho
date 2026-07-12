/*
Adds bank transfer receiving info to NhaCungUng, so suppliers can enter their bank
account once in their profile and have it auto-shown to OCOP buyers who choose
"Chuyển khoản" as payment method instead of relying on a phone call.
Guarded with IF NOT EXISTS so the script is safe to re-run.
*/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NhaCungUng' AND COLUMN_NAME = 'SoTaiKhoan')
BEGIN
    ALTER TABLE NhaCungUng ADD SoTaiKhoan NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NhaCungUng' AND COLUMN_NAME = 'TenNganHang')
BEGIN
    ALTER TABLE NhaCungUng ADD TenNganHang NVARCHAR(200) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NhaCungUng' AND COLUMN_NAME = 'ChuTaiKhoan')
BEGIN
    ALTER TABLE NhaCungUng ADD ChuTaiKhoan NVARCHAR(200) NULL;
END
GO
