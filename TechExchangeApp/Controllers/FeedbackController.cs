using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Helpers;
using TechExchangeApp.ViewModel;

namespace TechExchangeApp.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly string _mainDomain;
        private readonly ILogger<FeedbackController> _logger;

        private const string CaptchaSessionKey = "MathCaptchaAnswer";

        // ===== GIỮ NGUYÊN LOGIC WEBFORMS =====
        private int SiteId => 1;
        private string DomainName => string.IsNullOrWhiteSpace(_mainDomain) ? "techport.vn" : new Uri(_mainDomain).Host;

        public FeedbackController(AppDbContext context, IConfiguration config, IOptions<AppSettings> appSettings, ILogger<FeedbackController> logger)
        {
            _context    = context;
            _config     = config;
            _mainDomain = appSettings.Value.MainDomain;
            _logger     = logger;
        }

        // ──────────────────────────────────────────────────────
        // GET: /lien-he-74.html
        // ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Index()
        {
            var vm = new FeedbackCreateViewModel();

            // ===== LOAD USER FROM SESSION =====
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var user = _context.Users.FirstOrDefault(x => x.Id == userId.Value);
                if (user != null)
                {
                    vm.FullName = user.FullName ?? "";
                    vm.Email    = user.Email    ?? "";
                }
            }

            // ===== LOAD MENU ID = 74 =====
            var menu = _context.Menus.FirstOrDefault(x => x.MenuId == 74);
            if (menu != null)
            {
                vm.Title       = menu.Title;
                vm.Description = menu.Description;
            }

            // ===== SINH MATH CAPTCHA =====
            vm.CaptchaQuestion = GenerateCaptcha();

            return View("Index", vm);
        }

        // ──────────────────────────────────────────────────────
        // POST: /lien-he-74.html
        // ──────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(FeedbackCreateViewModel vm)
        {
            var languageId = HttpContext.Session.GetInt32("LanguageId") ?? 1;
            var lastPost   = HttpContext.Session.GetString("PostedFeedback");
            var settingTime = _config.GetValue<int>("SettingTimeUpdatePageView");

            // ===== VALIDATE MATH CAPTCHA (server-side) =====
            var correctAnswer = HttpContext.Session.GetString(CaptchaSessionKey);
            var userAnswer    = vm.CaptchaAnswer?.Trim() ?? "";

            if (string.IsNullOrEmpty(correctAnswer) || userAnswer != correctAnswer)
            {
                // Regenerate question for re-display
                vm.CaptchaQuestion = GenerateCaptcha();
                ModelState.AddModelError("CaptchaAnswer",
                    languageId == 1
                        ? "Mã xác thực không đúng. Vui lòng tính lại."
                        : "Incorrect captcha. Please try again.");
                LoadMenuDescription(vm);
                return View("Index", vm);
            }

            // Xoá session answer sau khi dùng (one-time use)
            HttpContext.Session.Remove(CaptchaSessionKey);

            // ===== CHỐNG SPAM =====
            if (lastPost != null &&
                (DateTime.Now - DateTime.Parse(lastPost)).TotalSeconds < settingTime)
            {
                TempData["Alert"] = languageId == 1
                    ? "Ý kiến của bạn trước đó đang được xử lý. Vui lòng đợi ít phút trước khi gửi tiếp!"
                    : "Your comments are being processed. Please wait a few minutes.";

                return Redirect("/lien-he-74");
            }

            try
            {
                var feedback = new Feedback
                {
                    FullName = vm.FullName,
                    Email    = vm.Email,
                    Address  = vm.Address,
                    Phone    = vm.Phone,
                    Title    = vm.Title,
                    Content  = vm.Content,
                    Created  = DateTime.Now,
                    StatusId = 2,
                    SiteId   = SiteId,
                    Domain   = DomainName
                };

                _context.Feedbacks.Add(feedback);
                _context.SaveChanges();

                // ===== SAVE POST TIME =====
                HttpContext.Session.SetString("PostedFeedback", DateTime.Now.ToString("O"));

                TempData["Alert"] = languageId == 1
                    ? "Ý kiến của bạn đã được gửi. Cám ơn bạn đã đóng góp!"
                    : "Your comment has been submitted. Thanks!";

                return Redirect("/lien-he-74");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FeedbackController] Lỗi khi lưu feedback: {Message}", ex.Message);

                TempData["Alert"] = languageId == 1
                    ? "Lưu thất bại. Vui lòng kiểm tra lại."
                    : "Save failed. Please try again.";

                return Redirect("/lien-he-74");
            }
        }

        // ──────────────────────────────────────────────────────
        // Helper: sinh phép tính + lưu đáp án vào Session
        // ──────────────────────────────────────────────────────
        private string GenerateCaptcha()
        {
            var rng = new Random();
            int a = rng.Next(2, 12);
            int b = rng.Next(1, 10);

            // Quyết định phép tính: + hoặc - (đảm bảo kết quả không âm)
            bool useAdd = rng.Next(0, 2) == 0;

            int bigger  = Math.Max(a, b);
            int smaller = Math.Min(a, b);

            int answer;
            string question;

            if (useAdd)
            {
                answer   = a + b;
                question = $"{a} + {b}";
            }
            else
            {
                // Dùng bigger - smaller để luôn >= 0
                answer   = bigger - smaller;
                question = $"{bigger} - {smaller}";
            }

            HttpContext.Session.SetString(CaptchaSessionKey, answer.ToString());
            return question;
        }

        private void LoadMenuDescription(FeedbackCreateViewModel vm)
        {
            var menu = _context.Menus.FirstOrDefault(x => x.MenuId == 74);
            if (menu != null) vm.Description = menu.Description;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitViolationReport(ViolationReportDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.ViolationType) || string.IsNullOrWhiteSpace(dto.ReportContent))
                {
                    return Json(new { success = false, message = "Vui long nhap day du loai vi pham va noi dung bao cao." });
                }

                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue &&
                    (string.IsNullOrWhiteSpace(dto.FullName) ||
                     (string.IsNullOrWhiteSpace(dto.Email) && string.IsNullOrWhiteSpace(dto.Phone))))
                {
                    return Json(new { success = false, message = "Vui long nhap ho ten va email hoac so dien thoai." });
                }

                var settingTime = _config.GetValue<int>("SettingTimeUpdatePageView");
                if (settingTime <= 0) settingTime = 60;

                var lastPost = HttpContext.Session.GetString("PostedViolationReport");
                if (lastPost != null && DateTime.TryParse(lastPost, out var postedAt) &&
                    (DateTime.Now - postedAt).TotalSeconds < settingTime)
                {
                    return Json(new { success = false, message = $"Thao tac qua nhanh. Vui long doi {settingTime} giay truoc khi gui tiep." });
                }

                var content = new System.Text.StringBuilder();
                content.AppendLine("Loai phan anh: Bao cao vi pham");
                content.AppendLine($"Loai du lieu bi bao cao: {dto.TargetDataType}");
                if (!string.IsNullOrWhiteSpace(dto.TargetSubType))
                    content.AppendLine($"Phan loai du lieu: {dto.TargetSubType}");
                content.AppendLine($"ID du lieu: {dto.TargetId}");
                content.AppendLine($"Tieu de du lieu: {dto.TargetTitle}");
                content.AppendLine($"URL du lieu: {dto.TargetUrl}");
                content.AppendLine($"Loai vi pham: {dto.ViolationType}");
                content.AppendLine($"Nguoi bao cao: {dto.FullName}");
                content.AppendLine($"Email nguoi bao cao: {dto.Email}");
                content.AppendLine($"So dien thoai nguoi bao cao: {dto.Phone}");
                if (userId.HasValue)
                    content.AppendLine($"UserId nguoi bao cao: {userId.Value}");
                content.AppendLine($"Thoi gian gui: {DateTime.Now:yyyy-MM-dd HH:mm}");
                content.AppendLine();
                content.AppendLine("Noi dung bao cao:");
                content.AppendLine(System.Net.WebUtility.HtmlEncode(dto.ReportContent));

                var feedback = new Feedback
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Title = "Bao cao vi pham",
                    Content = content.ToString(),
                    Created = DateTime.Now,
                    StatusId = 2,
                    SiteId = SiteId,
                    Domain = DomainName
                };

                _context.Feedbacks.Add(feedback);
                _context.SaveChanges();

                HttpContext.Session.SetString("PostedViolationReport", DateTime.Now.ToString("O"));

                return Json(new { success = true, message = "Gui bao cao vi pham thanh cong. Cam on ban da dong gop!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot save violation report.");
                return Json(new { success = false, message = "Co loi he thong xay ra. Vui long thu lai sau." });
            }
        }
    }
}
