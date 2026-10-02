using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>Quản lý câu hỏi thường gặp (HSMT A.7.3).</summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class FaqAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "FaqAdmin";
        private static readonly string[] LogFunctionAliases = { "Faq" };

        public FaqAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private Task WriteLog(int eventId, string content) =>
            CmsLogHelper.WriteLogAsync(_context, HttpContext, GetSiteId(), LogFunctionName, LogFunctionAliases, eventId, content);

        [HttpGet]
        public async Task<IActionResult> Index(int languageId = 1, string? keyword = null, string? group = null)
        {
            var query = _context.Faqs.AsNoTracking().Where(f => f.LanguageId == languageId);
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(f => f.Question.Contains(keyword) || f.Answer.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(group))
                query = query.Where(f => f.GroupName == group);

            var items = await query.OrderBy(f => f.GroupName).ThenBy(f => f.SortOrder).ThenBy(f => f.Id).ToListAsync();

            ViewBag.LanguageId = languageId;
            ViewBag.Keyword = keyword;
            ViewBag.Group = group;
            ViewBag.Groups = await _context.Faqs.AsNoTracking()
                .Where(f => f.LanguageId == languageId)
                .Select(f => f.GroupName).Distinct().OrderBy(g => g).ToListAsync();
            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var f = await _context.Faqs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (f == null) return Json(new { success = false, message = "Không tìm thấy câu hỏi." });
            return Json(new { success = true, item = new { f.Id, f.GroupName, f.Question, f.Answer, f.SortOrder, f.IsActive, f.LanguageId } });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(FaqFormVm model)
        {
            if (!ModelState.IsValid)
            {
                var msg = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = msg });
            }

            var who = User.Identity?.Name;
            Faq entity;
            if (model.Id > 0)
            {
                entity = await _context.Faqs.FindAsync(model.Id) ?? throw new InvalidOperationException();
                entity.Modified = DateTime.Now;
                entity.Modifier = who;
            }
            else
            {
                entity = new Faq { Created = DateTime.Now, Creator = who };
                _context.Faqs.Add(entity);
            }

            entity.GroupName = model.GroupName.Trim();
            entity.Question = model.Question.Trim();
            entity.Answer = model.Answer.Trim();
            entity.SortOrder = model.SortOrder;
            entity.IsActive = model.IsActive;
            entity.LanguageId = model.LanguageId == 2 ? 2 : 1;
            await _context.SaveChangesAsync();

            await WriteLog(model.Id > 0 ? 2 : 1, $"{(model.Id > 0 ? "Update" : "Create")} FAQ #{entity.Id}: {entity.Question}");
            return Json(new { success = true, message = "Đã lưu câu hỏi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var f = await _context.Faqs.FindAsync(id);
            if (f == null) return Json(new { success = false, message = "Không tìm thấy câu hỏi." });
            f.IsActive = !f.IsActive;
            f.Modified = DateTime.Now;
            f.Modifier = User.Identity?.Name;
            await _context.SaveChangesAsync();
            await WriteLog(2, $"FAQ #{id}: IsActive={f.IsActive}");
            return Json(new { success = true, isActive = f.IsActive });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var f = await _context.Faqs.FindAsync(id);
            if (f == null) return Json(new { success = false, message = "Không tìm thấy câu hỏi." });
            _context.Faqs.Remove(f);
            await _context.SaveChangesAsync();
            await WriteLog(3, $"Delete FAQ #{id}: {f.Question}");
            return Json(new { success = true });
        }
    }

    public class FaqFormVm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nhập nhóm câu hỏi.")]
        [StringLength(200)]
        public string GroupName { get; set; } = "";

        [Required(ErrorMessage = "Nhập câu hỏi.")]
        [StringLength(1000)]
        public string Question { get; set; } = "";

        [Required(ErrorMessage = "Nhập câu trả lời.")]
        public string Answer { get; set; } = "";

        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public int LanguageId { get; set; } = 1;
    }
}
