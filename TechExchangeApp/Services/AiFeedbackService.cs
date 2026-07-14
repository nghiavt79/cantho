using Microsoft.Extensions.Options;
using TechExchangeApp.Configuration;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Models;

namespace TechExchangeApp.Services
{
    public interface IAiFeedbackService
    {
        Task SaveAsync(AiChatFeedbackRequest request, CancellationToken cancellationToken = default);
    }

    public class AiFeedbackService : IAiFeedbackService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AiFeedbackService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task SaveAsync(AiChatFeedbackRequest request, CancellationToken cancellationToken = default)
        {
            var mainDomain = _configuration["AppSettings:MainDomain"] ?? "https://localhost:7232/";
            var domain = Uri.TryCreate(mainDomain, UriKind.Absolute, out var uri) ? uri.Host : "techport.vn";

            var feedback = new Feedback
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Title = "Yêu cầu hỗ trợ từ AI Chat Box",
                Content = BuildContent(request),
                Created = DateTime.Now,
                StatusId = 2,
                SiteId = 1,
                Domain = domain,
                Creator = "AIChat"
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private static string BuildContent(AiChatFeedbackRequest request)
        {
            var lines = new List<string>
            {
                "Nguồn: AI Support Chat Box"
            };

            if (!string.IsNullOrWhiteSpace(request.SessionKey))
            {
                lines.Add($"Session: {request.SessionKey}");
            }

            if (!string.IsNullOrWhiteSpace(request.Message))
            {
                lines.Add("Nội dung:");
                lines.Add(request.Message.Trim());
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
