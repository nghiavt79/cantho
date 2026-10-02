using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Services;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>Cảnh báo sản phẩm trùng lặp, quá hạn cập nhật (HSMT A.8.2).</summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class ProductWarningsAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "ProductWarningsAdmin";
        private static readonly string[] LogFunctionAliases = { "ProductWarnings" };

        public ProductWarningsAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private Task WriteLog(int eventId, string content) =>
            CmsLogHelper.WriteLogAsync(_context, HttpContext, GetSiteId(), LogFunctionName, LogFunctionAliases, eventId, content);

        [HttpGet]
        public async Task<IActionResult> Index(byte? type, byte status = ProductWarningStatuses.Open, string? keyword = null, int page = 1, int pageSize = 30)
        {
            if (page < 1) page = 1;
            var query = from w in _context.ProductWarnings.AsNoTracking()
                        join p in _context.SanPhamCNTBs.AsNoTracking() on w.SanPhamId equals p.ID
                        where w.Status == status
                        select new { w, p.Name, p.ProductType, p.StatusId, p.Creator };
            if (type.HasValue) query = query.Where(x => x.w.WarningType == type.Value);
            if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(x => x.Name != null && x.Name.Contains(keyword));

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.w.WarningType).ThenBy(x => x.w.GroupKey).ThenBy(x => x.w.SanPhamId)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new ProductWarningRow
                {
                    Id = x.w.Id, SanPhamId = x.w.SanPhamId, ProductName = x.Name ?? "", ProductType = x.ProductType,
                    ProductStatusId = x.StatusId, Creator = x.Creator, WarningType = x.w.WarningType, Detail = x.w.Detail,
                    DetectedAt = x.w.DetectedAt, LastSeenAt = x.w.LastSeenAt, ResolvedAt = x.w.ResolvedAt,
                    ResolvedBy = x.w.ResolvedBy, Note = x.w.Note
                }).ToListAsync();

            ViewBag.Counts = await _context.ProductWarnings.AsNoTracking()
                .Where(w => w.Status == ProductWarningStatuses.Open)
                .GroupBy(w => w.WarningType).Select(g => new { g.Key, N = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.N);
            ViewBag.Type = type;
            ViewBag.Status = status;
            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Total = total;
            ViewBag.LastScan = await _context.ProductWarnings.AsNoTracking().MaxAsync(w => (DateTime?)w.LastSeenAt);
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScanNow([FromServices] IProductWarningScanner scanner)
        {
            var r = await scanner.ScanAsync(HttpContext.RequestAborted);
            await WriteLog(2, $"Quét cảnh báo sản phẩm: trùng tên {r.DuplicateName}, trùng mô tả {r.DuplicateDescription}, quá hạn {r.Stale}");
            return Json(new
            {
                success = true,
                message = $"Đã quét: {r.DuplicateName} trùng tên, {r.DuplicateDescription} trùng mô tả, {r.Stale} quá hạn. Mới phát hiện {r.Opened}, tự đóng {r.AutoClosed}."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(List<int> ids, string? note)
        {
            if (ids == null || ids.Count == 0) return Json(new { success = false, message = "Chưa chọn cảnh báo." });
            var rows = await _context.ProductWarnings.Where(w => ids.Contains(w.Id) && w.Status == ProductWarningStatuses.Open).ToListAsync();
            var now = DateTime.Now;
            foreach (var w in rows)
            {
                w.Status = ProductWarningStatuses.Resolved;
                w.ResolvedAt = now;
                w.ResolvedBy = User.Identity?.Name;
                w.Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()[..Math.Min(note.Trim().Length, 500)];
            }
            await _context.SaveChangesAsync();
            await WriteLog(2, $"Đánh dấu đã xử lý {rows.Count} cảnh báo sản phẩm (IDs={string.Join(",", rows.Select(r => r.Id))})");
            return Json(new { success = true, message = $"Đã đánh dấu {rows.Count} cảnh báo là đã xử lý." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reopen(int id)
        {
            var w = await _context.ProductWarnings.FindAsync(id);
            if (w == null) return Json(new { success = false, message = "Không tìm thấy cảnh báo." });
            w.Status = ProductWarningStatuses.Open;
            w.ResolvedAt = null;
            w.ResolvedBy = null;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        /// <summary>Số cảnh báo đang mở — hiện badge ở menu CMS.</summary>
        public static Task<int> CountOpenAsync(AppDbContext db)
            => db.ProductWarnings.AsNoTracking().CountAsync(w => w.Status == ProductWarningStatuses.Open);
    }

    public class ProductWarningRow
    {
        public int Id { get; set; }
        public int SanPhamId { get; set; }
        public string ProductName { get; set; } = "";
        public int ProductType { get; set; }
        public int? ProductStatusId { get; set; }
        public string? Creator { get; set; }
        public byte WarningType { get; set; }
        public string? Detail { get; set; }
        public DateTime DetectedAt { get; set; }
        public DateTime LastSeenAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
        public string? Note { get; set; }
    }
}
