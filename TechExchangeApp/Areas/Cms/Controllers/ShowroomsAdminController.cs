using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>
    /// Phòng trưng bày và vị trí trưng bày (HSMT A.3.3, A.3.4): tạo phòng, tạo vị trí, gán sản phẩm
    /// "trưng bày tại Sàn" (DisplayMode 2/3) vào vị trí, xem vị trí trống / đang dùng.
    /// </summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class ShowroomsAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const string LogFunctionName = "ShowroomsAdmin";
        private static readonly string[] LogFunctionAliases = { "Showrooms" };

        public ShowroomsAdminController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetSiteId() => int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;
        private Task WriteLog(int eventId, string content) =>
            CmsLogHelper.WriteLogAsync(_context, HttpContext, GetSiteId(), LogFunctionName, LogFunctionAliases, eventId, content);

        [HttpGet]
        public async Task<IActionResult> Index(int? roomId)
        {
            var rooms = await _context.Showrooms.AsNoTracking().OrderBy(r => r.SortOrder).ThenBy(r => r.Id).ToListAsync();
            var counts = await _context.ShowroomSlots.AsNoTracking().Where(s => s.IsActive)
                .GroupBy(s => s.ShowroomId).Select(g => new { g.Key, Total = g.Count(), Used = g.Count(x => x.SanPhamId != null) })
                .ToDictionaryAsync(x => x.Key, x => (x.Total, x.Used));
            var room = rooms.FirstOrDefault(r => r.Id == roomId) ?? rooms.FirstOrDefault();

            var slots = new List<ShowroomSlotRow>();
            if (room != null)
                slots = await (from s in _context.ShowroomSlots.AsNoTracking()
                               join p in _context.SanPhamCNTBs.AsNoTracking() on s.SanPhamId equals p.ID into pj
                               from p in pj.DefaultIfEmpty()
                               where s.ShowroomId == room.Id
                               orderby s.Code
                               select new ShowroomSlotRow
                               {
                                   Id = s.Id, Code = s.Code, Description = s.Description, IsActive = s.IsActive,
                                   SanPhamId = s.SanPhamId, ProductName = p != null ? p.Name : null, AssignedAt = s.AssignedAt, AssignedBy = s.AssignedBy
                               }).ToListAsync();

            ViewBag.Rooms = rooms;
            ViewBag.Counts = counts;
            ViewBag.Room = room;
            // Sản phẩm "trưng bày tại Sàn" chưa có vị trí — gợi ý để gán
            var placed = _context.ShowroomSlots.Where(s => s.SanPhamId != null).Select(s => s.SanPhamId!.Value);
            ViewBag.Unplaced = await _context.SanPhamCNTBs.AsNoTracking()
                .Where(p => p.LanguageId == 1 && (p.DisplayMode == DisplayModes.Showroom || p.DisplayMode == DisplayModes.Both) && !placed.Contains(p.ID))
                .OrderBy(p => p.Name).Select(p => new { p.ID, p.Name }).Take(200).ToListAsync();
            return View(slots);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRoom(Showroom input)
        {
            if (string.IsNullOrWhiteSpace(input.Name)) return Json(new { success = false, message = "Nhập tên phòng trưng bày." });
            var r = input.Id > 0 ? await _context.Showrooms.FindAsync(input.Id) : null;
            if (r == null) { r = new Showroom(); _context.Showrooms.Add(r); }
            r.Name = input.Name.Trim(); r.Location = input.Location?.Trim(); r.Description = input.Description?.Trim();
            r.SortOrder = input.SortOrder; r.IsActive = input.IsActive;
            await _context.SaveChangesAsync();
            await WriteLog(2, $"Lưu phòng trưng bày #{r.Id} '{r.Name}'");
            return Json(new { success = true, id = r.Id });
        }

        /// <summary>Thêm nhiều vị trí một lần: tiền tố + số thứ tự (vd A-01 … A-20).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSlots(int roomId, string prefix, int count, int startAt = 1)
        {
            if (!await _context.Showrooms.AnyAsync(r => r.Id == roomId)) return Json(new { success = false, message = "Không tìm thấy phòng." });
            if (count < 1 || count > 200) return Json(new { success = false, message = "Số vị trí từ 1 đến 200." });
            prefix = (prefix ?? "").Trim();
            if (prefix.Length > 40) prefix = prefix[..40];
            var existing = (await _context.ShowroomSlots.Where(s => s.ShowroomId == roomId).Select(s => s.Code).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            int added = 0;
            for (int i = 0; i < count; i++)
            {
                var code = $"{prefix}{startAt + i:D2}";
                if (existing.Contains(code)) continue;
                _context.ShowroomSlots.Add(new ShowroomSlot { ShowroomId = roomId, Code = code, IsActive = true });
                added++;
            }
            await _context.SaveChangesAsync();
            await WriteLog(1, $"Thêm {added} vị trí trưng bày cho phòng #{roomId}");
            return Json(new { success = true, message = $"Đã thêm {added} vị trí." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int slotId, int sanPhamId)
        {
            var slot = await _context.ShowroomSlots.FindAsync(slotId);
            if (slot == null || !slot.IsActive) return Json(new { success = false, message = "Vị trí không hợp lệ." });
            if (slot.SanPhamId.HasValue) return Json(new { success = false, message = "Vị trí đang có sản phẩm, cần giải phóng trước." });
            var p = await _context.SanPhamCNTBs.FirstOrDefaultAsync(x => x.ID == sanPhamId && x.LanguageId == 1);
            if (p == null) return Json(new { success = false, message = "Không tìm thấy sản phẩm (nhập ID bản tiếng Việt)." });
            var other = await _context.ShowroomSlots.AsNoTracking().FirstOrDefaultAsync(s => s.SanPhamId == sanPhamId);
            if (other != null) return Json(new { success = false, message = $"Sản phẩm đang ở vị trí {other.Code}." });

            slot.SanPhamId = sanPhamId;
            slot.AssignedAt = DateTime.Now;
            slot.AssignedBy = User.Identity?.Name;
            // Sản phẩm đặt vào phòng trưng bày thì hình thức trưng bày phải có "tại Sàn"
            if (p.DisplayMode == DisplayModes.Online)
            {
                p.DisplayMode = DisplayModes.Both;
                foreach (var t in await _context.SanPhamCNTBs.Where(x => x.OriginalId == p.ID && x.ID != p.ID).ToListAsync())
                    t.DisplayMode = DisplayModes.Both;
            }
            await _context.SaveChangesAsync();
            await WriteLog(2, $"Gán sản phẩm #{sanPhamId} vào vị trí {slot.Code} (#{slot.Id})");
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Release(int slotId)
        {
            var slot = await _context.ShowroomSlots.FindAsync(slotId);
            if (slot == null) return Json(new { success = false, message = "Không tìm thấy vị trí." });
            var old = slot.SanPhamId;
            slot.SanPhamId = null; slot.AssignedAt = null; slot.AssignedBy = null;
            await _context.SaveChangesAsync();
            await WriteLog(2, $"Giải phóng vị trí {slot.Code} (sản phẩm #{old})");
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSlot(int id, string code, string? description, bool isActive)
        {
            var slot = await _context.ShowroomSlots.FindAsync(id);
            if (slot == null || string.IsNullOrWhiteSpace(code)) return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            slot.Code = code.Trim().Length > 50 ? code.Trim()[..50] : code.Trim();
            slot.Description = description?.Trim();
            slot.IsActive = isActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            var slot = await _context.ShowroomSlots.FindAsync(id);
            if (slot == null) return Json(new { success = false, message = "Không tìm thấy vị trí." });
            if (slot.SanPhamId.HasValue) return Json(new { success = false, message = "Vị trí đang có sản phẩm, cần giải phóng trước." });
            _context.ShowroomSlots.Remove(slot);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }

    public class ShowroomSlotRow
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int? SanPhamId { get; set; }
        public string? ProductName { get; set; }
        public DateTime? AssignedAt { get; set; }
        public string? AssignedBy { get; set; }
    }
}
