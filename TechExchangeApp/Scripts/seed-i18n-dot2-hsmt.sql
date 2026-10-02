-- Seed i18n cho Đợt 2 bổ sung chức năng HSMT: bình luận sản phẩm, bài viết nhà cung cấp. Idempotent.
-- Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -i seed-i18n-dot2-hsmt.sql
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'comment.title',          N'Bình luận',                                        N'Comments'),
 (N'comment.placeholder',    N'Chia sẻ câu hỏi hoặc nhận xét về sản phẩm...',     N'Share a question or comment about this product...'),
 (N'comment.submit',         N'Gửi bình luận',                                    N'Post comment'),
 (N'comment.loginToComment', N'để bình luận.',                                    N'to comment.'),
 (N'comment.empty',          N'Chưa có bình luận nào.',                           N'No comments yet.'),
 (N'comment.delete',         N'Xóa bình luận',                                    N'Delete comment'),
 (N'comment.confirmDelete',  N'Xóa bình luận này?',                               N'Delete this comment?'),
 (N'comment.err.empty',      N'Vui lòng nhập nội dung bình luận.',                N'Please enter a comment.'),
 (N'comment.err.tooLong',    N'Bình luận tối đa 2000 ký tự.',                     N'Comments are limited to 2000 characters.'),
 (N'comment.err.tooFast',    N'Anh/chị gửi quá nhanh, vui lòng thử lại sau ít giây.', N'You are posting too quickly. Please try again in a few seconds.'),
 (N'comment.err.notAllowed', N'Không thể gửi bình luận.',                         N'Unable to post the comment.'),
 (N'sidebar.myPosts',        N'Bài viết của tôi',                                 N'My posts');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-dot2-hsmt' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
