using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Areas.Cms.Models;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class SystemParameterAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "SysParam";
        private static readonly string[] LogFunctionAliases = { "SYS_PARAMETERS" };

        private static readonly string[] SensitiveKeywords =
        [
            "PASSWORD", "TOKEN", "SECRET", "API_KEY", "PRIVATE", "CLIENT_ID", "ACCOUNT_ID", "CREDENTIAL"
        ];

        public SystemParameterAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private async Task WriteLog(int eventId, string content)
        {
            await CmsLogHelper.WriteLogAsync(_context, HttpContext, GetSiteId(),
                LogFunctionName, LogFunctionAliases, eventId, content);
        }

        public async Task<IActionResult> Index(
            string? keyword, string? domain, int? type, bool? activated, int? siteId,
            int page = 1, int pageSize = 20)
        {
            if (!new[] { 10, 20, 50, 100 }.Contains(pageSize))
                pageSize = 20;

            var configSiteId = GetSiteId();
            var query = _context.SystemParameters.AsNoTracking()
                .Where(p => p.SiteId == null || p.SiteId == configSiteId);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(kw) ||
                    (p.Description != null && p.Description.ToLower().Contains(kw)) ||
                    (p.Domain != null && p.Domain.ToLower().Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(domain))
                query = query.Where(p => p.Domain == domain);
            if (type.HasValue)
                query = query.Where(p => p.Type == type.Value);
            if (activated.HasValue)
                query = query.Where(p => p.Activated == activated.Value);
            if (siteId.HasValue)
                query = query.Where(p => p.SiteId == siteId.Value);

            query = query.OrderBy(p => p.Domain).ThenBy(p => p.Name);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            var items = await query.Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new SystemParameterListItemVm
                {
                    ID = p.ID,
                    Name = p.Name,
                    Val = p.Val,
                    Val2 = p.Val2,
                    Type = p.Type,
                    Description = p.Description,
                    IsSystem = p.IsSystem,
                    Activated = p.Activated,
                    Domain = p.Domain,
                    LanguageId = p.LanguageId,
                    SiteId = p.SiteId
                })
                .ToListAsync();

            foreach (var item in items)
            {
                item.IsSensitive = IsSensitiveKey(item.Name);
                item.TypeLabel = GetTypeLabel(item.Type, item.Name);
                item.DisplayVal = item.Val ?? "";
                item.DisplayVal2 = item.Val2 ?? "";
            }

            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Keyword = keyword;
            ViewBag.Domain = domain;
            ViewBag.Type = type;
            ViewBag.Activated = activated;
            ViewBag.SiteId = siteId;
            ViewBag.CurrentSiteId = configSiteId;
            ViewBag.Domains = await _context.SystemParameters.AsNoTracking()
                .Where(p => p.Domain != null && p.Domain != "")
                .Select(p => p.Domain!)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
            ViewBag.Sites = await _context.RootSites.AsNoTracking().OrderBy(s => s.SiteId).ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ListPartial", items);

            return View(items);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.SystemParameters.AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID == id);
            if (entity == null)
                return NotFound();

            var isSensitive = IsSensitiveKey(entity.Name);
            var vm = new SystemParameterEditVm
            {
                ID = entity.ID,
                Name = entity.Name,
                Val = entity.Val,
                Val2 = entity.Val2,
                Type = entity.Type,
                Description = entity.Description,
                Activated = entity.Activated,
                IsSystem = entity.IsSystem,
                Domain = entity.Domain,
                LanguageId = entity.LanguageId,
                SiteId = entity.SiteId,
                IsSensitive = isSensitive,
                TypeLabel = GetTypeLabel(entity.Type, entity.Name),
                CurrentMaskedVal = MaskValue(entity.Val)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SystemParameterEditVm model)
        {
            var entity = await _context.SystemParameters.FirstOrDefaultAsync(p => p.ID == model.ID);
            if (entity == null)
                return NotFound();

            model.Name = entity.Name;
            model.Domain = entity.Domain;
            model.LanguageId = entity.LanguageId;
            model.SiteId = entity.SiteId;
            model.IsSystem = entity.IsSystem;
            model.IsSensitive = IsSensitiveKey(entity.Name);
            model.TypeLabel = GetTypeLabel(model.Type, entity.Name);
            model.CurrentMaskedVal = MaskValue(entity.Val);

            var normalizedVal = NormalizeValue(model.Val, model.Type, entity.Name, model.IsSensitive, entity.Val, nameof(model.Val));
            var normalizedVal2 = NormalizeValue(model.Val2, model.Type, entity.Name, model.IsSensitive, entity.Val2, nameof(model.Val2), allowEmpty: true);

            if (!ModelState.IsValid)
                return View(model);

            var oldVal = entity.Val;
            var oldVal2 = entity.Val2;
            var oldType = entity.Type;
            var oldDescription = entity.Description;
            var oldActivated = entity.Activated;

            entity.Type = model.Type;
            entity.Description = model.Description?.Trim();
            entity.Activated = model.Activated;
            entity.Val = normalizedVal ?? "";
            entity.Val2 = normalizedVal2;

            await _context.SaveChangesAsync();

            var diff = BuildDiff(entity.Name, model.IsSensitive, oldVal, entity.Val, oldVal2, entity.Val2, oldType, entity.Type, oldDescription, entity.Description, oldActivated, entity.Activated);
            await WriteLog(2, $"Update SYS_PARAMETERS: {entity.Name} (ID={entity.ID}); {diff}");

            TempData["Success"] = "Đã cập nhật tham số hệ thống.";
            return RedirectToAction(nameof(Edit), new { id = entity.ID });
        }

        private string? NormalizeValue(string? value, int? type, string key, bool isSensitive, string? currentValue, string fieldName, bool allowEmpty = false)
        {
            var input = value?.Trim() ?? "";
            if (allowEmpty && string.IsNullOrEmpty(input))
                return null;

            var effectiveKind = GetValueKind(type, key);
            switch (effectiveKind)
            {
                case ParameterValueKind.Int:
                    if (!int.TryParse(input, out _))
                        ModelState.AddModelError(fieldName, "Giá trị phải là số nguyên.");
                    break;
                case ParameterValueKind.Bool:
                    if (!TryNormalizeBool(input, out var boolValue))
                        ModelState.AddModelError(fieldName, "Giá trị boolean chỉ nhận true/false, 1/0, yes/no.");
                    else
                        input = boolValue ? "true" : "false";
                    break;
                case ParameterValueKind.Json:
                    try
                    {
                        using var _ = JsonDocument.Parse(input);
                    }
                    catch
                    {
                        ModelState.AddModelError(fieldName, "JSON không hợp lệ.");
                    }
                    break;
            }

            return input;
        }

        private static string BuildDiff(
            string key, bool isSensitive,
            string? oldVal, string? newVal, string? oldVal2, string? newVal2,
            int? oldType, int? newType, string? oldDescription, string? newDescription,
            bool oldActivated, bool newActivated)
        {
            var changes = new List<string>();
            if (!string.Equals(oldVal, newVal, StringComparison.Ordinal))
                changes.Add(isSensitive ? "Val changed" : $"Val: '{Shorten(oldVal)}' -> '{Shorten(newVal)}'");
            if (!string.Equals(oldVal2, newVal2, StringComparison.Ordinal))
                changes.Add(isSensitive ? "Val2 changed" : $"Val2: '{Shorten(oldVal2)}' -> '{Shorten(newVal2)}'");
            if (oldType != newType)
                changes.Add($"Type: {oldType?.ToString() ?? "null"} -> {newType?.ToString() ?? "null"}");
            if (!string.Equals(oldDescription, newDescription, StringComparison.Ordinal))
                changes.Add($"Description changed");
            if (oldActivated != newActivated)
                changes.Add($"Activated: {oldActivated} -> {newActivated}");

            return changes.Count == 0 ? "No visible changes" : string.Join("; ", changes);
        }

        private static bool IsSensitiveKey(string key)
        {
            var upper = key.ToUpperInvariant();
            return SensitiveKeywords.Any(upper.Contains);
        }

        private static string MaskValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            return value.Length <= 4 ? "****" : $"{new string('*', Math.Min(8, value.Length - 4))}{value[^4..]}";
        }

        private static string Shorten(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            return value.Length <= 40 ? value : value[..40] + "...";
        }

        private static string GetTypeLabel(int? type, string key)
        {
            return GetValueKind(type, key) switch
            {
                ParameterValueKind.Int => "Number",
                ParameterValueKind.Bool => "Boolean",
                ParameterValueKind.Json => "JSON",
                ParameterValueKind.Secret => "Secret",
                _ => "Text"
            };
        }

        private static ParameterValueKind GetValueKind(int? type, string key)
        {
            if (IsSensitiveKey(key))
                return ParameterValueKind.Secret;

            return type switch
            {
                2 => ParameterValueKind.Int,
                3 => ParameterValueKind.Bool,
                4 => ParameterValueKind.Json,
                5 => ParameterValueKind.Secret,
                _ => InferKindFromName(key)
            };
        }

        private static ParameterValueKind InferKindFromName(string key)
        {
            var upper = key.ToUpperInvariant();
            if (upper.Contains("JSON"))
                return ParameterValueKind.Json;
            if (upper.Contains("ENABLE") || upper.StartsWith("IS_") || upper.EndsWith("_SSL"))
                return ParameterValueKind.Bool;
            if (upper.Contains("MAX") || upper.Contains("SECONDS") || upper.Contains("PORT") || upper.Contains("SIZE") || upper.EndsWith("_HRS") || upper.EndsWith("_MB"))
                return ParameterValueKind.Int;
            return ParameterValueKind.Text;
        }

        private static bool TryNormalizeBool(string value, out bool result)
        {
            switch (value.Trim().ToLowerInvariant())
            {
                case "true":
                case "1":
                case "yes":
                case "on":
                    result = true;
                    return true;
                case "false":
                case "0":
                case "no":
                case "off":
                    result = false;
                    return true;
                default:
                    result = false;
                    return false;
            }
        }

        private enum ParameterValueKind
        {
            Text,
            Int,
            Bool,
            Json,
            Secret
        }
    }
}
