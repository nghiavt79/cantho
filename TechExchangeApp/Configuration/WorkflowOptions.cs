namespace TechExchangeApp.Configuration
{
    /// <summary>
    /// Cấu hình quy trình giao dịch chuyển giao công nghệ.
    /// Bộ KHCN quy định 14 bước; Chủ đầu tư có thể chọn số bước hiển thị (ví dụ 7, 10 hoặc 14)
    /// mà không cần chỉnh sửa mã nguồn. Các bước vượt quá số hiển thị vẫn được lưu vết đầy đủ.
    /// </summary>
    public class WorkflowOptions
    {
        public const string SectionName = "Workflow";

        /// <summary>Tổng số bước theo quy định (cố định).</summary>
        public const int TotalSteps = 14;

        /// <summary>Số bước hiển thị cho người dùng. Mặc định 7 (đến khâu ký hợp đồng).</summary>
        public int VisibleStepCount { get; set; } = 7;

        /// <summary>Số bước hiển thị đã được giới hạn trong khoảng hợp lệ [1, 14].</summary>
        public int EffectiveVisibleStepCount => VisibleStepCount < 1 ? 1 : (VisibleStepCount > TotalSteps ? TotalSteps : VisibleStepCount);

        /// <summary>Bước có được hiển thị cho người dùng hay không, theo số bước Chủ đầu tư đã chọn.</summary>
        public bool IsStepVisible(int stepNumber) => stepNumber <= EffectiveVisibleStepCount;
    }
}
