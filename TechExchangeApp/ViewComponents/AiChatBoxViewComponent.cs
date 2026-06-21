using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TechExchangeApp.Configuration;

namespace TechExchangeApp.ViewComponents
{
    public class AiChatBoxViewComponent : ViewComponent
    {
        private readonly AiChatOptions _options;

        public AiChatBoxViewComponent(IOptions<AiChatOptions> options)
        {
            _options = options.Value;
        }

        public IViewComponentResult Invoke()
        {
            return View(_options);
        }
    }
}
