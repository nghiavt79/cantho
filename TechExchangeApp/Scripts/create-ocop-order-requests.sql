/*
Creates the OcopOrderRequests table used by the standalone OCOP "Liên hệ đặt mua"
flow (simple retail order request), separate from the 14-step TechTransfer/Project
workflow used for Công nghệ/Sáng chế.
Guarded with IF NOT EXISTS so the script is safe to re-run.
*/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OcopOrderRequests')
BEGIN
    CREATE TABLE OcopOrderRequests
    (
        Id            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProductId     INT NOT NULL,
        SupplierId    INT NULL,
        HoTen         NVARCHAR(100) NOT NULL,
        DienThoai     NVARCHAR(20) NOT NULL,
        Email         NVARCHAR(100) NOT NULL,
        DiaChiGiao    NVARCHAR(300) NOT NULL,
        SoLuong       INT NOT NULL DEFAULT 1,
        GhiChu        NVARCHAR(MAX) NULL,
        StatusId      INT NOT NULL DEFAULT 1,
        NguoiTao      INT NULL,
        NgayTao       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        NguoiSua      INT NULL,
        NgaySua       DATETIME2 NULL
    );
END
GO
