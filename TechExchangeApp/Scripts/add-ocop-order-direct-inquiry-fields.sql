/*
Adds shared direct-inquiry fields to OcopOrderRequests so the same request flow
can serve OCOP products and direct-purchase equipment.
*/

IF COL_LENGTH('dbo.OcopOrderRequests', 'RequestProductType') IS NULL
BEGIN
    ALTER TABLE dbo.OcopOrderRequests
    ADD RequestProductType INT NULL;
END;
GO

IF COL_LENGTH('dbo.OcopOrderRequests', 'GiaThamKhao') IS NULL
BEGIN
    ALTER TABLE dbo.OcopOrderRequests
    ADD GiaThamKhao NVARCHAR(MAX) NULL;
END;
GO

UPDATE o
SET RequestProductType = p.ProductType
FROM dbo.OcopOrderRequests o
INNER JOIN dbo.SanPhamCNTB p ON p.ID = o.ProductId
WHERE o.RequestProductType IS NULL;
GO

UPDATE o
SET GiaThamKhao = NULLIF(LTRIM(RTRIM(p.GiaBanDuKien)), N'')
FROM dbo.OcopOrderRequests o
INNER JOIN dbo.SanPhamCNTB p ON p.ID = o.ProductId
WHERE o.GiaThamKhao IS NULL
  AND NULLIF(LTRIM(RTRIM(p.GiaBanDuKien)), N'') IS NOT NULL;
GO
