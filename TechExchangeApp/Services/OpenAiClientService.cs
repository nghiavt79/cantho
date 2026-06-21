using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TechExchangeApp.Configuration;

namespace TechExchangeApp.Services
{
    public interface IOpenAiClientService
    {
        Task<string?> CreateResponseAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
    }

    public class OpenAiClientService : IOpenAiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly AiChatOptions _options;
        private readonly ILogger<OpenAiClientService> _logger;

        public OpenAiClientService(
            HttpClient httpClient,
            IConfiguration configuration,
            IOptions<AiChatOptions> options,
            ILogger<OpenAiClientService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string?> CreateResponseAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return null;
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = JsonContent(new
            {
                model = _options.ModelName,
                input = new object[]
                {
                    new
                    {
                        role = "system",
                        content = new object[]
                        {
                            new { type = "input_text", text = systemPrompt }
                        }
                    },
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "input_text", text = userPrompt }
                        }
                    }
                }
            });

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(5, _options.TimeoutSeconds)));

                using var response = await _httpClient.SendAsync(request, cts.Token);
                var body = await response.Content.ReadAsStringAsync(cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OpenAI chat response failed: {StatusCode} {Body}", response.StatusCode, body);
                    return null;
                }

                return ExtractOutputText(body);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "OpenAI chat response failed.");
                return null;
            }
        }

        private static StringContent JsonContent(object value)
        {
            return new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
        }

        private static string? ExtractOutputText(string json)
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("output_text", out var outputText))
            {
                return outputText.GetString();
            }

            if (!document.RootElement.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var parts = new List<string>();
            foreach (var item in output.EnumerateArray())
            {
                if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var contentItem in content.EnumerateArray())
                {
                    if (contentItem.TryGetProperty("text", out var text))
                    {
                        var value = text.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            parts.Add(value);
                        }
                    }
                }
            }

            return parts.Count == 0 ? null : string.Join(Environment.NewLine, parts);
        }
    }
}
