using System.IO;
using System.IO.Compression;
using System.Text;
using FluentAssertions;
using TechExchangeApp.Services;
using TechExchangeApp.Web.Helpers;
using Xunit;

namespace TechExchangeApp.Tests.Security
{
    /// <summary>
    /// Kiểm thử bản vá Stored XSS và Unrestricted File Upload (pentest MobiFone 10/2026).
    /// </summary>
    public class HtmlSanitizerTests
    {
        [Theory]
        [InlineData("<script>alert(1)</script>xin chào", "xin chào")]
        [InlineData("<img src=x onerror=alert(1)>", "<img src=\"x\" />")]
        [InlineData("<a href=\"javascript:alert(1)\">x</a>", "<a>x</a>")]
        [InlineData("<a href=\"java\tscript:alert(1)\">x</a>", "<a>x</a>")]
        [InlineData("<svg onload=alert(1)><circle/></svg>ok", "ok")]
        [InlineData("<iframe src=\"https://evil.com\"></iframe>", "")]
        [InlineData("<p style=\"background:url(javascript:alert(1))\">a</p>", "<p>a</p>")]
        [InlineData("<div onclick=\"alert(1)\">a</div>", "<div>a</div>")]
        public void Sanitize_RemovesActiveContent(string input, string expected)
            => HtmlSanitizerHelper.Sanitize(input).Should().Be(expected);

        [Fact]
        public void Sanitize_KeepsEditorFormatting()
        {
            var html = "<p><strong>Mô tả</strong> <em>công nghệ</em></p><ul><li>Ý 1</li></ul>"
                     + "<a href=\"https://mekongx.vn/x\" target=\"_blank\">link</a><img src=\"/uploads/a.png\" alt=\"a\">";
            HtmlSanitizerHelper.Sanitize(html).Should().Be(
                "<p><strong>Mô tả</strong> <em>công nghệ</em></p><ul><li>Ý 1</li></ul>"
                + "<a href=\"https://mekongx.vn/x\" target=\"_blank\" rel=\"noopener noreferrer\">link</a>"
                + "<img src=\"/uploads/a.png\" alt=\"a\" />");
        }

        [Fact]
        public void Sanitize_KeepsYoutubeEmbed()
            => HtmlSanitizerHelper.Sanitize("<iframe src=\"https://www.youtube.com/embed/abc\"></iframe>")
                .Should().Be("<iframe src=\"https://www.youtube.com/embed/abc\"></iframe>");

        [Fact]
        public void Nl2Br_EncodesBeforeLineBreaks()
            => AppHtmlHelper.Nl2Br(null!, "<script>alert(1)</script>\ndòng 2").ToString()
                .Should().Be("&lt;script&gt;alert(1)&lt;/script&gt;<br />dòng 2");
    }

    public class UploadFileGuardTests
    {
        private static string? Check(string name, byte[] data)
            => UploadFileGuard.Validate(name, new MemoryStream(data), data.Length);

        private static byte[] Pdf(string body) => Encoding.Latin1.GetBytes("%PDF-1.4\n" + body + "\n%%EOF");

        [Fact]
        public void CleanPdf_IsAccepted()
            => Check("a.pdf", Pdf("1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj")).Should().BeNull();

        [Theory]
        [InlineData("1 0 obj << /OpenAction << /S /JavaScript /JS (app.alert('XSS')) >> >> endobj")]
        [InlineData("1 0 obj << /AA << /O << /S /J#61vaScript /JS (x) >> >> >> endobj")]
        [InlineData("1 0 obj << /S /Launch /F (cmd.exe) >> endobj")]
        public void PdfWithScript_IsRejected(string body)
            => Check("a.pdf", Pdf(body)).Should().NotBeNull();

        [Fact]
        public void PdfWithScriptInCompressedStream_IsRejected()
        {
            byte[] packed;
            using (var ms = new MemoryStream())
            {
                using (var z = new ZLibStream(ms, CompressionLevel.Optimal, leaveOpen: true))
                    z.Write(Encoding.Latin1.GetBytes("<< /S /JavaScript /JS (app.alert(1)) >>"));
                packed = ms.ToArray();
            }
            var head = Encoding.Latin1.GetBytes("%PDF-1.5\n5 0 obj << /Type /ObjStm /Filter /FlateDecode /Length " + packed.Length + " >>\nstream\n");
            var tail = Encoding.Latin1.GetBytes("\nendstream\nendobj\n%%EOF");
            Check("a.pdf", [.. head, .. packed, .. tail]).Should().NotBeNull();
        }

        [Theory]
        [InlineData("x.html")]
        [InlineData("x.svg")]
        [InlineData("x.exe")]
        [InlineData("x.docm")]
        public void DisallowedExtension_IsRejected(string name)
            => Check(name, Encoding.ASCII.GetBytes("<html><script>alert(1)</script></html>")).Should().NotBeNull();

        [Fact]
        public void HtmlRenamedToPdf_IsRejected()
            => Check("a.pdf", Encoding.ASCII.GetBytes("<html><script>alert(1)</script></html>")).Should().NotBeNull();

        [Fact]
        public void PngSignature_IsChecked()
        {
            Check("a.png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0 }).Should().BeNull();
            Check("a.png", Encoding.ASCII.GetBytes("GIF89a....")).Should().NotBeNull();
        }

        private static byte[] Docx(params string[] entries)
        {
            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
                foreach (var e in entries)
                    using (var w = new StreamWriter(zip.CreateEntry(e).Open())) w.Write("x");
            return ms.ToArray();
        }

        [Fact]
        public void Docx_WithoutMacro_IsAccepted()
            => Check("a.docx", Docx("[Content_Types].xml", "word/document.xml")).Should().BeNull();

        [Fact]
        public void Docx_WithVbaProject_IsRejected()
            => Check("a.docx", Docx("[Content_Types].xml", "word/document.xml", "word/vbaProject.bin")).Should().NotBeNull();

        [Fact]
        public void OleDoc_WithVbaProject_IsRejected()
        {
            var ole = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };
            var clean = new byte[512]; ole.CopyTo(clean, 0);
            Check("a.doc", clean).Should().BeNull();

            var name = Encoding.Unicode.GetBytes("_VBA_PROJECT");
            var macro = new byte[512]; ole.CopyTo(macro, 0); name.CopyTo(macro, 128);
            Check("a.doc", macro).Should().NotBeNull();
        }
    }
}
