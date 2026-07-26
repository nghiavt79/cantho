using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class SearchIndexController : Controller
    {
        private readonly AppDbContext _context;

        public SearchIndexController(AppDbContext context)
        {
            _context = context;
        }

        // POST: /cms/SearchIndex/Reindex
        // Gọi stored proc dbo.uspReIndexSearchContents để dựng lại chỉ mục tìm kiếm.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reindex(int languageId = 1)
        {
            try
            {
                // Reindex có thể lâu → nới command timeout.
                _context.Database.SetCommandTimeout(600);
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.uspReIndexSearchContents @LanguageId = {0}", languageId);

                return Json(new { success = true, message = "Đã cập nhật chỉ mục tìm kiếm." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi cập nhật chỉ mục: " + ex.Message });
            }
        }
    }
}
