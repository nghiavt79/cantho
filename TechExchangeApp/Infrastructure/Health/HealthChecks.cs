using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TechExchangeApp.Data;
using TechExchangeApp.Enums;

namespace TechExchangeApp.Infrastructure.Health
{
    // Health check cho /health (HSMT 2.2 — giám sát, cảnh báo sự cố).
    // Công cụ giám sát uptime gọi /health: HTTP 200 = Healthy/Degraded, 503 = Unhealthy.

    /// <summary>Kết nối được SQL Server và chạy được truy vấn.</summary>
    public class SqlServerHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _db;
        public SqlServerHealthCheck(AppDbContext db) => _db = db;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
        {
            try
            {
                var ok = await _db.Database.CanConnectAsync(ct);
                return ok ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Không kết nối được CSDL");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Lỗi kết nối CSDL", ex);
            }
        }
    }

    /// <summary>Ổ đĩa chứa ứng dụng (và thư mục upload) còn trống ≥ 5 GB; dưới 1 GB là lỗi.</summary>
    public class DiskSpaceHealthCheck : IHealthCheck
    {
        private const long WarnBytes = 5L * 1024 * 1024 * 1024;
        private const long FailBytes = 1L * 1024 * 1024 * 1024;
        private readonly IWebHostEnvironment _env;
        public DiskSpaceHealthCheck(IWebHostEnvironment env) => _env = env;

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
        {
            var root = Path.GetPathRoot(_env.ContentRootPath);
            if (string.IsNullOrEmpty(root)) return Task.FromResult(HealthCheckResult.Healthy());

            var free = new DriveInfo(root).AvailableFreeSpace;
            var data = new Dictionary<string, object> { ["freeGB"] = Math.Round(free / 1024d / 1024 / 1024, 1) };
            if (free < FailBytes) return Task.FromResult(HealthCheckResult.Unhealthy("Ổ đĩa gần đầy", data: data));
            if (free < WarnBytes) return Task.FromResult(HealthCheckResult.Degraded("Ổ đĩa còn ít dung lượng", data: data));
            return Task.FromResult(HealthCheckResult.Healthy(data: data));
        }
    }

    /// <summary>Hàng đợi thông báo không bị ùn: có thông báo chờ quá 30 phút là dấu hiệu worker ngừng chạy.</summary>
    public class NotificationQueueHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _db;
        public NotificationQueueHealthCheck(AppDbContext db) => _db = db;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
        {
            var threshold = DateTime.UtcNow.AddMinutes(-30);
            var stuck = await _db.Notifications.AsNoTracking()
                .CountAsync(n => n.Status == (int)NotificationStatus.Pending && n.CreatedDate < threshold, ct);
            var data = new Dictionary<string, object> { ["pendingOver30Min"] = stuck };
            return stuck == 0
                ? HealthCheckResult.Healthy(data: data)
                : HealthCheckResult.Degraded("Có thông báo chờ gửi quá 30 phút", data: data);
        }
    }

    /// <summary>Trả JSON gọn: trạng thái tổng và từng hạng mục, không lộ chi tiết exception.</summary>
    public static class HealthResponseWriter
    {
        public static Task WriteAsync(HttpContext ctx, HealthReport report)
        {
            ctx.Response.ContentType = "application/json; charset=utf-8";
            ctx.Response.Headers.CacheControl = "no-store";
            var payload = new
            {
                status = report.Status.ToString(),
                durationMs = Math.Round(report.TotalDuration.TotalMilliseconds),
                checks = report.Entries.ToDictionary(e => e.Key, e => new
                {
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    data = e.Value.Data.Count > 0 ? e.Value.Data : null
                })
            };
            return ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
