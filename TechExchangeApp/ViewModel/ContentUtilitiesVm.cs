namespace TechExchangeApp.ViewModel
{
    public class ContentUtilitiesVm
    {
        public string TargetDataType { get; set; } = "";
        public string TargetSubType { get; set; } = "";
        public long TargetId { get; set; }
        public string TargetTitle { get; set; } = "";
        public string TargetUrl { get; set; } = "";
        public string ShareUrl { get; set; } = "";
        public string ShareTitle { get; set; } = "";
        public bool ShowReport { get; set; } = true;
        public bool ShowShare { get; set; } = true;
        public bool ShowPrint { get; set; } = true;
    }
}
