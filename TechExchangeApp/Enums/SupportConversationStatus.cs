namespace TechExchangeApp.Enums
{
    /// <summary>Trạng thái xử lý của hội thoại hỗ trợ Sàn. Map tới ChatConversations.SupportStatus.</summary>
    public enum SupportConversationStatus
    {
        /// <summary>Không áp dụng (chat sản phẩm).</summary>
        None = 0,

        /// <summary>Chưa gán đầu mối — hiện ở inbox CMS.</summary>
        Unassigned = 1,

        /// <summary>Đang được một nhân viên/tư vấn phụ trách.</summary>
        InProgress = 2,

        /// <summary>Đã đóng (để dành cho phase sau).</summary>
        Closed = 3
    }
}
