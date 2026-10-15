using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace TechExchangeApp.Services
{
    /// <summary>
    /// Kiểm tra nội dung file tải lên (pentest MobiFone 10/2026 — Unrestricted File Upload):
    /// đuôi file nằm trong danh sách cho phép, chữ ký đầu file khớp đuôi,
    /// PDF không chứa JavaScript / hành động tự chạy, tài liệu Office không chứa macro.
    /// Dùng chung qua UploadValidationFilter cho mọi request có file.
    /// </summary>
    public static class UploadFileGuard
    {
        public const long MaxFileBytes = 50L * 1024 * 1024;

        private static readonly HashSet<string> Images = new(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        private static readonly HashSet<string> OleDocs = new(StringComparer.OrdinalIgnoreCase)
            { ".doc", ".xls", ".ppt" };
        private static readonly HashSet<string> OoxmlDocs = new(StringComparer.OrdinalIgnoreCase)
            { ".docx", ".xlsx", ".pptx" };
        private static readonly HashSet<string> Archives = new(StringComparer.OrdinalIgnoreCase)
            { ".zip", ".rar" };
        private static readonly HashSet<string> Videos = new(StringComparer.OrdinalIgnoreCase)
            { ".mp4", ".mov", ".avi", ".wmv", ".webm" };

        /// <summary>Loại file được xem trực tiếp trên trình duyệt; còn lại trả về dạng tải xuống.</summary>
        public static bool IsInlineSafe(string extension)
            => Images.Contains(extension) || Videos.Contains(extension) || extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

        public static bool IsAllowedExtension(string extension)
            => Images.Contains(extension) || OleDocs.Contains(extension) || OoxmlDocs.Contains(extension)
               || Archives.Contains(extension) || Videos.Contains(extension)
               || extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

        // Từ khóa nguy hiểm trong PDF: chạy script, mở chương trình, nhúng file / nội dung động
        private static readonly Regex PdfDangerous = new(
            @"/(JavaScript|JS|Launch|EmbeddedFiles?|RichMedia|SubmitForm|ImportData)\b",
            RegexOptions.Compiled);
        private static readonly Regex PdfHexEscape = new(@"#([0-9A-Fa-f]{2})", RegexOptions.Compiled);
        private static readonly Regex PdfFlateStream = new(
            @"/FlateDecode.*?stream\r?\n", RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>Trả về null nếu hợp lệ, ngược lại là lý do từ chối (hiển thị cho người dùng).</summary>
        public static string? Validate(string fileName, Stream content, long length)
        {
            var ext = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
            if (!IsAllowedExtension(ext))
                return $"Định dạng tệp \"{ext}\" không được phép tải lên.";
            if (length <= 0) return "Tệp rỗng.";
            if (length > MaxFileBytes) return "Tệp vượt quá dung lượng cho phép (50MB).";

            byte[] data;
            using (var ms = new MemoryStream())
            {
                content.CopyTo(ms);
                data = ms.ToArray();
            }

            if (!HasValidSignature(ext, data))
                return $"Nội dung tệp không đúng định dạng \"{ext}\".";

            if (ext == ".pdf" && PdfHasActiveContent(data))
                return "Tệp PDF chứa mã thực thi (JavaScript/hành động tự động) nên không được chấp nhận.";
            if (OleDocs.Contains(ext) && OleHasMacro(data))
                return "Tệp Office chứa macro nên không được chấp nhận.";
            if (OoxmlDocs.Contains(ext) && OoxmlHasMacro(data))
                return "Tệp Office chứa macro nên không được chấp nhận.";

            return null;
        }

        private static bool StartsWith(byte[] d, params byte[] sig)
            => d.Length >= sig.Length && d.AsSpan(0, sig.Length).SequenceEqual(sig);

        private static bool AsciiAt(byte[] d, int offset, string s)
            => d.Length >= offset + s.Length && Encoding.ASCII.GetString(d, offset, s.Length) == s;

        private static bool HasValidSignature(string ext, byte[] d)
        {
            switch (ext)
            {
                case ".jpg": case ".jpeg": return StartsWith(d, 0xFF, 0xD8, 0xFF);
                case ".png": return StartsWith(d, 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A);
                case ".gif": return AsciiAt(d, 0, "GIF87a") || AsciiAt(d, 0, "GIF89a");
                case ".webp": return AsciiAt(d, 0, "RIFF") && AsciiAt(d, 8, "WEBP");
                case ".bmp": return AsciiAt(d, 0, "BM");
                case ".pdf":
                    // %PDF- có thể đứng sau vài byte rác ở đầu tệp (trình đọc PDF chấp nhận trong 1KB đầu)
                    return Encoding.ASCII.GetString(d, 0, Math.Min(d.Length, 1024)).Contains("%PDF-");
                case ".doc": case ".xls": case ".ppt":
                    return StartsWith(d, 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1);
                case ".docx": case ".xlsx": case ".pptx": case ".zip":
                    return StartsWith(d, 0x50, 0x4B, 0x03, 0x04);
                case ".rar": return AsciiAt(d, 0, "Rar!");
                case ".mp4": case ".mov": return AsciiAt(d, 4, "ftyp") || AsciiAt(d, 4, "moov") || AsciiAt(d, 4, "wide") || AsciiAt(d, 4, "mdat");
                case ".avi": return AsciiAt(d, 0, "RIFF") && AsciiAt(d, 8, "AVI ");
                case ".wmv": return StartsWith(d, 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11);
                case ".webm": return StartsWith(d, 0x1A, 0x45, 0xDF, 0xA3);
                default: return false;
            }
        }

        private static bool PdfHasActiveContent(byte[] d)
        {
            // Latin1 giữ nguyên từng byte → regex chạy được trên dữ liệu nhị phân
            var text = Encoding.Latin1.GetString(d);
            if (ContainsDangerousName(text)) return true;

            // Từ khóa có thể nằm trong object stream đã nén (FlateDecode) → giải nén để quét tiếp
            foreach (Match m in PdfFlateStream.Matches(text))
            {
                var start = m.Index + m.Length;
                var end = text.IndexOf("endstream", start, StringComparison.Ordinal);
                if (end < 0) break;
                var inflated = TryInflate(d, start, end - start);
                if (inflated != null && ContainsDangerousName(Encoding.Latin1.GetString(inflated))) return true;
            }
            return false;
        }

        private static bool ContainsDangerousName(string text)
        {
            // Tên PDF có thể viết dạng /J#61vaScript → chuẩn hóa #xx trước khi so
            var normalized = text.Contains('#')
                ? PdfHexEscape.Replace(text, h => ((char)Convert.ToInt32(h.Groups[1].Value, 16)).ToString())
                : text;
            return PdfDangerous.IsMatch(normalized);
        }

        private static byte[]? TryInflate(byte[] d, int offset, int count)
        {
            if (count < 3) return null;
            try
            {
                // Bỏ 2 byte header zlib
                using var input = new MemoryStream(d, offset + 2, count - 2);
                using var z = new DeflateStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();
                var buffer = new byte[8192];
                int read;
                while ((read = z.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, read);
                    if (output.Length > 20 * 1024 * 1024) break; // chặn bom nén
                }
                return output.ToArray();
            }
            catch
            {
                return null; // stream hỏng / không phải zlib (ảnh, font...) → bỏ qua
            }
        }

        private static bool OleHasMacro(byte[] d)
        {
            // Thư mục OLE lưu tên dạng UTF-16LE: stream "_VBA_PROJECT" có trong mọi tài liệu Word/Excel chứa macro
            var utf16 = Encoding.Unicode.GetString(d, 0, d.Length - (d.Length % 2));
            if (utf16.Contains("_VBA_PROJECT", StringComparison.Ordinal))
                return true;
            // Mã VBA nén (PowerPoint) luôn bắt đầu bằng "Attribut" + "e VB_"
            var latin = Encoding.Latin1.GetString(d);
            var i = latin.IndexOf("Attribut", StringComparison.Ordinal);
            while (i >= 0)
            {
                var tail = latin.Substring(i + 8, Math.Min(6, latin.Length - i - 8));
                if (tail.Contains("e VB_")) return true;
                i = latin.IndexOf("Attribut", i + 8, StringComparison.Ordinal);
            }
            return false;
        }

        private static bool OoxmlHasMacro(byte[] d)
        {
            try
            {
                using var zip = new ZipArchive(new MemoryStream(d), ZipArchiveMode.Read);
                return zip.Entries.Any(e =>
                    e.FullName.EndsWith("vbaProject.bin", StringComparison.OrdinalIgnoreCase)
                    || e.FullName.EndsWith("vbaData.xml", StringComparison.OrdinalIgnoreCase)
                    || e.FullName.Contains("/activeX/", StringComparison.OrdinalIgnoreCase));
            }
            catch (InvalidDataException)
            {
                return true; // không đọc được như file Office hợp lệ → từ chối
            }
        }
    }
}
