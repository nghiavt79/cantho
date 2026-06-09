SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @Now DATETIME = GETDATE();

DECLARE @Customers TABLE
(
    Title NVARCHAR(255) NOT NULL,
    SRC NVARCHAR(500) NOT NULL,
    URL NVARCHAR(500) NULL,
    Sort INT NOT NULL
);

INSERT INTO @Customers (Title, SRC, URL, Sort)
VALUES
(N'Gạo Tây Đô Kim Cương', N'/image/logo-khach-hang/01-khach-hang.png', NULL, 1),
(N'Ong nhà Trọng', N'/image/logo-khach-hang/02-khach-hang.jpeg', NULL, 2),
(N'Nước ép trái cây và Rau củ Ms. Thủy', N'/image/logo-khach-hang/03-khach-hang.jpg', NULL, 3),
(N'Nông trại sạch Cần Thơ', N'/image/logo-khach-hang/04-khach-hang.jpg', NULL, 4),
(N'Nhãn Thanh Hữu Tâm', N'/image/logo-khach-hang/05-khach-hang.jpg', NULL, 5),
(N'Lò bún Út Tòng', N'/image/logo-khach-hang/06-khach-hang.jpg', NULL, 6),
(N'Hợp tác xã mãng cầu Thới Hưng', N'/image/logo-khach-hang/07-khach-hang.jpg', NULL, 7),
(N'Hợp Tác Xã Công nghệ cao ODA', N'/image/logo-khach-hang/08-khach-hang.png', NULL, 8),
(N'Hộ kinh doanh Yến Thiên VT', N'/image/logo-khach-hang/09-khach-hang.jpg', NULL, 9),
(N'Hộ kinh doanh Trần Văn Hon', N'/image/logo-khach-hang/10-khach-hang.jpg', NULL, 10),
(N'Hộ kinh doanh Trà Diệu Phúc', N'/image/logo-khach-hang/11-khach-hang.jpg', NULL, 11),
(N'Hộ kinh doanh Thế Vỹ', N'/image/logo-khach-hang/12-khach-hang.jpg', NULL, 12),
(N'Hộ kinh doanh Nguyễn Phú Tia', N'/image/logo-khach-hang/13-khach-hang.jpg', NULL, 13),
(N'Hộ kinh doanh Nguyễn Nhật Khoa', N'/image/logo-khach-hang/14-khach-hang.jpg', NULL, 14),
(N'HKD Cơ sở bánh tráng Hai Hiền', N'/image/logo-khach-hang/15-khach-hang.jpg', NULL, 15),
(N'HKD Chim Trĩ Đỏ Cần Thơ', N'/image/logo-khach-hang/16-khach-hang.jpg', NULL, 16),
(N'Giò Chả 69', N'/image/logo-khach-hang/17-khach-hang.png', NULL, 17),
(N'Công ty TNHH MTV SXTM&DV Sao Mai SMC', N'/image/logo-khach-hang/18-khach-hang.jpeg', NULL, 18),
(N'Công Ty TNHH Yến Sào Tịnh Hoằng', N'/image/logo-khach-hang/19-khach-hang.png', NULL, 19),
(N'Công ty TNHH Thương mại dịch vụ mật ong rừng Oginbee', N'/image/logo-khach-hang/20-khach-hang.png', NULL, 20),
(N'Công ty TNHH SUMOFOOD Việt Nam', N'/image/logo-khach-hang/21-khach-hang.jpeg', NULL, 21),
(N'Công ty TNHH MTV Thương mại Minh Đức Thành', N'/image/logo-khach-hang/22-khach-hang.png', NULL, 22),
(N'Công ty TNHH MTV Sản xuất Thương mại Đinh Gia Foods', N'/image/logo-khach-hang/23-khach-hang.png', NULL, 23),
(N'Cơ sở chế biến thực phẩm Sơn Uyên', N'/image/logo-khach-hang/24-khach-hang.jpg', NULL, 24),
(N'Cơ sở giá sạch Hồng Nhung', N'/image/logo-khach-hang/25-khach-hang.jpg', NULL, 25),
(N'Cơ sở sản xuất Út Anh', N'/image/logo-khach-hang/26-khach-hang.jpg', NULL, 26),
(N'Cơ sở Thuận Hòa', N'/image/logo-khach-hang/27-khach-hang.png', NULL, 27),
(N'Công ty Cổ phần Bia - Nước giải khát Sài Gòn - Tây Đô', N'/image/logo-khach-hang/28-khach-hang.jfif', NULL, 28),
(N'Công ty Cổ phần Thực phẩm Phạm Nghĩa', N'/image/logo-khach-hang/29-khach-hang.png', NULL, 29),
(N'Công ty TNHH CBNS Kim Nhiên', N'/image/logo-khach-hang/30-khach-hang.jpg', NULL, 30),
(N'Công ty TNHH MTV An Toàn Lương Thực Sạch Miền Tây', N'/image/logo-khach-hang/31-khach-hang.png', NULL, 31);

UPDATE ImagesAdver
SET StatusID = 2,
    Modified = @Now,
    Modifier = N'seed-homepage-customer-logos'
WHERE Subject = 4
  AND StatusID = 3
  AND SRC NOT IN (SELECT SRC FROM @Customers);

MERGE ImagesAdver AS target
USING @Customers AS source
ON target.Subject = 4
   AND target.LanguageID = 1
   AND target.SiteId = 1
   AND target.SRC = source.SRC
WHEN MATCHED THEN
    UPDATE SET
        target.Title = source.Title,
        target.URL = source.URL,
        target.StatusID = 3,
        target.Sort = source.Sort,
        target.Domain = N'techport.vn',
        target.Modified = @Now,
        target.Modifier = N'seed-homepage-customer-logos'
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Title, SRC, URL, Subject, StatusID, Sort, LanguageID, Domain, SiteId, Created, Creator)
    VALUES (source.Title, source.SRC, source.URL, 4, 3, source.Sort, 1, N'techport.vn', 1, @Now, N'seed-homepage-customer-logos');

COMMIT TRANSACTION;

SELECT ID, Title, SRC, Sort, StatusID
FROM ImagesAdver
WHERE Subject = 4
  AND StatusID = 3
ORDER BY Sort, ID;
