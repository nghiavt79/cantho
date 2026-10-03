using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Services;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>
    /// Phân hạng nhà cung cấp (HSMT A.2.2, A.2.4): danh mục hạng + tiêu chí, gợi ý hạng theo số liệu thật
    /// (sản phẩm đang xuất bản, lượt liên hệ, lượt xem, xác minh pháp lý), quản trị gán hạng.
    /// </summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class SupplierTiersAdminController : Controller
    {
        public const string AutoAssigner = "Tự động theo tiêu chí";
        private readonly AppDbContext _context;
        private readonly ISupplierStoreService _store;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "SupplierTiersAdmin";
        private static readonly string[] LogFunctionAliases = { "SupplierTiers" };

        public SupplierTiersAdminController(AppDbContext context, ISupplierStoreService store, IConfiguration configuration)
        {
            _context = context;
            _store = store;
            _configuration = configuration;
        }

        private int GetSiteId() => int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;
        private Task WriteLog(int eventId, string content) =>
            CmsLogHelper.WriteLogAsync(_context, HttpContext, GetSiteId(), LogFunctionName, LogFunctionAliases, eventId, content);

        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, int? tierId, bool mismatch = false)
        {
            var tiers = await _context.SupplierTiers.AsNoTracking().OrderByDescending(t => t.Rank).ToListAsync();
            var query = _context.NhaCungUngs.AsNoTracking().Where(n => n.LanguageId == 1);
            if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(n => n.FullName != null && n.FullName.Contains(keyword));
            if (tierId.HasValue) query = tierId == 0 ? query.Where(n => n.TierId == null) : query.Where(n => n.TierId == tierId);
            var suppliers = await query.OrderBy(n => n.FullName)
                .Select(n => new { n.CungUngId, n.FullName, n.TierId, n.TierAssignedBy, n.TierAssignedAt, n.IsActivated }).ToListAsync();

            var metrics = await _store.GetMetricsAsync(suppliers.Select(s => s.CungUngId).ToList());
            var rows = suppliers.Select(s =>
            {
                var m = metrics[s.CungUngId];
                return new SupplierTierRow
                {
                    CungUngId = s.CungUngId, Name = s.FullName ?? "", IsActivated = s.IsActivated == true,
                    TierId = s.TierId, AssignedBy = s.TierAssignedBy, AssignedAt = s.TierAssignedAt,
                    Metrics = m, SuggestedTierId = _store.SuggestTier(m, tiers)?.Id
                };
            }).ToList();
            if (mismatch) rows = rows.Where(r => r.SuggestedTierId != r.TierId).ToList();

            ViewBag.Tiers = tiers;
            ViewBag.Keyword = keyword;
            ViewBag.TierId = tierId;
            ViewBag.Mismatch = mismatch;
            return View(rows.OrderByDescending(r => tiers.FirstOrDefault(t => t.Id == r.TierId)?.Rank ?? 0).ThenByDescending(r => r.Metrics.PublishedProducts).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTier(SupplierTier input)
        {
            if (string.IsNullOrWhiteSpace(input.Name)) return Json(new { success = false, message = "Nhập tên hạng." });
            if (input.MinProducts < 0 || input.MinContacts < 0 || input.MinViews < 0) return Json(new { success = false, message = "Tiêu chí không được âm." });
            if (!System.Text.RegularExpressions.Regex.IsMatch(input.BadgeColor ?? "", "^#[0-9a-fA-F]{6}$")) input.BadgeColor = "#6c757d";

            var t = input.Id > 0 ? await _context.SupplierTiers.FindAsync(input.Id) : null;
            if (t == null) { t = new SupplierTier(); _context.SupplierTiers.Add(t); }
            t.Name = input.Name.Trim();
            t.Rank = input.Rank;
            t.BadgeColor = input.BadgeColor!;
            t.MinProducts = input.MinProducts;
            t.MinContacts = input.MinContacts;
            t.MinViews = input.MinViews;
            t.RequireVerified = input.RequireVerified;
            t.Description = input.Description?.Trim();
            t.IsActive = input.IsActive;
            await _context.SaveChangesAsync();
            await WriteLog(2, $"Lưu hạng nhà cung cấp #{t.Id} '{t.Name}'");
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int cungUngId, int? tierId)
        {
            var n = await _context.NhaCungUngs.FirstOrDefaultAsync(x => x.CungUngId == cungUngId && x.LanguageId == 1);
            if (n == null) return Json(new { success = false, message = "Không tìm thấy nhà cung cấp." });
            if (tierId.HasValue && !await _context.SupplierTiers.AnyAsync(t => t.Id == tierId)) return Json(new { success = false, message = "Hạng không hợp lệ." });
            n.TierId = tierId;
            n.TierAssignedAt = DateTime.Now;
            n.TierAssignedBy = User.Identity?.Name;
            await _context.SaveChangesAsync();
            await WriteLog(2, $"Gán hạng nhà cung cấp #{cungUngId}: {(tierId?.ToString() ?? "bỏ hạng")}");
            return Json(new { success = true });
        }

        /// <summary>Áp dụng hạng gợi ý cho nhà cung cấp chưa được quản trị gán tay.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplySuggestions()
        {
            var tiers = await _context.SupplierTiers.AsNoTracking().ToListAsync();
            var suppliers = await _context.NhaCungUngs.Where(n => n.LanguageId == 1 && (n.TierAssignedBy == null || n.TierAssignedBy == AutoAssigner)).ToListAsync();
            var metrics = await _store.GetMetricsAsync(suppliers.Select(s => s.CungUngId).ToList());
            int changed = 0;
            foreach (var s in suppliers)
            {
                var suggested = _store.SuggestTier(metrics[s.CungUngId], tiers)?.Id;
                if (s.TierId == suggested) continue;
                s.TierId = suggested;
                s.TierAssignedAt = DateTime.Now;
                s.TierAssignedBy = AutoAssigner;
                changed++;
            }
            await _context.SaveChangesAsync();
            await WriteLog(2, $"Áp dụng hạng gợi ý: {changed} nhà cung cấp");
            return Json(new { success = true, message = $"Đã cập nhật hạng cho {changed} nhà cung cấp (bỏ qua nhà cung cấp đã được gán tay)." });
        }
    }

    public class SupplierTierRow
    {
        public int CungUngId { get; set; }
        public string Name { get; set; } = "";
        public bool IsActivated { get; set; }
        public int? TierId { get; set; }
        public string? AssignedBy { get; set; }
        public DateTime? AssignedAt { get; set; }
        public SupplierMetrics Metrics { get; set; } = null!;
        public int? SuggestedTierId { get; set; }
    }
}
