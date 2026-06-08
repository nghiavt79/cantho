namespace TechExchangeApp.Interfaces
{
    public interface ISmsSender
    {
        Task SendAsync(string to, string message);
    }
}
