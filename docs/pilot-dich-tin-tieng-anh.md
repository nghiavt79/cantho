# Pilot: Dịch tiếng Anh cho TIN BÀI (Contents)

**Mục đích:** làm hoàn chỉnh **1 loại nội dung (Tin/`Contents`)** để review. Ổn → nhân pattern y hệt cho Sản phẩm / Chuyên gia / Nhà cung ứng.
**Nguyên tắc:** dòng riêng `LanguageId=2` + `OriginalId`, URL `/en/`, config ở SYS_PARAMETERS, không phá dữ liệu cũ.

---

## 1. Migration (chỉ bảng `Contents`)

Thêm 3 cột (nullable — không ảnh hưởng dữ liệu/mã hiện có):

```sql
ALTER TABLE Contents ADD OriginalId bigint NULL;   -- Id bản VI gốc (bản EN mới có)
ALTER TABLE Contents ADD EnStale bit NULL;          -- true = bản VI đã đổi, EN cần dịch lại
ALTER TABLE Contents ADD SourceHash varchar(64) NULL; -- hash nội dung VI lúc dịch
CREATE INDEX IX_Contents_Lang_Original ON Contents(LanguageId, OriginalId); -- tra nhanh phía CMS
```

- Bản VI: `LanguageId=1`, `OriginalId=NULL` (như cũ).
- Bản EN sinh ra: `LanguageId=2`, `OriginalId = Id bản VI`, có `Id`/`QueryString`(slug EN) riêng.
- Entity `Content.cs`: thêm 3 property tương ứng.

---

## 2. Config (SYS_PARAMETERS — dùng 1 lần cho cả hệ, không riêng Tin)

Mở cột + thêm key (xem đề xuất chính, mục 4bis-⑥):
```sql
ALTER TABLE SYS_PARAMETERS ALTER COLUMN Val nvarchar(500) NOT NULL;
```
Seed 4 key: `TRANSLATION_PROVIDER` (haiku/sonnet/google), `TRANSLATION_AUTO_PUBLISH` (0/1),
`TRANSLATION_ANTHROPIC_API_KEY`, `TRANSLATION_GOOGLE_API_KEY` (tự mask/no-log vì tên chứa `API_KEY`).

---

## 3. Routing — MIDDLEWARE gắn cờ + ROUTE EN segment tiếng Anh *(đã đổi theo yêu cầu URL tiếng Anh)*

URL tiếng Anh phải là **tiếng Anh thật** (`/en/news-event`), không phải `/en/`+slug Việt. Nên:

**a) Middleware (trước `UseRouting`) — chỉ gắn cờ, KHÔNG cắt path:**
```csharp
app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path.Value ?? "";
    ctx.Items["Lang"] = (path == "/en" || path.StartsWith("/en/", ...)) ? "en" : "vi";
    await next();
});
```

**b) Route EN (segment tiếng Anh), trỏ về đúng Controller/Action hiện có:**
```csharp
app.MapControllerRoute("en_home",          "en",                                   new { controller="Home", action="Index" });
app.MapControllerRoute("en_news_category", "en/news-event",                        new { controller="News", action="Category", menuId=44 });
app.MapControllerRoute("en_news_detail",   "en/news-event/{queryString}-{id:long}", new { controller="News", action="Detail",   menuId=44 });
```
- `/en/news-event/water-pump-105` → `News.Detail` nạp `Contents` Id=105 (bản EN có Id + slug EN riêng).
- **Nhân ra loại khác:** thêm route `en/products`, `en/experts`… trỏ về controller tương ứng. Middleware + cờ dùng chung.
- Helper `LangHelper`: `IsEnglish(ctx)`, `CurrentLangId(ctx)` (1/2), `EnNewsUrl(slug,id)`.

---

## 4. Sửa đọc dữ liệu (NewsController)

- **Category (list):** thay `langId = Session.GetInt32("LanguageId") ?? 1` → `langId = Lang(HttpContext)`.
- **Detail:** thêm guard `if (p.LanguageId != Lang(HttpContext)) return 404;` để không lẫn VI dưới `/en` và ngược lại.
- `hreflang`: ở detail, nếu là VI và có bản EN (`WHERE OriginalId=p.Id AND LanguageId=2 AND StatusId=3`) → thêm `<link rel="alternate" hreflang="en" href="/en/...">` (và ngược lại). *Chỉ cho SEO, không dùng cho nút chuyển ngôn ngữ.*

## 5. Nút chuyển ngôn ngữ
- EN → link `/en`. VI → link `/`. Bỏ cơ chế cookie `Language/SetLanguage`. Nút luôn hiện, không tra cặp bài.

---

## 6. Dịch service

- `ITranslationService.TranslateHtmlAsync(vi, targetLang)` + factory đọc `TRANSLATION_PROVIDER`.
- **Node-level:** parse `Contents.Contents` (HTML) bằng HtmlAgilityPack, dịch text node, ghép lại. Trường ngắn (`Title`, `Description`) dịch thẳng.
- Nạp glossary vào prompt. Validate cấu trúc HTML sau dịch; lỗi → không lưu, giữ VI.

## 7. CMS — nút bulk trên list tin (PostsController)

- Trang danh sách tin CMS: thêm checkbox mỗi dòng + nút **"Dịch sang tiếng Anh"** + cột **Trạng thái EN**.
- Bấm → với mỗi Contents VI đã chọn:
  - Nếu chưa có bản EN (`OriginalId` chưa tồn tại): tạo dòng mới `LanguageId=2`, `OriginalId=Id`, dịch các trường, sinh `QueryString` slug EN, set `StatusId` theo cờ `TRANSLATION_AUTO_PUBLISH` (1→hiển thị/3, 0→nháp).
  - Nếu đã có: bỏ qua hoặc hỏi "dịch lại?" (nếu `EnStale`).
- Màn sửa bản EN: hiển thị link **"Bản gốc VI"** (từ `OriginalId`) cho admin biết nguồn.

## 8. Cờ auto-publish
- `paramSvc.GetIntAsync("TRANSLATION_AUTO_PUBLISH", 0)`: 1 → bản EN `auto` hiển thị ngay (banner "machine-translated"); 0 → giữ nháp tới khi duyệt.

---

## 9. Thứ tự làm (pilot Tin)
1. Migration `Contents` + config SYS_PARAMETERS (script để review, chưa chạy).
2. Middleware `/en` + helper `Lang`/`LUrl` + sửa NewsController đọc cờ.
3. `ITranslationService` + factory + node-level HTML.
4. Nút bulk-translate trên list tin CMS + màn sửa bản EN.
5. Test trên môi trường test: dịch thử vài tin → xem `/en/tin-tuc-su-kien/...`.

**Nhân ra các loại khác:** lặp bước 1 (đổi bảng) + 4 (đổi controller list); bước 2–3 (routing + service) **dùng chung, làm 1 lần**.
