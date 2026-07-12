/*
Adds payment method to OcopOrderRequests (COD vs bank transfer) and documents the
new "Đã huỷ" (cancelled) status value (StatusId = 4) used by order cancellation.
Guarded with IF NOT EXISTS so the script is safe to re-run.
*/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OcopOrderRequests' AND COLUMN_NAME = 'HinhThucThanhToan')
BEGIN
    ALTER TABLE OcopOrderRequests ADD HinhThucThanhToan INT NOT NULL DEFAULT 1;
END
GO
