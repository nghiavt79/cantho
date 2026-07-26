/*
    Push products with image to the top by moving image products forward.

    Ready to run on server:
      - @Apply = 1 updates immediately.
      - Only products with non-empty QuyTrinhHinhAnh are updated.
      - By default, only approved/public products StatusId = 3 are updated.
      - No-image products are not touched.
      - ProductType = 4 (OCOP/COOP) is excluded when the column exists.

    Result:
      Product lists ordered by PublishedDate DESC will show image products first.
*/

SET NOCOUNT ON;

DECLARE @Apply bit = 1;
DECLARE @OnlyApproved bit = 1;
DECLARE @BaseDate datetime = GETDATE();
DECLARE @ShowDetail bit = 0;

IF OBJECT_ID('tempdb..#ProductWithImage') IS NOT NULL DROP TABLE #ProductWithImage;

;WITH Products AS
(
    SELECT
        p.ID,
        p.Name,
        p.PublishedDate,
        NewPublishedDate = DATEADD(SECOND,
            -ROW_NUMBER() OVER
            (
                ORDER BY
                    ISNULL(p.PublishedDate, p.Created) DESC,
                    p.ID DESC
            ),
            @BaseDate
        )
    FROM dbo.SanPhamCNTB p
    WHERE NULLIF(LTRIM(RTRIM(ISNULL(p.QuyTrinhHinhAnh, ''))), '') IS NOT NULL
      AND (@OnlyApproved = 0 OR p.StatusId = 3)
)
SELECT *
INTO #ProductWithImage
FROM Products;

IF COL_LENGTH('dbo.SanPhamCNTB', 'ProductType') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
        DELETE x
        FROM #ProductWithImage x
        JOIN dbo.SanPhamCNTB p ON p.ID = x.ID
        WHERE p.ProductType = 4;
    ';
END

SELECT
    ProductsWithImage = COUNT(*),
    OldNewestPublishedDate = MAX(PublishedDate),
    OldOldestPublishedDate = MIN(PublishedDate),
    NewNewestPublishedDate = MAX(NewPublishedDate),
    NewOldestPublishedDate = MIN(NewPublishedDate)
FROM #ProductWithImage;

IF @ShowDetail = 1
BEGIN
    SELECT
        ID,
        Name,
        OldPublishedDate = PublishedDate,
        NewPublishedDate
    FROM #ProductWithImage
    ORDER BY NewPublishedDate DESC;
END

IF @Apply = 1
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE p
           SET p.PublishedDate = x.NewPublishedDate,
               p.Modified = GETDATE(),
               p.Modifier = N'push-image-products-first'
        FROM dbo.SanPhamCNTB p
        JOIN #ProductWithImage x ON x.ID = p.ID;

        COMMIT TRANSACTION;

        SELECT UpdatedProducts = COUNT(*) FROM #ProductWithImage;

        SELECT TOP 20
            p.ID,
            p.Name,
            p.PublishedDate,
            HasImage = CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(p.QuyTrinhHinhAnh, ''))), '') IS NULL THEN 0 ELSE 1 END
        FROM dbo.SanPhamCNTB p
        WHERE (@OnlyApproved = 0 OR p.StatusId = 3)
        ORDER BY p.PublishedDate DESC, p.ID DESC;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END
ELSE
BEGIN
    PRINT N'Preview only. Change @Apply = 1 to update image dbo.SanPhamCNTB.PublishedDate.';
END
