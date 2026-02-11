using System.Text;

namespace _0_Framework.Application
{
    public static class GuidShortener
    {
        private static readonly string Base62Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string ToBase62(Guid guid)
        {
            var bytes = guid.ToByteArray();
            var longValue = BitConverter.ToInt64(bytes, 0);

            var result = new StringBuilder();
            while (longValue > 0)
            {
                result.Insert(0, Base62Chars[(int)(longValue % 62)]);
                longValue /= 62;
            }

            return result.ToString().PadLeft(16, '0');
        }

        public static Guid FromBase62(string base62)
        {
            long value = 0;
            foreach (var c in base62)
            {
                value = value * 62 + Base62Chars.IndexOf(c);
            }

            var bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }
    }

}
