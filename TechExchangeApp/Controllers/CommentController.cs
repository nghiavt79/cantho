using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechExchangeApp.Entities;
using TechExchangeApp.Localization;
using TechExchangeApp.Services;

namespace TechExchangeApp.Controllers
{
    /// <summary>Gửi / xóa bình luận (HSMT B.3.5). Gọi AJAX với header RequestVerificationToken.</summary>
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentService _comments;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(ICommentService comments, UserManager<ApplicationUser> userManager)
        {
            _comments = comments;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post(byte targetType, int targetId, string content)
        {
            var resolved = await _comments.ResolveTargetIdAsync(targetType, targetId);
            if (resolved == null)
                return Json(new { success = false, message = I18n.T(HttpContext, "comment.err.notAllowed") });

            var userId = int.Parse(_userManager.GetUserId(User)!);
            var (ok, errKey, item) = await _comments.AddAsync(targetType, resolved.Value, userId, content,
                HttpContext.Connection.RemoteIpAddress?.ToString());
            if (!ok) return Json(new { success = false, message = I18n.T(HttpContext, errKey!) });

            return Json(new
            {
                success = true,
                item = new { item!.Id, item.UserName, item.Content, created = item.Created.ToString("dd/MM/yyyy HH:mm") }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var ok = await _comments.DeleteOwnAsync(id, userId);
            return Json(new { success = ok });
        }
    }
}
