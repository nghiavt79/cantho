/*
Seeds a COMPLETE demo project (all 14 workflow steps marked done) for the
technology "Hệ thống và công nghệ tuyển rửa, xử lý cát sạch" of
Công ty Cổ phần Công nghệ Cát Sạch MeKong (NhaCungUng CungUngId = 8904,
SanPhamCNTB ID = 35587).

Purpose: demo the full 14-step transaction workflow end-to-end. Every step
table gets one realistic row with StatusId = 5 (Completed); the e-contract
gets StatusId = 5 (FullySigned) which drives both step 6 (LegalReview) and
step 7 (Signing); the NDA gets DaDongY = 1 (which the app reads as done).

Roles:
  - Buyer (Role 1)      = existing user 72  (Người mua)
  - Seller (Role 2)     = NEW user 'catsachmekong' (Mai Phương Trang), created
                          here by copying Password/Domain/AccountType from the
                          existing supplier account 14730 so it is a valid,
                          login-capable account tied to Cát Sạch MeKong.
  - Consultant (Role 3) = existing user 18973 (Tư vấn viên)

Idempotent: guarded by ProjectCode 'PRJ-CATSACH-2026' and by seller UserName
'catsachmekong'. Re-running does nothing if the project already exists.

Status enums used (see Enums/): StepStatus.Completed = 5, ContractStatus.FullySigned = 5.
*/

SET NOCOUNT ON;
DECLARE @Now DATETIME = GETDATE();

-- =====================================================================
-- 0. Seller account for Cát Sạch MeKong (copy constraints from user 14730)
-- =====================================================================
IF NOT EXISTS (SELECT 1 FROM Users WHERE UserName = 'catsachmekong')
BEGIN
    INSERT INTO Users
        (UserName, Password, Email, FullName, IsUser, Created, IsActivated,
         Phone, DiaChi, Domain, PhoneVerified, EmailVerified,
         AccountTypeId, VerificationLevel, UserTypeId, SiteId, LanguageId, IsAdmin)
    SELECT
        'catsachmekong', Password, 'catdasachct@gmail.com', N'Mai Phương Trang',
        IsUser, @Now, 1,
        '0939803803', N'Số 01, Đường B30, KDC Hưng Phú, KV8, P. Hưng Phú, TP. Cần Thơ',
        Domain, 1, 1,
        AccountTypeId, VerificationLevel, UserTypeId, SiteId, LanguageId, 0
    FROM Users WHERE UserId = 14730;
END

DECLARE @SellerId INT = (SELECT UserId FROM Users WHERE UserName = 'catsachmekong');
DECLARE @BuyerId  INT = 72;      -- Người mua
DECLARE @ConsId   INT = 18973;   -- Tư vấn viên
DECLARE @TechId   INT = 35587;   -- SanPhamCNTB
DECLARE @TenCN    NVARCHAR(200) = N'Hệ thống và công nghệ tuyển rửa, xử lý cát sạch (cát biển, cát sông, cát nhiễm mặn)';
DECLARE @BenBuyer NVARCHAR(200) = N'Công ty Cổ phần Vật liệu Xây dựng Tây Đô';
DECLARE @BenSeller NVARCHAR(200) = N'Công ty Cổ phần Công nghệ Cát Sạch MeKong';

-- =====================================================================
-- Guard: create everything only if the demo project does not exist yet
-- =====================================================================
IF NOT EXISTS (SELECT 1 FROM Projects WHERE ProjectCode = 'PRJ-CATSACH-2026')
BEGIN
    -- 1. Project ---------------------------------------------------------
    -- Projects.StatusId: 1=Draft, 2=Active, 3=Completed, 4=Cancelled (xem ProjectService.GetStatusName).
    -- Demo đã xong cả 14 bước nên dùng 3 (Completed) để trang MyProjects hiển thị đúng.
    INSERT INTO Projects (ProjectCode, ProjectName, Description, StatusId, CreatedBy, CreatedDate, SelectedSellerId, SelectedDate)
    VALUES ('PRJ-CATSACH-2026', @TenCN,
            N'Dự án chuyển giao công nghệ tuyển rửa, xử lý cát sạch từ Công ty CP Công nghệ Cát Sạch MeKong cho ' + @BenBuyer + N'. Demo trọn 14 bước quy trình giao dịch.',
            3, @BuyerId, @Now, @SellerId, @Now);

    DECLARE @ProjectId INT = SCOPE_IDENTITY();

    -- 2. Project members -------------------------------------------------
    INSERT INTO ProjectMembers (ProjectId, UserId, Role, JoinedDate, IsActive)
    VALUES (@ProjectId, @BuyerId,  1, @Now, 1),
           (@ProjectId, @SellerId, 2, @Now, 1),
           (@ProjectId, @ConsId,   3, @Now, 1);

    -- 3. Summary steps (ProjectSteps) — all completed --------------------
    -- QUAN TRỌNG: trang MyProjects (ProjectService.GetMyProjectsAsync) đếm bước hoàn tất bằng
    -- ProjectSteps.StatusId == 2, và toàn bộ dữ liệu project thực tế cũng dùng 2 = hoàn tất.
    -- (Enum StepStatus.Completed = 5 chỉ áp cho các bảng bước chi tiết, không khớp bảng tổng hợp này.)
    -- Dùng 2 để MyProjects hiển thị đúng 14/14.
    INSERT INTO ProjectSteps (ProjectId, StepNumber, StepName, StatusId, CreatedDate, CompletedDate)
    VALUES
        (@ProjectId, 1,  N'Yêu cầu chuyển giao công nghệ', 2, @Now, @Now),
        (@ProjectId, 2,  N'Thỏa thuận bảo mật (NDA)',      2, @Now, @Now),
        (@ProjectId, 3,  N'Yêu cầu báo giá (RFQ)',         2, @Now, @Now),
        (@ProjectId, 4,  N'Nộp hồ sơ đề xuất',             2, @Now, @Now),
        (@ProjectId, 5,  N'Đàm phán thương mại',           2, @Now, @Now),
        (@ProjectId, 6,  N'Kiểm tra pháp lý',              2, @Now, @Now),
        (@ProjectId, 7,  N'Ký hợp đồng điện tử',           2, @Now, @Now),
        (@ProjectId, 8,  N'Xác nhận tạm ứng',              2, @Now, @Now),
        (@ProjectId, 9,  N'Thử nghiệm Pilot',              2, @Now, @Now),
        (@ProjectId, 10, N'Bàn giao & triển khai thiết bị',2, @Now, @Now),
        (@ProjectId, 11, N'Đào tạo & chuyển giao vận hành',2, @Now, @Now),
        (@ProjectId, 12, N'Bàn giao hồ sơ kỹ thuật',       2, @Now, @Now),
        (@ProjectId, 13, N'Nghiệm thu',                    2, @Now, @Now),
        (@ProjectId, 14, N'Thanh lý hợp đồng',             2, @Now, @Now);

    -- ===================================================================
    -- STEP 1 — Yêu cầu chuyển giao công nghệ
    -- ===================================================================
    INSERT INTO TechTransferRequests
        (HoTen, ChucVu, DonVi, DiaChi, DienThoai, Email, TenCongNghe, MoTaNhuCau,
         LinhVuc, NganSachDuKien, ProjectId, StatusId, NguoiTao, NgayTao, FromId, TypeData)
    VALUES
        (N'Nguyễn Thị Dung', N'Giám đốc', @BenBuyer,
         N'Khu công nghiệp Trà Nóc, TP. Cần Thơ', '0939700303', 'nguyenthidung.mobifone@gmail.com',
         @TenCN,
         N'Cần nhận chuyển giao công nghệ và dây chuyền thiết bị tuyển rửa cát biển/cát nhiễm mặn công suất ~150 m³/giờ để sản xuất cát sạch đạt TCVN 13754:2023 phục vụ bê tông và san lấp, thay thế nguồn cát sông khan hiếm.',
         N'Vật liệu xây dựng', 4500000000, @ProjectId, 5, @BuyerId, @Now, @TechId, 1);

    -- ===================================================================
    -- STEP 2 — Thỏa thuận bảo mật (NDA)   (DaDongY = 1 => app reads as done)
    -- ===================================================================
    INSERT INTO NDAAgreements
        (BenA, BenB, LoaiNDA, ThoiHanBaoMat, XacNhanKySo, ProjectId, DaDongY, StatusId, NguoiTao, NgayTao)
    VALUES
        (@BenBuyer, @BenSeller, N'Hai chiều', N'3 năm kể từ ngày ký',
         N'Đã xác nhận bằng chữ ký điện tử ngày ' + CONVERT(VARCHAR, @Now, 103),
         @ProjectId, 1, 2, @BuyerId, @Now);

    -- ===================================================================
    -- STEP 3 — Yêu cầu báo giá (RFQ)
    -- ===================================================================
    INSERT INTO RFQRequests
        (MaRFQ, YeuCauKyThuat, TieuChuanNghiemThu, HanChotNopHoSo, ProjectId, DaGuiNhaCungUng, StatusId, NguoiTao, NgayTao)
    VALUES
        (N'RFQ-CATSACH-2026-001',
         N'1. Công suất tuyển rửa ≥ 150 m³/giờ cát nguyên liệu.
2. Cát thành phẩm: tạp chất hữu cơ < 1%, ion Cl⁻ < 0,01%, đạt TCVN 13754:2023.
3. Xử lý được cát biển, cát sông, cát nhiễm mặn.
4. Hệ thống hồ lắng, tuần hoàn nước, không dùng hóa chất.
5. Bàn giao kèm đào tạo vận hành và hồ sơ kỹ thuật.',
         N'Nghiệm thu theo mẻ cát thành phẩm; lấy mẫu kiểm định Cl⁻ và tạp chất tại Quatest; chạy thử đạt công suất thiết kế.',
         DATEADD(DAY, 30, @Now), @ProjectId, 1, 5, @BuyerId, @Now);

    -- ===================================================================
    -- STEP 4 — Nộp hồ sơ đề xuất
    -- ===================================================================
    INSERT INTO ProposalSubmissions
        (RFQId, ProjectId, GiaiPhapKyThuat, BaoGiaSoBo, ThoiGianTrienKhai, HoSoNangLucDinhKem, StatusId, NguoiTao, NgayTao, SubmittedDate)
    VALUES
        (NULL, @ProjectId,
         N'Cung cấp dây chuyền tuyển rửa cát biển đồng bộ theo 2 sáng chế độc quyền (Bằng 35015 và 42793): hệ thống sàng rửa – phân loại – khử mặn, hồ lắng tuần hoàn nước. Công suất 150 m³/giờ, mở rộng đến 1.000 m³/giờ.',
         4600000000, N'5 tháng (lắp đặt, chạy thử, đào tạo, nghiệm thu)',
         N'Hồ sơ năng lực, Giấy chứng nhận DN KH&CN số 30/DNKHCN, 2 bằng độc quyền sáng chế, hồ sơ thiết bị.',
         5, @SellerId, @Now, @Now);

    -- ===================================================================
    -- STEP 5 — Đàm phán thương mại
    -- ===================================================================
    INSERT INTO NegotiationForms
        (RFQId, ProjectId, GiaChotCuoiCung, DieuKhoanThanhToan, HinhThucKy, DaKySo, StatusId, NguoiTao, NgayTao,
         SellerId, SellerSigned, BuyerSigned, SellerSignedAt, BuyerSignedAt)
    VALUES
        (NULL, @ProjectId, 4500000000,
         N'Tạm ứng 30% khi ký hợp đồng; 40% khi lắp đặt xong chạy thử đạt; 30% sau nghiệm thu. Bảo hành 24 tháng.',
         N'Chữ ký số', 1, 5, @BuyerId, @Now,
         @SellerId, 1, 1, @Now, @Now);

    -- ===================================================================
    -- STEP 6 + 7 — Hợp đồng điện tử (FullySigned = 5 => cả 2 bước hoàn tất)
    -- ===================================================================
    INSERT INTO ProjectContracts
        (ProjectId, VersionNumber, SourceType, Title, StatusId, HtmlContent, ReadyToSignAt, IsActive, CreatedBy, CreatedDate, Note)
    VALUES
        (@ProjectId, 1, 0,
         N'Hợp đồng chuyển giao công nghệ tuyển rửa, xử lý cát sạch – ' + @BenSeller,
         5,
         N'<h3>HỢP ĐỒNG CHUYỂN GIAO CÔNG NGHỆ</h3><p>Bên A (Bên nhận): ' + @BenBuyer + N'</p><p>Bên B (Bên chuyển giao): ' + @BenSeller + N'</p><p>Đối tượng: ' + @TenCN + N'</p><p>Giá trị hợp đồng: 4.500.000.000 VNĐ. Thời gian triển khai: 5 tháng. Bảo hành: 24 tháng.</p><p>Hai bên đã ký số đầy đủ.</p>',
         @Now, 1, @BuyerId, @Now, N'Demo - đã ký số đầy đủ 2 bên');

    -- ===================================================================
    -- STEP 8 — Xác nhận tạm ứng (30%)
    -- ===================================================================
    INSERT INTO AdvancePaymentConfirmations
        (EContractId, ProjectId, SoTienTamUng, NgayChuyen, DaXacNhanNhanTien, StatusId, NguoiTao, NgayTao)
    VALUES
        (NULL, @ProjectId, 1350000000, @Now, 1, 5, @BuyerId, @Now);

    -- ===================================================================
    -- STEP 9 — Thử nghiệm Pilot
    -- ===================================================================
    INSERT INTO PilotTestReports
        (ProjectId, MoTaThuNghiem, KetQuaThuNghiem, VanDePhatSinh, GiaiPhap, DatYeuCau, NgayBatDau, NgayKetThuc, StatusId, NguoiTao, NgayTao)
    VALUES
        (@ProjectId,
         N'Chạy thử dây chuyền với cát biển nguyên liệu tại công trường, đo công suất và lấy mẫu cát thành phẩm.',
         N'Đạt công suất 152 m³/giờ; cát thành phẩm tạp chất hữu cơ 0,8%, ion Cl⁻ 0,008% — đạt TCVN 13754:2023.',
         N'Độ ẩm cát nguyên liệu cao làm giảm nhẹ tốc độ nạp liệu.', N'Bổ sung sàng tách nước sơ bộ đầu vào.',
         1, DATEADD(DAY,-20,@Now), DATEADD(DAY,-15,@Now), 5, @SellerId, @Now);

    -- ===================================================================
    -- STEP 10 — Bàn giao & triển khai thiết bị
    -- ===================================================================
    INSERT INTO HandoverReports
        (ProjectId, EContractId, DanhMucThietBiJson, DanhMucHoSoJson, DaHoanThanhDaoTao, DanhGiaSao, NhanXet, StatusId, NguoiTao, NgayTao)
    VALUES
        (@ProjectId, NULL,
         N'["Cụm sàng rửa – phân loại","Hệ thống bơm & tuần hoàn nước","Hồ lắng tách tạp chất","Băng tải & silo chứa cát thành phẩm"]',
         N'["Biên bản bàn giao thiết bị","Sơ đồ lắp đặt","Danh mục phụ tùng thay thế"]',
         1, 5, N'Thiết bị lắp đặt và vận hành đúng thiết kế, bên nhận hài lòng.', 5, @SellerId, @Now);

    -- ===================================================================
    -- STEP 11 — Đào tạo & chuyển giao vận hành
    -- ===================================================================
    INSERT INTO TrainingHandovers
        (ProjectId, EContractId, NoiDungDaoTao, DanhSachNguoiThamGia, SoNguoiThamGia, SoGioDaoTao, DaHoanThanh, NgayBatDau, NgayKetThuc, StatusId, NguoiTao, NgayTao)
    VALUES
        (@ProjectId, NULL,
         N'Vận hành dây chuyền tuyển rửa, quy trình khử mặn, bảo trì thiết bị, xử lý sự cố và kiểm soát chất lượng cát thành phẩm.',
         N'Tổ vận hành và kỹ thuật của ' + @BenBuyer, 12, 24, 1,
         DATEADD(DAY,-10,@Now), DATEADD(DAY,-7,@Now), 5, @SellerId, @Now);

    -- ===================================================================
    -- STEP 12 — Bàn giao hồ sơ kỹ thuật
    -- ===================================================================
    INSERT INTO TechnicalDocHandovers
        (ProjectId, EContractId, DanhMucHoSo, TaiLieuKyThuat, TaiLieuHuongDanSuDung, TaiLieuBaoTri, GhiChu, DaBanGiaoDayDu, NgayBanGiao, StatusId, NguoiTao, NgayTao)
    VALUES
        (@ProjectId, NULL,
         N'Bản vẽ thiết kế hệ thống, quy trình công nghệ tuyển rửa, tài liệu vận hành – bảo trì, hồ sơ kiểm định chất lượng cát thành phẩm.',
         N'/uploads/handover/catsach-tai-lieu-ky-thuat.pdf',
         N'/uploads/handover/catsach-huong-dan-van-hanh.pdf',
         N'/uploads/handover/catsach-bao-tri.pdf',
         N'Đã bàn giao đầy đủ bản in và bản mềm.', 1, @Now, 5, @SellerId, @Now);

    -- ===================================================================
    -- STEP 13 — Nghiệm thu
    -- ===================================================================
    INSERT INTO AcceptanceReports
        (ProjectId, EContractId, NgayNghiemThu, ThanhPhanThamGia, KetLuanNghiemThu, VanDeTonDong, TrangThaiKy, StatusId, NguoiTao, NgayTao)
    VALUES
        (@ProjectId, NULL, @Now,
         N'Đại diện ' + @BenBuyer + N', ' + @BenSeller + N' và tư vấn giám sát.',
         N'Hệ thống đạt công suất và chất lượng cát thành phẩm theo hợp đồng và TCVN 13754:2023. Nghiệm thu đạt, đồng ý đưa vào vận hành chính thức.',
         N'Không', N'Đã ký', 5, @BuyerId, @Now);

    -- ===================================================================
    -- STEP 14 — Thanh lý hợp đồng
    -- ===================================================================
    INSERT INTO LiquidationReports
        (ProjectId, EContractId, GiaTriThanhToanConLai, SoHoaDon, SanDaChuyenTien, HopDongClosed, StatusId, NguoiTao, NgayTao)
    VALUES
        (@ProjectId, NULL, 0, N'HD-CATSACH-2026-0012', 1, 1, 5, @BuyerId, @Now);

    PRINT '================================================';
    PRINT 'Created demo project PRJ-CATSACH-2026';
    PRINT 'ProjectId = ' + CAST(@ProjectId AS VARCHAR);
    PRINT 'SellerId  = ' + CAST(@SellerId AS VARCHAR) + ' (catsachmekong)';
    PRINT 'All 14 steps seeded as completed.';
    PRINT '================================================';
END
ELSE
BEGIN
    PRINT 'Project PRJ-CATSACH-2026 already exists — nothing to do.';
END

-- Report
SELECT p.Id AS ProjectId, p.ProjectCode, p.StatusId, p.SelectedSellerId,
       (SELECT COUNT(*) FROM ProjectMembers m WHERE m.ProjectId = p.Id) AS Members,
       (SELECT COUNT(*) FROM ProjectSteps s WHERE s.ProjectId = p.Id AND s.StatusId = 5) AS StepsDone
FROM Projects p WHERE p.ProjectCode = 'PRJ-CATSACH-2026';
GO
