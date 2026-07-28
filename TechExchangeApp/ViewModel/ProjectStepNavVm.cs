namespace TechExchangeApp.ViewModel
{
    public class ProjectStepNavVm
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public bool IsCurrent { get; set; }
        public string ControllerName { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public bool IsAccessible { get; set; }
        public bool IsVisible { get; set; } = true;
        /// <summary>Bước không áp dụng cho dự án này (ví dụ NDA khi sản phẩm không yêu cầu bảo mật).</summary>
        public bool IsNotApplicable { get; set; }
    }
}
