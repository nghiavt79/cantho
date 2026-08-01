using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Configuration;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Helpers;
using TechExchangeApp.Interfaces;
using TechExchangeApp.Services.Translation;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>
    /// Bulk-translate: từ trang list CMS chọn các mục VI → tạo bản EN (dòng riêng LanguageId=2 + OriginalId).
    /// Pilot: chỉ Tin (Contents). Nhân ra loại khác = thêm action tương tự.
    /// </summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class TranslationAdminController : Controller
    {
        private const int LangEn = 2;
        private const int StatusPublished = 3;
        private const int StatusDraft = 1;
        private const int EnNewsMenuId = 62;   // menu EN "News & Event" (bản tiếng Anh của Tin sự kiện = 44)

        private readonly AppDbContext _context;
        private readonly ITranslationServiceFactory _factory;
        private readonly ISystemParameterService _param;

        public TranslationAdminController(AppDbContext context, ITranslationServiceFactory factory, ISystemParameterService param)
        {
            _context = context;
            _factory = factory;
            _param = param;
        }

        /// <summary>POST: dịch các tin VI đã chọn sang EN. Trả JSON {ok, created, skipped, failed, message}.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkTranslateNews(long[] ids, CancellationToken ct)
        {
            if (ids == null || ids.Length == 0)
                return Json(new { ok = false, message = "Chưa chọn tin nào." });

            var svc = await _factory.CreateAsync(ct);
            if (svc == null)
                return NoKeyResult(ids, "Posts");

            bool autoPublish = await _param.GetIntAsync(ParameterKeys.TranslationAutoPublish, 0) == 1;
            var user = User.Identity?.Name ?? "system";

            int created = 0, skipped = 0, failed = 0;

            foreach (var id in ids.Distinct())
            {
                var vi = await _context.Contents.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id && c.LanguageId == 1, ct);
                if (vi == null) { skipped++; continue; }

                // Đã có bản EN cho tin này?
                bool hasEn = await _context.Contents
                    .AnyAsync(c => c.OriginalId == id && c.LanguageId == LangEn, ct);
                if (hasEn) { skipped++; continue; }

                try
                {
                    var titleEn = await svc.TranslatePlainAsync(vi.Title, ct);
                    var descEn = await svc.TranslatePlainAsync(vi.Description, ct);
                    var contentEn = await svc.TranslateHtmlAsync(vi.Contents, ct);

                    // Dịch hỏng hoàn toàn → bỏ qua (giữ nguyên, không tạo bản rác)
                    if (string.IsNullOrWhiteSpace(titleEn)) { failed++; continue; }

                    var en = new Content
                    {
                        Title = titleEn,
                        QueryString = SlugHelper.Slugify(titleEn),
                        Description = descEn ?? vi.Description,
                        Contents = contentEn ?? vi.Contents, // dịch lỗi phần thân → tạm giữ VI
                        Author = vi.Author,
                        Image = vi.Image,
                        ImageBig = vi.ImageBig,
                        MenuId = EnNewsMenuId,   // bài EN gắn menu EN "News & Event" (62)
                        TypeId = vi.TypeId,
                        Subject = vi.Subject,
                        Keyword = vi.Keyword,
                        PublishedDate = vi.PublishedDate,
                        IsHot = vi.IsHot,
                        IsNew = vi.IsNew,
                        Domain = vi.Domain,
                        SiteId = vi.SiteId,
                        ParentId = vi.ParentId,

                        LanguageId = LangEn,
                        OriginalId = id,
                        EnStale = false,
                        SourceHash = Hash(vi),
                        StatusId = autoPublish ? StatusPublished : StatusDraft,
                        Created = DateTime.Now,
                        Creator = user,
                    };
                    _context.Contents.Add(en);
                    await _context.SaveChangesAsync(ct);
                    created++;
                }
                catch { failed++; }
            }

            // Dịch 1 mục nhưng lỗi (key sai...) → chuyển sang nhập tay
            if (ids.Length == 1 && created == 0 && failed == 1)
                return Json(new { ok = false, needManual = true,
                    redirectUrl = Url.Action("Create", "Posts", new { area = "Cms", fromId = ids[0] }),
                    message = "Dịch tự động lỗi (kiểm tra API key). Chuyển sang nhập tay bản tiếng Anh." });

            return Json(new
            {
                ok = true,
                created,
                skipped,
                failed,
                provider = svc.ProviderName,
                autoPublish,
                message = $"Đã tạo {created} bản EN, bỏ qua {skipped} (đã có/không hợp lệ), lỗi {failed}."
            });
        }

        private IActionResult NoKeyResult(int[] ids, string controller)
            => NoKeyResult(ids.Select(i => (long)i).ToArray(), controller);

        // Không có key → nếu chọn đúng 1 mục thì chuyển sang form nhập tay bản EN
        private IActionResult NoKeyResult(long[] ids, string controller)
        {
            const string msg = "Chưa cấu hình nhà cung cấp dịch / API key trong Tham số hệ thống.";
            if (ids.Length == 1)
                return Json(new { ok = false, needManual = true,
                    redirectUrl = Url.Action("Create", controller, new { area = "Cms", fromId = ids[0] }),
                    message = msg + " Chuyển sang nhập tay bản tiếng Anh." });
            return Json(new { ok = false, message = msg + " Hãy nhập key, hoặc chọn từng mục để nhập tay." });
        }

        private static string Hash(Content c)
            => HashOf(c.Title, c.Description, c.Contents);

        private static string HashOf(params string?[] parts)
        {
            var raw = string.Join("␟", parts.Select(p => p ?? ""));
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
        }

        // ─────────────────────────────────────────
        // CHUYÊN GIA (NhaTuVan)
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkTranslateExperts(int[] ids, CancellationToken ct)
        {
            if (ids == null || ids.Length == 0)
                return Json(new { ok = false, message = "Chưa chọn chuyên gia nào." });

            var svc = await _factory.CreateAsync(ct);
            if (svc == null)
                return NoKeyResult(ids, "NhaTuVanAdmin");

            bool autoPublish = await _param.GetIntAsync(ParameterKeys.TranslationAutoPublish, 0) == 1;
            var user = User.Identity?.Name ?? "system";
            int created = 0, skipped = 0, failed = 0;

            foreach (var id in ids.Distinct())
            {
                var vi = await _context.NhaTuVans.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TuVanId == id && (x.LanguageId ?? 1) == 1, ct);
                if (vi == null) { skipped++; continue; }
                if (await _context.NhaTuVans.AnyAsync(x => x.OriginalId == id && x.LanguageId == LangEn, ct)) { skipped++; continue; }

                try
                {
                    var en = new NhaTuVan
                    {
                        // Định danh giữ nguyên (tên riêng, cơ quan)
                        FullName = vi.FullName,
                        CoQuan = vi.CoQuan,
                        QueryString = SlugHelper.Slugify(vi.FullName),
                        // Dịch phần mô tả
                        HocHam = await svc.TranslatePlainAsync(vi.HocHam, ct) ?? vi.HocHam,
                        ChucVu = await svc.TranslatePlainAsync(vi.ChucVu, ct) ?? vi.ChucVu,
                        DichVu = await svc.TranslatePlainAsync(vi.DichVu, ct) ?? vi.DichVu,
                        KetQuaNghienCuu = await svc.TranslateHtmlAsync(vi.KetQuaNghienCuu, ct) ?? vi.KetQuaNghienCuu,
                        QuaTrinhDaoTao = await svc.TranslateHtmlAsync(vi.QuaTrinhDaoTao, ct) ?? vi.QuaTrinhDaoTao,
                        QuaTrinhCongTac = await svc.TranslateHtmlAsync(vi.QuaTrinhCongTac, ct) ?? vi.QuaTrinhCongTac,
                        CongBoKhoaHoc = await svc.TranslateHtmlAsync(vi.CongBoKhoaHoc, ct) ?? vi.CongBoKhoaHoc,
                        SangChe = await svc.TranslateHtmlAsync(vi.SangChe, ct) ?? vi.SangChe,
                        DuAnNghienCuu = await svc.TranslateHtmlAsync(vi.DuAnNghienCuu, ct) ?? vi.DuAnNghienCuu,
                        KinhNghiem = await svc.TranslateHtmlAsync(vi.KinhNghiem, ct) ?? vi.KinhNghiem,
                        HiepHoiKhoaHoc = await svc.TranslatePlainAsync(vi.HiepHoiKhoaHoc, ct) ?? vi.HiepHoiKhoaHoc,
                        // Sao chép trường phi văn bản / bắt buộc
                        Email = vi.Email, DateOfBirth = vi.DateOfBirth, HinhDaiDien = vi.HinhDaiDien,
                        DiaChi = vi.DiaChi, Phone = vi.Phone, LinhVucId = vi.LinhVucId,
                        MaDinhDanh = vi.MaDinhDanh, TongTrichDan = vi.TongTrichDan, HIndex = vi.HIndex,
                        HoSoDinhKem = vi.HoSoDinhKem, UserId = vi.UserId, IsActivated = vi.IsActivated,
                        Domain = vi.Domain, SiteId = vi.SiteId, ParentId = vi.ParentId, Keywords = vi.Keywords,
                        Rating = vi.Rating,
                        LanguageId = LangEn, OriginalId = id, EnStale = false,
                        SourceHash = HashOf(vi.HocHam, vi.ChucVu, vi.DichVu, vi.KetQuaNghienCuu, vi.QuaTrinhCongTac, vi.CongBoKhoaHoc),
                        StatusId = autoPublish ? StatusPublished : StatusDraft,
                        Created = DateTime.Now, CreatedBy = user,
                    };
                    _context.NhaTuVans.Add(en);
                    await _context.SaveChangesAsync(ct);
                    created++;
                }
                catch { failed++; }
            }
            if (ids.Length == 1 && created == 0 && failed == 1)
                return Json(new { ok = false, needManual = true,
                    redirectUrl = Url.Action("Create", "NhaTuVanAdmin", new { area = "Cms", fromId = ids[0] }),
                    message = "Dịch tự động lỗi (kiểm tra API key). Chuyển sang nhập tay bản tiếng Anh." });
            return Json(new { ok = true, created, skipped, failed, provider = svc.ProviderName, message = $"Đã tạo {created} bản EN, bỏ qua {skipped}, lỗi {failed}." });
        }

        // ─────────────────────────────────────────
        // NHÀ CUNG ỨNG (NhaCungUng)
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkTranslateSuppliers(int[] ids, CancellationToken ct)
        {
            if (ids == null || ids.Length == 0)
                return Json(new { ok = false, message = "Chưa chọn nhà cung ứng nào." });

            var svc = await _factory.CreateAsync(ct);
            if (svc == null)
                return NoKeyResult(ids, "NhaCungUngAdmin");

            bool autoPublish = await _param.GetIntAsync(ParameterKeys.TranslationAutoPublish, 0) == 1;
            var user = User.Identity?.Name ?? "system";
            int created = 0, skipped = 0, failed = 0;

            foreach (var id in ids.Distinct())
            {
                var vi = await _context.NhaCungUngs.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.CungUngId == id && (x.LanguageId ?? 1) == 1, ct);
                if (vi == null) { skipped++; continue; }
                if (await _context.NhaCungUngs.AnyAsync(x => x.OriginalId == id && x.LanguageId == LangEn, ct)) { skipped++; continue; }

                try
                {
                    var en = new NhaCungUng
                    {
                        FullName = vi.FullName,            // tên đơn vị giữ nguyên
                        TenVietTat = vi.TenVietTat,
                        QueryString = SlugHelper.Slugify(vi.FullName),
                        ChucNangChinh = await svc.TranslateHtmlAsync(vi.ChucNangChinh, ct) ?? vi.ChucNangChinh,
                        DichVu = await svc.TranslatePlainAsync(vi.DichVu, ct) ?? vi.DichVu,
                        SanPham = await svc.TranslateHtmlAsync(vi.SanPham, ct) ?? vi.SanPham,
                        ChucVu = await svc.TranslatePlainAsync(vi.ChucVu, ct) ?? vi.ChucVu,
                        ChungNhan = await svc.TranslateHtmlAsync(vi.ChungNhan, ct) ?? vi.ChungNhan,
                        // Sao chép phi văn bản
                        HinhDaiDien = vi.HinhDaiDien, DiaChi = vi.DiaChi, Phone = vi.Phone, Email = vi.Email,
                        Fax = vi.Fax, Website = vi.Website, NguoiDaiDien = vi.NguoiDaiDien, LinhVucId = vi.LinhVucId,
                        Logo = vi.Logo, VideoUrl = vi.VideoUrl, LoaiHinhToChuc = vi.LoaiHinhToChuc, MaSoThue = vi.MaSoThue,
                        UserId = vi.UserId, IsActivated = vi.IsActivated, Domain = vi.Domain, SiteId = vi.SiteId,
                        ParentId = vi.ParentId, Keywords = vi.Keywords, Rating = vi.Rating,
                        LanguageId = LangEn, OriginalId = id, EnStale = false,
                        SourceHash = HashOf(vi.ChucNangChinh, vi.DichVu, vi.SanPham, vi.ChungNhan),
                        StatusId = autoPublish ? StatusPublished : StatusDraft,
                        Created = DateTime.Now, CreatedBy = user,
                    };
                    _context.NhaCungUngs.Add(en);
                    await _context.SaveChangesAsync(ct);
                    created++;
                }
                catch { failed++; }
            }
            if (ids.Length == 1 && created == 0 && failed == 1)
                return Json(new { ok = false, needManual = true,
                    redirectUrl = Url.Action("Create", "NhaCungUngAdmin", new { area = "Cms", fromId = ids[0] }),
                    message = "Dịch tự động lỗi (kiểm tra API key). Chuyển sang nhập tay bản tiếng Anh." });
            return Json(new { ok = true, created, skipped, failed, provider = svc.ProviderName, message = $"Đã tạo {created} bản EN, bỏ qua {skipped}, lỗi {failed}." });
        }
    }
}
