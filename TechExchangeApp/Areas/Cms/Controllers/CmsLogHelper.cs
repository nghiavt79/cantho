using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    internal static class CmsLogHelper
    {
        public static async Task WriteLogAsync(
            AppDbContext context,
            HttpContext httpContext,
            int siteId,
            string functionName,
            string[]? aliases,
            int eventId,
            string content)
        {
            var names = new[] { functionName }
                .Concat(aliases ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var functions = await context.SysFunctions.AsNoTracking()
                .Where(f => f.FunctionName != null && names.Contains(f.FunctionName))
                .Select(f => new { f.FunctionId, f.FunctionName })
                .ToListAsync();

            var functionId = functions.FirstOrDefault(f => f.FunctionName == functionName)?.FunctionId
                ?? functions.FirstOrDefault()?.FunctionId;

            context.Logs.Add(new Log
            {
                FunctionID = functionId,
                ActTime = DateTime.Now,
                EventID = eventId,
                Content = content,
                ClientIP = httpContext.Connection.RemoteIpAddress?.ToString(),
                UserName = httpContext.User.Identity?.Name,
                Domain = httpContext.Request.Host.Value,
                LanguageId = 1,
                ParentId = 0,
                SiteId = siteId
            });

            await context.SaveChangesAsync();
        }
    }
}
