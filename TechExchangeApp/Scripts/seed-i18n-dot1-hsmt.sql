-- Seed i18n cho Đợt 1 bổ sung chức năng HSMT: quên mật khẩu, khóa tài khoản, FAQ,
-- quan tâm sản phẩm, hình thức trưng bày, mã QR sản phẩm. Idempotent.
-- Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -i seed-i18n-dot1-hsmt.sql
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 -- Đăng nhập / quên mật khẩu / khóa tài khoản
 (N'auth.forgotPasswordQ',        N'Quên mật khẩu?',                                N'Forgot password?'),
 (N'auth.backToLogin',            N'Quay lại đăng nhập',                            N'Back to login'),
 (N'auth.err.locked',             N'Tài khoản đã bị tạm khóa. Vui lòng liên hệ Ban quản lý Sàn để được hỗ trợ.', N'This account has been suspended. Please contact the Exchange administrators for support.'),
 (N'auth.forgot.title',           N'Quên mật khẩu',                                 N'Forgot password'),
 (N'auth.forgot.intro',           N'Nhập email hoặc tên đăng nhập. Hệ thống sẽ gửi liên kết đặt lại mật khẩu tới email đã đăng ký.', N'Enter your email or username. We will send a password reset link to your registered email.'),
 (N'auth.forgot.label',           N'Email hoặc tên đăng nhập',                      N'Email or username'),
 (N'auth.forgot.submit',          N'Gửi liên kết đặt lại',                          N'Send reset link'),
 (N'auth.forgot.sent',            N'Nếu thông tin trùng khớp với một tài khoản, liên kết đặt lại mật khẩu đã được gửi tới email đăng ký. Liên kết có hiệu lực 30 phút.', N'If the details match an account, a password reset link has been sent to the registered email. The link is valid for 30 minutes.'),
 (N'auth.forgot.err.required',    N'Vui lòng nhập email hoặc tên đăng nhập.',       N'Please enter your email or username.'),
 (N'auth.reset.title',            N'Đặt lại mật khẩu',                              N'Reset password'),
 (N'auth.reset.newPassword',      N'Mật khẩu mới',                                  N'New password'),
 (N'auth.reset.confirmPassword',  N'Nhập lại mật khẩu mới',                         N'Confirm new password'),
 (N'auth.reset.submit',           N'Đặt mật khẩu mới',                              N'Set new password'),
 (N'auth.reset.done',             N'Đã đặt lại mật khẩu. Anh/chị có thể đăng nhập bằng mật khẩu mới.', N'Your password has been reset. You can now log in with the new password.'),
 (N'auth.reset.invalidToken',     N'Liên kết đặt lại mật khẩu không hợp lệ, đã hết hạn hoặc đã được sử dụng.', N'The password reset link is invalid, has expired or has already been used.'),
 (N'auth.reset.requestAgain',     N'Yêu cầu liên kết mới',                          N'Request a new link'),
 (N'auth.reset.err.passwordRequired', N'Vui lòng nhập mật khẩu mới.',               N'Please enter a new password.'),
 (N'auth.reset.err.passwordMin',  N'Mật khẩu tối thiểu 6 ký tự.',                   N'Password must be at least 6 characters.'),
 (N'auth.reset.err.passwordMismatch', N'Mật khẩu nhập lại không khớp.',             N'Passwords do not match.'),
 -- FAQ
 (N'faq.pageTitle',               N'Câu hỏi thường gặp',                            N'Frequently asked questions'),
 (N'faq.intro',                   N'Giải đáp các thắc mắc thường gặp khi sử dụng Sàn giao dịch công nghệ.', N'Answers to common questions about using the Technology Exchange.'),
 (N'faq.searchPh',                N'Tìm câu hỏi...',                                N'Search questions...'),
 (N'faq.searchBtn',               N'Tìm',                                           N'Search'),
 (N'faq.empty',                   N'Chưa có câu hỏi phù hợp.',                      N'No matching questions yet.'),
 (N'faq.moreHelp',                N'Chưa tìm thấy câu trả lời?',                    N'Still need help?'),
 (N'faq.contactLink',             N'Liên hệ với chúng tôi',                         N'Contact us'),
 -- Sản phẩm: quan tâm, hình thức trưng bày, QR
 (N'product.fav.save',            N'Quan tâm',                                      N'Save'),
 (N'product.fav.saved',           N'Đã quan tâm',                                   N'Saved'),
 (N'product.display.label',       N'Hình thức trưng bày',                           N'Display'),
 (N'product.display.all',         N'Mọi hình thức',                                 N'All displays'),
 (N'product.display.online',      N'Trực tuyến',                                    N'Online'),
 (N'product.display.showroom',    N'Trưng bày tại Sàn',                             N'On display at the Exchange'),
 (N'product.display.both',        N'Trực tuyến và tại Sàn',                         N'Online and at the Exchange'),
 (N'product.qr.alt',              N'Mã QR trang sản phẩm',                          N'Product page QR code'),
 (N'product.qr.hint',             N'Quét mã để mở trang sản phẩm',                  N'Scan to open this product page'),
 (N'product.qr.print',            N'In mã QR',                                      N'Print QR code'),
 -- Khu tài khoản: sản phẩm quan tâm
 (N'fav.pageTitle',               N'Sản phẩm quan tâm',                             N'Saved products'),
 (N'fav.empty',                   N'Chưa có sản phẩm nào. Bấm "Quan tâm" ở trang chi tiết sản phẩm để lưu lại.', N'No saved products yet. Click "Save" on a product page to keep it here.'),
 (N'fav.savedAt',                 N'Lưu lúc',                                       N'Saved on'),
 (N'fav.remove',                  N'Bỏ quan tâm',                                   N'Remove');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-dot1-hsmt' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
