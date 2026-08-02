using Microsoft.AspNetCore.Html;

namespace TechExchangeApp.Services.Localization
{
    /// <summary>Một dòng UI text đã nạp vào cache (đủ cả cờ AllowHtml).</summary>
    public sealed record UiTextEntry(string Key, string? Vi, string? En, bool AllowHtml);

    /// <summary>
    /// Cấp chuỗi giao diện song ngữ từ bảng UiTranslations (có cache).
    /// Ngôn ngữ lấy từ HttpContext.Items["Lang"] (middleware /en).
    /// </summary>
    public interface IUiTextService
    {
        /// <summary>Text thuần theo ngôn ngữ hiện tại; thiếu key -> trả về chính key.</summary>
        string Text(string key);

        /// <summary>Render: raw HTML CHỈ KHI key có AllowHtml=1, ngược lại vẫn encode.</summary>
        IHtmlContent TrHtml(string key);

        /// <summary>Toàn bộ map (key -> entry), IsActive=1. Dùng cho scan/admin.</summary>
        IReadOnlyDictionary<string, UiTextEntry> GetAll();

        /// <summary>Xoá cache (gọi sau khi CRUD ở admin).</summary>
        void Invalidate();
    }
}
