using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using TechExchangeApp.Configuration;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Helpers;
using TechExchangeApp.Interfaces;
using TechExchangeApp.ViewModel;

namespace TechExchangeApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProductService _productService;
        private readonly string _mainDomain;
        private readonly SiteBrandingOptions _branding;

        private static readonly int[] TinSuKienMenus = { 44, 72, 83, 46 };
        private const int VideoMenuId = 71;
        private const int YeuCauMenuId = 67;

        public HomeController(
            AppDbContext context,
            IProductService productService,
            IOptions<AppSettings> appSettings,
            IOptions<SiteBrandingOptions> branding)
        {
            _context = context;
            _productService = productService;
            _mainDomain = appSettings.Value.MainDomain;
            _branding = branding.Value;
        }

        public async Task<IActionResult> Index()
        {
            var newProducts = await _productService.GetNewProductsAsync(12);

            var model = new HomeViewModel
            {
                Branding = MapBranding(),
                CongNgheMoiCapNhatHtml = "",
                ProductCNMoiCapNhatHtml = "",
                TinSuKien = LoadTinSuKien(),
                VideoCongNghe = LoadVideoCongNghe(),
                YeuCauCongNghe = LoadYeuCauCongNghe(),
                WhatWeDo = BuildWhatWeDo(),
                Services = BuildServices(),
                Reasons = BuildReasons(),
                ProcessSteps = BuildProcessSteps(),
                Customers = await LoadLogoItemsAsync(subject: 4, take: 32),
                Partners = await LoadLogoItemsAsync(subject: 5, take: 8),
                PopularTags = BuildPopularTags(),
                FeaturedTechnologies = MapFeaturedTechnologies(newProducts.Take(5)),
                FeaturedNews = LoadFeaturedNews(),
                Experts = LoadExperts(),
                Stats = await BuildStatsAsync()
            };

            ViewBag.NewTech = newProducts.Take(10).ToList();
            ViewBag.NewProducts = newProducts;

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

        private async Task<List<HomeStatVm>> BuildStatsAsync()
        {
            var experts = await _context.NhaTuVans.AsNoTracking().CountAsync(x => x.StatusId == 3);
            var technologies = await _context.SanPhamCNTBs.AsNoTracking().CountAsync(x => x.StatusId == 3);
            var partners = await _context.NhaCungUngs.AsNoTracking().CountAsync();
            var labs = await _context.Categories.AsNoTracking().CountAsync();

            return new List<HomeStatVm>
            {
                new() { Icon = "bi-people", Value = FormatCount(Math.Max(experts, 500)), Label = "Nhà khoa học & chuyên gia" },
                new() { Icon = "bi-building-gear", Value = FormatCount(Math.Max(labs, 50)), Label = "Cơ sở & phòng thí nghiệm" },
                new() { Icon = "bi-person-workspace", Value = FormatCount(Math.Max(experts, 30)), Label = "Chuyên gia tư vấn" },
                new() { Icon = "bi-diagram-3", Value = FormatCount(Math.Max(partners, 100)), Label = "Đối tác doanh nghiệp" },
                new() { Icon = "bi-bank", Value = "20+", Label = "Năm hoạt động & phát triển" }
            };
        }

        private static string FormatCount(int value)
        {
            if (value < 10) return value.ToString();
            var rounded = value >= 100 ? value / 100 * 100 : value / 10 * 10;
            return $"{rounded}+";
        }

        private static List<HomeFeatureVm> BuildWhatWeDo()
        {
            return new List<HomeFeatureVm>
            {
                new() { Icon = "bi-cpu", Title = "Cung cấp công nghệ", Url = "/cong-nghe" },
                new() { Icon = "bi-link-45deg", Title = "Kết nối cung cầu công nghệ", Url = "/tim-kiem-doi-tac" },
                new() { Icon = "bi-lightbulb", Title = "Nghiên cứu - Ứng dụng - Chuyển giao công nghệ", Url = "/dich-vu-tu-van" },
                new() { Icon = "bi-flask", Title = "Thử nghiệm & kiểm định", Url = "/dich-vu-tu-van" },
                new() { Icon = "bi-stars", Title = "Tư vấn đổi mới sáng tạo & đổi mới công nghệ", Url = "/dang-ky-tu-van" },
                new() { Icon = "bi-mortarboard", Title = "Truyền thông & nâng cao năng lực", Url = "/tin-su-kien-44" }
            };
        }

        private static List<HomeFeatureVm> BuildServices()
        {
            return new List<HomeFeatureVm>
            {
                new() { Icon = "bi-bezier2", Title = "Tư vấn & chuyển giao công nghệ", Description = "Giải pháp phù hợp nhu cầu thực tiễn.", Url = "/dang-ky-tu-van" },
                new() { Icon = "bi-clipboard2-pulse", Title = "Kiểm định & thử nghiệm", Description = "Đánh giá chất lượng, độ tin cậy và an toàn.", Url = "/dich-vu-tu-van" },
                new() { Icon = "bi-easel2", Title = "Đào tạo & tập huấn", Description = "Nâng cao năng lực cho cá nhân, tổ chức.", Url = "/tin-su-kien-44" }
            };
        }

        private static List<HomeFeatureVm> BuildReasons()
        {
            return new List<HomeFeatureVm>
            {
                new() { Icon = "bi-award", Title = "Đội ngũ chuyên gia giàu kinh nghiệm" },
                new() { Icon = "bi-check2-circle", Title = "Giải pháp thiết thực, hiệu quả, bền vững" },
                new() { Icon = "bi-share", Title = "Mạng lưới đối tác rộng khắp" },
                new() { Icon = "bi-hand-thumbs-up", Title = "Đồng hành cùng doanh nghiệp và cộng đồng" }
            };
        }

        private static List<HomeProcessStepVm> BuildProcessSteps()
        {
            return new List<HomeProcessStepVm>
            {
                new() { Number = 1, Icon = "bi-bullseye", Title = "Tiếp nhận nhu cầu", Description = "Lắng nghe, phân tích nhu cầu thực tế." },
                new() { Number = 2, Icon = "bi-file-earmark-text", Title = "Tư vấn & đề xuất giải pháp", Description = "Đề xuất công nghệ, thiết bị phù hợp." },
                new() { Number = 3, Icon = "bi-clipboard2-check", Title = "Thử nghiệm & kiểm định", Description = "Đánh giá tính khả thi và hiệu quả." },
                new() { Number = 4, Icon = "bi-diagram-3", Title = "Chuyển giao & triển khai", Description = "Chuyển giao công nghệ và triển khai ứng dụng." },
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

        private static List<string> BuildPopularTags()
        {
            return new List<string>
            {
                "Công nghệ sau thu hoạch",
                "Nuôi cấy mô thực vật",
                "Xử lý nước thải",
                "Đóng gói - bảo quản",
                "Năng lượng tái tạo"
            };
        }

        private List<HomeTechCardVm> MapFeaturedTechnologies(IEnumerable<SanPhamCNTB> items)
        {
            var result = items.Select(x => new HomeTechCardVm
            {
                Title = x.Name ?? "Công nghệ đang cập nhật",
                Description = CleanSummary(x.MoTaNgan ?? x.MoTa, 120),
                ImageUrl = ProductController.CookedImageURL("254-170", x.QuyTrinhHinhAnh, _mainDomain),
                Url = $"{_mainDomain}2-cong-nghe-thiet-bi/{x.ProductType}/{x.QueryString}-{x.ID}",
                Category = x.ProductType == 2 ? "Thiết bị" : x.ProductType == 3 ? "Tài sản trí tuệ" : "Công nghệ"
            }).ToList();

            return result.Any() ? result : BuildFallbackTechnologies();
        }

        private static List<HomeTechCardVm> BuildFallbackTechnologies()
        {
            return new List<HomeTechCardVm>
            {
                new() { Title = "Hệ thống trồng rau thủy canh thông minh", Description = "Giải pháp canh tác sạch, tiết kiệm nước, năng suất cao.", ImageUrl = "/image/tuoinuocnhogiot.png", Url = "/cong-nghe", Category = "Nông nghiệp công nghệ cao" },
                new() { Title = "Công nghệ xử lý nước thải sinh hoạt", Description = "Hiệu quả cao, vận hành dễ dàng, thân thiện môi trường.", ImageUrl = "/image/thietbiloc.png", Url = "/cong-nghe", Category = "Môi trường" },
                new() { Title = "Hệ thống sấy lạnh nông sản", Description = "Giữ nguyên chất lượng, kéo dài thời gian bảo quản.", ImageUrl = "/image/dinhlang.png", Url = "/cong-nghe", Category = "Công nghệ sau thu hoạch" }
            };
        }

        private List<HomeNewsCardVm> LoadFeaturedNews()
        {
            var childIds = TinSuKienMenus.SelectMany(uspSelectSubMenu).ToList();
            var menuIds = TinSuKienMenus.Concat(childIds).Distinct().ToList();

            return _context.Contents.AsNoTracking()
                .Where(x => x.StatusId == 3 && x.MenuId.HasValue && menuIds.Contains(x.MenuId.Value))
                .OrderByDescending(x => x.PublishedDate)
                .Take(3)
                .Select(x => new HomeNewsCardVm
                {
                    Title = x.Title ?? "Tin tức - sự kiện",
                    Description = CleanSummary(x.Description ?? x.Contents, 120),
                    ImageUrl = ProductController.CookedImageURL("254-170", x.Image, _mainDomain),
                    Url = _mainDomain + x.MenuId + "/" + x.QueryString + "-" + x.Id + "",
                    PublishedDate = x.PublishedDate
                })
                .ToList();
        }

        private List<HomeExpertVm> LoadExperts()
        {
            var experts = _context.NhaTuVans.AsNoTracking()
                .Where(x => x.StatusId == 3)
                .OrderByDescending(x => x.Rating ?? 0)
                .ThenByDescending(x => x.Created)
                .Take(3)
                .Select(x => new HomeExpertVm
                {
                    Name = x.FullName,
                    Title = string.IsNullOrEmpty(x.HocHam) ? (x.ChucVu ?? "Chuyên gia tư vấn") : x.HocHam,
                    Field = x.DichVu ?? x.LinhVucId ?? x.CoQuan ?? "Khoa học và công nghệ",
                    ImageUrl = ProductController.CookedImageURL("254-170", x.HinhDaiDien, _mainDomain),
                    Url = _mainDomain + "chuyen-gia/" + x.QueryString + "-" + x.TuVanId + ""
                })
                .ToList();

            return experts.Any() ? experts : new List<HomeExpertVm>
            {
                new() { Name = "PGS.TS. Nguyễn Văn A", Title = "Chuyên gia Công nghệ sinh học", Field = "Nghiên cứu và ứng dụng công nghệ sinh học.", ImageUrl = "/images/NoImages.jpg", Url = "/chuyen-gia" },
                new() { Name = "TS. Trần Thị B", Title = "Chuyên gia Nông nghiệp công nghệ cao", Field = "Canh tác, bảo quản và chế biến nông sản.", ImageUrl = "/images/NoImages.jpg", Url = "/chuyen-gia" },
                new() { Name = "TS. Lê Hoàng C", Title = "Chuyên gia Môi trường", Field = "Xử lý môi trường và phát triển bền vững.", ImageUrl = "/images/NoImages.jpg", Url = "/chuyen-gia" }
            };
        }

        private List<TinSuKienTabVm> LoadTinSuKien()
        {
            var menus = _context.Menus
                .Where(x => TinSuKienMenus.Contains(x.MenuId))
                .OrderBy(x => x.Sort)
                .ToList();

            var result = new List<TinSuKienTabVm>();

            foreach (var m in menus)
            {
                var childIds = uspSelectSubMenu(m.MenuId);

                var items = _context.Contents
                    .Where(x => (x.MenuId == m.MenuId || childIds.Contains(x.MenuId ?? 0))
                             && x.StatusId == 3)
                    .OrderByDescending(x => x.PublishedDate)
                    .Take(6)
                    .Select(x => new TinSuKienItemVm
                    {
                        Title = x.Title,
                        Description = x.Description,
                        ImageUrl = ProductController.CookedImageURL("460-275", x.Image, _mainDomain),
                        Link = $"{_mainDomain}{x.MenuId}/{x.QueryString}-{x.Id}"
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
