using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TechExchangeApp.Configuration;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Enums;
using TechExchangeApp.Models;

namespace TechExchangeApp.Services
{
    public interface IAiChatService
    {
        Task<AiChatMessageResponse> ReplyAsync(AiChatMessageRequest request, int? userId, IReadOnlyList<int>? viewedProductIds = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AiChatHistoryItem>> GetHistoryAsync(string sessionKey, CancellationToken cancellationToken = default);
    }

    /// <summary>Sản phẩm đã xem trong phiên làm việc, mới nhất đứng đầu (HSMT C.10).</summary>
    public static class AiViewedProducts
    {
        private const string SessionKey = "AiViewedProductIds";
        private const int MaxItems = 10;

        public static void Remember(ISession session, int productId)
        {
            var ids = Read(session).Where(id => id != productId).Prepend(productId).Take(MaxItems);
            session.SetString(SessionKey, string.Join(',', ids));
        }

        public static IReadOnlyList<int> Read(ISession session)
        {
            var raw = session.GetString(SessionKey);
            if (string.IsNullOrEmpty(raw)) return Array.Empty<int>();
            return raw.Split(',').Select(x => int.TryParse(x, out var id) ? id : 0).Where(id => id > 0).ToList();
        }
    }

    public class AiChatService : IAiChatService
    {
        private static readonly Dictionary<string, string> TypeEmojis = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Công nghệ"] = "🔬",
            ["Thiết bị"] = "📦",
            ["Tài sản trí tuệ"] = "💡",
            ["Nhà cung ứng"] = "🏢",
            ["Chuyên gia"] = "👨‍🔬",
            ["OCOP"] = "🏅",
            ["Tin bài"] = "📰"
        };

        private readonly IAiKnowledgeService _knowledgeService;
        private readonly IOpenAiClientService _openAiClient;
        private readonly AppDbContext _context;
        private readonly AiChatOptions _options;
        private readonly ILogger<AiChatService> _logger;

        public AiChatService(
            IAiKnowledgeService knowledgeService,
            IOpenAiClientService openAiClient,
            AppDbContext context,
            IOptions<AiChatOptions> options,
            ILogger<AiChatService> logger)
        {
            _knowledgeService = knowledgeService;
            _openAiClient = openAiClient;
            _context = context;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<AiChatMessageResponse> ReplyAsync(AiChatMessageRequest request, int? userId, IReadOnlyList<int>? viewedProductIds = null, CancellationToken cancellationToken = default)
        {
            var sessionKey = string.IsNullOrWhiteSpace(request.SessionKey)
                ? Guid.NewGuid().ToString("N")
                : request.SessionKey.Trim();

            var question = request.Message.Trim();

            // Quick-action buttons route on the stable ActionId, never on label text — see
            // AiQuickActions. An unrecognized/stale id (e.g. an old cached frontend) falls
            // through to the normal free-text flow below instead of erroring.
            var action = AiQuickActions.FindById(request.ActionId);
            if (action != null)
            {
                var actionResponse = action.Kind == AiQuickActionKind.Personal
                    ? await BuildPersonalResponseAsync(sessionKey, userId, viewedProductIds, cancellationToken)
                    : action.Kind == AiQuickActionKind.Browse
                    ? await BuildBrowseResponseAsync(action, sessionKey, cancellationToken)
                    : new AiChatMessageResponse
                    {
                        Success = true,
                        SessionKey = sessionKey,
                        Mode = "action",
                        NeedsContactInfo = action.NeedsContactInfo,
                        Message = action.ReplyText!
                    };

                await PersistExchangeAsync(sessionKey, userId, question, actionResponse.Message, cancellationToken);
                return actionResponse;
            }

            AiChatMessageResponse response;

            if (!_options.IsEnabled)
            {
                response = new AiChatMessageResponse
                {
                    Success = true,
                    SessionKey = sessionKey,
                    NeedsContactInfo = true,
                    Message = "Hệ thống đang cập nhật dữ liệu. Anh/chị vui lòng để lại thông tin, bộ phận phụ trách sẽ liên hệ hỗ trợ."
                };
                return response;
            }

            var cannedResponse = TryGetCannedResponse(question);

            if (!string.IsNullOrWhiteSpace(cannedResponse))
            {
                response = new AiChatMessageResponse
                {
                    Success = true,
                    SessionKey = sessionKey,
                    Mode = "search",
                    NeedsContactInfo = false,
                    Message = cannedResponse
                };
            }
            else if (IsGenericSuggestionQuestion(question))
            {
                response = await BuildPersonalResponseAsync(sessionKey, userId, viewedProductIds, cancellationToken);
            }
            else
            {
                var sources = await _knowledgeService.SearchAsync(question, cancellationToken);
                string? localHeader = null;
                string? interestText = null;

                // Hỏi có ý xin gợi ý, hoặc không tìm thấy theo từ khóa: bổ sung theo sản phẩm đã xem, lĩnh vực
                if (sources.Count == 0 || HasSuggestionIntent(question))
                {
                    var interest = await LoadUserInterestAsync(userId, viewedProductIds, cancellationToken);
                    if (!interest.IsEmpty)
                    {
                        var take = Math.Clamp(_options.MaxContextItems, 1, 10);
                        var shown = new HashSet<string>(sources.Select(s => s.Url));
                        var extra = (await SearchByInterestAsync(interest, cancellationToken)).Where(s => shown.Add(s.Url));
                        interestText = interest.Describe();
                        if (sources.Count == 0)
                            localHeader = $"Tôi chưa tìm thấy đúng yêu cầu này. {interestText}, tôi gợi ý:";
                        sources = sources.Concat(extra).Take(take).ToList();
                    }
                }

                string? aiText = null;
                if (_options.UseOpenAiResponses)
                {
                    var prompt = BuildUserPrompt(question, sources, interestText);
                    aiText = await _openAiClient.CreateResponseAsync(_options.SystemPrompt, prompt, cancellationToken);
                }

                response = new AiChatMessageResponse
                {
                    Success = true,
                    SessionKey = sessionKey,
                    Sources = sources,
                    Mode = "search",
                    NeedsContactInfo = ShouldAskForContact(question, sources),
                    Message = string.IsNullOrWhiteSpace(aiText)
                        ? (localHeader != null && sources.Count > 0
                            ? BuildSourceListMessage(localHeader, sources, "Anh/chị muốn được tư vấn trực tiếp thì có thể để lại thông tin liên hệ.")
                            : BuildLocalResponse(question, sources))
                        : aiText.Trim()
                };
            }

            await PersistExchangeAsync(sessionKey, userId, question, response.Message, cancellationToken);

            return response;
        }

        public async Task<IReadOnlyList<AiChatHistoryItem>> GetHistoryAsync(string sessionKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sessionKey))
            {
                return Array.Empty<AiChatHistoryItem>();
            }

            var session = await _context.AiChatSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SessionKey == sessionKey, cancellationToken);

            if (session == null)
            {
                return Array.Empty<AiChatHistoryItem>();
            }

            return await _context.AiChatMessages
                .AsNoTracking()
                .Where(m => m.SessionId == session.Id)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new AiChatHistoryItem { Role = m.Role, Content = m.Content, CreatedAt = m.CreatedAt })
                .ToListAsync(cancellationToken);
        }

        private async Task<AiChatMessageResponse> BuildBrowseResponseAsync(AiQuickAction action, string sessionKey, CancellationToken cancellationToken)
        {
            var sources = await _knowledgeService.BrowseByTypeAsync(action.TypeNames!, cancellationToken);

            var message = sources.Count == 0
                ? "Hiện chưa có dữ liệu phù hợp. Anh/chị có thể để lại thông tin để trung tâm hỗ trợ tìm kiếm."
                : BuildSourceListMessage(
                    action.ResultsHeader ?? "Đây là một số kết quả mới nhất trên website:",
                    sources,
                    "Anh/chị muốn được tư vấn trực tiếp thì có thể để lại thông tin liên hệ.");

            return new AiChatMessageResponse
            {
                Success = true,
                SessionKey = sessionKey,
                Sources = sources,
                Mode = "browse",
                NeedsContactInfo = sources.Count == 0,
                Message = message
            };
        }

        // ── Gợi ý theo sản phẩm đã xem và lĩnh vực của tài khoản (HSMT C.10) ──────────────

        private sealed record UserInterest(IReadOnlyList<string> ViewedNames, IReadOnlyList<string> Fields)
        {
            public bool IsEmpty => ViewedNames.Count == 0 && Fields.Count == 0;

            public string Describe()
            {
                var parts = new List<string>();
                if (ViewedNames.Count > 0)
                    parts.Add("sản phẩm anh/chị đã xem gần đây (" + string.Join("; ", ViewedNames.Take(2).Select(n => n.Length > 60 ? n[..60].TrimEnd() + "..." : n)) + ")");
                if (Fields.Count > 0)
                    parts.Add("lĩnh vực " + string.Join(", ", Fields.Take(2)));
                return "Dựa trên " + string.Join(" và ", parts);
            }
        }

        private async Task<AiChatMessageResponse> BuildPersonalResponseAsync(string sessionKey, int? userId, IReadOnlyList<int>? viewedProductIds, CancellationToken cancellationToken)
        {
            var interest = await LoadUserInterestAsync(userId, viewedProductIds, cancellationToken);
            var sources = interest.IsEmpty
                ? (IReadOnlyList<AiKnowledgeItem>)Array.Empty<AiKnowledgeItem>()
                : (await SearchByInterestAsync(interest, cancellationToken)).Take(Math.Clamp(_options.MaxContextItems, 1, 10)).ToList();

            var message = sources.Count > 0
                ? BuildSourceListMessage($"{interest.Describe()}, tôi gợi ý:", sources, "Anh/chị muốn được tư vấn trực tiếp thì có thể để lại thông tin liên hệ.")
                : "Tôi chưa có đủ thông tin để gợi ý. Anh/chị hãy xem một vài sản phẩm hoặc cho tôi biết lĩnh vực, nhu cầu công nghệ đang quan tâm.";

            return new AiChatMessageResponse
            {
                Success = true,
                SessionKey = sessionKey,
                Sources = sources,
                Mode = "browse",
                NeedsContactInfo = false,
                Message = message
            };
        }

        private async Task<UserInterest> LoadUserInterestAsync(int? userId, IReadOnlyList<int>? viewedProductIds, CancellationToken cancellationToken)
        {
            const int maxViewed = 3;
            try
            {
                var candidates = (viewedProductIds ?? Array.Empty<int>()).ToList();
                var fieldIds = new List<int>();

                if (userId.HasValue)
                {
                    var uid = userId.Value.ToString();
                    candidates.AddRange(await _context.EntityActionLogs.AsNoTracking()
                        .Where(l => l.UserId == uid && l.EntityType == EntityTypes.SanPhamCNTB && l.ActionType == ActionTypes.View)
                        .OrderByDescending(l => l.Id)
                        .Select(l => l.EntityId)
                        .Take(30)
                        .ToListAsync(cancellationToken));

                    // Ngành nghề khai trong hồ sơ nhà cung ứng của tài khoản, dạng ";9;12;"
                    var profileFields = await _context.NhaCungUngs.AsNoTracking()
                        .Where(n => n.UserId == userId.Value && n.LinhVucId != null)
                        .Select(n => n.LinhVucId!)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (!string.IsNullOrEmpty(profileFields))
                        fieldIds.AddRange(profileFields.Split(';', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => int.TryParse(x, out var id) ? id : 0).Where(id => id > 0));
                }

                var ids = candidates.Distinct().Take(10).ToList();
                var names = await _context.SanPhamCNTBs.AsNoTracking()
                    .Where(p => ids.Contains(p.ID) && p.Name != null)
                    .Select(p => new { p.ID, p.Name })
                    .ToListAsync(cancellationToken);
                var viewed = ids.Join(names, id => id, p => p.ID, (_, p) => p).Take(maxViewed).ToList();

                var viewedIds = viewed.Select(p => p.ID).ToList();
                fieldIds.AddRange(await _context.SanPhamCNTBCategories.AsNoTracking()
                    .Where(c => viewedIds.Contains(c.SanPhamCNTBId))
                    .Select(c => c.CatId)
                    .ToListAsync(cancellationToken));

                var distinctFieldIds = fieldIds.Distinct().ToList();
                var fieldTitles = await _context.Categories.AsNoTracking()
                    .Where(c => distinctFieldIds.Contains(c.CatId) && c.Title != null)
                    .Select(c => new { c.CatId, c.Title })
                    .ToListAsync(cancellationToken);
                var fields = distinctFieldIds.Join(fieldTitles, id => id, c => c.CatId, (_, c) => c.Title!).Take(2).ToList();

                return new UserInterest(viewed.Select(p => p.Name!).ToList(), fields);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Cannot load AI chat interest for user {UserId}.", userId);
                return new UserInterest(Array.Empty<string>(), Array.Empty<string>());
            }
        }

        /// <summary>Tìm theo từng tên sản phẩm, từng lĩnh vực; bỏ chính các sản phẩm đã xem.</summary>
        private async Task<List<AiKnowledgeItem>> SearchByInterestAsync(UserInterest interest, CancellationToken cancellationToken)
        {
            var viewed = new HashSet<string>(interest.ViewedNames.Select(NormalizeIntentText));
            var seen = new HashSet<string>();
            var result = new List<AiKnowledgeItem>();

            foreach (var term in interest.ViewedNames.Concat(interest.Fields))
            {
                var items = await _knowledgeService.SearchAsync(term, cancellationToken);
                result.AddRange(items
                    .Where(x => !viewed.Contains(NormalizeIntentText(x.Title)) && seen.Add(x.Url))
                    .Take(3));
            }

            return result;
        }

        private static bool HasSuggestionIntent(string question)
        {
            var normalized = NormalizeIntentText(question);
            return normalized.Contains("goi y") || normalized.Contains("de xuat") || normalized.Contains("phu hop");
        }

        /// <summary>Câu xin gợi ý chung, không nêu nhu cầu cụ thể, ví dụ "có sản phẩm nào phù hợp với tôi không".</summary>
        private static bool IsGenericSuggestionQuestion(string question)
        {
            if (!HasSuggestionIntent(question)) return false;

            var generic = new HashSet<string>
            {
                "goi", "y", "de", "xuat", "phu", "hop", "cho", "toi", "minh", "em", "anh", "chi", "voi", "giup", "hay", "xin",
                "co", "khong", "nao", "gi", "nen", "chon", "san", "pham", "cong", "nghe", "giai", "phap", "thiet", "bi",
                "cac", "mot", "vai", "so", "nhung", "duoc", "la", "ban", "a", "nhe", "nhi", "dang", "can", "quan", "tam"
            };
            return NormalizeIntentText(question)
                .Split(new[] { ' ', '?', '.', ',', '!', ':', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .All(generic.Contains);
        }

        private async Task PersistExchangeAsync(string sessionKey, int? userId, string userMessage, string botMessage, CancellationToken cancellationToken)
        {
            try
            {
                var session = await _context.AiChatSessions
                    .FirstOrDefaultAsync(s => s.SessionKey == sessionKey, cancellationToken);

                if (session == null)
                {
                    session = new AiChatSession
                    {
                        SessionKey = sessionKey,
                        UserId = userId,
                        CreatedAt = DateTime.Now
                    };
                    _context.AiChatSessions.Add(session);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                else if (userId.HasValue && session.UserId != userId)
                {
                    // Anonymous session becomes identified once the visitor logs in.
                    session.UserId = userId;
                }

                session.LastMessageAt = DateTime.Now;
                _context.AiChatMessages.Add(new AiChatMessage { SessionId = session.Id, Role = "user", Content = userMessage, CreatedAt = DateTime.Now });
                _context.AiChatMessages.Add(new AiChatMessage { SessionId = session.Id, Role = "bot", Content = botMessage, CreatedAt = DateTime.Now });
                await _context.SaveChangesAsync(cancellationToken);

                var totalCount = await _context.AiChatMessages.CountAsync(m => m.SessionId == session.Id, cancellationToken);
                if (totalCount > _options.MaxMessagesPerSession)
                {
                    var overflow = totalCount - _options.MaxMessagesPerSession;
                    var oldest = await _context.AiChatMessages
                        .Where(m => m.SessionId == session.Id)
                        .OrderBy(m => m.CreatedAt)
                        .Take(overflow)
                        .ToListAsync(cancellationToken);
                    _context.AiChatMessages.RemoveRange(oldest);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Cannot persist AI chat history for session {SessionKey}.", sessionKey);
            }
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
                return "Xin chào anh/chị. Tôi có thể hỗ trợ tìm công nghệ, sản phẩm, nhà cung ứng, chuyên gia tư vấn hoặc ghi nhận yêu cầu liên hệ. Anh/chị muốn tìm thông tin nào?";
            }

            if (normalized is "cam on" or "thanks" or "thank you")
            {
                return "Rất vui được hỗ trợ anh/chị. Nếu cần thêm thông tin về công nghệ, sản phẩm hoặc dịch vụ tư vấn, anh/chị cứ nhắn tin tại đây.";
            }

            if (normalized.Contains("lien he") || normalized.Contains("so dien thoai") || normalized.Contains("dia chi"))
            {
                return "Anh/chị có thể để lại họ tên, số điện thoại hoặc email trong khung chat. Trung tâm sẽ ghi nhận và liên hệ hỗ trợ trong thời gian sớm nhất.";
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

        private static string BuildUserPrompt(string question, IReadOnlyList<AiKnowledgeItem> sources, string? interestText = null)
        {
            if (!string.IsNullOrEmpty(interestText))
                question += Environment.NewLine + $"(Thông tin thêm về người hỏi: {interestText}.)";

            var context = sources.Count == 0
                ? "Không tìm thấy dữ liệu phù hợp trong website."
                : string.Join(Environment.NewLine, sources.Select((x, i) =>
                    $"{i + 1}. [{x.SourceType}] {x.Title}\nURL: {x.Url}\nTóm tắt: {x.Summary}"));

            return $"""
            Câu hỏi của người dùng:
            {question}

            Dữ liệu website liên quan:
            {context}

            Hãy trả lời ngắn gọn, đúng tiếng Việt, ưu tiên đưa link nguồn nếu có. Nếu dữ liệu chưa đủ, không tự bịa thông tin; hãy đề nghị người dùng để lại tên, số điện thoại/email để trung tâm tư vấn.
            """;
        }

        private static string BuildLocalResponse(string question, IReadOnlyList<AiKnowledgeItem> sources)
        {
            if (sources.Count == 0)
            {
                return "Tôi chưa tìm thấy dữ liệu phù hợp trên website cho yêu cầu này. Anh/chị có thể để lại họ tên, số điện thoại hoặc email để trung tâm liên hệ tư vấn chi tiết.";
            }

            return BuildSourceListMessage(
                "Tôi tìm thấy một số thông tin liên quan trên website:",
                sources,
                "Anh/chị muốn được tư vấn trực tiếp thì có thể để lại thông tin liên hệ.");
        }

        /// <summary>
        /// Shared bullet-list renderer for both search results and quick-action browse results,
        /// so the two produce visually identical chat messages. Each source gets a per-type
        /// emoji and its own paragraph (blank line between entries) instead of the old
        /// single-line-per-source wall of text.
        /// </summary>
        private static string BuildSourceListMessage(string headerText, IReadOnlyList<AiKnowledgeItem> sources, string footerText)
        {
            var bulletsBlock = string.Join(
                Environment.NewLine + Environment.NewLine,
                sources.Take(4).Select(FormatSourceBullet));

            return string.Join(Environment.NewLine + Environment.NewLine, new[] { headerText, bulletsBlock, footerText });
        }

        private static string FormatSourceBullet(AiKnowledgeItem x)
        {
            var emoji = TypeEmojis.TryGetValue(x.SourceType, out var e) ? e : "📄";
            var lines = new List<string> { $"{emoji} {x.Title}" };

            if (!string.IsNullOrWhiteSpace(x.Summary))
            {
                lines.Add(x.Summary);
            }

            // Raw URLs are no longer inlined here — the client renders Sources as
            // clickable result rows instead (see ai-chatbox.js renderResultList).
            return string.Join(Environment.NewLine, lines);
        }

        private static bool ShouldAskForContact(string question, IReadOnlyList<AiKnowledgeItem> sources)
        {
            var normalized = NormalizeIntentText(question);
            return sources.Count == 0
                || normalized.Contains("tu van")
                || normalized.Contains("lien he")
                || normalized.Contains("bao gia")
                || normalized.Contains("ho tro");
        }
    }
}
