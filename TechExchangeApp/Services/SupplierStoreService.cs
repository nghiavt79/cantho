using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Interfaces;

namespace TechExchangeApp.Services
{
    public record SupplierMetrics(int CungUngId, int PublishedProducts, int Contacts, int Views, bool Verified,
        int Transactions = 0, decimal Revenue = 0, decimal Rating = 0, int RatingCount = 0);

    public record SupplierQuota(int Limit, int Used, bool Enforced, SupplierSubscription? Subscription)
    {
        public bool CanAdd => !Enforced || Used < Limit;
    }

    public interface ISupplierStoreService
    {
        /// <summary>Chỉ số để xếp hạng nhà cung cấp (A.2.4).</summary>
        Task<Dictionary<int, SupplierMetrics>> GetMetricsAsync(IReadOnlyCollection<int> cungUngIds);
        /// <summary>Hạng cao nhất mà nhà cung cấp đạt đủ tiêu chí; null nếu không đạt hạng nào.</summary>
        SupplierTier? SuggestTier(SupplierMetrics m, IEnumerable<SupplierTier> tiers);
        /// <summary>Hạn mức số sản phẩm theo gói gian hàng còn hiệu lực (A.2.3).</summary>
        Task<SupplierQuota> GetQuotaAsync(int cungUngId);
        /// <summary>Ghi / gỡ khoản thu tự động gắn với một nguồn (gói gian hàng, gian hàng sự kiện) (A.6.1).</summary>
        Task UpsertAutoIncomeAsync(string refType, int refId, string category, decimal amount, int? cungUngId, string description, string? by);
        Task RemoveAutoIncomeAsync(string refType, int refId);
    }

    public class SupplierStoreService : ISupplierStoreService
    {
        private readonly AppDbContext _db;
        private readonly ISystemParameterService _params;

        public SupplierStoreService(AppDbContext db, ISystemParameterService @params)
        {
            _db = db;
            _params = @params;
        }

        public async Task<Dictionary<int, SupplierMetrics>> GetMetricsAsync(IReadOnlyCollection<int> ids)
        {
            var products = await _db.SanPhamCNTBs.AsNoTracking()
                .Where(p => p.NCUId != null && ids.Contains(p.NCUId.Value) && p.LanguageId == 1)
                .Select(p => new { p.ID, Ncu = p.NCUId!.Value, p.StatusId, p.Viewed }).ToListAsync();
            var productIds = products.Select(p => p.ID).ToList();

            var chats = await _db.ChatConversations.AsNoTracking()
                .Where(c => c.ProductId != null && productIds.Contains(c.ProductId.Value))
                .Select(c => c.ProductId!.Value).ToListAsync();
            var orders = await _db.OcopOrderRequests.AsNoTracking()
                .Where(o => (o.SupplierId != null && ids.Contains(o.SupplierId.Value)) || productIds.Contains(o.ProductId))
                .Select(o => new { o.SupplierId, o.ProductId, o.StatusId, o.GiaTriDon, o.TrangThaiThanhToan }).ToListAsync();
            var productIdsInt = productIds.Select(i => (int)i).ToList();
            var ratings = await _db.EntityRatings.AsNoTracking()
                .Where(r => r.EntityType == TechExchangeApp.Enums.EntityTypes.SanPhamCNTB && r.StatusId == 1 && productIdsInt.Contains(r.EntityId))
                .Select(r => new { r.EntityId, r.Stars }).ToListAsync();
            var productOwner = products.ToDictionary(p => p.ID, p => p.Ncu);

            var verifiedUsers = await (from n in _db.NhaCungUngs.AsNoTracking()
                                       join u in _db.Users.AsNoTracking() on n.UserId equals u.Id
                                       where ids.Contains(n.CungUngId) && u.VerificationLevel >= 3
                                       select n.CungUngId).ToListAsync();

            var result = new Dictionary<int, SupplierMetrics>();
            foreach (var id in ids)
            {
                var mine = products.Where(p => p.Ncu == id).ToList();
                var contacts = chats.Count(pid => productOwner.TryGetValue(pid, out var o) && o == id)
                             + orders.Count(o => o.SupplierId == id || (o.SupplierId == null && productOwner.TryGetValue(o.ProductId, out var ow) && ow == id));
                var myOrders = orders.Where(o => o.SupplierId == id || (o.SupplierId == null && productOwner.TryGetValue(o.ProductId, out var ow2) && ow2 == id)).ToList();
                var transactions = myOrders.Count(o => o.StatusId != 4);
                var revenue = myOrders.Where(o => o.StatusId != 4 && (o.StatusId == 3 || o.TrangThaiThanhToan == 1)).Sum(o => o.GiaTriDon ?? 0);
                var myRatings = ratings.Where(r => productOwner.TryGetValue(r.EntityId, out var ow3) && ow3 == id).ToList();
                var rating = myRatings.Count == 0 ? 0m : Math.Round((decimal)myRatings.Average(r => r.Stars), 1);
                result[id] = new SupplierMetrics(id, mine.Count(p => p.StatusId == 3), contacts,
                    mine.Sum(p => p.Viewed ?? 0), verifiedUsers.Contains(id), transactions, revenue, rating, myRatings.Count);
            }
            return result;
        }

        public SupplierTier? SuggestTier(SupplierMetrics m, IEnumerable<SupplierTier> tiers)
            => tiers.Where(t => t.IsActive
                                && m.PublishedProducts >= t.MinProducts
                                && m.Contacts >= t.MinContacts
                                && m.Views >= t.MinViews
                                && m.Transactions >= t.MinTransactions
                                && m.Revenue >= t.MinRevenue
                                && m.Rating >= t.MinRating
                                && (!t.RequireVerified || m.Verified))
                    .OrderByDescending(t => t.Rank).FirstOrDefault();

        public async Task<SupplierQuota> GetQuotaAsync(int cungUngId)
        {
            var today = DateTime.Today;
            var sub = await _db.SupplierSubscriptions.AsNoTracking()
                .Where(s => s.CungUngId == cungUngId && s.Status == 1 && s.StartDate <= today && s.EndDate >= today)
                .OrderByDescending(s => s.MaxProducts).FirstOrDefaultAsync();
            var limit = sub?.MaxProducts ?? await _params.GetIntAsync("STORE_FREE_PRODUCT_QUOTA", 5);
            var enforced = await _params.GetBoolAsync("STORE_QUOTA_ENFORCED", false);
            // Đếm sản phẩm bản VI của nhà cung cấp (mọi trạng thái — kể cả đang chờ duyệt)
            var used = await _db.SanPhamCNTBs.CountAsync(p => p.NCUId == cungUngId && p.LanguageId == 1);
            return new SupplierQuota(limit, used, enforced, sub);
        }

        public async Task UpsertAutoIncomeAsync(string refType, int refId, string category, decimal amount, int? cungUngId, string description, string? by)
        {
            var e = await _db.FinanceEntries.FirstOrDefaultAsync(x => x.RefType == refType && x.RefId == refId);
            if (e == null)
            {
                e = new FinanceEntry { RefType = refType, RefId = refId, Created = DateTime.Now, CreatedBy = by, EntryDate = DateTime.Today };
                _db.FinanceEntries.Add(e);
            }
            e.Direction = FinanceDirections.Income;
            e.Category = category;
            e.Amount = amount;
            e.CungUngId = cungUngId;
            e.Description = description.Length > 500 ? description[..500] : description;
            await _db.SaveChangesAsync();
        }

        public async Task RemoveAutoIncomeAsync(string refType, int refId)
        {
            var rows = await _db.FinanceEntries.Where(x => x.RefType == refType && x.RefId == refId).ToListAsync();
            if (rows.Count == 0) return;
            _db.FinanceEntries.RemoveRange(rows);
            await _db.SaveChangesAsync();
        }
    }
}
