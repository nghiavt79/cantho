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

        // Route: /ocop — Góc trưng bày Sản phẩm OCOP & Truy xuất nguồn gốc
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetNewProductsByProductTypeAsync(OcopProductType, 24);

            var model = new OcopIndexViewModel
            {
                Products = products,
                TotalProducts = await _context.SanPhamCNTBs.CountAsync(x => x.ProductType == OcopProductType && x.StatusId == 3),
                TotalTraceable = await _context.SanPhamCNTBs.CountAsync(x => x.ProductType == OcopProductType && x.StatusId == 3 && x.MaTruyXuat != null),
                TotalOrigins = await _context.SanPhamCNTBs
                    .Where(x => x.ProductType == OcopProductType && x.StatusId == 3 && x.XuatXu != null)
                    .Select(x => x.XuatXu)
                    .Distinct()
                    .CountAsync()
            };

            return View(model);
        }

        // Route: /ocop/{slug}-{id} — hồ sơ truy xuất nguồn gốc sản phẩm OCOP
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.SanPhamCNTBs.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == id && x.ProductType == OcopProductType);

            if (product == null)
                return Redirect($"{_mainDomain}ocop");

            var traceUrl = $"{_mainDomain}ocop/{ProductController.MakeURLFriendly(product.Name)}-{product.ID}";

            var relatedProducts = await _context.SanPhamCNTBs.AsNoTracking()
                .Where(x => x.ProductType == OcopProductType && x.StatusId == 3 && x.ID != id)
                .OrderByDescending(x => x.Created)
                .Take(4)
                .ToListAsync();

            var model = new OcopDetailViewModel
            {
                Product = product,
                TraceUrl = traceUrl,
                QrDataUri = BuildQrDataUri(traceUrl),
                RelatedProducts = relatedProducts
            };

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
