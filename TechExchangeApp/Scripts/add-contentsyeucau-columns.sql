/* ============================================================================
   Phiếu "Tìm mua" (bảng ContentsYeuCau) — bổ sung các cột hồ sơ nhu cầu
   + liên kết PhieuYeuCauCNTBId. Tương ứng migration 20260726090000_AddContentsYeuCauSourceLink.
   Chạy trên SERVER (dự án apply DB bằng SQL script tay, không dùng EF migrate).
   Idempotent: chạy lại nhiều lần không lỗi, không đụng dữ liệu cũ.
   ============================================================================ */

SET NOCOUNT ON;

/* ── Các cột cấu trúc hồ sơ nhu cầu (tách từ khối Contents) ─────────────────── */

IF COL_LENGTH('dbo.ContentsYeuCau','TrangThaiNhuCau') IS NULL
BEGIN
    ALTER TABLE dbo.ContentsYeuCau ADD TrangThaiNhuCau INT NULL;      -- 1 Đang tiếp nhận, 2 Đang xử lý, 3 Đã kết thúc
    PRINT 'Added TrangThaiNhuCau.';
END

IF COL_LENGTH('dbo.ContentsYeuCau','DiaPhuong') IS NULL
BEGIN
    ALTER TABLE dbo.ContentsYeuCau ADD DiaPhuong NVARCHAR(200) NULL;
    PRINT 'Added DiaPhuong.';
END

IF COL_LENGTH('dbo.ContentsYeuCau','HanTiepNhan') IS NULL
BEGIN
    ALTER TABLE dbo.ContentsYeuCau ADD HanTiepNhan DATETIME NULL;
    PRINT 'Added HanTiepNhan.';
END

IF COL_LENGTH('dbo.ContentsYeuCau','NganSach') IS NULL
BEGIN
    ALTER TABLE dbo.ContentsYeuCau ADD NganSach NVARCHAR(200) NULL;
    PRINT 'Added NganSach.';
END

IF COL_LENGTH('dbo.ContentsYeuCau','HinhThucHopTac') IS NULL
BEGIN
    ALTER TABLE dbo.ContentsYeuCau ADD HinhThucHopTac NVARCHAR(500) NULL;  -- danh sách phân tách bằng ';'
    PRINT 'Added HinhThucHopTac.';
END

IF COL_LENGTH('dbo.ContentsYeuCau','MucTieu') IS NULL
BEGIN
    ALTER TABLE dbo.ContentsYeuCau ADD MucTieu NVARCHAR(MAX) NULL;
    PRINT 'Added MucTieu.';
END

IF COL_LENGTH('dbo.ContentsYeuCau','HienTrang') IS NULL
BEGIN
    ALTER TABLE dbo.ContentsYeuCau ADD HienTrang NVARCHAR(MAX) NULL;
    PRINT 'Added HienTrang.';
END

IF COL_LENGTH('dbo.ContentsYeuCau','YeuCauKyThuat') IS NULL
BEGIN
    ALTER TABLE dbo.ContentsYeuCau ADD YeuCauKyThuat NVARCHAR(MAX) NULL;
    PRINT 'Added YeuCauKyThuat.';
END

IF COL_LENGTH('dbo.ContentsYeuCau','QuyMoTrienKhai') IS NULL
BEGIN
    ALTER TABLE dbo.ContentsYeuCau ADD QuyMoTrienKhai NVARCHAR(MAX) NULL;
    PRINT 'Added QuyMoTrienKhai.';
END

IF COL_LENGTH('dbo.ContentsYeuCau','TieuChiChonDoiTac') IS NULL
BEGIN
    ALTER TABLE dbo.ContentsYeuCau ADD TieuChiChonDoiTac NVARCHAR(MAX) NULL;
    PRINT 'Added TieuChiChonDoiTac.';
END

/* ── Liên kết tới Phiếu yêu cầu CNTB (1-1) ──────────────────────────────────── */

IF COL_LENGTH('dbo.ContentsYeuCau','PhieuYeuCauCNTBId') IS NULL
BEGIN
    ALTER TABLE dbo.ContentsYeuCau ADD PhieuYeuCauCNTBId INT NULL;
    PRINT 'Added PhieuYeuCauCNTBId.';
END
GO

/* ── Unique filtered index cho PhieuYeuCauCNTBId (mỗi phiếu ↔ 1 tìm mua) ─────── */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'UX_ContentsYeuCau_PhieuYeuCauCNTBId'
                 AND object_id = OBJECT_ID(N'dbo.ContentsYeuCau'))
BEGIN
    CREATE UNIQUE INDEX UX_ContentsYeuCau_PhieuYeuCauCNTBId
        ON dbo.ContentsYeuCau(PhieuYeuCauCNTBId)
        WHERE PhieuYeuCauCNTBId IS NOT NULL;
    PRINT 'Created UX_ContentsYeuCau_PhieuYeuCauCNTBId.';
END
GO

PRINT 'Done: add-contentsyeucau-columns.sql';
