using Core.Collection;
using System;
using System.Security.Cryptography;
using System.Xml;

namespace Core.Security
{
	internal static class RSACryptoServiceProviderExtensions
	{
		public static void FromXmlStringExtended(this RSACryptoServiceProvider rsa, string xmlString)
		{
			RSAParameters parameters = new RSAParameters();

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlString);

			if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
			{
				foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
				{
					switch (node.Name)
					{
						case "Modulus": parameters.Modulus = Convert.FromBase64String(node.InnerText); break;
						case "Exponent": parameters.Exponent = Convert.FromBase64String(node.InnerText); break;
						case "P": parameters.P = Convert.FromBase64String(node.InnerText); break;
						case "Q": parameters.Q = Convert.FromBase64String(node.InnerText); break;
						case "DP": parameters.DP = Convert.FromBase64String(node.InnerText); break;
						case "DQ": parameters.DQ = Convert.FromBase64String(node.InnerText); break;
						case "InverseQ": parameters.InverseQ = Convert.FromBase64String(node.InnerText); break;
						case "D": parameters.D = Convert.FromBase64String(node.InnerText); break;
					}
				}
			}
			else throw new Exception("Invalid XML RSA key.");
			rsa.ImportParameters(parameters);
		}
		public static string ToXmlStringExtended(this RSACryptoServiceProvider rsa, bool includePrivateParameters)
		{
			RSAParameters parameters = rsa.ExportParameters(includePrivateParameters);
			if (includePrivateParameters)
			{
				return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
					Convert.ToBase64String(parameters.Modulus),
					Convert.ToBase64String(parameters.Exponent),
					Convert.ToBase64String(parameters.P),
					Convert.ToBase64String(parameters.Q),
					Convert.ToBase64String(parameters.DP),
					Convert.ToBase64String(parameters.DQ),
					Convert.ToBase64String(parameters.InverseQ),
					Convert.ToBase64String(parameters.D));
			}
			else
			{
				return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
					Convert.ToBase64String(parameters.Modulus),
					Convert.ToBase64String(parameters.Exponent));
			}
		}
	}
	/// <summary>
	/// 해쉬 타입을 지정합니다.
	/// </summary>
	public enum HashType
	{
		/// <summary>
		/// SHA1 해쉬입니다.
		/// </summary>
		SHA1,
		/// <summary>
		/// SHA256 해쉬입니다.
		/// </summary>
		SHA256,
		/// <summary>
		/// SHA384 해쉬입니다.
		/// </summary>
		SHA384,
		/// <summary>
		/// SHA512 해쉬입니다.
		/// </summary>
		SHA512
	}
	/// <summary>
	/// RSA 암호화 기능을 제공하는 클래스입니다..
	/// </summary>
	public class Rsa : IDisposable
	{
		private string privateKey;
		private readonly RingBuffer<byte> buffer;
		private readonly RSACryptoServiceProvider service;

		private static readonly RSACryptoServiceProvider s_service = new RSACryptoServiceProvider();

		/// <summary>
		/// RSA 공개키입니다.
		/// </summary>
		public string PublicKey;
		/// <summary>
		/// RSA 개인키입니다.
		/// </summary>
		public string PrivateKey
		{
			get => privateKey;
			set
			{
				privateKey = value;
				service.FromXmlStringExtended(value);
				PublicKey = service.ToXmlStringExtended(false);
			}
		}

		/// <summary>
		/// RSA 클래스를 초기화합니다.
		/// </summary>
		public Rsa()
		{
			buffer = new RingBuffer<byte>();
			service = new RSACryptoServiceProvider();
			PrivateKey = service.ToXmlStringExtended(true);
		}
		/// <summary>
		/// RSA 클래스를 초기화합니다.
		/// </summary>
		/// <param name="certificate">X.509 인증서를 지정합니다.</param>
		public Rsa(X509 certificate)
		{
			buffer = new RingBuffer<byte>();
			service = new RSACryptoServiceProvider();
			if (certificate.HasPrivateKey) PrivateKey = certificate.PrivateKey;
			else
			{
				privateKey = service.ToXmlStringExtended(true);
				PublicKey = certificate.PublicKey;
			}
		}

		/// <summary>
		/// 데이터를 암호화합니다.
		/// </summary>
		/// <param name="data">암호화할 데이터입니다.</param>
		/// <param name="encrypted">암호화된 데이터입니다.</param>
		/// <returns>암호화 결과입니다.</returns>
		public bool Encrypt(byte[] data, out byte[] encrypted)
		{
			try
			{
				service.FromXmlStringExtended(PublicKey);
				encrypted = service.Encrypt(data, true);
				return true;
			}
			catch
			{
				encrypted = default;
				return false;
			}
		}
		/// <summary>
		/// 데이터를 복호화합니다.
		/// </summary>
		/// <param name="data">복호화할 데이터입니다.</param>
		/// <param name="decrypted">복호화된 데이터입니다.</param>
		/// <returns>복호화 결과입니다.</returns>
		public bool Decrypt(byte[] data, out byte[] decrypted)
		{
			try
			{
				service.FromXmlStringExtended(privateKey);
				decrypted = service.Decrypt(data, true);
				return true;
			}
			catch
			{
				decrypted = default;
				return false;
			}
		}

		/// <summary>
		/// 데이터에 서명합니다.
		/// </summary>
		/// <param name="data">서명할 데이터입니다.</param>
		/// <param name="type">서명에 사용할 해쉬함수를 지정합니다.</param>
		/// <returns>서명된 데이터입니다.</returns>
		public byte[] Sign(byte[] data, HashType type = HashType.SHA256)
		{
			try
			{
				buffer.Clear();
				service.FromXmlStringExtended(privateKey);

				byte[] sign;
				switch (type)
				{
					case HashType.SHA1:
					{
						sign = service.SignData(data, new SHA1CryptoServiceProvider());
						break;
					}
					case HashType.SHA256:
					{
						sign = service.SignData(data, new SHA256CryptoServiceProvider());
						break;
					}
					case HashType.SHA384:
					{
						sign = service.SignData(data, new SHA384CryptoServiceProvider());
						break;
					}
					case HashType.SHA512:
					{
						sign = service.SignData(data, new SHA512CryptoServiceProvider());
						break;
					}
					default:
					{
						sign = service.SignData(data, new SHA256CryptoServiceProvider());
						break;
					}
				}

				byte[] dLength = BitConverter.GetBytes(data.Length);
				byte[] sLength = BitConverter.GetBytes(sign.Length);

				buffer.Write((byte)type);
				buffer.Write(dLength);
				buffer.Write(sLength);
				buffer.Write(data);
				buffer.Write(sign);

				byte[] result = buffer.ToArray();
				buffer.Clear();
				return result;
			}
			catch { throw; }
		}
		/// <summary>
		/// 서명된 데이터를 검증합니다.
		/// </summary>
		/// <param name="signedData">서명된 데이터입니다.</param>
		/// <returns>검증 결과입니다.</returns>
		public bool Verify(byte[] signedData)
		{
			try
			{
				buffer.Clear();
				buffer.Write(signedData);

				HashType type = (HashType)buffer.Read();
				int dLength = BitConverter.ToInt32(buffer.Read(4), 0);
				int sLength = BitConverter.ToInt32(buffer.Read(4), 0);
				byte[] data = buffer.Read(dLength);
				byte[] sign = buffer.Read(sLength);

				service.FromXmlStringExtended(PublicKey);
				switch(type)
				{
					case HashType.SHA1:
					{
						return service.VerifyData(data, new SHA1CryptoServiceProvider(), sign);
					}
					case HashType.SHA256:
					{
						return service.VerifyData(data, new SHA256CryptoServiceProvider(), sign);
					}
					case HashType.SHA384:
					{
						return service.VerifyData(data, new SHA384CryptoServiceProvider(), sign);
					}
					case HashType.SHA512:
					{
						return service.VerifyData(data, new SHA512CryptoServiceProvider(), sign);
					}
					default:
					{
						return service.VerifyData(data, new SHA256CryptoServiceProvider(), sign);
					}
				};
			}
			catch { throw; }
		}

		/// <summary>
		/// 데이터를 암호화합니다.
		/// </summary>
		/// <param name="publicKey">암호화에 사용할 공개키를 지정합니다.</param>
		/// <param name="data">암호화할 데이터입니다.</param>
		/// <param name="encrypted">암호화된 데이터입니다.</param>
		/// <returns>암호화 결과입니다.</returns>
		public static bool Encrypt(string publicKey, byte[] data, out byte[] encrypted)
		{
			try
			{
				s_service.FromXmlStringExtended(publicKey);
				encrypted = s_service.Encrypt(data, true);
				return true;
			}
			catch
			{
				encrypted = default;
				return false;
			}
		}
		/// <summary>
		/// 데이터를 복호화합니다.
		/// </summary>
		/// <param name="privatekey">복호화에 사용할 개인키를 지정합니다.</param>
		/// <param name="data">복호화할 데이터입니다.</param>
		/// <param name="decrypted">복호화된 데이터입니다.</param>
		/// <returns>복호화 결과입니다.</returns>
		public static bool Decrypt(string privatekey, byte[] data, out byte[] decrypted)
		{
			try
			{
				s_service.FromXmlStringExtended(privatekey);
				decrypted = s_service.Decrypt(data, true);
				return true;
			}
			catch
			{
				decrypted = default;
				return false;
			}
		}
		#region IDisposable Support
		private bool disposedValue = false;
		/// <summary>
		/// RSA 클래스를 제거합니다.
		/// </summary>
		~Rsa()
		{
			Dispose(false);
		}
		/// <summary>
		/// IDisposable 패턴을 구현합니다.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					service?.Dispose();
				}
				PublicKey = string.Empty;
				privateKey = string.Empty;
				disposedValue = true;
			}
		}
		/// <summary>
		/// 클래스를 제거하고 리소스를 반환합니다.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}