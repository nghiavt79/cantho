/*
    Add product-level NDA configuration.

    RequiresNDA: seller marks whether this product/technology requires an NDA.
    NDAContent: seller-defined NDA/confidentiality terms for this product.
*/

IF COL_LENGTH('dbo.SanPhamCNTB', 'RequiresNDA') IS NULL
BEGIN
    ALTER TABLE dbo.SanPhamCNTB
        ADD RequiresNDA BIT NULL CONSTRAINT DF_SanPhamCNTB_RequiresNDA DEFAULT (0);
END;

IF COL_LENGTH('dbo.SanPhamCNTB', 'NDAContent') IS NULL
BEGIN
    ALTER TABLE dbo.SanPhamCNTB
        ADD NDAContent NVARCHAR(MAX) NULL;
END;
