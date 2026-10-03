using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Services
{
    /// <summary>Đọc định nghĩa trường và kiểm tra dữ liệu gửi của biểu mẫu động (HSMT A.7.2).</summary>
    public static class DynamicFormService
    {
        private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        public const int MaxValueLength = 4000;

        public static List<DynamicFormField> ParseFields(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new();
            try { return JsonSerializer.Deserialize<List<DynamicFormField>>(json, Json) ?? new(); }
            catch (JsonException) { return new(); }
        }

        public static string SerializeFields(IEnumerable<DynamicFormField> fields) => JsonSerializer.Serialize(fields, Json);

        public static string SerializeData(Dictionary<string, string> data) => JsonSerializer.Serialize(data);

        public static Dictionary<string, string> ParseData(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new();
            try { return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new(); }
            catch (JsonException) { return new(); }
        }

        /// <summary>Kiểm tra định nghĩa trường do quản trị nhập. Trả lỗi đầu tiên hoặc null.</summary>
        public static string? ValidateDefinition(List<DynamicFormField> fields)
        {
            if (fields.Count == 0) return "Biểu mẫu cần ít nhất một trường.";
            var keys = new HashSet<string>();
            foreach (var f in fields)
            {
                if (!Regex.IsMatch(f.Key ?? "", "^[a-z][a-z0-9_]{0,49}$")) return $"Mã trường '{f.Key}' chỉ gồm chữ thường không dấu, số, gạch dưới, bắt đầu bằng chữ.";
                if (!keys.Add(f.Key!)) return $"Mã trường '{f.Key}' bị trùng.";
                if (string.IsNullOrWhiteSpace(f.Label)) return $"Trường '{f.Key}' chưa có nhãn.";
                if (!DynamicFormField.AllowedTypes.Contains(f.Type)) return $"Kiểu trường '{f.Type}' không hợp lệ.";
                if (f.Type == "select" && (f.Options == null || f.Options.Count(o => !string.IsNullOrWhiteSpace(o)) == 0)) return $"Trường '{f.Label}' kiểu danh sách cần có lựa chọn.";
            }
            return null;
        }

        /// <summary>Kiểm tra dữ liệu người dùng gửi theo định nghĩa trường.</summary>
        public static (Dictionary<string, string> Data, Dictionary<string, string> Errors) Validate(List<DynamicFormField> fields, IFormCollection form, bool isEn)
        {
            var data = new Dictionary<string, string>();
            var errors = new Dictionary<string, string>();
            foreach (var f in fields)
            {
                var raw = f.Type == "checkbox" ? (form[f.Key].Count > 0 ? "true" : "") : form[f.Key].ToString().Trim();
                if (raw.Length > MaxValueLength) raw = raw[..MaxValueLength];
                if (f.Required && string.IsNullOrEmpty(raw)) { errors[f.Key] = isEn ? "Required." : "Bắt buộc nhập."; continue; }
                if (string.IsNullOrEmpty(raw)) { data[f.Key] = ""; continue; }

                switch (f.Type)
                {
                    case "email" when !Regex.IsMatch(raw, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"):
                        errors[f.Key] = isEn ? "Invalid email." : "Email không hợp lệ."; continue;
                    case "tel" when !Regex.IsMatch(raw, @"^[0-9+()\s.-]{6,20}$"):
                        errors[f.Key] = isEn ? "Invalid phone number." : "Số điện thoại không hợp lệ."; continue;
                    case "number" when !decimal.TryParse(raw, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out _):
                        errors[f.Key] = isEn ? "Must be a number." : "Phải là số."; continue;
                    case "date" when !DateTime.TryParse(raw, out _):
                        errors[f.Key] = isEn ? "Invalid date." : "Ngày không hợp lệ."; continue;
                    case "select" when f.Options == null || !f.Options.Contains(raw):
                        errors[f.Key] = isEn ? "Invalid choice." : "Lựa chọn không hợp lệ."; continue;
                }
                data[f.Key] = raw;
            }
            return (data, errors);
        }

        /// <summary>Bảng HTML (đã mã hóa) liệt kê dữ liệu đã gửi — dùng trong email.</summary>
        public static string ToHtmlTable(List<DynamicFormField> fields, Dictionary<string, string> data)
        {
            var sb = new StringBuilder("<table cellpadding=\"6\" style=\"border-collapse:collapse\">");
            foreach (var f in fields)
            {
                data.TryGetValue(f.Key, out var v);
                if (f.Type == "checkbox") v = v == "true" ? "✓" : "";
                sb.Append("<tr><td style=\"border:1px solid #ddd\"><b>").Append(WebUtility.HtmlEncode(f.Label)).Append("</b></td><td style=\"border:1px solid #ddd\">")
                  .Append(WebUtility.HtmlEncode(v ?? "").Replace("\n", "<br/>")).Append("</td></tr>");
            }
            return sb.Append("</table>").ToString();
        }
    }
}
