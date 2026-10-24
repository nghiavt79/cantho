/*
Tạo bảng SiteVisitDaily — thống kê lượt truy cập toàn Sàn theo ngày
(số liệu Báo cáo kết quả cung cấp dịch vụ – Mẫu số 2 PL VIII TT41; xem tại CMS > Thống kê).
Ứng dụng cộng dồn số liệu mỗi phút; nếu chưa có bảng, ứng dụng vẫn chạy bình thường nhưng không lưu lượt truy cập.
Chạy TRƯỚC khi deploy code mới. An toàn, chạy lại được (idempotent).
*/
IF OBJECT_ID('dbo.SiteVisitDaily', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SiteVisitDaily (
        VisitDate date NOT NULL CONSTRAINT PK_SiteVisitDaily PRIMARY KEY,
        PageViews int  NOT NULL CONSTRAINT DF_SiteVisitDaily_PageViews DEFAULT 0,
        Visitors  int  NOT NULL CONSTRAINT DF_SiteVisitDaily_Visitors DEFAULT 0
    );
    PRINT 'SiteVisitDaily created';
END
ELSE
    PRINT 'SiteVisitDaily already exists';
