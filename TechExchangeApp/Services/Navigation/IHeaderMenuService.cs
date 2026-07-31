using TechExchangeApp.Models.Navigation;

namespace TechExchangeApp.Services.Navigation
{
    /// <summary>
    /// Cấp dữ liệu tĩnh cho thanh menu header, đọc từ bảng Menu (MenuPosition = "1"),
    /// lọc theo LanguageId, có cache. Danh mục con động do ViewComponent tự bơm.
    /// </summary>
    public interface IHeaderMenuService
    {
        /// <param name="languageId">1 = tiếng Việt, 2 = tiếng Anh.</param>
        IReadOnlyList<HeaderMenuItem> GetHeaderMenu(int languageId);

        /// <summary>Xoá cache cả 2 ngôn ngữ (gọi khi admin sửa/xoá menu).</summary>
        void Invalidate();
    }
}
