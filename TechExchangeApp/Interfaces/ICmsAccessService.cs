using System.Security.Claims;

namespace TechExchangeApp.Interfaces
{
    public interface ICmsAccessService
    {
        Task<bool> CanAccessCmsAsync(ClaimsPrincipal user);
    }
}
