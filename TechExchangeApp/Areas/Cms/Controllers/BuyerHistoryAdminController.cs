using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>
    /// Lịch sử mua hàng theo người mua (HSMT A.5.2): gộp trên một màn hình các yêu cầu đặt mua / quan tâm,
    /// hồ sơ giao dịch công nghệ, phiếu yêu cầu, tin tìm mua, giỏ hàng, đăng ký sự kiện và thuê gian hàng.
    /// Chỉ đọc — mỗi mục quản lý chi tiết ở màn hình CMS riêng của nó.
    /// </summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class BuyerHistoryAdminController : Controller
    {
        private readonly AppDbContext _context;
        public BuyerHistoryAdminController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Index(int? userId, string? find)
        {
            if (!string.IsNullOrWhiteSpace(find))
                ViewBag.FindResults = await _context.Users.AsNoTracking()
                    .Where(u => u.UserName.Contains(find) || (u.FullName != null && u.FullName.Contains(find)) || (u.Email != null && u.Email.Contains(find)) || (u.PhoneNumber != null && u.PhoneNumber.Contains(find)))
                    .OrderBy(u => u.UserName).Take(30)
                    .Select(u => new BuyerLookupRow { Id = u.Id, UserName = u.UserName, FullName = u.FullName, Email = u.Email, Phone = u.PhoneNumber })
                    .ToListAsync();
            ViewBag.Find = find;
            if (!userId.HasValue) return View(null);

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();
            var uid = user.Id;
            var email = user.Email ?? "";
            var userName = user.UserName;

            var vm = new BuyerHistoryVm
            {
                UserId = uid, UserName = userName, FullName = user.FullName, Email = user.Email, Phone = user.PhoneNumber,
                Created = user.Created, LastLogin = user.LastLogin, IsLocked = user.IsLocked,

                Orders = await (from o in _context.OcopOrderRequests.AsNoTracking()
                                join p in _context.SanPhamCNTBs.AsNoTracking() on o.ProductId equals p.ID into pj
                                from p in pj.DefaultIfEmpty()
                                join n in _context.NhaCungUngs.AsNoTracking() on o.SupplierId equals n.CungUngId into nj
                                from n in nj.DefaultIfEmpty()
                                where o.NguoiTao == uid || (email != "" && o.Email == email)
                                orderby o.NgayTao descending
                                select new BuyerOrderRow
                                {
                                    Id = o.Id, ProductId = o.ProductId, ProductName = p != null ? p.Name : null, ProductType = o.RequestProductType,
                                    SupplierName = n != null ? n.FullName : null, Quantity = o.SoLuong, Price = o.GiaThamKhao,
                                    StatusId = o.StatusId, Created = o.NgayTao
                                }).ToListAsync(),

                Projects = await _context.Projects.AsNoTracking().Where(p => p.CreatedBy == uid)
                    .OrderByDescending(p => p.CreatedDate)
                    .Select(p => new BuyerProjectRow
                    {
                        Id = p.Id, Code = p.ProjectCode, Name = p.ProjectName, StatusId = p.StatusId, Created = p.CreatedDate,
                        Contracts = _context.ProjectContracts.Count(c => c.ProjectId == p.Id)
                    }).ToListAsync(),

                Requests = await _context.PhieuYeuCauCNTBs.AsNoTracking()
                    .Where(r => r.UserId == uid || (email != "" && r.Email == email))
                    .OrderByDescending(r => r.Created)
                    .Select(r => new BuyerSimpleRow { Id = r.PhieuYeuCauId, Title = r.Title, Created = r.Created, Status = r.StatusId })
                    .ToListAsync(),

                Demands = await _context.ContentsYeuCaus.AsNoTracking()
                    .Where(c => c.Creator == userName)
                    .OrderByDescending(c => c.Created)
                    .Select(c => new BuyerSimpleRow { Id = c.Id, Title = c.Title, Created = c.Created, Status = c.StatusId })
                    .ToListAsync(),

                Cart = await (from c in _context.ShoppingCarts.AsNoTracking()
                              join p in _context.SanPhamCNTBs.AsNoTracking() on c.ProductId equals (long?)p.ID into pj
                              from p in pj.DefaultIfEmpty()
                              where c.UserId == uid
                              orderby c.DateCreated descending
                              select new BuyerSimpleRow { Id = c.RecordId, Title = p != null ? p.Name : ("#" + c.ProductId), Created = c.DateCreated, Status = c.Quantity })
                             .ToListAsync(),

                Events = await (from r in _context.EventRegistrations.AsNoTracking()
                                join c in _context.Contents.AsNoTracking() on r.ContentId equals c.Id
                                where r.UserId == uid || (email != "" && r.Email == email)
                                orderby r.Created descending
                                select new BuyerSimpleRow { Id = r.Id, Title = c.Title, Created = r.Created, Status = r.Status })
                               .ToListAsync(),

                Booths = await (from b in _context.EventBoothBookings.AsNoTracking()
                                join t in _context.EventBoothTypes.AsNoTracking() on b.BoothTypeId equals t.Id
                                join c in _context.Contents.AsNoTracking() on b.ContentId equals c.Id
                                where b.UserId == uid || (email != "" && b.Email == email)
                                orderby b.Created descending
                                select new BuyerSimpleRow { Id = b.Id, Title = c.Title + " – " + t.Name + " x" + b.Quantity, Created = b.Created, Status = b.Status })
                               .ToListAsync(),

                Favorites = await _context.ProductFavorites.CountAsync(f => f.UserId == uid),
                Comments = await _context.UserComments.CountAsync(c => c.UserId == uid)
            };
            return View(vm);
        }
    }

    public class BuyerLookupRow
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

    public class BuyerHistoryVm
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsLocked { get; set; }
        public List<BuyerOrderRow> Orders { get; set; } = new();
        public List<BuyerProjectRow> Projects { get; set; } = new();
        public List<BuyerSimpleRow> Requests { get; set; } = new();
        public List<BuyerSimpleRow> Demands { get; set; } = new();
        public List<BuyerSimpleRow> Cart { get; set; } = new();
        public List<BuyerSimpleRow> Events { get; set; } = new();
        public List<BuyerSimpleRow> Booths { get; set; } = new();
        public int Favorites { get; set; }
        public int Comments { get; set; }
    }

    public class BuyerOrderRow
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? ProductType { get; set; }
        public string? SupplierName { get; set; }
        public int Quantity { get; set; }
        public string? Price { get; set; }
        public int StatusId { get; set; }
        public DateTime Created { get; set; }
    }

    public class BuyerProjectRow
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int StatusId { get; set; }
        public DateTime Created { get; set; }
        public int Contracts { get; set; }
    }

    public class BuyerSimpleRow
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public DateTime? Created { get; set; }
        public int? Status { get; set; }
    }
}
