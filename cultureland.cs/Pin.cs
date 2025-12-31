using System;

namespace cultureland.cs
{
    public sealed class Pin
    {
        private readonly string[] _parts;

        public string[] Parts => _parts;

        public Pin(string pin)
        {
            if (!TryParse(pin, out _parts))
                throw new CulturelandError(CulturelandErrorNames.PinValidationError, "존재하지 않는 상품권입니다.");
        }

        public Pin(string pinPart1, string pinPart2, string pinPart3, string pinPart4)
            : this($"{pinPart1}-{pinPart2}-{pinPart3}-{pinPart4}") { }

        public override string ToString() => string.Join("-", _parts);

        public static string[] Format(string pin) => TryParse(pin, out var parts) ? parts : null;

        public static bool ValidateClientSide(string pin)
        {
            Span<char> digits = stackalloc char[18];
            if (!TryReadDigits(pin, digits, out var n)) return false;

            var lastLen = n - 12;
            return IsValidPrefix(digits, lastLen);
        }

        private static bool TryParse(string pin, out string[] parts)
        {
            parts = null;

            Span<char> digits = stackalloc char[18];
            if (!TryReadDigits(pin, digits, out var n)) return false;

            var lastLen = n - 12;
            if (!IsValidPrefix(digits, lastLen)) return false;

            parts = new[]
            {
                new string(digits.Slice(0, 4)),
                new string(digits.Slice(4, 4)),
                new string(digits.Slice(8, 4)),
                new string(digits.Slice(12, lastLen))
            };
            return true;
        }

        private static bool TryReadDigits(string pin, Span<char> digits, out int n)
        {
            n = 0;
            if (string.IsNullOrWhiteSpace(pin)) return false;

            for (int i = 0; i < pin.Length; i++)
            {
                char ch = pin[i];
                if ((uint)(ch - '0') <= 9)
                {
                    if (n == digits.Length) return false;
                    digits[n++] = ch;
                }
            }

            return n == 16 || n == 18;
        }

        private static bool IsValidPrefix(ReadOnlySpan<char> d, int lastLen)
        {
            if (d[0] == '4' && d[1] == '1')
            {
                if (d[2] == '6' || (d[2] == '8' && d[3] == '0'))
                    return lastLen == 4;

                return false;
            }

            if (d[0] == '3' && d[1] == '1' && d[2] >= '1' && d[2] <= '9')
                return lastLen == 4;

            char c = d[0];
            return (c == '2' || c == '3' || c == '4' || c == '5') && lastLen == 6;
        }
    }
}
