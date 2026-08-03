using Microsoft.AspNetCore.Http;

namespace TechExchangeApp.Helpers
{
    /// <summary>
    /// Ngôn ngữ hiện tại suy ra từ cờ do middleware "/en" gắn vào HttpContext.Items["Lang"].
    /// Không dùng Session. LanguageId: 1 = tiếng Việt, 2 = tiếng Anh.
    /// </summary>
    public static class LangHelper
    {
        public const int Vi = 1;
        public const int En = 2;

        public static bool IsEnglish(HttpContext? ctx)
            => (ctx?.Items["Lang"] as string) == "en";

        /// <summary>
        /// Toàn bộ path khu vực đã đăng nhập, được đổi ngôn ngữ bằng cookie "site_lang"
        /// vì không có URL /en riêng.
        /// Phủ hết mọi controller [Authorize] để menu/topbar/sidebar luôn theo ngôn ngữ
        /// người dùng chọn; trang nào chưa dịch thì phần ruột vẫn tiếng Việt cho tới khi
        /// dịch tới — đây là cách triển khai i18n thông thường, không gỡ bản dịch của
        /// chrome chỉ vì body chưa xong.
        /// KHÔNG đưa vào: khu quản trị /cms và toàn bộ site công khai.
        /// Lưu ý dùng StartsWithSegments nên khớp theo trọn segment:
        /// "/Project" KHÔNG khớp "/Projects" hay "/ProjectMembers" — phải liệt kê riêng.
        /// </summary>
        private static readonly string[] LocalizedWorkspacePaths =
        {
            // Khu tài khoản & tổng quan
            "/Account", "/Dashboard", "/Chat", "/Notifications", "/NotificationApi", "/Secure", "/Support",
            // Hồ sơ giao dịch
            "/Project", "/Projects", "/ProjectMembers", "/Seller",
            // Các bước trong quy trình chuyển giao
            "/TechTransfer", "/NDA", "/RFQ", "/Proposal", "/ProposalList", "/Scoring",
            "/Negotiation", "/LegalReview", "/Signing", "/Contract", "/EContract",
            "/AdvancePayment", "/PilotTest", "/Handover", "/Training", "/TechDoc",
            "/ImplementationLog", "/Acceptance", "/Liquidation",
            // Dịch vụ của tôi (kèm URL slug tiếng Việt được đăng ký trong Program.cs)
            "/OcopOrder",
            "/QuanLySanPham",       "/quan-ly-san-pham",
            "/DangKyTuVan",         "/dang-ky-tu-van",
            "/DangKyNhaCungUng",    "/dang-ky-nha-cung-ung"
        };

        public static bool IsLocalizedWorkspacePath(PathString path)
        {
            foreach (var prefix in LocalizedWorkspacePaths)
            {
                if (path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public static int CurrentLangId(HttpContext? ctx)
            => IsEnglish(ctx) ? En : Vi;

        /// <summary>URL trang chủ theo ngôn ngữ: "/en" khi đang tiếng Anh, ngược lại "/".</summary>
        public static string HomeUrl(HttpContext? ctx)
            => IsEnglish(ctx) ? "/en" : "/";

        /// <summary>URL trang tìm kiếm theo ngôn ngữ: "/en/search" khi tiếng Anh, ngược lại "/Search".</summary>
        public static string SearchUrl(HttpContext? ctx)
            => IsEnglish(ctx) ? "/en/search" : "/Search";

        /// <summary>
        /// URL trang nội dung menu (động) theo ngôn ngữ:
        /// tiếng Anh "/en/page/{slug}-{menuId}", tiếng Việt "/trang/{slug}-{menuId}".
        /// slug chỉ để đẹp URL; menuId mới quyết định trang.
        /// </summary>
        public static string MenuPath(bool isEnglish, string? slug, int menuId)
        {
            var s = string.IsNullOrWhiteSpace(slug) ? "muc" : slug.Trim();
            return (isEnglish ? "/en/page/" : "/trang/") + s + "-" + menuId;
        }

        /// <summary>URL bài tin tiếng Anh: /en/news-event/{slug}-{id}</summary>
        public static string EnNewsUrl(string? slug, long id)
            => $"/en/news-event/{slug}-{id}";
    }
}
