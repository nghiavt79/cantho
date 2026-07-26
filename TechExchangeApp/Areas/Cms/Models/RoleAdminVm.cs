using System.ComponentModel.DataAnnotations;

namespace TechExchangeApp.Areas.Cms.Models
{
    public class RoleListItemVm
    {
        public int RoleId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? SiteId { get; set; }
        public int? LanguageId { get; set; }
        public int? ParentId { get; set; }
        public string? Domain { get; set; }
        public int PermissionCount { get; set; }
    }

    public class RoleEditVm
    {
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Tên quyền (Title) không được trống.")]
        [StringLength(500, ErrorMessage = "Title tối đa 500 ký tự.")]
        public string Title { get; set; } = "";

        [StringLength(2000, ErrorMessage = "Description tối đa 2000 ký tự.")]
        public string? Description { get; set; }

        [StringLength(500, ErrorMessage = "Domain tối đa 500 ký tự.")]
        public string? Domain { get; set; }

        public int? LanguageId { get; set; }
        public int? ParentId { get; set; }
        public int? SiteId { get; set; }
    }

    public class RoleCreateVm : RoleEditVm
    {
    }
}
