namespace TechExchangeApp.Interfaces
{
    public interface ISmsSender
    {
        /// <summary>
        /// Gửi 1 tin SMS. smsId (tuỳ chọn) dùng để đối chiếu báo cáo gửi tin (DLR).
        /// Trả về phản hồi thô của nhà cung cấp (null nếu không có).
        /// Ném SmsPermanentException khi lỗi không nên gửi lại.
        /// </summary>
        Task<string?> SendAsync(string to, string message, string? smsId = null);
    }

    /// <summary>Lỗi gửi SMS không nên gửi lại (sai key, sai brandname, sai số, hết tiền...).</summary>
    public class SmsPermanentException : Exception
    {
        public int ErrorCode { get; }
        public SmsPermanentException(int errorCode, string message) : base(message) => ErrorCode = errorCode;
    }
}
