using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Services
{
    /// <summary>
    /// Đẩy người dùng bị khóa (Users.IsLocked, HSMT A.5.1) ra khỏi phiên đang đăng nhập,
    /// và từ chối cookie có số phiên (claim "sv") khác Users.SessionVersion — tức cookie
    /// đã đăng xuất / đổi mật khẩu (pentest 10/2026, Improper Logout).
    /// Gắn vào CookieAuthenticationEvents.OnValidatePrincipal; kết quả cache 60 giây,
    /// gọi Invalidate khi khóa/mở/đăng xuất/đổi mật khẩu để có hiệu lực ngay.
    /// </summary>
    public static class AccountLockGuard
    {
        public const string SessionVersionClaim = "sv";

        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);
        private static string CacheKey(int userId) => $"user-session-state:{userId}";

        private sealed record SessionState(bool IsLocked, int SessionVersion);

        public static async Task ValidatePrincipalAsync(CookieValidatePrincipalContext ctx)
        {
            var idValue = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idValue, out var userId)) return;

            var services = ctx.HttpContext.RequestServices;
            var cache = services.GetRequiredService<IMemoryCache>();
            var state = await cache.GetOrCreateAsync(CacheKey(userId), async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                var db = services.GetRequiredService<AppDbContext>();
                return await db.Users.AsNoTracking().Where(u => u.Id == userId)
                    .Select(u => new SessionState(u.IsLocked, u.SessionVersion)).FirstOrDefaultAsync();
            });

            // Cookie cấp trước khi có claim "sv" cũng bị từ chối → bắt đăng nhập lại một lần
            var cookieVersion = ctx.Principal?.FindFirstValue(SessionVersionClaim);
            var versionOk = state != null && cookieVersion == state.SessionVersion.ToString();

            if (state == null || state.IsLocked || !versionOk)
            {
                ctx.RejectPrincipal();
                await ctx.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                ctx.HttpContext.Session?.Clear();
            }
        }

        public static void Invalidate(IMemoryCache cache, int userId) => cache.Remove(CacheKey(userId));

        /// <summary>Tăng số phiên của user → mọi cookie đang có của user đó hết hiệu lực.</summary>
        public static async Task RevokeSessionsAsync(AppDbContext db, IMemoryCache cache, int userId)
        {
            await db.Users.Where(u => u.Id == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.SessionVersion, u => u.SessionVersion + 1));
            Invalidate(cache, userId);
        }
    }

    /// <summary>Ghi số phiên hiện tại (Users.SessionVersion) vào cookie đăng nhập.</summary>
    public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        public AppUserClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> options)
            : base(userManager, options) { }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(AccountLockGuard.SessionVersionClaim, user.SessionVersion.ToString()));
            return identity;
        }
    }
}
