using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Helpers;

namespace TechExchangeApp.Controllers
{
    /// <summary>Sản phẩm quan tâm của người mua (HSMT B.3.3).</summary>
    [Authorize]
    public class ProductFavoriteController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductFavoriteController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private int CurrentUserId => int.Parse(_userManager.GetUserId(User)!);

        // POST /ProductFavorite/Toggle  (AJAX, header RequestVerificationToken)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            // Bản EN (LanguageId=2) có ID riêng — luôn lưu theo ID bản gốc tiếng Việt
            var prod = await _context.SanPhamCNTBs.AsNoTracking()
                .Where(p => p.ID == id)
                .Select(p => new { p.ID, p.LanguageId, p.OriginalId })
                .FirstOrDefaultAsync();
            if (prod == null) return Json(new { success = false });
            id = prod.LanguageId == LangHelper.En && prod.OriginalId.HasValue ? prod.OriginalId.Value : prod.ID;

            var userId = CurrentUserId;
            var row = await _context.ProductFavorites.FirstOrDefaultAsync(f => f.UserId == userId && f.SanPhamId == id);
            bool isFavorite;
            if (row == null)
            {
                _context.ProductFavorites.Add(new ProductFavorite { UserId = userId, SanPhamId = id, Created = DateTime.Now });
                isFavorite = true;
            }
            else
            {
                _context.ProductFavorites.Remove(row);
                isFavorite = false;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Bấm 2 lần liên tiếp: chỉ mục duy nhất (UserId, SanPhamId) đã có bản ghi — coi như đã quan tâm
                isFavorite = true;
            }

            var count = await _context.ProductFavorites.CountAsync(f => f.SanPhamId == id);
            return Json(new { success = true, isFavorite, count });
        }

        // GET /san-pham-quan-tam
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;
            var isEn = LangHelper.IsEnglish(HttpContext);

            var rows = await (from f in _context.ProductFavorites.AsNoTracking()
                              join p in _context.SanPhamCNTBs.AsNoTracking() on f.SanPhamId equals p.ID
                              where f.UserId == userId
                              orderby f.Created descending
                              select new { f.Created, p.ID, p.Name, p.QuyTrinhHinhAnh, p.ProductType, p.OriginalId })
                             .ToListAsync();

            // Trang tiếng Anh: dùng dòng bản EN (LanguageId=2, OriginalId = ID gốc) để lấy tên và URL
            var enRows = new Dictionary<int, (int Id, string? Name)>();
            if (isEn && rows.Count > 0)
            {
                var ids = rows.Select(r => r.ID).ToList();
                var list = await _context.SanPhamCNTBs.AsNoTracking()
                    .Where(p => p.LanguageId == LangHelper.En && p.OriginalId != null && ids.Contains(p.OriginalId.Value))
                    .Select(p => new { p.ID, p.Name, OriginalId = p.OriginalId!.Value })
                    .ToListAsync();
                foreach (var e in list) enRows.TryAdd(e.OriginalId, (e.ID, e.Name));
            }

            var items = rows.Select(r =>
            {
                var linkId = r.ID;
                var name = r.Name ?? "";
                // Chưa có bản EN thì trỏ về trang tiếng Việt (URL /en/ với ID bản Việt sẽ 404)
                (int Id, string? Name) en = default;
                var useEnUrl = isEn && enRows.TryGetValue(r.ID, out en);
                if (useEnUrl)
                {
                    linkId = en.Id;
                    if (!string.IsNullOrWhiteSpace(en.Name)) name = en.Name!;
                }
                var slug = ProductController.MakeURLFriendly(name);
                var url = r.ProductType == 4
                    ? (useEnUrl ? $"/en/ocop/{slug}-{linkId}" : $"/ocop/{slug}-{linkId}")
                    : (useEnUrl ? $"/en/products/{slug}-{linkId}" : $"/san-pham/chi-tiet/{slug}-{linkId}");
                return new FavoriteItemVm { ProductId = r.ID, Name = name, Image = r.QuyTrinhHinhAnh, Url = url, Created = r.Created };
            }).ToList();

            return View(items);
        }
    }

    public class FavoriteItemVm
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public string? Image { get; set; }
        public string Url { get; set; } = "";
        public DateTime Created { get; set; }
    }
}
