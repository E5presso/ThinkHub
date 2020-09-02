using System;
using System.Text;

namespace Core.Security
{
	/// <summary>
	/// Base64 변환기입니다.
	/// </summary>
	public class Base64
	{
		/// <summary>
		/// 바이트 시퀀스를 Base64 문자열로 변환합니다.
		/// </summary>
		/// <param name="binary">바이트 시퀀스입니다.</param>
		/// <returns>Base64 문자열입니다.</returns>
		public static string GetString(byte[] binary)
		{
			return Convert.ToBase64String(binary);
		}
		/// <summary>
		/// Base64 문자열을 바이트 시퀀스로 변환합니다.
		/// </summary>
		/// <param name="base64">Base64 문자열입니다.</param>
		/// <returns>바이트 시퀀스입니다.</returns>
		public static byte[] GetBytes(string base64)
		{
			return Convert.FromBase64String(base64);
		}

		/// <summary>
		/// UTF8문자열을 Base64로 인코딩합니다.
		/// </summary>
		/// <param name="utf8">인코딩할 UTF8문자열입니다.</param>
		/// <returns>인코딩된 Base64문자열입니다.</returns>
		public static string Encode(string utf8)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(utf8));
		}
		/// <summary>
		/// Base64문자열을 UTF8로 디코딩합니다.
		/// </summary>
		/// <param name="base64">디코딩할 Base64문자열입니다.</param>
		/// <returns>디코딩된 UTF8문자열입니다.</returns>
		public static string Decode(string base64)
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
		}
	}
}