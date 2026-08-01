using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net;
using TechExchangeApp.Areas.Cms.Models;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Helpers;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class ContentsYeuCauAdminController : Controller
    {
        private const int YeuCauMenuId = 67;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public ContentsYeuCauAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        public async Task<IActionResult> Index(
            string? keyword,
            int? statusId,
            string? source,
            int? siteId,
            int page = 1,
            int pageSize = 20)
        {
            if (!new[] { 10, 20, 50, 100 }.Contains(pageSize))
                pageSize = 20;

            ViewData["Title"] = "Quản lý nhu cầu công khai";

            var configSiteId = GetSiteId();
            var effectiveSiteId = siteId ?? configSiteId;

            var query = _context.ContentsYeuCaus.AsNoTracking()
                .Where(c => c.MenuId == YeuCauMenuId);

            if (effectiveSiteId > 0)
                query = query.Where(c => c.SiteId == null || c.SiteId == effectiveSiteId);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(c =>
                    c.Id.ToString().Contains(kw) ||
                    (c.Title != null && c.Title.Contains(kw)) ||
                    (c.Description != null && c.Description.Contains(kw)) ||
                    (c.Contents != null && c.Contents.Contains(kw)) ||
                    (c.Keyword != null && c.Keyword.Contains(kw)));
            }

            if (statusId.HasValue)
                query = query.Where(c => c.StatusId == statusId.Value);

            source = string.IsNullOrWhiteSpace(source) ? null : source.Trim().ToLowerInvariant();
            query = source switch
            {
                "phieu" => query.Where(c => c.PhieuYeuCauCNTBId != null),
                "cms" => query.Where(c => c.PhieuYeuCauCNTBId == null && (c.Creator != null || c.Modifier != null)),
                "unknown" => query.Where(c => c.PhieuYeuCauCNTBId == null && c.Creator == null && c.Modifier == null),
                _ => query
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            var statuses = await _context.Statuses.AsNoTracking()
                .OrderBy(s => s.StatusId)
                .Select(s => new { s.StatusId, s.Title })
                .ToListAsync();
            var statusMap = statuses.ToDictionary(s => s.StatusId, s => s.Title);

            var categories = await _context.Categories.AsNoTracking()
                .Where(c => c.ParentId == 1)
                .Select(c => new { c.CatId, c.Title })
                .ToListAsync();
            var categoryMap = categories.ToDictionary(c => c.CatId, c => c.Title);

            var rows = await query
                .OrderByDescending(c => c.Modified ?? c.PublishedDate ?? c.Created)
                .ThenByDescending(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.QueryString,
                    c.LinhVucId,
                    c.DiaPhuong,
                    c.Author,
                    c.StatusId,
                    c.Created,
                    c.Modified,
                    c.PublishedDate,
                    c.PhieuYeuCauCNTBId,
                    c.Creator,
                    c.Modifier
                })
                .ToListAsync();

            var items = rows.Select(c => new ContentsYeuCauListItemVm
            {
                Id = c.Id,
                Title = c.Title ?? "",
                QueryString = c.QueryString,
                LinhVucText = ResolveLinhVuc(c.LinhVucId, categoryMap),
                DiaPhuong = c.DiaPhuong,
                Author = c.Author,
                StatusId = c.StatusId,
                StatusTitle = c.StatusId.HasValue && statusMap.TryGetValue(c.StatusId.Value, out var title) ? title : null,
                Created = c.Created,
                Modified = c.Modified,
                PublishedDate = c.PublishedDate,
                PhieuYeuCauCNTBId = c.PhieuYeuCauCNTBId,
                SourceLabel = ResolveSourceLabel(c.PhieuYeuCauCNTBId, c.Creator, c.Modifier)
            }).ToList();

            ViewBag.Keyword = keyword;
            ViewBag.StatusId = statusId;
            ViewBag.Source = source;
            ViewBag.SiteId = siteId;
            ViewBag.CurrentSiteId = configSiteId;
            ViewBag.Statuses = statuses;
            ViewBag.Sites = await _context.RootSites.AsNoTracking().OrderBy(s => s.SiteId).ToListAsync();
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ListPartial", items);

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? phieuYeuCauCNTBId)
        {
            ContentsYeuCauEditVm vm;

            if (phieuYeuCauCNTBId.HasValue)
            {
                var existingContent = await _context.ContentsYeuCaus.AsNoTracking()
                    .Where(c => c.PhieuYeuCauCNTBId == phieuYeuCauCNTBId.Value)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync();

                if (existingContent > 0)
                    return RedirectToAction(nameof(Edit), new { id = existingContent });

                var phieu = await _context.PhieuYeuCauCNTBs.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PhieuYeuCauId == phieuYeuCauCNTBId.Value);

                if (phieu == null)
                    return NotFound();

                var title = BuildDefaultTitle(phieu);
                vm = new ContentsYeuCauEditVm
                {
                    Title = title,
                    QueryString = SlugHelper.Slugify(title),
                    Description = BuildDescription(phieu.NoiDung),
                    Contents = BuildContents(phieu),
                    Author = User.Identity?.Name ?? "Biên tập",
                    StatusId = 1,
                    MenuId = YeuCauMenuId,
                    TypeId = 7,
                    LanguageId = phieu.LanguageId ?? 1,
                    Domain = phieu.Domain,
                    SiteId = phieu.SiteId ?? GetSiteId(),
                    PublishedDate = DateTime.Now,
                    PhieuYeuCauCNTBId = phieu.PhieuYeuCauId,
                    SourceLabel = "Phiếu người dùng",
                    SourceDetail = $"Phiếu #{phieu.PhieuYeuCauId}"
                };
            }
            else
            {
                vm = new ContentsYeuCauEditVm
                {
                    StatusId = 1,
                    MenuId = YeuCauMenuId,
                    TypeId = 7,
                    LanguageId = 1,
                    Domain = _configuration["AppSettings:MainDomain"]?.TrimEnd('/') ?? "",
                    SiteId = GetSiteId(),
                    PublishedDate = DateTime.Now,
                    SourceLabel = "CMS"
                };
            }

            await LoadFormSelectListsAsync(vm);
            ViewData["Title"] = "Tạo nhu cầu công khai";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContentsYeuCauEditVm vm)
        {
            if (vm.PhieuYeuCauCNTBId.HasValue)
            {
                var duplicatedContent = await _context.ContentsYeuCaus.AsNoTracking()
                    .AnyAsync(c => c.PhieuYeuCauCNTBId == vm.PhieuYeuCauCNTBId.Value);

                if (duplicatedContent)
                    ModelState.AddModelError(nameof(vm.PhieuYeuCauCNTBId), "Phiếu này đã có nhu cầu công khai tương ứng.");
            }

            if (!ModelState.IsValid)
            {
                await LoadFormSelectListsAsync(vm);
                vm.SourceLabel = ResolveSourceLabel(vm.PhieuYeuCauCNTBId, vm.Creator, vm.Modifier);
                vm.SourceDetail = vm.PhieuYeuCauCNTBId.HasValue ? $"Phiếu #{vm.PhieuYeuCauCNTBId.Value}" : null;
                ViewData["Title"] = "Tạo nhu cầu công khai";
                return View(vm);
            }

            if (string.IsNullOrWhiteSpace(vm.QueryString))
                vm.QueryString = SlugHelper.Slugify(vm.Title);

            var slug = await EnsureUniqueSlugAsync(vm.QueryString, null);
            var now = DateTime.Now;
            var entity = new ContentsYeuCau
            {
                MenuId = YeuCauMenuId,
                TypeId = vm.TypeId ?? 7,
                LanguageId = vm.LanguageId == 0 ? 1 : vm.LanguageId,
                Domain = string.IsNullOrWhiteSpace(vm.Domain) ? (_configuration["AppSettings:MainDomain"]?.TrimEnd('/') ?? "") : vm.Domain,
                SiteId = vm.SiteId ?? GetSiteId(),
                Created = now,
                Creator = User.Identity?.Name,
                Viewed = 0,
                Like = 0,
                PhieuYeuCauCNTBId = vm.PhieuYeuCauCNTBId
            };

            vm.QueryString = slug;
            UpdateEntity(entity, vm);

            _context.ContentsYeuCaus.Add(entity);

            if (vm.PhieuYeuCauCNTBId.HasValue)
            {
                var phieu = await _context.PhieuYeuCauCNTBs.FindAsync(vm.PhieuYeuCauCNTBId.Value);
                if (phieu != null)
                {
                    phieu.Title = entity.Title;
                    phieu.StatusId = entity.StatusId == 3 ? 3 : phieu.StatusId;
                    phieu.IsActivated = true;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã tạo nhu cầu công khai.";
            return RedirectToAction(nameof(Edit), new { id = entity.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var entity = await _context.ContentsYeuCaus.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.MenuId == YeuCauMenuId);

            if (entity == null)
                return NotFound();

            await LoadFormSelectListsAsync(entity);
            ViewData["Title"] = $"Sửa nhu cầu: {entity.Title}";
            return View(MapToEditVm(entity));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContentsYeuCauEditVm vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateReadonlySourceAsync(vm);
                await LoadFormSelectListsAsync(vm);
                ViewData["Title"] = $"Sửa nhu cầu: {vm.Title}";
                return View(vm);
            }

            var entity = await _context.ContentsYeuCaus
                .FirstOrDefaultAsync(c => c.Id == vm.Id && c.MenuId == YeuCauMenuId);

            if (entity == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(vm.QueryString))
                vm.QueryString = SlugHelper.Slugify(vm.Title);
            vm.QueryString = await EnsureUniqueSlugAsync(vm.QueryString, vm.Id);

            UpdateEntity(entity, vm);
            entity.Modified = DateTime.Now;
            entity.Modifier = User.Identity?.Name;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật nhu cầu công khai.";
            return RedirectToAction(nameof(Edit), new { id = entity.Id });
        }

        // GET /cms/ContentsYeuCauAdmin/QuickConfig?id= — modal cấu hình nhanh (theo mẫu SanPhamCNTB)
        [HttpGet]
        public async Task<IActionResult> QuickConfig(long id)
        {
            var entity = await _context.ContentsYeuCaus.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.MenuId == YeuCauMenuId);

            if (entity == null)
                return NotFound();

            ViewBag.ItemId = entity.Id;
            ViewBag.ItemName = entity.Title;
            ViewBag.StatusId = entity.StatusId;
            ViewBag.PublishedDate = entity.PublishedDate;
            ViewBag.Statuses = await _context.Statuses.AsNoTracking()
                .OrderBy(s => s.StatusId)
                .Select(s => new { s.StatusId, s.Title })
                .ToListAsync();

            return PartialView("_QuickConfigPartial");
        }

        // POST /cms/ContentsYeuCauAdmin/QuickConfig — cập nhật trạng thái nhanh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickConfig(long id, int? statusId, DateTime? publishedDate)
        {
            var entity = await _context.ContentsYeuCaus
                .FirstOrDefaultAsync(c => c.Id == id && c.MenuId == YeuCauMenuId);

            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy nhu cầu công khai." });

            entity.StatusId = statusId;
            // Chuyển sang Xuất bản (3) mà chưa có ngày đăng thì set mặc định
            if (statusId == 3)
                entity.PublishedDate = publishedDate ?? entity.PublishedDate ?? DateTime.Now;
            else if (publishedDate.HasValue)
                entity.PublishedDate = publishedDate;

            entity.Modified = DateTime.Now;
            entity.Modifier = User.Identity?.Name;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã cập nhật trạng thái." });
        }

        private async Task LoadFormSelectListsAsync(ContentsYeuCau entity)
        {
            await LoadFormSelectListsAsync(new ContentsYeuCauEditVm
            {
                StatusId = entity.StatusId,
                LinhVucId = entity.LinhVucId,
                TrangThaiNhuCau = entity.TrangThaiNhuCau
            });
        }

        private async Task LoadFormSelectListsAsync(ContentsYeuCauEditVm vm)
        {
            ViewBag.Statuses = new SelectList(
                await _context.Statuses.AsNoTracking().OrderBy(s => s.StatusId).ToListAsync(),
                "StatusId", "Title", vm.StatusId);

            var categories = await _context.Categories.AsNoTracking()
                .Where(c => c.ParentId == 1)
                .OrderBy(c => c.Sort)
                .ThenBy(c => c.Title)
                .Select(c => new SelectListItem
                {
                    Value = c.CatId.ToString(),
                    Text = c.Title ?? ""
                })
                .ToListAsync();

            ViewBag.LinhVucs = new SelectList(categories, "Value", "Text", NormalizeSingleCategoryId(vm.LinhVucId));
            ViewBag.LinhVucList = categories; // cho checkbox multi-select (mẫu SanPhamCNTB)
        }

        private async Task PopulateReadonlySourceAsync(ContentsYeuCauEditVm vm)
        {
            var source = await _context.ContentsYeuCaus.AsNoTracking()
                .Where(c => c.Id == vm.Id)
                .Select(c => new
                {
                    c.MenuId,
                    c.TypeId,
                    c.LanguageId,
                    c.Domain,
                    c.SiteId,
                    c.Created,
                    c.Creator,
                    c.Modified,
                    c.Modifier,
                    c.PhieuYeuCauCNTBId
                })
                .FirstOrDefaultAsync();

            if (source == null)
                return;

            vm.MenuId = source.MenuId;
            vm.TypeId = source.TypeId;
            vm.LanguageId = source.LanguageId;
            vm.Domain = source.Domain;
            vm.SiteId = source.SiteId;
            vm.Created = source.Created;
            vm.Creator = source.Creator;
            vm.Modified = source.Modified;
            vm.Modifier = source.Modifier;
            vm.PhieuYeuCauCNTBId = source.PhieuYeuCauCNTBId;
            vm.SourceLabel = ResolveSourceLabel(source.PhieuYeuCauCNTBId, source.Creator, source.Modifier);
            vm.SourceDetail = source.PhieuYeuCauCNTBId.HasValue
                ? $"Phiếu #{source.PhieuYeuCauCNTBId.Value}"
                : null;
        }

        private static ContentsYeuCauEditVm MapToEditVm(ContentsYeuCau entity)
        {
            return new ContentsYeuCauEditVm
            {
                Id = entity.Id,
                Title = entity.Title ?? "",
                QueryString = entity.QueryString,
                Description = entity.Description,
                Contents = entity.Contents,
                LinhVucId = entity.LinhVucId,
                Keyword = entity.Keyword,
                Image = entity.Image,
                StatusId = entity.StatusId,
                PublishedDate = entity.PublishedDate,
                Author = entity.Author,
                URL = entity.URL,
                TrangThaiNhuCau = entity.TrangThaiNhuCau,
                DiaPhuong = entity.DiaPhuong,
                HanTiepNhan = entity.HanTiepNhan,
                NganSach = entity.NganSach,
                HinhThucHopTac = entity.HinhThucHopTac,
                MucTieu = entity.MucTieu,
                HienTrang = entity.HienTrang,
                YeuCauKyThuat = entity.YeuCauKyThuat,
                QuyMoTrienKhai = entity.QuyMoTrienKhai,
                TieuChiChonDoiTac = entity.TieuChiChonDoiTac,
                MenuId = entity.MenuId,
                TypeId = entity.TypeId,
                LanguageId = entity.LanguageId,
                Domain = entity.Domain,
                SiteId = entity.SiteId,
                Created = entity.Created,
                Creator = entity.Creator,
                Modified = entity.Modified,
                Modifier = entity.Modifier,
                PhieuYeuCauCNTBId = entity.PhieuYeuCauCNTBId,
                SourceLabel = ResolveSourceLabel(entity.PhieuYeuCauCNTBId, entity.Creator, entity.Modifier),
                SourceDetail = entity.PhieuYeuCauCNTBId.HasValue
                    ? $"Phiếu #{entity.PhieuYeuCauCNTBId.Value}"
                    : null
            };
        }

        private static void UpdateEntity(ContentsYeuCau entity, ContentsYeuCauEditVm vm)
        {
            entity.Title = vm.Title.Trim();
            entity.QueryString = vm.QueryString?.Trim();
            entity.Description = vm.Description;
            entity.Contents = vm.Contents;
            entity.LinhVucId = string.IsNullOrWhiteSpace(vm.LinhVucId) ? null : $";{vm.LinhVucId.Trim(';')};";
            entity.Keyword = vm.Keyword;
            entity.Image = vm.Image;
            entity.StatusId = vm.StatusId;
            entity.PublishedDate = vm.PublishedDate;
            entity.Author = vm.Author;
            entity.URL = vm.URL;
            entity.TrangThaiNhuCau = vm.TrangThaiNhuCau;
            entity.DiaPhuong = vm.DiaPhuong;
            entity.HanTiepNhan = vm.HanTiepNhan;
            entity.NganSach = vm.NganSach;
            entity.HinhThucHopTac = vm.HinhThucHopTac;
            entity.MucTieu = vm.MucTieu;
            entity.HienTrang = vm.HienTrang;
            entity.YeuCauKyThuat = vm.YeuCauKyThuat;
            entity.QuyMoTrienKhai = vm.QuyMoTrienKhai;
            entity.TieuChiChonDoiTac = vm.TieuChiChonDoiTac;
        }

        private async Task<string> EnsureUniqueSlugAsync(string? slug, long? currentId)
        {
            var baseSlug = string.IsNullOrWhiteSpace(slug) ? "tim-mua-cong-nghe" : slug.Trim();
            var candidate = baseSlug;
            var suffix = 2;

            while (await _context.ContentsYeuCaus.AnyAsync(c =>
                c.MenuId == YeuCauMenuId &&
                c.QueryString == candidate &&
                (!currentId.HasValue || c.Id != currentId.Value)))
            {
                candidate = $"{baseSlug}-{suffix++}";
            }

            return candidate;
        }

        private static string BuildDefaultTitle(PhieuYeuCauCNTB phieu)
        {
            var text = phieu.NoiDung?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return $"Tìm mua công nghệ #{phieu.PhieuYeuCauId}";

            return text.Length <= 90 ? text : text[..90].Trim() + "...";
        }

        private static string BuildDescription(string? text)
        {
            // Mô tả ngắn hiển thị trong <textarea>/plain-text — KHÔNG HtmlEncode ở đây
            // (view tự encode khi xuất). Trước đây encode gây hiện &#39; &amp; ... trong ô nhập.
            var value = text?.Trim() ?? "";
            if (value.Length > 220)
                value = value[..220].Trim() + "...";
            return value;
        }

        private static string BuildContents(PhieuYeuCauCNTB phieu)
        {
            static string E(string? value) => WebUtility.HtmlEncode(value?.Trim() ?? "");

            var lines = new List<string>
            {
                $"<p>{E(phieu.NoiDung).Replace("\r\n", "<br>").Replace("\n", "<br>")}</p>"
            };

            if (!string.IsNullOrWhiteSpace(phieu.TenDonVi))
                lines.Add($"<p><strong>Đơn vị:</strong> {E(phieu.TenDonVi)}</p>");
            if (!string.IsNullOrWhiteSpace(phieu.FullName))
                lines.Add($"<p><strong>Người liên hệ:</strong> {E(phieu.FullName)}</p>");
            if (!string.IsNullOrWhiteSpace(phieu.Phone))
                lines.Add($"<p><strong>Điện thoại:</strong> {E(phieu.Phone)}</p>");
            if (!string.IsNullOrWhiteSpace(phieu.Email))
                lines.Add($"<p><strong>Email:</strong> {E(phieu.Email)}</p>");

            return string.Join(Environment.NewLine, lines);
        }

        private static string? ResolveLinhVuc(string? value, Dictionary<int, string?> map)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            foreach (var part in value.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(part, out var id) && map.TryGetValue(id, out var title))
                    return title;
            }

            return null;
        }

        private static string? NormalizeSingleCategoryId(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
        }

        private static string ResolveSourceLabel(int? phieuYeuCauId, string? creator, string? modifier)
        {
            if (phieuYeuCauId.HasValue)
                return "Phiếu người dùng";

            if (!string.IsNullOrWhiteSpace(creator) || !string.IsNullOrWhiteSpace(modifier))
                return "CMS";

            return "Dữ liệu cũ hoặc chưa xác định";
        }
    }
}
