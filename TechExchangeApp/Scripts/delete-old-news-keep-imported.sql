/*
Customer request: delete all old "tin tức" content across the 3 news-related menus
(44 = Tin sự kiện, 46 = Báo cáo phân tích xu hướng công nghệ, 72 = Giải pháp công nghệ),
keeping ONLY the 44 articles just imported from ttud.com.vn/su-kien (Contents.Id
117040-117083, MenuId=44, StatusId=1).

Safety: takes a full backup copy into Contents_Backup_TinTuc_20260711 (same DB) before
the hard delete, since this is an irreversible operation on ~2406 rows of real content.
*/

SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.Contents_Backup_TinTuc_20260711') IS NOT NULL
    DROP TABLE dbo.Contents_Backup_TinTuc_20260711;

SELECT *
INTO dbo.Contents_Backup_TinTuc_20260711
FROM Contents
WHERE MenuId IN (44, 46, 72)
  AND Id NOT BETWEEN 117040 AND 117083;

DELETE FROM Contents
WHERE MenuId IN (44, 46, 72)
  AND Id NOT BETWEEN 117040 AND 117083;
GO
