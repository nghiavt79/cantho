/*
Consolidate data ownership to the 'admin' account (UserId 55) — requested 2026-07-16.
Keep accounts admin(55) and maihuong(4793) with their own data; reassign every OTHER
owner value to admin. Scope (confirmed): all owner fields = id columns + string
Creator/Modifier/CreatedBy audit columns.

String owner columns: set to 'admin' wherever the value is not null/empty and not one
of the kept owners ('admin','maihuong'). Applied across every base table via dynamic
SQL, EXCLUDING the *_Backup_/*Cleanup_/*DeletedBackup backup tables.

Id owner columns (FK to Users): Product.CreatorId/ModifierId, NhaCungUng.UserId,
NhaTuVan.UserId -> 55 where not null and not in (55,4793). This also clears any FK
owner reference to the 8 accounts about to be deleted.

Reversible via the user's full DB .bak (this is a metadata UPDATE, not a delete).
*/

SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO
USE TechExchangeNew;
GO

BEGIN TRANSACTION;

DECLARE @keptNames TABLE (nm nvarchar(50));
INSERT @keptNames VALUES (N'admin'), (N'maihuong');

-- String owner columns -> 'admin' -------------------------------------------------------
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql +
   'UPDATE [' + t.name + '] SET [' + c.name + ']=N''admin'' WHERE [' + c.name + '] IS NOT NULL AND LTRIM(RTRIM([' + c.name + ']))<>'''' AND [' + c.name + '] NOT IN (N''admin'',N''maihuong'');' + CHAR(10)
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id
JOIN sys.types ty ON c.user_type_id = ty.user_type_id
WHERE c.name IN ('Creator','Modifier','CreatedBy','ModifiedBy')
  AND ty.name IN ('nvarchar','varchar')
  AND t.name NOT LIKE '%Backup[_]2026%'
  AND t.name NOT LIKE '%Cleanup[_]2026%'
  AND t.name NOT LIKE '%DeletedBackup%';

EXEC sp_executesql @sql;

-- Id owner columns -> 55 ----------------------------------------------------------------
UPDATE Product    SET CreatorId  = 55 WHERE CreatorId  IS NOT NULL AND CreatorId  NOT IN (55,4793);
UPDATE Product    SET ModifierId = 55 WHERE ModifierId IS NOT NULL AND ModifierId NOT IN (55,4793);
UPDATE NhaCungUng SET UserId     = 55 WHERE UserId     IS NOT NULL AND UserId     NOT IN (55,4793);
UPDATE NhaTuVan   SET UserId     = 55 WHERE UserId     IS NOT NULL AND UserId     NOT IN (55,4793);

-- Report: distinct owners remaining in the main data tables ------------------------------
SELECT 'SanPhamCNTB' tbl, ISNULL(Creator,'(null)') owner, COUNT(*) c FROM SanPhamCNTB GROUP BY Creator
UNION ALL SELECT 'ChuyenGia', ISNULL(Creator,'(null)'), COUNT(*) FROM ChuyenGia GROUP BY Creator
UNION ALL SELECT 'Contents', ISNULL(Creator,'(null)'), COUNT(*) FROM Contents GROUP BY Creator
ORDER BY tbl, c DESC;

COMMIT TRANSACTION;
GO
