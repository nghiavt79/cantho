using System.Security.Cryptography;
using System.Text;

namespace TechExchangeApp.Helpers
{
    public static class TranslationHashHelper
    {
        public static string HashOf(params string?[] parts)
        {
            var raw = string.Join("\u241F", parts.Select(p => p ?? ""));
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
        }
    }
}
