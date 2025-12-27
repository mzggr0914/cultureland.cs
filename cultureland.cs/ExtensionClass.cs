using System;
using System.Security.Cryptography;

namespace cultureland.cs
{
    public static class ExtensionClass
    {
        public static string EncodeURIComponent(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            string encoded = Uri.EscapeDataString(value);

            encoded = encoded.Replace("%21", "!")
                             .Replace("%27", "'")
                             .Replace("%28", "(")
                             .Replace("%29", ")")
                             .Replace("%2A", "*")
                             .Replace("%20", "+");
            return encoded;
        }
        public static string GenerateRandomBytes(int bytecount)
        {
            byte[] randomBytes = new byte[bytecount];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            string base64 = Convert.ToBase64String(randomBytes);

            string base64Url = base64.Replace('+', '-')
                                     .Replace('/', '_')
                                     .TrimEnd('=');

            return base64Url;
        }

    }
}
