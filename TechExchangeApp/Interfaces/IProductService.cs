using TechExchangeApp.Entities;
using TechExchangeApp.ViewModel;

namespace TechExchangeApp.Interfaces
{
    public interface IProductService
    {
        Task<List<SanPhamCNTB>> GetNewProductsAsync(int take, bool excludeOcop = false, bool hotFirst = false, int languageId = 1);
        Task<List<SanPhamCNTB>> GetProductsByCategoryAsync(int catId, int languageId, int take);
        Task<SanPhamCNTB?> GetProductByIdAsync(int id, int languageId = 1);
        Task<List<SanPhamCNTB>> GetRelatedProductsAsync(int productId, int languageId, int take);
        Task<int> GetProductCountByCategoryAsync(int catId, int languageId = 1, string? keyword = null);
        Task<List<SanPhamCNTB>> GetPagedProductsByCategoryAsync(int catId, int languageId = 1, int page = 1, int pageSize = 12, string? keyword = null, string? sort = null);

        // --- ProductType-scoped queries (CongNghe / ThietBi / SanPhamTriTue pages) ---
        Task<List<SanPhamCNTB>> GetNewProductsByProductTypeAsync(int productType, int take, int languageId = 1);
        Task<List<SanPhamCNTB>> GetProductsByCategoryAndProductTypeAsync(
            int cateId, int productType, int languageId, int take);
    }
}
