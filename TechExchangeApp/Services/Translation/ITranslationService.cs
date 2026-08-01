namespace TechExchangeApp.Services.Translation
{
    /// <summary>Dịch cấp cao: đoạn thuần và HTML (giữ nguyên thẻ, chỉ dịch text node).</summary>
    public interface ITranslationService
    {
        string ProviderName { get; }
        Task<string?> TranslatePlainAsync(string? vi, CancellationToken ct = default);
        Task<string?> TranslateHtmlAsync(string? viHtml, CancellationToken ct = default);
    }
}
