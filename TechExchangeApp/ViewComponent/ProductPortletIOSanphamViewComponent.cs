using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TechExchangeApp.Controllers;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Helpers;
using TechExchangeApp.ViewModel;

/// <summary>
/// Renders a portlet of related SanPhamCNTB items based on shared categories.
/// Uses EXISTS subquery — no JOIN, no DISTINCT — for optimal SQL Server execution plan.
///
/// Usage:
///   @await Component.InvokeAsync("ProductPortletIOSanpham", new { productId = Model.Product.ID })
///   @await Component.InvokeAsync("ProductPortletIOSanpham", new { productId = Model.Product.ID, productType = 1 })
/// </summary>
public class ProductPortletIOSanphamViewComponent : ViewComponent
{
    // Widget này chỉ dành cho sản phẩm Công nghệ/Thiết bị/Sản phẩm trí tuệ.
    // OCOP có khối "Sản phẩm OCOP khác" riêng (Views/Ocop/Detail.cshtml) nên
    // phải loại trừ ở đây, kể cả khi rơi vào nhánh fallback không lọc category.
    private const int OcopProductType = 4;

    private readonly AppDbContext _context;
    private readonly string _mainDomain;

    public ProductPortletIOSanphamViewComponent(AppDbContext context, IOptions<AppSettings> appSettings)
    {
        _context = context;
        _mainDomain = appSettings.Value.MainDomain;
    }

    /// <param name="productId">ID of the currently viewed SanPhamCNTB. Pass 0 when no product context (e.g. NhaTuVan page).</param>
    /// <param name="productType">Optional TypeId to narrow results (1=CongNghe, 2=SangChe, 3=ThietBi).</param>
    public IViewComponentResult Invoke(int productId = 0, int? productType = null)
    {
        // ── Fallback path: no product context ──────────────────────────────────
        if (productId == 0)
        {
            return TopProducts(productType, excludeId: null);
        }

        // ── Materialize category IDs for this product (one small, fast query) ────
        // Uses index on SanPhamCNTBCategory(SanPhamCNTBId).
        var categoryIds = _context.SanPhamCNTBCategories
            .Where(m => m.SanPhamCNTBId == productId)
            .Select(m => m.CatId)
            .ToList();   // small list, typically 1-5 entries

        if (categoryIds.Count == 0)
        {
            // Edge case: product exists but has no categories → top products fallback
            return TopProducts(productType, excludeId: productId);
        }

        // ── EXISTS pattern: no JOIN, no DISTINCT ───────────────────────────────
        // EF Core translates .Any(m => categoryIds.Contains(m.CatId)) to:
        //   EXISTS (SELECT 1 FROM SanPhamCNTBCategory m
        //           WHERE m.SanPhamCNTBId = p.ID
        //             AND m.CatId IN (1, 3, 7))   ← literal IN list
        // → Index seek on IX_SanPhamCNTBCategory_CatId, no hash join, no sort for DISTINCT.
        // Bản tiếng Anh là dòng riêng (LanguageId = 2), nên phải lọc theo ngôn ngữ
        // hiện tại — nếu không, trang /en/... sẽ hiện sản phẩm liên quan tiếng Việt.
        var langId = LangHelper.CurrentLangId(HttpContext);

        var products = _context.SanPhamCNTBs
            .AsNoTracking()
            .Where(p =>
                p.LanguageId == langId &&
                p.StatusId   == 3 &&
                p.ID         != productId &&
                p.ProductType != OcopProductType &&
                (!productType.HasValue || p.TypeId == productType.Value) &&
                _context.SanPhamCNTBCategories
                    .Any(m => m.SanPhamCNTBId == p.ID && categoryIds.Contains(m.CatId))
            )
            .OrderByDescending(p => p.Viewed)
            .Take(16)
            .ToList();

        return View(MapToVm(products));
    }

    // ── Fallback: top N products by Viewed ─────────────────────────────────────
    private IViewComponentResult TopProducts(int? productType, int? excludeId)
    {
        var langId = LangHelper.CurrentLangId(HttpContext);
        var q = _context.SanPhamCNTBs
            .AsNoTracking()
            .Where(p => p.LanguageId == langId && p.StatusId == 3 && p.ProductType != OcopProductType);

        if (productType.HasValue) q = q.Where(p => p.TypeId == productType.Value);
        if (excludeId.HasValue)   q = q.Where(p => p.ID != excludeId.Value);

        return View(MapToVm(q.OrderByDescending(p => p.Viewed).Take(16).ToList()));
    }

    // ── ViewModel mapping ───────────────────────────────────────────────────────
    private List<ProductPortletItemVm> MapToVm(List<SanPhamCNTB> products)
    {
        // URL chi tiết theo ngôn ngữ: route en_product_detail là "en/products/{slug}-{id}",
        // bản tiếng Việt là "san-pham/chi-tiet/{slug}-{id}". Trước đây luôn sinh URL tiếng
        // Việt nên bấm từ trang /en/... là rớt khỏi bản tiếng Anh.
        var detailPrefix = LangHelper.IsEnglish(HttpContext)
            ? "en/products/"
            : "san-pham/chi-tiet/";

        return products.Select(row => new ProductPortletItemVm
        {
            Id       = row.ID,
            Name     = row.Name,
            Code     = row.Code,
            Star     = row.Rating ?? 0,
            IsSC     = row.TypeId == 2,
            IsNC     = row.TypeId == 3,

            ImageUrl = string.IsNullOrEmpty(row.QuyTrinhHinhAnh)
                ? (row.TypeId == 2
                    ? _mainDomain + "images/sangche.png"
                    : _mainDomain + "images/research.jpg")
                : ProductController.CookedImageURL("254-170", row.QuyTrinhHinhAnh, _mainDomain),

            PriceText = row.OriginalPrice == null
                ? ""
                : ProductController.FormatCurrencyOto((decimal?)row.OriginalPrice, row.Currency),

            Url = _mainDomain +
                  detailPrefix +
                  ProductController.MakeURLFriendly(row.Name) +
                  "-" + row.ID
        }).ToList();
    }
}
