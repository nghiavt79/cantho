using TechExchangeApp.Entities;

namespace TechExchangeApp.Interfaces
{
    public interface INotificationProcessor
    {
        Task ProcessAsync(Notification notification, CancellationToken cancellationToken = default);
    }
}
