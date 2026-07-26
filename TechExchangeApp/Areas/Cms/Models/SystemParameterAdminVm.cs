using System.ComponentModel.DataAnnotations;

namespace TechExchangeApp.Areas.Cms.Models
{
    public class SystemParameterListItemVm
    {
        public int ID { get; set; }
        public string Name { get; set; } = "";
        public string? Val { get; set; }
        public string? Val2 { get; set; }
        public int? Type { get; set; }
        public string? Description { get; set; }
        public bool? IsSystem { get; set; }
        public bool Activated { get; set; }
        public string? Domain { get; set; }
        public int? LanguageId { get; set; }
        public int? SiteId { get; set; }
        public bool IsSensitive { get; set; }
        public string TypeLabel { get; set; } = "Text";
        public string DisplayVal { get; set; } = "";
        public string DisplayVal2 { get; set; } = "";
    }

    public class SystemParameterEditVm
    {
        public int ID { get; set; }
        public string Name { get; set; } = "";

        [StringLength(50, ErrorMessage = "Val tối đa 50 ký tự.")]
        public string? Val { get; set; }

        [StringLength(50, ErrorMessage = "Val2 tối đa 50 ký tự.")]
        public string? Val2 { get; set; }

        public int? Type { get; set; }

        [StringLength(200, ErrorMessage = "Mô tả tối đa 200 ký tự.")]
        public string? Description { get; set; }

        public bool Activated { get; set; }
        public bool? IsSystem { get; set; }
        public string? Domain { get; set; }
        public int? LanguageId { get; set; }
        public int? SiteId { get; set; }
        public bool IsSensitive { get; set; }
        public string TypeLabel { get; set; } = "Text";
        public string? CurrentMaskedVal { get; set; }
    }
}
