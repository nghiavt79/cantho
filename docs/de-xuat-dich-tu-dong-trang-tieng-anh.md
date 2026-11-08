# Đề xuất: Dịch tự động / bán tự động trang tiếng Anh

**Dự án:** Sàn Giao dịch Công nghệ (TechExchangeApp)
**Ngày:** 31/07/2026 · **Phiên bản:** v5 (chốt config **để toàn bộ ở SYS_PARAMETERS** — provider/key/cờ auto-publish, dùng màn hình tham số có sẵn, mở cột `Val`. Giữ: dòng riêng `LanguageId=2` + `OriginalId` cho site EN chọn lọc, phạm vi 4 bảng `SanPhamCNTB`/`Contents`/`NhaTuVan`/`NhaCungUng`; SEO `/en/`; chống vỡ HTML node-level; glossary động; bảo vệ bản duyệt; chi phí phạm vi chọn ~$60 Sonnet)
**Bối cảnh:** Khách hàng thiếu nhân sự biên tập nội dung tiếng Anh → cần giải pháp để **máy dịch tự động**, người chỉ duyệt lại (hoặc không duyệt).

---

## 1. Hiện trạng trang tiếng Anh

Trang **đã có khung chuyển ngôn ngữ VI/EN nhưng mới hoàn thiện một phần** — chỉ dịch được giao diện tĩnh, còn toàn bộ **nội dung động (sản phẩm, tin tức, nhà cung ứng, tư vấn…) luôn hiển thị tiếng Việt** dù người dùng bấm EN.

### Cơ chế chuyển ngôn ngữ
- Nút VI/EN gọi `LanguageController.SetLanguage` → ghi cookie `site_lang = en/vi` (hạn 1 năm).

### 3 lớp nội dung
| Lớp | Tình trạng | Cách biên tập hiện tại |
|-----|-----------|------------------------|
| Chuỗi giao diện tĩnh | Dịch một phần (`ui.json`, ~20 khóa, dùng ở 5 view) | Sửa tay file `Localization/ui.json` |
| Section trang chủ + Header/Footer | Dịch bằng chuỗi viết cứng trong C#/cshtml | Sửa code, build lại |
| **Nội dung từ CSDL** (sản phẩm, tin, tư vấn) | **KHÔNG dịch** | **Chưa có công cụ nào** |

### Vì sao nội dung CSDL luôn ra tiếng Việt
- Mọi truy vấn lọc theo `LanguageId` lấy từ `Session.GetInt32("LanguageId") ?? 1`.
- **Không chỗ nào trong code gọi `Session.SetInt32("LanguageId", …)`** → biến này luôn = 1 (tiếng Việt). Cookie `site_lang=en` **không** đổi `LanguageId`.
- Form CMS lưu sản phẩm/tin **ghi cứng `LanguageId = 1`** → không có cách tạo bản EN.

---

## 2. Hướng lưu bản dịch: DÒNG RIÊNG (LanguageId=2) + `OriginalId`

> ⚠️ **Sửa lại (v4):** site EN là **nội dung chọn lọc, không ánh xạ 1-1** với site VI. Do đó bỏ hướng "cột `*_En` cùng dòng" (v3) — nó ép mọi bản ghi VI phải có ô EN. Cũng không cần bảng ánh xạ.

**Mô hình:**
- Bản EN = **dòng riêng** (`LanguageId=2`), **chỉ tạo cho mục được chọn** đưa lên tiếng Anh → không dịch thừa, nội dung EN có thể khác/gọn hơn VI.
- Thêm **đúng 1 cột** `OriginalId int NULL` → trỏ về id gốc tiếng Việt (đây là cột liên kết trước đây thiếu).
- `OriginalId = null` → nội dung **viết thẳng bằng tiếng Anh** (không dịch từ VI nào).

```
VI  id=10   LanguageId=1   Name="Máy bơm nước"
EN  id=105  LanguageId=2   Name="Water pump"   OriginalId=10   (+ EnStale, SourceHash)
EN  id=112  LanguageId=2   Name="Company profile"  OriginalId=null   (bài EN gốc)
```

- **Schema thay đổi tối thiểu:** 1 cột `OriginalId` (+ vài cột cờ đồng bộ) trên các bảng nội dung. Nhẹ hơn nhiều so với loạt cột `*_En`.
- **Hybrid hợp lý:** Danh mục/Menu (cấu trúc, gần 1-1) dùng cột cùng dòng — `Category` **đã có** `TitleEn`/`ParentIdEN`. Nội dung (chọn lọc) dùng dòng riêng + `OriginalId`. Mỗi loại hợp một cách.

---

## 3. Phạm vi: 4 bảng nội dung (thêm `OriginalId` + dịch)

Chỉ tập trung 4 bảng nội dung chọn lọc dưới đây. **Menu/Category KHÔNG đụng** — đã dùng `TitleEn` cùng dòng, gần 1-1, để nguyên.

| Bảng | Khóa chính | Trường cần dịch | Ghi chú |
|------|-----------|-----------------|---------|
| `SanPhamCNTB` (sản phẩm) | `ID` | `Name`, `MoTaNgan`, `MoTa`, `ThongSo`, `UuDiem`, `GiaiThuong` | `MoTa/ThongSo/UuDiem` là HTML |
| `Contents` (tin/bài) | `Id` | `Title`, `Description`, `Contents`, `Keyword` | `Contents` là HTML |
| `NhaTuVan` (chuyên gia / dịch vụ tư vấn) | `TuVanId` | `FullName`, `HocHam`, `CoQuan`, `ChucVu`, `DichVu`, `KetQuaNghienCuu`, `QuaTrinhDaoTao`, `QuaTrinhCongTac`, `CongBoKhoaHoc`, `SangChe`, `DuAnNghienCuu`, `KinhNghiem`, `HiepHoiKhoaHoc` | nhiều trường HTML (bảng) |
| `NhaCungUng` (dịch vụ cung ứng) | `CungUngId` | `FullName`, `ChucNangChinh`, `DichVu`, `SanPham`, `ChucVu`, `TenVietTat`, `ChungNhan` | `ChungNhan` là HTML |

+ Giao diện tĩnh: bổ sung khóa còn thiếu trong `ui.json` (không thuộc 4 bảng trên).

**Lưu ý dòng riêng:** mỗi bảng thêm `OriginalId` trỏ về **chính khóa của bảng đó** (EN của sản phẩm → id sản phẩm VI; EN của chuyên gia → `TuVanId` VI…). Các trường phi văn bản (ảnh, phone, `Rating`, `Viewed`) khi tạo dòng EN thì **sao chép/tham chiếu**, không dịch; số liệu `Rating`/`Viewed` có thể gộp theo `OriginalId` nếu muốn thống kê chung.

---

## 4. Điểm phải làm rõ trước khi duyệt (đã kiểm chứng trong code)

Phần này ghi lại các lỗ hổng đã được phản biện và trả lời bằng dữ liệu thật, **không nói chung chung**.

### 4.1 Cơ chế xác định ngôn ngữ — PHẢI dùng URL, không dùng session (vấn đề SEO)
Hiện trạng đã kiểm tra trong [Program.cs](../TechExchangeApp/Program.cs):
- `Session.IdleTimeout = 30 phút`, cookie `site_lang` sống 1 năm → **không khớp**: tắt browser mở lại thì mất ngôn ngữ.
- **Không có middleware đọc cookie → session**, và không chỗ nào set `Session["LanguageId"]`. Nên việc "chọn ngôn ngữ" **không phải sửa 1 chỗ** — phải thêm một lớp middleware chạy trên mọi request.
- **Toàn bộ ~60 route là slug tiếng Việt, không có cấu trúc `/en/`.** Googlebot không giữ cookie/session → nếu ngôn ngữ dựa vào session, bot **luôn nhận tiếng Việt**, bản EN **không bao giờ được index**. Điều này phá đúng mục tiêu của khách (thu hút đối tác nước ngoài).

➡️ **Quyết định kiến trúc:** ngôn ngữ xác định bằng **URL** — prefix `/en/` cho bản EN (route VI giữ nguyên), kèm thẻ `hreflang` để Google hiểu hai bản là song ngữ. Đây là **khối việc lớn** (đụng toàn bộ bảng route), không phải "giai đoạn nhẹ".

### 4.2 Chất lượng bản "auto" — có nhãn cảnh báo + có preview
- Bản `auto` (chưa duyệt) **hiển thị kèm banner "Machine-translated — may contain errors"**, gỡ banner khi được đánh dấu `reviewed`.
- **Trường nhạy cảm** (`ThongSo`, `GiaiThuong`, tên riêng, đơn vị đo): khi chưa duyệt thì **ẩn bản dịch máy hoặc kèm nguyên văn tiếng Việt**, không để dịch sai thông tin sản phẩm lên công khai.
- **Bắt buộc có luồng preview/staging**: dịch → xem thử trên môi trường test → khách duyệt mẫu → mới đẩy lên production.

### 4.3 Glossary — có kế hoạch cụ thể, không nói một câu
- **Ai xây:** đội dev tạo bản nháp từ danh mục thuật ngữ sẵn có (CNTB, OCOP, chuyển giao công nghệ, tên đơn vị/địa danh), **khách/chuyên gia lĩnh vực duyệt** danh sách.
- **Quy mô:** bắt đầu ~50–100 thuật ngữ cốt lõi + tên riêng, mở rộng dần.
- Glossary được nạp vào mỗi lần dịch để đảm bảo nhất quán; là file version-controlled, sửa được không cần build lại.

### 4.4 Đồng bộ khi sửa VI — cơ chế rõ ràng
- Sửa bản VI → đánh dấu bản EN `NeedsRetranslation` + cập nhật `SourceModifiedAt`.
- **Job nền chạy cron mỗi ~5 phút**, **gộp** nhiều lần sửa trong khoảng đó thành **1 lần dịch** (debounce) → sửa VI 5 lần/ngày **không** tốn 5 lần phí.
- Độ trễ tối đa bản EN cập nhật: ~5 phút. Trong lúc chờ, trang EN hiển thị bản cũ kèm nhãn "updating".

### 4.5 Ai duyệt? — nói thẳng rủi ro
Bối cảnh: **khách thiếu nhân sự EN**, nên thực tế **phần lớn nội dung sẽ ở trạng thái `auto` vĩnh viễn**. Đề xuất **không mập mờ**: nếu không có người duyệt, khách **chấp nhận rủi ro nội dung dịch máy sai hiển thị công khai** — được giảm thiểu bằng nhãn cảnh báo (4.2), glossary (4.3) và chọn nhà cung cấp chất lượng cao. Đây là điều khoản cần khách xác nhận, không phải "tuỳ chọn để đó".

---

## 4bis. Thiết kế kỹ thuật chi tiết (trả lời 5 rủi ro vận hành)

### ① Nút chuyển ngôn ngữ — quăng về trang chủ, KHÔNG tra cặp bài
- Bấm **EN** → về `/en` (trang chủ tiếng Anh) + set ngôn ngữ EN. Bấm **VI** → về `/` + set VI.
- **Nút luôn hiện**, không `FindCounterpart`, không ẩn nút, **không lo 404** — vì không nhảy sang bài đối ứng.
- Site EN là site riêng: khách vào cửa chính rồi tự duyệt trong đó (đúng bản chất chọn lọc, không mirror).
- `OriginalId` **KHÔNG dùng cho nút chuyển ngôn ngữ** — chỉ dùng phía CMS (xem ③).

### ② Routing SEO — prefix `/en/`, không đập Controller
- Nhân đôi bảng route trong [Program.cs](../TechExchangeApp/Program.cs) với prefix `en/`, **trỏ về đúng Controller/Action hiện tại** (route VI giữ nguyên).
- Middleware nhẹ đọc segment đầu `/en/` → `HttpContext.Items["Lang"]="en"`. Tầng đọc dữ liệu lọc `LanguageId=2` thay vì `=1`. Bản EN có **slug tiếng Anh riêng** (`/en/products/water-pump-105`) → SEO tốt hơn.
- Thêm `<link rel="alternate" hreflang="en|vi">` (dùng `OriginalId` để sinh URL đối ứng) → Google index được bản EN.

### ③ Bảo vệ bản đã duyệt — không bao giờ đè *(dùng `OriginalId` phía CMS)*
- Bản EN là **dòng độc lập** → sửa VI **không đụng** tới nội dung EN.
- `OriginalId` dùng ở đây để: (a) biết bản EN dịch từ VI nào → tránh dịch trùng; (b) cảnh báo lệch.
- Cột cờ: `SourceHash` (hash bản VI lúc dịch), `EnStale` (VI đã đổi?).
- Sửa VI → so hash qua `OriginalId`: khác → set `EnStale=true`, hiện gợi ý **"Bản gốc VI đã đổi — dịch lại?"** kèm **diff VI cũ↔mới** + preview. **Người quyết, máy không tự đè.**
- **Giữ `OriginalId`** (đã chốt) — để admin biết bản EN dịch từ mục VI nào; hiển thị trên màn hình sửa bản EN (link về bản gốc).

### ④ Chống vỡ HTML — 2 lớp
- **Phòng ngừa cấu trúc:** KHÔNG gửi HTML thô cho LLM. Dùng HtmlAgilityPack/AngleSharp **tách text node**, chỉ dịch chữ, **ghép lại vào DOM gốc**. Thẻ/class/thuộc tính không đi qua model → không thể thiếu thẻ, dịch tên class, hay chèn câu rác.
- **Validate trước khi lưu:** parse lại, kiểm tra cấu trúc khớp gốc. Lỗi → **không lưu**, giữ VI làm fallback, gắn cờ xử lý tay.
- **Fallback:** trang EN chưa có bản dịch → hiển thị nội dung VI kèm nhãn, **không ra trang vỡ**.

### ⑤ Glossary động — có màn hình CMS
- Bảng `TranslationGlossary` (`TermVi`, `TermEn`, `Domain`, `Note`, `Active`) + **màn hình CMS "Quản lý Từ điển"** (CRUD) để tự thêm/sửa, không cần gọi dev.
- Nạp vào mỗi lần dịch để ép nhất quán; có hậu kiểm thay thế thuật ngữ.

### ⑥ Config provider + key + cờ duyệt — GOM VÀO SYS_PARAMETERS (dùng màn hình có sẵn)

Màn hình `SystemParameterAdmin` **đã có** cơ chế nhạy cảm: key tên chứa `API_KEY`/`TOKEN`/`SECRET` → tự **che giá trị** + **không ghi log** ([SystemParameterAdminController.cs:20,240,259](../TechExchangeApp/Areas/Cms/Controllers/SystemParameterAdminController.cs#L20)). → Không cần dựng màn hình config riêng, không cần bảng `TranslationSetting`.

| Key (thêm vào `ParameterKeys.cs`) | Val | Ghi chú |
|---|---|---|
| `TRANSLATION_PROVIDER` | haiku / sonnet / google | chọn nhà cung cấp |
| `TRANSLATION_AUTO_PUBLISH` | 1 / 0 | 1=hiển thị auto ngay; 0=cần duyệt mới lên (mặc định) |
| `TRANSLATION_ANTHROPIC_API_KEY` | key | nhạy cảm — tự mask/no-log (Haiku+Sonnet chung) |
| `TRANSLATION_GOOGLE_API_KEY` | key | nhạy cảm |

- **Mở cột:** `ALTER COLUMN Val nvarchar(500)` (hoặc `max` nếu mã hóa `IDataProtector`) + sửa `[StringLength]` entity (key ~100-200 ký tự, `Val` hiện chỉ 50).
- **Cần hoàn thiện:** `NormalizeValue` ([dòng 198](../TechExchangeApp/Areas/Cms/Controllers/SystemParameterAdminController.cs#L198)) hiện chưa dùng `currentValue` → thêm logic **"nhập trống = giữ key cũ"** (write-only) khi dùng cho key.
- **Tùy chọn nâng cấp:** mã hóa key bằng `IDataProtector` (DataProtection đã bật [Program.cs:15](../TechExchangeApp/Program.cs#L15)) → DB không chứa plaintext (cần `nvarchar(max)`).
- **Cờ auto-publish runtime:** `paramSvc.GetIntAsync(ParameterKeys.TranslationAutoPublish, 0)` — đổi tức thì, không build lại.
- **Runtime provider:** `ITranslationService` sau **factory** — đọc `TRANSLATION_PROVIDER` + key (thứ tự env var → appsettings → SYS_PARAMETERS) → khởi tạo `ClaudeTranslator`(haiku/sonnet) hoặc `GoogleTranslator`. Đổi provider = sửa tham số + lưu.
- **Admin tự nhập key của mình** (không nhập hộ).

---

## 5. Chọn nhà cung cấp dịch + chi phí thật

### Giá đơn vị (bảng giá chính thức, đã tra cứu)
| Nhà cung cấp | Giá | Ghi chú |
|---|---|---|
| **Google Cloud Translation** | **~$20 / 1 triệu ký tự** | rẻ cho khối lượng lớn, có glossary |
| **Claude Haiku** | $1 input / $5 output / 1 triệu **token** | LLM rẻ nhất |
| **Claude Sonnet** | $3 / $15 / 1 triệu token | chất lượng cao |
| **Claude Opus** | $5 / $25 / 1 triệu token | chất lượng cao nhất |

### Số liệu THẬT (đã đo trên DB `TechExchangeNew` ngày 31/07/2026)

| Loại | Số bản ghi | Tổng ký tự |
|---|---|---|
| Sản phẩm | 4.334 | 69.932.434 |
| Tin/bài | 50 | 176.869 |
| Chuyên gia | 12 | 44.199 |
| Dịch vụ cung ứng | 77 | 15.786 |
| **Tổng** | **4.473** | **~70,17 triệu ký tự** |

> ⚠️ **Dữ liệu sản phẩm bị phồng:** 1 sản phẩm nặng **2,8 triệu ký tự** (ảnh base64/HTML rác); **121 sản phẩm chiếm 38%** tổng dung lượng; trung vị thực chỉ ~6.280 ký tự/sản phẩm. Dịch node-level tự bỏ phần rác này. **Nên dọn rác trước khi dịch** (giảm ~38% chi phí + tốt cho tốc độ tải).

### Chi phí dịch TOÀN BỘ hiện có (một lần, rồi cache)

**Kịch bản A — dịch mù cả 70,17 triệu ký tự (chặn trên):**
| Nhà cung cấp | Chi phí một lần |
|---|---|
| **Claude Haiku** | **~$140** (~3,5 triệu đ) |
| Claude Sonnet | ~$420 (~10,5 triệu đ) |
| Claude Opus | ~$690 (~17 triệu đ) |
| Google Translate | ~$1.400 (~35 triệu đ) |

**Kịch bản B — dọn rác + chỉ dịch text thật (ước ~25–30 triệu ký tự):**
| Nhà cung cấp | Chi phí một lần |
|---|---|
| **Claude Haiku** | **~$55–65** |
| Claude Sonnet | ~$170–200 |
| Google Translate | ~$500–600 |

*Ước LLM theo ~3 ký tự/token (tiếng Việt), output EN ~tương đương, sai số ±20%. Google tính theo ký tự nguồn.*

**Khuyến nghị:** không nên dịch mù 4.334 sản phẩm (phần lớn là hàng cũ/rác). Dịch **có lọc** (đang bán / nổi bật / theo danh mục) đúng tinh thần chọn lọc v4 → chi phí chỉ **vài chục đô**, đúng đối tượng cần cho đối tác nước ngoài.

### Phạm vi khách chốt (chọn dịch ở trang list) — chi phí thật

| Nhóm | Số lượng | Ký tự (đo/ước) |
|---|---|---|
| Sản phẩm (chọn lọc) | ~500 | 5–17 triệu (tùy hàng thường/chi tiết; SP sạch avg 10.350 kt) |
| Tin bài | ~100 | ~0,35 triệu (⚠️ DB hiện chỉ có **50 tin** LanguageId=1) |
| Nhà cung ứng | 77 (toàn bộ) | 0,016 triệu |
| Tư vấn | 12 (toàn bộ) | 0,044 triệu |
| **Tổng (ước giữa)** | ~589 | **~10 triệu ký tự** |

**Chi phí một lần:** Haiku **~$20** · Sonnet **~$60** · Google ~$200 (khoảng: Haiku $11–35, Sonnet $32–104). Khuyến nghị **Sonnet ~$60 (~1,5 triệu đ)** cho chất lượng chuyên ngành.

**Cơ chế chọn dịch (trang list CMS):** checkbox chọn nhiều dòng + nút "Dịch sang tiếng Anh" (bulk) trên list của từng loại → tạo dòng EN (`LanguageId=2`+`OriginalId`), dịch node-level, cờ `auto`. Cột trạng thái trên list: *Chưa có EN / Auto / Đã duyệt / VI đã đổi*.

**Cần xác nhận:** (1) 100 tin — DB hiện chỉ 50, viết thêm hay dịch 50 đang có? (2) "Nhà cung ứng liên quan" = toàn bộ 77 hay chỉ những NCU gắn với 500 SP đã chọn (`NCUId`)?

### Câu SQL đo (đã chạy — lưu để đo lại sau khi dọn dữ liệu)
```sql
SELECT
  -- Sản phẩm
  (SELECT COUNT(*) FROM SanPhamCNTB WHERE LanguageId = 1) AS SoSanPham,
  (SELECT SUM(LEN(ISNULL(Name,'')) + LEN(ISNULL(MoTaNgan,'')) + LEN(ISNULL(MoTa,'')) + LEN(ISNULL(ThongSo,'')) + LEN(ISNULL(UuDiem,'')) + LEN(ISNULL(GiaiThuong,''))) FROM SanPhamCNTB WHERE LanguageId = 1) AS KyTuSanPham,
  -- Tin/bài
  (SELECT COUNT(*) FROM Contents WHERE LanguageId = 1) AS SoTin,
  (SELECT SUM(LEN(ISNULL(Title,'')) + LEN(ISNULL(Description,'')) + LEN(ISNULL(Contents,''))) FROM Contents WHERE LanguageId = 1) AS KyTuTin,
  -- Chuyên gia / dịch vụ tư vấn
  (SELECT COUNT(*) FROM NhaTuVan WHERE LanguageId = 1) AS SoChuyenGia,
  (SELECT SUM(LEN(ISNULL(FullName,'')) + LEN(ISNULL(DichVu,'')) + LEN(ISNULL(KetQuaNghienCuu,'')) + LEN(ISNULL(QuaTrinhDaoTao,'')) + LEN(ISNULL(QuaTrinhCongTac,'')) + LEN(ISNULL(CongBoKhoaHoc,'')) + LEN(ISNULL(SangChe,'')) + LEN(ISNULL(DuAnNghienCuu,'')) + LEN(ISNULL(KinhNghiem,''))) FROM NhaTuVan WHERE LanguageId = 1) AS KyTuChuyenGia,
  -- Dịch vụ cung ứng
  (SELECT COUNT(*) FROM NhaCungUng WHERE LanguageId = 1) AS SoCungUng,
  (SELECT SUM(LEN(ISNULL(FullName,'')) + LEN(ISNULL(ChucNangChinh,'')) + LEN(ISNULL(DichVu,'')) + LEN(ISNULL(SanPham,'')) + LEN(ISNULL(ChungNhan,''))) FROM NhaCungUng WHERE LanguageId = 1) AS KyTuCungUng;
```
Có 4 con số này → tính được chi phí lô đầu chính xác (± vài %) và ước lượng thời gian chạy job.

> Phần dịch đặt sau interface `ITranslationService` → đổi nhà cung cấp về sau chỉ là thay 1 lớp cài đặt.

**Khuyến nghị:** **hybrid** — Claude cho tiêu đề/mô tả/thông số quan trọng (chất lượng), Google cho đoạn dài để tiết kiệm; hoặc dùng Haiku toàn bộ nếu ưu tiên chi phí.

---

## 6. Kế hoạch triển khai + timeline (ước lượng, chốt lại sau khi đo)

| GĐ | Nội dung | Ước lượng | Rủi ro dữ liệu |
|---|---|---|---|
| **1. Định tuyến ngôn ngữ theo URL** | Prefix `/en/` cho toàn bộ route + middleware suy ngôn ngữ từ URL + `hreflang`. Đây là phần SEO, khối lượng lớn nhất. | ~5–8 ngày | Thấp (chỉ đụng route/middleware) |
| **2. Hạ tầng dịch + cột `OriginalId`** | Thêm cột `OriginalId` + `EnStale`/`SourceHash`; `ITranslationService` + **factory chọn provider**; config gom vào **SYS_PARAMETERS** (mở cột `Val` + 4 key TRANSLATION_*, tận dụng màn hình có sẵn; hoàn thiện write-only cho key); cờ `TRANSLATION_AUTO_PUBLISH`; dịch **node-level HTML** + validate; **Glossary động**; helper `FindCounterpart` | ~6–8 ngày | Thấp (thêm cột, không phá dữ liệu) |
| **3. Công cụ CMS + nhãn + chọn lọc EN** | Nút "Tạo/Dịch bản EN" (chọn lọc từng mục), banner "machine-translated", nút chuyển ngôn ngữ **quăng về `/en` hoặc `/`** (không tra cặp), gợi ý "VI đã đổi — dịch lại?" + diff + preview | ~4–5 ngày | Thấp |
| **4. Chạy thử trên TEST + duyệt mẫu** | Dịch lô nhỏ trên môi trường test, xuất báo cáo chi phí thật + mẫu dịch để khách duyệt | ~2 ngày | Không (test) |
| **5. Job hàng loạt + đồng bộ** | Chạy dịch lô lớn (sau khi khách duyệt mẫu), job cron debounce đồng bộ khi VI đổi | ~2–3 ngày | Trung bình — chạy giờ thấp điểm, có thể dừng/tiếp |

*Tổng thô: ~4–5 tuần công + thời gian khách duyệt mẫu. Chốt chính xác sau bước đo (mục 5).*
*Job hàng loạt chạy vào giờ thấp điểm, chia lô, có thể tạm dừng — không nghẽn production.*

---

## 7. Trách nhiệm & điều khoản cần khách xác nhận

- **Bản `auto` hiển thị công khai:** khách chấp nhận đây là dịch máy, có thể sai; đội dev cung cấp công cụ + nhãn cảnh báo, **không cam kết độ chính xác từng câu**. Muốn chuẩn 100% cần người duyệt (`reviewed`).
- **Không làm thẳng production:** mọi lô dịch chạy thử trên test, khách duyệt mẫu trước.
- **Chi phí dịch máy** do khách chi trả theo dung lượng thực (đo ở mục 5); chi phí nhân lực triển khai theo timeline mục 6.

---

## 8. Trạng thái chốt & bước đi tiếp theo

### Đã chốt
- **Kiến trúc:** dòng riêng `LanguageId=2` + `OriginalId` (site EN chọn lọc), phạm vi 4 bảng (`SanPhamCNTB`, `Contents`, `NhaTuVan`, `NhaCungUng`); Menu/Category giữ `TitleEn`.
- **SEO:** định tuyến `/en/` theo URL (bỏ session).
- **Config:** toàn bộ để ở **SYS_PARAMETERS** (dùng màn hình tham số có sẵn) — 4 key `TRANSLATION_PROVIDER` / `TRANSLATION_AUTO_PUBLISH` / `TRANSLATION_ANTHROPIC_API_KEY` / `TRANSLATION_GOOGLE_API_KEY`; mở cột `Val`.
- **Duyệt:** cờ `TRANSLATION_AUTO_PUBLISH` (1=auto ngay / 0=cần duyệt).
- **Cơ chế dịch:** chọn ở trang list CMS (checkbox + nút bulk); dịch node-level HTML + validate; glossary động.
- **Khối lượng phạm vi chọn:** ~589 mục ≈ ~10 triệu ký tự; chi phí một lần **Sonnet ~$60** (khuyến nghị).

### Còn chờ khách xác nhận (không đội chi phí)
1. **Tin bài:** DB hiện chỉ 50 (LanguageId=1) — dịch 50 đang có, hay viết thêm cho đủ ~100?
2. **"Nhà cung ứng liên quan":** toàn bộ 77 hay chỉ NCU gắn với 500 SP đã chọn (`NCUId`)?

### Thứ tự triển khai khi có lệnh bắt đầu
1. **Migration + nền config** — cột `OriginalId`/`EnStale`/`SourceHash` cho 4 bảng; mở cột `SYS_PARAMETERS.Val` → `nvarchar(500)`; thêm 4 key `TRANSLATION_*` vào `ParameterKeys.cs` + seed; hoàn thiện "nhập trống = giữ key cũ". *(viết script cho khách review trước, chưa chạy vào DB)*
2. **`ITranslationService` + factory** (Claude Haiku/Sonnet, Google) + dịch node-level HTML + validate + glossary.
3. **Nút bulk-translate trên list CMS** + route `/en/` + nút chuyển ngôn ngữ (quăng về `/en` hoặc `/`, không tra cặp). `OriginalId` hiển thị ở màn sửa bản EN cho admin biết nguồn gốc.
4. **Chạy thử trên TEST** + khách duyệt mẫu → chạy lô lớn.
