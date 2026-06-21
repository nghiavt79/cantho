/*
    Sync website data into AiKnowledgeDocuments.

    How to run:
        Use the approved internal DB connection profile to run this script after Create_AiChatSupport_Tables.sql.

    Notes:
    - This script does not delete old knowledge rows. It marks older dataset rows inactive.
    - Feedback/lead data is not included.
    - Re-run after importing/resetting website content.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @DatasetVersion NVARCHAR(100) = FORMAT(GETDATE(), 'yyyyMMdd-HHmmss');
DECLARE @JobId BIGINT;

INSERT INTO dbo.AiKnowledgeSyncJobs
(
    DatasetVersion,
    Status,
    StartedAt,
    CreatedAt
)
VALUES
(
    @DatasetVersion,
    N'Running',
    SYSDATETIME(),
    SYSDATETIME()
);

SET @JobId = SCOPE_IDENTITY();

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID('tempdb..#AiSource') IS NOT NULL
    BEGIN
        DROP TABLE #AiSource;
    END;

    CREATE TABLE #AiSource
    (
        SourceType NVARCHAR(100) NOT NULL,
        SourceId NVARCHAR(100) NOT NULL,
        SourceSlug NVARCHAR(500) NULL,
        Title NVARCHAR(500) NOT NULL,
        Url NVARCHAR(1000) NULL,
        ContentText NVARCHAR(MAX) NOT NULL,
        ContentHash NVARCHAR(100) NULL
    );

    INSERT INTO #AiSource (SourceType, SourceId, SourceSlug, Title, Url, ContentText, ContentHash)
    SELECT
        N'Content',
        CONVERT(NVARCHAR(100), c.Id),
        c.QueryString,
        LEFT(ISNULL(NULLIF(c.Title, N''), N'No title'), 500),
        CASE
            WHEN NULLIF(c.URL, N'') IS NOT NULL THEN c.URL
            WHEN c.QueryString IS NOT NULL THEN CONCAT(N'/', ISNULL(c.MenuId, 0), N'/', c.QueryString, N'-', c.Id)
            ELSE NULL
        END,
        CONCAT(
            ISNULL(c.Title, N''), CHAR(10),
            ISNULL(c.Description, N''), CHAR(10),
            ISNULL(c.Contents, N''), CHAR(10),
            ISNULL(c.Subject, N''), CHAR(10),
            ISNULL(c.Keyword, N'')
        ),
        CONVERT(NVARCHAR(100), HASHBYTES('SHA2_256', CONVERT(VARBINARY(8000), LEFT(CONCAT(
            ISNULL(c.Title, N''), CHAR(10),
            ISNULL(c.Description, N''), CHAR(10),
            ISNULL(c.Contents, N''), CHAR(10),
            ISNULL(c.Subject, N''), CHAR(10),
            ISNULL(c.Keyword, N'')
        ), 4000))), 2)
    FROM dbo.Contents c
    WHERE ISNULL(c.StatusId, 0) = 1
      AND (NULLIF(c.Title, N'') IS NOT NULL OR NULLIF(c.Description, N'') IS NOT NULL OR NULLIF(c.Contents, N'') IS NOT NULL);

    INSERT INTO #AiSource (SourceType, SourceId, SourceSlug, Title, Url, ContentText, ContentHash)
    SELECT
        CASE sp.ProductType
            WHEN 1 THEN N'CongNghe'
            WHEN 2 THEN N'ThietBi'
            WHEN 3 THEN N'TaiSanTriTue'
            ELSE N'SanPhamCNTB'
        END,
        CONVERT(NVARCHAR(100), sp.ID),
        sp.QueryString,
        LEFT(ISNULL(NULLIF(sp.Name, N''), N'San pham CNTB'), 500),
        CASE
            WHEN NULLIF(sp.URL, N'') IS NOT NULL THEN sp.URL
            ELSE CONCAT(N'/2-cong-nghe-thiet-bi/', ISNULL(sp.TypeId, sp.ProductType), N'/', ISNULL(NULLIF(sp.QueryString, N''), N'san-pham'), N'-', sp.ID)
        END,
        CONCAT(
            ISNULL(sp.Name, N''), CHAR(10),
            ISNULL(sp.MoTaNgan, N''), CHAR(10),
            ISNULL(sp.MoTa, N''), CHAR(10),
            ISNULL(sp.ThongSo, N''), CHAR(10),
            ISNULL(sp.UuDiem, N''), CHAR(10),
            ISNULL(sp.Keywords, N''), CHAR(10),
            ISNULL(sp.TransferMethod, N''), CHAR(10),
            ISNULL(sp.TargetCustomer, N''), CHAR(10),
            ISNULL(sp.CooperationGoal, N'')
        ),
        CONVERT(NVARCHAR(100), HASHBYTES('SHA2_256', CONVERT(VARBINARY(8000), LEFT(CONCAT(
            ISNULL(sp.Name, N''), CHAR(10),
            ISNULL(sp.MoTaNgan, N''), CHAR(10),
            ISNULL(sp.MoTa, N''), CHAR(10),
            ISNULL(sp.ThongSo, N''), CHAR(10),
            ISNULL(sp.UuDiem, N''), CHAR(10),
            ISNULL(sp.Keywords, N'')
        ), 4000))), 2)
    FROM dbo.SanPhamCNTB sp
    WHERE ISNULL(sp.StatusId, 0) = 1
      AND (NULLIF(sp.Name, N'') IS NOT NULL OR NULLIF(sp.MoTa, N'') IS NOT NULL OR NULLIF(sp.MoTaNgan, N'') IS NOT NULL);

    INSERT INTO #AiSource (SourceType, SourceId, SourceSlug, Title, Url, ContentText, ContentHash)
    SELECT
        N'NhaCungUng',
        CONVERT(NVARCHAR(100), ncu.CungUngId),
        ncu.QueryString,
        LEFT(ISNULL(NULLIF(ncu.FullName, N''), N'Nha cung ung'), 500),
        CONCAT(N'/11-tim-kiem-doi-tac/', ISNULL(NULLIF(ncu.QueryString, N''), N'nha-cung-ung'), N'-', ncu.CungUngId),
        CONCAT(
            ISNULL(ncu.FullName, N''), CHAR(10),
            ISNULL(ncu.ChucNangChinh, N''), CHAR(10),
            ISNULL(ncu.DichVu, N''), CHAR(10),
            ISNULL(ncu.SanPham, N''), CHAR(10),
            ISNULL(ncu.Keywords, N''), CHAR(10),
            ISNULL(ncu.DiaChi, N''), CHAR(10),
            ISNULL(ncu.Website, N'')
        ),
        CONVERT(NVARCHAR(100), HASHBYTES('SHA2_256', CONVERT(VARBINARY(8000), LEFT(CONCAT(
            ISNULL(ncu.FullName, N''), CHAR(10),
            ISNULL(ncu.ChucNangChinh, N''), CHAR(10),
            ISNULL(ncu.DichVu, N''), CHAR(10),
            ISNULL(ncu.SanPham, N''), CHAR(10),
            ISNULL(ncu.Keywords, N'')
        ), 4000))), 2)
    FROM dbo.NhaCungUng ncu
    WHERE ISNULL(ncu.StatusId, 0) = 1
      AND (NULLIF(ncu.FullName, N'') IS NOT NULL OR NULLIF(ncu.ChucNangChinh, N'') IS NOT NULL OR NULLIF(ncu.SanPham, N'') IS NOT NULL);

    INSERT INTO #AiSource (SourceType, SourceId, SourceSlug, Title, Url, ContentText, ContentHash)
    SELECT
        N'NhaTuVan',
        CONVERT(NVARCHAR(100), ntv.TuVanId),
        ntv.QueryString,
        LEFT(ISNULL(NULLIF(ntv.FullName, N''), N'Chuyen gia tu van'), 500),
        N'/chuyen-gia',
        CONCAT(
            ISNULL(ntv.FullName, N''), CHAR(10),
            ISNULL(ntv.HocHam, N''), CHAR(10),
            ISNULL(ntv.CoQuan, N''), CHAR(10),
            ISNULL(ntv.ChucVu, N''), CHAR(10),
            ISNULL(ntv.DichVu, N''), CHAR(10),
            ISNULL(ntv.KetQuaNghienCuu, N''), CHAR(10),
            ISNULL(ntv.KinhNghiem, N''), CHAR(10),
            ISNULL(ntv.Keywords, N'')
        ),
        CONVERT(NVARCHAR(100), HASHBYTES('SHA2_256', CONVERT(VARBINARY(8000), LEFT(CONCAT(
            ISNULL(ntv.FullName, N''), CHAR(10),
            ISNULL(ntv.HocHam, N''), CHAR(10),
            ISNULL(ntv.CoQuan, N''), CHAR(10),
            ISNULL(ntv.ChucVu, N''), CHAR(10),
            ISNULL(ntv.DichVu, N''), CHAR(10),
            ISNULL(ntv.KetQuaNghienCuu, N''), CHAR(10),
            ISNULL(ntv.KinhNghiem, N''), CHAR(10),
            ISNULL(ntv.Keywords, N'')
        ), 4000))), 2)
    FROM dbo.NhaTuVan ntv
    WHERE ISNULL(ntv.StatusId, 0) = 1
      AND (NULLIF(ntv.FullName, N'') IS NOT NULL OR NULLIF(ntv.DichVu, N'') IS NOT NULL OR NULLIF(ntv.KetQuaNghienCuu, N'') IS NOT NULL);

    UPDATE dbo.AiKnowledgeDocuments
    SET IsActive = 0
    WHERE IsActive = 1
      AND DatasetVersion <> @DatasetVersion;

    MERGE dbo.AiKnowledgeDocuments AS target
    USING #AiSource AS source
        ON target.SourceType = source.SourceType
       AND target.SourceId = source.SourceId
    WHEN MATCHED THEN
        UPDATE SET
            target.SourceSlug = source.SourceSlug,
            target.Title = source.Title,
            target.Url = source.Url,
            target.ContentText = source.ContentText,
            target.ContentHash = source.ContentHash,
            target.IsActive = 1,
            target.DatasetVersion = @DatasetVersion,
            target.LastSyncedAt = SYSDATETIME()
    WHEN NOT MATCHED BY TARGET THEN
        INSERT
        (
            SourceType,
            SourceId,
            SourceSlug,
            Title,
            Url,
            ContentText,
            ContentHash,
            IsActive,
            DatasetVersion,
            LastSyncedAt
        )
        VALUES
        (
            source.SourceType,
            source.SourceId,
            source.SourceSlug,
            source.Title,
            source.Url,
            source.ContentText,
            source.ContentHash,
            1,
            @DatasetVersion,
            SYSDATETIME()
        );

    UPDATE dbo.AiKnowledgeSyncJobs
    SET
        Status = N'Completed',
        TotalItems = (SELECT COUNT(*) FROM #AiSource),
        SuccessItems = (SELECT COUNT(*) FROM #AiSource),
        FailedItems = 0,
        CompletedAt = SYSDATETIME()
    WHERE Id = @JobId;

    COMMIT TRANSACTION;

    SELECT
        @DatasetVersion AS DatasetVersion,
        COUNT(*) AS TotalSyncedItems
    FROM #AiSource;

    SELECT
        SourceType,
        COUNT(*) AS TotalItems
    FROM dbo.AiKnowledgeDocuments
    WHERE DatasetVersion = @DatasetVersion
      AND IsActive = 1
    GROUP BY SourceType
    ORDER BY SourceType;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    UPDATE dbo.AiKnowledgeSyncJobs
    SET
        Status = N'Failed',
        ErrorMessage = ERROR_MESSAGE(),
        CompletedAt = SYSDATETIME()
    WHERE Id = @JobId;

    THROW;
END CATCH;
