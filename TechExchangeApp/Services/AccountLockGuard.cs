using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TechExchangeApp.Data;

namespace TechExchangeApp.Services
{
    /// <summary>
    /// Đẩy người dùng bị khóa (Users.IsLocked, HSMT A.5.1) ra khỏi phiên đang đăng nhập.
    /// Gắn vào CookieAuthenticationEvents.OnValidatePrincipal; kết quả cache 60 giây,
    /// CMS gọi Invalidate khi khóa/mở để có hiệu lực ngay.
    /// </summary>
    public static class AccountLockGuard
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);
        private static string CacheKey(int userId) => $"user-locked:{userId}";

        public static async Task ValidatePrincipalAsync(CookieValidatePrincipalContext ctx)
        {
            var idValue = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idValue, out var userId)) return;

            var services = ctx.HttpContext.RequestServices;
            var cache = services.GetRequiredService<IMemoryCache>();
            var locked = await cache.GetOrCreateAsync(CacheKey(userId), async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                var db = services.GetRequiredService<AppDbContext>();
                return await db.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => u.IsLocked).FirstOrDefaultAsync();
            });

            if (locked)
            {
                ctx.RejectPrincipal();
                await ctx.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                ctx.HttpContext.Session?.Clear();
            }
        }

        public static void Invalidate(IMemoryCache cache, int userId) => cache.Remove(CacheKey(userId));
    }
}
