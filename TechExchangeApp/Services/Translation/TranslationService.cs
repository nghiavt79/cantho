using HtmlAgilityPack;

namespace TechExchangeApp.Services.Translation
{
    /// <summary>
    /// Dịch cấp cao dùng ITranslator. HTML: parse bằng HtmlAgilityPack, CHỈ dịch text node
    /// (bỏ script/style), thẻ/class/thuộc tính không đi qua model → không vỡ giao diện.
    /// Nếu dịch lỗi/kết quả rỗng → trả null để tầng gọi giữ bản VI (fallback).
    /// </summary>
    public sealed class TranslationService : ITranslationService
    {
        private readonly ITranslator _translator;
        public string ProviderName => _translator.Name;

        public TranslationService(ITranslator translator) => _translator = translator;

        public async Task<string?> TranslatePlainAsync(string? vi, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(vi)) return vi;
            var r = await _translator.TranslateAsync(new[] { vi }, ct);
            return (r.Count == 1 && !string.IsNullOrWhiteSpace(r[0])) ? r[0] : null;
        }

        public async Task<string?> TranslateHtmlAsync(string? viHtml, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(viHtml)) return viHtml;

            var doc = new HtmlDocument();
            doc.LoadHtml(viHtml);

            // Gom text node cần dịch (bỏ trắng, bỏ trong script/style)
            var textNodes = doc.DocumentNode.SelectNodes("//text()")?
                .Where(n => !string.IsNullOrWhiteSpace(n.InnerText)
                            && !IsInSkip(n))
                .ToList() ?? new List<HtmlNode>();

            if (textNodes.Count == 0) return viHtml; // không có chữ để dịch

            // Dedup theo nội dung thật (đã giải mã entity)
            var distinct = new List<string>();
            var indexOf = new Dictionary<string, int>();
            foreach (var n in textNodes)
            {
                var raw = HtmlEntity.DeEntitize(n.InnerText);
                if (!indexOf.ContainsKey(raw))
                {
                    indexOf[raw] = distinct.Count;
                    distinct.Add(raw);
                }
            }

            IReadOnlyList<string> translated;
            try { translated = await _translator.TranslateAsync(distinct, ct); }
            catch { return null; } // lỗi dịch → giữ VI ở tầng gọi

            if (translated.Count != distinct.Count) return null;

            foreach (var n in textNodes)
            {
                var raw = HtmlEntity.DeEntitize(n.InnerText);
                var en = translated[indexOf[raw]];
                if (string.IsNullOrEmpty(en)) continue;
                n.InnerHtml = HtmlEntity.Entitize(en, useNames: false); // encode &,<,> → không vỡ markup
            }

            return doc.DocumentNode.OuterHtml;
        }

        private static bool IsInSkip(HtmlNode n)
        {
            for (var p = n.ParentNode; p != null; p = p.ParentNode)
            {
                var name = p.Name?.ToLowerInvariant();
                if (name == "script" || name == "style") return true;
            }
            return false;
        }
    }
}
