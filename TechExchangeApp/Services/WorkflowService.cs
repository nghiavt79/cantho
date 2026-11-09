using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Services
{
    public interface IWorkflowService
    {
        Task InitializeProjectSteps(int projectId);
        Task CompleteStep(int projectId, int stepNumber);
        Task<bool> CanAccessStep(int projectId, int stepNumber);
        Task<List<ProjectStep>> GetProjectSteps(int projectId);
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;

        public WorkflowService(AppDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        /// <summary>Ghi nhật ký chuyển bước quy trình (HSMT A.8.5): bước trước/sau, người thực hiện, IP, trình duyệt.</summary>
        private void AddTransitionLog(int projectId, int fromStep, int toStep, string action, string message)
        {
            var ctx = _http.HttpContext;
            int.TryParse(ctx?.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var actor);
            if (actor <= 0) return; // ActorUserId có khóa ngoại tới Users: chỉ ghi khi xác định được người thực hiện
            var ua = ctx?.Request.Headers.UserAgent.ToString();
            _context.WorkflowTransitionLogs.Add(new WorkflowTransitionLog
            {
                ProjectId = projectId,
                FromStep = fromStep,
                ToStep = toStep,
                Action = action,
                ActorUserId = actor,
                IpAddress = ctx?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = ua is { Length: > 300 } ? ua[..300] : ua,
                Message = message,
                CreatedAt = DateTime.UtcNow
            });
            if (ctx != null)
                _context.Logs.Add(LoginAuditLog.Build(ctx, ctx.User?.Identity?.Name, LoginAuditLog.EventUpdate,
                    $"Hồ sơ #{projectId}: {message}"));
        }

        public async Task InitializeProjectSteps(int projectId)
        {
            var steps = new List<ProjectStep>
            {
                new ProjectStep { ProjectId = projectId, StepNumber = 1, StepName = "Yêu cầu chuyển giao công nghệ", StatusId = 1 }, // InProgress
                new ProjectStep { ProjectId = projectId, StepNumber = 2, StepName = "Thỏa thuận bảo mật (NDA)", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 3, StepName = "Yêu cầu báo giá (RFQ)", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 4, StepName = "Nộp hồ sơ đề xuất", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 5, StepName = "Đàm phán thương mại", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 6, StepName = "Kiểm tra pháp lý", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 7, StepName = "Ký hợp đồng", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 8, StepName = "Xác nhận tạm ứng", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 9, StepName = "Thử nghiệm Pilot", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 10, StepName = "Bàn giao & triển khai thiết bị", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 11, StepName = "Đào tạo & chuyển giao vận hành", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 12, StepName = "Bàn giao hồ sơ kỹ thuật", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 13, StepName = "Nghiệm thu", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 14, StepName = "Thanh lý hợp đồng", StatusId = 0 }
            };

            await _context.ProjectSteps.AddRangeAsync(steps);
            await _context.SaveChangesAsync();
        }

        public async Task CompleteStep(int projectId, int stepNumber)
        {
            var currentStep = await _context.ProjectSteps
                .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.StepNumber == stepNumber);

            if (currentStep != null && currentStep.StatusId != 2)
            {
                currentStep.StatusId = 2; // Completed
                currentStep.CompletedDate = DateTime.Now;

                // Unlock next step
                var nextStep = await _context.ProjectSteps
                    .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.StepNumber == stepNumber + 1);
                
                if (nextStep != null)
                {
                    nextStep.StatusId = 1; // InProgress
                }

                // L2-04: hồ sơ không áp dụng NDA thì bước 1 chuyển thẳng sang bước 3 (RFQ), ghi nhật ký đúng như vậy
                var message = nextStep != null
                    ? $"Hoàn thành bước {stepNumber} – {currentStep.StepName}; mở bước {nextStep.StepNumber} – {nextStep.StepName}"
                    : $"Hoàn thành bước {stepNumber} – {currentStep.StepName}";
                var toStep = nextStep?.StepNumber ?? stepNumber;
                if (stepNumber == 1 && nextStep != null && !await ProjectRequiresNda(projectId))
                {
                    var rfqStep = await _context.ProjectSteps.AsNoTracking()
                        .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.StepNumber == 3);
                    message = $"Hoàn thành bước 1 – {currentStep.StepName}; bước 2 – {nextStep.StepName}: không áp dụng; "
                        + $"mở bước 3 – {rfqStep?.StepName ?? "Yêu cầu báo giá (RFQ)"}";
                    toStep = 3;
                }

                AddTransitionLog(projectId, stepNumber, toStep, "Complete", message);

                await _context.SaveChangesAsync();
            }
        }

        // Cùng quy tắc với ProjectController.GetProjectRequiresNda / ESignGateway.ProjectRequiresNda
        private async Task<bool> ProjectRequiresNda(int projectId)
        {
            var tech = await _context.TechTransferRequests.AsNoTracking()
                .Where(t => t.ProjectId == projectId)
                .Select(t => new { t.FromId, t.TypeData })
                .FirstOrDefaultAsync();
            if (tech?.FromId == null || (tech.TypeData ?? 1) != 1) return false;

            return await _context.SanPhamCNTBs.AsNoTracking()
                .Where(p => p.ID == tech.FromId.Value)
                .Select(p => p.RequiresNDA)
                .FirstOrDefaultAsync() == true;
        }

        public async Task<bool> CanAccessStep(int projectId, int stepNumber)
        {
            // Step 1 check logic (always allowed if project exists? or implicit)
            if (stepNumber == 1) return true;

            var step = await _context.ProjectSteps
                .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.StepNumber == stepNumber);

            // Access allowed if NotStarted (0) is FALSE. 
            // Only InProgress (1) or Completed (2) allow access (to view/edit).
            // Actually, requirements say: "Nếu StatusId = NotStarted -> return false"
            
            if (step == null) return false;
            return step.StatusId != 0;
        }

        public async Task<List<ProjectStep>> GetProjectSteps(int projectId)
        {
            return await _context.ProjectSteps
                .Where(x => x.ProjectId == projectId)
                .OrderBy(x => x.StepNumber)
                .ToListAsync();
        }
    }
}
