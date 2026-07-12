namespace TechExchangeApp.ViewModel
{
    public class HuongDanArticleSummaryVm
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public string QueryString { get; set; } = "";
        public bool IsActive { get; set; }
    }

    public class HuongDanMenuVm
    {
        public int MenuId { get; set; }
        public string Title { get; set; } = "";
        public string QueryString { get; set; } = "";
        public bool IsActive { get; set; }
        public List<HuongDanArticleSummaryVm> Articles { get; set; } = new();
    }

    public class HuongDanIndexVm
    {
        public List<HuongDanMenuVm> Roles { get; set; } = new();
    }

    public class HuongDanCategoryVm
    {
        public int MenuId { get; set; }
        public string CategoryTitle { get; set; } = "";
        public string? CategoryDescription { get; set; }
        public List<HuongDanMenuVm> Sidebar { get; set; } = new();
    }

    public class HuongDanDetailVm
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? Content { get; set; }
        public int MenuId { get; set; }
        public List<HuongDanMenuVm> Sidebar { get; set; } = new();
    }
}
