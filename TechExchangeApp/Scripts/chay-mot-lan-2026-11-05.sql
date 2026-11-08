/* =====================================================================
   CẬP NHẬT MÁY CHỦ 05/11/2026 - CHẠY MỘT LẦN (đã gộp, CÓ XÓA DỮ LIỆU)
   Chỉ chạy sau khi đã sao lưu cơ sở dữ liệu. Chạy TRƯỚC khi chép TechExchangeApp.dll.
   Thứ tự:
     1) eContract: cột, bảng, tham số mới
     2) Bổ sung lĩnh vực sản phẩm; dữ liệu tìm kiếm thêm mô tả chi tiết
     3) Xóa sản phẩm thử "Quy trinh pentest" (sao lưu *_DeletedBackup_20261031)
     4) Dựng lại dữ liệu tìm kiếm
     5) Xóa nhà cung ứng, hồ sơ rác (sao lưu *_DeletedBackup_20261104)
   Dữ liệu VHT2- được giữ nguyên. Dùng -b để dừng ngay khi có lỗi.
   Chạy:  sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -i chay-mot-lan-2026-11-05.sql
   ===================================================================== */

GO
PRINT N'=== 1. eContract ===';
GO
/* ============================================================================
   Tích hợp eContract MobiFone - Giai đoạn 1 (docs/PLAN_EContract_USBToken.md).
   Idempotent script, no EF migration. Không sửa dữ liệu chữ ký cũ.
   PHẢI chạy trước khi deploy code có các cột mới của ProjectContracts.
   Chạy: sqlcmd -f 65001 -i add-econtract-columns.sql (file UTF-8 không BOM).
   ============================================================================ */

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---- ProjectContracts ---------------------------------------------------- */
IF COL_LENGTH('dbo.ProjectContracts', 'SigningProvider') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD SigningProvider NVARCHAR(30) NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'EContractEnv') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD EContractEnv NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'EContractId') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD EContractId BIGINT NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'EContractNo') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD EContractNo NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'EContractStatus') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD EContractStatus INT NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'IntegrationState') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD IntegrationState INT NOT NULL
        CONSTRAINT DF_ProjectContracts_IntegrationState DEFAULT (0);
IF COL_LENGTH('dbo.ProjectContracts', 'FrozenPdfPath') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD FrozenPdfPath NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'FrozenPdfSha256') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD FrozenPdfSha256 NVARCHAR(128) NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'EContractIssuedAt') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD EContractIssuedAt DATETIME2 NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'EContractSyncedAt') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD EContractSyncedAt DATETIME2 NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'EContractCompletedAt') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD EContractCompletedAt DATETIME2 NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_ProjectContracts_EContractId' AND object_id = OBJECT_ID('dbo.ProjectContracts'))
    CREATE UNIQUE INDEX UX_ProjectContracts_EContractId
        ON dbo.ProjectContracts (EContractEnv, EContractId)
        WHERE EContractId IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_ProjectContracts_EContractNo' AND object_id = OBJECT_ID('dbo.ProjectContracts'))
    CREATE UNIQUE INDEX UX_ProjectContracts_EContractNo
        ON dbo.ProjectContracts (EContractEnv, EContractNo)
        WHERE EContractNo IS NOT NULL;
GO

/* ---- EContractRecipients: người ký, snapshot lúc phát hành ------------------ */
IF OBJECT_ID('dbo.EContractRecipients', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EContractRecipients
    (
        Id                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EContractRecipients PRIMARY KEY,
        ProjectContractId INT NOT NULL
            CONSTRAINT FK_EContractRecipients_ProjectContracts REFERENCES dbo.ProjectContracts (Id),
        UserId            INT NULL,
        BusinessRole      INT NOT NULL,          -- 1=Buyer (Bên B), 2=Seller (Bên A)
        RecipientId       BIGINT NULL,           -- id phía eContract, chưa rõ response có trả không
        FullName          NVARCHAR(200) NOT NULL,
        Email             NVARCHAR(200) NULL,
        Phone             NVARCHAR(30) NULL,
        CardId            NVARCHAR(50) NULL,     -- MST/CCCD
        Ordering          INT NOT NULL CONSTRAINT DF_EContractRecipients_Ordering DEFAULT (1),
        Status            INT NULL,
        CreatedDate       DATETIME2 NOT NULL CONSTRAINT DF_EContractRecipients_CreatedDate DEFAULT (SYSUTCDATETIME()),
        ModifiedDate      DATETIME2 NULL
    );
    CREATE INDEX IX_EContractRecipients_Contract ON dbo.EContractRecipients (ProjectContractId);
    PRINT 'Created table EContractRecipients.';
END
GO

/* ---- EContractFields: vùng ký đã gửi lên eContract ------------------------- */
IF OBJECT_ID('dbo.EContractFields', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EContractFields
    (
        Id                   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EContractFields PRIMARY KEY,
        EContractRecipientId INT NOT NULL
            CONSTRAINT FK_EContractFields_Recipients REFERENCES dbo.EContractRecipients (Id),
        FieldId              BIGINT NULL,
        DocumentId           BIGINT NULL,
        FieldType            INT NOT NULL CONSTRAINT DF_EContractFields_FieldType DEFAULT (3),
        Page                 INT NOT NULL,
        CoordinateX          FLOAT NOT NULL,
        CoordinateY          FLOAT NOT NULL,     -- cộng dồn theo trang
        Width                FLOAT NOT NULL,
        Height               FLOAT NOT NULL
    );
    CREATE INDEX IX_EContractFields_Recipient ON dbo.EContractFields (EContractRecipientId);
    PRINT 'Created table EContractFields.';
END
GO

/* ---- EContractOperations: checkpoint chống tạo trùng và đối soát ------------ */
IF OBJECT_ID('dbo.EContractOperations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EContractOperations
    (
        Id                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EContractOperations PRIMARY KEY,
        OperationKey      NVARCHAR(200) NOT NULL,
        Action            NVARCHAR(50) NOT NULL,
        ProjectContractId INT NOT NULL
            CONSTRAINT FK_EContractOperations_ProjectContracts REFERENCES dbo.ProjectContracts (Id),
        InputSha256       NVARCHAR(128) NULL,
        State             INT NOT NULL CONSTRAINT DF_EContractOperations_State DEFAULT (0), -- 0=Pending,1=Succeeded,2=Failed,3=Unknown
        CorrelationId     NVARCHAR(64) NULL,
        Error             NVARCHAR(1000) NULL,   -- đã lược dữ liệu nhạy cảm
        LeaseUntil        DATETIME2 NULL,
        CreatedDate       DATETIME2 NOT NULL CONSTRAINT DF_EContractOperations_CreatedDate DEFAULT (SYSUTCDATETIME()),
        ModifiedDate      DATETIME2 NULL,
        CONSTRAINT UX_EContractOperations_Key UNIQUE (OperationKey)
    );
    CREATE INDEX IX_EContractOperations_Contract ON dbo.EContractOperations (ProjectContractId);
    PRINT 'Created table EContractOperations.';
END
GO

/* ---- SYS_PARAMETERS: chỉ thêm nếu chưa có, không ghi đè giá trị đã cấu hình -- */
DECLARE @p TABLE (Name VARCHAR(100), Val NVARCHAR(500), Descr NVARCHAR(500));
INSERT INTO @p (Name, Val, Descr) VALUES
('ECONTRACT_ENABLED',        '0', N'Bật gửi hợp đồng sang eContract MobiFone: 1 = bật, 0 = tắt'),
('ECONTRACT_ENV',            'PRODUCTION', N'Tên môi trường eContract, lưu kèm mã hợp đồng để đối soát'),
('ECONTRACT_BASE_URL',       'https://mobifone-econtract.vn/service', N'Địa chỉ eContract, không gồm /api/v1'),
('ECONTRACT_EMAIL',          '', N'Tài khoản tích hợp eContract (username trong mail tạo tài khoản)'),
('ECONTRACT_PASSWORD',       '', N'Mật khẩu tài khoản tích hợp eContract, lưu đã mã hóa qua /Secure/Encrypt'),
('ECONTRACT_TYPE_ID',        '', N'Mã Loại tài liệu trên eContract, để trống nếu không dùng'),
('ECONTRACT_ORG_ID',         '13592', N'Mã tổ chức của Trung tâm trên eContract'),
('ECONTRACT_DOWNLOAD_HOSTS', '', N'Các host được phép tải PDF đã ký ngoài host eContract, cách nhau dấu phẩy');

INSERT INTO dbo.SYS_PARAMETERS (Name, Val, [Description], Domain, Activated)
SELECT p.Name, p.Val, p.Descr, N'CONTRACT', 1
FROM @p p
WHERE NOT EXISTS (SELECT 1 FROM dbo.SYS_PARAMETERS s WHERE s.Name = p.Name);
GO

PRINT 'add-econtract-columns: done.';
GO
GO
PRINT N'=== 2. Linh vuc, tim kiem ===';
GO
/* =====================================================================
   Cập nhật máy chủ theo bản sửa lỗi L2 (báo cáo kiểm tra 03/11/2026). Chạy lại được nhiều lần.
   1) Bổ sung bảng lĩnh vực cho sản phẩm đã đăng mà chưa có (L2-09)
   2) Dữ liệu tra cứu sản phẩm thêm mô tả chi tiết (L2-09)
   3) Dựng lại dữ liệu tra cứu tiếng Việt, tiếng Anh
   Chạy:  sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -i deploy-server-2026-11-04.sql
   ===================================================================== */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
INSERT INTO dbo.SanPhamCNTBCategory (SanPhamCNTBId, CatId)
SELECT DISTINCT s.ID, k.CatId
FROM dbo.SanPhamCNTB s
CROSS APPLY STRING_SPLIT(REPLACE(ISNULL(s.CategoryId, ''), ',', ';'), ';') v
JOIN dbo.Category k ON k.CatId = TRY_CAST(LTRIM(RTRIM(v.value)) AS INT)
WHERE NOT EXISTS (SELECT 1 FROM dbo.SanPhamCNTBCategory c WHERE c.SanPhamCNTBId = s.ID);
PRINT N'Đã bổ sung lĩnh vực: ' + CAST(@@ROWCOUNT AS NVARCHAR(20));
GO
ALTER PROCEDURE [dbo].[uspReIndexSearchContents]
	@LanguageId int
AS
BEGIN
    SET NOCOUNT ON;

    -- Chỉ xoá index của ĐÚNG ngôn ngữ đang reindex (không đụng ngôn ngữ còn lại).
    DELETE FROM SearchIndexContents WHERE LanguageId = CAST(@LanguageId AS NVARCHAR(10));

    DECLARE @UrlPrefix NVARCHAR(20) = CASE WHEN @LanguageId = 2 THEN N'/en' ELSE N'' END;

    ;WITH ABC AS (
        -- ═══ SanPhamCNTB: tách theo ProductType ═══
        SELECT
            s.ID AS Id,
            REPLACE(S.Name, '-', ' ') AS Title,
            S.MoTaNgan AS [Description],
            s.QuyTrinhHinhAnh AS [Image],
            CASE WHEN s.ProductType = 4
                THEN @UrlPrefix + '/ocop/' + s.QueryString + '-' + CAST(s.ID AS NVARCHAR(50))
                ELSE @UrlPrefix + CASE WHEN @LanguageId = 2 THEN '/products/' ELSE '/san-pham/chi-tiet/' END
                     + s.QueryString + '-' + CAST(s.ID AS NVARCHAR(50))
            END AS URL,
            CASE s.ProductType
                WHEN 1 THEN N'Công nghệ'
                WHEN 2 THEN N'Thiết bị'
                WHEN 3 THEN N'Tài sản trí tuệ'
				WHEN 4 THEN N'OCOP'
            END AS TypeId,
            ISNULL(S.MoTaNgan,'') + ' ' + ISNULL(S.Thongso,'') + ' ' + ISNULL(S.UuDiem,'') + ' ' + ISNULL(CAST(S.MoTa AS NVARCHAR(MAX)),'') AS Contents,
            s.ID AS RefId
        FROM dbo.SanPhamCNTB S
        WHERE s.StatusId = 3 AND s.LanguageId = @LanguageId

        UNION ALL
        -- ═══ NhaCungUng → Nhà cung ứng ═══
        SELECT
            s.CungUngId, S.FullName, s.DiaChi,
            '' AS [Image],
            @UrlPrefix + CASE WHEN @LanguageId = 2 THEN '/suppliers/' ELSE '/nha-cung-ung/' END
                + s.QueryString + '-' + CAST(s.CungUngId AS NVARCHAR(50)),
            N'Nhà cung ứng',
            ISNULL(Phone,'') + ' ' + ISNULL(Email,'') + ' ' + ISNULL(Fax,'')
                + ' ' + ISNULL(Website,'') + ' ' + ISNULL(ChucNangChinh,''),
            s.CungUngId
        FROM dbo.NhaCungUng S
        WHERE s.LanguageId = @LanguageId AND s.IsActivated = 1

        UNION ALL
        -- ═══ NhaTuVan → Chuyên gia ═══
        SELECT
            s.TuVanId, S.FullName, s.HocHam,
            s.HinhDaiDien,
            @UrlPrefix + CASE WHEN @LanguageId = 2 THEN '/experts/' ELSE '/chuyen-gia/' END
                + s.QueryString + '-' + CAST(s.TuVanId AS NVARCHAR(50)),
            N'Chuyên gia',
            '',
            s.TuVanId
        FROM dbo.NhaTuVan S
        WHERE s.StatusId = 3 AND s.LanguageId = @LanguageId

        UNION ALL
        -- ═══ Contents → Tin bài ═══
        SELECT
            s.Id, S.Title, S.[Description],
            s.[Image],
            @UrlPrefix + CASE WHEN @LanguageId = 2 THEN '/news-event/' ELSE '/tin-tuc-su-kien/' END
                + s.QueryString + '-' + CAST(s.Id AS NVARCHAR(50)),
            N'Tin bài',
            s.Contents,
            s.Id
        FROM dbo.Contents S
        WHERE s.LanguageId = @LanguageId AND s.StatusId = 3
    )
    INSERT INTO SearchIndexContents (ImgPreview, Title, [Description], Contents, RefId, TypeName, URL, Created, Creator, LanguageId)
    SELECT A.[Image], A.Title, A.[Description], A.Contents, A.RefId, A.TypeId, A.URL, GETDATE(), 'sys', CAST(@LanguageId AS NVARCHAR(10))
    FROM ABC A
    LEFT JOIN SearchIndexContents B ON A.TypeId = B.TypeName AND A.RefId = B.RefId AND B.LanguageId = CAST(@LanguageId AS NVARCHAR(10))
    WHERE B.Id IS NULL;

    -- Handle nulls
    UPDATE SearchIndexContents SET [Description] = '' WHERE [Description] IS NULL;
    UPDATE SearchIndexContents SET Contents = '' WHERE Contents IS NULL;
END
GO
GO
PRINT N'=== 3. Xoa san pham thu ===';
GO
/* =====================================================================
   Xóa sản phẩm thử còn sót sau đợt pentest (vd. "QUY TRINH PENTEST", ID 44048)
   ---------------------------------------------------------------------
   Chọn sản phẩm cần xóa theo:
     - Tên có chữ "pentest", hoặc
     - Mô tả/thông số có mã thử tấn công (onerror=, <script, javascript:)
     - kèm bản tiếng Anh (OriginalId) của các sản phẩm đó.
   Chạy 2 lần:
     1) Để nguyên @ThucHien = 0  → chỉ LIỆT KÊ, không thay đổi gì. Đọc kỹ danh sách.
     2) Sửa @ThucHien = 1        → sao lưu rồi xóa trong 1 giao dịch.
   Sao lưu: bảng *_DeletedBackup_20261031 (khôi phục được).
   Chạy:  sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -i xoa-san-pham-pentest.sql
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @ThucHien BIT = 1;

IF OBJECT_ID('tempdb..#Xoa') IS NOT NULL DROP TABLE #Xoa;
CREATE TABLE #Xoa (ID INT PRIMARY KEY);

INSERT INTO #Xoa (ID)
SELECT ID FROM SanPhamCNTB
WHERE Name LIKE N'%pentest%'
   OR MoTa      LIKE N'%onerror=%' OR MoTa      LIKE N'%<script%' OR MoTa      LIKE N'%javascript:%'
   OR MoTaNgan  LIKE N'%onerror=%' OR MoTaNgan  LIKE N'%<script%' OR MoTaNgan  LIKE N'%javascript:%'
   OR ThongSo   LIKE N'%onerror=%' OR ThongSo   LIKE N'%<script%'
   OR UuDiem    LIKE N'%onerror=%' OR UuDiem    LIKE N'%<script%';

-- Bản tiếng Anh của các sản phẩm trên
INSERT INTO #Xoa (ID)
SELECT s.ID FROM SanPhamCNTB s
WHERE s.OriginalId IN (SELECT ID FROM #Xoa) AND s.ID NOT IN (SELECT ID FROM #Xoa);

PRINT N'== Sản phẩm sẽ xóa ==';
SELECT s.ID, s.LanguageId, s.OriginalId, s.Name, s.Created, s.Creator,
       LEFT(REPLACE(REPLACE(CAST(s.MoTa AS NVARCHAR(MAX)), CHAR(13), ' '), CHAR(10), ' '), 120) AS MoTa
FROM SanPhamCNTB s JOIN #Xoa x ON x.ID = s.ID
ORDER BY s.ID;

-- Sản phẩm có mô tả rác nhưng KHÔNG tự xóa (có thể là sản phẩm thật bị sửa mô tả) → sửa tay trong CMS
PRINT N'== Cần xem lại (không xóa): mô tả có chữ rác ==';
SELECT ID, Name, LEFT(CAST(MoTa AS NVARCHAR(MAX)), 120) AS MoTa
FROM SanPhamCNTB
WHERE (MoTa LIKE N'%abcafgedfh%' OR MoTaNgan LIKE N'%abcafgedfh%')
  AND ID NOT IN (SELECT ID FROM #Xoa);

-- Dữ liệu nghiệp vụ đang trỏ tới các sản phẩm này
PRINT N'== Dữ liệu liên quan ==';
IF OBJECT_ID('ChatConversations') IS NOT NULL
    SELECT N'ChatConversations (giữ hội thoại, bỏ liên kết sản phẩm)' AS Bang, COUNT(*) AS SoDong FROM ChatConversations WHERE ProductId IN (SELECT ID FROM #Xoa);
IF OBJECT_ID('TechTransferRequests') IS NOT NULL
    SELECT N'TechTransferRequests (giữ yêu cầu, bỏ liên kết)' AS Bang, COUNT(*) AS SoDong FROM TechTransferRequests WHERE TypeData = 1 AND FromId IN (SELECT ID FROM #Xoa);
IF OBJECT_ID('ShowroomSlots') IS NOT NULL
    SELECT N'ShowroomSlots (bỏ sản phẩm khỏi ô trưng bày)' AS Bang, COUNT(*) AS SoDong FROM ShowroomSlots WHERE SanPhamId IN (SELECT ID FROM #Xoa);
IF OBJECT_ID('OcopOrderRequests') IS NOT NULL
    SELECT N'OcopOrderRequests (có dòng → DỪNG, không xóa)' AS Bang, COUNT(*) AS SoDong FROM OcopOrderRequests WHERE ProductId IN (SELECT ID FROM #Xoa);

IF @ThucHien = 0
BEGIN
    PRINT N'Chỉ liệt kê (@ThucHien = 0). Kiểm tra danh sách rồi sửa @ThucHien = 1 để xóa.';
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM #Xoa)
BEGIN
    PRINT N'Không có sản phẩm nào cần xóa.';
    RETURN;
END

IF OBJECT_ID('OcopOrderRequests') IS NOT NULL
   AND EXISTS (SELECT 1 FROM OcopOrderRequests WHERE ProductId IN (SELECT ID FROM #Xoa))
BEGIN
    RAISERROR(N'Có đơn OCOP trỏ tới sản phẩm cần xóa — dừng, cần xem tay.', 16, 1);
    RETURN;
END

BEGIN TRAN;

-- 1. Sao lưu rồi xóa bảng con; sao lưu bảng sản phẩm (bảng nào không có trên máy chủ thì bỏ qua)
DECLARE @Bang TABLE (STT INT IDENTITY, Bang SYSNAME, Cot SYSNAME, Xoa BIT);
INSERT INTO @Bang (Bang, Cot, Xoa) VALUES
 ('SanPhamCNTB','ID',0),
 ('SanPhamCNTBCategory','SanPhamCNTBId',1), ('SanPhamCNTBMedia','SanPhamCNTBId',1),
 ('SanPhamCNTBPrice','SanPhamCNTBId',1),    ('SanPhamCNTBIP','SanPhamCNTBId',1),
 ('SanPhamEmbeddings','SanPhamId',1),       ('SanPhamCNTBVersions','SanPhamId',1),
 ('ProductFavorites','SanPhamId',1),        ('ProductWarnings','SanPhamId',1),
 ('SanPhamKhachHang','SanPhamId',1);
-- TimKiemDoiTac.SanPhamId là chuỗi danh sách mã ("32;45;"), không phải khóa → không xử lý

DECLARE @i INT = 1, @n INT = (SELECT COUNT(*) FROM @Bang), @b SYSNAME, @c SYSNAME, @x BIT,
        @bk SYSNAME, @cols NVARCHAR(MAX), @sql NVARCHAR(MAX), @dong INT;
WHILE @i <= @n
BEGIN
    SELECT @b = Bang, @c = Cot, @x = Xoa FROM @Bang WHERE STT = @i;
    SET @i += 1;
    IF OBJECT_ID(@b) IS NULL OR COL_LENGTH(@b, @c) IS NULL CONTINUE;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(@b) AND name = @c
                   AND TYPE_NAME(system_type_id) IN ('int','bigint')) CONTINUE;   -- chỉ cột mã số

    SET @bk = @b + N'_DeletedBackup_20261031';
    IF OBJECT_ID(@bk) IS NULL
        SET @sql = N'SELECT * INTO ' + QUOTENAME(@bk) + N' FROM ' + QUOTENAME(@b)
                 + N' WHERE ' + QUOTENAME(@c) + N' IN (SELECT ID FROM #Xoa);';
    ELSE
    BEGIN   -- chạy lại lần sau: chèn thêm vào bảng sao lưu đã có
        SET @cols = STUFF((SELECT ',' + QUOTENAME(name) FROM sys.columns
                           WHERE object_id = OBJECT_ID(@b) AND is_computed = 0 AND system_type_id <> 189
                           ORDER BY column_id FOR XML PATH('')), 1, 1, '');
        SET @sql = N'INSERT INTO ' + QUOTENAME(@bk) + N' (' + @cols + N') SELECT ' + @cols + N' FROM ' + QUOTENAME(@b)
                 + N' WHERE ' + QUOTENAME(@c) + N' IN (SELECT ID FROM #Xoa);';
        IF OBJECTPROPERTY(OBJECT_ID(@bk), 'TableHasIdentity') = 1
            SET @sql = N'SET IDENTITY_INSERT ' + QUOTENAME(@bk) + N' ON; ' + @sql
                     + N' SET IDENTITY_INSERT ' + QUOTENAME(@bk) + N' OFF;';
    END
    EXEC (@sql);

    IF @x = 1
    BEGIN
        SET @sql = N'DELETE FROM ' + QUOTENAME(@b) + N' WHERE ' + QUOTENAME(@c) + N' IN (SELECT ID FROM #Xoa); SET @d = @@ROWCOUNT;';
        EXEC sp_executesql @sql, N'@d INT OUTPUT', @d = @dong OUTPUT;
        PRINT @b + N': xóa ' + CAST(@dong AS NVARCHAR(10)) + N' dòng';
    END
END

-- 2. Dữ liệu nghiệp vụ: giữ lại, chỉ bỏ liên kết tới sản phẩm đã xóa
IF OBJECT_ID('ChatConversations') IS NOT NULL
    UPDATE ChatConversations SET ProductId = NULL WHERE ProductId IN (SELECT ID FROM #Xoa);
IF OBJECT_ID('TechTransferRequests') IS NOT NULL
    UPDATE TechTransferRequests SET FromId = NULL WHERE TypeData = 1 AND FromId IN (SELECT ID FROM #Xoa);
IF OBJECT_ID('ShowroomSlots') IS NOT NULL
    UPDATE ShowroomSlots SET SanPhamId = NULL WHERE SanPhamId IN (SELECT ID FROM #Xoa);

-- 3. Xóa sản phẩm (bản tiếng Anh trước, bản gốc sau)
DELETE FROM SanPhamCNTB WHERE ID IN (SELECT ID FROM #Xoa) AND OriginalId IS NOT NULL;
DELETE FROM SanPhamCNTB WHERE ID IN (SELECT ID FROM #Xoa);
PRINT N'SanPhamCNTB: xóa ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng (gốc)';

COMMIT;
PRINT N'Hoàn tất. Sao lưu ở các bảng *_DeletedBackup_20261031.';
GO
PRINT N'=== 4. Dung lai du lieu tim kiem ===';
GO
EXEC dbo.uspReIndexSearchContents @LanguageId = 1;
EXEC dbo.uspReIndexSearchContents @LanguageId = 2;
GO
GO
PRINT N'=== 5. Xoa nha cung ung, ho so rac ===';
GO
/* =====================================================================
   Dọn dữ liệu không hợp lệ theo L2-01, L2-03 (báo cáo kiểm tra 03/11/2026)
   ---------------------------------------------------------------------
   Chọn:
     - Nhà cung ứng tên "pHqghUme..." hoặc số 555-666-0606, không gắn sản phẩm, gói gian hàng, thu chi, không thuộc tài khoản vht_*.
       (Hồ sơ 17924 gắn sản phẩm VHT2 nên không bị chọn: sửa thông tin trong CMS.)
     - Hồ sơ giao dịch có tên chứa pHqghUme, chuỗi mã thử hoặc chữ pentest, hoặc yêu cầu chuyển giao
       ghi số 555-666-0606; không gồm hồ sơ thử VHT2 và hồ sơ đã có hồ sơ đề xuất, hợp đồng.
   Sản phẩm "Quy trinh pentest": dùng script xoa-san-pham-pentest.sql.
   Chạy 2 lần:
     1) Để nguyên @ThucHien = 0  → chỉ LIỆT KÊ, không thay đổi gì. Đọc kỹ số lượng.
     2) Sửa @ThucHien = 1        → sao lưu rồi xóa trong 1 giao dịch.
   Sao lưu: NhaCungUng_DeletedBackup_20261104, Projects_DeletedBackup_20261104.
   Chạy:  sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -i xoa-du-lieu-ra-quet-2026-11.sql
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @ThucHien BIT = 1;

IF OBJECT_ID('tempdb..#Ncc') IS NOT NULL DROP TABLE #Ncc;
IF OBJECT_ID('tempdb..#Prj') IS NOT NULL DROP TABLE #Prj;
CREATE TABLE #Ncc (Id INT PRIMARY KEY);
CREATE TABLE #Prj (Id INT PRIMARY KEY);

INSERT INTO #Ncc (Id)
SELECT n.CungUngId FROM NhaCungUng n
WHERE (n.FullName LIKE N'pHqghUme%' OR n.Phone = '555-666-0606')
  AND NOT EXISTS (SELECT 1 FROM SanPhamCNTB s WHERE s.NCUId = n.CungUngId)
  AND NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = n.UserId AND u.UserName LIKE 'vht[_]%')
  AND NOT EXISTS (SELECT 1 FROM SupplierSubscriptions x WHERE x.CungUngId = n.CungUngId)
  AND NOT EXISTS (SELECT 1 FROM FinanceEntries x WHERE x.CungUngId = n.CungUngId);

INSERT INTO #Prj (Id)
SELECT p.Id FROM Projects p
WHERE (p.ProjectName LIKE N'%pHqghUme%' OR p.ProjectName LIKE N'%<img src=%' OR p.ProjectName LIKE N'%pentest%'
       OR EXISTS (SELECT 1 FROM TechTransferRequests t WHERE t.ProjectId = p.Id AND t.DienThoai = '555-666-0606'))
  AND p.ProjectName NOT LIKE N'%VHT2%'
  AND NOT EXISTS (SELECT 1 FROM ProposalSubmissions x WHERE x.ProjectId = p.Id)
  AND NOT EXISTS (SELECT 1 FROM ProjectContracts x WHERE x.ProjectId = p.Id)
  AND NOT EXISTS (SELECT 1 FROM EContracts x WHERE x.ProjectId = p.Id);

SELECT (SELECT COUNT(*) FROM NhaCungUng) AS TongNhaCungUng, (SELECT COUNT(*) FROM #Ncc) AS NhaCungUngSeXoa,
       (SELECT COUNT(*) FROM Projects) AS TongHoSo, (SELECT COUNT(*) FROM #Prj) AS HoSoSeXoa;
SELECT TOP 20 n.CungUngId, n.FullName, n.Phone, n.Email FROM NhaCungUng n JOIN #Ncc x ON x.Id = n.CungUngId ORDER BY n.CungUngId;
SELECT TOP 20 p.Id, p.ProjectCode, LEFT(p.ProjectName, 100) AS ProjectName, p.CreatedDate FROM Projects p JOIN #Prj x ON x.Id = p.Id ORDER BY p.Id;

IF @ThucHien = 0
BEGIN
    PRINT N'Chỉ liệt kê. Sửa @ThucHien = 1 để sao lưu và xóa.';
    RETURN;
END

BEGIN TRAN;

IF OBJECT_ID('NhaCungUng_DeletedBackup_20261104') IS NULL
    SELECT * INTO NhaCungUng_DeletedBackup_20261104 FROM NhaCungUng WHERE 1 = 0;
IF OBJECT_ID('Projects_DeletedBackup_20261104') IS NULL
    SELECT * INTO Projects_DeletedBackup_20261104 FROM Projects WHERE 1 = 0;

DECLARE @Sql NVARCHAR(MAX);

-- Sao lưu (bảng sao lưu có cột IDENTITY nên phải liệt kê cột)
SELECT @Sql = N'SET IDENTITY_INSERT Projects_DeletedBackup_20261104 ON;
INSERT INTO Projects_DeletedBackup_20261104 (' + STRING_AGG(QUOTENAME(name), ',') + N')
SELECT ' + STRING_AGG(QUOTENAME(name), ',') + N' FROM Projects WHERE Id IN (SELECT Id FROM #Prj);
SET IDENTITY_INSERT Projects_DeletedBackup_20261104 OFF;'
FROM sys.columns WHERE object_id = OBJECT_ID('Projects');
EXEC sp_executesql @Sql;

SELECT @Sql = N'SET IDENTITY_INSERT NhaCungUng_DeletedBackup_20261104 ON;
INSERT INTO NhaCungUng_DeletedBackup_20261104 (' + STRING_AGG(QUOTENAME(name), ',') + N')
SELECT ' + STRING_AGG(QUOTENAME(name), ',') + N' FROM NhaCungUng WHERE CungUngId IN (SELECT Id FROM #Ncc);
SET IDENTITY_INSERT NhaCungUng_DeletedBackup_20261104 OFF;'
FROM sys.columns WHERE object_id = OBJECT_ID('NhaCungUng');
EXEC sp_executesql @Sql;

-- Bảng con có khóa ngoại tới bảng cũng có ProjectId: phải xóa trước
DELETE FROM SupportRequests WHERE ProjectId IN (SELECT Id FROM #Prj)
   OR ConversationId IN (SELECT Id FROM ChatConversations WHERE ProjectId IN (SELECT Id FROM #Prj));
DELETE FROM RFQInvitations WHERE ProjectId IN (SELECT Id FROM #Prj);

-- Xóa dữ liệu con ở mọi bảng có cột ProjectId (trừ bảng sao lưu), rồi xóa hồ sơ
SELECT @Sql = STRING_AGG(CAST(N'DELETE FROM ' AS NVARCHAR(MAX)) + QUOTENAME(t.name) + N' WHERE ProjectId IN (SELECT Id FROM #Prj);', CHAR(10))
FROM sys.tables t JOIN sys.columns c ON c.object_id = t.object_id
WHERE c.name = 'ProjectId' AND t.name NOT LIKE '%Backup%';
EXEC sp_executesql @Sql;

DELETE FROM Projects WHERE Id IN (SELECT Id FROM #Prj);
DELETE FROM NhaCungUng WHERE CungUngId IN (SELECT Id FROM #Ncc);

COMMIT;
PRINT N'Đã sao lưu và xóa.';
GO
PRINT N'=== HOAN THANH ===';
GO
