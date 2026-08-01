using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TechExchangeApp.Services.Translation
{
    /// <summary>
    /// Dịch qua Claude (Anthropic Messages API) bằng HttpClient thô.
    /// haiku → claude-haiku-4-5, sonnet → claude-sonnet-5. Key do admin nhập (SYS_PARAMETERS).
    /// Dịch cả lô trong 1 request (trả JSON mảng); nếu lệch số phần tử → fallback dịch từng đoạn.
    /// </summary>
    public sealed class ClaudeTranslator : ITranslator
    {
        private const string Endpoint = "https://api.anthropic.com/v1/messages";
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _model;

        public string Name => "claude:" + _model;

        public ClaudeTranslator(HttpClient http, string apiKey, string model)
        {
            _http = http;
            _apiKey = apiKey;
            _model = model; // "claude-haiku-4-5" | "claude-sonnet-5"
        }

        public async Task<IReadOnlyList<string>> TranslateAsync(IReadOnlyList<string> segments, CancellationToken ct = default)
        {
            if (segments.Count == 0) return Array.Empty<string>();

            var result = await TryBatchAsync(segments, ct);
            if (result != null && result.Count == segments.Count) return result;

            // Fallback: dịch từng đoạn (bền vững khi model trả JSON không khớp)
            var one = new string[segments.Count];
            for (int i = 0; i < segments.Count; i++)
            {
                var r = await TryBatchAsync(new[] { segments[i] }, ct);
                one[i] = (r != null && r.Count == 1) ? r[0] : segments[i]; // hỏng → giữ nguyên VI
            }
            return one;
        }

        private async Task<IReadOnlyList<string>?> TryBatchAsync(IReadOnlyList<string> segments, CancellationToken ct)
        {
            var input = JsonSerializer.Serialize(segments);
            var sys =
                "You are a professional Vietnamese→English translator for a technology-exchange marketplace. " +
                "You will receive a JSON array of Vietnamese strings. Translate each into natural, professional English. " +
                "Return ONLY a JSON array of the same length, same order, no extra text, no markdown. " +
                "Preserve numbers, units, proper nouns and any placeholder tokens exactly.";

            var body = new
            {
                model = _model,
                max_tokens = 8000,
                system = sys,
                messages = new[] { new { role = "user", content = input } }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            req.Headers.TryAddWithoutValidation("x-api-key", _apiKey);
            req.Headers.TryAddWithoutValidation("anthropic-version", "2023-06-01");
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8);
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                return null;

            var sb = new StringBuilder();
            foreach (var block in content.EnumerateArray())
                if (block.TryGetProperty("type", out var t) && t.GetString() == "text"
                    && block.TryGetProperty("text", out var txt))
                    sb.Append(txt.GetString());

            var outText = sb.ToString().Trim();
            outText = StripFence(outText);

            try
            {
                using var arr = JsonDocument.Parse(outText);
                if (arr.RootElement.ValueKind != JsonValueKind.Array) return null;
                return arr.RootElement.EnumerateArray().Select(x => x.GetString() ?? "").ToList();
            }
            catch { return null; }
        }

        private static string StripFence(string s)
        {
            if (s.StartsWith("```"))
            {
                int nl = s.IndexOf('\n');
                if (nl >= 0) s = s[(nl + 1)..];
                if (s.EndsWith("```")) s = s[..^3];
            }
            return s.Trim();
        }
    }
}
