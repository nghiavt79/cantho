using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    [Table("SysFunction")]
    public class SysFunction
    {
        [Key]
        public int FunctionId { get; set; }
        public string? FunctionName { get; set; }
        public string? URL { get; set; }
        public bool? IsMenu { get; set; }
        public bool? IsStatus { get; set; }
        public int? LanguageId { get; set; }
        public bool IsShow { get; set; }
        public string? Domain { get; set; }
        public string? HrefName { get; set; }
        public int? ParentId { get; set; }
        public int? Sort { get; set; }
        public int? SiteId { get; set; }
    }
}
