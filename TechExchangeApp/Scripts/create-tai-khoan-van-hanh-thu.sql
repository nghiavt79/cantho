/* ============================================================================
   TẠO BỘ TÀI KHOẢN VẬN HÀNH THỬ – 10 TÀI KHOẢN
   Sàn Giao dịch Công nghệ thành phố Cần Thơ – Gói thầu số 4
   ----------------------------------------------------------------------------
   Mật khẩu mặc định : Hello@2026  (người dùng đổi mật khẩu ở lần đăng nhập đầu tiên)
   Cơ cấu            : Chủ trì thuê 2 · Đơn vị vận hành thử 6 · Đơn vị giám sát 1 · Nhà cung cấp 1

   CÁCH CHẠY (BẮT BUỘC có -f 65001 để không lỗi font tiếng Việt):
       sqlcmd -S <server> -U <user> -P <pwd> -d TechExchangeNew -C -f 65001 ^
              -i create-tai-khoan-van-hanh-thu.sql

   GHI CHÚ
   - Hash theo chuẩn ASP.NET Core Identity v3 (PBKDF2-HMAC-SHA512, 100.000 vòng,
     salt 16 byte riêng từng tài khoản) – cùng định dạng tài khoản 'admin'.
   - CMS mở cho tài khoản IsAdmin = 1; CMS CHƯA có vai trò chỉ xem, nên tài khoản
     giám sát (gs_theodoi) vào được CMS như quản trị – thống nhất chỉ xem; mọi thao
     tác đều ghi vào Nhật ký.
   - Khi bật xác thực lớp 2 CMS (CMS_MFA_ENABLED = 1), mã OTP gửi về email: SỬA EMAIL
     THẬT cho các tài khoản IsAdmin = 1 trước khi chạy.
   - Không cấp RoleId 0 (SysAdmin), 20 (Root admin).
   - Script CHẠY LẠI ĐƯỢC: tài khoản đã tồn tại được bỏ qua, không đổi mật khẩu.

   >>> SỬA HỌ TÊN / EMAIL / ĐIỆN THOẠI TẠI BẢNG @Acc TRƯỚC KHI CHẠY <<<
   ============================================================================ */

SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Domain  nvarchar(1000) = N'techport.vn';
    DECLARE @SiteId  int = 1;
    DECLARE @Created datetime = GETDATE();

    DECLARE @Acc TABLE (
        Seq        int,
        UserName   nvarchar(100),
        FullName   nvarchar(400),
        Email      nvarchar(100),
        Phone      varchar(50),
        DonVi      nvarchar(100),
        VaiTro     nvarchar(100),
        IsAdmin    bit,
        UserTypeId int,
        AcctType   int,
        PwdHash    nvarchar(max)
    );

    INSERT INTO @Acc (Seq, UserName, FullName, Email, Phone, DonVi, VaiTro, IsAdmin, UserTypeId, AcctType, PwdHash) VALUES
    (1, N'ttud_admin', N'Quản trị Sàn – Trung tâm Ứng dụng', N'ttud.admin@example.com', '0901000001', N'Chủ trì thuê (Trung tâm Ứng dụng)', N'Quản trị Sàn', 1, 1, 1,
        N'AQAAAAIAAYagAAAAEMkqNQV9GqRM3frUgG7JC4Ey+/9Kaehesl8FXu4gU2+Rvw9MqF0Qz0IIToOCNG2krQ=='),
    (2, N'ttud_bientap', N'Biên tập Sàn – Trung tâm Ứng dụng', N'ttud.bientap@example.com', '0901000002', N'Chủ trì thuê (Trung tâm Ứng dụng)', N'Biên tập nội dung', 1, 1, 1,
        N'AQAAAAIAAYagAAAAEKRwSJ/YMEvCOP5KUYjklvyNtnjpDVyzfbrwu5MkkHBHBmYf/Uvl2oVBNb9S8OKdlA=='),
    (3, N'vht_admin', N'Kiểm thử quản trị – Đơn vị vận hành thử', N'vht.admin@example.com', '0902000001', N'Đơn vị vận hành thử (Bình Định)', N'Quản trị (kiểm thử CMS)', 1, 1, 1,
        N'AQAAAAIAAYagAAAAEDJ0GXjt/dBFvR31oWDzYy65cHYS4P2IJP8xm2DxYezGfBvUgKlFXQQJKjrPNjCUJw=='),
    (4, N'vht_buyer01', N'Người mua 01 – Vận hành thử', N'vht.buyer01@example.com', '0902000002', N'Đơn vị vận hành thử (Bình Định)', N'Người mua công nghệ', 0, 5, 2,
        N'AQAAAAIAAYagAAAAEJSV2mtP9hQAMywSRjx+aG4EWLjdq28lq6xidEfkumPmzPeKfv5LyDZiyIh1odIF3A=='),
    (5, N'vht_buyer02', N'Người mua 02 – Vận hành thử', N'vht.buyer02@example.com', '0902000003', N'Đơn vị vận hành thử (Bình Định)', N'Người mua công nghệ', 0, 5, 2,
        N'AQAAAAIAAYagAAAAEG2NiDUyq8uL6Yw9MSGa6wATNq9MoMagFDIcon7hF+ir7p+FaKvoMIvxLX06/VXKYA=='),
    (6, N'vht_seller01', N'Nhà cung ứng 01 – Vận hành thử', N'vht.seller01@example.com', '0902000004', N'Đơn vị vận hành thử (Bình Định)', N'Người bán (nhà cung ứng)', 0, 5, 2,
        N'AQAAAAIAAYagAAAAEEMEY66X481/UyVJYWVIm0zP0bB7GaISc9n0vtMKV/D1h8nYJHQ2ToL0BbxOEFQ+GQ=='),
    (7, N'vht_seller02', N'Nhà cung ứng 02 – Vận hành thử', N'vht.seller02@example.com', '0902000005', N'Đơn vị vận hành thử (Bình Định)', N'Người bán (nhà cung ứng)', 0, 5, 2,
        N'AQAAAAIAAYagAAAAEDUMU8g7y1L0tqvboN0hoQdMAR3cOva0lYXSLwRvGfBFKp6B74CzzIKvoEUSuXCV5w=='),
    (8, N'vht_expert01', N'Chuyên gia tư vấn 01 – Vận hành thử', N'vht.expert01@example.com', '0902000006', N'Đơn vị vận hành thử (Bình Định)', N'Chuyên gia, tư vấn', 0, 5, 1,
        N'AQAAAAIAAYagAAAAEMqLt83L3q1CdalTgspT/RmHX3Ws/UVg7EiunD5Eqr7LSNTiornvyWUgZ/3In3PgSA=='),
    (9, N'gs_theodoi', N'Theo dõi giám sát – Chuyển Đổi Số 365', N'gs.theodoi@example.com', '0903000001', N'Đơn vị giám sát (Chuyển Đổi Số 365)', N'Theo dõi CMS (nhật ký, thống kê, hỗ trợ)', 1, 1, 1,
        N'AQAAAAIAAYagAAAAEC6H7sB3U3iAZOmFGgs3O9i/d1PUG7+KaqBDyJi9VIZUst+NN3IpJbJFo732JVCXZg=='),
    (10, N'ncc_hotro', N'Cán bộ hỗ trợ – Nhà cung cấp', N'ncc.hotro@example.com', '0904000001', N'Nhà cung cấp dịch vụ (MobiFone)', N'Hỗ trợ người dùng', 1, 1, 1,
        N'AQAAAAIAAYagAAAAEP/z/QH0vy7yvdKjPuKW3xWTvCZNEViLdZefAaeXaZSVdwikOpi962YMMRQ7Q7c+ag==');

    DECLARE @Roles TABLE (UserName nvarchar(100), RoleId int);
    INSERT INTO @Roles (UserName, RoleId) VALUES
    (N'ttud_admin', 19),
    (N'ttud_admin', 21),
    (N'ttud_bientap', 21),
    (N'vht_admin', 19),
    (N'vht_admin', 21),
    (N'vht_buyer01', 11),
    (N'vht_buyer02', 11),
    (N'vht_seller01', 11),
    (N'vht_seller02', 11),
    (N'vht_expert01', 11),
    (N'gs_theodoi', 19),
    (N'ncc_hotro', 19),
    (N'ncc_hotro', 21);

    IF EXISTS (SELECT 1 FROM dbo.Users u JOIN @Acc a ON a.UserName = u.UserName)
    BEGIN
        PRINT N'--- CẢNH BÁO: các tài khoản sau ĐÃ TỒN TẠI, script bỏ qua ---';
        SELECT u.UserId, u.UserName, u.FullName, u.Email
        FROM dbo.Users u JOIN @Acc a ON a.UserName = u.UserName;
    END

    /* 1) Tạo tài khoản */
    INSERT INTO dbo.Users
        (UserName, Password, Email, FullName, Phone, IsUser, Created, IsActivated,
         UserTypeId, IsAdmin, Domain, LanguageId, ParentId, SiteId,
         PhoneVerified, EmailVerified, AccountTypeId, VerificationLevel)
    SELECT a.UserName, a.PwdHash, a.Email, a.FullName, a.Phone,
           CAST(a.UserTypeId AS tinyint), @Created, 1,
           a.UserTypeId, a.IsAdmin, @Domain, 1, 0, @SiteId,
           0, 0, a.AcctType, 0
    FROM @Acc a
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Users u WHERE u.UserName = a.UserName);
    DECLARE @Inserted int = @@ROWCOUNT;

    /* 2) Gán vai trò: 19 Admin Sàn, 21 Biên tập sàn, 11 Member */
    INSERT INTO dbo.UserRole (UserId, RoleId, Domain, LanguageId, ParentId, SiteId)
    SELECT u.UserId, r.RoleId, @Domain, 1, 0, @SiteId
    FROM @Roles r
    JOIN dbo.Users u ON u.UserName = r.UserName
    WHERE NOT EXISTS (SELECT 1 FROM dbo.UserRole ur WHERE ur.UserId = u.UserId AND ur.RoleId = r.RoleId);

    COMMIT TRANSACTION;
    PRINT N'=== HOÀN TẤT – số tài khoản mới tạo: ' + CAST(@Inserted AS nvarchar(10)) + N' ===';

    /* 3) Bảng bàn giao */
    SELECT a.Seq AS [STT], a.DonVi AS [Đơn vị], a.VaiTro AS [Vai trò], u.UserName AS [Tên đăng nhập],
           u.FullName AS [Họ và tên], u.Email AS [Email],
           CASE WHEN u.IsAdmin = 1 THEN N'Có (vào /cms)' ELSE N'Không' END AS [Quản trị], u.UserId AS [ID]
    FROM dbo.Users u JOIN @Acc a ON a.UserName = u.UserName
    ORDER BY a.Seq;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT N'=== LỖI – đã hoàn tác toàn bộ, không tài khoản nào được tạo ===';
    PRINT ERROR_MESSAGE();
    THROW;
END CATCH
