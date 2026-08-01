using TechExchangeApp.Configuration;
using TechExchangeApp.Interfaces;

namespace TechExchangeApp.Services.Translation
{
    public interface ITranslationServiceFactory
    {
        /// <summary>Tạo service theo cấu hình SYS_PARAMETERS. Trả về null nếu chưa nhập key / provider sai.</summary>
        Task<ITranslationService?> CreateAsync(CancellationToken ct = default);
    }

    public sealed class TranslationServiceFactory : ITranslationServiceFactory
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ISystemParameterService _param;

        public TranslationServiceFactory(IHttpClientFactory httpFactory, ISystemParameterService param)
        {
            _httpFactory = httpFactory;
            _param = param;
        }

        public async Task<ITranslationService?> CreateAsync(CancellationToken ct = default)
        {
            var provider = (await _param.GetAsync(ParameterKeys.TranslationProvider) ?? "sonnet").Trim().ToLowerInvariant();
            var http = _httpFactory.CreateClient("translation");
            http.Timeout = TimeSpan.FromMinutes(3);

            switch (provider)
            {
                case "haiku":
                case "sonnet":
                {
                    var key = (await _param.GetAsync(ParameterKeys.TranslationAnthropicApiKey))?.Trim();
                    if (string.IsNullOrEmpty(key)) return null;
                    var model = provider == "haiku" ? "claude-haiku-4-5" : "claude-sonnet-5";
                    return new TranslationService(new ClaudeTranslator(http, key, model));
                }
                case "google":
                {
                    var key = (await _param.GetAsync(ParameterKeys.TranslationGoogleApiKey))?.Trim();
                    if (string.IsNullOrEmpty(key)) return null;
                    return new TranslationService(new GoogleTranslator(http, key));
                }
                default:
                    return null;
            }
        }
    }
}
