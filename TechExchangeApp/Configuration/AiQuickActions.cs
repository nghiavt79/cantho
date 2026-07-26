namespace TechExchangeApp.Configuration
{
    public enum AiQuickActionKind { Browse, Action }

    /// <summary>
    /// A chatbox quick-action button. Routing is keyed by <see cref="Id"/> (stable, never shown
    /// to the user) rather than <see cref="Title"/> (display copy, free to change anytime)
    /// so relabeling a button in the UI can never silently break backend routing.
    /// </summary>
    public record AiQuickAction(
        string Id,
        string Title,
        AiQuickActionKind Kind,
        IReadOnlyList<string>? TypeNames = null,
        string? ReplyText = null,
        bool NeedsContactInfo = false,
        string? ResultsHeader = null);

    /// <summary>
    /// Single source of truth for chatbox quick actions. Adding a new action, regrouping which
    /// TypeNames a "browse" button covers, or changing display copy is a one-place edit here —
    /// nothing else in the codebase needs to change.
    /// </summary>
    public static class AiQuickActions
    {
        public static readonly IReadOnlyList<AiQuickAction> All = new[]
        {
            new AiQuickAction("browse-congnghe", "Công nghệ", AiQuickActionKind.Browse,
                TypeNames: new[] { "Công nghệ" },
                ResultsHeader: "Đây là một số công nghệ mới nhất trên website:"),

            new AiQuickAction("browse-cntb", "Thiết bị & sản phẩm", AiQuickActionKind.Browse,
                TypeNames: new[] { "Công nghệ", "Thiết bị", "Tài sản trí tuệ" },
                ResultsHeader: "Đây là một số sản phẩm mới nhất trên website:"),

            new AiQuickAction("browse-chuyengia", "Chuyên gia", AiQuickActionKind.Browse,
                TypeNames: new[] { "Chuyên gia" },
                ResultsHeader: "Đây là một số chuyên gia tư vấn mới nhất trên website:"),

            new AiQuickAction("browse-nhacungung", "Nhà cung ứng", AiQuickActionKind.Browse,
                TypeNames: new[] { "Nhà cung ứng" },
                ResultsHeader: "Đây là một số nhà cung ứng mới nhất trên website:"),

            new AiQuickAction("action-hotro", "Gửi yêu cầu tư vấn", AiQuickActionKind.Action,
                ReplyText: "Anh/chị vui lòng để lại họ tên, số điện thoại hoặc email bên dưới, trung tâm sẽ liên hệ hỗ trợ sớm nhất.",
                NeedsContactInfo: true),
        };

        public static AiQuickAction? FindById(string? id) =>
            string.IsNullOrWhiteSpace(id) ? null : All.FirstOrDefault(a => a.Id == id);
    }
}
