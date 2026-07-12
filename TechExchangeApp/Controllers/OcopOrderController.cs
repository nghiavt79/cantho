using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.ViewModel;

namespace TechExchangeApp.Controllers
{
    // Standalone 3-step retail order flow for OCOP products (Đặt hàng -> NCC xác nhận & giao hàng -> Hoàn tất).
    // Intentionally does NOT go through Project/WorkflowService (the 14-step "chuyển giao công nghệ" process),
    // since buying OCOP specialty goods is a simple retail transaction, not a technology transfer negotiation.
    [Authorize]
    public class OcopOrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Services.INotificationQueueService _notifQueue;

        public OcopOrderController(AppDbContext context, UserManager<ApplicationUser> userManager, Services.INotificationQueueService notifQueue)
        {
            _context = context;
            _userManager = userManager;
            _notifQueue = notifQueue;
        }

        private int GetCurrentUserId()
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID");
            }
            return userId;
        }

        // GET: /OcopOrder/Create?productId=18
        [HttpGet]
        public async Task<IActionResult> Create(int productId)
        {
            var product = await _context.SanPhamCNTBs.AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID == productId && p.ProductType == 4);

            if (product == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var model = new OcopOrderRequest
            {
                ProductId = productId,
                SupplierId = product.NCUId,
                SoLuong = 1
            };

            if (user != null)
            {
                model.HoTen = user.FullName ?? "";
                model.DienThoai = user.Phone ?? user.PhoneNumber ?? "";
                model.Email = user.Email ?? "";
                model.DiaChiGiao = user.DiaChi ?? "";
            }

            ViewBag.Product = product;
            return View(model);
        }

        // POST: /OcopOrder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OcopOrderRequest model)
        {
            var product = await _context.SanPhamCNTBs.AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID == model.ProductId && p.ProductType == 4);

            if (product == null) return NotFound();

            if (ModelState.IsValid)
            {
                var userId = GetCurrentUserId();

                model.SupplierId = product.NCUId;
                model.NguoiTao = userId;
                model.NgayTao = DateTime.Now;
                model.StatusId = 1; // Mới đặt

                _context.OcopOrderRequests.Add(model);
                await _context.SaveChangesAsync();

                if (product.NCUId.HasValue)
                {
                    var supplier = await _context.NhaCungUngs.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.CungUngId == product.NCUId.Value);
                    if (supplier?.UserId.HasValue == true)
                    {
                        await _notifQueue.QueueAsync(supplier.UserId.Value, null,
                            "Có đơn đặt mua sản phẩm OCOP mới",
                            $"{model.HoTen} vừa đặt mua {model.SoLuong} \"{product.Name}\". Vui lòng liên hệ khách hàng qua {model.DienThoai} để xác nhận và giao hàng.");
                    }
                }

                return RedirectToAction("Success", new { id = model.Id });
            }

            ViewBag.Product = product;
            return View(model);
        }

        // GET: /OcopOrder/Success/5
        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var userId = GetCurrentUserId();
            var order = await _context.OcopOrderRequests.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id && o.NguoiTao == userId);

            if (order == null) return NotFound();

            var product = await _context.SanPhamCNTBs.AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID == order.ProductId);

            ViewBag.Product = product;

            if (order.HinhThucThanhToan == 2 && order.SupplierId.HasValue)
            {
                ViewBag.Supplier = await _context.NhaCungUngs.AsNoTracking()
                    .FirstOrDefaultAsync(n => n.CungUngId == order.SupplierId.Value);
            }

            return View(order);
        }

        // GET: /OcopOrder/Index — "Đơn hàng OCOP của tôi" (buyer)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            var orders = await (
                from o in _context.OcopOrderRequests.AsNoTracking()
                where o.NguoiTao == userId
                join p in _context.SanPhamCNTBs.AsNoTracking() on o.ProductId equals p.ID into products
                from p in products.DefaultIfEmpty()
                join n in _context.NhaCungUngs.AsNoTracking() on o.SupplierId equals n.CungUngId into suppliers
                from n in suppliers.DefaultIfEmpty()
                orderby o.NgayTao descending
                select new OcopOrderVm
                {
                    Id = o.Id,
                    ProductId = o.ProductId,
                    ProductName = p != null ? p.Name ?? "" : "",
                    HoTen = o.HoTen,
                    DienThoai = o.DienThoai,
                    DiaChiGiao = o.DiaChiGiao,
                    SoLuong = o.SoLuong,
                    GhiChu = o.GhiChu,
                    StatusId = o.StatusId,
                    HinhThucThanhToan = o.HinhThucThanhToan,
                    NgayTao = o.NgayTao,
                    SoTaiKhoan = n != null ? n.SoTaiKhoan : null,
                    TenNganHang = n != null ? n.TenNganHang : null,
                    ChuTaiKhoan = n != null ? n.ChuTaiKhoan : null
                }).ToListAsync();

            return View(orders);
        }

        // GET: /OcopOrder/Manage — quản lý đơn khách đặt (nhà cung ứng)
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var userId = GetCurrentUserId();
            var supplierId = await _context.NhaCungUngs.AsNoTracking()
                .Where(n => n.UserId == userId)
                .Select(n => (int?)n.CungUngId)
                .FirstOrDefaultAsync();

            if (!supplierId.HasValue) return Forbid();

            var orders = await (
                from o in _context.OcopOrderRequests.AsNoTracking()
                where o.SupplierId == supplierId.Value
                join p in _context.SanPhamCNTBs.AsNoTracking() on o.ProductId equals p.ID into products
                from p in products.DefaultIfEmpty()
                orderby o.NgayTao descending
                select new OcopOrderVm
                {
                    Id = o.Id,
                    ProductId = o.ProductId,
                    ProductName = p != null ? p.Name ?? "" : "",
                    HoTen = o.HoTen,
                    DienThoai = o.DienThoai,
                    DiaChiGiao = o.DiaChiGiao,
                    SoLuong = o.SoLuong,
                    GhiChu = o.GhiChu,
                    StatusId = o.StatusId,
                    HinhThucThanhToan = o.HinhThucThanhToan,
                    NgayTao = o.NgayTao
                }).ToListAsync();

            return View(orders);
        }

        // POST: /OcopOrder/UpdateStatus — nhà cung ứng xác nhận / hoàn tất / huỷ đơn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int statusId)
        {
            var userId = GetCurrentUserId();
            var supplierId = await _context.NhaCungUngs.AsNoTracking()
                .Where(n => n.UserId == userId)
                .Select(n => (int?)n.CungUngId)
                .FirstOrDefaultAsync();

            if (!supplierId.HasValue) return Forbid();

            var order = await _context.OcopOrderRequests
                .FirstOrDefaultAsync(o => o.Id == id && o.SupplierId == supplierId.Value);

            if (order == null) return NotFound();
            if (statusId < 1 || statusId > 4) return BadRequest();
            if (order.StatusId == 3 || order.StatusId == 4) return RedirectToAction("Manage"); // already final

            order.StatusId = statusId;
            order.NguoiSua = userId;
            order.NgaySua = DateTime.Now;
            await _context.SaveChangesAsync();

            if (statusId == 4)
            {
                var product = await _context.SanPhamCNTBs.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ID == order.ProductId);
                if (order.NguoiTao.HasValue)
                {
                    await _notifQueue.QueueAsync(order.NguoiTao.Value, null,
                        "Đơn đặt mua OCOP đã bị huỷ",
                        $"Nhà cung ứng đã huỷ đơn đặt mua \"{product?.Name}\". Vui lòng liên hệ nhà cung ứng nếu cần biết thêm chi tiết.");
                }
            }

            return RedirectToAction("Manage");
        }

        // POST: /OcopOrder/Cancel — người mua tự huỷ đơn khi còn "Mới đặt"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetCurrentUserId();
            var order = await _context.OcopOrderRequests
                .FirstOrDefaultAsync(o => o.Id == id && o.NguoiTao == userId);

            if (order == null) return NotFound();

            if (order.StatusId == 1)
            {
                order.StatusId = 4; // Đã huỷ
                order.NguoiSua = userId;
                order.NgaySua = DateTime.Now;
                await _context.SaveChangesAsync();

                if (order.SupplierId.HasValue)
                {
                    var supplier = await _context.NhaCungUngs.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.CungUngId == order.SupplierId.Value);
                    var product = await _context.SanPhamCNTBs.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.ID == order.ProductId);
                    if (supplier?.UserId.HasValue == true)
                    {
                        await _notifQueue.QueueAsync(supplier.UserId.Value, null,
                            "Khách hàng đã huỷ đơn đặt mua OCOP",
                            $"{order.HoTen} đã huỷ đơn đặt mua \"{product?.Name}\".");
                    }
                }
            }

            return RedirectToAction("Index");
        }
    }
}
