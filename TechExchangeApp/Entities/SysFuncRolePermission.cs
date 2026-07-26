using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    /// <summary>
    /// Bảng phân quyền legacy: mỗi dòng = 1 Role được phép thao tác 1 SysFunction
    /// khi nội dung ở 1 Status nhất định. StatusId = 0 nghĩa là quyền chung
    /// (function không theo trạng thái). Khóa chính tổ hợp (FunctionId, RoleId, StatusId).
    /// </summary>
    [Table("SysFuncRolesStatusPermission")]
    public class SysFuncRolePermission
    {
        public int FunctionId { get; set; }
        public int RoleId { get; set; }
        public byte StatusId { get; set; }
        public int? AuditNumber { get; set; }
        public string? Domain { get; set; }
        public int? LanguageId { get; set; }
        public int? ParentId { get; set; }
        public int? SiteId { get; set; }
    }
}
