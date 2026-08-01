namespace TechExchangeApp.Models.Navigation
{
    /// <summary>
    /// Một mục cấp 1 trên thanh menu header, lấy từ bảng Menu (MenuPosition = "1").
    /// Con cấp 2 (Children) là các row Menu có ParentId = MenuId này — cũng lấy từ DB,
    /// dựng sẵn và cache trong service (menu 2 cấp hoàn toàn data-driven).
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

        /// <summary>Mục con cấp 2 (row Menu có ParentId = MenuId này). Rỗng = link đơn.</summary>
        public List<HeaderMenuChild> Children { get; set; } = new();
    }

    public class HeaderMenuChild
    {
        public string Label { get; set; } = "";
        public string Url { get; set; } = "#";
    }
}
