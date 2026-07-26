namespace TechExchangeApp.Areas.Cms.Models
{
    public class LogListItemVm
    {
        public int LogID { get; set; }
        public int? FunctionID { get; set; }
        public string? FunctionName { get; set; }
        public DateTime? ActTime { get; set; }
        public int? EventID { get; set; }
        public string? EventName { get; set; }
        public string? Content { get; set; }
        public string? ClientIP { get; set; }
        public string? UserName { get; set; }
        public string? Domain { get; set; }
        public int? LanguageId { get; set; }
        public int? SiteId { get; set; }
    }
}
