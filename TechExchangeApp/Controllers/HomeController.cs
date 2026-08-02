using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;
using TechExchangeApp.Configuration;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Helpers;
using TechExchangeApp.Interfaces;
using TechExchangeApp.Localization;
using TechExchangeApp.ViewModel;

namespace TechExchangeApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProductService _productService;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly string _mainDomain;
        private readonly SiteBrandingOptions _branding;
        private readonly Services.IHomeCacheSignal _homeCacheSignal;

        private static readonly int[] TinSuKienMenus = { 44, 72, 83, 46 };
        private const int VideoMenuId = 71;
        private const int YeuCauMenuId = 67;
        private const string HeroStatsDataPath = "js/hero-stats-data.json";

        public HomeController(
            AppDbContext context,
            IProductService productService,
            IMemoryCache cache,
            IWebHostEnvironment env,
            IConfiguration configuration,
            IOptions<AppSettings> appSettings,
            IOptions<SiteBrandingOptions> branding,
            Services.IHomeCacheSignal homeCacheSignal)
        {
            _context = context;
            _productService = productService;
            _cache = cache;
            _env = env;
            _configuration = configuration;
            _mainDomain = appSettings.Value.MainDomain;
            _branding = branding.Value;
            _homeCacheSignal = homeCacheSignal;
        }

        public async Task<IActionResult> Index()
        {
            var lang = LangHelper.CurrentLangId(HttpContext);
            var isEn = LangHelper.IsEnglish(HttpContext);
            var cacheKey = $"home:index:{(isEn ? "en" : "vi")}:{_mainDomain}";
            var data = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_configuration.GetValue("HomeCache:AbsoluteSeconds", 180));
                entry.SlidingExpiration = TimeSpan.FromSeconds(_configuration.GetValue("HomeCache:SlidingSeconds", 60));
                entry.AddExpirationToken(_homeCacheSignal.GetChangeToken()); // cho phép CMS xóa cache trang chủ tức thì
                var newProducts = await _productService.GetNewProductsAsync(12, excludeOcop: true, hotFirst: true, languageId: lang);

                return new HomeIndexCacheVm
                {
                    TinSuKien = LoadTinSuKien(),
                    VideoCongNghe = LoadVideoCongNghe(),
                    YeuCauCongNghe = LoadYeuCauCongNghe(),
                    Customers = await LoadLogoItemsAsync(subject: 4, take: 32),
                    Partners = await LoadLogoItemsAsync(subject: 5, take: 8),
                    FeaturedTechnologies = MapFeaturedTechnologies(newProducts.Take(10), isEn),
                    FeaturedNews = LoadFeaturedNews(isEn),
                    Experts = LoadExperts(isEn, lang),
                    Stats = await BuildStatsAsync(lang, isEn),
                    NewProducts = newProducts
                };
            }) ?? new HomeIndexCacheVm();

            var model = new HomeViewModel
            {
                IsEnglish = isEn,
                Branding = MapBranding(),
                CongNgheMoiCapNhatHtml = "",
                ProductCNMoiCapNhatHtml = "",
                TinSuKien = data.TinSuKien,
                VideoCongNghe = data.VideoCongNghe,
                YeuCauCongNghe = data.YeuCauCongNghe,
                WhatWeDo = BuildWhatWeDo(isEn),
                Services = BuildServices(isEn),
                Reasons = BuildReasons(isEn),
                ProcessSteps = BuildProcessSteps(isEn),
                Customers = data.Customers,
                Partners = data.Partners,
                PopularTags = BuildPopularTags(isEn),
                FeaturedTechnologies = data.FeaturedTechnologies,
                FeaturedNews = data.FeaturedNews,
                Experts = data.Experts,
                Stats = data.Stats,
                SearchFields = _context.Categories
                    .Where(x => x.ParentId == 1 && x.MainCate == true && x.StatusId == 1 && x.LanguageId == lang)
                    .OrderBy(x => x.Sort ?? int.MaxValue)
                    .ThenBy(x => x.Title)
                    .Select(x => new HomeFieldOptionVm
                    {
                        Value = x.CatId.ToString(),
                        // EN: dùng TitleEn nếu có, thiếu thì fallback Title.
                        Label = isEn && x.TitleEn != null && x.TitleEn != "" ? x.TitleEn : (x.Title ?? "")
                    })
                    .ToList()
            };

            ViewBag.NewTech = data.NewProducts.Take(10).ToList();
            ViewBag.NewProducts = data.NewProducts;

            return View(model);
        }

        private HomeBrandingVm MapBranding()
        {
            return new HomeBrandingVm
            {
                ShortName = _branding.ShortName,
                OrganizationName = _branding.OrganizationName,
                Tagline = _branding.Tagline,
                ProvinceName = _branding.ProvinceName,
                Phone = _branding.Phone,
                Email = _branding.Email,
                Address = _branding.Address,
                WorkingHours = _branding.WorkingHours
            };
        }

        private async Task<List<HomeStatVm>> BuildStatsAsync(int langId, bool isEn)
        {
            var experts = await _context.NhaTuVans.AsNoTracking().CountAsync(x => x.LanguageId == langId && x.StatusId == 3);
            var technologies = await _context.SanPhamCNTBs.AsNoTracking().CountAsync(x => x.LanguageId == langId && x.StatusId == 3);
            var partners = await _context.NhaCungUngs.AsNoTracking().CountAsync(x => x.LanguageId == langId);
            var labs = await _context.Categories.AsNoTracking().CountAsync(x => x.LanguageId == langId);
            var floors = ReadHeroStatsFloors();

            if (isEn)
            {
                return new List<HomeStatVm>
                {
                    new() { Icon = "bi-people", Value = FormatCount(Math.Max(experts, floors.ExpertsFloor)), Label = "Scientists & experts" },
                    new() { Icon = "bi-building-gear", Value = FormatCount(Math.Max(labs, floors.LabsFloor)), Label = "Facilities & laboratories" },
                    new() { Icon = "bi-person-workspace", Value = FormatCount(Math.Max(experts, floors.ExpertsFloor)), Label = "Consulting experts" },
                    new() { Icon = "bi-diagram-3", Value = FormatCount(Math.Max(partners, floors.PartnersFloor)), Label = "Business partners" },
                    new() { Icon = "bi-bank", Value = floors.YearsLabel, Label = "Years of operation & development" }
                };
            }

            return new List<HomeStatVm>
            {
                new() { Icon = "bi-people", Value = FormatCount(Math.Max(experts, floors.ExpertsFloor)), Label = "Nhà khoa học & chuyên gia" },
                new() { Icon = "bi-building-gear", Value = FormatCount(Math.Max(labs, floors.LabsFloor)), Label = "Cơ sở & phòng thí nghiệm" },
                new() { Icon = "bi-person-workspace", Value = FormatCount(Math.Max(experts, floors.ExpertsFloor)), Label = "Chuyên gia tư vấn" },
                new() { Icon = "bi-diagram-3", Value = FormatCount(Math.Max(partners, floors.PartnersFloor)), Label = "Đối tác doanh nghiệp" },
                new() { Icon = "bi-bank", Value = floors.YearsLabel, Label = "Năm hoạt động & phát triển" }
            };
        }

        private record HeroStatsFloors(int ExpertsFloor, int LabsFloor, int PartnersFloor, string YearsLabel);

        private HeroStatsFloors ReadHeroStatsFloors()
        {
            var defaults = new HeroStatsFloors(1000, 70, 2400, "20+");

            try
            {
                var filePath = Path.Combine(_env.WebRootPath, HeroStatsDataPath);
                if (!System.IO.File.Exists(filePath))
                {
                    return defaults;
                }

                using var doc = JsonDocument.Parse(System.IO.File.ReadAllText(filePath));
                var root = doc.RootElement;

                return new HeroStatsFloors(
                    root.TryGetProperty("expertsFloor", out var e) && e.TryGetInt32(out var ev) ? ev : defaults.ExpertsFloor,
                    root.TryGetProperty("labsFloor", out var l) && l.TryGetInt32(out var lv) ? lv : defaults.LabsFloor,
                    root.TryGetProperty("partnersFloor", out var p) && p.TryGetInt32(out var pv) ? pv : defaults.PartnersFloor,
                    root.TryGetProperty("yearsLabel", out var y) && y.GetString() is { Length: > 0 } yv ? yv : defaults.YearsLabel);
            }
            catch
            {
                return defaults;
            }
        }

        private static string FormatCount(int value)
        {
            if (value < 10) return value.ToString();
            var rounded = value >= 100 ? value / 100 * 100 : value / 10 * 10;
            return $"{rounded}+";
        }

        private static List<HomeFeatureVm> BuildWhatWeDo(bool isEn)
        {
            if (isEn)
            {
                return new List<HomeFeatureVm>
                {
                    new() { Icon = "bi-cpu", Title = "Supply technologies", Url = "/en/technologies" },
                    new() { Icon = "bi-link-45deg", Title = "Connect supply & demand", Url = "/en/search" },
                    new() { Icon = "bi-lightbulb", Title = "Research - Application - Technology transfer", Url = "/en/services" },
                    new() { Icon = "bi-upc-scan", Title = "Product traceability", Url = "/en/services" },
                    new() { Icon = "bi-stars", Title = "Innovation & technology upgrade consulting", Url = "/en/services" },
                    new() { Icon = "bi-mortarboard", Title = "Outreach & capacity building", Url = "/en/news-event" }
                };
            }

            return new List<HomeFeatureVm>
            {
                new() { Icon = "bi-cpu", Title = "Cung cấp công nghệ", Url = "/cong-nghe" },
                new() { Icon = "bi-link-45deg", Title = "Kết nối cung cầu công nghệ", Url = "/tim-kiem-doi-tac" },
                new() { Icon = "bi-lightbulb", Title = "Nghiên cứu - Ứng dụng - Chuyển giao công nghệ", Url = "/dich-vu-tu-van" },
                new() { Icon = "bi-upc-scan", Title = "Truy xuất nguồn gốc sản phẩm", Url = "/dich-vu-tu-van" },
                new() { Icon = "bi-stars", Title = "Tư vấn đổi mới sáng tạo & đổi mới công nghệ", Url = "/dang-ky-tu-van" },
                new() { Icon = "bi-mortarboard", Title = "Truyền thông & nâng cao năng lực", Url = "/tin-su-kien-44" }
            };
        }

        private static List<HomeFeatureVm> BuildServices(bool isEn)
        {
            if (isEn)
            {
                return new List<HomeFeatureVm>
                {
                    new() { Icon = "bi-bezier2", Title = "Technology consulting & transfer", Description = "Solutions tailored to real-world needs.", Url = "/en/services" },
                    new() { Icon = "bi-upc-scan", Title = "Traceability", Description = "Product origin tracing and QR-code verification.", Url = "/en/services" },
                    new() { Icon = "bi-easel2", Title = "Training & capacity building", Description = "Upskilling for individuals and organizations.", Url = "/en/news-event" }
                };
            }

            return new List<HomeFeatureVm>
            {
                new() { Icon = "bi-bezier2", Title = "Tư vấn & chuyển giao công nghệ", Description = "Giải pháp phù hợp nhu cầu thực tiễn.", Url = "/dich-vu-tu-van" },
                new() { Icon = "bi-upc-scan", Title = "Truy xuất nguồn gốc", Description = "Xác thực nguồn gốc sản phẩm qua mã QR truy xuất.", Url = "/dich-vu-tu-van" },
                new() { Icon = "bi-easel2", Title = "Đào tạo & tập huấn", Description = "Nâng cao năng lực cho cá nhân, tổ chức.", Url = "/tin-su-kien-44" }
            };
        }

        private static List<HomeFeatureVm> BuildReasons(bool isEn)
        {
            if (isEn)
            {
                return new List<HomeFeatureVm>
                {
                    new() { Icon = "bi-award", Title = "Experienced expert network" },
                    new() { Icon = "bi-check2-circle", Title = "Practical, effective, sustainable solutions" },
                    new() { Icon = "bi-share", Title = "Wide partner network" },
                    new() { Icon = "bi-hand-thumbs-up", Title = "A steady companion for businesses and community" }
                };
            }

            return new List<HomeFeatureVm>
            {
                new() { Icon = "bi-award", Title = "Đội ngũ chuyên gia giàu kinh nghiệm" },
                new() { Icon = "bi-check2-circle", Title = "Giải pháp thiết thực, hiệu quả, bền vững" },
                new() { Icon = "bi-share", Title = "Mạng lưới đối tác rộng khắp" },
                new() { Icon = "bi-hand-thumbs-up", Title = "Đồng hành cùng doanh nghiệp và cộng đồng" }
            };
        }

        private static List<HomeProcessStepVm> BuildProcessSteps(bool isEn)
        {
            if (isEn)
            {
                return new List<HomeProcessStepVm>
                {
                    new() { Number = 1, Icon = "bi-bullseye", Title = "Receive requirements", Description = "Listen to and analyze real-world needs." },
                    new() { Number = 2, Icon = "bi-file-earmark-text", Title = "Advise & propose solutions", Description = "Recommend suitable technologies and equipment." },
                    new() { Number = 3, Icon = "bi-clipboard2-check", Title = "Technology and proposal review", Description = "Assess technology fit and supplier proposals." },
                    new() { Number = 4, Icon = "bi-shield-check", Title = "Negotiation, legal and signing support", Description = "Support negotiation, legal review and contract signing." },
                    new() { Number = 5, Icon = "bi-headset", Title = "Support & accompany", Description = "Technical support throughout the process." },
                    new() { Number = 6, Icon = "bi-graph-up-arrow", Title = "Grow & improve", Description = "Training, technology updates and expanded adoption." }
                };
            }

            return new List<HomeProcessStepVm>
            {
                new() { Number = 1, Icon = "bi-bullseye", Title = "Tiếp nhận nhu cầu", Description = "Lắng nghe, phân tích nhu cầu thực tế." },
                new() { Number = 2, Icon = "bi-file-earmark-text", Title = "Tư vấn & đề xuất giải pháp", Description = "Đề xuất công nghệ, thiết bị phù hợp." },
                new() { Number = 3, Icon = "bi-clipboard2-check", Title = "Đánh giá công nghệ và hồ sơ đề xuất", Description = "Đánh giá mức độ phù hợp của công nghệ và hồ sơ nhà cung cấp." },
                new() { Number = 4, Icon = "bi-shield-check", Title = "Hỗ trợ đàm phán, pháp lý và ký kết", Description = "Hỗ trợ điều khoản đàm phán, rà soát pháp lý và ký hợp đồng." },
                new() { Number = 5, Icon = "bi-headset", Title = "Đồng hành & hỗ trợ", Description = "Hỗ trợ kỹ thuật trong suốt quá trình." },
                new() { Number = 6, Icon = "bi-graph-up-arrow", Title = "Nâng cao & phát triển", Description = "Đào tạo, cập nhật công nghệ và mở rộng ứng dụng." }
            };
        }

        private async Task<List<HomePartnerVm>> LoadLogoItemsAsync(int subject, int take)
        {
            var lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            return await _context.ImageAdvers
                .AsNoTracking()
                .Where(x => x.Subject == subject && x.StatusID == 3 && x.LanguageID == lang)
                .OrderBy(x => x.Sort ?? int.MaxValue)
                .ThenBy(x => x.ID)
                .Take(take)
                .Select(x => new HomePartnerVm
                {
                    Name = x.Title ?? "",
                    LogoUrl = NormalizeImageUrl(x.SRC)
                })
                .ToListAsync();
        }

        private static string NormalizeImageUrl(string? src)
        {
            if (string.IsNullOrWhiteSpace(src))
            {
                return "";
            }

            if (src.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                src.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                src.StartsWith("/", StringComparison.Ordinal))
            {
                return src;
            }

            return "/" + src.TrimStart('~', '/');
        }

        private static List<HomeFieldOptionVm> BuildPopularTags(bool isEn)
        {
            // Value = từ khóa tìm kiếm (giữ tiếng Việt để khớp dữ liệu), Label = hiển thị theo ngôn ngữ.
            var tags = new (string Vi, string En)[]
            {
                ("Công nghệ sau thu hoạch", "Post-harvest technology"),
                ("Nuôi cấy mô thực vật",     "Plant tissue culture"),
                ("Xử lý nước thải",          "Wastewater treatment"),
                ("Đóng gói - bảo quản",      "Packaging & preservation"),
                ("Năng lượng tái tạo",       "Renewable energy")
            };
            // EN: cả từ khóa tìm lẫn nhãn đều tiếng Anh (search sẽ dùng /en/search).
            return tags
                .Select(t => new HomeFieldOptionVm { Value = isEn ? t.En : t.Vi, Label = isEn ? t.En : t.Vi })
                .ToList();
        }

        private static string ProductCompany(SanPhamCNTB x)
        {
            if (!string.IsNullOrWhiteSpace(x.CoQuanChuTri)) return x.CoQuanChuTri;
            if (!string.IsNullOrWhiteSpace(x.CoQuanChuQuan)) return x.CoQuanChuQuan;
            if (!string.IsNullOrWhiteSpace(x.HoTen)) return x.HoTen;
            return "";
        }

        private static (string Badge, string BadgeType) ProductBadge(SanPhamCNTB x, HttpContext http)
        {
            if (x.IsHot == true) return (I18n.T(http, "common.featured"), "hot");
            if (x.Created.HasValue && x.Created.Value >= DateTime.Now.AddDays(-30)) return (I18n.T(http, "common.new"), "new");
            return ("", "new");
        }

        private List<HomeTechCardVm> MapFeaturedTechnologies(IEnumerable<SanPhamCNTB> items, bool isEn)
        {
            var result = items.Select(x =>
            {
                var (badge, badgeType) = ProductBadge(x, HttpContext);
                return new HomeTechCardVm
                {
                    Title = x.Name ?? I18n.T(HttpContext, "common.techUpdating"),
                    Description = CleanSummary(x.MoTaNgan ?? x.MoTa, 120),
                    ImageUrl = ProductController.CookedImageURL("254-170", x.QuyTrinhHinhAnh, _mainDomain),
                    Url = isEn ? $"{_mainDomain}en/products/{x.QueryString}-{x.ID}" : $"{_mainDomain}san-pham/chi-tiet/{x.QueryString}-{x.ID}",
                    Category = x.ProductType == 2 ? I18n.T(HttpContext, "common.equipment")
                             : x.ProductType == 3 ? I18n.T(HttpContext, "common.ipAssetsFull")
                             : I18n.T(HttpContext, "common.technology"),
                    Company = ProductCompany(x),
                    Badge = badge,
                    BadgeType = badgeType
                };
            }).ToList();

            return result.Any() ? result : BuildFallbackTechnologies(isEn);
        }

        private static List<HomeTechCardVm> BuildFallbackTechnologies(bool isEn)
        {
            if (isEn)
            {
                return new List<HomeTechCardVm>
                {
                    new() { Title = "Smart hydroponic vegetable system", Description = "Clean cultivation, water saving and high productivity.", ImageUrl = "/image/tuoinuocnhogiot.png", Url = "/en/technologies", Category = "High-tech agriculture", Badge = "New", BadgeType = "new" },
                    new() { Title = "Domestic wastewater treatment technology", Description = "Efficient, easy to operate and environmentally friendly.", ImageUrl = "/image/thietbiloc.png", Url = "/en/technologies", Category = "Environment", Badge = "Featured", BadgeType = "hot" },
                    new() { Title = "Cold drying system for agricultural products", Description = "Preserves quality and extends product shelf life.", ImageUrl = "/image/dinhlang.png", Url = "/en/technologies", Category = "Post-harvest technology", Badge = "", BadgeType = "new" }
                };
            }

            return new List<HomeTechCardVm>
            {
                new() { Title = "Hệ thống trồng rau thủy canh thông minh", Description = "Giải pháp canh tác sạch, tiết kiệm nước, năng suất cao.", ImageUrl = "/image/tuoinuocnhogiot.png", Url = "/cong-nghe", Category = "Nông nghiệp công nghệ cao", Badge = "Mới", BadgeType = "new" },
                new() { Title = "Công nghệ xử lý nước thải sinh hoạt", Description = "Hiệu quả cao, vận hành dễ dàng, thân thiện môi trường.", ImageUrl = "/image/thietbiloc.png", Url = "/cong-nghe", Category = "Môi trường", Badge = "Nổi bật", BadgeType = "hot" },
                new() { Title = "Hệ thống sấy lạnh nông sản", Description = "Giữ nguyên chất lượng, kéo dài thời gian bảo quản.", ImageUrl = "/image/dinhlang.png", Url = "/cong-nghe", Category = "Công nghệ sau thu hoạch", Badge = "", BadgeType = "new" }
            };
        }

        private List<HomeNewsCardVm> LoadFeaturedNews(bool isEn)
        {
            // EN: menu "News & Event" (62); VI: các menu Tin sự kiện gốc
            var rootMenus = isEn ? new List<int> { 62 } : TinSuKienMenus.ToList();
            var childIds = rootMenus.SelectMany(uspSelectSubMenu).ToList();
            var menuIds = rootMenus.Concat(childIds).Distinct().ToList();

            return _context.Contents.AsNoTracking()
                // VI giữ nguyên (không thêm ràng buộc ngôn ngữ); EN chỉ lấy bản LanguageId=2
                .Where(x => x.StatusId == 3 && x.MenuId.HasValue && menuIds.Contains(x.MenuId.Value)
                    && (!isEn || x.LanguageId == 2))
                .OrderByDescending(x => x.PublishedDate)
                .Take(3)
                .Select(x => new HomeNewsCardVm
                {
                    Title = x.Title ?? "Tin tức - sự kiện",
                    Description = CleanSummary(x.Description ?? x.Contents, 120),
                    ImageUrl = ProductController.CookedImageURL("254-170", x.Image, _mainDomain),
                    Url = isEn
                        ? _mainDomain + "en/news-event/" + x.QueryString + "-" + x.Id
                        : _mainDomain + (x.MenuId == 44 ? "tin-tuc-su-kien" : x.MenuId.ToString()) + "/" + x.QueryString + "-" + x.Id,
                    PublishedDate = x.PublishedDate
                })
                .ToList();
        }

        private List<HomeExpertVm> LoadExperts(bool isEn, int langId)
        {
            var experts = _context.NhaTuVans.AsNoTracking()
                .Where(x => x.LanguageId == langId && x.StatusId == 3)
                .OrderByDescending(x => x.Rating ?? 0)
                .ThenByDescending(x => x.Created)
                .Take(3)
                .Select(x => new HomeExpertVm
                {
                    Name = x.FullName,
                    Title = string.IsNullOrEmpty(x.HocHam) ? (x.ChucVu ?? "Chuyên gia tư vấn") : x.HocHam,
                    Field = x.DichVu ?? x.LinhVucId ?? x.CoQuan ?? "Khoa học và công nghệ",
                    ImageUrl = ProductController.CookedImageURL("254-170", x.HinhDaiDien, _mainDomain),
                    Url = isEn ? _mainDomain + "en/experts/" + x.QueryString + "-" + x.TuVanId : _mainDomain + "chuyen-gia/" + x.QueryString + "-" + x.TuVanId
                })
                .ToList();

            if (experts.Any())
            {
                return experts;
            }

            if (isEn)
            {
                return new List<HomeExpertVm>
                {
                    new() { Name = "Technology consultant", Title = "Biotechnology specialist", Field = "Research and applied biotechnology.", ImageUrl = "/images/NoImages.jpg", Url = "/en/experts" },
                    new() { Name = "Agritech consultant", Title = "High-tech agriculture specialist", Field = "Cultivation, preservation and agricultural processing.", ImageUrl = "/images/NoImages.jpg", Url = "/en/experts" },
                    new() { Name = "Environment consultant", Title = "Environmental technology specialist", Field = "Environmental treatment and sustainable development.", ImageUrl = "/images/NoImages.jpg", Url = "/en/experts" }
                };
            }

            return new List<HomeExpertVm>
            {
                new() { Name = "PGS.TS. Nguyễn Văn A", Title = "Chuyên gia Công nghệ sinh học", Field = "Nghiên cứu và ứng dụng công nghệ sinh học.", ImageUrl = "/images/NoImages.jpg", Url = "/chuyen-gia" },
                new() { Name = "TS. Trần Thị B", Title = "Chuyên gia Nông nghiệp công nghệ cao", Field = "Canh tác, bảo quản và chế biến nông sản.", ImageUrl = "/images/NoImages.jpg", Url = "/chuyen-gia" },
                new() { Name = "TS. Lê Hoàng C", Title = "Chuyên gia Môi trường", Field = "Xử lý môi trường và phát triển bền vững.", ImageUrl = "/images/NoImages.jpg", Url = "/chuyen-gia" }
            };
        }

        private List<TinSuKienTabVm> LoadTinSuKien()
        {
            var menus = _context.Menus
                .AsNoTracking()
                .Where(x => TinSuKienMenus.Contains(x.MenuId))
                .OrderBy(x => x.Sort)
                .ToList();

            var menuIds = menus.Select(x => x.MenuId).ToList();
            var childIdsByParent = _context.Menus
                .AsNoTracking()
                .Where(x => x.ParentId.HasValue && menuIds.Contains(x.ParentId.Value))
                .Select(x => new { ParentId = x.ParentId!.Value, x.MenuId })
                .ToList()
                .GroupBy(x => x.ParentId)
                .ToDictionary(x => x.Key, x => x.Select(i => i.MenuId).ToList());

            var result = new List<TinSuKienTabVm>();

            foreach (var m in menus)
            {
                childIdsByParent.TryGetValue(m.MenuId, out var childIds);
                childIds ??= new List<int>();

                var items = _context.Contents
                    .AsNoTracking()
                    .Where(x => (x.MenuId == m.MenuId || childIds.Contains(x.MenuId ?? 0))
                             && x.StatusId == 3)
                    .OrderByDescending(x => x.PublishedDate)
                    .Take(6)
                    .Select(x => new TinSuKienItemVm
                    {
                        Title = x.Title,
                        Description = x.Description,
                        ImageUrl = ProductController.CookedImageURL("460-275", x.Image, _mainDomain),
                        Link = $"{_mainDomain}{(x.MenuId == 44 ? "tin-tuc-su-kien" : x.MenuId.ToString())}/{x.QueryString}-{x.Id}"
                    })
                    .ToList();

                result.Add(new TinSuKienTabVm
                {
                    MenuId = m.MenuId,
                    Title = m.Title,
                    Items = items
                });
            }

            return result;
        }

        private List<VideoVm> LoadVideoCongNghe()
        {
             var childIds = uspSelectSubMenu(VideoMenuId);

            return _context.Contents
                .AsNoTracking()
                .Where(x => (x.MenuId == VideoMenuId || childIds.Contains(x.MenuId ?? 0))
                         && x.StatusId == 3)
                .OrderByDescending(x => x.PublishedDate)
                .Take(9)
                .Select(x => new VideoVm
                {
                    Title = x.Title,
                    VideoUrl = ExtractYouTubeUrl(x.Description),
                    ImageUrl = ProductController.CookedImageURL("460-275", x.Image, _mainDomain),
                    Link = $"{_mainDomain}{x.MenuId}/{x.QueryString}-{x.Id}"
                })
                .ToList();
        }

        private YeuCauCongNgheVm LoadYeuCauCongNghe()
        {
            var childIds = uspSelectSubMenu(YeuCauMenuId);

            var list = _context.ContentsYeuCaus
                .AsNoTracking()
                .Where(x => (x.MenuId == YeuCauMenuId || childIds.Contains(x.MenuId ?? 0))
                         && x.StatusId == 3)
                .OrderByDescending(x => x.PublishedDate)
                .Take(6)
                .ToList();

            return new YeuCauCongNgheVm
            {
                Title = "Yêu cầu công nghệ",
                Col1 = list.Take(3).Select(MapYeuCau).ToList(),
                Col2 = list.Skip(3).Take(3).Select(MapYeuCau).ToList()
            };
        }

        public static string ExtractYouTubeUrl(string? desc)
        {
            if (string.IsNullOrEmpty(desc)) return "";
            var idx = desc.IndexOf("https://www.youtube.com", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return "";
            var end = desc.IndexOf("\"", idx);
            return end < 0 ? "" : desc.Substring(idx, end - idx);
        }

        private List<int> uspSelectSubMenu(int parentId)
        {
            return _context.Menus
                .AsNoTracking()
                .Where(x => x.ParentId == parentId)
                .Select(x => x.MenuId)
                .ToList();
        }

        private YeuCauItemVm MapYeuCau(ContentsYeuCau x)
        {
            return new YeuCauItemVm
            {
                Title = x.Title,
                Viewed = x.Viewed,
                ImageUrl = ProductController.CookedImageURL("254-170", x.Image, _mainDomain),
                Link = $"{_mainDomain}{x.MenuId}/yeu-cau/{x.QueryString}-{x.Id}"
            };
        }

        private static string CleanSummary(string? html, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(html)) return "";
            var text = Regex.Replace(html, "<.*?>", " ");
            text = Regex.Replace(text, "\\s+", " ").Trim();
            if (text.Length <= maxLength) return text;
            return text.Substring(0, maxLength).TrimEnd() + "...";
        }

        private sealed class HomeIndexCacheVm
        {
            public List<TinSuKienTabVm> TinSuKien { get; set; } = new();
            public List<VideoVm> VideoCongNghe { get; set; } = new();
            public YeuCauCongNgheVm YeuCauCongNghe { get; set; } = new();
            public List<HomePartnerVm> Customers { get; set; } = new();
            public List<HomePartnerVm> Partners { get; set; } = new();
            public List<HomeTechCardVm> FeaturedTechnologies { get; set; } = new();
            public List<HomeNewsCardVm> FeaturedNews { get; set; } = new();
            public List<HomeExpertVm> Experts { get; set; } = new();
            public List<HomeStatVm> Stats { get; set; } = new();
            public List<SanPhamCNTB> NewProducts { get; set; } = new();
        }

        public IActionResult ContractDashboard()
        {
            return View();
        }

        public IActionResult ConnectionDashboard()
        {
            return View();
        }
    }
}

