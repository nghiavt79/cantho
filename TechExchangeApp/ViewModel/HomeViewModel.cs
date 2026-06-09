namespace TechExchangeApp.ViewModel
{
    public class HomeViewModel
    {
        public string? CongNgheMoiCapNhatHtml { get; set; }
        public string? ProductCNMoiCapNhatHtml { get; set; }

        public List<TinSuKienTabVm>? TinSuKien { get; set; }
        public List<VideoVm>? VideoCongNghe { get; set; }
        public YeuCauCongNgheVm? YeuCauCongNghe { get; set; }

        public HomeAnalyticsVm Analytics { get; set; } = new();
        public HomeBrandingVm Branding { get; set; } = new();
        public List<HomeStatVm> Stats { get; set; } = new();
        public List<HomeFeatureVm> WhatWeDo { get; set; } = new();
        public List<HomeFeatureVm> Services { get; set; } = new();
        public List<HomeFeatureVm> Reasons { get; set; } = new();
        public List<HomeProcessStepVm> ProcessSteps { get; set; } = new();
        public List<HomeTechCardVm> FeaturedTechnologies { get; set; } = new();
        public List<HomeNewsCardVm> FeaturedNews { get; set; } = new();
        public List<HomeExpertVm> Experts { get; set; } = new();
        public List<HomePartnerVm> Partners { get; set; } = new();
        public List<string> PopularTags { get; set; } = new();
    }

    public class HomeBrandingVm
    {
        public string ShortName { get; set; } = "";
        public string OrganizationName { get; set; } = "";
        public string Tagline { get; set; } = "";
        public string ProvinceName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string WorkingHours { get; set; } = "";
    }

    public class HomeStatVm
    {
        public string Icon { get; set; } = "";
        public string Value { get; set; } = "";
        public string Label { get; set; } = "";
    }

    public class HomeFeatureVm
    {
        public string Icon { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Url { get; set; } = "#";
    }

    public class HomeProcessStepVm
    {
        public int Number { get; set; }
        public string Icon { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class HomeTechCardVm
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Url { get; set; } = "#";
        public string Category { get; set; } = "Công nghệ";
    }

    public class HomeNewsCardVm
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Url { get; set; } = "#";
        public DateTime? PublishedDate { get; set; }
    }

    public class HomeExpertVm
    {
        public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public string Field { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Url { get; set; } = "#";
    }

    public class HomePartnerVm
    {
        public string Name { get; set; } = "";
        public string LogoUrl { get; set; } = "";
    }
}
