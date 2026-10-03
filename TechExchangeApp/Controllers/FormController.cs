using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Enums;
using TechExchangeApp.Helpers;
using TechExchangeApp.Services;

namespace TechExchangeApp.Controllers
{
    /// <summary>
    /// Trang biểu mẫu động công khai (HSMT A.7.2, B.1.6): /bieu-mau/{slug}, /en/form/{slug}.
    /// Sau khi gửi: báo email cho người nhận cấu hình, gửi email xác nhận cho người gửi (nếu bật),
    /// rồi chuyển trang hoặc hiện thông báo.
    /// </summary>
    public class FormController : Controller
    {
        private const int MaxSubmissionsPerIpPer10Min = 10;
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;

        public FormController(AppDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string slug)
        {
            var form = await _db.DynamicForms.AsNoTracking().FirstOrDefaultAsync(f => f.Slug == slug && f.IsActive);
            if (form == null) return NotFound();
            ViewBag.Fields = DynamicFormService.ParseFields(form.FieldsJson);
            return View(form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string slug, string? website)
        {
            var form = await _db.DynamicForms.AsNoTracking().FirstOrDefaultAsync(f => f.Slug == slug && f.IsActive);
            if (form == null) return NotFound();
            var fields = DynamicFormService.ParseFields(form.FieldsJson);
            var isEn = LangHelper.IsEnglish(HttpContext);
            ViewBag.Fields = fields;

            if (!string.IsNullOrEmpty(website)) { ViewBag.Done = true; return View(form); } // bẫy bot

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var key = "form-submit-ip:" + ip;
            var n = _cache.GetOrCreate(key, e => { e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10); return 0; });
            if (n >= MaxSubmissionsPerIpPer10Min)
            {
                ViewBag.Error = isEn ? "Too many submissions, please try again later." : "Gửi quá nhiều lần, vui lòng thử lại sau.";
                return View(form);
            }

            var (data, errors) = DynamicFormService.Validate(fields, Request.Form, isEn);
            if (errors.Count > 0)
            {
                ViewBag.Errors = errors;
                ViewBag.Values = data;
                return View(form);
            }
            _cache.Set(key, n + 1, TimeSpan.FromMinutes(10));

            var emailField = fields.FirstOrDefault(f => f.Type == "email");
            var submitterEmail = emailField != null && data.TryGetValue(emailField.Key, out var em) && !string.IsNullOrEmpty(em) ? em : null;
            int? userId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : null;

            _db.DynamicFormSubmissions.Add(new DynamicFormSubmission
            {
                FormId = form.Id, DataJson = DynamicFormService.SerializeData(data), Email = submitterEmail,
                UserId = userId, IpAddress = ip, Created = DateTime.Now
            });

            var table = DynamicFormService.ToHtmlTable(fields, data);
            foreach (var to in (form.NotifyEmails ?? "").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(10))
                _db.Notifications.Add(new Notification
                {
                    Target = to, Title = "Biểu mẫu mới: " + form.Title, Content = $"<p>Có lượt gửi mới cho biểu mẫu <b>{System.Net.WebUtility.HtmlEncode(form.Title)}</b>.</p>{table}",
                    Channel = (int)NotificationChannel.Email, Status = (int)NotificationStatus.Pending, CreatedDate = DateTime.UtcNow
                });
            if (form.SendConfirmation && submitterEmail != null)
                _db.Notifications.Add(new Notification
                {
                    UserId = userId?.ToString(), Target = submitterEmail,
                    Title = (isEn ? "We received your submission: " : "Đã nhận thông tin: ") + form.Title,
                    Content = $"<p>{System.Net.WebUtility.HtmlEncode(form.SuccessMessage ?? (isEn ? "Thank you." : "Cảm ơn anh/chị."))}</p>{table}",
                    Channel = (int)NotificationChannel.Email, Status = (int)NotificationStatus.Pending, CreatedDate = DateTime.UtcNow
                });
            await _db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(form.RedirectUrl) && Url.IsLocalUrl(form.RedirectUrl))
                return LocalRedirect(form.RedirectUrl);
            ViewBag.Done = true;
            return View(form);
        }
    }
}
