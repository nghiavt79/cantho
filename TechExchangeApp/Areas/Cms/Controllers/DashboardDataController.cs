using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace TechExchangeApp.Areas.Cms.Controllers
{
    /// <summary>
    /// Quản trị số liệu Hero trang chủ (Techport trong con số)
    /// </summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class DashboardDataController : Controller
    {
        private readonly IWebHostEnvironment _env;

        private static readonly Dictionary<string, string> DataFiles = new()
        {
            ["herostats"] = "js/hero-stats-data.json"
        };

        public DashboardDataController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // GET: /cms/DashboardData
        public IActionResult Index()
        {
            return View();
        }

        // GET: /cms/DashboardData/Load?key=herostats
        [HttpGet]
        public IActionResult Load(string key)
        {
            if (!DataFiles.TryGetValue(key ?? "", out var relativePath))
                return Json(new { error = $"Key '{key}' không hợp lệ" });

            var filePath = Path.Combine(_env.WebRootPath, relativePath);
            if (!System.IO.File.Exists(filePath))
                return Json(new { error = "File không tồn tại" });

            var json = System.IO.File.ReadAllText(filePath);
            return Content(json, "application/json");
        }

        // POST: /cms/DashboardData/Save?key=herostats
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Save(string key, [FromBody] JsonElement body)
        {
            if (!DataFiles.TryGetValue(key ?? "", out var relativePath))
                return Json(new { success = false, error = $"Key '{key}' không hợp lệ" });

            var filePath = Path.Combine(_env.WebRootPath, relativePath);

            try
            {
                var dir = Path.GetDirectoryName(filePath)!;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var backupPath = filePath + ".bak";
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Copy(filePath, backupPath, overwrite: true);

                var options = new JsonSerializerOptions { WriteIndented = true };
                var pretty = JsonSerializer.Serialize(body, options);
                await System.IO.File.WriteAllTextAsync(filePath, pretty);

                return Json(new { success = true, message = $"Đã cập nhật {key}", lastUpdated = DateTime.Now.ToString("dd/MM/yyyy HH:mm") });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, error = $"Không có quyền ghi file '{relativePath}'. Kiểm tra quyền Write cho IIS AppPool trên thư mục wwwroot/js." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi lưu file: {ex.GetType().Name} - {ex.Message}" });
            }
        }

        // POST: /cms/DashboardData/RestoreDefault?key=herostats
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult RestoreDefault(string key)
        {
            if (!DataFiles.TryGetValue(key ?? "", out var relativePath))
                return Json(new { success = false, error = $"Key '{key}' không hợp lệ" });

            var defaultPath = Path.Combine(_env.WebRootPath, "js/dataDefault", Path.GetFileName(relativePath));
            var targetPath = Path.Combine(_env.WebRootPath, relativePath);

            if (!System.IO.File.Exists(defaultPath))
                return Json(new { success = false, error = "File mặc định không tồn tại" });

            try
            {
                var backupPath = targetPath + ".bak";
                if (System.IO.File.Exists(targetPath))
                    System.IO.File.Copy(targetPath, backupPath, overwrite: true);

                System.IO.File.Copy(defaultPath, targetPath, overwrite: true);
                return Json(new { success = true, message = $"Đã khôi phục dữ liệu mặc định cho {key}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi khôi phục: {ex.Message}" });
            }
        }
    }
}
