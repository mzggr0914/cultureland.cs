using System;

namespace cultureland.cs
{
	/// <summary>
	/// 커스텀 오류 클래스입니다.
	/// 오류가 발생할 경우 Error 대신 이 클래스가 반환됩니다.
	/// </summary>
	public class CulturelandError : Exception
	{
		private readonly CulturelandErrorNames _name;

		/// <summary>
		/// 오류 이름
		/// </summary>
		public CulturelandErrorNames Name => _name;

		/// <summary>
		/// 커스텀 오류 클래스의 생성자입니다.
		/// </summary>
		/// <param name="name">오류 이름</param>
		/// <param name="message">오류 메시지</param>
		/// <param name="additionalValues">추가 값</param>
		public CulturelandError(CulturelandErrorNames name, string message, object additionalValues = null)
			: base(message)
		{
			_name = name;
			AdditionalValues = additionalValues;
		}

		/// <summary>
		/// 추가 값
		/// </summary>
		public object AdditionalValues { get; }
	}
}
