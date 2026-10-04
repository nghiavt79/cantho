using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>Quản lý bình luận theo người dùng: xem, sửa, ẩn, xóa (HSMT A.5.4, A.7.1).</summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class CommentsAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "CommentsAdmin";
        private static readonly string[] LogFunctionAliases = { "Comments" };

        public CommentsAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private Task WriteLog(int eventId, string content) =>
            CmsLogHelper.WriteLogAsync(_context, HttpContext, GetSiteId(), LogFunctionName, LogFunctionAliases, eventId, content);

        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, string? user, int? userId, byte? status, int page = 1, int pageSize = 30)
        {
            if (page < 1) page = 1;
            var query = from c in _context.UserComments.AsNoTracking()
                        join u in _context.Users.AsNoTracking() on c.UserId equals u.Id
                        join p in _context.SanPhamCNTBs.AsNoTracking() on new { c.TargetId, T = (int)c.TargetType } equals new { TargetId = p.ID, T = (int)CommentTargetTypes.Product } into pj
                        from p in pj.DefaultIfEmpty()
                        join n in _context.Contents.AsNoTracking() on new { Id = (long)c.TargetId, T = (int)c.TargetType } equals new { n.Id, T = (int)CommentTargetTypes.Post } into nj
                        from n in nj.DefaultIfEmpty()
                        join s in _context.NhaCungUngs.AsNoTracking() on new { c.TargetId, T = (int)c.TargetType } equals new { TargetId = s.CungUngId, T = (int)CommentTargetTypes.Store } into sj
                        from s in sj.DefaultIfEmpty()
                        select new { c, u.UserName, u.FullName, ProductName = p != null ? p.Name : n != null ? n.Title : s != null ? s.FullName : null,
                                     PostMenuId = n != null ? n.MenuId : null, PostSlug = n != null ? n.QueryString : null };

            if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(x => x.c.Content.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(user)) query = query.Where(x => x.UserName.Contains(user) || (x.FullName != null && x.FullName.Contains(user)));
            if (userId.HasValue) query = query.Where(x => x.c.UserId == userId.Value);
            if (status.HasValue) query = query.Where(x => x.c.StatusId == status.Value);

            var total = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.c.Created)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new CommentAdminRow
                {
                    Id = x.c.Id, TargetType = x.c.TargetType, TargetId = x.c.TargetId, TargetName = x.ProductName,
                    TargetUrl = x.c.TargetType == CommentTargetTypes.Product ? "/san-pham/chi-tiet/x-" + x.c.TargetId
                              : x.c.TargetType == CommentTargetTypes.Store ? "/nha-cung-ung/x-" + x.c.TargetId
                              : x.PostMenuId == 44 ? "/tin-tuc-su-kien/" + x.PostSlug + "-" + x.c.TargetId
                              : "/" + x.PostMenuId + "/" + x.PostSlug + "-" + x.c.TargetId,
                    UserId = x.c.UserId, UserName = x.UserName, FullName = x.FullName, Content = x.c.Content,
                    StatusId = x.c.StatusId, Created = x.c.Created, Modified = x.c.Modified, ModifiedBy = x.c.ModifiedBy,
                    IpAddress = x.c.IpAddress
                }).ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.User = user;
            ViewBag.UserId = userId;
            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewBag.Total = total;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, byte status)
        {
            var c = await _context.UserComments.FindAsync(id);
            if (c == null) return Json(new { success = false, message = "Không tìm thấy bình luận." });
            c.StatusId = status == 0 ? (byte)0 : (byte)1;
            c.Modified = DateTime.Now;
            c.ModifiedBy = User.Identity?.Name;
            await _context.SaveChangesAsync();
            await WriteLog(2, $"Comment #{id}: {(c.StatusId == 1 ? "hiện" : "ẩn")}");
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content)
        {
            content = (content ?? "").Trim();
            if (content.Length == 0 || content.Length > 2000)
                return Json(new { success = false, message = "Nội dung từ 1 đến 2000 ký tự." });
            var c = await _context.UserComments.FindAsync(id);
            if (c == null) return Json(new { success = false, message = "Không tìm thấy bình luận." });
            c.Content = content;
            c.Modified = DateTime.Now;
            c.ModifiedBy = User.Identity?.Name;
            await _context.SaveChangesAsync();
            await WriteLog(2, $"Sửa bình luận #{id}");
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.UserComments.FindAsync(id);
            if (c == null) return Json(new { success = false, message = "Không tìm thấy bình luận." });
            _context.UserComments.Remove(c);
            await _context.SaveChangesAsync();
            await WriteLog(3, $"Xóa bình luận #{id} của UserId={c.UserId}: {(c.Content.Length > 200 ? c.Content[..200] : c.Content)}");
            return Json(new { success = true });
        }
    }

    public class CommentAdminRow
    {
        public int Id { get; set; }
        public byte TargetType { get; set; }
        public int TargetId { get; set; }
        public string? TargetName { get; set; }
        public string? TargetUrl { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string? FullName { get; set; }
        public string Content { get; set; } = "";
        public byte StatusId { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
        public string? ModifiedBy { get; set; }
        public string? IpAddress { get; set; }
    }
}
