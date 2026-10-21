using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Infrastructure
{
    /// <summary>
    /// Ghi nhật ký "ai tải tệp" (HSMT A.8.5) cho tệp hồ sơ giao dịch, hợp đồng, xác minh tài khoản.
    /// Ghi vào bảng Log (sự kiện 6 = Tải tệp, tra cứu tại CMS > Nhật ký) và ProjectAccessLogs khi xác định được hồ sơ.
    /// </summary>
    public class DownloadAuditMiddleware
    {
        public const int EventDownload = 6;

        private static readonly string[] AuditedFolders =
        {
            "proposals", "negotiations", "training", "liquidations", "implementation-logs", "contracts",
            "advance-payments", "verify", "techdoc", "pilot-tests", "esign", "nda", "legal-reviews", "handovers", "acceptance"
        };
        private static readonly Regex ProjectIdInPath = new(@"proj_(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly RequestDelegate _next;
        private readonly ILogger<DownloadAuditMiddleware> _logger;

        public DownloadAuditMiddleware(RequestDelegate next, ILogger<DownloadAuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            var path = ctx.Request.Path.Value ?? "";
            if (!HttpMethods.IsGet(ctx.Request.Method) || !IsAudited(path))
            {
                await _next(ctx);
                return;
            }

            // Tệp tĩnh được phục vụ trước UseAuthentication → tự xác thực cookie để biết người tải
            // (phải làm trước khi phản hồi bắt đầu gửi)
            ClaimsPrincipal? user = null;
            try
            {
                var auth = await ctx.AuthenticateAsync();
                user = auth.Succeeded ? auth.Principal : null;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Không xác thực được người tải tệp {Path}", path);
            }

            await _next(ctx);

            if (ctx.Response.StatusCode != StatusCodes.Status200OK) return;
            try
            {
                var userName = user?.Identity?.Name ?? "(khách)";
                int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
                var ip = ctx.Connection.RemoteIpAddress?.ToString();

                var db = ctx.RequestServices.GetRequiredService<AppDbContext>();
                db.Logs.Add(new Log
                {
                    FunctionID = 0,
                    ActTime = DateTime.Now,
                    EventID = EventDownload,
                    Content = Trunc("Tải tệp: " + path, 1000),
                    ClientIP = ip,
                    UserName = userName,
                    Domain = ctx.Request.Host.Value
                });

                var m = ProjectIdInPath.Match(path);
                if (userId > 0 && m.Success && int.TryParse(m.Groups[1].Value, out var projectId) && db.Projects.Any(p => p.Id == projectId))
                {
                    db.ProjectAccessLogs.Add(new ProjectAccessLog
                    {
                        ProjectId = projectId,
                        UserId = userId,
                        Action = "DownloadFile",
                        Timestamp = DateTime.Now,
                        IpAddress = ip,
                        UserAgent = Trunc(ctx.Request.Headers.UserAgent.ToString(), 500),
                        AdditionalData = Trunc(path, 1000)
                    });
                }
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Không làm hỏng việc tải tệp nếu ghi nhật ký lỗi
                _logger.LogWarning(ex, "Không ghi được nhật ký tải tệp {Path}", path);
            }
        }

        private static bool IsAudited(string path)
        {
            foreach (var root in new[] { "/uploads/", "/upload/" })
            {
                if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) continue;
                var rest = path[root.Length..];
                var folder = rest.Split('/', 2)[0];
                return AuditedFolders.Contains(folder, StringComparer.OrdinalIgnoreCase);
            }
            return false;
        }

        private static string? Trunc(string? s, int max) => string.IsNullOrEmpty(s) || s.Length <= max ? s : s[..max];
    }
}
