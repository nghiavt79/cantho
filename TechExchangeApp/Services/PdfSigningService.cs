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

    // ════════════════════════════════════════════════════════════════════
    //  Ký số nhúng vào PDF (adbe.pkcs7.detached) cho USB Token qua LocalSigner
    //  B1 PrepareDeferredAsync: tạo ô chữ ký + chỗ trống, trả mã băm cần ký
    //  B2 LocalSigner ký mã băm trên token
    //  B3 CompleteDeferredAsync: nhúng chữ ký vào chỗ trống, kiểm tra toàn vẹn
    //  Ký ở chế độ nối thêm (append) nên chữ ký trước vẫn hợp lệ.
    // ════════════════════════════════════════════════════════════════════

    private const int SignatureReserve = 16384;

    public record DeferredSignature(string PreparedPath, string FieldName, byte[] Digest);

    public async Task<DeferredSignature> PrepareDeferredAsync(
        string sourcePdfPath, int role, int contractId, int projectId,
        string signerName, string certIssuer, string certSerial)
    {
        var dir = SysPath.Combine(_env.WebRootPath, "uploads", "contracts", $"proj_{projectId}", "signed");
        Directory.CreateDirectory(dir);
        var fieldName    = $"Ky_{(role == 1 ? "BenB" : "BenA")}_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        var preparedPath = SysPath.Combine(dir, $"pending_{contractId}_{role}.pdf");

        var pdfBytes = await File.ReadAllBytesAsync(sourcePdfPath);
        var capture  = new DigestCapture();

        using (var reader = new PdfReader(new MemoryStream(pdfBytes)))
        using (var output = new FileStream(preparedPath, FileMode.Create, FileAccess.Write))
        {
            var signer = new iText.Signatures.PdfSigner(reader, output,
                new StampingProperties().UseAppendMode());
            signer.SetFieldName(fieldName);

            var doc = signer.GetDocument();
            var (pageNum, rect) = FindSignatureRect(doc, role);
            var appearance = signer.GetSignatureAppearance()
                .SetPageNumber(pageNum)
                .SetPageRect(rect)
                .SetReuseAppearance(false);
            DrawPanel(new PdfCanvas(appearance.GetLayer2(), doc), rect.GetWidth(), rect.GetHeight(),
                role, ParseCN(signerName) ?? signerName, ParseCN(certIssuer) ?? certIssuer, certSerial, DateTime.Now);

            signer.SignExternalContainer(capture, SignatureReserve);
        }

        await File.WriteAllTextAsync(preparedPath + ".field", fieldName);
        _log.LogInformation("Prepared deferred signature: contract={Id} role={Role} field={Field}", contractId, role, fieldName);
        return new DeferredSignature(preparedPath, fieldName, capture.Digest);
    }

    /// <summary>Nhúng chữ ký; trả null nếu chữ ký không khớp tài liệu hoặc chứng thư.</summary>
    public async Task<string?> CompleteDeferredAsync(
        int contractId, int role, int projectId, byte[] signatureBytes, byte[] certificateBytes)
    {
        var dir = SysPath.Combine(_env.WebRootPath, "uploads", "contracts", $"proj_{projectId}", "signed");
        var preparedPath = SysPath.Combine(dir, $"pending_{contractId}_{role}.pdf");
        if (!File.Exists(preparedPath) || !File.Exists(preparedPath + ".field"))
            return null;
        var fieldName = (await File.ReadAllTextAsync(preparedPath + ".field")).Trim();

        var factory = iText.Bouncycastleconnector.BouncyCastleFactoryCreator.GetFactory();
        var cert    = factory.CreateX509Certificate(new MemoryStream(certificateBytes));
        var pkcs7   = new iText.Signatures.PdfPKCS7(null, new[] { cert }, "SHA-256", false);
        pkcs7.SetExternalSignatureValue(signatureBytes, null, "RSA");
        var cms = pkcs7.GetEncodedPKCS7(null);

        var outputPath = SysPath.Combine(dir, $"signed_{(role == 1 ? "buyer" : "seller")}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        using (var reader = new PdfReader(preparedPath))
        using (var doc = new PdfDocument(reader))
        using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
        {
            iText.Signatures.PdfSigner.SignDeferred(doc, fieldName, output, new FixedContainer(cms));
        }

        bool valid;
        try
        {
            using var check = new PdfDocument(new PdfReader(outputPath));
            valid = new iText.Signatures.SignatureUtil(check).ReadSignatureData(fieldName)
                .VerifySignatureIntegrityAndAuthenticity();
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Verify embedded signature failed: contract={Id}", contractId);
            valid = false;
        }

        if (!valid)
        {
            File.Delete(outputPath);
            _log.LogWarning("Rejected signature: does not match document/certificate. contract={Id} role={Role}", contractId, role);
            return null;
        }

        File.Delete(preparedPath);
        File.Delete(preparedPath + ".field");
        _log.LogInformation("✅ Embedded PDF signature: {Path}", outputPath);
        return outputPath;
    }

    private (int page, Rectangle rect) FindSignatureRect(PdfDocument doc, int role)
    {
        var anchors = role == 2 ? new[] { "Đại diện Bên A" } : new[] { "Đại diện Bên B" };
        const float blockW = 180f, blockH = 58f, margin = 18f;
        for (int p = doc.GetNumberOfPages(); p >= 1; p--)
        {
            var a = FindTextPosition(doc.GetPage(p), anchors);
            if (a == null) continue;
            var size = doc.GetPage(p).GetPageSize();
            var x = a.GetX() + a.GetWidth() / 2f - blockW / 2f;
            x = Math.Max(size.GetLeft() + margin, Math.Min(x, size.GetRight() - blockW - margin));
            var y = Math.Max(size.GetBottom() + margin, a.GetY() - 25f - blockH);
            return (p, new Rectangle(x, y, blockW, blockH));
        }
        var last = doc.GetNumberOfPages();
        var ps   = doc.GetPage(last).GetPageSize();
        var fx   = role == 2 ? ps.GetLeft() + margin : ps.GetRight() - blockW - margin;
        return (last, new Rectangle(fx, ps.GetBottom() + margin, blockW, blockH));
    }

    private static void DrawPanel(PdfCanvas c, float w, float h, int role,
        string owner, string issuer, string serial, DateTime signedAt)
    {
        PdfFont regular, bold;
        if (File.Exists(@"C:\Windows\Fonts\times.ttf") && File.Exists(@"C:\Windows\Fonts\timesbd.ttf"))
        {
            regular = PdfFontFactory.CreateFont(@"C:\Windows\Fonts\times.ttf", PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            bold    = PdfFontFactory.CreateFont(@"C:\Windows\Fonts\timesbd.ttf", PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
        }
        else
        {
            regular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            bold    = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        }

        const float hdrH = 14f, lineH = 10f;
        var header = role == 2 ? new DeviceRgb(0.72f, 0.25f, 0.05f) : new DeviceRgb(0.1f, 0.45f, 0.75f);
        var roleLabel = role == 1 ? "Bên B (Người mua)" : "Bên A (Người bán)";

        c.SetFillColor(new DeviceRgb(0.96f, 0.98f, 1.0f)).Rectangle(0, 0, w, h).Fill();
        c.SetStrokeColor(new DeviceRgb(0.2f, 0.4f, 0.8f)).SetLineWidth(1.5f).Rectangle(0.75f, 0.75f, w - 1.5f, h - 1.5f).Stroke();
        c.SetFillColor(header).Rectangle(0, h - hdrH, w, hdrH).Fill();

        void Text(PdfFont f, float size, Color color, float x, float y, string s) =>
            c.BeginText().SetFontAndSize(f, size).SetFillColor(color).MoveText(x, y).ShowText(s).EndText();

        Text(bold, 7.5f, ColorConstants.WHITE, 8f, h - hdrH + 4f, $"Chữ ký số — {roleLabel}");
        var y = h - hdrH - 10f;
        var dark = new DeviceRgb(0.1f, 0.1f, 0.1f);
        var grey = new DeviceRgb(0.35f, 0.35f, 0.35f);
        Text(bold, 7.5f, dark, 8f, y, "Ký bởi: ");    Text(regular, 7.5f, dark, 42f, y, Truncate(owner, 28));  y -= lineH;
        Text(bold, 7f, grey, 8f, y, "Cấp bởi: ");     Text(regular, 7f, grey, 42f, y, Truncate(issuer, 26));   y -= lineH;
        Text(bold, 6.5f, grey, 8f, y, "Serial: ");    Text(regular, 6.5f, grey, 36f, y, Truncate(serial, 34)); y -= lineH;
        Text(bold, 7f, header, 8f, y, "Ngày ký: ");   Text(regular, 7f, header, 46f, y, signedAt.ToString("dd/MM/yyyy HH:mm"));
    }

    /// <summary>Chỉ tính SHA-256 phần tài liệu cần ký, chưa ký.</summary>
    private sealed class DigestCapture : iText.Signatures.IExternalSignatureContainer
    {
        public byte[] Digest { get; private set; } = [];

        public byte[] Sign(Stream data)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            Digest = sha.ComputeHash(data);
            return [];
        }

        public void ModifySigningDictionary(PdfDictionary signDic)
        {
            signDic.Put(PdfName.Filter, PdfName.Adobe_PPKLite);
            signDic.Put(PdfName.SubFilter, PdfName.Adbe_pkcs7_detached);
        }
    }

    private sealed class FixedContainer(byte[] cms) : iText.Signatures.IExternalSignatureContainer
    {
        public byte[] Sign(Stream data) => cms;
        public void ModifySigningDictionary(PdfDictionary signDic) { }
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
