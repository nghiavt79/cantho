using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Services.Localization;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>Quản lý chuỗi giao diện i18n (bảng UiTranslations). Khác TranslationAdmin (dịch nội dung).</summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class UiTranslationAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IUiTextService _uiText;
        private readonly IWebHostEnvironment _env;

        public UiTranslationAdminController(AppDbContext context, IUiTextService uiText, IWebHostEnvironment env)
        {
            _context = context;
            _uiText = uiText;
            _env = env;
        }

        private string? CurrentUser() => User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;

        // Nhóm = tiền tố trước dấu '.' đầu tiên trong Key (common, news, ...).
        private static string GroupOf(string key)
        {
            var i = key.IndexOf('.');
            return i > 0 ? key[..i] : "(khác)";
        }

        // ─────────────────────────── LIST ───────────────────────────
        public async Task<IActionResult> Index(string? keyword, string? group, bool? activeOnly,
            int page = 1, int pageSize = 30)
        {
            var query = _context.UiTranslations.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.Key.Contains(keyword)
                                         || (x.Vi != null && x.Vi.Contains(keyword))
                                         || (x.En != null && x.En.Contains(keyword)));
            if (!string.IsNullOrWhiteSpace(group))
                query = query.Where(x => x.Key.StartsWith(group + "."));
            if (activeOnly == true)
                query = query.Where(x => x.IsActive);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.Key)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Danh sách nhóm cho dropdown (prefix trước dấu chấm).
            var allKeys = await _context.UiTranslations.AsNoTracking().Select(x => x.Key).ToListAsync();
            ViewBag.Groups = allKeys.Select(GroupOf).Distinct().OrderBy(g => g).ToList();

            ViewBag.Keyword = keyword;
            ViewBag.Group = group;
            ViewBag.ActiveOnly = activeOnly;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            return View(items);
        }

        // ─────────────────────────── CREATE ───────────────────────────
        [HttpGet]
        public IActionResult Create() => View(new UiTranslationFormVm { IsActive = true });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UiTranslationFormVm vm)
        {
            vm.Key = (vm.Key ?? "").Trim();
            if (string.IsNullOrWhiteSpace(vm.Key))
                ModelState.AddModelError(nameof(vm.Key), "Key không được để trống.");
            else if (await _context.UiTranslations.AnyAsync(x => x.Key == vm.Key))
                ModelState.AddModelError(nameof(vm.Key), "Key đã tồn tại.");

            if (!ModelState.IsValid) return View(vm);

            _context.UiTranslations.Add(new UiTranslation
            {
                Key = vm.Key,
                Vi = vm.Vi,
                En = vm.En,
                Note = vm.Note,
                AllowHtml = vm.AllowHtml,
                IsActive = vm.IsActive,
                Created = DateTime.Now,
                Creator = CurrentUser()
            });
            await _context.SaveChangesAsync();
            _uiText.Invalidate();

            TempData["Success"] = "Đã thêm key.";
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────── EDIT ───────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var e = await _context.UiTranslations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (e == null) return NotFound();
            return View(new UiTranslationFormVm
            {
                Id = e.Id, Key = e.Key, Vi = e.Vi, En = e.En, Note = e.Note,
                AllowHtml = e.AllowHtml, IsActive = e.IsActive, RowVersion = e.RowVersion
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UiTranslationFormVm vm)
        {
            var e = await _context.UiTranslations.FindAsync(vm.Id);
            if (e == null) return NotFound();
            if (!ModelState.IsValid) { vm.Key = e.Key; return View(vm); }

            // Snapshot giá trị cũ trước khi ghi đè (audit).
            _context.UiTranslationHistories.Add(new UiTranslationHistory
            {
                TranslationId = e.Id, Vi = e.Vi, En = e.En, AllowHtml = e.AllowHtml,
                ChangedBy = CurrentUser(), ChangedAt = DateTime.Now
            });

            // Key readonly (không đổi). Cập nhật nội dung.
            e.Vi = vm.Vi;
            e.En = vm.En;
            e.Note = vm.Note;
            e.AllowHtml = vm.AllowHtml;
            e.IsActive = vm.IsActive;
            e.Modified = DateTime.Now;
            e.Modifier = CurrentUser();

            // Optimistic concurrency qua RowVersion.
            _context.Entry(e).Property(x => x.RowVersion).OriginalValue = vm.RowVersion;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Bản ghi đã bị người khác sửa. Tải lại và thử lại.");
                var fresh = await _context.UiTranslations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == vm.Id);
                vm.RowVersion = fresh?.RowVersion;
                vm.Key = e.Key;
                return View(vm);
            }

            _uiText.Invalidate();
            TempData["Success"] = "Đã cập nhật key.";
            return RedirectToAction(nameof(Index), new { keyword = ViewBag.Keyword });
        }

        // ─────────────────────────── TOGGLE ACTIVE ───────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var e = await _context.UiTranslations.FindAsync(id);
            if (e == null) return Json(new { success = false, message = "Không tìm thấy." });
            e.IsActive = !e.IsActive;
            e.Modified = DateTime.Now;
            e.Modifier = CurrentUser();
            await _context.SaveChangesAsync();
            _uiText.Invalidate();
            return Json(new { success = true, value = e.IsActive });
        }

        // ─────────────────────────── HISTORY ───────────────────────────
        [HttpGet]
        public async Task<IActionResult> History(int id)
        {
            var e = await _context.UiTranslations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (e == null) return NotFound();
            ViewBag.KeyName = e.Key;
            var rows = await _context.UiTranslationHistories.AsNoTracking()
                .Where(h => h.TranslationId == id)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();
            return View(rows);
        }

        // ─────────────────────────── IMPORT ui.json ───────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportUiJson(bool overwrite = false)
        {
            var path = Path.Combine(_env.ContentRootPath, "Localization", "ui.json");
            if (!System.IO.File.Exists(path))
            {
                TempData["Error"] = "Không thấy ui.json.";
                return RedirectToAction(nameof(Index));
            }

            var json = await System.IO.File.ReadAllTextAsync(path, Encoding.UTF8);
            var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json)
                       ?? new();

            int inserted = 0, updated = 0, skipped = 0;
            var existing = await _context.UiTranslations.ToDictionaryAsync(x => x.Key, StringComparer.Ordinal);

            foreach (var (key, langs) in data)
            {
                if (string.IsNullOrWhiteSpace(key)) continue;
                langs.TryGetValue("vi", out var vi);
                langs.TryGetValue("en", out var en);

                if (!existing.TryGetValue(key, out var e))
                {
                    _context.UiTranslations.Add(new UiTranslation
                    {
                        Key = key, Vi = vi, En = en, IsActive = true,
                        Created = DateTime.Now, Creator = CurrentUser()
                    });
                    inserted++;
                }
                else if (overwrite && (e.Vi != vi || e.En != en))
                {
                    _context.UiTranslationHistories.Add(new UiTranslationHistory
                    {
                        TranslationId = e.Id, Vi = e.Vi, En = e.En, AllowHtml = e.AllowHtml,
                        ChangedBy = CurrentUser(), ChangedAt = DateTime.Now
                    });
                    e.Vi = vi; e.En = en; e.Modified = DateTime.Now; e.Modifier = CurrentUser();
                    updated++;
                }
                else skipped++;
            }

            await _context.SaveChangesAsync();
            _uiText.Invalidate();
            TempData["Success"] = $"Import: thêm {inserted}, cập nhật {updated}, bỏ qua {skipped}.";
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────── EXPORT ui.json ───────────────────────────
        [HttpGet]
        public async Task<IActionResult> ExportUiJson()
        {
            var rows = await _context.UiTranslations.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Key)
                .ToListAsync();

            var map = new Dictionary<string, Dictionary<string, string>>();
            foreach (var r in rows)
                map[r.Key] = new Dictionary<string, string> { ["vi"] = r.Vi ?? "", ["en"] = r.En ?? "" };

            var opts = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) // giữ tiếng Việt, không escape \u
            };
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(map, opts));
            return File(bytes, "application/json", "ui.json");
        }
    }

    public class UiTranslationFormVm
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Key không được để trống.")]
        [System.ComponentModel.DataAnnotations.StringLength(250)]
        public string Key { get; set; } = "";

        public string? Vi { get; set; }
        public string? En { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(500)]
        public string? Note { get; set; }

        public bool AllowHtml { get; set; }
        public bool IsActive { get; set; } = true;
        public byte[]? RowVersion { get; set; }
    }
}
