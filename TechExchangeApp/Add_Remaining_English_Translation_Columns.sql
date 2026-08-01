IF COL_LENGTH('dbo.SanPhamCNTB', 'OriginalId') IS NULL
    ALTER TABLE dbo.SanPhamCNTB ADD OriginalId int NULL;
IF COL_LENGTH('dbo.SanPhamCNTB', 'EnStale') IS NULL
    ALTER TABLE dbo.SanPhamCNTB ADD EnStale bit NULL;
IF COL_LENGTH('dbo.SanPhamCNTB', 'SourceHash') IS NULL
    ALTER TABLE dbo.SanPhamCNTB ADD SourceHash varchar(64) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SanPhamCNTB_Lang_Original' AND object_id = OBJECT_ID('dbo.SanPhamCNTB'))
    CREATE INDEX IX_SanPhamCNTB_Lang_Original ON dbo.SanPhamCNTB(LanguageId, OriginalId);

IF COL_LENGTH('dbo.NhaTuVan', 'OriginalId') IS NULL
    ALTER TABLE dbo.NhaTuVan ADD OriginalId int NULL;
IF COL_LENGTH('dbo.NhaTuVan', 'EnStale') IS NULL
    ALTER TABLE dbo.NhaTuVan ADD EnStale bit NULL;
IF COL_LENGTH('dbo.NhaTuVan', 'SourceHash') IS NULL
    ALTER TABLE dbo.NhaTuVan ADD SourceHash varchar(64) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NhaTuVan_Lang_Original' AND object_id = OBJECT_ID('dbo.NhaTuVan'))
    CREATE INDEX IX_NhaTuVan_Lang_Original ON dbo.NhaTuVan(LanguageId, OriginalId);

IF COL_LENGTH('dbo.NhaCungUng', 'OriginalId') IS NULL
    ALTER TABLE dbo.NhaCungUng ADD OriginalId int NULL;
IF COL_LENGTH('dbo.NhaCungUng', 'EnStale') IS NULL
    ALTER TABLE dbo.NhaCungUng ADD EnStale bit NULL;
IF COL_LENGTH('dbo.NhaCungUng', 'SourceHash') IS NULL
    ALTER TABLE dbo.NhaCungUng ADD SourceHash varchar(64) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NhaCungUng_Lang_Original' AND object_id = OBJECT_ID('dbo.NhaCungUng'))
    CREATE INDEX IX_NhaCungUng_Lang_Original ON dbo.NhaCungUng(LanguageId, OriginalId);
