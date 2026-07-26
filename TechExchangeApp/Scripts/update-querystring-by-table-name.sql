/*
    Store: dbo.UpdateQueryStringByTableName

    Backfill QueryString for one supported table.
    Requires existing function: dbo.fnUrlSlug(@input).

    Usage:
        EXEC dbo.UpdateQueryStringByTableName @TableName = N'SanPhamCNTB', @DryRun = 1;
        EXEC dbo.UpdateQueryStringByTableName @TableName = N'SanPhamCNTB', @DryRun = 0;
        EXEC dbo.UpdateQueryStringByTableName @TableName = N'NhaCungUng', @DryRun = 0;

    Only updates rows where ISNULL(QueryString, '') = '' after trimming.
    Does not update SearchIndexContents.
*/

CREATE OR ALTER PROCEDURE dbo.UpdateQueryStringByTableName
    @TableName SYSNAME,
    @DryRun BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @TableName = LTRIM(RTRIM(ISNULL(@TableName, N'')));

    IF OBJECT_ID(N'dbo.fnUrlSlug', N'FN') IS NULL
    BEGIN
        RAISERROR('Missing required function dbo.fnUrlSlug.', 16, 1);
        RETURN;
    END;

    IF @TableName NOT IN (N'SanPhamCNTB', N'NhaCungUng')
    BEGIN
        RAISERROR('Invalid @TableName. Use SanPhamCNTB or NhaCungUng.', 16, 1);
        RETURN;
    END;

    IF @TableName = N'SanPhamCNTB'
    BEGIN
        SELECT
            p.ID,
            p.Name,
            p.QueryString AS OldQueryString,
            dbo.fnUrlSlug(p.Name) AS NewQueryString
        FROM dbo.SanPhamCNTB p
        WHERE NULLIF(LTRIM(RTRIM(ISNULL(p.QueryString, N''))), N'') IS NULL
          AND NULLIF(LTRIM(RTRIM(ISNULL(p.Name, N''))), N'') IS NOT NULL
        ORDER BY p.ID;

        IF @DryRun = 1
            RETURN;

        UPDATE p
        SET p.QueryString = dbo.fnUrlSlug(p.Name)
        FROM dbo.SanPhamCNTB p
        WHERE NULLIF(LTRIM(RTRIM(ISNULL(p.QueryString, N''))), N'') IS NULL
          AND NULLIF(LTRIM(RTRIM(ISNULL(p.Name, N''))), N'') IS NOT NULL;

        SELECT @@ROWCOUNT AS UpdatedRows;
        RETURN;
    END;

    IF @TableName = N'NhaCungUng'
    BEGIN
        SELECT
            n.CungUngId,
            n.FullName,
            n.QueryString AS OldQueryString,
            dbo.fnUrlSlug(n.FullName) AS NewQueryString
        FROM dbo.NhaCungUng n
        WHERE NULLIF(LTRIM(RTRIM(ISNULL(n.QueryString, N''))), N'') IS NULL
          AND NULLIF(LTRIM(RTRIM(ISNULL(n.FullName, N''))), N'') IS NOT NULL
        ORDER BY n.CungUngId;

        IF @DryRun = 1
            RETURN;

        UPDATE n
        SET n.QueryString = dbo.fnUrlSlug(n.FullName)
        FROM dbo.NhaCungUng n
        WHERE NULLIF(LTRIM(RTRIM(ISNULL(n.QueryString, N''))), N'') IS NULL
          AND NULLIF(LTRIM(RTRIM(ISNULL(n.FullName, N''))), N'') IS NOT NULL;

        SELECT @@ROWCOUNT AS UpdatedRows;
        RETURN;
    END;
END;
GO
