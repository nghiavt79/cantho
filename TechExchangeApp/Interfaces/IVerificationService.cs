using TechExchangeApp.ViewModel;

namespace TechExchangeApp.Interfaces
{
    public interface IVerificationService
    {
        Task<bool> SendEmailOtpAsync(int userId);
        Task<bool> SendPhoneOtpAsync(int userId);
        /// <summary>msgKey: key i18n (profile.err.*), controller tự resolve theo ngôn ngữ hiện tại.</summary>
        Task<(bool ok, string msgKey)> VerifyEmailOtpAsync(int userId, string otp);
        Task<(bool ok, string msgKey)> VerifyPhoneOtpAsync(int userId, string otp);
        /// <summary>msgKey rỗng khi ok=true (controller tự ghép nhãn loại giấy tờ + hậu tố).</summary>
        Task<(bool ok, string msgKey)> UploadDocAsync(int userId, int docType, IFormFile file, IWebHostEnvironment env);
        Task<List<VerifyDocVm>> GetDocsAsync(int userId);
        Task<bool> UpdatePhoneAsync(int userId, string phone);
    }
}
