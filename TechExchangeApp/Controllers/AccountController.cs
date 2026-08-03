using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TechExchangeApp.Entities;
using TechExchangeApp.Helpers;
using TechExchangeApp.ViewModel;
using TechExchangeApp.Interfaces;
using TechExchangeApp.Localization;

namespace TechExchangeApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAccountService _accountService;
        private readonly IVerificationService _verify;
        private readonly string _domainName;

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            IAccountService accountService,
            IVerificationService verify,
            IOptions<AppSettings> appSettings)
        {
            _userManager    = userManager;
            _signInManager  = signInManager;
            _accountService = accountService;
            _verify         = verify;

            // Lấy domain từ AppSettings, fallback về techport.vn
            var raw = appSettings?.Value?.MainDomain ?? string.Empty;
            _domainName = string.IsNullOrWhiteSpace(raw) ? "techport.vn" : new Uri(raw).Host;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /dang-ky.html
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, [FromServices] TechExchangeApp.Data.AppDbContext context)
        {
            if (ModelState.IsValid)
            {
                // Manual check because Normalized columns are ignored
                if (context.Users.Any(u => u.UserName == model.UserName))
                {
                     ModelState.AddModelError("UserName", I18n.T(HttpContext, "auth.err.usernameExists"));
                     return View(model);
                }
                // Check Email if needed. IdentityUser allows duplicate emails by default if configured so,
                // but usually we want unique. Note: NormalizedEmail is ignored so checking Email directly.
                if (context.Users.Any(u => u.Email == model.Email))
                {
                     ModelState.AddModelError("Email", I18n.T(HttpContext, "auth.err.emailExists"));
                     return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.UserName, 
                    Email = model.Email,
                    FullName = model.FullName,
                    Created = DateTime.Now,
                    IsActivated = true,
                    Domain = _domainName,
                    // PhoneNumber maps to Mobile column automatically
                    PhoneNumber = model.PhoneNumber 
                };

                // Manual Password Hashing
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);

                // Add to Context directly to bypass Identity Validators that might rely on Normalized columns
                context.Users.Add(user);
                await context.SaveChangesAsync();
                
                // Sign In
                await _signInManager.SignInAsync(user, isPersistent: false);
                    
                // Set Session for legacy support
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.UserName);

                return RedirectToAction("Index", "Dashboard");
            }
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /dang-nhap.html
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, [FromServices] TechExchangeApp.Data.AppDbContext context, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Manual lookup because Normalized columns are ignored
                var user = context.Users.FirstOrDefault(u => u.UserName == model.UserName);
                
                if (user != null)
                {
                    // Verify password
                    var passwordVerification = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
                    if (passwordVerification == PasswordVerificationResult.Success)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

                        user.LastLogin = DateTime.Now;
                        await _userManager.UpdateAsync(user);

                        // Set Session for legacy support
                        HttpContext.Session.SetInt32("UserId", user.Id);
                        HttpContext.Session.SetString("Username", user.UserName);

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Dashboard");
                    }
                }
                
                ModelState.AddModelError(string.Empty, I18n.T(HttpContext, "auth.err.loginFailed"));
            }
            return View(model);
        }

        // POST: /Account/LoginAjax - AJAX Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAjax(LoginViewModel model, [FromServices] TechExchangeApp.Data.AppDbContext context, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }

            // Manual lookup because Normalized columns are ignored
            var user = context.Users.FirstOrDefault(u => u.UserName == model.UserName);
            
            if (user != null)
            {
                // Verify password
                var passwordVerification = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
                if (passwordVerification == PasswordVerificationResult.Success)
                {
                    await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

                    user.LastLogin = DateTime.Now;
                    await _userManager.UpdateAsync(user);

                    // Set Session for legacy support
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("Username", user.UserName);

                    var redirectUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) 
                        ? returnUrl 
                        : Url.Action("Index", "Dashboard");

                    return Json(new { success = true, redirectUrl });
                }
            }
            
            return Json(new { success = false, errors = new[] { I18n.T(HttpContext, "auth.err.loginFailed") } });
        }

        // POST: /Account/RegisterAjax - AJAX Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAjax(RegisterViewModel model, [FromServices] TechExchangeApp.Data.AppDbContext context)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }

            // Case-insensitive duplicate checks
            var userNameLower = model.UserName.Trim().ToLower();
            var emailLower = model.Email.Trim().ToLower();

            if (context.Users.Any(u => u.UserName.ToLower() == userNameLower))
            {
                return Json(new { success = false, errors = new[] { I18n.T(HttpContext, "auth.err.usernameExists") } });
            }

            if (context.Users.Any(u => u.Email.ToLower() == emailLower))
            {
                return Json(new { success = false, errors = new[] { I18n.T(HttpContext, "auth.err.emailExists") } });
            }

            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName.Trim(),
                    Email = model.Email.Trim(),
                    FullName = model.FullName,
                    Created = DateTime.Now,
                    IsActivated = true,
                    Domain = _domainName,
                    PhoneNumber = model.PhoneNumber
                };

                // Manual Password Hashing
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);

                // Add to Context directly
                context.Users.Add(user);
                await context.SaveChangesAsync();

                // Sign In
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Set Session for legacy support
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.UserName);

                var redirectUrl = Url.Action("Index", "Dashboard");
                return Json(new { success = true, redirectUrl });
            }
            catch (Exception)
            {
                return Json(new { success = false, errors = new[] { I18n.T(HttpContext, "auth.err.registerFailed") } });
            }
        }


        // GET: /Account/Profile
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var model = await _accountService.GetProfileAsync(userId);
            
            if (model == null)
                return NotFound();
            
            return View(model);
        }

        // POST: /Account/Profile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileVm model, IFormFile? avatar)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = int.Parse(_userManager.GetUserId(User)!);

            try
            {
                // Upload avatar if provided
                if (avatar != null)
                {
                    var avatarPath = await _accountService.UploadAvatarAsync(userId, avatar);
                    model.AvatarUrl = avatarPath;
                }

                // Update profile
                var success = await _accountService.UpdateProfileAsync(userId, model);
                
                if (success)
                {
                    TempData["SuccessMessage"] = I18n.T(HttpContext, "profile.success");
                    return RedirectToAction(nameof(Profile));
                }

                ModelState.AddModelError("", I18n.T(HttpContext, "profile.err.updateFailed"));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", I18n.T(HttpContext, ex.Message));
            }

            return View(model);
        }

        // GET: /Account/ChangePassword
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Account/ChangePassword
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = int.Parse(_userManager.GetUserId(User)!);

            try
            {
                var success = await _accountService.ChangePasswordAsync(userId, model);
                
                if (success)
                {
                    TempData["SuccessMessage"] = I18n.T(HttpContext, "changepwd.success");
                    return RedirectToAction(nameof(Profile));
                }

                ModelState.AddModelError("", I18n.T(HttpContext, "changepwd.err.updateFailed"));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", I18n.T(HttpContext, ex.Message));
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear(); // Clear legacy session
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ─── POST /Account/UpdatePhone (AJAX) ──────────────────────────────────
        [Authorize, HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdatePhone([FromBody] UpdatePhoneDto dto)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            if (string.IsNullOrWhiteSpace(dto.Phone))
                return Json(new { success = false, message = I18n.T(HttpContext, "verify.phoneInvalid") });
            var ok = await _verify.UpdatePhoneAsync(userId, dto.Phone.Trim());
            return Json(new { success = ok, message = I18n.T(HttpContext, ok ? "verify.phoneUpdated" : "verify.updateFailed") });
        }

        // ─── POST /Account/SendEmailOtp (AJAX) ─────────────────────────────────
        [Authorize, HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> SendEmailOtp()
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var ok = await _verify.SendEmailOtpAsync(userId);
            return Json(new { success = ok, message = I18n.T(HttpContext, ok ? "verify.emailOtpSent" : "verify.emailOtpSendFailed") });
        }

        // ─── POST /Account/SendPhoneOtp (AJAX) ─────────────────────────────────
        [Authorize, HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> SendPhoneOtp()
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var ok = await _verify.SendPhoneOtpAsync(userId);
            return Json(new { success = ok, message = I18n.T(HttpContext, ok ? "verify.phoneOtpSent" : "verify.phoneOtpSendFailed") });
        }

        // ─── POST /Account/VerifyEmailOtp (AJAX) ───────────────────────────────
        [Authorize, HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> VerifyEmailOtp([FromBody] OtpDto dto)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var (ok, msgKey) = await _verify.VerifyEmailOtpAsync(userId, dto.Otp);
            return Json(new { success = ok, message = I18n.T(HttpContext, msgKey) });
        }

        // ─── POST /Account/VerifyPhoneOtp (AJAX) ───────────────────────────────
        [Authorize, HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> VerifyPhoneOtp([FromBody] OtpDto dto)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var (ok, msgKey) = await _verify.VerifyPhoneOtpAsync(userId, dto.Otp);
            return Json(new { success = ok, message = I18n.T(HttpContext, msgKey) });
        }

        // ─── POST /Account/UploadDoc (form) ─────────────────────────────────────
        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDoc(int docType, IFormFile? docFile,
            [FromServices] IWebHostEnvironment env)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            if (docFile == null)
            {
                TempData["ErrorMessage"] = I18n.T(HttpContext, "verify.selectFileRequired");
                return RedirectToAction(nameof(Profile));
            }
            var (ok, msgKey) = await _verify.UploadDocAsync(userId, docType, docFile, env);
            var message = BuildDocUploadMessage(ok, msgKey, docType);
            if (ok) TempData["SuccessMessage"] = message;
            else    TempData["ErrorMessage"]   = message;
            return RedirectToAction(nameof(Profile));
        }

        // ─── POST /Account/UploadDocAjax (AJAX – used from Step 6) ──────────────
        [Authorize, HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> UploadDocAjax(int docType, IFormFile? docFile,
            [FromServices] IWebHostEnvironment env)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            if (docFile == null)
                return Json(new { success = false, message = I18n.T(HttpContext, "verify.selectFileRequired") });
            var (ok, msgKey) = await _verify.UploadDocAsync(userId, docType, docFile, env);
            return Json(new { success = ok, message = BuildDocUploadMessage(ok, msgKey, docType) });
        }

        // Upload thành công trả về key rỗng; controller tự ghép "<nhãn loại giấy tờ> <hậu tố i18n>".
        private string BuildDocUploadMessage(bool ok, string msgKey, int docType)
        {
            if (!ok) return I18n.T(HttpContext, msgKey);
            var labelKey = docType == DocType.CccdFront ? "doc.cccdFront"
                         : docType == DocType.CccdBack  ? "doc.cccdBack"
                         : "doc.businessLicense";
            return $"✅ {I18n.T(HttpContext, labelKey)} {I18n.T(HttpContext, "doc.uploadedSuffix")}";
        }
    }
}

// ─── DTOs (file-level, outside namespace scope) ──────────────────────────────
public record UpdatePhoneDto(string Phone);
public record OtpDto(string Otp);
