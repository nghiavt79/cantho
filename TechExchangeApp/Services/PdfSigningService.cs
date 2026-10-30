using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using SysPath = System.IO.Path;

namespace TechExchangeApp.Services;

/// <summary>
/// Embeds a visible digital signature block into a PDF using iText7.
/// Tries to place the signature near "Đại diện Bên A/B" text anchor.
/// Fallback: Buyer → bottom-right, Seller → bottom-left.
/// </summary>
public class PdfSigningService
{
    private readonly ILogger<PdfSigningService> _log;
    private readonly IWebHostEnvironment _env;

    public PdfSigningService(ILogger<PdfSigningService> log, IWebHostEnvironment env)
    {
        _log  = log;
        _env  = env;
    }

    public async Task<string> EmbedVisibleSignatureAsync(
        string   sourcePdfPath,
        byte[]   signatureBytes,
        byte[]   certificateBytes,
        string   certSubject,
        string   certIssuer,
        string   certSerial,
        int      role,           // 1 = Buyer (Bên A), 2 = Seller (Bên B)
        int      projectId,
        bool     otp      = false,   // true = chữ ký điện tử xác thực OTP (không có chứng thư số)
        DateTime? signedAt = null)    // giờ ký thực tế (giờ địa phương); null = hiện tại
    {
        _log.LogInformation("Embedding visible signature into PDF: {Path}", sourcePdfPath);

        var dir = SysPath.Combine(_env.WebRootPath, "uploads", "contracts", $"proj_{projectId}", "signed");
        Directory.CreateDirectory(dir);
        var fileName   = $"signed_{(role == 1 ? "buyer" : "seller")}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var outputPath = SysPath.Combine(dir, fileName);

        var ownerName  = ParseCN(certSubject)  ?? certSubject;
        var issuerName = ParseCN(certIssuer)   ?? certIssuer;
        var signedDate = (signedAt ?? DateTime.Now).ToString("dd/MM/yyyy HH:mm");
        // Bên A = Người bán (Seller, role=2) | Bên B = Người mua (Buyer, role=1)
        var roleLabel  = role == 1 ? "Bên B (Người mua)" : "Bên A (Người bán)";

        var pdfBytes = await File.ReadAllBytesAsync(sourcePdfPath);

        using var srcMs = new MemoryStream(pdfBytes);
        using var dstMs = new MemoryStream();

        using (var reader = new PdfReader(srcMs))
        {
            using var writer = new PdfWriter(dstMs, new WriterProperties());
            writer.SetCloseStream(false);
            using var pdfDoc = new PdfDocument(reader, writer);

            // ── Tìm tọa độ anchor text trên TỪNG trang (ưu tiên trang cuối) ──
            // Bên A (Seller, role=2) → anchor "Đại diện Bên A" (cột TRÁI trong template)
            // Bên B (Buyer,  role=1) → anchor "Đại diện Bên B" (cột PHẢI trong template)
            var anchorTexts = role == 2
                ? new[] { "Đại diện Bên A", "BEN A", "Bên A", "ĐẠI DIỆN BÊN A" }
                : new[] { "Đại diện Bên B", "BEN B", "Bên B", "ĐẠI DIỆN BÊN B" };

            Rectangle? anchorRect = null;
            int targetPageNum = pdfDoc.GetNumberOfPages(); // mặc định trang cuối

            // Tìm từ trang cuối ngược lên
            for (int p = pdfDoc.GetNumberOfPages(); p >= 1; p--)
            {
                anchorRect = FindTextPosition(pdfDoc.GetPage(p), anchorTexts);
                if (anchorRect != null) { targetPageNum = p; break; }
            }

            var targetPage = pdfDoc.GetPage(targetPageNum);
            var pageSize   = targetPage.GetPageSize();

            // ── Tính vị trí signature block ──
            // Tìm thấy ô ký → khung gọn, nằm giữa dòng "(Ký, ghi rõ họ tên…)" và tên người đại diện
            bool  compact = anchorRect != null;
            float blockW = compact ? 180f : 220f;
            float blockH = compact ? 58f  : 80f;
            float hdrH   = compact ? 14f  : 18f;
            float lineH  = compact ? 10f  : 12f;
            float margin  = 18f;
            float x, y;

            if (anchorRect != null)
            {
                // Căn giữa theo dòng "ĐẠI DIỆN BÊN A/B", mép trên cách đáy dòng đó 25pt (chừa dòng "(Ký, ghi rõ…)")
                x = anchorRect.GetX() + anchorRect.GetWidth() / 2f - blockW / 2f;
                y = anchorRect.GetY() - 25f - blockH;

                // Clamp để không tràn ra ngoài trang
                x = Math.Max(pageSize.GetLeft() + margin, Math.Min(x, pageSize.GetRight() - blockW - margin));
                y = Math.Max(pageSize.GetBottom() + margin, y);
            }
            else
            {
                // Fallback: Bên A (Seller, role=2) → trái | Bên B (Buyer, role=1) → phải
                x = role == 2
                    ? pageSize.GetLeft()  + margin           // Bên A: trái
                    : pageSize.GetRight() - blockW - margin; // Bên B: phải
                y = pageSize.GetBottom() + margin;
            }

            _log.LogInformation("Signature pos role={Role}: x={X:F0} y={Y:F0} (anchor={Found})",
                role, x, y, anchorRect != null ? "found" : "fallback");

            // ── Font Unicode ──
            PdfFont fontRegular, fontBold;
            var ttrRegular = @"C:\Windows\Fonts\times.ttf";
            var ttrBold    = @"C:\Windows\Fonts\timesbd.ttf";
            var arialPath  = @"C:\Windows\Fonts\arial.ttf";
            if (File.Exists(ttrRegular) && File.Exists(ttrBold))
            {
                fontRegular = PdfFontFactory.CreateFont(ttrRegular, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                fontBold    = PdfFontFactory.CreateFont(ttrBold,    PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            }
            else if (File.Exists(arialPath))
            {
                fontRegular = PdfFontFactory.CreateFont(arialPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                fontBold    = fontRegular;
            }
            else
            {
                fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                fontBold    = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            }

            // ── Vẽ signature panel ──
            var canvas = new PdfCanvas(targetPage);

            // Nền
            canvas.SetFillColor(new DeviceRgb(0.96f, 0.98f, 1.0f));
            canvas.Rectangle(x, y, blockW, blockH);
            canvas.Fill();

            // Border
            canvas.SetStrokeColor(new DeviceRgb(0.2f, 0.4f, 0.8f));
            canvas.SetLineWidth(1.5f);
            canvas.Rectangle(x, y, blockW, blockH);
            canvas.Stroke();

            // Header strip (màu theo vai trò)
            // Màu header: Bên A (Seller) → cam/nâu đỏ | Bên B (Buyer) → xanh dương
            var headerColor = role == 2
                ? new DeviceRgb(0.72f, 0.25f, 0.05f)  // đỏ cam — Bên A (Người bán)
                : new DeviceRgb(0.1f, 0.45f, 0.75f);  // xanh dương — Bên B (Người mua)
            canvas.SetFillColor(headerColor);
            canvas.Rectangle(x, y + blockH - hdrH, blockW, hdrH);
            canvas.Fill();

            // Header text
            canvas.BeginText()
                  .SetFontAndSize(fontBold, compact ? 7.5f : 8.5f)
                  .SetFillColor(ColorConstants.WHITE)
                  .MoveText(x + 8f, y + blockH - hdrH + 4f)
                  .ShowText($"\u2714 {(otp ? "Chữ ký điện tử (OTP)" : "Chữ ký số")} — {roleLabel}")
                  .EndText();

            float lineY = y + blockH - hdrH - 10f;

            // Ký bởi
            canvas.BeginText()
                  .SetFontAndSize(fontBold, 7.5f)
                  .SetFillColor(new DeviceRgb(0.1f, 0.1f, 0.1f))
                  .MoveText(x + 8f, lineY)
                  .ShowText("Ký bởi: ")
                  .EndText();
            canvas.BeginText()
                  .SetFontAndSize(fontRegular, 7.5f)
                  .SetFillColor(new DeviceRgb(0.1f, 0.1f, 0.1f))
                  .MoveText(x + 42f, lineY)
                  .ShowText(Truncate(ownerName, 28))
                  .EndText();

            lineY -= lineH;

            // Cấp bởi
            canvas.BeginText()
                  .SetFontAndSize(fontBold, 7f)
                  .SetFillColor(new DeviceRgb(0.3f, 0.3f, 0.3f))
                  .MoveText(x + 8f, lineY)
                  .ShowText(otp ? "Xác thực: " : "Cấp bởi: ")
                  .EndText();
            canvas.BeginText()
                  .SetFontAndSize(fontRegular, 7f)
                  .SetFillColor(new DeviceRgb(0.3f, 0.3f, 0.3f))
                  .MoveText(x + 42f, lineY)
                  .ShowText(Truncate(issuerName, 26))
                  .EndText();

            lineY -= lineH;

            // Serial
            canvas.BeginText()
                  .SetFontAndSize(fontBold, 6.5f)
                  .SetFillColor(new DeviceRgb(0.4f, 0.4f, 0.4f))
                  .MoveText(x + 8f, lineY)
                  .ShowText(otp ? "Mã ký: " : "Serial: ")
                  .EndText();
            canvas.BeginText()
                  .SetFontAndSize(fontRegular, 6.5f)
                  .SetFillColor(new DeviceRgb(0.4f, 0.4f, 0.4f))
                  .MoveText(x + 36f, lineY)
                  .ShowText(Truncate(certSerial, 34))
                  .EndText();

            lineY -= lineH;

            // Ngày ký
            canvas.BeginText()
                  .SetFontAndSize(fontBold, 7f)
                  .SetFillColor(headerColor)
                  .MoveText(x + 8f, lineY)
                  .ShowText("Ngày ký: ")
                  .EndText();
            canvas.BeginText()
                  .SetFontAndSize(fontRegular, 7f)
                  .SetFillColor(headerColor)
                  .MoveText(x + 46f, lineY)
                  .ShowText(signedDate)
                  .EndText();

            canvas.Release();

            // Metadata
            var info = pdfDoc.GetDocumentInfo();
            info.SetMoreInfo($"SignatureHex_{role}",  Convert.ToHexString(signatureBytes));
            info.SetMoreInfo($"CertSerial_{role}",    certSerial);
            info.SetMoreInfo($"CertSubject_{role}",   certSubject);
            info.SetMoreInfo($"SignedBy_{role}",       ownerName);
            info.SetMoreInfo($"SignedAt_{role}",       DateTime.UtcNow.ToString("O"));
        }

        await File.WriteAllBytesAsync(outputPath, dstMs.ToArray());
        _log.LogInformation("✅ Signed PDF: {Path} ({Size} bytes)", outputPath, dstMs.Length);

        return outputPath;
    }

    /// <summary>
    /// Tìm tọa độ thật của dòng "Đại diện Bên A/B" (ô ký) trên trang.
    /// Trả về Rectangle bao dòng chữ đó, hoặc null nếu trang không có.
    /// </summary>
    private Rectangle? FindTextPosition(PdfPage page, string[] anchors)
    {
        try
        {
            // anchors[0] = "Đại diện Bên A|B" → regex không phân biệt hoa thường, cho phép nhiều khoảng trắng
            var words   = anchors[0].Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(System.Text.RegularExpressions.Regex.Escape);
            var pattern = new System.Text.RegularExpressions.Regex(string.Join(@"\s+", words),
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

            var strategy = new RegexBasedLocationExtractionStrategy(pattern);
            new PdfCanvasProcessor(strategy).ProcessPageContent(page);

            // Nhiều kết quả → lấy dòng thấp nhất trang (khối ký thường ở cuối)
            return strategy.GetResultantLocations()
                .Select(l => l.GetRectangle())
                .OrderBy(r => r.GetY())
                .FirstOrDefault();
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "FindTextPosition failed, fallback used");
            return null;
        }
    }

    private static string? ParseCN(string? dn)
    {
        if (string.IsNullOrEmpty(dn)) return null;
        var m = System.Text.RegularExpressions.Regex.Match(dn, @"CN=([^,]+)");
        return m.Success ? m.Groups[1].Value.Trim() : null;
    }

    private static string Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Length <= max ? s : s[..max] + "…";
    }
}
