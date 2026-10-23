using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Configuration;
using TechExchangeApp.Data;
using TechExchangeApp.Enums;
using TechExchangeApp.Interfaces;
using TechExchangeApp.Services;

namespace TechExchangeApp.Controllers.Api
{
    /// <summary>
    /// GET /api/sms/dlr — South Telecom gọi về báo kết quả đẩy tin sang nhà mạng (mục 2.6).
    /// Chỉ nhận từ IP trong tham số SMS_DLR_ALLOWED_IPS ("*" = không kiểm tra).
    /// smsid dạng "N{NotificationId}" sẽ cập nhật lại bảng Notifications.
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("api/sms")]
    public class SmsDlrController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISystemParameterService _params;
        private readonly ILogger<SmsDlrController> _logger;

        public SmsDlrController(AppDbContext context, ISystemParameterService @params, ILogger<SmsDlrController> logger)
        {
            _context = context;
            _params  = @params;
            _logger  = logger;
        }

        [HttpGet("dlr")]
        public async Task<IActionResult> Dlr(
            string? smsid, int? status, int? errorcode, string? carrier, int? mnp,
            long? receivedts, long? deliveredts, string? to)
        {
            var ip = HttpContext.Connection.RemoteIpAddress;
            if (!await IsAllowedIpAsync(ip))
            {
                _logger.LogWarning("[SMS-DLR] Từ chối IP {Ip} (smsid={SmsId})", ip, smsid);
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            _logger.LogInformation("[SMS-DLR] smsid={SmsId} status={Status} errorcode={Error} carrier={Carrier} to={To}",
                smsid, status, errorcode, carrier, to == null ? null : SouthTelecomSmsSender.MaskPhone(to));

            if (!string.IsNullOrEmpty(smsid) && smsid.StartsWith("N") && int.TryParse(smsid[1..], out var notificationId))
            {
                var n = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId);
                if (n != null && n.Channel == (int)NotificationChannel.Sms)
                {
                    var delivered = deliveredts.HasValue
                        ? DateTimeOffset.FromUnixTimeSeconds(deliveredts.Value).UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss") + " UTC"
                        : "";
                    n.ProviderResponse = $"DLR status={status}; errorcode={errorcode}; carrier={carrier}; mnp={mnp}; delivered={delivered}";

                    if (status == 1)
                    {
                        n.Status    = (int)NotificationStatus.Success;
                        n.LastError = null;
                    }
                    else
                    {
                        n.Status    = (int)NotificationStatus.Failed;
                        n.LastError = $"Nhà mạng từ chối (DLR {errorcode}): {SouthTelecomSmsSender.DescribeDlrError(errorcode ?? 99)}";
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return Ok("OK");
        }

        private async Task<bool> IsAllowedIpAsync(IPAddress? ip)
        {
            var raw = (await _params.GetAsync(ParameterKeys.SmsDlrAllowedIps))?.Trim();
            if (string.IsNullOrEmpty(raw) || ip == null) return false;
            if (raw == "*") return true;

            var caller = ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip;
            return raw.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                      .Any(x => IPAddress.TryParse(x, out var allowed) && allowed.Equals(caller));
        }
    }
}
