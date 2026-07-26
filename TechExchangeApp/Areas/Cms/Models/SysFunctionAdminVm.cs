using System.ComponentModel.DataAnnotations;

namespace TechExchangeApp.Areas.Cms.Models
{
    public class SysFunctionListItemVm
    {
        public int FunctionId { get; set; }
        public string? FunctionName { get; set; }
        public string? URL { get; set; }
        public bool? IsMenu { get; set; }
        public bool? IsStatus { get; set; }
        public bool IsShow { get; set; }
        public string? HrefName { get; set; }
        public int? ParentId { get; set; }
        public int? Sort { get; set; }
        public int? SiteId { get; set; }
        public string? Domain { get; set; }
        public int? LanguageId { get; set; }
    }

    public class SysFunctionEditVm
    {
        public int FunctionId { get; set; }

        [Required(ErrorMessage = "FunctionName không được trống.")]
        [StringLength(500, ErrorMessage = "FunctionName tối đa 500 ký tự.")]
        public string FunctionName { get; set; } = "";

        [StringLength(300, ErrorMessage = "URL tối đa 300 ký tự.")]
        public string? URL { get; set; }

        public bool? IsMenu { get; set; }
        public bool? IsStatus { get; set; }
        public bool IsShow { get; set; }

        [StringLength(500, ErrorMessage = "Domain tối đa 500 ký tự.")]
        public string? Domain { get; set; }

        [StringLength(100, ErrorMessage = "HrefName tối đa 100 ký tự.")]
        public string? HrefName { get; set; }

        public int? ParentId { get; set; }
        public int? Sort { get; set; }
        public int? SiteId { get; set; }
        public int? LanguageId { get; set; }
    }

    public class SysFunctionCreateVm : SysFunctionEditVm
    {
    }
}
