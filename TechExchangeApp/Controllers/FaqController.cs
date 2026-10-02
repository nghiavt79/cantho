using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Helpers;

namespace TechExchangeApp.Controllers
{
    /// <summary>Trang câu hỏi thường gặp công khai (HSMT A.7.3): /hoi-dap, /en/faq.</summary>
    public class FaqController : Controller
    {
        private readonly AppDbContext _context;
        public FaqController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Index(string? q)
        {
            var langId = LangHelper.CurrentLangId(HttpContext);
            var query = _context.Faqs.AsNoTracking().Where(f => f.IsActive && f.LanguageId == langId);
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(f => f.Question.Contains(q) || f.Answer.Contains(q));
            }

            var items = await query.OrderBy(f => f.GroupName).ThenBy(f => f.SortOrder).ThenBy(f => f.Id).ToListAsync();
            ViewBag.Query = q;
            return View(items);
        }
    }
}
