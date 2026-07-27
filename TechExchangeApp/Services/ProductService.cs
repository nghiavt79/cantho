using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Interfaces;

namespace TechExchangeApp.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SanPhamCNTB>> GetNewProductsAsync(int take, bool excludeOcop = false, bool hotFirst = false)
        {
            // NOTE: bEffectiveDate/eEffectiveDate range filter removed — it caused a full table scan
            // (no index on those columns). StatusId + LanguageId filter is sufficient for homepage.
            var query = _context.SanPhamCNTBs
                .AsNoTracking()
                .Where(x => x.StatusId == 3 && x.LanguageId == 1);

            if (excludeOcop)
            {
                query = query.Where(x => x.ProductType != 4);
            }

            // hotFirst: sản phẩm IsHot xếp lên đầu, sau đó theo ngày CẬP NHẬT mới nhất
            // (dùng Modified vì nhiều sản phẩm có PublishedDate = null, fallback Created).
            var ordered = hotFirst
                ? query.OrderByDescending(x => x.IsHot == true)
                       .ThenByDescending(x => x.Modified ?? x.Created)
                : query.OrderByDescending(x => x.PublishedDate ?? x.Modified ?? x.Created);

            return await ordered
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<SanPhamCNTB>> GetProductsByCategoryAsync(int catId, int languageId, int take)
        {
            // Join with SanPhamCNTBCategory table instead of string parsing
            return await (from p in _context.SanPhamCNTBs
                          join c in _context.SanPhamCNTBCategories on p.ID equals c.SanPhamCNTBId
                          where c.CatId == catId
                                && p.LanguageId == languageId
                                && p.StatusId == 3
                          orderby p.Created descending
                          select p)
                          .Distinct()
                          .Take(take)
                          .ToListAsync();
        }

        public async Task<SanPhamCNTB?> GetProductByIdAsync(int id)
        {
            return await _context.SanPhamCNTBs
                .FirstOrDefaultAsync(x => x.ID == id && x.LanguageId == 1 && x.StatusId == 3);
        }

        public async Task<List<SanPhamCNTB>> GetRelatedProductsAsync(int productId, int languageId, int take)
        {
            // Get category IDs for current product from junction table
            var catIds = await _context.SanPhamCNTBCategories
                .Where(x => x.SanPhamCNTBId == productId)
                .Select(x => x.CatId)
                .ToListAsync();

            if (!catIds.Any()) return new List<SanPhamCNTB>();

            // Find products in the same categories
            return await (from p in _context.SanPhamCNTBs
                          join c in _context.SanPhamCNTBCategories on p.ID equals c.SanPhamCNTBId
                          where catIds.Contains(c.CatId)
                                && p.LanguageId == languageId
                                && p.StatusId == 3
                                && p.ID != productId
                          orderby p.Created descending
                          select p)
                          .Distinct()
                          .Take(take)
                          .ToListAsync();
        }

        public async Task<int> GetProductCountByCategoryAsync(int catId, string? keyword = null)
        {
            var q = from p in _context.SanPhamCNTBs
                    join c in _context.SanPhamCNTBCategories on p.ID equals c.SanPhamCNTBId
                    where c.CatId == catId && p.StatusId == 3
                    select p;

            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(p => p.Name != null && EF.Functions.Like(p.Name, $"%{keyword}%"));

            return await q.Select(p => p.ID).Distinct().CountAsync();
        }

        public async Task<List<SanPhamCNTB>> GetPagedProductsByCategoryAsync(int catId, int page, int pageSize, string? keyword = null, string? sort = null)
        {
            var q = from p in _context.SanPhamCNTBs
                    join c in _context.SanPhamCNTBCategories on p.ID equals c.SanPhamCNTBId
                    where c.CatId == catId && p.StatusId == 3
                    select p;

            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(p => p.Name != null && EF.Functions.Like(p.Name, $"%{keyword}%"));

            // Chỉ lấy đúng các cột listing cần — KHÔNG kéo cột LOB (MoTa/ThongSo/Keywords).
            var proj = q.Select(p => new SanPhamCNTB
            {
                ID = p.ID,
                Name = p.Name,
                Code = p.Code,
                ProductType = p.ProductType,
                TypeId = p.TypeId,
                QuyTrinhHinhAnh = p.QuyTrinhHinhAnh,
                MoTaNgan = p.MoTaNgan,
                NCUId = p.NCUId,
                CategoryId = p.CategoryId,
                OriginalPrice = p.OriginalPrice,
                Currency = p.Currency,
                Rating = p.Rating,
                PublishedDate = p.PublishedDate,
                Modified = p.Modified,
                Created = p.Created
            }).Distinct();

            proj = sort switch
            {
                "name-asc" => proj.OrderBy(p => p.Name),
                "name-desc" => proj.OrderByDescending(p => p.Name),
                "price-asc" => proj.OrderBy(p => p.OriginalPrice ?? 0),
                "price-desc" => proj.OrderByDescending(p => p.OriginalPrice ?? 0),
                _ => proj.OrderByDescending(p => p.PublishedDate ?? p.Modified ?? p.Created) // newest
            };

            return await proj
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // ── ProductType-scoped queries ─────────────────────────────────────────────

        public async Task<List<SanPhamCNTB>> GetNewProductsByProductTypeAsync(int productType, int take)
        {
            return await _context.SanPhamCNTBs
                .AsNoTracking()
                .Where(x => x.ProductType == productType && x.StatusId == 3)
                .OrderByDescending(x => x.Modified)
                .ThenByDescending(x => x.Created)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<SanPhamCNTB>> GetProductsByCategoryAndProductTypeAsync(
            int cateId, int productType, int languageId, int take)
        {
            return await (from p in _context.SanPhamCNTBs
                          join c in _context.SanPhamCNTBCategories on p.ID equals c.SanPhamCNTBId
                          where c.CatId == cateId
                                && p.ProductType == productType
                                && p.StatusId == 3
                                && p.LanguageId == languageId
                          orderby p.Created descending
                          select p)
                          .Distinct()
                          .Take(take)
                          .ToListAsync();
        }
    }
}
