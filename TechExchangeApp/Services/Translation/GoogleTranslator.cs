using System.Text.Json;

namespace TechExchangeApp.Services.Translation
{
    /// <summary>
    /// Dịch qua Google Cloud Translation API v2 (REST, key=API key).
    /// Hỗ trợ dịch nhiều đoạn trong 1 request (nhiều tham số q), trả về đúng thứ tự.
    /// format=text để không diễn giải HTML (ta đã tách text node ở tầng trên).
    /// </summary>
    public sealed class GoogleTranslator : ITranslator
    {
        private const string Endpoint = "https://translation.googleapis.com/language/translate/v2";
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public string Name => "google";

        public GoogleTranslator(HttpClient http, string apiKey)
        {
            _http = http;
            _apiKey = apiKey;
        }

        public async Task<IReadOnlyList<string>> TranslateAsync(IReadOnlyList<string> segments, CancellationToken ct = default)
        {
            if (segments.Count == 0) return Array.Empty<string>();

            var form = new List<KeyValuePair<string, string>>
            {
                new("key", _apiKey),
                new("source", "vi"),
                new("target", "en"),
                new("format", "text"),
            };
            foreach (var s in segments) form.Add(new("q", s));

            using var resp = await _http.PostAsync(Endpoint, new FormUrlEncodedContent(form), ct);
            if (!resp.IsSuccessStatusCode) return segments; // hỏng → giữ nguyên VI

            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("data", out var data)
                || !data.TryGetProperty("translations", out var trans)
                || trans.ValueKind != JsonValueKind.Array)
                return segments;

            var outList = trans.EnumerateArray()
                .Select(x => x.TryGetProperty("translatedText", out var tt) ? tt.GetString() ?? "" : "")
                .ToList();

            return outList.Count == segments.Count ? outList : segments;
        }
    }
}
