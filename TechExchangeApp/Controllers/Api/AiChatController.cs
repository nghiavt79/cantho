using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechExchangeApp.Configuration;
using TechExchangeApp.Models;
using TechExchangeApp.Services;

namespace TechExchangeApp.Controllers.Api
{
    [ApiController]
    [Route("api/ai-chat")]
    public class AiChatController : ControllerBase
    {
        private readonly IAiChatService _chatService;
        private readonly IAiFeedbackService _feedbackService;
        private readonly ILogger<AiChatController> _logger;

        public AiChatController(
            IAiChatService chatService,
            IAiFeedbackService feedbackService,
            ILogger<AiChatController> logger)
        {
            _chatService = chatService;
            _feedbackService = feedbackService;
            _logger = logger;
        }

        [HttpGet("suggestions")]
        public IActionResult Suggestions()
        {
            return Ok(AiQuickActions.All.Select(a => new { a.Id, a.Title }));
        }

        [HttpGet("history")]
        public async Task<ActionResult<IReadOnlyList<AiChatHistoryItem>>> History(string sessionKey, CancellationToken cancellationToken)
        {
            var items = await _chatService.GetHistoryAsync(sessionKey, cancellationToken);
            return Ok(items);
        }

        [HttpPost("message")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<AiChatMessageResponse>> Message(
            [FromBody] AiChatMessageRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : (int?)null;
                var response = await _chatService.ReplyAsync(request, userId, cancellationToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot process AI chat message.");
                return StatusCode(StatusCodes.Status500InternalServerError, new AiChatMessageResponse
                {
                    Success = false,
                    SessionKey = string.IsNullOrWhiteSpace(request.SessionKey) ? Guid.NewGuid().ToString("N") : request.SessionKey,
                    NeedsContactInfo = true,
                    Message = "Hệ thống AI chat đang gặp lỗi khi xử lý yêu cầu. Anh/chị vui lòng để lại thông tin để trung tâm liên hệ hỗ trợ."
                });
            }
        }

        [HttpPost("submit-feedback")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<AiChatFeedbackResponse>> SubmitFeedback(
            [FromBody] AiChatFeedbackRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Phone) && string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new AiChatFeedbackResponse
                {
                    Success = false,
                    Message = "Vui lòng nhập số điện thoại hoặc email để trung tâm liên hệ."
                });
            }

            try
            {
                await _feedbackService.SaveAsync(request, cancellationToken);
                return Ok(new AiChatFeedbackResponse
                {
                    Success = true,
                    Message = "Thông tin của anh/chị đã được ghi nhận. Trung tâm sẽ liên hệ hỗ trợ trong thời gian sớm nhất."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot save AI chat feedback.");
                return StatusCode(StatusCodes.Status500InternalServerError, new AiChatFeedbackResponse
                {
                    Success = false,
                    Message = "Chưa thể lưu thông tin. Anh/chị vui lòng thử lại sau."
                });
            }
        }
    }
}
