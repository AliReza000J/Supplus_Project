using System.Security.Cryptography;
using System.Text;

namespace _0_Framework.Application
{
    public class HmacTokenProtector : IResetTokenProtector
    {
        private readonly byte[] _key;
        public HmacTokenProtector(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));
            _key = Encoding.UTF8.GetBytes(key);
        }

        public string Protect(string rawToken)
        {
            using var hmac = new HMACSHA256(_key);
            var bytes = Encoding.UTF8.GetBytes(rawToken);
            var hash = hmac.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool Verify(string rawToken, string protectedToken)
        {
            var computed = Protect(rawToken);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(computed),
                Convert.FromBase64String(protectedToken));
        }
    }

}
