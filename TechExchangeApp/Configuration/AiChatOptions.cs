namespace TechExchangeApp.Configuration
{
    public class AiChatOptions
    {
        public const string SectionName = "AiChat";

        public bool IsEnabled { get; set; } = true;
        public string BotName { get; set; } = "AI Support";
        public string WelcomeMessage { get; set; } = "Xin chao, toi co the ho tro anh/chi tim cong nghe, san pham CNTB, nha cung ung hoac gui yeu cau tu van.";
        public string SystemPrompt { get; set; } = "Ban la tro ly AI cua San giao dich Cong nghe thanh pho Can Tho. Chi tra loi dua tren du lieu website duoc cung cap. Neu chua co du lieu phu hop, hay de nghi nguoi dung de lai thong tin tu van.";
        public string ModelName { get; set; } = "gpt-4o-mini";
        public bool UseOpenAiResponses { get; set; } = false;
        public int MaxContextItems { get; set; } = 6;
        public int MaxMessagesPerSession { get; set; } = 30;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
