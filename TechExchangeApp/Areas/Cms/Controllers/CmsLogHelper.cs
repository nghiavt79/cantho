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

            // Cắt cho vừa độ dài cột (ký tự) để không dính "String or binary data would be truncated".
            static string? Trunc(string? s, int max) =>
                string.IsNullOrEmpty(s) || s.Length <= max ? s : s[..max];

            context.Logs.Add(new Log
            {
                // Log.FunctionID là NOT NULL; nếu không map được tên → 0 (không có FK ràng buộc).
                FunctionID = functionId ?? 0,
                ActTime = DateTime.Now,
                EventID = eventId,
                Content = Trunc(content, 2000),         // nvarchar(2000)
                ClientIP = Trunc(httpContext.Connection.RemoteIpAddress?.ToString(), 50),   // nvarchar(100)
                UserName = Trunc(httpContext.User.Identity?.Name, 50),                       // nvarchar(100)
                Domain = Trunc(httpContext.Request.Host.Value, 500) ?? "",                   // nvarchar(1000) NOT NULL
                LanguageId = 1,
                ParentId = 0,
                SiteId = siteId
            });

            await context.SaveChangesAsync();
        }
    }
}
