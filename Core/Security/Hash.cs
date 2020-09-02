using Core.Utility;
using System.Security.Cryptography;
using System.Text;

namespace Core.Security
{
	/// <summary>
	/// 해쉬함수 모음입니다.
	/// </summary>
	public class Hash
	{
		private static readonly MD5CryptoServiceProvider MD5Provider = new MD5CryptoServiceProvider();
		private static readonly SHA1CryptoServiceProvider SHA1Provider = new SHA1CryptoServiceProvider();
		private static readonly SHA256CryptoServiceProvider SHA256Provider = new SHA256CryptoServiceProvider();
		private static readonly SHA384CryptoServiceProvider SHA384Provider = new SHA384CryptoServiceProvider();
		private static readonly SHA512CryptoServiceProvider SHA512Provider = new SHA512CryptoServiceProvider();

		/// <summary>
		/// MD5 해쉬코드를 생성합니다.
		/// </summary>
		/// <param name="content">해쉬코드 생성에 사용할 문자열입니다.</param>
		/// <returns>생성된 MD5 해쉬코드입니다.</returns>
		public static string MD5(string content)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			byte[] HashContent = MD5Provider.ComputeHash(bytes);
			return Converter.ToHexCode(HashContent);
		}
		/// <summary>
		/// 솔트값을 이용해 MD5 해쉬코드를 생성합니다.
		/// </summary>
		/// <param name="content">해쉬코드 생성에 사용할 문자열입니다.</param>
		/// <param name="salt">해쉬에 포함할 솔트값을 지정합니다.</param>
		/// <returns>생성된 MD5 해쉬코드입니다.</returns>
		public static string MD5(string content, string salt)
		{
			content += salt;
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			byte[] HashContent = MD5Provider.ComputeHash(bytes);
			return Converter.ToHexCode(HashContent);
		}
		/// <summary>
		/// MD5 해쉬 데이터를 생성합니다.
		/// </summary>
		/// <param name="data">해쉬데이터 생성에 사용할 데이터입니다.</param>
		/// <returns>생성된 MD5 해쉬 데이터입니다.</returns>
		public static byte[] MD5(byte[] data)
		{
			return MD5Provider.ComputeHash(data);
		}

		/// <summary>
		/// SHA1 해쉬코드를 생성합니다.
		/// </summary>
		/// <param name="content">해쉬코드 생성에 사용할 문자열입니다.</param>
		/// <returns>생성된 SHA1 해쉬코드입니다.</returns>
		public static string SHA1(string content)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			byte[] HashContent = SHA1Provider.ComputeHash(bytes);
			return Converter.ToHexCode(HashContent);
		}
		/// <summary>
		/// 솔트값을 이용해 SHA1 해쉬코드를 생성합니다.
		/// </summary>
		/// <param name="content">해쉬코드 생성에 사용할 문자열입니다.</param>
		/// <param name="salt">해쉬에 포함할 솔트값을 지정합니다.</param>
		/// <returns>생성된 SHA1 해쉬코드입니다.</returns>
		public static string SHA1(string content, string salt)
		{
			content += salt;
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			byte[] HashContent = SHA1Provider.ComputeHash(bytes);
			return Converter.ToHexCode(HashContent);
		}
		/// <summary>
		/// SHA1 해쉬 데이터를 생성합니다.
		/// </summary>
		/// <param name="data">해쉬 데이터 생성에 사용할 데이터입니다.</param>
		/// <returns>생성된 SHA1 해쉬 데이터입니다.</returns>
		public static byte[] SHA1(byte[] data)
		{
			return SHA1Provider.ComputeHash(data);
		}

		/// <summary>
		/// SHA256 해쉬코드를 생성합니다.
		/// </summary>
		/// <param name="content">해쉬코드 생성에 사용할 문자열입니다.</param>
		/// <returns>생성된 SHA256 해쉬코드입니다.</returns>
		public static string SHA256(string content)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			byte[] HashContent = SHA256Provider.ComputeHash(bytes);
			return Converter.ToHexCode(HashContent);
		}
		/// <summary>
		/// 솔트값을 이용해 SHA256 해쉬코드를 생성합니다.
		/// </summary>
		/// <param name="content">해쉬코드 생성에 사용할 문자열입니다.</param>
		/// <param name="salt">해쉬에 포함할 솔트값을 지정합니다.</param>
		/// <returns>생성된 SHA256 해쉬코드입니다.</returns>
		public static string SHA256(string content, string salt)
		{
			content += salt;
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			byte[] HashContent = SHA256Provider.ComputeHash(bytes);
			return Converter.ToHexCode(HashContent);
		}
		/// <summary>
		/// SHA256 해쉬 데이터를 생성합니다.
		/// </summary>
		/// <param name="data">해쉬 데이터 생성에 사용할 데이터입니다.</param>
		/// <returns>생성된 SHA256 해쉬 데이터입니다.</returns>
		public static byte[] SHA256(byte[] data)
		{
			return SHA256Provider.ComputeHash(data);
		}

		/// <summary>
		/// SHA384 해쉬코드를 생성합니다.
		/// </summary>
		/// <param name="content">해쉬코드 생성에 사용할 문자열입니다.</param>
		/// <returns>생성된 SHA384 해쉬코드입니다.</returns>
		public static string SHA384(string content)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			byte[] HashContent = SHA384Provider.ComputeHash(bytes);
			return Converter.ToHexCode(HashContent);
		}
		/// <summary>
		/// 솔트값을 이용해 SHA384 해쉬코드를 생성합니다.
		/// </summary>
		/// <param name="content">해쉬코드 생성에 사용할 문자열입니다.</param>
		/// <param name="salt">해쉬에 포함할 솔트값을 지정합니다.</param>
		/// <returns>생성된 SHA384 해쉬코드입니다.</returns>
		public static string SHA384(string content, string salt)
		{
			content += salt;
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			byte[] HashContent = SHA384Provider.ComputeHash(bytes);
			return Converter.ToHexCode(HashContent);
		}
		/// <summary>
		/// SHA384 해쉬 데이터를 생성합니다.
		/// </summary>
		/// <param name="data">해쉬 데이터 생성에 사용할 데이터입니다.</param>
		/// <returns>생성된 SHA384 해쉬 데이터입니다.</returns>
		public static byte[] SHA384(byte[] data)
		{
			return SHA384Provider.ComputeHash(data);
		}

		/// <summary>
		/// SHA512 해쉬코드를 생성합니다.
		/// </summary>
		/// <param name="content">해쉬코드 생성에 사용할 문자열입니다.</param>
		/// <returns>생성된 SHA512 해쉬코드입니다.</returns>
		public static string SHA512(string content)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			byte[] HashContent = SHA512Provider.ComputeHash(bytes);
			return Converter.ToHexCode(HashContent);
		}
		/// <summary>
		/// 솔트값을 이용해 SHA512 해쉬코드를 생성합니다.
		/// </summary>
		/// <param name="content">해쉬코드 생성에 사용할 문자열입니다.</param>
		/// <param name="salt">해쉬에 포함할 솔트값을 지정합니다.</param>
		/// <returns>생성된 SHA512 해쉬코드입니다.</returns>
		public static string SHA512(string content, string salt)
		{
			content += salt;
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			byte[] HashContent = SHA512Provider.ComputeHash(bytes);
			return Converter.ToHexCode(HashContent);
		}
		/// <summary>
		/// SHA512 해쉬 데이터를 생성합니다.
		/// </summary>
		/// <param name="data">해쉬 데이터 생성에 사용할 데이터입니다.</param>
		/// <returns>생성된 SHA512 해쉬 데이터입니다.</returns>
		public static byte[] SHA512(byte[] data)
		{
			return SHA512Provider.ComputeHash(data);
		}
	}
}