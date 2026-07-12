using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;

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
            return View(order);
        }
    }
}
