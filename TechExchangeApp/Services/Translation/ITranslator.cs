namespace TechExchangeApp.Services.Translation
{
    /// <summary>Bộ dịch cấp thấp: dịch một lô đoạn văn thuần VI→EN, trả về đúng số phần tử, đúng thứ tự.</summary>
    public interface ITranslator
    {
        string Name { get; }
        Task<IReadOnlyList<string>> TranslateAsync(IReadOnlyList<string> segments, CancellationToken ct = default);
    }
}
