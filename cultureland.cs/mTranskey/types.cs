using System.Collections.Generic;

namespace cultureland.cs.mTranskey
{
	public class ServletData
	{
		public string requestToken { get; set; }
		public string initTime { get; set; }
		public KeyInfo keyInfo { get; set; }
	}

	public class KeyInfo
	{
		public List<(int, int)> qwerty { get; set; }
		public List<(int, int)> number { get; set; }
	}
}