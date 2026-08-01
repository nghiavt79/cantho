using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Enums;
using TechExchangeApp.Interfaces;

namespace TechExchangeApp.Controllers
{
    /// <summary>
    /// Thành viên dự án tạo ticket hỗ trợ/tư vấn Trung tâm ở bất kỳ bước nào.
    /// Tái dùng hạ tầng Chat (ChatConversation type=2), mỗi yêu cầu là một ticket riêng.
    /// </summary>
    [Authorize]
    public class SupportController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IChatService _chatService;
        private readonly IESignGateway _eSignGateway;

        public SupportController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IChatService chatService,
            IESignGateway eSignGateway)
        {
            _context = context;
            _userManager = userManager;
            _chatService = chatService;
            _eSignGateway = eSignGateway;
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            var u = await _userManager.GetUserAsync(User);
            return u?.Id ?? 0;
        }

        // POST /api/support/start
        [HttpPost("/api/support/start")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start([FromBody] SupportStartRequest request)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "Vui lòng đăng nhập." });

            if (request == null || request.ProjectId <= 0)
                return BadRequest(new { success = false, error = "Thiếu thông tin dự án." });

            var project = await _context.Projects.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId);
            if (project == null)
                return NotFound(new { success = false, error = "Không tìm thấy dự án." });

            if (!await CanAccessProjectAsync(project, userId))
                return StatusCode(403, new { success = false, error = "Bạn không có quyền truy cập dự án này." });

            int step = request.StepNumber.GetValueOrDefault(1);
            if (step < 1 || step > 14) step = 1;

            var message = (request.Message ?? request.Description ?? "").Trim();
            if (message.Length > 2000) message = message.Substring(0, 2000);
            if (string.IsNullOrWhiteSpace(message))
                return BadRequest(new { success = false, error = "Vui lòng nhập nội dung cần hỗ trợ." });

            int requestType = request.RequestType == (int)SupportRequestType.TransactionConsulting
                ? (int)SupportRequestType.TransactionConsulting
                : (int)SupportRequestType.GeneralSupport;
            int? serviceType = null;
            if (requestType == (int)SupportRequestType.TransactionConsulting)
            {
                serviceType = request.ServiceType;
                var validServiceTypes = new[]
                {
                    (int)SupportServiceType.SupplierMatching,
                    (int)SupportServiceType.ProposalEvaluation,
                    (int)SupportServiceType.NegotiationAdvice,
                    (int)SupportServiceType.LegalReview,
                    (int)SupportServiceType.ElectronicContractSupport,
                    (int)SupportServiceType.Other
                };
                if (!serviceType.HasValue || !validServiceTypes.Contains(serviceType.Value))
                    return BadRequest(new { success = false, error = "Vui lòng chọn loại tư vấn hợp lệ." });
            }

            var conversationId = await _chatService.StartSupportConversationAsync(
                request.ProjectId,
                userId,
                new SupportStartOptions
                {
                    StepNumber = step,
                    RequestType = requestType,
                    ServiceType = serviceType,
                    SupportContextCode = request.SupportContextCode,
                    DisplayStepNumber = step,
                    InternalStepNumber = step,
                    Subject = request.Subject,
                    Description = message
                });

            return Ok(new { success = true, conversationId, redirectUrl = $"/chat/{conversationId}" });
        }

        /// <summary>
        /// Mirror đúng logic truy cập của ProjectController.Details:
        /// creator / ProjectMembers active / SelectedSeller / invited seller (RFQInvitations active + đã ký NDA).
        /// Consultant được thêm dưới dạng ProjectMembers Role=3 nên đã nằm trong nhánh "member active".
        /// </summary>
        private async Task<bool> CanAccessProjectAsync(Project project, int userId)
        {
            if (project.CreatedBy == userId) return true;

            bool isMember = await _context.ProjectMembers
                .AnyAsync(m => m.ProjectId == project.Id && m.UserId == userId && m.IsActive);
            if (isMember) return true;

            if (project.SelectedSellerId == userId) return true;

            bool invited = await _context.RFQInvitations
                .AnyAsync(i => i.ProjectId == project.Id && i.SellerId == userId && i.IsActive);
            if (invited)
            {
                // Invited seller non-selected: Details yêu cầu đã ký NDA. API trả 403 nếu chưa ký (không redirect).
                var ndaSigned = await _eSignGateway.HasUserSignedProjectNda(project.Id, userId);
                if (ndaSigned) return true;
            }

            return false;
        }
    }

    public class SupportStartRequest
    {
        public int ProjectId { get; set; }
        public int? StepNumber { get; set; }
        public int RequestType { get; set; } = 1;
        public int? ServiceType { get; set; }
        public string? SupportContextCode { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public string Message { get; set; } = "";
    }
}
