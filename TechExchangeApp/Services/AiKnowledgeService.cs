using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using TechExchangeApp.Application.Services;
using TechExchangeApp.Configuration;
using TechExchangeApp.Data.Entities;
using TechExchangeApp.Models;

namespace TechExchangeApp.Services
{
    public interface IAiKnowledgeService
    {
        Task<IReadOnlyList<AiKnowledgeItem>> SearchAsync(string query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Plain category browse for chatbox quick actions — no text search, no ranking.
        /// See ISearchService.GetRecentByTypeAsync.
        /// </summary>
        Task<IReadOnlyList<AiKnowledgeItem>> BrowseByTypeAsync(IReadOnlyList<string> typeNames, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Looks up chat context using the site's existing Full-Text Search index
    /// (SearchIndexContents + CONTAINSTABLE, see ISearchService.SearchByPhrasesAsync) instead of
    /// re-implementing search with LIKE/COLLATE. That index already covers every entity type
    /// the chatbot needs (news, tech, thiết bị, nhà cung ứng, chuyên gia, OCOP, tài sản trí tuệ)
    /// and gives real relevance ranking (KEY_TBL.RANK) plus proper word-boundary matching —
    /// a hand-rolled substring search could not do either without much more work.
    /// </summary>
    public class AiKnowledgeService : IAiKnowledgeService
    {
        // Natural questions carry filler words (pronouns, question words, generic verbs). Most
        // Vietnamese words are two syllables ("chuyên gia", "tư vấn", "cần thơ") that only carry
        // their real meaning as a pair, so significant words are paired up (non-overlapping)
        // into phrases rather than searched as lone syllables — searching for "gia" or "tư" alone
        // would substring/word-match almost anything. Cap how many phrases we OR together.
        private const int MaxSearchTerms = 6;

        private static readonly HashSet<string> StopWords = new(StringComparer.Ordinal)
        {
            "toi", "ban", "minh", "chung", "cac", "anh", "chi", "em", "ong", "ba", "ho",
            "muon", "can", "hay", "xin", "cho", "giup", "lam", "on", "nho", "vui", "long",
            "la", "co", "duoc", "va", "hoac", "voi", "cua", "nhung", "mot", "nay", "do", "kia", "ay", "nhu",
            "trong", "ngoai", "tai", "tren", "duoi", "ve", "den", "theo", "boi", "qua",
            // "tu" ("từ" = from) deliberately NOT listed — it collides after accent-stripping
            // with "tư" (as in "tư vấn"), a core domain term this chatbox must keep matching.
            "gi", "sao", "nao", "dau", "khi", "bao", "nhieu", "the", "vay", "a", "nhe", "nhi", "u", "oi", "ma",
            "khong", "biet", "hieu", "hoi", "nghi", "neu", "thi", "nen",
            "tim", "kiem", "mua", "ban", "gia", "re", "dat", "coi", "xem", "hien"
        };

        private readonly ISearchService _searchService;
        private readonly AiChatOptions _options;
        private readonly ILogger<AiKnowledgeService> _logger;

        public AiKnowledgeService(
            ISearchService searchService,
            IOptions<AiChatOptions> options,
            ILogger<AiKnowledgeService> logger)
        {
            _searchService = searchService;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<IReadOnlyList<AiKnowledgeItem>> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            var phrases = BuildSearchPhrases(query);
            if (phrases.Count == 0)
            {
                return Array.Empty<AiKnowledgeItem>();
            }

            var take = Math.Clamp(_options.MaxContextItems, 1, 10);

            try
            {
                var result = await _searchService.SearchByPhrasesAsync(phrases, take, cancellationToken);

                return result.Items.Select(MapToKnowledgeItem)
                    .Where(x => !string.IsNullOrWhiteSpace(x.Summary) || !string.IsNullOrWhiteSpace(x.Title))
                    .ToList();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "AI chat full-text search failed for query: {Query}", query);
                return Array.Empty<AiKnowledgeItem>();
            }
        }

        public async Task<IReadOnlyList<AiKnowledgeItem>> BrowseByTypeAsync(IReadOnlyList<string> typeNames, CancellationToken cancellationToken = default)
        {
            if (typeNames == null || typeNames.Count == 0)
            {
                return Array.Empty<AiKnowledgeItem>();
            }

            var take = Math.Clamp(_options.MaxContextItems, 1, 10);

            try
            {
                var result = await _searchService.GetRecentByTypeAsync(typeNames, take, cancellationToken);

                return result.Items.Select(MapToKnowledgeItem)
                    .Where(x => !string.IsNullOrWhiteSpace(x.Summary) || !string.IsNullOrWhiteSpace(x.Title))
                    .ToList();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "AI chat browse-by-type failed for types: {Types}", string.Join(", ", typeNames));
                return Array.Empty<AiKnowledgeItem>();
            }
        }

        private static AiKnowledgeItem MapToKnowledgeItem(SearchIndexContent x) => new()
        {
            SourceType = string.IsNullOrWhiteSpace(x.TypeName) ? "Noi dung" : x.TypeName!,
            Title = CleanText(x.Title, 160, "Noi dung"),
            Url = NormalizeUrl(x.URL),
            Summary = BuildSummary(x.Description, x.Contents)
        };

        /// <summary>
        /// Description usually repeats Contents' own lead paragraph, so concatenating them (the
        /// old behavior) showed the same sentence twice. Pick one: Description if it's
        /// substantial, else the first sentence of Contents.
        /// </summary>
        private static string BuildSummary(string? description, string? contents, int maxLength = 200)
        {
            var desc = CleanText(description, int.MaxValue);
            var source = desc.Length >= 20 ? desc : CleanText(contents, int.MaxValue);
            return TruncateAtSentence(source, maxLength);
        }

        private static string TruncateAtSentence(string value, int maxLength)
        {
            if (value.Length <= maxLength)
            {
                return value;
            }

            var window = value[..Math.Min(value.Length, maxLength + 100)];
            var cut = window.IndexOf(". ", StringComparison.Ordinal);
            if (cut > 0 && cut <= maxLength)
            {
                return window[..(cut + 1)];
            }

            return value[..maxLength].TrimEnd() + "...";
        }

        /// <summary>
        /// Pairs adjacent significant words (non-overlapping) into phrases for OR-combined
        /// CONTAINSTABLE search — see the "why pairing" note on MaxSearchTerms above. A pair is
        /// kept unless BOTH of its words are filler; a leftover final word (odd count) is kept
        /// alone if it isn't filler. Falls back to the whole cleaned question if nothing
        /// survives filtering (e.g. a 1-2 word query).
        /// </summary>
        private static List<string> BuildSearchPhrases(string query)
        {
            var cleaned = CleanText(query, 200).Trim();
            if (cleaned.Length < 2)
            {
                return new List<string>();
            }

            var words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var noAccentWords = RemoveVietnameseMarks(cleaned).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var wordCount = Math.Min(words.Length, noAccentWords.Length);

            bool IsFiller(string plain) => plain.Length < 2 || StopWords.Contains(plain);

            var phrases = new List<string>();
            var i = 0;
            for (; i + 1 < wordCount; i += 2)
            {
                if (IsFiller(noAccentWords[i]) && IsFiller(noAccentWords[i + 1]))
                {
                    continue;
                }

                phrases.Add($"{words[i]} {words[i + 1]}");
            }

            if (i < wordCount && !IsFiller(noAccentWords[i]))
            {
                phrases.Add(words[i]);
            }

            if (phrases.Count == 0)
            {
                return new List<string> { cleaned };
            }

            if (phrases.Count > MaxSearchTerms)
            {
                phrases = phrases
                    .OrderByDescending(p => p.Length)
                    .Take(MaxSearchTerms)
                    .ToList();
            }

            return phrases;
        }

        private static string RemoveVietnameseMarks(string value)
        {
            var chars = value.Trim().ToLowerInvariant()
                .Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray();

            return new string(chars)
                .Normalize(System.Text.NormalizationForm.FormC)
                .Replace('đ', 'd');
        }

        private static string CleanText(string? value, int maxLength, string fallback = "")
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            var withoutHtml = Regex.Replace(value, "<.*?>", " ");
            var decoded = System.Net.WebUtility.HtmlDecode(withoutHtml);
            var compact = Regex.Replace(decoded, "\\s+", " ").Trim();
            return compact.Length <= maxLength ? compact : compact[..maxLength].TrimEnd() + "...";
        }

        private static string? NormalizeUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            var normalized = url.StartsWith("http", StringComparison.OrdinalIgnoreCase) || url.StartsWith("/")
                ? url
                : "/" + url.TrimStart('~', '/');

            return normalized.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                ? normalized[..^5]
                : normalized;
        }
    }
}
