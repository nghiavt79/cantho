/*
    Add direct inquiry flag for technology equipment/products.

    CanDirectInquiry = 1:
      Buyer may contact or submit a direct purchase/interest request.

    CanDirectInquiry = 0:
      Product should continue through quotation/transfer workflow when applicable.
*/

IF COL_LENGTH('dbo.SanPhamCNTB', 'CanDirectInquiry') IS NULL
BEGIN
    ALTER TABLE dbo.SanPhamCNTB
        ADD CanDirectInquiry BIT NULL
            CONSTRAINT DF_SanPhamCNTB_CanDirectInquiry DEFAULT (0);
END;
GO

/* Default existing equipment with visible price into direct inquiry unless it was already configured. */
UPDATE dbo.SanPhamCNTB
SET CanDirectInquiry = 1
WHERE ProductType = 2
  AND CanDirectInquiry IS NULL
  AND (
        SellPrice IS NOT NULL
        OR OriginalPrice IS NOT NULL
        OR NULLIF(LTRIM(RTRIM(ISNULL(GiaBanDuKien, N''))), N'') IS NOT NULL
      );
GO
