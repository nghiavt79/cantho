/*
Replaces the "Kiểm định, hiệu chuẩn, thử nghiệm, tư vấn, chứng nhận" service
category (CatId=29, under "Dịch vụ" / ParentId=2) with "Truy xuất nguồn gốc",
per the technology exchange platform redesign notes. No products are linked
to this category (verified via SanPhamCNTBCategory), so it is safe to rename
in place rather than insert a new row.
Idempotent — safe to re-run.
*/

UPDATE Category
SET Title = N'Truy xuất nguồn gốc',
    QueryString = 'truy-xuat-nguon-goc',
    Description = N'Dịch vụ hỗ trợ truy xuất nguồn gốc sản phẩm, xây dựng mã QR truy xuất, minh bạch hóa chuỗi cung ứng.'
WHERE CatId = 29;
GO
