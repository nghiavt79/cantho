using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Interfaces;
using TechExchangeApp.Services;

namespace TechExchangeApp.Controllers
{
    /// <summary>Trang nhập mã xác thực lớp 2 trước khi vào CMS (HSMT 2.11): /cms-xac-thuc.</summary>
    [Authorize]
    public class CmsMfaController : Controller
    {
        private readonly AppDbContext _db;
        private readonly CmsMfaService _mfa;
        private readonly IEmailSender _email;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CmsMfaController> _logger;

        public CmsMfaController(AppDbContext db, CmsMfaService mfa, IEmailSender email, IWebHostEnvironment env, ILogger<CmsMfaController> logger)
        {
            _db = db; _mfa = mfa; _email = email; _env = env; _logger = logger;
        }

        private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private static string SafeReturn(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/cms", StringComparison.OrdinalIgnoreCase) && !returnUrl.StartsWith("//") ? returnUrl : "/cms";

        [HttpGet]
        public async Task<IActionResult> Index(string? returnUrl)
        {
            ViewBag.ReturnUrl = SafeReturn(returnUrl);
            ViewBag.MaskedEmail = Mask(await _db.Users.Where(u => u.Id == UserId).Select(u => u.Email).FirstOrDefaultAsync());
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string? returnUrl)
        {
            var email = await _db.Users.Where(u => u.Id == UserId).Select(u => u.Email).FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["MfaError"] = "Tài khoản chưa có email để nhận mã. Liên hệ quản trị hệ thống.";
                return RedirectToAction(nameof(Index), new { returnUrl });
            }
            var code = _mfa.CreateCode(UserId);
            if (code == null)
            {
                TempData["MfaError"] = "Mã vừa được gửi, vui lòng đợi 60 giây trước khi gửi lại.";
                return RedirectToAction(nameof(Index), new { returnUrl });
            }
            try
            {
                await _email.SendAsync(email, "Mã xác thực đăng nhập CMS",
                    $"<p>Mã xác thực vào trang quản trị Sàn giao dịch công nghệ: <b style=\"font-size:20px\">{code}</b></p><p>Mã có hiệu lực 5 phút. Không chia sẻ mã này cho người khác.</p>");
                TempData["MfaInfo"] = "Đã gửi mã tới " + Mask(email) + ".";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CmsMfa] Gửi email mã xác thực thất bại cho UserId={UserId}", UserId);
                TempData["MfaError"] = "Không gửi được email mã xác thực. Kiểm tra cấu hình email của hệ thống.";
            }
            // Chỉ môi trường phát triển: ghi mã ra log để kiểm thử khi chưa cấu hình email
            if (_env.IsDevelopment()) _logger.LogInformation("[CmsMfa][DEV] Mã xác thực của UserId={UserId}: {Code}", UserId, code);
            return RedirectToAction(nameof(Index), new { returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Verify(string? code, string? returnUrl)
        {
            if (!_mfa.Verify(UserId, code))
            {
                TempData["MfaError"] = "Mã không đúng hoặc đã hết hạn.";
                return RedirectToAction(nameof(Index), new { returnUrl });
            }
            HttpContext.Session.SetString(CmsMfaService.SessionKey, UserId.ToString());
            return LocalRedirect(SafeReturn(returnUrl));
        }

        private static string Mask(string? email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@')) return "";
            var at = email.IndexOf('@');
            return (at <= 2 ? email[..1] : email[..2]) + new string('*', Math.Max(1, at - 2)) + email[at..];
        }
    }
}
