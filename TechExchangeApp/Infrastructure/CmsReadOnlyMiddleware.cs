using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Interfaces;

namespace TechExchangeApp.Infrastructure
{
    /// <summary>
    /// Tài khoản CMS chỉ xem (tham số CMS_READONLY_USERS, vd tài khoản đơn vị giám sát gs_theodoi — NCC-06):
    /// chỉ được mở (GET) các trang theo dõi; mọi thao tác thêm, sửa, xóa hoặc trang khác bị từ chối (403)
    /// và ghi nhật ký (sự kiện 8 = Từ chối thao tác, tra cứu tại CMS > Nhật ký).
    /// </summary>
    public class CmsReadOnlyMiddleware
    {
        public const string UsersParam = "CMS_READONLY_USERS";
        public const int EventDenied = 8;

        // Trang được xem: Tổng quan, Nhật ký, Thống kê, Yêu cầu hỗ trợ, Hướng dẫn
        private static readonly HashSet<string> ViewableControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "", "Dashboard", "DashboardData", "LogAdmin", "AdminAnalytics", "SupportRequestsAdmin", "HuongDanCms"
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<CmsReadOnlyMiddleware> _logger;

        public CmsReadOnlyMiddleware(RequestDelegate next, ILogger<CmsReadOnlyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            var userName = ctx.User.Identity?.IsAuthenticated == true ? ctx.User.Identity.Name : null;
            if (string.IsNullOrEmpty(userName)
                || !await IsReadOnlyAsync(ctx.RequestServices.GetRequiredService<ISystemParameterService>(), userName))
            {
                await _next(ctx);
                return;
            }

            var method = ctx.Request.Method;
            var isRead = HttpMethods.IsGet(method) || HttpMethods.IsHead(method);
            var controller = (ctx.Request.Path.Value ?? "").Trim('/').Split('/') switch
            {
                var s when s.Length > 1 => s[1],
                _ => ""
            };
            if (isRead && ViewableControllers.Contains(controller))
            {
                await _next(ctx);
                return;
            }

            try
            {
                var db = ctx.RequestServices.GetRequiredService<AppDbContext>();
                db.Logs.Add(new Log
                {
                    FunctionID = 0,
                    ActTime = DateTime.Now,
                    EventID = EventDenied,
                    Content = $"Từ chối thao tác của tài khoản chỉ xem: {method} {ctx.Request.Path}",
                    ClientIP = ctx.Connection.RemoteIpAddress?.ToString(),
                    UserName = userName,
                    Domain = ctx.Request.Host.Value ?? ""
                });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không ghi được nhật ký từ chối thao tác {Method} {Path}", method, ctx.Request.Path);
            }

            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            var isAjax = ctx.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (isAjax || !isRead)
            {
                ctx.Response.ContentType = "application/json; charset=utf-8";
                await ctx.Response.WriteAsync("{\"success\":false,\"message\":\"Tài khoản chỉ có quyền xem.\"}");
                return;
            }
            ctx.Response.ContentType = "text/html; charset=utf-8";
            await ctx.Response.WriteAsync(
                "<!doctype html><meta charset=\"utf-8\"><title>Không có quyền</title>"
                + "<div style=\"font-family:sans-serif;max-width:560px;margin:60px auto\">"
                + "<h3>Tài khoản chỉ có quyền xem</h3>"
                + "<p>Tài khoản được xem Tổng quan, Nhật ký, Thống kê và Yêu cầu hỗ trợ; không thêm, sửa, xóa dữ liệu. Thao tác này đã được ghi nhật ký.</p>"
                + "<p><a href=\"/cms\">Về trang Tổng quan</a></p></div>");
        }

        /// <summary>Tên đăng nhập có trong danh sách CMS_READONLY_USERS (phân tách bằng dấu phẩy, chấm phẩy).</summary>
        public static async Task<bool> IsReadOnlyAsync(ISystemParameterService sysParams, string? userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) return false;
            var list = await sysParams.GetAsync(UsersParam);
            if (string.IsNullOrWhiteSpace(list)) return false;
            return list.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains(userName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
