using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Enums;
using TechExchangeApp.Interfaces;
using TechExchangeApp.ViewModel;

namespace TechExchangeApp.Services
{
    /// <summary>
    /// Service implementation for dashboard operations.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private const int MergedNegotiationStep = 5;
        private const int LegalReviewStep = 6;
        private const int DashboardTotalDisplaySteps = 6;
        private const string MergedStepName = "Đàm phán thương mại / Kiểm tra pháp lý hợp đồng";

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserDashboardVm> GetDashboardForUserAsync(string userId)
        {
            if (!int.TryParse(userId, out int userIdInt))
            {
                throw new ArgumentException("Invalid user ID format", nameof(userId));
            }

            var user = await _context.Users
                .Where(u => u.Id == userIdInt)
                .Select(u => new { u.FullName, u.UserName })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            var userProjectIds = await _context.ProjectMembers
                .Where(pm => pm.UserId == userIdInt && pm.IsActive)
                .Select(pm => pm.ProjectId)
                .ToListAsync();

            if (!userProjectIds.Any())
            {
                return new UserDashboardVm
                {
                    FullName = user.FullName ?? user.UserName ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    TotalProjects = 0,
                    InProgressProjects = 0,
                    WaitingForMe = 0,
                    CompletedProjects = 0,
                    Projects = new List<ProjectDashboardItemVm>()
                };
            }

            var projects = await _context.Projects
                .Where(p => userProjectIds.Contains(p.Id))
                .Select(p => new
                {
                    p.Id,
                    p.ProjectCode,
                    p.ProjectName
                })
                .ToListAsync();

            var projectMembers = await _context.ProjectMembers
                .Where(pm => userProjectIds.Contains(pm.ProjectId) && pm.UserId == userIdInt)
                .Select(pm => new
                {
                    pm.ProjectId,
                    pm.Role
                })
                .ToListAsync();

            var allSteps = await _context.ProjectSteps
                .Where(ps => userProjectIds.Contains(ps.ProjectId))
                .OrderBy(ps => ps.ProjectId)
                .ThenBy(ps => ps.StepNumber)
                .Select(ps => new StepDto
                {
                    ProjectId = ps.ProjectId,
                    StepNumber = ps.StepNumber,
                    StepName = ps.StepName,
                    StatusId = ps.StatusId
                })
                .ToListAsync();

            var stepsByProject = allSteps
                .GroupBy(s => s.ProjectId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var projectItems = new List<ProjectDashboardItemVm>();

            foreach (var project in projects)
            {
                var member = projectMembers.FirstOrDefault(pm => pm.ProjectId == project.Id);
                var steps = stepsByProject.TryGetValue(project.Id, out var projectSteps)
                    ? projectSteps
                    : new List<StepDto>();

                var displaySteps = BuildDisplaySteps(steps);
                var currentDisplayStep = displaySteps.FirstOrDefault(s => s.StatusId != 2)
                    ?? displaySteps.LastOrDefault();

                var currentStepNumber = Math.Clamp(currentDisplayStep?.DisplayStepNumber ?? 1, 1, DashboardTotalDisplaySteps);
                var currentStepName = currentDisplayStep?.StepName ?? "Bước 1";
                var currentStepStatus = currentDisplayStep?.StatusId switch
                {
                    2 => "Completed",
                    1 => "InProgress",
                    _ => "NotStarted"
                };

                var dashboardSteps = displaySteps
                    .Where(s => s.DisplayStepNumber <= DashboardTotalDisplaySteps)
                    .ToList();
                var totalDisplaySteps = DashboardTotalDisplaySteps;
                var completedSteps = Math.Min(dashboardSteps.Count(s => s.StatusId == 2), totalDisplaySteps);
                if (currentStepNumber >= totalDisplaySteps && currentStepStatus != "NotStarted")
                {
                    completedSteps = totalDisplaySteps;
                }
                var progressPercent = totalDisplaySteps > 0
                    ? (int)Math.Round((completedSteps / (double)totalDisplaySteps) * 100)
                    : 0;

                var stepsSummary = dashboardSteps
                    .Select(s => new StepMiniVm
                    {
                        StepNumber = s.DisplayStepNumber,
                        StatusId = s.StatusId
                    })
                    .ToList();

                string roleName = member?.Role switch
                {
                    (int)ProjectRole.Buyer => "Buyer",
                    (int)ProjectRole.Seller => "Seller",
                    (int)ProjectRole.Consultant => "Consultant",
                    _ => "Unknown"
                };

                projectItems.Add(new ProjectDashboardItemVm
                {
                    ProjectId = project.Id,
                    Code = project.ProjectCode,
                    Name = project.ProjectName,
                    RoleName = roleName,
                    CurrentStepNumber = currentStepNumber,
                    CurrentStepName = currentStepName,
                    CurrentStepStatus = currentStepStatus,
                    CompletedSteps = completedSteps,
                    TotalDisplaySteps = totalDisplaySteps,
                    ProgressPercent = progressPercent,
                    StepsSummary = stepsSummary
                });
            }

            int totalProjects = projectItems.Count;
            int inProgressProjects = projectItems.Count(p =>
                p.StepsSummary.Any(s => s.StatusId == 1) ||
                (p.StepsSummary.Any(s => s.StatusId == 2) && p.StepsSummary.Any(s => s.StatusId == 0)));
            int completedProjects = projectItems.Count(p =>
                p.StepsSummary.Any() && p.StepsSummary.All(s => s.StatusId == 2));
            int waitingForMe = inProgressProjects;

            return new UserDashboardVm
            {
                FullName = user.FullName ?? user.UserName ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                TotalProjects = totalProjects,
                InProgressProjects = inProgressProjects,
                WaitingForMe = waitingForMe,
                CompletedProjects = completedProjects,
                Projects = projectItems
            };
        }

        private static List<DisplayStep> BuildDisplaySteps(List<StepDto> steps)
        {
            return steps
                .Where(s => s.StepNumber != LegalReviewStep)
                .Select(s =>
                {
                    var statusId = s.StatusId;
                    var stepName = s.StepName;

                    if (s.StepNumber == MergedNegotiationStep)
                    {
                        var legalReview = steps.FirstOrDefault(x => x.StepNumber == LegalReviewStep);
                        statusId = s.StatusId == 2 && (legalReview == null || legalReview.StatusId == 2)
                            ? 2
                            : (s.StatusId > 0 || legalReview?.StatusId > 0 ? 1 : 0);
                        stepName = MergedStepName;
                    }

                    return new DisplayStep
                    {
                        DisplayStepNumber = s.StepNumber > LegalReviewStep ? s.StepNumber - 1 : s.StepNumber,
                        StepName = stepName,
                        StatusId = statusId
                    };
                })
                .ToList();
        }

        private sealed class DisplayStep
        {
            public int DisplayStepNumber { get; set; }
            public string StepName { get; set; } = string.Empty;
            public int StatusId { get; set; }
        }
    }
}
