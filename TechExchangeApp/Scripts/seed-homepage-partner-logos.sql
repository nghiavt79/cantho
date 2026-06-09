SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @Now DATETIME = GETDATE();

DECLARE @Partners TABLE
(
    Title NVARCHAR(255) NOT NULL,
    SRC NVARCHAR(500) NOT NULL,
    URL NVARCHAR(500) NULL,
    Sort INT NOT NULL
);

INSERT INTO @Partners (Title, SRC, URL, Sort)
VALUES
(N'S.Light Ora', N'/image/logo-doi-tac/01-slight-ora.jpg', NULL, 1),
(N'Viện Khoa học và Kỹ thuật Cần Thơ', N'/image/logo-doi-tac/02-vien-khkt-can-tho.png', NULL, 2),
(N'Pharos Marine', N'/image/logo-doi-tac/03-pharos-marine.png', NULL, 3),
(N'Design24', N'/image/logo-doi-tac/04-design24.webp', NULL, 4),
(N'FPT Polytechnic Cần Thơ', N'/image/logo-doi-tac/05-fpt-polytechnic-can-tho.png', NULL, 5),
(N'Phù Sa Genomics', N'/image/logo-doi-tac/06-phu-sa-genomics.png', NULL, 6),
(N'Bệnh viện Đại học Nam Cần Thơ', N'/image/logo-doi-tac/07-benh-vien-dai-hoc-nam-can-tho.png', NULL, 7),
(N'Hygie & Panacee', N'/image/logo-doi-tac/08-hygie-panacee.png', NULL, 8);

UPDATE ImagesAdver
SET StatusID = 2,
    Modified = @Now,
    Modifier = N'seed-homepage-partner-logos'
WHERE Subject = 5
  AND StatusID = 3
  AND SRC NOT IN (SELECT SRC FROM @Partners);

MERGE ImagesAdver AS target
USING @Partners AS source
ON target.Subject = 5
   AND target.LanguageID = 1
   AND target.SiteId = 1
   AND target.SRC = source.SRC
WHEN MATCHED THEN
    UPDATE SET
        target.Title = source.Title,
        target.URL = source.URL,
        target.StatusID = 3,
        target.Sort = source.Sort,
        target.Domain = N'techport.vn',
        target.Modified = @Now,
        target.Modifier = N'seed-homepage-partner-logos'
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Title, SRC, URL, Subject, StatusID, Sort, LanguageID, Domain, SiteId, Created, Creator)
    VALUES (source.Title, source.SRC, source.URL, 5, 3, source.Sort, 1, N'techport.vn', 1, @Now, N'seed-homepage-partner-logos');

COMMIT TRANSACTION;

SELECT ID, Title, SRC, Sort, StatusID
FROM ImagesAdver
WHERE Subject = 5
  AND StatusID = 3
ORDER BY Sort, ID;
