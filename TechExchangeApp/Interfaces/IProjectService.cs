using TechExchangeApp.ViewModel;

namespace TechExchangeApp.Interfaces
{
    public interface IProjectService
    {
        Task<List<MyProjectVm>> GetMyProjectsAsync(int userId);
        Task<int> GetProjectCountAsync(int userId);
    }
}
