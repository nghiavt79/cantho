using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Helpers;

namespace TechExchangeApp.Services
{
    public interface ICommentService
    {
        /// <summary>Đưa ID về bản gốc tiếng Việt để bình luận dùng chung giữa trang VI và EN.</summary>
        Task<int?> ResolveTargetIdAsync(byte targetType, int targetId);
        Task<List<CommentItemVm>> GetVisibleAsync(byte targetType, int targetId, int take = 100);
        Task<(bool Ok, string? ErrorKey, CommentItemVm? Item)> AddAsync(byte targetType, int targetId, int userId, string content, string? ip);
        Task<bool> DeleteOwnAsync(int commentId, int userId);
    }

    public class CommentItemVm
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string? Avatar { get; set; }
        public string Content { get; set; } = "";
        public DateTime Created { get; set; }
    }

    /// <summary>Bình luận của người dùng (HSMT B.3.5, A.7.1). Hiện bình luận sản phẩm; bảng dùng chung cho gian hàng, bài viết.</summary>
    public class CommentService : ICommentService
    {
        public const int MaxLength = 2000;
        private static readonly TimeSpan MinInterval = TimeSpan.FromSeconds(20);

        private readonly AppDbContext _db;
        public CommentService(AppDbContext db) => _db = db;

        /// <summary>Trả null nếu đối tượng không tồn tại hoặc không cho bình luận.</summary>
        public async Task<int?> ResolveTargetIdAsync(byte targetType, int targetId)
        {
            switch (targetType)
            {
                case CommentTargetTypes.Product:
                {
                    var p = await _db.SanPhamCNTBs.AsNoTracking().Where(x => x.ID == targetId)
                        .Select(x => new { x.ID, x.LanguageId, x.OriginalId }).FirstOrDefaultAsync();
                    if (p == null) return null;
                    return p.LanguageId == LangHelper.En && p.OriginalId.HasValue ? p.OriginalId.Value : p.ID;
                }
                case CommentTargetTypes.Post:
                {
                    // Chỉ tin bài đang xuất bản; bản EN quy về Id bản VI (OriginalId)
                    var c = await _db.Contents.AsNoTracking().Where(x => x.Id == targetId && x.StatusId == 3)
                        .Select(x => new { x.Id, x.LanguageId, x.OriginalId }).FirstOrDefaultAsync();
                    if (c == null) return null;
                    var id = c.LanguageId == LangHelper.En && c.OriginalId.HasValue ? c.OriginalId.Value : c.Id;
                    return id <= int.MaxValue ? (int)id : null;
                }
                case CommentTargetTypes.Store:
                {
                    // Gian hàng = trang nhà cung cấp đang kích hoạt
                    var n = await _db.NhaCungUngs.AsNoTracking().Where(x => x.CungUngId == targetId && x.IsActivated == true)
                        .Select(x => new { x.CungUngId, x.LanguageId, x.OriginalId }).FirstOrDefaultAsync();
                    if (n == null) return null;
                    return n.LanguageId == LangHelper.En && n.OriginalId.HasValue ? n.OriginalId.Value : n.CungUngId;
                }
                default:
                    return null;
            }
        }

        public async Task<List<CommentItemVm>> GetVisibleAsync(byte targetType, int targetId, int take = 100)
        {
            return await (from c in _db.UserComments.AsNoTracking()
                          join u in _db.Users.AsNoTracking() on c.UserId equals u.Id
                          where c.TargetType == targetType && c.TargetId == targetId && c.StatusId == 1
                          orderby c.Created descending
                          select new CommentItemVm
                          {
                              Id = c.Id, UserId = c.UserId,
                              UserName = u.FullName ?? u.UserName ?? "",
                              Avatar = u.Img, Content = c.Content, Created = c.Created
                          }).Take(take).ToListAsync();
        }

        public async Task<(bool Ok, string? ErrorKey, CommentItemVm? Item)> AddAsync(byte targetType, int targetId, int userId, string content, string? ip)
        {
            content = (content ?? "").Trim();
            if (content.Length == 0) return (false, "comment.err.empty", null);
            if (content.Length > MaxLength) return (false, "comment.err.tooLong", null);

            var since = DateTime.Now - MinInterval;
            if (await _db.UserComments.AnyAsync(c => c.UserId == userId && c.Created >= since))
                return (false, "comment.err.tooFast", null);

            var user = await _db.Users.AsNoTracking().Where(u => u.Id == userId)
                .Select(u => new { u.FullName, u.UserName, u.Img, u.IsLocked }).FirstOrDefaultAsync();
            if (user == null || user.IsLocked) return (false, "comment.err.notAllowed", null);

            var entity = new UserComment
            {
                TargetType = targetType, TargetId = targetId, UserId = userId,
                Content = content, StatusId = 1, Created = DateTime.Now, IpAddress = ip
            };
            _db.UserComments.Add(entity);
            await _db.SaveChangesAsync();

            return (true, null, new CommentItemVm
            {
                Id = entity.Id, UserId = userId, UserName = user.FullName ?? user.UserName ?? "",
                Avatar = user.Img, Content = entity.Content, Created = entity.Created
            });
        }

        public async Task<bool> DeleteOwnAsync(int commentId, int userId)
        {
            var c = await _db.UserComments.FirstOrDefaultAsync(x => x.Id == commentId && x.UserId == userId);
            if (c == null) return false;
            _db.UserComments.Remove(c);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
