using TechExchangeApp.Interfaces;
using TechExchangeApp.Services;

namespace TechExchangeApp.BackgroundServices
{
    /// <summary>
    /// Chạy quét cảnh báo sản phẩm trùng lặp / quá hạn mỗi ngày một lần (HSMT A.8.2),
    /// vào giờ PRODUCT_WARNING_SCAN_HOUR (mặc định 2 giờ sáng). Lỗi chỉ ghi log, không làm dừng ứng dụng.
    /// </summary>
    public class ProductWarningBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ProductWarningBackgroundService> _logger;
        private DateTime _lastRunDate = DateTime.MinValue;

        public ProductWarningBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ProductWarningBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var hour = await scope.ServiceProvider.GetRequiredService<ISystemParameterService>()
                        .GetIntAsync("PRODUCT_WARNING_SCAN_HOUR", 2);
                    var now = DateTime.Now;
                    if (now.Date > _lastRunDate && now.Hour >= hour)
                    {
                        await scope.ServiceProvider.GetRequiredService<IProductWarningScanner>().ScanAsync(stoppingToken);
                        _lastRunDate = now.Date;
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    try { _logger.LogWarning(ex, "[ProductWarning] Quét định kỳ lỗi, thử lại sau."); } catch { }
                }

                try { await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }
}
