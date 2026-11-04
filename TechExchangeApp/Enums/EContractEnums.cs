namespace TechExchangeApp.Enums
{
    /// <summary>Tiến trình hợp đồng trên Sàn khi gửi sang eContract, tách riêng với trạng thái phía eContract.</summary>
    public enum EContractIntegrationState
    {
        None = 0,
        Creating = 1,
        Created = 2,
        Issuing = 3,
        Processing = 4,
        Completed = 5,
        Failed = 8,
        Unknown = 9
    }

    public enum EContractOperationState
    {
        Pending = 0,
        Succeeded = 1,
        Failed = 2,
        Unknown = 3
    }
}
