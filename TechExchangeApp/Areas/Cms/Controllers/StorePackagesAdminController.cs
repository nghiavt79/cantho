using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Interfaces;
using TechExchangeApp.Services;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>
    /// Gói gian hàng tính theo số sản phẩm (HSMT A.2.3): danh mục gói, cấp gói cho nhà cung cấp
    /// (tự ghi khoản thu "Phí gian hàng" vào sổ thu chi), xem mức sử dụng hạn mức.
    /// Bật/tắt gian hàng dùng cờ IsActivated có sẵn của nhà cung ứng (cấu hình nhanh ở CMS Nhà cung ứng).
    /// </summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class StorePackagesAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ISupplierStoreService _store;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "StorePackagesAdmin";
        private static readonly string[] LogFunctionAliases = { "StorePackages" };

        public StorePackagesAdminController(AppDbContext context, ISupplierStoreService store, IConfiguration configuration)
        {
            _context = context;
            _store = store;
            _configuration = configuration;
        }

        private int GetSiteId() => int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;
        private Task WriteLog(int eventId, string content) =>
            CmsLogHelper.WriteLogAsync(_context, HttpContext, GetSiteId(), LogFunctionName, LogFunctionAliases, eventId, content);

        [HttpGet]
        public async Task<IActionResult> Index([FromServices] ISystemParameterService parameters)
        {
            ViewBag.Packages = await _context.StorePackages.AsNoTracking().OrderBy(p => p.SortOrder).ThenBy(p => p.MaxProducts).ToListAsync();
            var today = DateTime.Today;
            ViewBag.Subscriptions = await (from s in _context.SupplierSubscriptions.AsNoTracking()
                                           join n in _context.NhaCungUngs.AsNoTracking() on s.CungUngId equals n.CungUngId
                                           join p in _context.StorePackages.AsNoTracking() on s.PackageId equals p.Id
                                           orderby s.Status descending, s.EndDate descending
                                           select new SubscriptionRow
                                           {
                                               Id = s.Id, CungUngId = s.CungUngId, SupplierName = n.FullName ?? "", PackageName = p.Name,
                                               StartDate = s.StartDate, EndDate = s.EndDate, MaxProducts = s.MaxProducts, Price = s.Price,
                                               Status = s.Status, Active = s.Status == 1 && s.StartDate <= today && s.EndDate >= today,
                                               Used = _context.SanPhamCNTBs.Count(x => x.NCUId == s.CungUngId && x.LanguageId == 1)
                                           }).Take(500).ToListAsync();
            ViewBag.Suppliers = await _context.NhaCungUngs.AsNoTracking().Where(n => n.LanguageId == 1).OrderBy(n => n.FullName)
                .Select(n => new { n.CungUngId, n.FullName }).ToListAsync();
            ViewBag.Enforced = await parameters.GetBoolAsync("STORE_QUOTA_ENFORCED", false);
            ViewBag.FreeQuota = await parameters.GetIntAsync("STORE_FREE_PRODUCT_QUOTA", 5);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePackage(StorePackage input)
        {
            if (string.IsNullOrWhiteSpace(input.Name) || input.MaxProducts < 1 || input.Price < 0 || input.DurationDays < 1)
                return Json(new { success = false, message = "Nhập tên gói, số sản phẩm ≥ 1, giá ≥ 0, thời hạn ≥ 1 ngày." });
            var p = input.Id > 0 ? await _context.StorePackages.FindAsync(input.Id) : null;
            if (p == null) { p = new StorePackage(); _context.StorePackages.Add(p); }
            p.Name = input.Name.Trim();
            p.MaxProducts = input.MaxProducts;
            p.Price = input.Price;
            p.DurationDays = input.DurationDays;
            p.Description = input.Description?.Trim();
            p.SortOrder = input.SortOrder;
            p.IsActive = input.IsActive;
            await _context.SaveChangesAsync();
            await WriteLog(2, $"Lưu gói gian hàng #{p.Id} '{p.Name}'");
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSubscription(int cungUngId, int packageId, DateTime? startDate, string? note)
        {
            var pkg = await _context.StorePackages.FindAsync(packageId);
            if (pkg == null || !pkg.IsActive) return Json(new { success = false, message = "Gói không hợp lệ." });
            var supplier = await _context.NhaCungUngs.AsNoTracking().FirstOrDefaultAsync(n => n.CungUngId == cungUngId && n.LanguageId == 1);
            if (supplier == null) return Json(new { success = false, message = "Không tìm thấy nhà cung cấp." });

            var start = (startDate ?? DateTime.Today).Date;
            var sub = new SupplierSubscription
            {
                CungUngId = cungUngId, PackageId = pkg.Id, StartDate = start, EndDate = start.AddDays(pkg.DurationDays - 1),
                MaxProducts = pkg.MaxProducts, Price = pkg.Price, Status = 1, Note = note?.Trim(), CreatedBy = User.Identity?.Name
            };
            _context.SupplierSubscriptions.Add(sub);
            await _context.SaveChangesAsync();
            if (pkg.Price > 0)
                await _store.UpsertAutoIncomeAsync(FinanceRefTypes.Subscription, sub.Id, FinanceCategories.StoreFee, pkg.Price, cungUngId,
                    $"Gói gian hàng '{pkg.Name}' – {supplier.FullName} ({sub.StartDate:dd/MM/yyyy}–{sub.EndDate:dd/MM/yyyy})", User.Identity?.Name);
            await WriteLog(1, $"Cấp gói gian hàng #{sub.Id} '{pkg.Name}' cho nhà cung cấp #{cungUngId}");
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelSubscription(int id)
        {
            var s = await _context.SupplierSubscriptions.FindAsync(id);
            if (s == null) return Json(new { success = false, message = "Không tìm thấy." });
            s.Status = 0;
            await _context.SaveChangesAsync();
            await _store.RemoveAutoIncomeAsync(FinanceRefTypes.Subscription, id);
            await WriteLog(2, $"Hủy gói gian hàng #{id} của nhà cung cấp #{s.CungUngId}");
            return Json(new { success = true });
        }
    }

    public class SubscriptionRow
    {
        public int Id { get; set; }
        public int CungUngId { get; set; }
        public string SupplierName { get; set; } = "";
        public string PackageName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxProducts { get; set; }
        public decimal Price { get; set; }
        public byte Status { get; set; }
        public bool Active { get; set; }
        public int Used { get; set; }
    }
}
