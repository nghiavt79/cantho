using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechExchangeApp.Migrations
{
    public partial class AddContentsYeuCauSourceLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.ContentsYeuCau','TrangThaiNhuCau') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD TrangThaiNhuCau INT NULL;
IF COL_LENGTH('dbo.ContentsYeuCau','DiaPhuong') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD DiaPhuong NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.ContentsYeuCau','HanTiepNhan') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD HanTiepNhan DATETIME NULL;
IF COL_LENGTH('dbo.ContentsYeuCau','NganSach') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD NganSach NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.ContentsYeuCau','HinhThucHopTac') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD HinhThucHopTac NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.ContentsYeuCau','MucTieu') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD MucTieu NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.ContentsYeuCau','HienTrang') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD HienTrang NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.ContentsYeuCau','YeuCauKyThuat') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD YeuCauKyThuat NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.ContentsYeuCau','QuyMoTrienKhai') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD QuyMoTrienKhai NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.ContentsYeuCau','TieuChiChonDoiTac') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD TieuChiChonDoiTac NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.ContentsYeuCau','PhieuYeuCauCNTBId') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD PhieuYeuCauCNTBId INT NULL;

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_ContentsYeuCau_PhieuYeuCauCNTBId'
      AND object_id = OBJECT_ID(N'dbo.ContentsYeuCau')
)
BEGIN
    CREATE UNIQUE INDEX UX_ContentsYeuCau_PhieuYeuCauCNTBId
    ON dbo.ContentsYeuCau(PhieuYeuCauCNTBId)
    WHERE PhieuYeuCauCNTBId IS NOT NULL;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_ContentsYeuCau_PhieuYeuCauCNTBId'
      AND object_id = OBJECT_ID(N'dbo.ContentsYeuCau')
)
    DROP INDEX UX_ContentsYeuCau_PhieuYeuCauCNTBId ON dbo.ContentsYeuCau;

IF COL_LENGTH('dbo.ContentsYeuCau','PhieuYeuCauCNTBId') IS NOT NULL
    ALTER TABLE dbo.ContentsYeuCau DROP COLUMN PhieuYeuCauCNTBId;
");
        }
    }
}
