using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>Xem và so sánh các phiên bản nội dung sản phẩm (HSMT A.2.7, A.3.2, A.8.3).</summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class SanPhamCNTBHistoryController : Controller
    {
        private readonly AppDbContext _context;
        public SanPhamCNTBHistoryController(AppDbContext context) => _context = context;

        /// <summary>Nhãn tiếng Việt cho các trường hay thay đổi; trường khác hiện tên cột.</summary>
        public static readonly Dictionary<string, string> FieldLabels = new()
        {
            ["Name"] = "Tên sản phẩm", ["Code"] = "Mã", ["MoTaNgan"] = "Mô tả ngắn", ["MoTa"] = "Mô tả chi tiết",
            ["ThongSo"] = "Thông số kỹ thuật", ["UuDiem"] = "Ưu điểm", ["GiaBanDuKien"] = "Giá bán dự kiến",
            ["SellPrice"] = "Giá bán", ["OriginalPrice"] = "Giá gốc", ["Currency"] = "Đơn vị tiền",
            ["StatusId"] = "Trạng thái", ["ProductType"] = "Loại sản phẩm", ["CategoryId"] = "Lĩnh vực",
            ["NCUId"] = "Nhà cung ứng", ["QuyTrinhHinhAnh"] = "Ảnh đại diện", ["URL"] = "Video",
            ["XuatXu"] = "Xuất xứ", ["XuatXuId"] = "Xuất xứ (mã)", ["MucDoId"] = "Mức độ phát triển",
            ["SoBang"] = "Số bằng", ["NgayCapBang"] = "Ngày cấp bằng", ["ThoiHan"] = "Thời hạn bảo hộ",
            ["ApplicationNumber"] = "Số đơn", ["AcceptedDate"] = "Ngày chấp nhận đơn", ["CoQuanChuTri"] = "Cơ quan chủ trì",
            ["CoQuanChuQuan"] = "Cơ quan chủ quản", ["TRLLevel"] = "Mức TRL", ["TransferMethod"] = "Phương thức chuyển giao",
            ["ChungNhanISO"] = "Chứng nhận ISO", ["ChungNhanQuatest"] = "Chứng nhận Quatest", ["ChungNhanKhacText"] = "Chứng nhận khác",
            ["BaoHanhHoTro"] = "Bảo hành, hỗ trợ", ["ChiPhiPhatSinh"] = "Chi phí phát sinh", ["Keywords"] = "Từ khóa",
            ["IsHot"] = "Ghim", ["DisplayMode"] = "Hình thức trưng bày", ["RequiresNDA"] = "Yêu cầu NDA",
            ["CanDirectInquiry"] = "Cho phép liên hệ trực tiếp", ["bEffectiveDate"] = "Ngày hiệu lực",
            ["eEffectiveDate"] = "Ngày hết hiệu lực", ["PublishedDate"] = "Ngày xuất bản", ["SoSaoOCOP"] = "Hạng sao OCOP",
            ["Phone"] = "Điện thoại", ["DiaChi"] = "Địa chỉ", ["HoTen"] = "Người liên hệ", ["WebUrl"] = "Website",
        };

        public static string Label(string field) => FieldLabels.TryGetValue(field, out var l) ? l : field;

        [HttpGet]
        public async Task<IActionResult> Index(int id, int? from, int? to)
        {
            var product = await _context.SanPhamCNTBs.AsNoTracking()
                .Where(p => p.ID == id).Select(p => new { p.ID, p.Name, p.ProductType }).FirstOrDefaultAsync();
            if (product == null) return NotFound();

            var versions = await _context.SanPhamCNTBVersions.AsNoTracking()
                .Where(v => v.SanPhamId == id).OrderByDescending(v => v.VersionNo).ToListAsync();

            var vm = new ProductHistoryVm
            {
                ProductId = product.ID,
                ProductName = product.Name ?? "",
                ProductType = product.ProductType,
                Versions = versions.Select(v => new ProductVersionRow
                {
                    VersionNo = v.VersionNo,
                    Action = v.Action,
                    ChangedBy = v.ChangedBy,
                    Created = v.Created,
                    ChangedFields = (v.ChangedFields ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Label).ToList()
                }).ToList()
            };

            if (versions.Count >= 2 || (versions.Count == 1 && from.HasValue))
            {
                var right = to.HasValue ? versions.FirstOrDefault(v => v.VersionNo == to) : versions[0];
                var left = from.HasValue
                    ? versions.FirstOrDefault(v => v.VersionNo == from)
                    : versions.Where(v => right != null && v.VersionNo < right.VersionNo).OrderByDescending(v => v.VersionNo).FirstOrDefault();
                if (left != null && right != null)
                {
                    vm.FromNo = left.VersionNo;
                    vm.ToNo = right.VersionNo;
                    vm.Diff = BuildDiff(left.SnapshotJson, right.SnapshotJson);
                }
            }
            return View(vm);
        }

        private static List<ProductDiffRow> BuildDiff(string leftJson, string rightJson)
        {
            var left = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(leftJson) ?? new();
            var right = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(rightJson) ?? new();
            var keys = right.Keys.Union(left.Keys).ToList();
            var rows = new List<ProductDiffRow>();
            foreach (var k in keys)
            {
                var a = Display(left.TryGetValue(k, out var l) ? l : default);
                var b = Display(right.TryGetValue(k, out var r) ? r : default);
                rows.Add(new ProductDiffRow { Field = k, Label = Label(k), Before = a, After = b, Changed = a != b });
            }
            // Trường có nhãn lên trước, theo thứ tự khai báo nhãn
            var order = FieldLabels.Keys.Select((k, i) => (k, i)).ToDictionary(x => x.k, x => x.i);
            return rows.OrderBy(x => order.TryGetValue(x.Field, out var i) ? i : 1000).ThenBy(x => x.Field).ToList();
        }

        /// <summary>Giá trị để hiển thị: bỏ thẻ HTML của các trường soạn thảo, rút gọn ngày giờ.</summary>
        private static string Display(JsonElement e)
        {
            switch (e.ValueKind)
            {
                case JsonValueKind.Undefined:
                case JsonValueKind.Null: return "";
                case JsonValueKind.True: return "Có";
                case JsonValueKind.False: return "Không";
                case JsonValueKind.String:
                    var s = e.GetString() ?? "";
                    if (DateTime.TryParse(s, out var dt) && Regex.IsMatch(s, @"^\d{4}-\d{2}-\d{2}T")) return dt.ToString("dd/MM/yyyy HH:mm");
                    if (s.Contains('<'))
                    {
                        s = Regex.Replace(s, @"<(br|/p|/div|/li|/h\d)[^>]*>", "\n", RegexOptions.IgnoreCase);
                        s = Regex.Replace(s, "<.*?>", "");
                        s = WebUtility.HtmlDecode(s);
                        s = Regex.Replace(s, @"\n\s*\n+", "\n").Trim();
                    }
                    return s;
                default: return e.GetRawText();
            }
        }
    }

    public class ProductHistoryVm
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int ProductType { get; set; }
        public List<ProductVersionRow> Versions { get; set; } = new();
        public int? FromNo { get; set; }
        public int? ToNo { get; set; }
        public List<ProductDiffRow> Diff { get; set; } = new();
    }

    public class ProductVersionRow
    {
        public int VersionNo { get; set; }
        public byte Action { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime Created { get; set; }
        public List<string> ChangedFields { get; set; } = new();
    }

    public class ProductDiffRow
    {
        public string Field { get; set; } = "";
        public string Label { get; set; } = "";
        public string Before { get; set; } = "";
        public string After { get; set; } = "";
        public bool Changed { get; set; }
    }
}
