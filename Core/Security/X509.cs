using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace Core.Security
{
	internal static class RSAExtensions
	{
		public static void FromXmlStringExtended(this System.Security.Cryptography.RSA rsa, string xmlString)
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
		public static string ToXmlStringExtended(this System.Security.Cryptography.RSA rsa, bool includePrivateParameters)
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
	/// Windows Certificate Store 저장소의 종류를 정의합니다.
	/// </summary>
	public enum LocationType
	{
		/// <summary>
		/// 현재 사용자의 저장소입니다.
		/// </summary>
		CurrentUser = 1,
		/// <summary>
		/// 현재 컴퓨터의 저장소입니다.
		/// </summary>
		LocalMachine = 2
	}
	/// <summary>
	/// X.509 인증서 개체를 나타냅니다.
	/// </summary>
	public struct X509
	{
		private readonly X509Certificate cert;
		private readonly X509Certificate2 cert2;

		/// <summary>
		/// 인증서가 개인키를 포함하는지에 대한 여부를 가져옵니다.
		/// </summary>
		public bool HasPrivateKey => cert2.HasPrivateKey;
		/// <summary>
		/// 인증서의 개인키를 가져옵니다.
		/// </summary>
		public string PrivateKey
		{
			get
			{
				if (cert2.HasPrivateKey) return cert2.GetRSAPrivateKey().ToXmlStringExtended(true);
				else return default;
			}
		}
		/// <summary>
		/// 인증서의 공개키를 가져옵니다.
		/// </summary>
		public string PublicKey => cert2.GetRSAPublicKey().ToXmlStringExtended(false);
		/// <summary>
		/// 직렬화된 인증서 개체를 가져옵니다.
		/// </summary>
		public byte[] Serialized => cert.Export(X509ContentType.Cert);

		/// <summary>
		/// 직렬화된 인증서 개체를 이용해 X509 구조체를 초기화합니다.
		/// </summary>
		/// <param name="serialized">직렬화된 인증서 개체를 지정합니다.</param>
		public X509(byte[] serialized)
		{
			cert = new X509Certificate(serialized);
			cert2 = new X509Certificate2(cert);
		}
		/// <summary>
		/// 지정한 위치의 공개키 인증서를 이용해 X509 구조체를 초기화합니다.
		/// </summary>
		/// <param name="path">인증서의 위치를 지정합니다.</param>
		public X509(string path)
		{
			cert = new X509Certificate(path);
			cert2 = new X509Certificate2(cert);
		}
		/// <summary>
		/// 개인키 인증서 위치와 비밀번호를 이용해 X509 구조체를 초기화합니다.
		/// </summary>
		/// <param name="path">인증서의 위치를 지정합니다.</param>
		/// <param name="password">인증서의 비밀번호를 지정합니다.</param>
		public X509(string path, string password)
		{
			cert = new X509Certificate(path, password, X509KeyStorageFlags.Exportable);
			cert2 = new X509Certificate2(cert);
		}
		/// <summary>
		/// Windows Certificate Store 위치와 인증서 주체를 이용해 X509 구조체를 초기화합니다.
		/// </summary>
		/// <param name="location">Windows Certificate Store 저장소의 종류를 지정합니다.</param>
		/// <param name="subject">인증서의 주체를 지정합니다.</param>
		public X509(LocationType location, string subject)
		{
			X509Store store = new X509Store((StoreLocation)location);
			store.Open(OpenFlags.ReadOnly);
			X509CertificateCollection certificates = store.Certificates.Find(X509FindType.FindBySubjectName, subject, true);
			store.Dispose();
			if (certificates.Count > 0)
			{
				cert = certificates[0];
				cert2 = new X509Certificate2(cert);
			}
			else throw new InvalidOperationException("인증서를 찾을 수 없습니다.");
		}

		/// <summary>
		/// 인증서의 유효성 검사를 수행합니다.
		/// </summary>
		/// <param name="domain">확인할 도메인을 지정합니다.</param>
		/// <returns>유효성 검사 결과입니다.</returns>
		public bool Verify(string domain)
		{
			bool verified = cert2.Verify();
			string subject = cert2.GetNameInfo(X509NameType.DnsName, false);

			return verified && subject == domain;
		}
	}
}