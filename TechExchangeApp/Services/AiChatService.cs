using Microsoft.Extensions.Options;
using TechExchangeApp.Configuration;
using TechExchangeApp.Models;

namespace TechExchangeApp.Services
{
    public interface IAiChatService
    {
        Task<AiChatMessageResponse> ReplyAsync(AiChatMessageRequest request, CancellationToken cancellationToken = default);
        IReadOnlyList<string> GetSuggestions();
    }

    public class AiChatService : IAiChatService
    {
        private readonly IAiKnowledgeService _knowledgeService;
        private readonly IOpenAiClientService _openAiClient;
        private readonly AiChatOptions _options;

        public AiChatService(
            IAiKnowledgeService knowledgeService,
            IOpenAiClientService openAiClient,
            IOptions<AiChatOptions> options)
        {
            _knowledgeService = knowledgeService;
            _openAiClient = openAiClient;
            _options = options.Value;
        }

        public async Task<AiChatMessageResponse> ReplyAsync(AiChatMessageRequest request, CancellationToken cancellationToken = default)
        {
            var sessionKey = string.IsNullOrWhiteSpace(request.SessionKey)
                ? Guid.NewGuid().ToString("N")
                : request.SessionKey.Trim();

            if (!_options.IsEnabled)
            {
                return new AiChatMessageResponse
                {
                    Success = true,
                    SessionKey = sessionKey,
                    NeedsContactInfo = true,
                    Message = "He thong dang cap nhat du lieu. Anh/chi vui long de lai thong tin, bo phan phu trach se lien he ho tro."
                };
            }

            var question = request.Message.Trim();

            var cannedResponse = TryGetCannedResponse(question);
            if (!string.IsNullOrWhiteSpace(cannedResponse))
            {
                return new AiChatMessageResponse
                {
                    Success = true,
                    SessionKey = sessionKey,
                    NeedsContactInfo = false,
                    Message = cannedResponse
                };
            }

            var sources = await _knowledgeService.SearchAsync(question, cancellationToken);
            string? aiText = null;
            if (_options.UseOpenAiResponses)
            {
                var prompt = BuildUserPrompt(question, sources);
                aiText = await _openAiClient.CreateResponseAsync(_options.SystemPrompt, prompt, cancellationToken);
            }

            return new AiChatMessageResponse
            {
                Success = true,
                SessionKey = sessionKey,
                Sources = sources,
                NeedsContactInfo = ShouldAskForContact(question, sources),
                Message = string.IsNullOrWhiteSpace(aiText)
                    ? BuildLocalResponse(question, sources)
                    : aiText.Trim()
            };
        }

        private static string? TryGetCannedResponse(string message)
        {
            var normalized = NormalizeIntentText(message);

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return null;
            }

            var greetings = new[]
            {
                "xin chao",
                "chao",
                "hello",
                "hi",
                "alo"
            };

            if (greetings.Any(x => normalized == x || normalized.StartsWith(x + " ")))
            {
                return "Xin chao anh/chi. Toi co the ho tro tim cong nghe, san pham CNTB, nha cung ung, chuyen gia tu van hoac ghi nhan yeu cau lien he. Anh/chi muon tim thong tin nao?";
            }

            if (normalized is "cam on" or "thanks" or "thank you")
            {
                return "Rat vui duoc ho tro anh/chi. Neu can them thong tin ve cong nghe, san pham CNTB hoac dich vu tu van, anh/chi cu nhan tin tai day.";
            }

            if (normalized.Contains("lien he") || normalized.Contains("so dien thoai") || normalized.Contains("dia chi"))
            {
                return "Anh/chi co the de lai ho ten, so dien thoai hoac email trong khung chat. Trung tam se ghi nhan va lien he ho tro trong thoi gian som nhat.";
            }

            return null;
        }

        private static string NormalizeIntentText(string value)
        {
            var normalized = value.Trim().ToLowerInvariant();
            var chars = normalized.Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray();

            return new string(chars)
                .Normalize(System.Text.NormalizationForm.FormC)
                .Replace('đ', 'd')
                .Replace('Đ', 'd');
        }

        public IReadOnlyList<string> GetSuggestions()
        {
            return new[]
            {
                "Tim cong nghe",
                "Tim san pham CNTB",
                "Dich vu tu van",
                "Gui yeu cau ho tro",
                "Lien he trung tam"
            };
        }

        private static string BuildUserPrompt(string question, IReadOnlyList<AiKnowledgeItem> sources)
        {
            var context = sources.Count == 0
                ? "Khong tim thay du lieu phu hop trong website."
                : string.Join(Environment.NewLine, sources.Select((x, i) =>
                    $"{i + 1}. [{x.SourceType}] {x.Title}\nURL: {x.Url}\nTom tat: {x.Summary}"));

            return $"""
            Cau hoi cua nguoi dung:
            {question}

            Du lieu website lien quan:
            {context}

            Hay tra loi ngan gon, dung tieng Viet, uu tien dua link nguon neu co. Neu du lieu chua du, khong tu bia thong tin; hay de nghi nguoi dung de lai ten, so dien thoai/email de trung tam tu van.
            """;
        }

        private static string BuildLocalResponse(string question, IReadOnlyList<AiKnowledgeItem> sources)
        {
            if (sources.Count == 0)
            {
                return "Toi chua tim thay du lieu phu hop tren website cho yeu cau nay. Anh/chi co the de lai ho ten, so dien thoai hoac email de trung tam lien he tu van chi tiet.";
            }

            var lines = new List<string>
            {
                "Toi tim thay mot so thong tin lien quan tren website:"
            };

            lines.AddRange(sources.Take(4).Select(x =>
            {
                var url = string.IsNullOrWhiteSpace(x.Url) ? string.Empty : $" ({x.Url})";
                return $"- {x.Title}: {x.Summary}{url}";
            }));

            lines.Add("Anh/chi muon duoc tu van truc tiep thi co the de lai thong tin lien he.");
            return string.Join(Environment.NewLine, lines);
        }

        private static bool ShouldAskForContact(string question, IReadOnlyList<AiKnowledgeItem> sources)
        {
            var normalized = question.ToLowerInvariant();
            return sources.Count == 0
                || normalized.Contains("tu van")
                || normalized.Contains("lien he")
                || normalized.Contains("bao gia")
                || normalized.Contains("ho tro");
        }
    }
}
