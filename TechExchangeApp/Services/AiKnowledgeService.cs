using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TechExchangeApp.Configuration;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Models;

namespace TechExchangeApp.Services
{
    public interface IAiKnowledgeService
    {
        Task<IReadOnlyList<AiKnowledgeItem>> SearchAsync(string query, CancellationToken cancellationToken = default);
    }

    public class AiKnowledgeService : IAiKnowledgeService
    {
        private readonly AppDbContext _context;
        private readonly AiChatOptions _options;
        private readonly ILogger<AiKnowledgeService> _logger;

        public AiKnowledgeService(
            AppDbContext context,
            IOptions<AiChatOptions> options,
            ILogger<AiKnowledgeService> logger)
        {
            _context = context;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<IReadOnlyList<AiKnowledgeItem>> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            var keyword = NormalizeKeyword(query);
            var keywordNoAccent = RemoveVietnameseMarks(keyword);
            if (keyword.Length < 2)
            {
                return Array.Empty<AiKnowledgeItem>();
            }

            var take = Math.Clamp(_options.MaxContextItems, 1, 10);
            var results = new List<AiKnowledgeItem>();

            try
            {
                var knowledgeItems = await _context.AiKnowledgeDocuments
                    .AsNoTracking()
                    .Where(x => x.IsActive && (
                        EF.Functions.Collate(x.Title, "Vietnamese_CI_AI").Contains(keyword) ||
                        EF.Functions.Collate(x.ContentText, "Vietnamese_CI_AI").Contains(keyword) ||
                        EF.Functions.Collate(x.Title, "Vietnamese_CI_AI").Contains(keywordNoAccent) ||
                        EF.Functions.Collate(x.ContentText, "Vietnamese_CI_AI").Contains(keywordNoAccent) ||
                        (x.SourceSlug != null && EF.Functions.Collate(x.SourceSlug, "Vietnamese_CI_AI").Contains(keywordNoAccent))))
                    .OrderByDescending(x => x.LastSyncedAt)
                    .Take(take)
                    .ToListAsync(cancellationToken);

                results.AddRange(knowledgeItems.Select(x => new AiKnowledgeItem
                {
                    SourceType = x.SourceType,
                    Title = CleanText(x.Title, 160, "Noi dung"),
                    Url = NormalizeUrl(x.Url),
                    Summary = CleanText(x.ContentText, 700)
                }));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "AI chat knowledge document lookup failed.");
            }

            if (results.Count >= take)
            {
                return results.Take(take).ToList();
            }

            try
            {
                var indexItems = await _context.SearchIndexContents
                    .AsNoTracking()
                    .Where(x =>
                        (x.Title != null && EF.Functions.Collate(x.Title, "Vietnamese_CI_AI").Contains(keyword)) ||
                        (x.Description != null && EF.Functions.Collate(x.Description, "Vietnamese_CI_AI").Contains(keyword)) ||
                        (x.Contents != null && EF.Functions.Collate(x.Contents, "Vietnamese_CI_AI").Contains(keyword)) ||
                        (x.Title != null && EF.Functions.Collate(x.Title, "Vietnamese_CI_AI").Contains(keywordNoAccent)) ||
                        (x.Description != null && EF.Functions.Collate(x.Description, "Vietnamese_CI_AI").Contains(keywordNoAccent)) ||
                        (x.Contents != null && EF.Functions.Collate(x.Contents, "Vietnamese_CI_AI").Contains(keywordNoAccent)) ||
                        (x.RemovedUnicode != null && x.RemovedUnicode.Contains(keywordNoAccent)))
                    .OrderByDescending(x => x.Modified ?? x.IndexTime ?? x.Created)
                    .Take(take)
                    .ToListAsync(cancellationToken);

                results.AddRange(indexItems.Select(x => new AiKnowledgeItem
                {
                    SourceType = string.IsNullOrWhiteSpace(x.TypeName) ? "Noi dung" : x.TypeName!,
                    Title = CleanText(x.Title, 160, "Noi dung"),
                    Url = NormalizeUrl(x.URL),
                    Summary = CleanText($"{x.Description} {x.Contents}", 700)
                }));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "AI chat search index lookup failed.");
            }

            if (results.Count < take)
            {
                try
                {
                    var products = await _context.SanPhamCNTBs
                        .AsNoTracking()
                        .Where(x => x.StatusId == 1 && (
                            (x.Name != null && EF.Functions.Collate(x.Name, "Vietnamese_CI_AI").Contains(keyword)) ||
                            (x.MoTa != null && EF.Functions.Collate(x.MoTa, "Vietnamese_CI_AI").Contains(keyword)) ||
                            (x.MoTaNgan != null && EF.Functions.Collate(x.MoTaNgan, "Vietnamese_CI_AI").Contains(keyword)) ||
                            (x.ThongSo != null && EF.Functions.Collate(x.ThongSo, "Vietnamese_CI_AI").Contains(keyword)) ||
                            (x.Keywords != null && EF.Functions.Collate(x.Keywords, "Vietnamese_CI_AI").Contains(keyword)) ||
                            (x.Name != null && EF.Functions.Collate(x.Name, "Vietnamese_CI_AI").Contains(keywordNoAccent)) ||
                            (x.MoTa != null && EF.Functions.Collate(x.MoTa, "Vietnamese_CI_AI").Contains(keywordNoAccent)) ||
                            (x.MoTaNgan != null && EF.Functions.Collate(x.MoTaNgan, "Vietnamese_CI_AI").Contains(keywordNoAccent)) ||
                            (x.ThongSo != null && EF.Functions.Collate(x.ThongSo, "Vietnamese_CI_AI").Contains(keywordNoAccent)) ||
                            (x.Keywords != null && EF.Functions.Collate(x.Keywords, "Vietnamese_CI_AI").Contains(keywordNoAccent))))
                        .OrderByDescending(x => x.PublishedDate ?? x.Modified ?? x.Created)
                        .Take(take - results.Count)
                        .ToListAsync(cancellationToken);

                    results.AddRange(products.Select(x => new AiKnowledgeItem
                    {
                        SourceType = ProductTypeName(x.ProductType),
                        Title = CleanText(x.Name, 160, "San pham CNTB"),
                        Url = ProductUrl(x),
                        Summary = CleanText($"{x.MoTaNgan} {x.MoTa} {x.UuDiem} {x.ThongSo}", 700)
                    }));
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex, "AI chat product lookup failed.");
                }
            }

            if (results.Count < take)
            {
                try
                {
                    var suppliers = await _context.NhaCungUngs
                        .AsNoTracking()
                        .Where(x => x.StatusId == 1 && (
                            (x.FullName != null && EF.Functions.Collate(x.FullName, "Vietnamese_CI_AI").Contains(keyword)) ||
                            (x.ChucNangChinh != null && EF.Functions.Collate(x.ChucNangChinh, "Vietnamese_CI_AI").Contains(keyword)) ||
                            (x.DichVu != null && EF.Functions.Collate(x.DichVu, "Vietnamese_CI_AI").Contains(keyword)) ||
                            (x.SanPham != null && EF.Functions.Collate(x.SanPham, "Vietnamese_CI_AI").Contains(keyword)) ||
                            (x.Keywords != null && EF.Functions.Collate(x.Keywords, "Vietnamese_CI_AI").Contains(keyword)) ||
                            (x.FullName != null && EF.Functions.Collate(x.FullName, "Vietnamese_CI_AI").Contains(keywordNoAccent)) ||
                            (x.ChucNangChinh != null && EF.Functions.Collate(x.ChucNangChinh, "Vietnamese_CI_AI").Contains(keywordNoAccent)) ||
                            (x.DichVu != null && EF.Functions.Collate(x.DichVu, "Vietnamese_CI_AI").Contains(keywordNoAccent)) ||
                            (x.SanPham != null && EF.Functions.Collate(x.SanPham, "Vietnamese_CI_AI").Contains(keywordNoAccent)) ||
                            (x.Keywords != null && EF.Functions.Collate(x.Keywords, "Vietnamese_CI_AI").Contains(keywordNoAccent))))
                        .OrderByDescending(x => x.Modified ?? x.Created)
                        .Take(take - results.Count)
                        .ToListAsync(cancellationToken);

                    results.AddRange(suppliers.Select(x => new AiKnowledgeItem
                    {
                        SourceType = "Nha cung ung",
                        Title = CleanText(x.FullName, 160, "Nha cung ung"),
                        Url = $"/11-tim-kiem-doi-tac/{SlugOrDefault(x.QueryString, "nha-cung-ung")}-{x.CungUngId}",
                        Summary = CleanText($"{x.ChucNangChinh} {x.DichVu} {x.SanPham}", 700)
                    }));
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex, "AI chat supplier lookup failed.");
                }
            }

            if (results.Count < take)
            {
                try
                {
                    var consultants = await _context.NhaTuVans
                        .AsNoTracking()
                        .Where(x => x.StatusId == 1 && (
                            EF.Functions.Collate(x.FullName, "Vietnamese_CI_AI").Contains(keyword) ||
                            (x.DichVu != null && EF.Functions.Collate(x.DichVu, "Vietnamese_CI_AI").Contains(keyword)) ||
                            (x.KetQuaNghienCuu != null && EF.Functions.Collate(x.KetQuaNghienCuu, "Vietnamese_CI_AI").Contains(keyword)) ||
                            (x.KinhNghiem != null && EF.Functions.Collate(x.KinhNghiem, "Vietnamese_CI_AI").Contains(keyword)) ||
                            (x.Keywords != null && EF.Functions.Collate(x.Keywords, "Vietnamese_CI_AI").Contains(keyword)) ||
                            EF.Functions.Collate(x.FullName, "Vietnamese_CI_AI").Contains(keywordNoAccent) ||
                            (x.DichVu != null && EF.Functions.Collate(x.DichVu, "Vietnamese_CI_AI").Contains(keywordNoAccent)) ||
                            (x.KetQuaNghienCuu != null && EF.Functions.Collate(x.KetQuaNghienCuu, "Vietnamese_CI_AI").Contains(keywordNoAccent)) ||
                            (x.KinhNghiem != null && EF.Functions.Collate(x.KinhNghiem, "Vietnamese_CI_AI").Contains(keywordNoAccent)) ||
                            (x.Keywords != null && EF.Functions.Collate(x.Keywords, "Vietnamese_CI_AI").Contains(keywordNoAccent))))
                        .OrderByDescending(x => x.Modified ?? x.Created)
                        .Take(take - results.Count)
                        .ToListAsync(cancellationToken);

                    results.AddRange(consultants.Select(x => new AiKnowledgeItem
                    {
                        SourceType = "Chuyen gia tu van",
                        Title = CleanText(x.FullName, 160, "Chuyen gia"),
                        Url = "/chuyen-gia",
                        Summary = CleanText($"{x.DichVu} {x.KetQuaNghienCuu} {x.KinhNghiem}", 700)
                    }));
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex, "AI chat consultant lookup failed.");
                }
            }

            return results
                .Where(x => !string.IsNullOrWhiteSpace(x.Summary) || !string.IsNullOrWhiteSpace(x.Title))
                .Take(take)
                .ToList();
        }

        private static string NormalizeKeyword(string value)
        {
            var keyword = CleanText(value, 120).Trim();
            var normalized = RemoveVietnameseMarks(keyword);
            var prefixes = new[]
            {
                "tim kiem ",
                "tim ",
                "toi can ",
                "can tim ",
                "cho toi ",
                "hay tim ",
                "search "
            };

            foreach (var prefix in prefixes)
            {
                if (normalized.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return keyword[prefix.Length..].Trim();
                }
            }

            return keyword;
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

            return url.StartsWith("http", StringComparison.OrdinalIgnoreCase) || url.StartsWith("/")
                ? url
                : "/" + url.TrimStart('~', '/');
        }

        private static string ProductTypeName(int productType)
        {
            return productType switch
            {
                1 => "Cong nghe",
                2 => "Thiet bi",
                3 => "Tai san tri tue",
                _ => "San pham CNTB"
            };
        }

        private static string ProductUrl(SanPhamCNTB product)
        {
            if (!string.IsNullOrWhiteSpace(product.URL))
            {
                return NormalizeUrl(product.URL) ?? "/";
            }

            var slug = string.IsNullOrWhiteSpace(product.QueryString) ? "san-pham" : product.QueryString;
            return $"/san-pham/chi-tiet/{slug}-{product.ID}";
        }

        private static string SlugOrDefault(string? slug, string fallback)
        {
            return string.IsNullOrWhiteSpace(slug) ? fallback : slug;
        }
    }
}
