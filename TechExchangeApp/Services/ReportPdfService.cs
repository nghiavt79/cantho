using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace TechExchangeApp.Services
{
    /// <summary>
    /// Xuất báo cáo dạng bảng ra PDF (HSMT nhóm 12: kết xuất bảng tính và PDF).
    /// Tiêu đề cột lấy từ [Display(Name)] giống xuất Excel; dùng HtmlToPdfService (font tiếng Việt).
    /// </summary>
    public class ReportPdfService
    {
        private readonly HtmlToPdfService _pdf;
        public ReportPdfService(HtmlToPdfService pdf) => _pdf = pdf;

        public record Section(string Heading, string Html);

        /// <summary>Tạo HTML bảng từ danh sách dòng.</summary>
        public static string Table<T>(IEnumerable<T> rows)
        {
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var sb = new StringBuilder("<table><thead><tr>");
            foreach (var p in props)
                sb.Append("<th>").Append(Enc(p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name)).Append("</th>");
            sb.Append("</tr></thead><tbody>");
            var any = false;
            foreach (var r in rows)
            {
                any = true;
                sb.Append("<tr>");
                foreach (var p in props)
                {
                    var v = p.GetValue(r);
                    var isNum = v is decimal or int or long or double;
                    var text = v switch
                    {
                        null => "",
                        decimal d => d.ToString("#,##0.##", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")),
                        DateTime dt => dt.ToString("dd/MM/yyyy"),
                        _ => v.ToString() ?? ""
                    };
                    sb.Append(isNum ? "<td class=\"num\">" : "<td>").Append(Enc(text)).Append("</td>");
                }
                sb.Append("</tr>");
            }
            if (!any) sb.Append($"<tr><td colspan=\"{props.Length}\" class=\"empty\">Không có dữ liệu</td></tr>");
            sb.Append("</tbody></table>");
            return sb.ToString();
        }

        public FileContentResult Build(string title, string subtitle, IEnumerable<Section> sections, string fileName)
        {
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html lang=\"vi\"><head><meta charset=\"UTF-8\"/><title>").Append(Enc(title)).Append("</title><style>")
              .Append("@page{size:A4 landscape;margin:1.5cm 1.2cm}")
              .Append("body{font-family:'Times New Roman',Arial,sans-serif;font-size:10.5pt;color:#111}")
              .Append("h1{font-size:15pt;text-align:center;margin:0 0 4px}.sub{text-align:center;font-style:italic;margin-bottom:12px}")
              .Append("h2{font-size:12pt;margin:14px 0 6px}table{width:100%;border-collapse:collapse}")
              .Append("th,td{border:1px solid #888;padding:3px 5px;vertical-align:top}th{background:#e8eef7}")
              .Append("td.num{text-align:right;white-space:nowrap}td.empty{text-align:center;color:#666}")
              .Append(".foot{margin-top:14px;font-size:9.5pt;color:#555}</style></head><body>");
            sb.Append("<h1>").Append(Enc(title)).Append("</h1><div class=\"sub\">").Append(Enc(subtitle)).Append("</div>");
            foreach (var s in sections)
            {
                if (!string.IsNullOrEmpty(s.Heading)) sb.Append("<h2>").Append(Enc(s.Heading)).Append("</h2>");
                sb.Append(s.Html);
            }
            sb.Append("<div class=\"foot\">Xuất lúc ").Append(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).Append(" – Sàn giao dịch công nghệ Cần Thơ</div></body></html>");
            var bytes = _pdf.Convert(sb.ToString(), title);
            return new FileContentResult(bytes, "application/pdf") { FileDownloadName = fileName };
        }

        private static string Enc(string s) => WebUtility.HtmlEncode(s);
    }
}
