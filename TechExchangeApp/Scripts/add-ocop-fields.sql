/*
Adds fields needed for the OCOP showcase corner + traceability feature:
- SoSaoOCOP: OCOP star rating (1-5), only meaningful when ProductType = 4 (SanPhamOCOP)
- MaTruyXuat: public traceability code, used in the QR-linked lookup page
- QRCodeUrl: URL/path of the generated QR image for the product's traceability code
Guarded with IF NOT EXISTS so the script is safe to re-run.
*/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SanPhamCNTB' AND COLUMN_NAME = 'SoSaoOCOP')
BEGIN
    ALTER TABLE SanPhamCNTB ADD SoSaoOCOP INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SanPhamCNTB' AND COLUMN_NAME = 'MaTruyXuat')
BEGIN
    ALTER TABLE SanPhamCNTB ADD MaTruyXuat NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SanPhamCNTB' AND COLUMN_NAME = 'QRCodeUrl')
BEGIN
    ALTER TABLE SanPhamCNTB ADD QRCodeUrl NVARCHAR(500) NULL;
END
GO
