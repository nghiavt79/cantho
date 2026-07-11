/*
Fills in QuyTrinhHinhAnh for 16 of the 27 published products that had no image,
using real product/company photos sourced from the web (official brand sites,
check.cantho.gov.vn official traceability portal, Báo Nhân Dân OCOP catalog,
food e-commerce listings). See chat for per-item sourcing notes and the 11
items left unresolved (Vidaca x2, Mắm cá lóc Út Anh, Nước ép ổi, Bánh tráng
ngọt Thới Lai, Gạo sạch Thới Lai, Mật ong Phong Điền, Bún khô Nhà Bè,
MISA eShop, MISA meInvoice, China Ecotek LED — no confident image found).
*/

SET QUOTED_IDENTIFIER ON;
GO

UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/06cfcb881071789a.jpg' WHERE ID = 35373;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/e91342c9f27c9d5e.jpg' WHERE ID = 35376;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/d70f09330cf29173.jpg' WHERE ID = 35377;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/4eeada6ac38d865b.jpg' WHERE ID = 35378;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/96abacb0cc414d75.jpg' WHERE ID = 35379;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/e79b83e069bd26cc.jpg' WHERE ID = 35380;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/293cff9e601724d4.webp' WHERE ID = 35381;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/666b2894b603a001.jpg' WHERE ID = 35382;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/8809865ab4765dea.jpg' WHERE ID = 35384;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/a88b146f3f75a404.jpg' WHERE ID = 35385;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/f2b7a9f7a22023bc.jpg' WHERE ID = 35390;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/c7901e4257283f79.jpg' WHERE ID = 35391;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/dd5f93a0e8560ac8.webp' WHERE ID = 35394;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/0e8bf9b716f41558.webp' WHERE ID = 35397;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/3bc5a93eb15a44cb.jpg' WHERE ID = 35398;
UPDATE SanPhamCNTB SET QuyTrinhHinhAnh = '/uploads/san-pham-web/4ba190b0fd234f35.jpg' WHERE ID = 35399;

SELECT ID, Name, QuyTrinhHinhAnh FROM SanPhamCNTB WHERE ID IN
    (35373,35376,35377,35378,35379,35380,35381,35382,35384,35385,35390,35391,35394,35397,35398,35399)
ORDER BY ID;
GO
