namespace TechExchangeApp.ViewModel
{
    public class PhieuYeuCauCNViewModel
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? NoiDung { get; set; }
        public string? LinhVuc { get; set; }
        public string? NganSach { get; set; }
        public DateTime? ThoiHan { get; set; }
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> LinhVucs { get; set; } = new();

        // Math captcha (server-side, kiểu trang liên hệ)
        public string? CaptchaQuestion { get; set; }
        public string? CaptchaAnswer { get; set; }
    }

}
