using QRCoder;

namespace TechExchangeApp.Helpers
{
    /// <summary>Sinh mã QR dạng data URI PNG (dùng cho in nhãn sản phẩm — HSMT C.5).</summary>
    public static class QrCodeHelper
    {
        public static string ToPngDataUri(string content, int pixelsPerModule = 8)
        {
            using var generator = new QRCodeGenerator();
            using var qrData = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var bytes = new PngByteQRCode(qrData).GetGraphic(pixelsPerModule);
            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        }
    }
}
