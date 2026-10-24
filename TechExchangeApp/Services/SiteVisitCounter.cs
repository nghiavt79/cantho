using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;

namespace TechExchangeApp.Services
{
    /// <summary>
    /// Đếm lượt truy cập toàn Sàn trong bộ nhớ, ghi dồn vào bảng SiteVisitDaily mỗi phút (không ghi DB theo từng request).
    /// Người truy cập phân biệt theo IP + trình duyệt trong ngày; không dùng cookie, không lưu IP.
    /// </summary>
    public sealed class SiteVisitCounter
    {
        private const int MaxVisitorKeysPerDay = 500_000;

        private readonly object _lock = new();
        private DateTime _visitorDay = DateTime.Now.Date;
        private HashSet<string> _visitorKeys = new();
        private readonly Dictionary<DateTime, (int PageViews, int Visitors)> _pending = new();

        public void Record(HttpContext ctx)
        {
            var now = DateTime.Now;
            var key = VisitorKey(ctx);
            lock (_lock)
            {
                if (now.Date != _visitorDay)
                {
                    _visitorDay = now.Date;
                    _visitorKeys = new HashSet<string>();
                }
                var newVisitor = _visitorKeys.Count < MaxVisitorKeysPerDay && _visitorKeys.Add(key);
                _pending.TryGetValue(now.Date, out var c);
                _pending[now.Date] = (c.PageViews + 1, c.Visitors + (newVisitor ? 1 : 0));
            }
        }

        public async Task FlushAsync(AppDbContext db, CancellationToken ct)
        {
            List<KeyValuePair<DateTime, (int PageViews, int Visitors)>> batch;
            lock (_lock)
            {
                if (_pending.Count == 0) return;
                batch = _pending.ToList();
                _pending.Clear();
            }

            for (var i = 0; i < batch.Count; i++)
            {
                var (day, c) = batch[i];
                try
                {
                    await db.Database.ExecuteSqlInterpolatedAsync($@"
MERGE SiteVisitDaily WITH (HOLDLOCK) AS t
USING (SELECT CAST({day} AS date) AS VisitDate) AS s ON t.VisitDate = s.VisitDate
WHEN MATCHED THEN UPDATE SET PageViews = t.PageViews + {c.PageViews}, Visitors = t.Visitors + {c.Visitors}
WHEN NOT MATCHED THEN INSERT (VisitDate, PageViews, Visitors) VALUES (s.VisitDate, {c.PageViews}, {c.Visitors});", ct);
                }
                catch
                {
                    // Ghi lỗi (vd. chưa chạy script tạo bảng) → trả số liệu lại hàng đợi, lần sau ghi tiếp
                    lock (_lock)
                    {
                        foreach (var (d, n) in batch.Skip(i))
                        {
                            _pending.TryGetValue(d, out var p);
                            _pending[d] = (p.PageViews + n.PageViews, p.Visitors + n.Visitors);
                        }
                    }
                    throw;
                }
            }
        }

        /// <summary>Chỉ đếm trang HTML trả về thành công cho người dùng; bỏ CMS, API, AJAX, bot.</summary>
        public static bool ShouldCount(HttpContext ctx)
        {
            if (!HttpMethods.IsGet(ctx.Request.Method)) return false;
            if (ctx.Response.StatusCode != StatusCodes.Status200OK) return false;
            if (ctx.Response.ContentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) != true) return false;

            var path = ctx.Request.Path;
            if (path.StartsWithSegments("/cms", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/Errors", StringComparison.OrdinalIgnoreCase))
                return false;

            if (ctx.Request.Headers.XRequestedWith == "XMLHttpRequest") return false;

            var ua = ctx.Request.Headers.UserAgent.ToString();
            if (string.IsNullOrWhiteSpace(ua)) return false;
            var uaLower = ua.ToLowerInvariant();
            return !(uaLower.Contains("bot") || uaLower.Contains("spider") || uaLower.Contains("crawl")
                     || uaLower.Contains("slurp") || uaLower.Contains("monitor") || uaLower.Contains("curl")
                     || uaLower.Contains("wget") || uaLower.Contains("python") || uaLower.Contains("headless"));
        }

        private static string VisitorKey(HttpContext ctx)
        {
            var raw = $"{ctx.Connection.RemoteIpAddress}|{ctx.Request.Headers.UserAgent}";
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)), 0, 16);
        }
    }

    /// <summary>Ghi dồn lượt truy cập vào DB mỗi phút và khi ứng dụng dừng.</summary>
    public sealed class SiteVisitFlushService : BackgroundService
    {
        private readonly SiteVisitCounter _counter;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SiteVisitFlushService> _logger;

        public SiteVisitFlushService(SiteVisitCounter counter, IServiceScopeFactory scopeFactory, ILogger<SiteVisitFlushService> logger)
        {
            _counter = counter;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                    await FlushAsync(stoppingToken);
            }
            catch (OperationCanceledException) { }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await FlushAsync(CancellationToken.None);
        }

        private async Task FlushAsync(CancellationToken ct)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await _counter.FlushAsync(db, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Không ghi được thống kê lượt truy cập (kiểm tra bảng SiteVisitDaily)");
            }
        }
    }
}
