using Microsoft.AspNetCore.Mvc;
using TechExchangeApp.Services;

namespace TechExchangeApp.ViewComponents
{
    /// <summary>
    /// Khối bình luận (HSMT B.3.5). Dùng:
    ///   @await Component.InvokeAsync("Comments", new { targetType = CommentTargetTypes.Product, targetId = Model.Product.ID })
    /// </summary>
    public class CommentsViewComponent : ViewComponent
    {
        private readonly ICommentService _comments;
        public CommentsViewComponent(ICommentService comments) => _comments = comments;

        public async Task<IViewComponentResult> InvokeAsync(byte targetType, int targetId)
        {
            var resolved = await _comments.ResolveTargetIdAsync(targetType, targetId) ?? targetId;
            var items = await _comments.GetVisibleAsync(targetType, resolved);
            ViewBag.TargetType = targetType;
            ViewBag.TargetId = targetId;
            return View(items);
        }
    }
}
