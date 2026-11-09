using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Interfaces;
using TechExchangeApp.ViewModel;

namespace TechExchangeApp.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private const int MergedNegotiationStep = 5;
        private const int LegalReviewStep = 6;
        private readonly TechExchangeApp.Configuration.WorkflowOptions _workflowOptions;

        public ProjectService(AppDbContext context,
            Microsoft.Extensions.Options.IOptions<TechExchangeApp.Configuration.WorkflowOptions> workflowOptions)
        {
            _context = context;
            _workflowOptions = workflowOptions.Value;
        }

        public async Task<List<MyProjectVm>> GetMyProjectsAsync(int userId)
        {
            var projects = await _context.ProjectMembers
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .Include(pm => pm.Project)
                .Select(pm => new
                {
                    pm.Project.Id,
                    pm.Project.ProjectCode,
                    pm.Project.ProjectName,
                    pm.Role,
                    pm.Project.StatusId,
                    pm.Project.CreatedDate
                })
                .ToListAsync();

            var result = new List<MyProjectVm>();

            foreach (var p in projects)
            {
                var allSteps = await _context.ProjectSteps
                    .Where(ps => ps.ProjectId == p.Id)
                    .OrderBy(ps => ps.StepNumber)
                    .Select(ps => new { ps.StepNumber, ps.StatusId })
                    .ToListAsync();

                // Theo Workflow:VisibleStepCount như trang chi tiết hồ sơ; bước 5–6 gộp một dòng, đánh số theo bước quy trình
                var displaySteps = allSteps
                    .Where(s => s.StepNumber != LegalReviewStep && _workflowOptions.IsStepVisible(s.StepNumber))
                    .Select(s =>
                    {
                        var statusId = s.StatusId;
                        if (s.StepNumber == MergedNegotiationStep)
                        {
                            var legalReview = allSteps.FirstOrDefault(x => x.StepNumber == LegalReviewStep);
                            statusId = s.StatusId == 2 && (legalReview == null || legalReview.StatusId == 2)
                                ? 2
                                : (s.StatusId > 0 || legalReview?.StatusId > 0 ? 1 : 0);
                        }

                        return new
                        {
                            InternalStepNumber = s.StepNumber,
                            DisplayStepNumber = s.StepNumber,
                            StatusId = statusId
                        };
                    })
                    .ToList();

                int totalDisplaySteps = _workflowOptions.EffectiveVisibleStepCount;
                int completedSteps = Math.Min(
                    allSteps.Count(s => _workflowOptions.IsStepVisible(s.StepNumber) && s.StatusId == 2), totalDisplaySteps);

                int currentStep;
                int currentStepStatus;
                if (!allSteps.Any())
                {
                    currentStep = 1;
                    currentStepStatus = 0;
                }
                else
                {
                    var currentDisplayStep = displaySteps.FirstOrDefault(s => s.StatusId != 2)
                        ?? displaySteps.LastOrDefault();
                    currentStep = currentDisplayStep?.DisplayStepNumber ?? 1;
                    currentStepStatus = currentDisplayStep?.StatusId ?? 0;
                }
                currentStep = Math.Clamp(currentStep, 1, totalDisplaySteps);
                if (currentStep >= totalDisplaySteps && currentStepStatus > 0)
                {
                    completedSteps = totalDisplaySteps;
                }

                int progress = totalDisplaySteps > 0 ? (int)Math.Round((completedSteps / (double)totalDisplaySteps) * 100) : 0;

                result.Add(new MyProjectVm
                {
                    Id = p.Id,
                    Code = p.ProjectCode,
                    Name = p.ProjectName,
                    RoleId = p.Role,
                    Role = GetRoleName(p.Role),
                    CurrentStep = currentStep,
                    StatusId = p.StatusId,
                    Status = GetStatusName(p.StatusId),
                    ProgressPercent = progress,
                    TotalDisplaySteps = totalDisplaySteps,
                    CreatedDate = p.CreatedDate
                });
            }

            return result.OrderByDescending(p => p.CreatedDate).ToList();
        }

        public async Task<int> GetProjectCountAsync(int userId)
        {
            return await _context.ProjectMembers
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .CountAsync();
        }

        private string GetRoleName(int roleId)
        {
            return roleId switch
            {
                1 => "Buyer",
                2 => "Seller",
                3 => "Consultant",
                _ => "Unknown"
            };
        }

        private string GetStatusName(int statusId)
        {
            return statusId switch
            {
                1 => "Draft",
                2 => "Active",
                3 => "Completed",
                4 => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}
