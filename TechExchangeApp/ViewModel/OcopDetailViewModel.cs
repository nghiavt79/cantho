using TechExchangeApp.Entities;

namespace TechExchangeApp.ViewModel
{
    public class OcopDetailViewModel
    {
        public SanPhamCNTB Product { get; set; } = null!;
        public string QrDataUri { get; set; } = "";
        public string TraceUrl { get; set; } = "";
        public string? SupplierName { get; set; }
        public string? SupplierUrl { get; set; }
        public List<SanPhamCNTB> RelatedProducts { get; set; } = new();
    }
}
