using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using TechExchangeApp.Configuration;
using TechExchangeApp.Interfaces;

namespace TechExchangeApp.Services.EContracts
{
    public interface IEContractClient
    {
        /// <summary>POST tp/contracts/create. Không tự gửi lại; lỗi Unknown phải đối soát trước khi tạo lại.</summary>
        Task<EContractDto> CreateContractAsync(EContractCreateRequest request, byte[] pdf, string fileName, CancellationToken cancellationToken = default);

        /// <summary>PUT tp/contracts/my-contract/issue cho một hợp đồng đã có id.</summary>
        Task IssueContractAsync(long contractId, CancellationToken cancellationToken = default);

        /// <summary>GET tp/contracts. Không thấy ở trang này không có nghĩa hợp đồng không tồn tại.</summary>
        Task<EContractPage> SearchContractsAsync(string? email, string? phone, int page, int size, CancellationToken cancellationToken = default);

        /// <summary>GET tp/contracts/{id}/documents.</summary>
        Task<List<EContractDocumentDto>> GetDocumentsAsync(long contractId, CancellationToken cancellationToken = default);

        /// <summary>Tải PDF theo đúng URL do API trả; chỉ gắn token khi URL thuộc host eContract.</summary>
        Task<byte[]> DownloadPdfAsync(string documentPath, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Client eContract MobiFone. Dùng một tài khoản tích hợp của Trung tâm.
    /// Không ghi log token, mật khẩu, nội dung PDF hay URL tải có chữ ký truy cập.
    /// </summary>
    public class EContractClient : IEContractClient
    {
        private const string ApiPrefix = "/api/v1";
        private const int ReadAttempts = 3;
        private const int MaxRedirects = 3;
        private const int MaxPdfBytes = 50 * 1024 * 1024;
        private static readonly TimeSpan DefaultTokenLifetime = TimeSpan.FromMinutes(30);
        private static readonly SemaphoreSlim LoginLock = new(1, 1);

        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _httpClient;
        private readonly ISystemParameterService _parameters;
        private readonly IMemoryCache _cache;
        private readonly ILogger<EContractClient> _logger;

        public EContractClient(
            HttpClient httpClient,
            ISystemParameterService parameters,
            IMemoryCache cache,
            ILogger<EContractClient> logger)
        {
            _httpClient = httpClient;
            _parameters = parameters;
            _cache = cache;
            _logger = logger;
        }

        public async Task<EContractDto> CreateContractAsync(EContractCreateRequest request, byte[] pdf, string fileName, CancellationToken cancellationToken = default)
        {
            if (!IsPdf(pdf))
                throw new EContractException(EContractFailureKind.Rejected, "File gửi eContract không phải PDF.");

            var json = JsonSerializer.Serialize(request, WriteOptions);

            var body = await SendJsonAsync(HttpMethod.Post, "/tp/contracts/create", () =>
            {
                var form = new MultipartFormDataContent();

                var file = new ByteArrayContent(pdf);
                file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                form.Add(file, "file_contract", fileName);

                // Giống Postman: part text không có Content-Type riêng
                var jsonPart = new StringContent(json, Encoding.UTF8);
                jsonPart.Headers.ContentType = null;
                form.Add(jsonPart, "jsonRequest");

                return form;
            }, isRead: false, cancellationToken);

            var contract = Deserialize<EContractDto>(body, "tạo hợp đồng");
            if (contract.Id <= 0)
                throw new EContractException(EContractFailureKind.Unknown, "Tạo hợp đồng: response không có id hợp lệ.");
            return contract;
        }

        public async Task IssueContractAsync(long contractId, CancellationToken cancellationToken = default)
        {
            var payload = JsonSerializer.Serialize(new[] { contractId });
            var body = await SendJsonAsync(HttpMethod.Put, "/tp/contracts/my-contract/issue",
                () => new StringContent(payload, Encoding.UTF8, "application/json"),
                isRead: false, cancellationToken);

            using var doc = Parse(body, "phát hành hợp đồng");
            var confirmed = doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("success", out var success)
                && success.ValueKind == JsonValueKind.True;
            if (!confirmed)
                throw new EContractException(EContractFailureKind.Unknown, "Phát hành hợp đồng: response không xác nhận success=true.");
        }

        public async Task<EContractPage> SearchContractsAsync(string? email, string? phone, int page, int size, CancellationToken cancellationToken = default)
        {
            var query = new StringBuilder("?contractStatus=-1");
            if (!string.IsNullOrWhiteSpace(email)) query.Append("&email=").Append(Uri.EscapeDataString(email));
            if (!string.IsNullOrWhiteSpace(phone)) query.Append("&phone=").Append(Uri.EscapeDataString(phone));
            query.Append("&page=").Append(page).Append("&size=").Append(size);

            var body = await SendJsonAsync(HttpMethod.Get, "/tp/contracts" + query, null, isRead: true, cancellationToken);
            return Deserialize<EContractPage>(body, "tra cứu hợp đồng");
        }

        public async Task<List<EContractDocumentDto>> GetDocumentsAsync(long contractId, CancellationToken cancellationToken = default)
        {
            var body = await SendJsonAsync(HttpMethod.Get, $"/tp/contracts/{contractId}/documents", null, isRead: true, cancellationToken);
            return Deserialize<List<EContractDocumentDto>>(body, "lấy tài liệu");
        }

        public async Task<byte[]> DownloadPdfAsync(string documentPath, CancellationToken cancellationToken = default)
        {
            var baseUrl = await GetBaseUrlAsync();
            var eContractAuthority = new Uri(baseUrl).Authority;
            var allowed = await GetDownloadAuthoritiesAsync(eContractAuthority);

            var uri = RequireAllowedUrl(documentPath, allowed);

            for (var hop = 0; hop <= MaxRedirects; hop++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);

                // Không gửi token sang host lưu trữ (presigned URL)
                if (string.Equals(uri.Authority, eContractAuthority, StringComparison.OrdinalIgnoreCase))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync(cancellationToken));

                using var response = await SendRawAsync(request, "tải PDF", cancellationToken);

                if (IsRedirect(response.StatusCode) && response.Headers.Location != null)
                {
                    var next = response.Headers.Location.IsAbsoluteUri
                        ? response.Headers.Location
                        : new Uri(uri, response.Headers.Location);
                    uri = RequireAllowedUrl(next.ToString(), allowed);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    // 403/404: presigned URL có thể đã hết hạn, lấy lại danh sách tài liệu
                    _logger.LogWarning("eContract download failed: host={Host} status={Status}", uri.Host, (int)response.StatusCode);
                    throw new EContractException(EContractFailureKind.Rejected, $"Tải PDF thất bại (HTTP {(int)response.StatusCode}).", (int)response.StatusCode);
                }

                var bytes = await ReadLimitedAsync(response, cancellationToken);
                if (!IsPdf(bytes))
                    throw new EContractException(EContractFailureKind.Rejected, "File tải về không phải PDF.");
                return bytes;
            }

            throw new EContractException(EContractFailureKind.Rejected, "Tải PDF: quá nhiều lần chuyển hướng.");
        }

        // ── Gửi request ──────────────────────────────────────────────────────

        private async Task<string> SendJsonAsync(HttpMethod method, string relativePath, Func<HttpContent>? contentFactory, bool isRead, CancellationToken cancellationToken)
        {
            var url = await GetBaseUrlAsync() + ApiPrefix + relativePath;
            var operation = relativePath.Split('?')[0];
            var attempts = isRead ? ReadAttempts : 1;
            var reauthenticated = false;

            for (var attempt = 1; ; attempt++)
            {
                var token = await GetTokenAsync(cancellationToken);

                using var request = new HttpRequestMessage(method, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (contentFactory != null) request.Content = contentFactory();

                HttpResponseMessage response;
                try
                {
                    response = await SendRawAsync(request, operation, cancellationToken);
                }
                catch (EContractException) when (attempt < attempts)
                {
                    await Task.Delay(200 * attempt, cancellationToken);
                    continue;
                }

                using (response)
                {
                    var status = (int)response.StatusCode;
                    _logger.LogInformation("eContract {Method} {Operation} -> {Status}", method, operation, status);

                    // 401: server chưa xử lý nghiệp vụ nên đăng nhập lại và gửi lại một lần là an toàn
                    if (response.StatusCode == HttpStatusCode.Unauthorized && !reauthenticated)
                    {
                        reauthenticated = true;
                        await InvalidateTokenAsync();
                        attempt--;
                        continue;
                    }

                    var body = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (status >= 500)
                    {
                        if (attempt < attempts)
                        {
                            await Task.Delay(200 * attempt, cancellationToken);
                            continue;
                        }
                        throw new EContractException(EContractFailureKind.Unknown, $"eContract lỗi máy chủ (HTTP {status}).", status);
                    }

                    if (status < 200 || status >= 300)
                        throw new EContractException(EContractFailureKind.Rejected, $"eContract từ chối (HTTP {status}): {ReadMessage(body)}", status);

                    // HTTP 2xx vẫn có thể là thất bại nghiệp vụ
                    if (IsBusinessFailure(body))
                        throw new EContractException(EContractFailureKind.Rejected, $"eContract từ chối: {ReadMessage(body)}", status);

                    return body;
                }
            }
        }

        /// <summary>Lỗi mạng và timeout quy về Unknown: không biết server đã xử lý hay chưa.</summary>
        private async Task<HttpResponseMessage> SendRawAsync(HttpRequestMessage request, string operation, CancellationToken cancellationToken)
        {
            try
            {
                return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("eContract timeout: {Operation}", operation);
                throw new EContractException(EContractFailureKind.Unknown, "eContract không phản hồi kịp (timeout).", null, ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("eContract network error: {Operation} {Error}", operation, ex.GetType().Name);
                throw new EContractException(EContractFailureKind.Unknown, "Không kết nối được eContract.", null, ex);
            }
        }

        // ── Đăng nhập ────────────────────────────────────────────────────────

        private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
        {
            var cacheKey = await GetTokenCacheKeyAsync();
            if (_cache.TryGetValue(cacheKey, out string? cached) && !string.IsNullOrEmpty(cached))
                return cached;

            await LoginLock.WaitAsync(cancellationToken);
            try
            {
                if (_cache.TryGetValue(cacheKey, out cached) && !string.IsNullOrEmpty(cached))
                    return cached;

                var token = await LoginAsync(cancellationToken);
                _cache.Set(cacheKey, token, GetTokenLifetime(token));
                return token;
            }
            finally
            {
                LoginLock.Release();
            }
        }

        private async Task InvalidateTokenAsync() => _cache.Remove(await GetTokenCacheKeyAsync());

        private async Task<string> GetTokenCacheKeyAsync()
            => $"econtract:token:{await GetBaseUrlAsync()}:{await _parameters.GetAsync(ParameterKeys.EContractEmail)}";

        private async Task<string> LoginAsync(CancellationToken cancellationToken)
        {
            var email = await _parameters.GetAsync(ParameterKeys.EContractEmail);
            var password = await _parameters.GetAsync(ParameterKeys.EContractPassword);
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
                throw new EContractException(EContractFailureKind.Configuration, "Chưa cấu hình tài khoản eContract.");

            using var request = new HttpRequestMessage(HttpMethod.Post, await GetBaseUrlAsync() + ApiPrefix + "/auth")
            {
                Content = new StringContent(JsonSerializer.Serialize(new { email, password, type = 0 }), Encoding.UTF8, "application/json")
            };

            string body;
            int status;
            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                status = (int)response.StatusCode;
                body = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
            {
                throw new EContractException(EContractFailureKind.Authentication, "Không kết nối được eContract để đăng nhập.", null, ex);
            }

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (status is >= 200 and < 300
                    && root.ValueKind == JsonValueKind.Object
                    && root.TryGetProperty("code", out var code) && code.GetString() == "00"
                    && root.TryGetProperty("access_token", out var token) && !string.IsNullOrEmpty(token.GetString()))
                {
                    return token.GetString()!;
                }
            }
            catch (JsonException)
            {
                // Xử lý chung bên dưới
            }

            _logger.LogWarning("eContract login failed: status={Status}", status);
            throw new EContractException(EContractFailureKind.Authentication, "Đăng nhập eContract thất bại, kiểm tra tài khoản tích hợp.", status);
        }

        /// <summary>Dùng hạn trong JWT nếu đọc được, trừ hao 60 giây.</summary>
        private static TimeSpan GetTokenLifetime(string token)
        {
            try
            {
                var payload = token.Split('.')[1].Replace('-', '+').Replace('_', '/');
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
                if (doc.RootElement.TryGetProperty("exp", out var exp))
                {
                    var lifetime = DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64()) - DateTimeOffset.UtcNow - TimeSpan.FromSeconds(60);
                    if (lifetime > TimeSpan.Zero) return lifetime;
                }
            }
            catch (Exception ex) when (ex is FormatException or JsonException or IndexOutOfRangeException or InvalidOperationException or ArgumentOutOfRangeException)
            {
                // Token không phải JWT đọc được: dùng hạn mặc định, gặp 401 sẽ đăng nhập lại
            }
            return DefaultTokenLifetime;
        }

        // ── Cấu hình và URL ──────────────────────────────────────────────────

        private async Task<string> GetBaseUrlAsync()
        {
            var baseUrl = (await _parameters.GetAsync(ParameterKeys.EContractBaseUrl))?.Trim().TrimEnd('/');
            if (string.IsNullOrEmpty(baseUrl)
                || !Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
                || uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new EContractException(EContractFailureKind.Configuration, "ECONTRACT_BASE_URL phải là địa chỉ https.");
            }
            return baseUrl;
        }

        private async Task<HashSet<string>> GetDownloadAuthoritiesAsync(string eContractAuthority)
        {
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { eContractAuthority };
            var extra = await _parameters.GetAsync(ParameterKeys.EContractDownloadHosts);
            if (!string.IsNullOrWhiteSpace(extra))
            {
                foreach (var host in extra.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    allowed.Add(host);
            }
            return allowed;
        }

        private static Uri RequireAllowedUrl(string url, HashSet<string> allowedAuthorities)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                throw new EContractException(EContractFailureKind.Rejected, "Đường dẫn tải PDF phải là URL https đầy đủ.");
            if (!allowedAuthorities.Contains(uri.Authority))
                throw new EContractException(EContractFailureKind.Configuration, $"Host tải PDF chưa được phép: {uri.Authority}. Thêm vào ECONTRACT_DOWNLOAD_HOSTS nếu đúng là host của eContract.");
            return uri;
        }

        private static bool IsRedirect(HttpStatusCode status)
            => status is HttpStatusCode.Moved or HttpStatusCode.Found or HttpStatusCode.SeeOther
                or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;

        private static async Task<byte[]> ReadLimitedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.Content.Headers.ContentLength > MaxPdfBytes)
                throw new EContractException(EContractFailureKind.Rejected, "File PDF vượt giới hạn kích thước.");

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var buffer = new MemoryStream();
            var chunk = new byte[81920];
            int read;
            while ((read = await stream.ReadAsync(chunk, cancellationToken)) > 0)
            {
                if (buffer.Length + read > MaxPdfBytes)
                    throw new EContractException(EContractFailureKind.Rejected, "File PDF vượt giới hạn kích thước.");
                buffer.Write(chunk, 0, read);
            }
            return buffer.ToArray();
        }

        // ── Đọc response ─────────────────────────────────────────────────────

        private static bool IsPdf(byte[]? bytes)
            => bytes is { Length: > 5 } && bytes[0] == '%' && bytes[1] == 'P' && bytes[2] == 'D' && bytes[3] == 'F' && bytes[4] == '-';

        private static bool IsBusinessFailure(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                return doc.RootElement.ValueKind == JsonValueKind.Object
                    && doc.RootElement.TryGetProperty("success", out var success)
                    && success.ValueKind == JsonValueKind.False;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>Chỉ lấy trường message, không đưa toàn bộ body vào thông báo lỗi.</summary>
        private static string ReadMessage(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind == JsonValueKind.Object
                    && doc.RootElement.TryGetProperty("message", out var message)
                    && message.ValueKind == JsonValueKind.String)
                {
                    var text = message.GetString() ?? "";
                    return text.Length > 300 ? text[..300] : text;
                }
            }
            catch (JsonException)
            {
                // Response không phải JSON
            }
            return "không có thông báo chi tiết";
        }

        private static JsonDocument Parse(string body, string operation)
        {
            try
            {
                return JsonDocument.Parse(body);
            }
            catch (JsonException ex)
            {
                throw new EContractException(EContractFailureKind.Unknown, $"Không đọc được response khi {operation}.", null, ex);
            }
        }

        private static T Deserialize<T>(string body, string operation) where T : class
        {
            try
            {
                return JsonSerializer.Deserialize<T>(body)
                    ?? throw new EContractException(EContractFailureKind.Unknown, $"Response rỗng khi {operation}.");
            }
            catch (JsonException ex)
            {
                throw new EContractException(EContractFailureKind.Unknown, $"Không đọc được response khi {operation}.", null, ex);
            }
        }
    }
}
