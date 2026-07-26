using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Helpers;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    // ── DTOs ──
    public class PhieuYeuCauListItem
    {
        public int PhieuYeuCauId { get; set; }
        public string? FullName { get; set; }
        public string? NoiDung { get; set; }
        public string? TenDonVi { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public bool? IsActivated { get; set; }
        public int? StatusId { get; set; }
        public string? StatusTitle { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Ngayyeucau { get; set; }
        public int? Viewed { get; set; }
    }

    // ── Controller ──
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class PhieuYeuCauCNTBAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "PhieuYeuCauCNTB";
        private static readonly string[] LogFunctionAliases = { "Yêu Cầu Công nghệ - Thiết bị" };

        public PhieuYeuCauCNTBAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private async Task WriteLog(int eventId, string content)
        {
            await CmsLogHelper.WriteLogAsync(_context, HttpContext, GetSiteId(),
                LogFunctionName, LogFunctionAliases, eventId, content);
        }

        // ── INDEX ──
        public async Task<IActionResult> Index(
            string? keyword, int? statusId, bool? isActivated,
            int? siteId,
            DateTime? createdFrom, DateTime? createdTo,
            string? sortBy, string? sortDir,
            int page = 1, int pageSize = 20)
        {
            if (!new[] { 10, 20, 50, 100 }.Contains(pageSize))
                pageSize = 20;

            var configSiteId = GetSiteId();

            var query = _context.PhieuYeuCauCNTBs.AsNoTracking()
                .Where(p => p.SiteId == null || p.SiteId == configSiteId);

            // Filters
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(kw)) ||
                    (p.FullName != null && p.FullName.ToLower().Contains(kw)) ||
                    (p.TenDonVi != null && p.TenDonVi.ToLower().Contains(kw)) ||
                    (p.Phone != null && p.Phone.Contains(kw)) ||
                    (p.Email != null && p.Email.ToLower().Contains(kw)));
            }
            if (statusId.HasValue)
                query = query.Where(p => p.StatusId == statusId.Value);
            if (isActivated.HasValue)
                query = query.Where(p => p.IsActivated == isActivated.Value);
            if (siteId.HasValue)
                query = query.Where(p => p.SiteId == siteId.Value);
            if (createdFrom.HasValue)
                query = query.Where(p => p.Created >= createdFrom.Value);
            if (createdTo.HasValue)
                query = query.Where(p => p.Created <= createdTo.Value.AddDays(1));

            // Sort
            query = sortBy?.ToLower() switch
            {
                "fullname" => sortDir == "desc" ? query.OrderByDescending(p => p.FullName) : query.OrderBy(p => p.FullName),
                "noidung" => sortDir == "desc" ? query.OrderByDescending(p => p.NoiDung) : query.OrderBy(p => p.NoiDung),
                "email" => sortDir == "desc" ? query.OrderByDescending(p => p.Email) : query.OrderBy(p => p.Email),
                "created" => sortDir == "desc" ? query.OrderByDescending(p => p.Created) : query.OrderBy(p => p.Created),
                "ngayyeucau" => sortDir == "desc" ? query.OrderByDescending(p => p.Ngayyeucau) : query.OrderBy(p => p.Ngayyeucau),
                _ => query.OrderByDescending(p => p.Created)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            // Statuses
            var statuses = await _context.Statuses.AsNoTracking()
                .OrderBy(s => s.StatusId).ToListAsync();
            var statusDict = statuses.ToDictionary(s => s.StatusId, s => s.Title);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PhieuYeuCauListItem
                {
                    PhieuYeuCauId = p.PhieuYeuCauId,
                    FullName = p.FullName,
                    NoiDung = p.NoiDung,
                    TenDonVi = p.TenDonVi,
                    Phone = p.Phone,
                    Email = p.Email,
                    DiaChi = p.DiaChi,
                    IsActivated = p.IsActivated,
                    StatusId = p.StatusId,
                    CreatedBy = p.CreatedBy,
                    Created = p.Created,
                    Ngayyeucau = p.Ngayyeucau,
                    Viewed = p.Viewed
                })
                .ToListAsync();

            // Map status titles
            foreach (var item in items)
            {
                if (item.StatusId.HasValue && statusDict.TryGetValue(item.StatusId.Value, out var sTitle))
                    item.StatusTitle = sTitle;
            }

            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Keyword = keyword;
            ViewBag.StatusId = statusId;
            ViewBag.IsActivated = isActivated;
            ViewBag.CreatedFrom = createdFrom?.ToString("yyyy-MM-dd");
            ViewBag.CreatedTo = createdTo?.ToString("yyyy-MM-dd");
            ViewBag.Statuses = statuses;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.SiteId = siteId;
            ViewBag.CurrentSiteId = configSiteId;
            ViewBag.Sites = await _context.RootSites.AsNoTracking().OrderBy(s => s.SiteId).ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ListPartial", items);

            return View(items);
        }

        // ── DELETE ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.PhieuYeuCauCNTBs.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu yêu cầu." });

            _context.PhieuYeuCauCNTBs.Remove(entity);
            await _context.SaveChangesAsync();

            await WriteLog(3, $"Delete PhieuYeuCauCNTB: ID={id}");

            return Json(new { success = true, message = "Đã xóa phiếu yêu cầu #" + id });
        }

        // ── TOGGLE ACTIVATE ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivate(int id)
        {
            var entity = await _context.PhieuYeuCauCNTBs.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu yêu cầu." });

            entity.IsActivated = !(entity.IsActivated ?? false);
            await _context.SaveChangesAsync();

            var status = entity.IsActivated == true ? "Kích hoạt" : "Hủy kích hoạt";
            await WriteLog(2, $"ToggleActivate PhieuYeuCauCNTB: ID={id} -> {status}");

            return Json(new { success = true, message = $"Đã {status.ToLower()} phiếu yêu cầu #{id}", activated = entity.IsActivated });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishToMarket(int id, string? title)
        {
            var entity = await _context.PhieuYeuCauCNTBs.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu tìm mua." });

            var existingContent = await _context.ContentsYeuCaus
                .FirstOrDefaultAsync(c => c.PhieuYeuCauCNTBId == id);

            if (existingContent != null)
            {
                existingContent.StatusId = 3;
                existingContent.MenuId ??= 67;
                existingContent.TypeId ??= 7;
                existingContent.PublishedDate ??= DateTime.Now;
                existingContent.bEffectiveDate ??= DateTime.Now;
                existingContent.Modified = DateTime.Now;
                existingContent.Modifier = User.Identity?.Name;

                entity.StatusId = 3;
                entity.IsActivated = true;
                await _context.SaveChangesAsync();

                await WriteLog(2, $"Republish PhieuYeuCauCNTB #{id} to existing ContentsYeuCau #{existingContent.Id}");

                return Json(new
                {
                    success = true,
                    message = "Da cap nhat trang thai dang, noi dung CMS duoc giu nguyen.",
                    url = $"/tim-mua-cong-nghe/{existingContent.QueryString}-{existingContent.Id}"
                });
            }

            var cleanTitle = string.IsNullOrWhiteSpace(title)
                ? BuildDefaultTitle(entity)
                : title.Trim();

            var slug = SlugHelper.Slugify(cleanTitle);
            if (string.IsNullOrWhiteSpace(slug))
                slug = $"tim-mua-cong-nghe-{id}";

            var existingSlug = slug;
            var suffix = 2;
            while (await _context.ContentsYeuCaus.AnyAsync(c => c.QueryString == existingSlug && c.MenuId == 67))
            {
                existingSlug = $"{slug}-{suffix++}";
            }

            var description = BuildDescription(entity.NoiDung);
            var contents = BuildContents(entity);

            var content = new ContentsYeuCau
            {
                Title = cleanTitle,
                QueryString = existingSlug,
                Description = description,
                Contents = contents,
                Author = User.Identity?.Name ?? "Biên tập",
                StatusId = 3,
                MenuId = 67,
                TypeId = 7,
                Created = DateTime.Now,
                Creator = User.Identity?.Name,
                PublishedDate = DateTime.Now,
                bEffectiveDate = DateTime.Now,
                LanguageId = entity.LanguageId ?? 1,
                Domain = entity.Domain,
                SiteId = entity.SiteId ?? GetSiteId(),
                PhieuYeuCauCNTBId = entity.PhieuYeuCauId,
                Viewed = 0,
                Like = 0
            };

            _context.ContentsYeuCaus.Add(content);
            entity.Title = cleanTitle;
            entity.StatusId = 3;
            entity.IsActivated = true;
            await _context.SaveChangesAsync();

            await WriteLog(2, $"Publish PhieuYeuCauCNTB #{id} to ContentsYeuCau #{content.Id}");

            return Json(new
            {
                success = true,
                message = "Đã đưa nhu cầu tìm mua lên sàn.",
                url = $"/tim-mua-cong-nghe/{content.QueryString}-{content.Id}"
            });
        }

        private static string BuildDefaultTitle(PhieuYeuCauCNTB entity)
        {
            var text = entity.NoiDung?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return $"Tìm mua công nghệ #{entity.PhieuYeuCauId}";

            return text.Length <= 90 ? text : text[..90].Trim() + "...";
        }

        private static string BuildDescription(string? text)
        {
            var value = text?.Trim() ?? "";
            if (value.Length > 220)
                value = value[..220].Trim() + "...";
            return WebUtility.HtmlEncode(value);
        }

        private static string BuildContents(PhieuYeuCauCNTB entity)
        {
            static string E(string? value) => WebUtility.HtmlEncode(value?.Trim() ?? "");

            var lines = new List<string>
            {
                $"<p>{E(entity.NoiDung).Replace("\r\n", "<br>").Replace("\n", "<br>")}</p>"
            };

            if (!string.IsNullOrWhiteSpace(entity.TenDonVi))
                lines.Add($"<p><strong>Đơn vị:</strong> {E(entity.TenDonVi)}</p>");
            if (!string.IsNullOrWhiteSpace(entity.FullName))
                lines.Add($"<p><strong>Người liên hệ:</strong> {E(entity.FullName)}</p>");

            return string.Join(Environment.NewLine, lines);
        }
    }
}
