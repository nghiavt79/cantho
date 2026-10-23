using TechExchangeApp.Configuration;
using TechExchangeApp.Interfaces;

namespace TechExchangeApp.Services
{
    /// <summary>
    /// Chọn nhà cung cấp SMS theo tham số SMS_PROVIDER (đổi trong CMS, không cần deploy lại):
    /// SOUTHTELECOM = gửi thật qua South Telecom; giá trị khác = chỉ ghi log (StubSmsSender).
    /// </summary>
    public class SmsSenderRouter : ISmsSender
    {
        private readonly ISystemParameterService _params;
        private readonly SouthTelecomSmsSender _southTelecom;
        private readonly StubSmsSender _stub;

        public SmsSenderRouter(ISystemParameterService @params, SouthTelecomSmsSender southTelecom, StubSmsSender stub)
        {
            _params       = @params;
            _southTelecom = southTelecom;
            _stub         = stub;
        }

        public async Task<string?> SendAsync(string to, string message, string? smsId = null)
        {
            var provider = (await _params.GetAsync(ParameterKeys.SmsProvider))?.Trim().ToUpperInvariant();
            ISmsSender sender = provider == "SOUTHTELECOM" ? _southTelecom : _stub;
            return await sender.SendAsync(to, message, smsId);
        }
    }
}
