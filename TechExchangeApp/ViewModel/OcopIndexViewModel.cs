using TechExchangeApp.Entities;

namespace TechExchangeApp.ViewModel
{
    public class OcopIndexViewModel
    {
        public List<SanPhamCNTB> Products { get; set; } = new();

        public int TotalProducts { get; set; }
        public int TotalTraceable { get; set; }
        public int TotalOrigins { get; set; }

        // --- Phân trang (cùng khuôn với listing sản phẩm) ---
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }
        public int TotalPage => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

        /// <summary>Dải số trang hiển thị, tối đa 5 nút quanh trang hiện tại.</summary>
        public IEnumerable<int> Pages
        {
            get
            {
                if (TotalPage <= 0) yield break;
                var start = Math.Max(1, CurrentPage - 2);
                var end = Math.Min(TotalPage, start + 4);
                start = Math.Max(1, end - 4);
                for (var p = start; p <= end; p++) yield return p;
            }
        }
    }
}
