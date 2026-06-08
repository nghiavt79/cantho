using TechExchangeApp.Interfaces;
using TechExchangeApp.Services.Signing;

namespace TechExchangeApp.Services
{
    public interface ISigningProviderFactory
    {
        ISigningProvider Resolve(string providerName);
    }

    public class SigningProviderFactory : ISigningProviderFactory
    {
        private readonly IEnumerable<ISigningProvider> _providers;

        public SigningProviderFactory(IEnumerable<ISigningProvider> providers)
        {
            _providers = providers;
        }

        public ISigningProvider Resolve(string providerName)
        {
            var p = _providers.FirstOrDefault(x =>
                x.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
            if (p == null)
                throw new NotSupportedException($"Signing provider '{providerName}' is not registered.");
            return p;
        }
    }
}
