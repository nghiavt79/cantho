using Microsoft.AspNetCore.Mvc;
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
            return Ok(_chatService.GetSuggestions());
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
                var response = await _chatService.ReplyAsync(request, cancellationToken);
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
                    Message = "He thong AI chat dang gap loi khi xu ly yeu cau. Anh/chi vui long de lai thong tin de trung tam lien he ho tro."
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
                    Message = "Vui long nhap so dien thoai hoac email de trung tam lien he."
                });
            }

            try
            {
                await _feedbackService.SaveAsync(request, cancellationToken);
                return Ok(new AiChatFeedbackResponse
                {
                    Success = true,
                    Message = "Thong tin cua anh/chi da duoc ghi nhan. Trung tam se lien he ho tro trong thoi gian som nhat."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot save AI chat feedback.");
                return StatusCode(StatusCodes.Status500InternalServerError, new AiChatFeedbackResponse
                {
                    Success = false,
                    Message = "Chua the luu thong tin. Anh/chi vui long thu lai sau."
                });
            }
        }
    }
}
