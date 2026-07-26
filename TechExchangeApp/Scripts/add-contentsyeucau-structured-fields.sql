/*
    Thêm các cột cấu trúc cho ContentsYeuCau (tách khối Contents thành trường riêng)
    để trang chi tiết nhu cầu hiển thị dạng hồ sơ B2B. Idempotent — chạy lại an toàn.
*/
SET NOCOUNT ON;

IF COL_LENGTH('dbo.ContentsYeuCau','TrangThaiNhuCau') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD TrangThaiNhuCau INT NULL;      -- 1 Đang tiếp nhận, 2 Đang xử lý, 3 Đã kết thúc

IF COL_LENGTH('dbo.ContentsYeuCau','DiaPhuong') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD DiaPhuong NVARCHAR(200) NULL;

IF COL_LENGTH('dbo.ContentsYeuCau','HanTiepNhan') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD HanTiepNhan DATETIME NULL;

IF COL_LENGTH('dbo.ContentsYeuCau','NganSach') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD NganSach NVARCHAR(200) NULL;   -- text để cho phép "Trao đổi khi kết nối"

IF COL_LENGTH('dbo.ContentsYeuCau','HinhThucHopTac') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD HinhThucHopTac NVARCHAR(500) NULL; -- danh sách phân tách bằng ';'

IF COL_LENGTH('dbo.ContentsYeuCau','MucTieu') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD MucTieu NVARCHAR(MAX) NULL;    -- Tổng quan / mục tiêu

IF COL_LENGTH('dbo.ContentsYeuCau','HienTrang') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD HienTrang NVARCHAR(MAX) NULL;  -- Hiện trạng & vấn đề

IF COL_LENGTH('dbo.ContentsYeuCau','YeuCauKyThuat') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD YeuCauKyThuat NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.ContentsYeuCau','QuyMoTrienKhai') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD QuyMoTrienKhai NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.ContentsYeuCau','TieuChiChonDoiTac') IS NULL
    ALTER TABLE dbo.ContentsYeuCau ADD TieuChiChonDoiTac NVARCHAR(MAX) NULL;

PRINT 'Done: ContentsYeuCau structured fields.';
