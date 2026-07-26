namespace TechExchangeApp.Areas.Cms.Models
{
    /// <summary>Một cột trạng thái trong ma trận phân quyền.</summary>
    public class PermissionStatusColumn
    {
        public int StatusId { get; set; }
        public string Title { get; set; } = "";
    }

    /// <summary>Một dòng (SysFunction) trong ma trận phân quyền của 1 Role.</summary>
    public class SysFuncPermissionRowVm
    {
        public int FunctionId { get; set; }
        public string? FunctionName { get; set; }
        public string? HrefName { get; set; }
        public string? URL { get; set; }
        public bool IsStatusBased { get; set; }
        public int? ParentId { get; set; }
        /// <summary>Các StatusId mà Role hiện đang được cấp quyền cho function này.</summary>
        public HashSet<int> GrantedStatusIds { get; set; } = new();
    }

    /// <summary>ViewModel cho trang ma trận phân quyền theo Role.</summary>
    public class SysFuncPermissionMatrixVm
    {
        public int? SelectedRoleId { get; set; }
        public string? SelectedRoleTitle { get; set; }
        public List<(int RoleId, string Title)> Roles { get; set; } = new();
        public List<PermissionStatusColumn> StatusColumns { get; set; } = new();
        public List<SysFuncPermissionRowVm> Rows { get; set; } = new();
    }
}
