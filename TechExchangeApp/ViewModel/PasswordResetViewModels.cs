using System.ComponentModel.DataAnnotations;

namespace TechExchangeApp.ViewModel
{
    public class ForgotPasswordVm
    {
        [Required(ErrorMessage = "auth.forgot.err.required")]
        [StringLength(256)]
        public string EmailOrUserName { get; set; } = "";
    }

    public class ResetPasswordVm
    {
        [Required]
        public string Token { get; set; } = "";

        [Required(ErrorMessage = "auth.reset.err.passwordRequired")]
        [MinLength(6, ErrorMessage = "auth.reset.err.passwordMin")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "auth.reset.err.passwordRequired")]
        [Compare(nameof(NewPassword), ErrorMessage = "auth.reset.err.passwordMismatch")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";
    }
}
