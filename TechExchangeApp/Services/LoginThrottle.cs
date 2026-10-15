using Microsoft.Extensions.Caching.Memory;

namespace TechExchangeApp.Services
{
    /// <summary>
    /// Tạm khóa đăng nhập sau nhiều lần nhập sai (pentest MobiFone 10/2026 — Account Lockout).
    /// - Theo tên đăng nhập: sai 5 lần trong 5 phút → khóa 15 phút (kể cả nhập đúng trong lúc khóa).
    /// - Theo địa chỉ IP: sai 20 lần trong 5 phút → chặn IP 15 phút (dò nhiều tài khoản từ một máy).
    /// Đếm cả tên đăng nhập không tồn tại để không lộ tài khoản nào có thật.
    /// Lưu trong MemoryCache (một máy chủ); khởi động lại ứng dụng thì bộ đếm về 0.
    /// </summary>
    public class LoginThrottle
    {
        public const int MaxUserFailures = 5;
        public const int MaxIpFailures = 20;
        public static readonly TimeSpan Window = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

        private readonly IMemoryCache _cache;
        private readonly Func<DateTime> _now;

        public LoginThrottle(IMemoryCache cache) : this(cache, () => DateTime.UtcNow) { }

        public LoginThrottle(IMemoryCache cache, Func<DateTime> now)
        {
            _cache = cache;
            _now = now;
        }

        private sealed class Counter
        {
            public int Failures;
            public DateTime WindowStart;
            public DateTime? LockedUntil;
        }

        private static string UserKey(string? userName) => "login-fail:u:" + (userName ?? "").Trim().ToLowerInvariant();
        private static string IpKey(string? ip) => "login-fail:ip:" + (ip ?? "unknown");

        /// <summary>Thời gian còn bị khóa (null nếu được phép thử đăng nhập).</summary>
        public TimeSpan? GetLockRemaining(string? userName, string? ip)
        {
            var now = _now();
            TimeSpan? remaining = null;
            foreach (var key in new[] { UserKey(userName), IpKey(ip) })
            {
                if (_cache.TryGetValue(key, out Counter? c) && c!.LockedUntil is DateTime until && until > now)
                {
                    var left = until - now;
                    if (remaining == null || left > remaining) remaining = left;
                }
            }
            return remaining;
        }

        public void RegisterFailure(string? userName, string? ip)
        {
            Bump(UserKey(userName), MaxUserFailures);
            Bump(IpKey(ip), MaxIpFailures);
        }

        /// <summary>Đăng nhập đúng → xóa bộ đếm của tài khoản (bộ đếm IP giữ nguyên).</summary>
        public void RegisterSuccess(string? userName) => _cache.Remove(UserKey(userName));

        private void Bump(string key, int max)
        {
            var now = _now();
            var c = _cache.GetOrCreate(key, e =>
            {
                e.SlidingExpiration = LockDuration + Window;
                return new Counter { WindowStart = now };
            })!;

            lock (c)
            {
                if (c.LockedUntil is DateTime until && until <= now)
                {
                    c.LockedUntil = null; c.Failures = 0; c.WindowStart = now;
                }
                if (now - c.WindowStart > Window)
                {
                    c.Failures = 0; c.WindowStart = now;
                }
                c.Failures++;
                if (c.Failures >= max && c.LockedUntil == null)
                    c.LockedUntil = now + LockDuration;
            }
        }

        /// <summary>Số phút làm tròn lên để hiển thị cho người dùng.</summary>
        public static int Minutes(TimeSpan remaining) => Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));
    }
}
