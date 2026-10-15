using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechExchangeApp.Web.Helpers
{
    /// <summary>
    /// Lọc HTML do người dùng nhập (CKEditor, mô tả, tin nhắn...) theo danh sách cho phép
    /// trước khi hiển thị — chống Stored XSS (pentest MobiFone 10/2026).
    /// Dựng lại HTML từ cây đã phân tích và mã hóa lại toàn bộ text/thuộc tính,
    /// nên không phụ thuộc vào cách viết của chuỗi gốc.
    /// </summary>
    public static class HtmlSanitizerHelper
    {
        private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "p", "br", "b", "strong", "i", "em", "u", "s", "strike", "del", "ins", "sub", "sup", "small", "mark",
            "span", "div", "font", "h1", "h2", "h3", "h4", "h5", "h6", "ul", "ol", "li", "dl", "dt", "dd",
            "blockquote", "pre", "code", "hr", "a", "img", "figure", "figcaption", "section", "article",
            "table", "thead", "tbody", "tfoot", "tr", "th", "td", "caption", "colgroup", "col", "iframe"
        };

        // Bỏ luôn cả nội dung bên trong
        private static readonly HashSet<string> DroppedTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "script", "style", "object", "embed", "applet", "form", "input", "button", "textarea", "select",
            "option", "svg", "math", "meta", "link", "base", "noscript", "template", "frame", "frameset",
            "head", "title", "xml"
        };

        private static readonly HashSet<string> VoidTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "br", "hr", "img", "col"
        };

        private static readonly HashSet<string> GlobalAttrs = new(StringComparer.OrdinalIgnoreCase)
        {
            "class", "style", "title", "align", "dir", "lang", "width", "height", "border",
            "colspan", "rowspan", "cellpadding", "cellspacing", "valign", "color", "face", "size", "start", "type"
        };

        private static readonly Dictionary<string, HashSet<string>> TagAttrs = new(StringComparer.OrdinalIgnoreCase)
        {
            ["a"] = new(StringComparer.OrdinalIgnoreCase) { "href", "target", "rel", "name" },
            ["img"] = new(StringComparer.OrdinalIgnoreCase) { "src", "alt" },
            ["iframe"] = new(StringComparer.OrdinalIgnoreCase) { "src", "allow", "allowfullscreen", "frameborder" },
        };

        // Chỉ cho nhúng video/bản đồ từ các nguồn quen thuộc
        private static readonly string[] IframeHosts =
        {
            "www.youtube.com", "youtube.com", "www.youtube-nocookie.com", "player.vimeo.com",
            "www.google.com", "maps.google.com", "www.facebook.com"
        };

        private static readonly Regex UnsafeStyle = new(
            @"expression|javascript|vbscript|url\s*\(|behavior|@import|-moz-binding|\\|/\*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex DataImage = new(
            @"^data:image/(png|jpe?g|gif|webp);base64,[a-z0-9+/=\s]+$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string Sanitize(string? html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            var doc = new HtmlDocument { OptionFixNestedTags = true };
            doc.LoadHtml(html);

            var sb = new StringBuilder(html.Length);
            foreach (var node in doc.DocumentNode.ChildNodes)
                Write(node, sb);
            return sb.ToString();
        }

        /// <summary>Hiển thị HTML người dùng nhập đã qua bộ lọc (thay cho Html.Raw).</summary>
        public static IHtmlContent SafeHtml(this IHtmlHelper html, string? value)
            => new HtmlString(Sanitize(value));

        private static void Write(HtmlNode node, StringBuilder sb)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Text:
                    sb.Append(Encode(HtmlEntity.DeEntitize(((HtmlTextNode)node).Text)));
                    return;
                case HtmlNodeType.Comment:
                    return;
                case HtmlNodeType.Document:
                    foreach (var c in node.ChildNodes) Write(c, sb);
                    return;
            }

            var name = node.Name.ToLowerInvariant();
            if (DroppedTags.Contains(name)) return;

            if (!AllowedTags.Contains(name))
            {
                // Thẻ lạ: bỏ thẻ, giữ nội dung
                foreach (var c in node.ChildNodes) Write(c, sb);
                return;
            }

            if (name == "iframe" && !IsAllowedIframe(node.GetAttributeValue("src", ""))) return;

            sb.Append('<').Append(name);
            var hasBlankTarget = false;
            foreach (var attr in node.Attributes)
            {
                var an = attr.Name.ToLowerInvariant();
                if (!GlobalAttrs.Contains(an) && !(TagAttrs.TryGetValue(name, out var allowed) && allowed.Contains(an)))
                    continue;

                var value = HtmlEntity.DeEntitize(attr.Value ?? string.Empty);
                if (an == "href" || an == "src")
                {
                    if (!IsSafeUrl(value, allowDataImage: name == "img" && an == "src")) continue;
                }
                else if (an == "style")
                {
                    if (UnsafeStyle.IsMatch(value)) continue;
                }
                else if (an == "target")
                {
                    hasBlankTarget = value.Equals("_blank", StringComparison.OrdinalIgnoreCase);
                }
                else if (an == "rel") continue; // tự đặt lại bên dưới

                sb.Append(' ').Append(an).Append("=\"").Append(Encode(value)).Append('"');
            }
            if (name == "a" && hasBlankTarget) sb.Append(" rel=\"noopener noreferrer\"");

            if (VoidTags.Contains(name))
            {
                sb.Append(" />");
                return;
            }

            sb.Append('>');
            foreach (var c in node.ChildNodes) Write(c, sb);
            sb.Append("</").Append(name).Append('>');
        }

        /// <summary>Chỉ thoát &amp; &lt; &gt; " ' — giữ nguyên chữ tiếng Việt (WebUtility mã hóa cả à, é...).</summary>
        public static string Encode(string value)
        {
            var sb = new StringBuilder(value.Length + 16);
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '&': sb.Append("&amp;"); break;
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    case '"': sb.Append("&quot;"); break;
                    case '\'': sb.Append("&#39;"); break;
                    default: sb.Append(ch); break;
                }
            }
            return sb.ToString();
        }

        private static bool IsSafeUrl(string url, bool allowDataImage)
        {
            // Bỏ ký tự điều khiển / khoảng trắng mà trình duyệt bỏ qua khi đọc scheme (java\tscript:)
            var compact = new string(url.Where(ch => !char.IsControl(ch) && !char.IsWhiteSpace(ch)).ToArray());
            if (compact.Length == 0) return false;
            if (allowDataImage && DataImage.IsMatch(url.Trim())) return true;

            var colon = compact.IndexOf(':');
            var firstDelim = compact.IndexOfAny(new[] { '/', '?', '#' });
            if (colon < 0 || (firstDelim >= 0 && firstDelim < colon)) return true; // đường dẫn tương đối

            var scheme = compact[..colon].ToLowerInvariant();
            return scheme is "http" or "https" or "mailto" or "tel";
        }

        private static bool IsAllowedIframe(string src)
        {
            if (!Uri.TryCreate(HtmlEntity.DeEntitize(src).Trim(), UriKind.Absolute, out var uri)) return false;
            return uri.Scheme == Uri.UriSchemeHttps
                && IframeHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase);
        }
    }
}
