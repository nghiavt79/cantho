using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Helpers;
using TechExchangeApp.Services;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>
    /// Quản lý mẫu đăng ký / biểu mẫu động (HSMT A.7.2, B.1.6): định nghĩa trường, trường bắt buộc,
    /// hành động sau khi gửi (thông báo email, email xác nhận, chuyển trang); xem và xuất dữ liệu đã gửi.
    /// </summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class FormsAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "FormsAdmin";
        private static readonly string[] LogFunctionAliases = { "Forms" };

        public FormsAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() => int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;
        private Task WriteLog(int eventId, string content) =>
            CmsLogHelper.WriteLogAsync(_context, HttpContext, GetSiteId(), LogFunctionName, LogFunctionAliases, eventId, content);

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var forms = await _context.DynamicForms.AsNoTracking().OrderByDescending(f => f.Id)
                .Select(f => new FormAdminRow
                {
                    Id = f.Id, Title = f.Title, Slug = f.Slug, LanguageId = f.LanguageId, IsActive = f.IsActive,
                    Submissions = _context.DynamicFormSubmissions.Count(s => s.FormId == f.Id),
                    LastSubmission = _context.DynamicFormSubmissions.Where(s => s.FormId == f.Id).Max(s => (DateTime?)s.Created)
                }).ToListAsync();
            return View(forms);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            var form = id.HasValue ? await _context.DynamicForms.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id) : null;
            form ??= new DynamicForm { FieldsJson = "[{\"key\":\"ho_ten\",\"label\":\"Họ tên\",\"type\":\"text\",\"required\":true},{\"key\":\"email\",\"label\":\"Email\",\"type\":\"email\",\"required\":true}]", SubmitText = "Gửi" };
            return View(form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DynamicForm input)
        {
            var fields = DynamicFormService.ParseFields(input.FieldsJson);
            foreach (var f in fields) f.Options = f.Options?.Select(o => o.Trim()).Where(o => o.Length > 0).Distinct().ToList();
            var err = DynamicFormService.ValidateDefinition(fields);
            input.Slug = string.IsNullOrWhiteSpace(input.Slug) ? SlugHelper.Slugify(input.Title ?? "") : input.Slug.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(input.Title)) err ??= "Nhập tiêu đề biểu mẫu.";
            if (!Regex.IsMatch(input.Slug, "^[a-z0-9][a-z0-9-]{1,199}$")) err ??= "Đường dẫn chỉ gồm chữ thường không dấu, số và gạch ngang.";
            if (await _context.DynamicForms.AnyAsync(f => f.Slug == input.Slug && f.Id != input.Id)) err ??= "Đường dẫn đã được dùng cho biểu mẫu khác.";
            if (!string.IsNullOrWhiteSpace(input.RedirectUrl) && !Url.IsLocalUrl(input.RedirectUrl)) err ??= "Trang chuyển tới phải là đường dẫn trong website, bắt đầu bằng /";
            var emails = (input.NotifyEmails ?? "").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (emails.Any(e => !Regex.IsMatch(e, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))) err ??= "Danh sách email nhận thông báo không hợp lệ.";
            if (err != null)
            {
                ViewBag.Error = err;
                return View(input);
            }

            var form = input.Id > 0 ? await _context.DynamicForms.FindAsync(input.Id) : null;
            var isNew = form == null;
            if (form == null) { form = new DynamicForm { Created = DateTime.Now }; _context.DynamicForms.Add(form); }
            form.Title = input.Title!.Trim();
            form.Slug = input.Slug;
            form.Description = input.Description?.Trim();
            form.FieldsJson = DynamicFormService.SerializeFields(fields);
            form.SubmitText = input.SubmitText?.Trim();
            form.SuccessMessage = input.SuccessMessage?.Trim();
            form.RedirectUrl = string.IsNullOrWhiteSpace(input.RedirectUrl) ? null : input.RedirectUrl.Trim();
            form.NotifyEmails = emails.Length == 0 ? null : string.Join(", ", emails);
            form.SendConfirmation = input.SendConfirmation;
            form.LanguageId = input.LanguageId == 2 ? 2 : 1;
            form.IsActive = input.IsActive;
            form.Modified = DateTime.Now;
            form.Modifier = User.Identity?.Name;
            await _context.SaveChangesAsync();
            await WriteLog(isNew ? 1 : 2, $"{(isNew ? "Tạo" : "Sửa")} biểu mẫu #{form.Id} '{form.Title}'");
            TempData["Success"] = "Đã lưu biểu mẫu.";
            return RedirectToAction(nameof(Edit), new { id = form.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Submissions(int id)
        {
            var form = await _context.DynamicForms.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
            if (form == null) return NotFound();
            ViewBag.Form = form;
            ViewBag.Fields = DynamicFormService.ParseFields(form.FieldsJson);
            var rows = await _context.DynamicFormSubmissions.AsNoTracking().Where(s => s.FormId == id).OrderByDescending(s => s.Created).Take(1000).ToListAsync();
            return View(rows);
        }

        [HttpGet]
        public async Task<IActionResult> Export(int id)
        {
            var form = await _context.DynamicForms.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
            if (form == null) return NotFound();
            var fields = DynamicFormService.ParseFields(form.FieldsJson);
            var rows = await _context.DynamicFormSubmissions.AsNoTracking().Where(s => s.FormId == id).OrderBy(s => s.Created).ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("DuLieu");
            var headers = new List<string> { "STT", "Thời điểm gửi" };
            headers.AddRange(fields.Select(f => f.Label));
            for (int c = 0; c < headers.Count; c++) { ws.Cells[1, c + 1].Value = headers[c]; ws.Cells[1, c + 1].Style.Font.Bold = true; }
            for (int r = 0; r < rows.Count; r++)
            {
                var data = DynamicFormService.ParseData(rows[r].DataJson);
                ws.Cells[r + 2, 1].Value = r + 1;
                ws.Cells[r + 2, 2].Value = rows[r].Created.ToString("dd/MM/yyyy HH:mm");
                for (int c = 0; c < fields.Count; c++)
                {
                    data.TryGetValue(fields[c].Key, out var v);
                    ws.Cells[r + 2, c + 3].Value = fields[c].Type == "checkbox" ? (v == "true" ? "Có" : "") : v;
                }
            }
            if (ws.Dimension != null) ws.Cells[ws.Dimension.Address].AutoFitColumns(10, 60);
            await WriteLog(4, $"Xuất Excel dữ liệu biểu mẫu #{id} ({rows.Count} dòng)");
            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BieuMau_{form.Slug}_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubmission(int id)
        {
            var s = await _context.DynamicFormSubmissions.FindAsync(id);
            if (s == null) return Json(new { success = false, message = "Không tìm thấy." });
            _context.DynamicFormSubmissions.Remove(s);
            await _context.SaveChangesAsync();
            await WriteLog(3, $"Xóa dữ liệu biểu mẫu #{s.FormId} (lượt gửi #{id})");
            return Json(new { success = true });
        }
    }

    public class FormAdminRow
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public int LanguageId { get; set; }
        public bool IsActive { get; set; }
        public int Submissions { get; set; }
        public DateTime? LastSubmission { get; set; }
    }
}
