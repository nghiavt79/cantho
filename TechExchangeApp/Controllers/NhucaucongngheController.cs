using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Enums;
using TechExchangeApp.Helpers;
using TechExchangeApp.Interfaces;
using TechExchangeApp.ViewModel;

namespace TechExchangeApp.Controllers.FrontEnd
{
    public class NhucaucongngheController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string _mainDomain;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<NhucaucongngheController> _logger;
        private readonly IConfiguration _configuration;

        private const string CaptchaSessionKey = "TechNeedCaptchaAnswer";

        public NhucaucongngheController(
            AppDbContext context,
            IOptions<AppSettings> appSettings,
            IEmailSender emailSender,
            ILogger<NhucaucongngheController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _mainDomain = appSettings.Value.MainDomain;
            _emailSender = emailSender;
            _logger = logger;
            _configuration = configuration;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        // Sinh math captcha (kiểu trang liên hệ), lưu đáp án vào Session, trả câu hỏi.
        private string GenerateCaptcha()
        {
            var rng = new Random();
            int a = rng.Next(2, 12);
            int b = rng.Next(1, 10);
            bool useAdd = rng.Next(0, 2) == 0;
            int bigger = Math.Max(a, b);
            int smaller = Math.Min(a, b);

            int answer;
            string question;
            if (useAdd) { answer = a + b; question = $"{a} + {b}"; }
            else { answer = bigger - smaller; question = $"{bigger} - {smaller}"; }

            HttpContext.Session.SetString(CaptchaSessionKey, answer.ToString());
            return question;
        }

        // Nếu người dùng đã đăng nhập, tự điền Họ tên / Email / SĐT từ hồ sơ tài khoản.
        private void PrefillFromCurrentUser(PhieuYeuCauCNViewModel vm)
        {
            if (vm == null || User?.Identity?.IsAuthenticated != true) return;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return;

            var u = _context.Users
                .Where(x => x.Id == userId)
                .Select(x => new { x.FullName, x.Email, x.PhoneNumber })
                .FirstOrDefault();
            if (u == null) return;

            if (string.IsNullOrWhiteSpace(vm.FullName)) vm.FullName = u.FullName;
            if (string.IsNullOrWhiteSpace(vm.Email)) vm.Email = u.Email;
            if (string.IsNullOrWhiteSpace(vm.Phone)) vm.Phone = u.PhoneNumber;
        }


        public IActionResult CateTechNeeds(
            int menuId,
            string? linhvuc,
            string? keyword,
            string sort = "newest",
            int page = 1)
        {
            ViewData["TechNeedPageTitle"] = "Tìm mua công nghệ";
            ViewData["TechNeedFormTitle"] = "TÌM MUA CÔNG NGHỆ";
            ViewData["TechNeedFormIntro"] = "Vui lòng mô tả nhu cầu tìm mua công nghệ, thiết bị hoặc giải pháp để Sàn tiếp nhận và hỗ trợ kết nối.";

            var vm = new CateTechNeedsViewModel
            {
                MenuId = menuId,
                SelectedLinhVuc = linhvuc,
                Keyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim(),
                Sort = string.IsNullOrWhiteSpace(sort) ? "newest" : sort,
                CurrentPage = page
            };

            LoadLinhVuc(vm);
            BindToGrid(vm, page);

            // Math captcha cho form gửi nhu cầu
            vm.PhieuYeuCau.CaptchaQuestion = GenerateCaptcha();

            // Tự điền thông tin liên hệ nếu người dùng đã đăng nhập
            PrefillFromCurrentUser(vm.PhieuYeuCau);

            return View("~/Views/Nhucaucongnghe/TechNeedsByMenu.cshtml", vm);

        }

        #region === PRIVATE METHODS (GIỮ NGUYÊN LOGIC) ===

        private void LoadLinhVuc(CateTechNeedsViewModel vm)
        {
            vm.LinhVucs.Add(new SelectListItem
            {
                Text = " --- Chọn lĩnh vực --- ",
                Value = ""
            });

            var list = _context.Categories
                .Where(x => x.ParentId == 1)
                .Select(x => new SelectListItem
                {
                    Value = x.CatId.ToString(),
                    Text = x.Title
                })
                .ToList();

            vm.LinhVucs.AddRange(list);
        }

        private void BindToGrid(CateTechNeedsViewModel vm, int page)
        {
            int pageSize = 9;
            int lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var subMenus = _context.UspSelectSubMenu(vm.MenuId);

            // Map lĩnh vực (CatId -> Title) để dựng badge trên card
            var lvMap = _context.Categories
                .Where(c => c.ParentId == 1)
                .ToDictionary(c => c.CatId, c => c.Title);

            var query = _context.ContentsYeuCaus.Where(q =>
                (q.MenuId == vm.MenuId || subMenus.Contains(q.MenuId ?? 0)) &&
                q.LanguageId == lang &&
                q.StatusId == 3 &&
                q.PublishedDate <= DateTime.Now &&
                (q.eEffectiveDate >= DateTime.Now || q.eEffectiveDate == null));

            // Lọc lĩnh vực theo token CatId chính xác. Không dùng Contains(id) trần vì
            // CatId=4 có thể match nhầm ;14;, ;40;, ;104;...
            if (int.TryParse(vm.SelectedLinhVuc, out var selectedLinhVucId))
            {
                var id = selectedLinhVucId.ToString();
                query = query.Where(q =>
                    q.LinhVucId != null &&
                    (
                        q.LinhVucId == id ||
                        q.LinhVucId.StartsWith(id + ";") ||
                        q.LinhVucId.EndsWith(";" + id) ||
                        q.LinhVucId.Contains(";" + id + ";") ||
                        q.LinhVucId.StartsWith(id + ",") ||
                        q.LinhVucId.EndsWith("," + id) ||
                        q.LinhVucId.Contains("," + id + ",")
                    ));
            }

            // Tìm kiếm từ khóa (Title / Description / Keyword)
            if (!string.IsNullOrWhiteSpace(vm.Keyword))
            {
                var kw = vm.Keyword;
                query = query.Where(q =>
                    (q.Title != null && q.Title.Contains(kw)) ||
                    (q.Description != null && q.Description.Contains(kw)) ||
                    (q.Keyword != null && q.Keyword.Contains(kw)));
            }

            int total = query.Count();
            vm.TotalCount = total;

            // Sắp xếp
            query = vm.Sort == "oldest"
                ? query.OrderBy(q => q.PublishedDate)
                : query.OrderByDescending(q => q.PublishedDate);

            var rows = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            vm.Items = rows.Select(q =>
            {
                var it = MapItem(q);
                it.Author = q.Author;
                it.LinhVucText = ResolveLinhVucBadge(q.LinhVucId, lvMap);
                return it;
            }).ToList();

            CreatePager(vm, total, page, pageSize, 10);
        }

        // Lấy tên lĩnh vực đầu tiên từ chuỗi LinhVucId (vd ";4;12;") để làm badge
        private static string? ResolveLinhVucBadge(string? linhVucId, Dictionary<int, string?> map)
        {
            if (string.IsNullOrWhiteSpace(linhVucId)) return null;
            foreach (var part in linhVucId.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(part, out var id) && map.TryGetValue(id, out var title) && !string.IsNullOrWhiteSpace(title))
                    return title;
            }
            return null;
        }

        private TechNeedItemVm MapItem(dynamic q)
        {
            int menuId = q.MenuId == null ? 0 : Convert.ToInt32(q.MenuId);
            int id = q.Id == null ? 0 : Convert.ToInt32(q.Id);

            return new TechNeedItemVm
            {
                MenuId = menuId,
                Id = id,
                Title = q.Title,
                QueryString = q.QueryString,
                Image = q.Image,
                Description = q.Description,
                PublishedDate = q.PublishedDate,
                DetailUrl = $"{_mainDomain}tim-mua-cong-nghe/{q.QueryString}-{id}"
            };
        }


        private void CreatePager(
            CateTechNeedsViewModel vm,
            int totalRecord,
            int pageIndex,
            int pageSize,
            int page2Show)
        {
            int totalPage =
                totalRecord % pageSize == 0
                    ? totalRecord / pageSize
                    : totalRecord / pageSize + 1;

            vm.TotalPage = totalPage;
            vm.CurrentPage = pageIndex;

            IEnumerable<int> left =
                pageIndex <= page2Show
                    ? Enumerable.Range(1, pageIndex)
                    : Enumerable.Range(pageIndex - page2Show, page2Show);

            IEnumerable<int> right =
                pageIndex + page2Show <= totalPage
                    ? Enumerable.Range(pageIndex, page2Show + 1)
                    : Enumerable.Range(pageIndex, totalPage - pageIndex + 1);

            vm.Pages = left.Union(right).Distinct().ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiPhieuYeuCau(PhieuYeuCauCNViewModel model)
        {
            bool isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            try
            {
                // ── Anti-spam: honeypot field ──
                var honeypot = Request.Form["website_url"].ToString();
                if (!string.IsNullOrEmpty(honeypot))
                {
                    _logger.LogWarning("[AntiSpam] Honeypot triggered. IP={IP}", HttpContext.Connection.RemoteIpAddress);
                    return Redirect(_mainDomain + "page/thanks");
                }

                // ── Anti-spam: time-based (form must take >= 3 seconds) ──
                var formTimestamp = Request.Form["_ts"].ToString();
                if (long.TryParse(formTimestamp, out var ts))
                {
                    var elapsed = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ts;
                    if (elapsed < 3)
                    {
                        _logger.LogWarning("[AntiSpam] Too fast submission ({Sec}s). IP={IP}", elapsed, HttpContext.Connection.RemoteIpAddress);
                        return Redirect(_mainDomain + "page/thanks");
                    }
                }

                // ── Math captcha (server-side, one-time; regenerate cho lần sau) ──
                var correctCaptcha = HttpContext.Session.GetString(CaptchaSessionKey);
                var userCaptcha = model.CaptchaAnswer?.Trim() ?? "";
                bool captchaOk = !string.IsNullOrEmpty(correctCaptcha) && userCaptcha == correctCaptcha;
                var newCaptcha = GenerateCaptcha();
                if (!captchaOk)
                {
                    if (isAjax)
                        return Json(new { success = false, message = "Mã xác thực không đúng.", captcha = newCaptcha });
                    TempData["Error"] = "Mã xác thực không đúng.";
                    return RedirectToAction(nameof(CateTechNeeds));
                }

                // ── Anti-spam: basic content checks ──
                var noiDung = model.NoiDung?.Trim() ?? "";
                var email = model.Email?.Trim() ?? "";
                var phone = model.Phone?.Trim() ?? "";
                bool isSpam = string.IsNullOrEmpty(noiDung)
                    || phone == "555-666-0606"
                    || email.EndsWith("@example.com", StringComparison.OrdinalIgnoreCase)
                    || email.EndsWith("@email.tst", StringComparison.OrdinalIgnoreCase)
                    || email.EndsWith("@test.com", StringComparison.OrdinalIgnoreCase)
                    || noiDung.Contains("http://") || noiDung.Contains("https://")
                    || System.Text.RegularExpressions.Regex.IsMatch(noiDung, @"\[url[=\]]");

                // ── Assign UserId if logged in ──
                int? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (claim != null && int.TryParse(claim.Value, out var uid))
                        userId = uid;
                }

                var p = new PhieuYeuCauCNTB
                {
                    NoiDung = noiDung,
                    FullName = model.FullName?.Trim(),
                    HinhDaiDien = "",
                    DiaChi = "",
                    Phone = phone,
                    Email = email,
                    Created = DateTime.Now,
                    CreatedBy = model.FullName,
                    UserId = userId,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    IsActivated = true,
                    Domain = string.IsNullOrWhiteSpace(_mainDomain) ? "techport.vn" : new Uri(_mainDomain).Host,
                    StatusId = 1,
                    LanguageId = 1,
                    ParentId = 0,
                    Ngayyeucau = DateTime.Now,
                    SiteId = GetSiteId()
                };

                _context.PhieuYeuCauCNTBs.Add(p);
                await _context.SaveChangesAsync();

                // ── Send email notification to admin ──
                if (!isSpam)
                {
                    try
                    {
                        var adminEmail = _configuration["AppSettings:AdminEmail"] ?? "admin@techport.vn";
                        var subject = $"[TechPort] Nhu cầu tìm mua công nghệ mới #{p.PhieuYeuCauId}";
                        var body = $@"
<h3>Nhu cầu tìm mua công nghệ mới</h3>
<table style='border-collapse:collapse;width:100%;max-width:600px;'>
  <tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold;width:140px;'>Họ và tên</td><td style='padding:8px;border:1px solid #ddd;'>{p.FullName}</td></tr>
  <tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold;'>Email</td><td style='padding:8px;border:1px solid #ddd;'>{p.Email}</td></tr>
  <tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold;'>Điện thoại</td><td style='padding:8px;border:1px solid #ddd;'>{p.Phone}</td></tr>
  <tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold;'>Nội dung</td><td style='padding:8px;border:1px solid #ddd;'>{p.NoiDung}</td></tr>
  <tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold;'>Ngày gửi</td><td style='padding:8px;border:1px solid #ddd;'>{p.Created:dd/MM/yyyy HH:mm}</td></tr>
  <tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold;'>IP</td><td style='padding:8px;border:1px solid #ddd;'>{p.IPAddress}</td></tr>
</table>
<p style='margin-top:16px;color:#888;font-size:12px;'>Email này được gửi tự động từ hệ thống TechPort.</p>";

                        await _emailSender.SendAsync(adminEmail, subject, body, isHtml: true);
                        _logger.LogInformation("[PhieuYeuCau] Email sent to {Admin} for PhieuYeuCauId={Id}", adminEmail, p.PhieuYeuCauId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[PhieuYeuCau] Failed to send email for PhieuYeuCauId={Id}", p.PhieuYeuCauId);
                    }
                }
                else
                {
                    _logger.LogWarning("[AntiSpam] Spam detected, email skipped. PhieuYeuCauId={Id}, IP={IP}", p.PhieuYeuCauId, p.IPAddress);
                }

                if (isAjax)
                    return Json(new { success = true, message = "Đã tiếp nhận nhu cầu của bạn.", captcha = newCaptcha });

                return Redirect(_mainDomain + "page/thanks");
            }
            catch
            {
                if (isAjax)
                    return Json(new { success = false, message = "Gửi thất bại, vui lòng kiểm tra lại đường truyền hoặc thử lại sau." });

                TempData["Error"] =
                    "Gửi thất bại hãy kiểm tra lại đường truyền hoặc liên hệ với quản lý.";
                return RedirectToAction(nameof(CateTechNeeds));
            }
        }
        public IActionResult PortletYeuCauMoi()
        {
            var vm = new PortletYeuCauMoiViewModel();

            // === BindToGrid (GIỮ NGUYÊN LOGIC VB.NET) ===
            vm.Items = _context.PhieuYeuCauCNTBs
                .Where(x => x.StatusId == 3)
                .OrderByDescending(x => x.Created)
                .Take(10)
                .ToList();

            return PartialView(
                "_PortletYeuCauMoi.cshtml",
                vm
            );
        }


        // URL cũ {menuId}/yeu-cau/{slug}-{id} → 301 sang /tim-mua-cong-nghe/{slug}-{id}
        public IActionResult DetailLegacyRedirect(int id, string slug)
        {
            return RedirectPermanent($"{_mainDomain}tim-mua-cong-nghe/{slug}-{id}");
        }

        public IActionResult Detail(int id)
        {
            var vm = new NhuCauCongNgheDetailViewModel();

            vm.TargetId = id;

            // Map lĩnh vực (CatId -> Title) để dựng badge
            var lvMap = _context.Categories
                .Where(c => c.ParentId == 1)
                .ToDictionary(c => c.CatId, c => c.Title);

            // === LoadData(intID) ===
            var raw = _context.ContentsYeuCaus
                .Where(q => q.Id == id && q.StatusId == 3)
                .OrderByDescending(q => q.PublishedDate)
                .FirstOrDefault();

            if (raw == null)
                return NotFound();

            var p = new ContentYeucauDetailVm
            {
                Id = (int)raw.Id,
                Title = raw.Title,
                Description = raw.Description,
                Contents = raw.Contents,
                Author = raw.Author,
                QueryString = raw.QueryString,
                MenuId = (int)raw.MenuId,
                Viewed = raw.Viewed,
                Like = raw.Like,
                PublishedDate = raw.PublishedDate,
                Modified = raw.Modified,
                TypeId = (int)raw.TypeId,
                LinhVucText = ResolveLinhVucBadge(raw.LinhVucId, lvMap),
                TrangThaiNhuCau = raw.TrangThaiNhuCau,
                DiaPhuong = raw.DiaPhuong,
                HanTiepNhan = raw.HanTiepNhan,
                NganSach = raw.NganSach,
                HinhThucHopTac = raw.HinhThucHopTac,
                MucTieu = raw.MucTieu,
                HienTrang = raw.HienTrang,
                YeuCauKyThuat = raw.YeuCauKyThuat,
                QuyMoTrienKhai = raw.QuyMoTrienKhai,
                TieuChiChonDoiTac = raw.TieuChiChonDoiTac
            };

            vm.Detail = p;

            // === Update Viewed (GIỮ LOGIC) ===
            raw.Viewed = raw.Viewed == null ? 1 : raw.Viewed + 1;
            _context.SaveChanges();

            // === CommentTypeID ===
            vm.CommentTypeId = p.TypeId switch
            {
                7 => (int)CommentType.Yeucaucongnghe,
                8 => (int)CommentType.Yeucautuvan,
                9 => (int)CommentType.Yeucautimkiemdoitac,
                _ => null
            };

            // === Images (TypeId = 4) ===
            if (p.TypeId == 4)
            {
                vm.Images = _context.Albums
                    .Where(x => x.ContensID == p.Id)
                    .ToList();
            }

            // === Relation ===
            int lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var subMenuIds = _context
                .UspSelectSubMenu(p.MenuId)
                .Select(x => (long)x)
                .ToArray();

            var relRaw = _context.ContentsYeuCaus
                .Where(q =>
                    q.Id != id &&
                    q.StatusId == 3 &&
                    (
                        q.MenuId == p.MenuId ||
                        System.Linq.Enumerable.Contains(subMenuIds, (int)q.MenuId)
                    ) &&
                    q.LanguageId == lang
                )
                .OrderByDescending(q => q.PublishedDate)
                .Take(4)
                .Select(q => new { q.Id, q.Title, q.QueryString, q.MenuId, q.PublishedDate, q.Image, q.LinhVucId, q.TrangThaiNhuCau })
                .ToList();

            vm.Relations = relRaw.Select(q => new RelationItemVm
            {
                Id = (int)q.Id,
                Title = q.Title,
                QueryString = q.QueryString,
                MenuId = (int)q.MenuId,
                PublishedDate = q.PublishedDate,
                Image = q.Image,
                LinhVucText = ResolveLinhVucBadge(q.LinhVucId, lvMap),
                TrangThaiNhuCau = q.TrangThaiNhuCau,
                DetailUrl = $"{_mainDomain}tim-mua-cong-nghe/{q.QueryString}-{q.Id}"
            }).ToList();


            // Captcha cho modal liên hệ/tư vấn
            vm.PhieuYeuCau.CaptchaQuestion = GenerateCaptcha();

            // Tự điền thông tin liên hệ nếu người dùng đã đăng nhập
            PrefillFromCurrentUser(vm.PhieuYeuCau);

            ViewData["Title"] = p.Title;
            ViewData["MetaDescription"] = p.Description;
            ViewData["BackLink"] = _mainDomain + "tim-mua-cong-nghe";

            return View("Detail", vm);
        }

        #endregion
    }
}
