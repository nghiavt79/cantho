using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TechExchangeApp.Configuration;
using TechExchangeApp.Interfaces;

namespace TechExchangeApp.Services
{
    /// <summary>
    /// Gửi SMS Brandname CSKH trả trước qua API South Telecom (worldsms.vn).
    /// Tham số: SMS_API_URL, SMS_API_KEY (AUTHORIZATION_KEY), SMS_BRANDNAME, SMS_DLR_ENABLED.
    /// Tài liệu: docs/sms/Southtelecom - Hướng dẫn kết nối API SMS CSKH Trả trước.pdf
    /// </summary>
    public class SouthTelecomSmsSender : ISmsSender
    {
        public const string DefaultApiUrl = "https://api-04.worldsms.vn/apidebit/sendSMS";

        private readonly ISystemParameterService _params;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<SouthTelecomSmsSender> _logger;

        public SouthTelecomSmsSender(ISystemParameterService @params, IHttpClientFactory httpFactory, ILogger<SouthTelecomSmsSender> logger)
        {
            _params      = @params;
            _httpFactory = httpFactory;
            _logger      = logger;
        }

        public async Task<string?> SendAsync(string to, string message, string? smsId = null)
        {
            var apiUrl    = await _params.GetAsync(ParameterKeys.SmsApiUrl);
            var apiKey    = await _params.GetRequiredAsync(ParameterKeys.SmsApiKey);
            var brandname = await _params.GetRequiredAsync(ParameterKeys.SmsBrandname);
            var dlr       = await _params.GetBoolAsync(ParameterKeys.SmsDlrEnabled, false);

            var phone = NormalizePhone(to)
                ?? throw new SmsPermanentException(53, $"Số điện thoại không hợp lệ: {to}");

            var payload = new Dictionary<string, object?>
            {
                ["from"]    = brandname.Trim(),
                ["to"]      = phone,
                ["text"]    = message,
                ["unicode"] = message.Any(c => c > 127) ? 1 : 0,
                ["dlr"]     = dlr ? 1 : 0,
                ["smsid"]   = string.IsNullOrEmpty(smsId) ? Guid.NewGuid().ToString("N") : smsId
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, string.IsNullOrWhiteSpace(apiUrl) ? DefaultApiUrl : apiUrl.Trim());
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", apiKey.Trim());
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var http = _httpFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(30);

            using var response = await http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            int status = 0, errorCode = -1;
            string? description = null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.TryGetProperty("status", out var s)) status = ReadInt(s);
                if (root.TryGetProperty("errorcode", out var e)) errorCode = ReadInt(e);
                if (root.TryGetProperty("description", out var d)) description = d.ToString();
            }
            catch (JsonException)
            {
                // Phản hồi không phải JSON (lỗi gateway/HTTP) → cho phép gửi lại
                throw new Exception($"South Telecom HTTP {(int)response.StatusCode}: {Trunc(body, 300)}");
            }

            if (status == 1)
            {
                _logger.LogInformation("[SMS-ST] Sent smsid={SmsId} to {To}: {Body}", payload["smsid"], MaskPhone(phone), body);
                return Trunc(body, 1000);
            }

            var reason = $"South Telecom lỗi {errorCode}: {DescribeError(errorCode) ?? description}";
            _logger.LogWarning("[SMS-ST] Failed smsid={SmsId} to {To}: {Body}", payload["smsid"], MaskPhone(phone), body);

            // 50 = Gateway error → gửi lại được; các mã còn lại là lỗi cấu hình/dữ liệu
            if (errorCode == 50 || errorCode < 0)
                throw new Exception(reason);
            throw new SmsPermanentException(errorCode, reason);
        }

        /// <summary>Chuẩn hoá về dạng 84xxxxxxxxx. Trả null nếu không hợp lệ.</summary>
        public static string? NormalizePhone(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var digits = new string(input.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("0")) digits = "84" + digits[1..];
            else if (!digits.StartsWith("84")) digits = "84" + digits;
            // 84 + 9 số (di động VN)
            return digits.Length == 11 ? digits : null;
        }

        /// <summary>Mô tả mã lỗi API (mục 2.4) và mã lỗi DLR (mục 2.6).</summary>
        public static string? DescribeError(int code) => code switch
        {
            40 or 41 or 42 => "Sai khoá kết nối (SMS_API_KEY)",
            51  => "IP máy chủ chưa được South Telecom cho phép",
            52  => "Tham số gửi không hợp lệ",
            53  => "Số điện thoại không hợp lệ",
            531 => "Thuê bao chuyển mạng, mạng mới chưa đăng ký với South Telecom",
            54  => "Brandname không hợp lệ (SMS_BRANDNAME)",
            55  => "Nội dung không hợp lệ",
            551 => "Nội dung quá dài",
            50  => "Lỗi gateway South Telecom",
            80  => "Tài khoản không được gửi SMS trả trước qua API",
            81  => "Tài khoản không được gửi SMS trả trước",
            82  => "Tài khoản hết hạn mức/số dư",
            _   => null
        };

        public static string? DescribeDlrError(int code) => code switch
        {
            1  => "Tin trùng, không gửi sang nhà mạng",
            2  => "Brandname chưa đăng ký",
            3  => "Lỗi dịch vụ nhà mạng",
            4  => "Độ dài tin vượt quy định nhà mạng",
            5  => "Nội dung chưa đăng ký (template)",
            6  => "Nội dung chứa từ khoá bị chặn",
            13 => "Thuê bao đăng ký chuyển mạng chưa hoàn tất",
            80 => "Tài khoản bị chặn gửi SMS trả trước",
            82 => "Số dư tài khoản không đủ",
            98 => "Tin quảng cáo bị chặn",
            99 => "Lỗi không xác định",
            _  => null
        };

        public static string MaskPhone(string phone) =>
            phone.Length > 6 ? phone[..4] + new string('*', phone.Length - 7) + phone[^3..] : phone;

        private static int ReadInt(JsonElement el) =>
            el.ValueKind == JsonValueKind.Number ? el.GetInt32()
            : int.TryParse(el.ToString(), out var v) ? v : -1;

        private static string Trunc(string s, int max) => s.Length <= max ? s : s[..max];
    }
}
