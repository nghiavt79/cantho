using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QRCoder;
using TechExchangeApp.Data;
using TechExchangeApp.Helpers;
using TechExchangeApp.Interfaces;
using TechExchangeApp.ViewModel;

namespace TechExchangeApp.Controllers
{
    public class OcopController : Controller
    {
        private const int OcopProductType = 4;

        private readonly IProductService _productService;
        private readonly AppDbContext _context;
        private readonly string _mainDomain;

        public OcopController(IProductService productService, AppDbContext context, IOptions<AppSettings> appSettings)
        {
            _productService = productService;
            _context = context;
            _mainDomain = appSettings.Value.MainDomain;
        }

        private const int PageSize = 12;

        // Route: /ocop — Góc trưng bày Sản phẩm OCOP & Truy xuất nguồn gốc
        // Phân trang + IsHot xếp trước, lọc theo ngôn ngữ (bản tiếng Anh là dòng LanguageId = 2).
        public async Task<IActionResult> Index(int page = 1)
        {
            var langId = LangHelper.CurrentLangId(HttpContext);
            if (page < 1) page = 1;

            var totalCount = await _productService.GetProductCountByProductTypeAsync(OcopProductType, langId);
            var products = await _productService.GetPagedProductsByProductTypeAsync(
                OcopProductType, langId, page, PageSize);

            var model = new OcopIndexViewModel
            {
                Products = products,
                CurrentPage = page,
                PageSize = PageSize,
                TotalCount = totalCount,
                TotalProducts = totalCount,
                TotalTraceable = await _context.SanPhamCNTBs.CountAsync(x => x.ProductType == OcopProductType && x.LanguageId == langId && x.StatusId == 3 && x.MaTruyXuat != null),
                TotalOrigins = await _context.SanPhamCNTBs
                    .Where(x => x.ProductType == OcopProductType && x.LanguageId == langId && x.StatusId == 3 && x.XuatXu != null)
                    .Select(x => x.XuatXu)
                    .Distinct()
                    .CountAsync()
            };

            return View(model);
        }

        // Route: /ocop/{slug}-{id} — hồ sơ truy xuất nguồn gốc sản phẩm OCOP
        public async Task<IActionResult> Detail(int id)
        {
            bool isEn = LangHelper.IsEnglish(HttpContext);
            var langId = LangHelper.CurrentLangId(HttpContext);

            var product = await _context.SanPhamCNTBs.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == id && x.ProductType == OcopProductType
                                          && x.LanguageId == langId && x.StatusId == 3);

            if (product == null)
                return Redirect(isEn ? $"{_mainDomain}en/ocop" : $"{_mainDomain}ocop");

            var traceUrl = isEn
                ? $"{_mainDomain}en/ocop/{ProductController.MakeURLFriendly(product.Name)}-{product.ID}"
                : $"{_mainDomain}ocop/{ProductController.MakeURLFriendly(product.Name)}-{product.ID}";

            // Cùng ngôn ngữ với trang đang xem, IsHot xếp trước — nhất quán với listing.
            var relatedProducts = await _context.SanPhamCNTBs.AsNoTracking()
                .Where(x => x.ProductType == OcopProductType && x.LanguageId == langId
                            && x.StatusId == 3 && x.ID != id)
                .OrderByDescending(x => x.IsHot == true)
                .ThenByDescending(x => x.Modified ?? x.Created)
                .Take(4)
                .ToListAsync();

            var model = new OcopDetailViewModel
            {
                Product = product,
                TraceUrl = traceUrl,
                QrDataUri = BuildQrDataUri(traceUrl),
                RelatedProducts = relatedProducts
            };

            if (product.NCUId.HasValue)
            {
                var supplier = await _context.NhaCungUngs.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.CungUngId == product.NCUId.Value);
                if (supplier != null)
                {
                    model.SupplierName = supplier.FullName;
                    model.SupplierUrl = isEn
                        ? $"{_mainDomain}en/suppliers/{ProductController.MakeURLFriendly(supplier.FullName)}-{supplier.CungUngId}"
                        : $"{_mainDomain}nha-cung-ung/{ProductController.MakeURLFriendly(supplier.FullName)}-{supplier.CungUngId}";
                }
            }

            return View(model);
        }

        private static string BuildQrDataUri(string content)
        {
            using var generator = new QRCodeGenerator();
            using var qrData = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            var bytes = qrCode.GetGraphic(8);
            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        }
    }
}
