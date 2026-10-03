using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Helpers;
using TechExchangeApp.Localization;

namespace TechExchangeApp.ViewComponents
{
    /// <summary>
    /// Huy hiệu hạng nhà cung cấp (HSMT A.2.2) trên trang gian hàng. Hạng gán trên bản VI;
    /// bản EN tra theo OriginalId. Tên hạng tiếng Anh lấy từ khóa i18n "supplier.tier.{Id}".
    ///   @await Component.InvokeAsync("SupplierTierBadge", new { cungUngId = Model.Id })
    /// </summary>
    public class SupplierTierBadgeViewComponent : ViewComponent
    {
        private readonly AppDbContext _db;
        public SupplierTierBadgeViewComponent(AppDbContext db) => _db = db;

        public async Task<IViewComponentResult> InvokeAsync(int cungUngId)
        {
            var row = await _db.NhaCungUngs.AsNoTracking().Where(n => n.CungUngId == cungUngId)
                .Select(n => new { n.LanguageId, n.OriginalId, n.TierId }).FirstOrDefaultAsync();
            if (row == null) return Content(string.Empty);

            var tierId = row.TierId;
            if (row.LanguageId == LangHelper.En && row.OriginalId.HasValue)
                tierId = await _db.NhaCungUngs.AsNoTracking().Where(n => n.CungUngId == row.OriginalId).Select(n => n.TierId).FirstOrDefaultAsync();
            if (!tierId.HasValue) return Content(string.Empty);

            var tier = await _db.SupplierTiers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tierId && t.IsActive);
            if (tier == null) return Content(string.Empty);

            var name = tier.Name;
            if (LangHelper.IsEnglish(HttpContext))
            {
                var key = "supplier.tier." + tier.Id;
                var en = I18n.T(HttpContext, key);
                if (en != key) name = en;
            }
            var label = System.Net.WebUtility.HtmlEncode(I18n.T(HttpContext, "supplier.tier.label"));
            var html = $"<span class=\"badge supplier-tier-badge\" style=\"background:{tier.BadgeColor};font-size:.85rem\" title=\"{label}\">" +
                       $"<i class=\"bi bi-award-fill me-1\"></i>{System.Net.WebUtility.HtmlEncode(name)}</span>";
            return new HtmlContentViewComponentResult(new HtmlString(html));
        }
    }
}
