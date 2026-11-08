# Pilot: Dịch tiếng Anh cho CÁC DỮ LIỆU CÒN LẠI

**Phạm vi:** nhân pattern đã làm cho Tin/`Contents` sang 3 nhóm còn lại:
- `SanPhamCNTB`: Công nghệ (`ProductType=1`), Thiết bị (`ProductType=2`), Tài sản trí tuệ (`ProductType=3`)
- `NhaTuVan`: Chuyên gia / dịch vụ tư vấn
- `NhaCungUng`: Nhà cung ứng / dịch vụ cung ứng

**Nguyên tắc giữ nguyên:** dòng riêng `LanguageId=2` + `OriginalId`, URL `/en/...`, config ở `SYS_PARAMETERS`, dịch HTML theo node-level, không phá dữ liệu cũ. Phần dùng chung trong pilot Tin không làm lại: middleware `/en`, `LangHelper`, `ITranslationService`, factory provider, glossary, `TRANSLATION_AUTO_PUBLISH`.

---

## 1. Migration cho các bảng còn lại

Thêm 3 cột nullable giống `Contents`, nhưng kiểu `OriginalId` theo khóa chính từng bảng.

```sql
-- Sản phẩm / Công nghệ / Thiết bị / Tài sản trí tuệ
ALTER TABLE SanPhamCNTB ADD OriginalId int NULL;
ALTER TABLE SanPhamCNTB ADD EnStale bit NULL;
ALTER TABLE SanPhamCNTB ADD SourceHash varchar(64) NULL;
CREATE INDEX IX_SanPhamCNTB_Lang_Original ON SanPhamCNTB(LanguageId, OriginalId);

-- Chuyên gia / tư vấn
ALTER TABLE NhaTuVan ADD OriginalId int NULL;
ALTER TABLE NhaTuVan ADD EnStale bit NULL;
ALTER TABLE NhaTuVan ADD SourceHash varchar(64) NULL;
CREATE INDEX IX_NhaTuVan_Lang_Original ON NhaTuVan(LanguageId, OriginalId);

-- Nhà cung ứng
ALTER TABLE NhaCungUng ADD OriginalId int NULL;
ALTER TABLE NhaCungUng ADD EnStale bit NULL;
ALTER TABLE NhaCungUng ADD SourceHash varchar(64) NULL;
CREATE INDEX IX_NhaCungUng_Lang_Original ON NhaCungUng(LanguageId, OriginalId);
```

Entity cần thêm property:
- `SanPhamCNTB`: `int? OriginalId`, `bool? EnStale`, `string? SourceHash`
- `NhaTuVan`: `int? OriginalId`, `bool? EnStale`, `string? SourceHash`
- `NhaCungUng`: `int? OriginalId`, `bool? EnStale`, `string? SourceHash`

Bản VI giữ `LanguageId=1`, `OriginalId=NULL`. Bản EN sinh mới có `LanguageId=2`, `OriginalId` trỏ về ID bản VI gốc, có ID và `QueryString` tiếng Anh riêng.

---

## 2. Sản phẩm / Công nghệ / Thiết bị / Tài sản trí tuệ (`SanPhamCNTB`)

### 2.1 Phạm vi dữ liệu

Ba loại này dùng chung bảng `SanPhamCNTB`, phân biệt bằng `ProductType`:

| Loại | Điều kiện | URL VI hiện có | URL EN đề xuất |
|---|---:|---|---|
| Công nghệ | `ProductType=1` | `/cong-nghe` | `/en/technologies` |
| Thiết bị | `ProductType=2` | `/thiet-bi` | `/en/equipment` |
| Tài sản trí tuệ | `ProductType=3` | `/tai-san-tri-tue` | `/en/intellectual-property` |
| Chi tiết chung | theo `ID` | `/san-pham/chi-tiet/{slug}-{id}` | `/en/products/{slug}-{id}` |

`ProductType=4` (OCOP) không nằm trong scope proposal hiện tại; nếu khách muốn đưa OCOP lên site EN thì làm sau như một nhánh riêng vì có trường truy xuất nguồn gốc, thanh toán/đặt hàng khác.

### 2.2 Trường cần dịch

```text
Name
MoTaNgan
MoTa
ThongSo
UuDiem
GiaiThuong
Keywords
XuatXu
Khachhang
CoQuanChuTri
CoQuanChuQuan
LoaiDeTaiKhac
TransferMethodKhac
TargetCustomer
DevelopmentStage
CooperationGoal
CooperationType
GiaBanDuKien
ChiPhiPhatSinh
BaoHanhHoTro
ChungNhanKhacText
InvestmentGoalKhac
NDAContent
```

Các trường HTML/CKEditor cần dịch node-level: `MoTa`, `ThongSo`, `UuDiem`, `GiaiThuong`, `GiaBanDuKien`, `ChiPhiPhatSinh`, `BaoHanhHoTro`, `NDAContent`.

Không dịch/copy nguyên: `Code`, ảnh/video/file (`QuyTrinhHinhAnh`, `BrochureUrl`, `URL`, `WebUrl`), giá, số liệu, ngày cấp bằng, số bằng, `NCUId`, `CategoryId`, `TRLLevel`, `ProductType`, `RequiresNDA`, `CanDirectInquiry`, trạng thái, thống kê view/rating.

### 2.3 Routing

Thêm route EN vào `Program.cs` trước route VI:

```csharp
app.MapControllerRoute("en_products_all", "en/products",
    new { controller = "Product", action = "TatCaSanPham" });
app.MapControllerRoute("en_products_tech", "en/technologies",
    new { controller = "Product", action = "CongNghe" });
app.MapControllerRoute("en_products_equipment", "en/equipment",
    new { controller = "Product", action = "ThietBi" });
app.MapControllerRoute("en_products_ip", "en/intellectual-property",
    new { controller = "Product", action = "TaiSanTriTue" });
app.MapControllerRoute("en_product_detail", "en/products/{slug}-{id:int}",
    new { controller = "Product", action = "Detail" });
app.MapControllerRoute("en_product_category", "en/products/category/{slug}-{cateId:int}",
    new { controller = "Product", action = "ProductByCate" });
```

### 2.4 Sửa đọc dữ liệu

- `ProductController.TatCaSanPham`: thêm lọc `LanguageId == LangHelper.CurrentLangId(HttpContext)`.
- `BuildIndexByProductTypeAsync`: thay session bằng `LangHelper.CurrentLangId(HttpContext)`; các count nên tính theo lang hiện tại để số liệu tab không lệch.
- `Detail`: guard `if (p.LanguageId != currentLang) return NotFound();`.
- Sản phẩm liên quan / sản phẩm cùng nhà cung ứng: lọc cùng `LanguageId`; nếu trang EN chưa có bản EN tương ứng thì không hiển thị hoặc fallback có nhãn.
- URL sinh trong card: EN dùng `/en/products/{QueryString}-{ID}`, VI giữ `/san-pham/chi-tiet/...`.

### 2.5 CMS bulk translate

Trong `TranslationAdminController` thêm action:

```csharp
BulkTranslateProducts(int[] ids, CancellationToken ct)
```

Logic:
- Chỉ nhận bản VI: `SanPhamCNTB.ID in ids && LanguageId == 1`.
- Bỏ qua nếu đã có `SanPhamCNTB.OriginalId == id && LanguageId == 2`.
- Dịch trường text theo danh sách 2.2.
- Tạo dòng EN mới, copy trường phi ngôn ngữ, giữ `ProductType`, `CategoryId`, `NCUId`, giá, ảnh, file.
- Sinh `QueryString = SlugHelper.Slugify(NameEn)`.
- `StatusId = 3` nếu `TRANSLATION_AUTO_PUBLISH=1`, ngược lại `StatusId=1`.
- `SourceHash` hash từ các trường text đã dịch.

Trên 3 list CMS `CongNghe`, `ThietBi`, `SanPhamTriTue`: thêm checkbox, nút **"Dịch sang tiếng Anh"**, cột **Trạng thái EN**: Chưa có EN / Auto / Đã duyệt / VI đã đổi.

### 2.6 Lưu ý quan hệ nhà cung ứng

`SanPhamCNTB.NCUId` hiện trỏ sang `NhaCungUng.CungUngId` bản VI. Có 2 cách:

- Giai đoạn pilot: giữ `NCUId` trỏ bản VI để không đứt quan hệ nội bộ, nhưng khi render trang EN thì ưu tiên tìm `NhaCungUng` EN bằng `OriginalId = NCUId`. Không có EN thì hiển thị tên VI kèm nhãn fallback.
- Giai đoạn hoàn thiện: sau khi dịch nhà cung ứng, chạy job map lại hoặc helper `FindEnglishSupplierId(viCungUngId)`.

Khuyến nghị dùng helper thay vì sửa `NCUId` trực tiếp, vì một nhà cung ứng có thể chưa được chọn đưa lên site EN.

---

## 3. Chuyên gia / tư vấn (`NhaTuVan`)

### 3.1 Trường cần dịch

```text
FullName
HocHam
CoQuan
ChucVu
DichVu
KetQuaNghienCuu
QuaTrinhDaoTao
QuaTrinhCongTac
CongBoKhoaHoc
SangChe
DuAnNghienCuu
KinhNghiem
HiepHoiKhoaHoc
Keywords
```

Các trường HTML/bảng cần node-level: `KetQuaNghienCuu`, `QuaTrinhDaoTao`, `QuaTrinhCongTac`, `CongBoKhoaHoc`, `SangChe`, `DuAnNghienCuu`, `KinhNghiem`.

Không dịch/copy nguyên: email, phone, ngày sinh, địa chỉ nếu khách không yêu cầu, ảnh, `MaDinhDanh`, `TongTrichDan`, `HIndex`, `HoSoDinhKem`, `LinhVucId`, `UserId`, rating/view.

`DichVu` đang là chuỗi ID danh mục dạng `;id1;id2;`, không dịch giá trị này như text tự do. Khi render EN, controller phải resolve title danh mục theo `LanguageId=2`. Nếu danh mục chưa có EN thì dùng `Category.TitleEn` hoặc fallback VI.

### 3.2 Routing

```csharp
app.MapControllerRoute("en_experts", "en/experts",
    new { controller = "ChuyenGia", action = "Index" });
app.MapControllerRoute("en_expert_detail", "en/experts/{slug}-{id:int}",
    new { controller = "ChuyenGia", action = "Detail" });
```

### 3.3 Sửa đọc dữ liệu

`ChuyenGiaController` hiện đang có `private const int LangId = 1`; phải bỏ const này và thay bằng:

```csharp
var langId = LangHelper.CurrentLangId(HttpContext);
```

Áp dụng cho:
- list chuyên gia
- chi tiết chuyên gia
- sidebar chuyên gia khác
- resolve `DichVu` và `LinhVucId`
- category sidebar

Detail guard: `TuVanId == id && LanguageId == langId && StatusId == 3`.

URL card: EN dùng `/en/experts/{QueryString}-{TuVanId}`, VI giữ `/chuyen-gia/{QueryString}-{TuVanId}`.

### 3.4 CMS bulk translate

Thêm:

```csharp
BulkTranslateConsultants(int[] ids, CancellationToken ct)
```

Logic:
- Chỉ lấy bản VI `LanguageId == 1`.
- Bỏ qua nếu có bản EN `OriginalId == TuVanId && LanguageId == 2`.
- Dịch các trường text/HTML ở 3.1.
- Copy field định danh/liên hệ/ảnh/số liệu.
- Tạo `QueryString` từ `FullName` EN. Riêng tên người Việt có thể giữ nguyên dấu hoặc transliterate theo chính sách SEO; khuyến nghị slug ASCII không dấu để ổn định URL.
- `StatusId` theo `TRANSLATION_AUTO_PUBLISH`.

Trên list CMS `NhaTuVanAdmin`: thêm checkbox, nút bulk, cột trạng thái EN. Màn sửa bản EN hiển thị link **"Bản gốc VI"**.

---

## 4. Nhà cung ứng (`NhaCungUng`)

### 4.1 Trường cần dịch

```text
FullName
DiaChi
NguoiDaiDien
ChucVu
ChucNangChinh
DichVu
SanPham
TenVietTat
ChungNhan
Keywords
```

HTML/node-level: `ChucNangChinh`, `SanPham`, `ChungNhan`.

Không dịch/copy nguyên: phone, email, fax, website, logo, video, mã số thuế, tài khoản ngân hàng, `LoaiHinhToChuc` code, `LinhVucId`, `UserId`, rating/view.

`DichVu` và `LinhVucId` là chuỗi ID danh mục; giữ nguyên ID, khi render EN resolve title danh mục theo `LanguageId=2` hoặc fallback `TitleEn`.

### 4.2 Routing

```csharp
app.MapControllerRoute("en_suppliers", "en/suppliers",
    new { controller = "NhaCungUng", action = "Index" });
app.MapControllerRoute("en_supplier_detail", "en/suppliers/{slug}-{id:int}",
    new { controller = "NhaCungUng", action = "Detail" });
```

### 4.3 Sửa đọc dữ liệu

`NhaCungUngController` hiện cũng đang có `private const int LangId = 1`; thay bằng `LangHelper.CurrentLangId(HttpContext)`.

Áp dụng cho:
- list nhà cung ứng
- detail
- sidebar nhà cung ứng khác
- category sidebar
- resolve `DichVu`/`LinhVucId`

Detail guard: `CungUngId == id && LanguageId == langId && IsActivated == true`.

Danh sách sản phẩm của nhà cung ứng trên trang EN:
- Nếu đang xem bản EN của nhà cung ứng, cần lấy sản phẩm EN có `OriginalId` thuộc các sản phẩm VI của nhà cung ứng gốc.
- Cách pilot đơn giản:
  1. Lấy `originSupplierId = entity.OriginalId ?? entity.CungUngId`.
  2. Lấy sản phẩm VI `NCUId == originSupplierId`.
  3. Map sang sản phẩm EN bằng `SanPhamCNTB.OriginalId`.
  4. Chỉ hiển thị sản phẩm EN đã publish.

### 4.4 CMS bulk translate

Thêm:

```csharp
BulkTranslateSuppliers(int[] ids, CancellationToken ct)
```

Logic:
- Chỉ lấy bản VI `LanguageId == 1`.
- Bỏ qua nếu đã có EN `OriginalId == CungUngId && LanguageId == 2`.
- Dịch text/HTML ở 4.1.
- Copy liên hệ, mã số thuế, tài khoản, logo, video, `LoaiHinhToChuc`, `LinhVucId`.
- Tạo `QueryString` từ `FullName` EN.
- Set `IsActivated` theo `TRANSLATION_AUTO_PUBLISH`: nếu auto publish thì giữ trạng thái kích hoạt như bản VI; nếu không thì tạo nháp/chưa kích hoạt để duyệt.

Trên list CMS `NhaCungUngAdmin`: thêm checkbox, nút bulk, cột trạng thái EN. Màn sửa bản EN hiển thị link **"Bản gốc VI"**.

---

## 5. Cập nhật `SourceHash` và cờ stale

Khi sửa bản VI trong CMS:

- `SanPhamCNTBController.ApplyEdit` / action edit: sau khi lưu VI, tìm EN `OriginalId == ID && LanguageId == 2`; nếu hash mới khác `SourceHash` thì set `EnStale = true`.
- `NhaTuVanAdminController.Edit`: tương tự theo `TuVanId`.
- `NhaCungUngAdminController.Edit`: tương tự theo `CungUngId`.

Không tự đè bản EN đã duyệt. Chỉ hiện cảnh báo **"Bản gốc VI đã đổi - dịch lại?"** ở list và màn sửa EN.

---

## 6. Thứ tự làm sau pilot Tin

1. Migration 3 bảng + thêm property entity.
2. Thêm route EN cho products/experts/suppliers.
3. Sửa controller public đọc `LangHelper` thay vì session/const `LangId=1`.
4. Thêm 3 action bulk trong `TranslationAdminController`.
5. Thêm checkbox/nút/cột trạng thái EN trên các list CMS:
   - `SanPhamCNTBController`: `CongNghe`, `ThietBi`, `SanPhamTriTue`
   - `NhaTuVanAdminController`
   - `NhaCungUngAdminController`
6. Test trên môi trường test:
   - 3 công nghệ
   - 3 thiết bị
   - 3 tài sản trí tuệ
   - 3 chuyên gia
   - 3 nhà cung ứng
7. Review URL EN:
   - `/en/technologies`
   - `/en/equipment`
   - `/en/intellectual-property`
   - `/en/products/{slug}-{id}`
   - `/en/experts`
   - `/en/experts/{slug}-{id}`
   - `/en/suppliers`
   - `/en/suppliers/{slug}-{id}`

---

## 7. Ghi chú chốt

- Công nghệ, Thiết bị, Tài sản trí tuệ không cần 3 cơ chế dịch khác nhau; chỉ là 3 filter `ProductType` trên `SanPhamCNTB`.
- Nhà cung ứng và chuyên gia có nhiều trường danh mục dạng ID; không dịch chuỗi ID, chỉ đổi cách resolve title theo ngôn ngữ.
- Quan hệ sản phẩm ↔ nhà cung ứng phải xử lý bằng `OriginalId` khi render EN, tránh sửa trực tiếp khóa `NCUId` của dữ liệu gốc.
- Các trường HTML lớn phải đi qua node-level parser; không gửi nguyên HTML cho model.
