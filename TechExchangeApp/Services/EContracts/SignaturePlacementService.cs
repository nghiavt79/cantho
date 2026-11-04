using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace TechExchangeApp.Services.EContracts
{
    public enum EContractSignatureParty
    {
        BenA,
        BenB
    }

    public record EContractSignaturePlacement(int Page, double X, double Y, double Width, double Height);

    public interface ISignaturePlacementService
    {
        /// <summary>Trả về vùng ký theo tọa độ eContract; null nếu không xác định được, khi đó người chốt phải chọn tay.</summary>
        EContractSignaturePlacement? FindPlacement(byte[] pdf, EContractSignatureParty party);
    }

    /// <summary>
    /// Tìm vị trí thật của dòng "Đại diện Bên A/B" trên PDF và tính vùng ký ngay bên dưới.
    /// Hệ tọa độ eContract theo phản hồi MobiFone: trang từ 1, y cộng dồn theo trang, đơn vị pixel.
    /// Giả định chưa kiểm chứng (Giai đoạn 0): 1 pixel = 1 point PDF, gốc ở góc trên bên trái trang đầu.
    /// </summary>
    public class SignaturePlacementService : ISignaturePlacementService
    {
        private const double FieldWidth = 180;
        private const double FieldHeight = 50; // mẫu HTML chừa khoảng 52pt trước dòng tên người ký
        private const double GapBelowAnchor = 20; // bỏ qua dòng "(Ký, ghi rõ họ tên, đóng dấu)"
        private const float SameLineTolerance = 2f;

        private readonly ILogger<SignaturePlacementService> _logger;

        public SignaturePlacementService(ILogger<SignaturePlacementService> logger)
        {
            _logger = logger;
        }

        public EContractSignaturePlacement? FindPlacement(byte[] pdf, EContractSignatureParty party)
        {
            var anchor = Normalize(party == EContractSignatureParty.BenA ? "Đại diện Bên A" : "Đại diện Bên B");

            try
            {
                using var document = new PdfDocument(new PdfReader(new MemoryStream(pdf)));
                var pageCount = document.GetNumberOfPages();

                var pageTops = new double[pageCount + 1];
                for (var p = 1; p <= pageCount; p++)
                {
                    // Trang xoay chưa rõ eContract tính tọa độ thế nào: không đoán
                    if (document.GetPage(p).GetRotation() != 0)
                    {
                        _logger.LogInformation("Signature placement skipped: rotated page {Page}", p);
                        return null;
                    }
                    if (p < pageCount)
                        pageTops[p + 1] = pageTops[p] + document.GetPage(p).GetPageSize().GetHeight();
                }

                // Khối ký nằm cuối hợp đồng: dò từ trang cuối
                for (var p = pageCount; p >= 1; p--)
                {
                    var page = document.GetPage(p);
                    var match = FindAnchor(page, anchor);
                    if (match == null) continue;

                    var size = page.GetPageSize();
                    var topInPage = size.GetTop() - match.Value.Baseline + GapBelowAnchor;
                    if (topInPage + FieldHeight > size.GetHeight()) return null;

                    var center = (match.Value.StartX + match.Value.EndX) / 2 - size.GetLeft();
                    var x = Math.Clamp(center - FieldWidth / 2, 0, Math.Max(0, size.GetWidth() - FieldWidth));

                    return new EContractSignaturePlacement(
                        p,
                        Math.Round(x),
                        Math.Round(pageTops[p] + topInPage),
                        FieldWidth,
                        FieldHeight);
                }
            }
            catch (Exception ex) when (ex is iText.Commons.Exceptions.ITextException or IOException)
            {
                _logger.LogWarning(ex, "Signature placement failed");
            }

            return null;
        }

        private static (float StartX, float EndX, float Baseline)? FindAnchor(PdfPage page, string anchor)
        {
            var collector = new GlyphCollector();
            new PdfCanvasProcessor(collector).ProcessPageContent(page);

            foreach (var line in collector.GroupIntoLines())
            {
                var text = new StringBuilder();
                var owners = new List<Glyph>();
                foreach (var glyph in line)
                {
                    foreach (var c in Normalize(glyph.Text))
                    {
                        text.Append(c);
                        owners.Add(glyph);
                    }
                }

                var index = text.ToString().IndexOf(anchor, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                    return (owners[index].StartX, owners[index + anchor.Length - 1].EndX, owners[index].Baseline);
            }

            return null;
        }

        /// <summary>Bỏ khoảng trắng, chuẩn hóa dấu tiếng Việt để so khớp không phụ thuộc cách PDF tách chữ.</summary>
        private static string Normalize(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (var c in value.Normalize(NormalizationForm.FormC))
            {
                if (!char.IsWhiteSpace(c)) builder.Append(c);
            }
            return builder.ToString();
        }

        private readonly record struct Glyph(string Text, float StartX, float EndX, float Baseline);

        private sealed class GlyphCollector : IEventListener
        {
            private readonly List<Glyph> _glyphs = new();

            public void EventOccurred(IEventData data, EventType type)
            {
                if (type != EventType.RENDER_TEXT || data is not TextRenderInfo info) return;

                foreach (var character in info.GetCharacterRenderInfos())
                {
                    var text = character.GetText();
                    if (string.IsNullOrEmpty(text)) continue;
                    var baseline = character.GetBaseline();
                    _glyphs.Add(new Glyph(text, baseline.GetStartPoint().Get(0), baseline.GetEndPoint().Get(0), baseline.GetStartPoint().Get(1)));
                }
            }

            public ICollection<EventType> GetSupportedEvents() => new[] { EventType.RENDER_TEXT };

            public IEnumerable<List<Glyph>> GroupIntoLines()
            {
                var lines = new List<List<Glyph>>();
                foreach (var glyph in _glyphs.OrderByDescending(g => g.Baseline))
                {
                    var last = lines.LastOrDefault();
                    if (last != null && Math.Abs(last[0].Baseline - glyph.Baseline) <= SameLineTolerance)
                        last.Add(glyph);
                    else
                        lines.Add(new List<Glyph> { glyph });
                }
                return lines.Select(l => l.OrderBy(g => g.StartX).ToList());
            }
        }
    }
}
