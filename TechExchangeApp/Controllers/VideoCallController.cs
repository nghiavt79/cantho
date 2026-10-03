using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Helpers;
using TechExchangeApp.Interfaces;
using TechExchangeApp.Localization;

namespace TechExchangeApp.Controllers
{
    /// <summary>
    /// Gọi video từ màn hình chat (HSMT C.7, mục 2.1 – tương tác trực tuyến).
    /// Tạo một phòng họp trực tuyến có tên ngẫu nhiên (khó đoán) trên máy chủ họp cấu hình bởi
    /// tham số VIDEO_CALL_BASE_URL (mặc định Jitsi Meet công cộng; nên thay bằng máy chủ Jitsi tự dựng),
    /// rồi gửi lời mời vào hội thoại để bên kia và cán bộ Sàn (xem ở CMS Lịch sử chat) cùng tham gia.
    /// </summary>
    [Authorize]
    public class VideoCallController : Controller
    {
        public const string BaseUrlParam = "VIDEO_CALL_BASE_URL";
        private const string DefaultBaseUrl = "https://meet.jit.si/";

        private readonly AppDbContext _db;
        private readonly ISystemParameterService _params;

        public VideoCallController(AppDbContext db, ISystemParameterService @params)
        {
            _db = db;
            _params = @params;
        }

        [HttpPost("/api/chat/{id:long}/video-call")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(long id)
        {
            var uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var conv = await _db.ChatConversations.FirstOrDefaultAsync(c => c.Id == id && (c.BuyerUserId == uid || c.SupplierUserId == uid));
            if (conv == null) return NotFound(new { success = false });

            var baseUrl = (await _params.GetAsync(BaseUrlParam))?.Trim();
            if (string.IsNullOrEmpty(baseUrl) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out var b) || b.Scheme != "https")
                baseUrl = DefaultBaseUrl;
            if (!baseUrl.EndsWith('/')) baseUrl += "/";

            // Tên phòng: tiền tố + mã hội thoại + 12 ký tự ngẫu nhiên — người ngoài không đoán được
            var room = $"SanGDCN-CanTho-{id}-{Convert.ToHexString(RandomNumberGenerator.GetBytes(6))}";
            var url = baseUrl + room;

            var myId = int.Parse(uid);
            var me = await _db.Users.AsNoTracking().Where(u => u.Id == myId).Select(u => u.FullName ?? u.UserName).FirstOrDefaultAsync() ?? "";
            var isEn = LangHelper.IsEnglish(HttpContext);
            var invite = isEn
                ? $"<i class=\"fa fa-video\"></i> {WebUtility.HtmlEncode(me)} started a video call: <a href=\"{WebUtility.HtmlEncode(url)}\" target=\"_blank\" rel=\"noopener\">Join the call</a>"
                : $"<i class=\"fa fa-video\"></i> {WebUtility.HtmlEncode(me)} mời gọi video: <a href=\"{WebUtility.HtmlEncode(url)}\" target=\"_blank\" rel=\"noopener\">Tham gia cuộc gọi</a>";

            _db.ChatMessages.Add(new ChatMessage
            {
                ConversationId = id, SenderUserId = uid, Message = invite, IsSystem = true, IsRead = false, Created = DateTime.UtcNow
            });
            conv.LastMessageAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { success = true, url });
        }
    }
}
