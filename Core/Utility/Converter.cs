using System;
using System.Text;

namespace Core.Utility
{
	/// <summary>
	/// Core 데이터 형식을 다른 데이터 형식으로 변환합니다.
	/// </summary>
	public class Converter
	{
		/// <summary>
		/// 바이트 배열을 16진수 문자열로 변환합니다.
		/// </summary>
		/// <param name="data">변환할 바이트 배열입니다.</param>
		/// <returns>변환된 문자열입니다.</returns>
		public static string ToHexCode(byte[] data)
		{
			StringBuilder Hex = new StringBuilder();
			if (data != null)
			{
				int count = data.Length;
				for (int i = 0; i < count; i++)
				{
					Hex.AppendFormat("{0:x2}", data[i]);
				}

				return Hex.ToString();
			}
			else
			{
				throw new ArgumentException("인수는 NULL일 수 없습니다.");
			}
		}
		/// <summary>
		/// 16진수 문자열을 바이트 배열로 변환합니다.
		/// </summary>
		/// <param name="hex">변환할 16진수 문자열입니다.</param>
		/// <returns>변환된 바이트 배열입니다.</returns>
		public static byte[] GetBytes(string hex)
		{
			byte[] Bytes = new byte[hex.Length / 2];
			try
			{
				for (int i = 0; i < Bytes.Length; i++)
				{
					Bytes[i] = System.Convert.ToByte(hex.Substring(i * 2, 2), 16);
				}
			}
			catch { throw new ArgumentException("잘못된 형식입니다."); }
			return Bytes;
		}
	}
}