namespace TechExchangeApp.ViewModel
{
    public class ViolationReportDto
    {
        public string? TargetDataType { get; set; }
        public string? TargetSubType { get; set; }
        public int TargetId { get; set; }
        public string? TargetTitle { get; set; }
        public string? TargetUrl { get; set; }

        public string? ViolationType { get; set; }
        public string? ReportContent { get; set; }

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
