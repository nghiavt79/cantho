using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Interfaces;

namespace TechExchangeApp.Services
{
    public interface IPasswordResetService
    {
        /// <summary>
        /// Tạo token và gửi link đặt lại mật khẩu qua email. Không cho biết tài khoản có tồn tại
        /// hay không (tránh dò tài khoản) — controller luôn hiện cùng một thông báo.
        /// </summary>
        Task RequestAsync(string emailOrUserName, string resetUrlBase, bool isEnglish, string? requestIp);

        /// <summary>Trả về UserId nếu token còn hiệu lực, ngược lại null.</summary>
        Task<int?> ValidateAsync(string? token);

        /// <summary>Đặt mật khẩu mới, đánh dấu token đã dùng và vô hiệu các token khác của người dùng.</summary>
        Task<bool> ResetAsync(string token, string newPassword);
    }

    /// <summary>Quên mật khẩu (HSMT A.1.1). Token lưu SHA-256, hết hạn 30 phút, dùng 1 lần.</summary>
    public class PasswordResetService : IPasswordResetService
    {
        public static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(30);
        private const int MaxRequestsPer15Min = 3;

        private readonly AppDbContext _context;
        private readonly IAccountService _accountService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(AppDbContext context, IAccountService accountService,
            IEmailSender emailSender, ILogger<PasswordResetService> logger)
        {
            _context = context;
            _accountService = accountService;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task RequestAsync(string emailOrUserName, string resetUrlBase, bool isEnglish, string? requestIp)
        {
            var key = (emailOrUserName ?? "").Trim().ToLower();
            if (key.Length == 0) return;

            var user = await _context.Users
                .Where(u => (u.Email != null && u.Email.ToLower() == key) || u.UserName.ToLower() == key)
                .OrderByDescending(u => u.LastLogin)
                .FirstOrDefaultAsync();

            if (user == null || user.IsLocked || string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogInformation("[PasswordReset] Bỏ qua yêu cầu cho '{Key}' (không có tài khoản/email hoặc đang khóa)", key);
                return;
            }

            var since = DateTime.Now.AddMinutes(-15);
            var recent = await _context.PasswordResetTokens.CountAsync(t => t.UserId == user.Id && t.Created >= since);
            if (recent >= MaxRequestsPer15Min)
            {
                _logger.LogWarning("[PasswordReset] Quá số lần yêu cầu cho UserId={UserId}", user.Id);
                return;
            }

            var token = ToBase64Url(RandomNumberGenerator.GetBytes(32));
            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = Hash(token),
                ExpiresAt = DateTime.Now.Add(TokenLifetime),
                Created = DateTime.Now,
                RequestIp = requestIp
            });
            await _context.SaveChangesAsync();

            var link = resetUrlBase + "?token=" + Uri.EscapeDataString(token);
            var name = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(user.FullName) ? user.UserName : user.FullName);
            var safeLink = WebUtility.HtmlEncode(link);
            var minutes = (int)TokenLifetime.TotalMinutes;

            var subject = isEnglish ? "Reset your password" : "Đặt lại mật khẩu";
            var body = isEnglish
                ? $"<p>Hello {name},</p><p>We received a request to reset the password for account <b>{WebUtility.HtmlEncode(user.UserName)}</b>.</p>" +
                  $"<p><a href=\"{safeLink}\">Click here to set a new password</a> (valid for {minutes} minutes, single use).</p>" +
                  "<p>If you did not request this, you can ignore this email; your password will not change.</p>"
                : $"<p>Xin chào {name},</p><p>Hệ thống nhận được yêu cầu đặt lại mật khẩu cho tài khoản <b>{WebUtility.HtmlEncode(user.UserName)}</b>.</p>" +
                  $"<p><a href=\"{safeLink}\">Bấm vào đây để đặt mật khẩu mới</a> (hiệu lực {minutes} phút, dùng một lần).</p>" +
                  "<p>Nếu không phải anh/chị yêu cầu, vui lòng bỏ qua email này; mật khẩu sẽ không thay đổi.</p>";

            try
            {
                await _emailSender.SendAsync(user.Email!, subject, body);
                _logger.LogInformation("[PasswordReset] Đã gửi link đặt lại mật khẩu cho UserId={UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PasswordReset] Gửi email thất bại cho UserId={UserId}", user.Id);
            }
        }

        public async Task<int?> ValidateAsync(string? token)
        {
            var row = await FindValidAsync(token);
            return row?.UserId;
        }

        public async Task<bool> ResetAsync(string token, string newPassword)
        {
            var row = await FindValidAsync(token);
            if (row == null) return false;

            var ok = await _accountService.SetPasswordAsync(row.UserId, newPassword);
            if (!ok) return false;

            var now = DateTime.Now;
            var pending = await _context.PasswordResetTokens
                .Where(t => t.UserId == row.UserId && t.UsedAt == null)
                .ToListAsync();
            foreach (var t in pending) t.UsedAt = now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("[PasswordReset] UserId={UserId} đã đặt lại mật khẩu", row.UserId);
            return true;
        }

        private async Task<PasswordResetToken?> FindValidAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token) || token.Length > 100) return null;
            var hash = Hash(token);
            var now = DateTime.Now;
            var row = await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
            if (row == null || row.UsedAt != null || row.ExpiresAt < now) return null;

            var locked = await _context.Users.Where(u => u.Id == row.UserId).Select(u => u.IsLocked).FirstOrDefaultAsync();
            return locked ? null : row;
        }

        private static string Hash(string token)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

        private static string ToBase64Url(byte[] bytes)
            => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
