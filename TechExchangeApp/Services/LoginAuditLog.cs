using TechExchangeApp.Data;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Services
{
    /// <summary>
    /// Ghi nhật ký đăng nhập, đăng xuất vào bảng Logs (lưu vết truy cập – tiêu chí an ninh mạng PL VII TT41).
    /// EventID dùng chung danh mục trang CMS "Nhật ký": 4 = Đăng nhập, 5 = Đăng xuất, 7 = Đăng nhập không thành công.
    /// </summary>
    public static class LoginAuditLog
    {
        public const int EventLogin = 4;
        public const int EventLogout = 5;
        public const int EventLoginFailed = 7;
        // Thao tác ở trang người dùng dùng chung danh mục sự kiện với CMS
        public const int EventAdd = 1;
        public const int EventUpdate = 2;
        public const int EventDelete = 3;

        /// <summary>Dựng bản ghi nhật ký, chưa lưu (dùng khi cần lưu cùng thao tác chính).</summary>
        public static Log Build(HttpContext httpContext, string? userName, int eventId, string content)
        {
            static string? Trunc(string? s, int max) =>
                string.IsNullOrEmpty(s) || s.Length <= max ? s : s[..max];

            var config = httpContext.RequestServices.GetService<IConfiguration>();
            var siteId = int.TryParse(config?["AppSettings:SiteId"], out var id) ? id : 1;

            return new Log
            {
                FunctionID = 0,
                ActTime = DateTime.Now,
                EventID = eventId,
                Content = Trunc(content, 2000),
                ClientIP = Trunc(httpContext.Connection.RemoteIpAddress?.ToString(), 50),
                UserName = Trunc(userName?.Trim(), 50),
                Domain = Trunc(httpContext.Request.Host.Value, 500) ?? "",
                LanguageId = 1,
                ParentId = 0,
                SiteId = siteId
            };
        }

        public static async Task WriteAsync(AppDbContext context, HttpContext httpContext, string? userName, int eventId, string content)
        {
            try
            {
                context.Logs.Add(Build(httpContext, userName, eventId, content));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Không để lỗi ghi nhật ký làm hỏng việc đăng nhập
                httpContext.RequestServices.GetService<ILogger<AppDbContext>>()
                    ?.LogWarning(ex, "Không ghi được nhật ký đăng nhập ({EventId}) cho {UserName}", eventId, userName);
            }
        }
    }
}
