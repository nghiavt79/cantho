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

        public async Task<List<SanPhamCNTB>> GetNewProductsAsync(int take, bool excludeOcop = false, bool hotFirst = false, int languageId = 1)
        {
            // NOTE: bEffectiveDate/eEffectiveDate range filter removed â€” it caused a full table scan
            // (no index on those columns). StatusId + LanguageId filter is sufficient for homepage.
            var query = _context.SanPhamCNTBs
                .AsNoTracking()
                .Where(x => x.StatusId == 3 && x.LanguageId == languageId);

            if (excludeOcop)
            {
                query = query.Where(x => x.ProductType != 4);
            }

            // hotFirst: sáº£n pháº©m IsHot xáº¿p lÃªn Ä‘áº§u, sau Ä‘Ã³ theo ngÃ y Cáº¬P NHáº¬T má»›i nháº¥t
            // (dÃ¹ng Modified vÃ¬ nhiá»u sáº£n pháº©m cÃ³ PublishedDate = null, fallback Created).
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

        public async Task<SanPhamCNTB?> GetProductByIdAsync(int id, int languageId = 1)
        {
            return await _context.SanPhamCNTBs
                .FirstOrDefaultAsync(x => x.ID == id && x.LanguageId == languageId && x.StatusId == 3);
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

        public async Task<int> GetProductCountByCategoryAsync(int catId, int languageId = 1, string? keyword = null)
        {
            var q = from p in _context.SanPhamCNTBs
                    join c in _context.SanPhamCNTBCategories on p.ID equals c.SanPhamCNTBId
                    where c.CatId == catId
                          && p.LanguageId == languageId
                          && p.StatusId == 3
                          && p.ProductType != 4
                    select p;

            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(p => p.Name != null && EF.Functions.Like(p.Name, $"%{keyword}%"));

            return await q.Select(p => p.ID).Distinct().CountAsync();
        }

        public async Task<List<SanPhamCNTB>> GetPagedProductsByCategoryAsync(int catId, int languageId = 1, int page = 1, int pageSize = 12, string? keyword = null, string? sort = null)
        {
            var q = from p in _context.SanPhamCNTBs
                    join c in _context.SanPhamCNTBCategories on p.ID equals c.SanPhamCNTBId
                    where c.CatId == catId
                          && p.LanguageId == languageId
                          && p.StatusId == 3
                          && p.ProductType != 4
                    select p;

            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(p => p.Name != null && EF.Functions.Like(p.Name, $"%{keyword}%"));

            // Chá»‰ láº¥y Ä‘Ãºng cÃ¡c cá»™t listing cáº§n â€” KHÃ”NG kÃ©o cá»™t LOB (MoTa/ThongSo/Keywords).
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

        // â”€â”€ ProductType-scoped queries â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public async Task<List<SanPhamCNTB>> GetNewProductsByProductTypeAsync(int productType, int take, int languageId = 1)
        {
            return await _context.SanPhamCNTBs
                .AsNoTracking()
                .Where(x => x.ProductType == productType && x.LanguageId == languageId && x.StatusId == 3)
                .OrderByDescending(x => x.Modified)
                .ThenByDescending(x => x.Created)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// Listing phân trang theo ProductType. IsHot xếp trước rồi tới ngày cập nhật mới nhất —
        /// cùng thứ tự với GetNewProductsAsync(hotFirst: true) để trang OCOP nhất quán với
        /// trang sản phẩm. Lọc LanguageId nên bản tiếng Anh chỉ lấy dòng LanguageId = 2.
        /// </summary>
        public async Task<List<SanPhamCNTB>> GetPagedProductsByProductTypeAsync(
            int productType, int languageId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 12;

            return await _context.SanPhamCNTBs
                .AsNoTracking()
                .Where(x => x.ProductType == productType && x.LanguageId == languageId && x.StatusId == 3)
                .OrderByDescending(x => x.IsHot == true)
                .ThenByDescending(x => x.Modified ?? x.Created)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetProductCountByProductTypeAsync(int productType, int languageId)
        {
            return await _context.SanPhamCNTBs
                .AsNoTracking()
                .CountAsync(x => x.ProductType == productType && x.LanguageId == languageId && x.StatusId == 3);
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

