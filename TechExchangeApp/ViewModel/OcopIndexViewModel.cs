using TechExchangeApp.Entities;

namespace TechExchangeApp.ViewModel
{
    public class OcopIndexViewModel
    {
        public List<SanPhamCNTB> Products { get; set; } = new();

        public int TotalProducts { get; set; }
        public int TotalTraceable { get; set; }
        public int TotalOrigins { get; set; }
    }
}
