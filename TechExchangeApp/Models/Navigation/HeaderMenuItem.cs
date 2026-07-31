namespace TechExchangeApp.Models.Navigation
{
    /// <summary>
    /// Một mục trên thanh menu header. Dữ liệu tĩnh lấy từ bảng Menu (MenuPosition = "1").
    /// Danh sách con (Children) do ViewComponent bơm vào theo request (danh mục Sản phẩm/Dịch vụ),
    /// KHÔNG lưu trong bản cache của service.
    /// </summary>
    public class HeaderMenuItem
    {
        public int MenuId { get; set; }
        public string Label { get; set; } = "";
        public string Url { get; set; } = "#";

        /// <summary>Class bootstrap-icon (vd "bi-star-fill"), null nếu không có.</summary>
        public string? Icon { get; set; }

        /// <summary>Class CSS phụ cho link desktop (vd "site-header-v2__ocop-link").</summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Nhóm danh mục cần bơm submenu động: 1 = Sản phẩm (Category.ParentId=1),
        /// 2 = Dịch vụ (Category.ParentId=2). Null = mục thường, không có submenu.
        /// </summary>
        public int? CategoryGroup { get; set; }

        /// <summary>Danh mục con (ViewComponent điền theo ngôn ngữ; rỗng khi CategoryGroup null).</summary>
        public List<HeaderMenuChild> Children { get; set; } = new();
    }

    public class HeaderMenuChild
    {
        public string Label { get; set; } = "";
        public string Url { get; set; } = "#";
    }
}
