using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Interfaces;

namespace TechExchangeApp.Services
{
    public interface IProductWarningScanner
    {
        /// <summary>Quét toàn bộ sản phẩm tiếng Việt, cập nhật bảng ProductWarnings. Trả về số cảnh báo đang mở.</summary>
        Task<ProductWarningScanResult> ScanAsync(CancellationToken ct = default);
    }

    public record ProductWarningScanResult(int DuplicateName, int DuplicateDescription, int Stale, int Opened, int AutoClosed);

    /// <summary>
    /// Cảnh báo sản phẩm trùng lặp, quá hạn cập nhật (HSMT A.8.2).
    /// - Trùng tên: tên chuẩn hóa (bỏ dấu, chữ thường, bỏ ký tự đặc biệt) giống nhau.
    /// - Trùng mô tả: mô tả ngắn chuẩn hóa (≥ 40 ký tự) giống nhau.
    /// - Quá hạn: đang xuất bản và không cập nhật quá PRODUCT_STALE_MONTHS tháng (mặc định 12).
    /// Cảnh báo quản trị đã đánh dấu "đã xử lý" không bị mở lại; cảnh báo không còn đúng ở lần quét sau tự đóng.
    /// </summary>
    public class ProductWarningScanner : IProductWarningScanner
    {
        private const int MinNameLength = 8;
        private const int MinDescriptionLength = 40;

        private readonly AppDbContext _db;
        private readonly ISystemParameterService _params;
        private readonly ILogger<ProductWarningScanner> _logger;

        public ProductWarningScanner(AppDbContext db, ISystemParameterService @params, ILogger<ProductWarningScanner> logger)
        {
            _db = db;
            _params = @params;
            _logger = logger;
        }

        public async Task<ProductWarningScanResult> ScanAsync(CancellationToken ct = default)
        {
            var staleMonths = await _params.GetIntAsync("PRODUCT_STALE_MONTHS", 12);
            if (staleMonths < 1) staleMonths = 12;
            var staleBefore = DateTime.Now.AddMonths(-staleMonths);

            var products = await _db.SanPhamCNTBs.AsNoTracking()
                .Where(p => p.LanguageId == 1 && p.StatusId != null)
                .Select(p => new ProductRow(p.ID, p.Name, p.MoTaNgan, p.StatusId, p.Modified, p.PublishedDate, p.Created))
                .ToListAsync(ct);

            // (SanPhamId, Type) -> (GroupKey, Detail)
            var found = new Dictionary<(int, byte), (string? Key, string Detail)>();

            void AddDuplicates(byte type, Func<ProductRow, string?> keyOf, string label)
            {
                var groups = products
                    .Select(p => new { p.ID, Key = keyOf(p) })
                    .Where(x => !string.IsNullOrEmpty(x.Key))
                    .GroupBy(x => x.Key!)
                    .Where(g => g.Count() > 1);
                foreach (var g in groups)
                {
                    var ids = g.Select(x => x.ID).OrderBy(x => x).ToList();
                    foreach (var id in ids)
                    {
                        var others = ids.Where(x => x != id).Take(10).Select(x => "#" + x);
                        var key = g.Key.Length > 450 ? g.Key[..450] : g.Key;
                        found[(id, type)] = (key, $"{label} với: {string.Join(", ", others)}{(ids.Count > 11 ? "…" : "")}");
                    }
                }
            }

            AddDuplicates(ProductWarningTypes.DuplicateName, p => NormalizeText(p.Name, MinNameLength), "Trùng tên");
            AddDuplicates(ProductWarningTypes.DuplicateDescription, p => NormalizeText(StripHtml(p.MoTaNgan), MinDescriptionLength), "Trùng mô tả ngắn");

            foreach (var p in products.Where(p => p.StatusId == 3))
            {
                var last = p.Modified ?? p.PublishedDate ?? p.Created;
                if (last.HasValue && last.Value < staleBefore)
                    found[(p.ID, ProductWarningTypes.Stale)] = (null, $"Chưa cập nhật từ {last.Value:dd/MM/yyyy} (quá {staleMonths} tháng)");
            }

            var now = DateTime.Now;
            var existing = await _db.ProductWarnings.Where(w => w.Status != ProductWarningStatuses.AutoClosed).ToListAsync(ct);
            var byKey = existing.GroupBy(w => (w.SanPhamId, w.WarningType)).ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Id).First());

            int opened = 0, closed = 0;
            foreach (var ((id, type), info) in found)
            {
                if (byKey.TryGetValue((id, type), out var w))
                {
                    w.LastSeenAt = now;
                    if (w.Status == ProductWarningStatuses.Open) { w.GroupKey = info.Key; w.Detail = info.Detail; }
                }
                else
                {
                    _db.ProductWarnings.Add(new ProductWarning
                    {
                        SanPhamId = id, WarningType = type, GroupKey = info.Key, Detail = info.Detail,
                        DetectedAt = now, LastSeenAt = now, Status = ProductWarningStatuses.Open
                    });
                    opened++;
                }
            }
            foreach (var w in existing.Where(w => w.Status == ProductWarningStatuses.Open && !found.ContainsKey((w.SanPhamId, w.WarningType))))
            {
                w.Status = ProductWarningStatuses.AutoClosed;
                w.ResolvedAt = now;
                w.ResolvedBy = "Tự động";
                closed++;
            }
            await _db.SaveChangesAsync(ct);

            var result = new ProductWarningScanResult(
                found.Keys.Count(k => k.Item2 == ProductWarningTypes.DuplicateName),
                found.Keys.Count(k => k.Item2 == ProductWarningTypes.DuplicateDescription),
                found.Keys.Count(k => k.Item2 == ProductWarningTypes.Stale),
                opened, closed);
            _logger.LogInformation("[ProductWarning] Quét xong: {Result}", result);
            return result;
        }

        private record ProductRow(int ID, string? Name, string? MoTaNgan, int? StatusId, DateTime? Modified, DateTime? PublishedDate, DateTime? Created);

        private static string StripHtml(string? s)
            => string.IsNullOrEmpty(s) ? "" : WebUtility.HtmlDecode(Regex.Replace(s, "<.*?>", " "));

        /// <summary>Bỏ dấu, chữ thường, chỉ giữ chữ/số, gộp khoảng trắng. Ngắn hơn minLength thì bỏ qua.</summary>
        public static string? NormalizeText(string? s, int minLength)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var d = s.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(d.Length);
            foreach (var c in d)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat == UnicodeCategory.NonSpacingMark) continue;
                sb.Append(char.IsLetterOrDigit(c) ? c : ' ');
            }
            var n = Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
            return n.Length >= minLength ? n : null;
        }
    }
}
