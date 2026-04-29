using System.Security.Cryptography;

namespace _0_Framework.Application
{
    public class ResetTokenGenerator : IResetTokenGenerator
    {
        public string Generate()
        {
            var bytes = RandomNumberGenerator.GetBytes(32); // 256-bit
            return Base64UrlEncode(bytes);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }

}
