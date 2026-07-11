using TechExchangeApp.Entities;
using TechExchangeApp.ViewModel;

namespace TechExchangeApp.Interfaces
{
    public interface IProductService
    {
        Task<List<SanPhamCNTB>> GetNewProductsAsync(int take, bool excludeOcop = false);
        Task<List<SanPhamCNTB>> GetProductsByCategoryAsync(int catId, int languageId, int take);
        Task<SanPhamCNTB?> GetProductByIdAsync(int id);
        Task<List<SanPhamCNTB>> GetRelatedProductsAsync(int productId, int languageId, int take);
        Task<int> GetProductCountByCategoryAsync(int catId);
        Task<List<SanPhamCNTB>> GetPagedProductsByCategoryAsync(int catId, int page, int pageSize);

        // --- ProductType-scoped queries (CongNghe / ThietBi / SanPhamTriTue pages) ---
        Task<List<SanPhamCNTB>> GetNewProductsByProductTypeAsync(int productType, int take);
        Task<List<SanPhamCNTB>> GetProductsByCategoryAndProductTypeAsync(
            int cateId, int productType, int languageId, int take);
    }
}
