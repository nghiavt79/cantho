-- Seed i18n cho Đăng nhập/Đăng ký (LoginModal, RegisterModal, Views/Account/Login+Register, AccountController). Idempotent.
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'auth.username',        N'Tên đăng nhập',                       N'Username'),
 (N'auth.usernamePh',      N'Nhập tên đăng nhập',                  N'Enter your username'),
 (N'auth.usernamePhShort', N'Tên đăng nhập',                       N'Username'),
 (N'auth.password',        N'Mật khẩu',                            N'Password'),
 (N'auth.passwordPh',      N'Nhập mật khẩu',                       N'Enter your password'),
 (N'auth.rememberMe',      N'Ghi nhớ đăng nhập',                   N'Remember me'),
 (N'auth.noAccountQ',      N'Chưa có tài khoản?',                  N'Don''t have an account yet?'),
 (N'auth.registerNow',     N'Đăng ký ngay',                        N'Register now'),
 (N'auth.haveAccountQ',    N'Đã có tài khoản?',                    N'Already have an account?'),
 (N'auth.registerTitle',   N'Đăng ký thành viên',                  N'Create an account'),
 (N'auth.fullName',        N'Họ và tên',                           N'Full name'),
 (N'auth.fullNamePh',      N'Họ và tên đầy đủ',                    N'Your full name'),
 (N'auth.confirmPassword', N'Nhập lại mật khẩu',                   N'Confirm password'),
 (N'auth.minChars',        N'Tối thiểu 6 ký tự',                   N'Minimum 6 characters'),
 (N'auth.err.usernameExists', N'Tên đăng nhập đã tồn tại.',        N'Username already exists.'),
 (N'auth.err.emailExists', N'Email đã được sử dụng.',              N'Email is already in use.'),
 (N'auth.err.loginFailed', N'Tên đăng nhập hoặc mật khẩu không chính xác.', N'Incorrect username or password.'),
 (N'auth.err.registerFailed', N'Không thể tạo tài khoản. Vui lòng thử lại sau.', N'Unable to create account. Please try again.');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
