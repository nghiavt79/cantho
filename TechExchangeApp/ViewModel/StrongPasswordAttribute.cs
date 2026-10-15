using System.ComponentModel.DataAnnotations;

namespace TechExchangeApp.ViewModel
{
    /// <summary>
    /// Chính sách mật khẩu (pentest MobiFone 10/2026 — Weak Password Policy):
    /// tối thiểu 8 ký tự, có chữ hoa, chữ thường, chữ số và ký tự đặc biệt.
    /// Áp dụng khi đăng ký, đổi, đặt lại mật khẩu và khi quản trị tạo / đặt lại mật khẩu trong CMS.
    /// Mật khẩu cũ không bị bắt đổi; chỉ áp dụng khi đặt mật khẩu mới.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class StrongPasswordAttribute : ValidationAttribute
    {
        public const int MinLength = 8;
        public const string DefaultMessage =
            "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt.";

        public StrongPasswordAttribute() : base(DefaultMessage) { }

        public static bool IsStrong(string? password)
            => !string.IsNullOrEmpty(password)
               && password.Length >= MinLength
               && password.Any(char.IsUpper)
               && password.Any(char.IsLower)
               && password.Any(char.IsDigit)
               && password.Any(ch => !char.IsLetterOrDigit(ch) && !char.IsWhiteSpace(ch));

        // Để trống thì giao cho [Required] báo lỗi
        public override bool IsValid(object? value)
            => value is not string s || s.Length == 0 || IsStrong(s);
    }
}
