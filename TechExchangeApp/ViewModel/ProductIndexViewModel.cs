using TechExchangeApp.Entities;

namespace TechExchangeApp.ViewModel
{
    public class ProductIndexViewModel
    {
        public List<SanPhamCNTB> NewProducts { get; set; } = new();
        public List<CategoryBlockVm> Categories { get; set; } = new();

        /// <summary>All categories with ParentId=1 for the filter bar (including those with no products).</summary>
        public List<Category> AllCategories { get; set; } = new();

        // Hero stats
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalSuppliers { get; set; }
    }

    public class CategoryBlockVm
    {
        public Category Category { get; set; }
        public List<SanPhamCNTB> Products { get; set; } = new();
    }

    public class ProductRelatedItemVm
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        public string? PriceText { get; set; }
        public string? Url { get; set; }
        public int Star { get; set; }
    }

    /// <summary>Unified "Sản phẩm" listing across all 3 product types (Công nghệ / Thiết bị / Sản phẩm trí tuệ).</summary>
    public class AllProductsViewModel
    {
        public List<SanPhamCNTB> Products { get; set; } = new();
        public int TotalCongNghe { get; set; }
        public int TotalThietBi { get; set; }
        public int TotalTaiSanTriTue { get; set; }
        public int TotalProducts => TotalCongNghe + TotalThietBi + TotalTaiSanTriTue;

        // Phân trang + lọc theo loại (server-side)
        public int CurrentType { get; set; }          // 0 = tất cả, 1/2/3 = loại sản phẩm
        public int CurPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int Total { get; set; }                // tổng SP khớp bộ lọc hiện tại (để dựng pager)
        public List<PageItemVm> Pages { get; set; } = new();
    }
}
