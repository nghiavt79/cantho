namespace TechExchangeApp.Configuration
{
    public class AiChatOptions
    {
        public const string SectionName = "AiChat";

        public bool IsEnabled { get; set; } = true;
        public string BotName { get; set; } = "AI Support";
        public string WelcomeMessage { get; set; } = "Xin chào, tôi có thể hỗ trợ anh/chị tìm công nghệ, sản phẩm CNTB, nhà cung ứng hoặc gửi yêu cầu tư vấn.";
        public string SystemPrompt { get; set; } = "Bạn là trợ lý AI của Sàn giao dịch Công nghệ thành phố Cần Thơ. Chỉ trả lời dựa trên dữ liệu website được cung cấp. Nếu chưa có dữ liệu phù hợp, hãy đề nghị người dùng để lại thông tin tư vấn.";
        public string ModelName { get; set; } = "gpt-4o-mini";
        public bool UseOpenAiResponses { get; set; } = false;
        public int MaxContextItems { get; set; } = 6;
        public int MaxMessagesPerSession { get; set; } = 30;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
