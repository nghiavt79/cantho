using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace TechExchangeApp.Services
{
    /// <summary>
    /// Mã xác thực lớp 2 (OTP 6 số qua email) cho tài khoản quản trị khi vào CMS (HSMT 2.11).
    /// Bật bằng tham số CMS_MFA_ENABLED = 1. Mã sống 5 phút, sai quá 5 lần phải gửi mã mới.
    /// Đã xác thực thì lưu cờ trong session (hết session phải xác thực lại).
    /// </summary>
    public class CmsMfaService
    {
        public const string EnabledParam = "CMS_MFA_ENABLED";
        public const string SessionKey = "cms_mfa_uid";
        private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);
        private const int MaxAttempts = 5;

        private record Pending(string Hash, DateTime ExpiresAt, int Attempts, DateTime SentAt);

        private readonly IMemoryCache _cache;
        public CmsMfaService(IMemoryCache cache) => _cache = cache;

        private static string Key(int userId) => "cms-mfa:" + userId;
        private static string Hash(int userId, string code) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userId + ":" + code)));

        /// <summary>Tạo mã mới; trả null nếu vừa gửi chưa quá 60 giây.</summary>
        public string? CreateCode(int userId)
        {
            if (_cache.TryGetValue(Key(userId), out Pending? p) && p != null && DateTime.UtcNow - p.SentAt < ResendCooldown)
                return null;
            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            _cache.Set(Key(userId), new Pending(Hash(userId, code), DateTime.UtcNow.Add(CodeLifetime), 0, DateTime.UtcNow), CodeLifetime);
            return code;
        }

        public bool Verify(int userId, string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || !_cache.TryGetValue(Key(userId), out Pending? p) || p == null) return false;
            if (p.ExpiresAt < DateTime.UtcNow || p.Attempts >= MaxAttempts) { _cache.Remove(Key(userId)); return false; }
            if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(p.Hash), Encoding.UTF8.GetBytes(Hash(userId, code.Trim()))))
            {
                _cache.Set(Key(userId), p with { Attempts = p.Attempts + 1 }, p.ExpiresAt - DateTime.UtcNow);
                return false;
            }
            _cache.Remove(Key(userId));
            return true;
        }
    }
}
