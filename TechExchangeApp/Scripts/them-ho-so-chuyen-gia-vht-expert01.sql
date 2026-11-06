/* =====================================================================
   L2-11: tạo hồ sơ chuyên gia cho tài khoản vận hành thử vht_expert01 để chọn được làm nhà tư vấn.
   Hồ sơ ở trạng thái Nháp, đã Kích hoạt: có trong danh sách nhà tư vấn, không hiển thị công khai.
   Chạy lại được.
   sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -i them-ho-so-chuyen-gia-vht-expert01.sql
   ===================================================================== */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;

DECLARE @UserId INT = (SELECT UserId FROM dbo.Users WHERE UserName = 'vht_expert01');
IF @UserId IS NULL
BEGIN
    RAISERROR(N'Không có tài khoản vht_expert01.', 16, 1);
    RETURN;
END

BEGIN TRAN;
IF EXISTS (SELECT 1 FROM dbo.NhaTuVan WHERE UserId = @UserId)
    UPDATE dbo.NhaTuVan SET IsActivated = 1 WHERE UserId = @UserId;
ELSE
    INSERT INTO dbo.NhaTuVan (FullName, DateOfBirth, DiaChi, Phone, Email, UserId, IsActivated, StatusId, Domain, LanguageId, SiteId)
    SELECT u.FullName, '1980-01-01', N'Dữ liệu vận hành thử', ISNULL(u.Phone, ''), u.Email, u.UserId, 1, 1, ISNULL(u.Domain, ''), 1, ISNULL(u.SiteId, 1)
    FROM dbo.Users u WHERE u.UserId = @UserId;
COMMIT;

SELECT n.TuVanId, n.FullName, n.UserId, n.IsActivated, n.StatusId FROM dbo.NhaTuVan n WHERE n.UserId = @UserId;
