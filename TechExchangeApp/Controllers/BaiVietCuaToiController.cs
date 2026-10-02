using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Helpers;

namespace TechExchangeApp.Controllers
{
    /// <summary>
    /// Nhà cung cấp gửi bài viết để Sàn duyệt (HSMT A.2.8).
    /// Lưu nháp = StatusId 1, gửi duyệt = 2; quản trị xuất bản (3) hoặc trả về (4) trong CMS Tin bài.
    /// Nội dung nhập dạng văn bản thuần, được mã hóa HTML khi lưu — không nhận HTML từ người dùng.
    /// </summary>
    [Authorize]
    public class BaiVietCuaToiController : Controller
    {
        public const int MenuTinTuc = 44;
        private const int StatusDraft = 1, StatusPending = 2, StatusReturned = 4;

        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BaiVietCuaToiController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private int CurrentUserId => int.Parse(_userManager.GetUserId(User)!);
        private string CurrentUserName => User.Identity?.Name ?? "";

        private Task<string?> SupplierNameAsync()
            => _context.NhaCungUngs.AsNoTracking().Where(n => n.UserId == CurrentUserId).Select(n => n.FullName).FirstOrDefaultAsync();

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var supplier = await SupplierNameAsync();
            ViewBag.IsSupplier = supplier != null;
            var me = CurrentUserName;
            var items = await _context.Contents.AsNoTracking()
                .Where(c => c.Creator == me && c.TypeId == SupplierPostTypeId)
                .OrderByDescending(c => c.Modified ?? c.Created)
                .Select(c => new SupplierPostRow { Id = c.Id, Title = c.Title ?? "", StatusId = c.StatusId, Created = c.Created, Modified = c.Modified, QueryString = c.QueryString })
                .ToListAsync();
            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Form(long? id)
        {
            if (await SupplierNameAsync() == null) return RedirectToAction(nameof(Index));
            var vm = new SupplierPostVm();
            if (id.HasValue)
            {
                var c = await FindOwnAsync(id.Value);
                if (c == null) return NotFound();
                if (!CanEdit(c.StatusId)) return RedirectToAction(nameof(Index));
                vm = new SupplierPostVm { Id = c.Id, Title = c.Title ?? "", Description = c.Description, Body = HtmlToPlain(c.Contents) };
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Form(SupplierPostVm vm, bool submit)
        {
            var supplier = await SupplierNameAsync();
            if (supplier == null) return RedirectToAction(nameof(Index));
            if (!ModelState.IsValid) return View(vm);

            Content c;
            if (vm.Id > 0)
            {
                c = await FindOwnAsync(vm.Id) ?? throw new InvalidOperationException();
                if (!CanEdit(c.StatusId)) return RedirectToAction(nameof(Index));
                c.Modified = DateTime.Now;
                c.Modifier = CurrentUserName;
            }
            else
            {
                c = new Content
                {
                    MenuId = MenuTinTuc, LanguageId = 1, Domain = "VN", SiteId = 1, ParentId = 0,
                    TypeId = SupplierPostTypeId, Created = DateTime.Now, Creator = CurrentUserName
                };
                _context.Contents.Add(c);
            }

            c.Title = vm.Title.Trim();
            c.QueryString = SlugHelper.Slugify(c.Title);
            c.Description = vm.Description?.Trim();
            c.Contents = PlainToHtml(vm.Body);
            c.Author = supplier;
            c.StatusId = submit ? StatusPending : StatusDraft;
            await _context.SaveChangesAsync();

            TempData["Success"] = submit ? "Đã gửi bài, đang chờ Sàn duyệt." : "Đã lưu nháp.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var c = await FindOwnAsync(id);
            if (c == null || !CanEdit(c.StatusId)) return Json(new { success = false });
            _context.Contents.Remove(c);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        /// <summary>TypeId đánh dấu bài do nhà cung cấp gửi (để CMS lọc và phân biệt với bài của Sàn).</summary>
        public const int SupplierPostTypeId = 9;

        private Task<Content?> FindOwnAsync(long id)
        {
            var me = CurrentUserName;
            return _context.Contents.FirstOrDefaultAsync(c => c.Id == id && c.Creator == me && c.TypeId == SupplierPostTypeId);
        }

        private static bool CanEdit(int? statusId) => statusId is null or StatusDraft or StatusReturned;

        private static string PlainToHtml(string? text)
        {
            var paragraphs = Regex.Split((text ?? "").Replace("\r\n", "\n").Trim(), @"\n\s*\n")
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => "<p>" + WebUtility.HtmlEncode(p.Trim()).Replace("\n", "<br />") + "</p>");
            return string.Join("\n", paragraphs);
        }

        private static string HtmlToPlain(string? html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            var s = Regex.Replace(html, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"</p>\s*", "\n\n", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, "<.*?>", "");
            return WebUtility.HtmlDecode(s).Trim();
        }
    }

    public class SupplierPostRow
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public int? StatusId { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public string? QueryString { get; set; }
    }

    public class SupplierPostVm
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
        [StringLength(500, ErrorMessage = "Tiêu đề tối đa 500 ký tự.")]
        public string Title { get; set; } = "";

        [StringLength(1000, ErrorMessage = "Tóm tắt tối đa 1000 ký tự.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        [StringLength(50000, ErrorMessage = "Nội dung quá dài.")]
        public string Body { get; set; } = "";
    }
}
