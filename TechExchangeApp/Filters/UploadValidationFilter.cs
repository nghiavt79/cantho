using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TechExchangeApp.Services;

namespace TechExchangeApp.Filters
{
    /// <summary>
    /// Bộ lọc toàn cục: mọi request gửi kèm tệp đều qua UploadFileGuard trước khi vào action
    /// (pentest MobiFone 10/2026 — Unrestricted File Upload). Tệp không hợp lệ → từ chối cả request.
    /// </summary>
    public class UploadValidationFilter : IAsyncActionFilter
    {
        private readonly ILogger<UploadValidationFilter> _logger;
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        public UploadValidationFilter(ILogger<UploadValidationFilter> logger, ITempDataDictionaryFactory tempDataFactory)
        {
            _logger = logger;
            _tempDataFactory = tempDataFactory;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context.HttpContext.Request;
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();
                foreach (var file in form.Files)
                {
                    if (file.Length == 0) continue; // input file để trống

                    string? error;
                    using (var stream = file.OpenReadStream())
                        error = UploadFileGuard.Validate(file.FileName, stream, file.Length);

                    if (error == null) continue;

                    _logger.LogWarning("[Upload] Từ chối tệp {File} ({Field}) tại {Path}: {Error}",
                        file.FileName, file.Name, request.Path, error);
                    context.Result = Reject(context, error);
                    return;
                }
            }

            await next();
        }

        private IActionResult Reject(ActionExecutingContext context, string error)
        {
            var request = context.HttpContext.Request;
            var wantsJson = request.Headers["X-Requested-With"] == "XMLHttpRequest"
                         || request.Headers.Accept.ToString().Contains("application/json")
                         || request.Path.StartsWithSegments("/cms/Upload", StringComparison.OrdinalIgnoreCase);

            if (wantsJson)
            {
                // Có cả "message" (các form AJAX) lẫn "error.message" (CKEditor)
                return new JsonResult(new { success = false, message = error, error = new { message = error } })
                { StatusCode = StatusCodes.Status400BadRequest };
            }

            var tempData = _tempDataFactory.GetTempData(context.HttpContext);
            tempData["ErrorMessage"] = error;

            var referer = request.Headers.Referer.ToString();
            if (Uri.TryCreate(referer, UriKind.Absolute, out var uri)
                && string.Equals(uri.Host, request.Host.Host, StringComparison.OrdinalIgnoreCase))
                return new RedirectResult(uri.PathAndQuery);

            return new ContentResult
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ContentType = "text/plain; charset=utf-8",
                Content = error
            };
        }
    }
}
