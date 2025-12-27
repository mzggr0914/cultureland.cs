using System.Linq;
using System.Text.RegularExpressions;

namespace cultureland.cs
{
	public class Pin
	{
		private readonly string[] _parts;

		// 핀번호를 자동으로 포맷팅합니다.
		public Pin(string pin)
		{
			var validationResult = ValidateClientSide(pin);
			if (!validationResult)
				throw new CulturelandError(CulturelandErrorNames.PinValidationError, "존재하지 않는 상품권입니다.");
			_parts = Format(pin);
		}

		public Pin(string pinPart1, string pinPart2, string pinPart3, string pinPart4)
			: this($"{pinPart1}-{pinPart2}-{pinPart3}-{pinPart4}") { }

		// 포맷팅이 완료된 핀번호입니다.
		public string[] Parts
		{
			get { return _parts; }
		}

		// 핀번호를 string으로 반환합니다.
		public override string ToString()
		{
			return string.Join("-", Parts);
		}

		// 핀번호를 포맷팅하여 CulturelandPinParts 형식으로 반환합니다.
		public static string[] Format(string pin)
		{
			if (!ValidateClientSide(pin)) return null;
			var pinMatches = Regex.Match(pin, @"(\d{4})\D*(\d{4})\D*(\d{4})\D*(\d{6}|\d{4})");
			if (!pinMatches.Success) return null;

			return new string[] { pinMatches.Groups[1].Value, pinMatches.Groups[2].Value, pinMatches.Groups[3].Value, pinMatches.Groups[4].Value };
		}

		// 핀번호의 유효성을 검증합니다.
		public static bool ValidateClientSide(string pin)
		{
			var pinMatches = Regex.Match(pin, @"(\d{4})\D*(\d{4})\D*(\d{4})\D*(\d{6}|\d{4})");
			if (!pinMatches.Success) return false;
			string[] blacklist = { "2", "3", "4", "5" };
			var parts = new string[] { pinMatches.Groups[1].Value, pinMatches.Groups[2].Value, pinMatches.Groups[3].Value, pinMatches.Groups[4].Value };

			if (parts[0].StartsWith("416") || parts[0].StartsWith("4180"))
			{
				if (parts[3].Length != 4)
					return false;
			}
			else if (parts[0].StartsWith("41"))
			{
				return false;
			}
			else if (Regex.IsMatch(parts[0], @"^31[1-9]") && parts[3].Length == 4)
			{
				// 검증 성공
			}
			else if (blacklist.Contains(parts[0][0].ToString()))
			{
				if (parts[3].Length != 6)
					return false;
			}
			else
			{
				return false;
			}

			return true;
		}
	}
}