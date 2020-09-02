using System;
using System.IO;
using System.Security.Cryptography;

namespace Core.Security
{
	/// <summary>
	/// AES 암호화 기능을 제공하는 클래스입니다.
	/// </summary>
	public class Aes : IDisposable
	{
		private MemoryStream memory1;
		private MemoryStream memory2;
		private MemoryStream memory3;

		private CryptoStream encrypt;
		private CryptoStream decrypt;

		private static readonly MemoryStream s_memory1 = new MemoryStream();
		private static readonly MemoryStream s_memory2 = new MemoryStream();
		private static readonly MemoryStream s_decrypted = new MemoryStream();

		private static readonly RijndaelManaged s_Encryptor = new RijndaelManaged()
		{
			KeySize = 256,
			BlockSize = 128,
			Mode = CipherMode.CBC,
			Padding = PaddingMode.PKCS7
		};
		private static readonly RijndaelManaged s_Decryptor = new RijndaelManaged()
		{
			KeySize = 256,
			BlockSize = 128,
			Mode = CipherMode.CBC,
			Padding = PaddingMode.PKCS7
		};

		private static CryptoStream s_encrypt;
		private static CryptoStream s_decrypt;

		/// <summary>
		/// 암호기를 가져옵니다.
		/// </summary>
		public RijndaelManaged Encryptor { get; private set; }
		/// <summary>
		/// 복호기를 가져옵니다.
		/// </summary>
		public RijndaelManaged Decryptor { get; private set; }

		/// <summary>
		/// 대칭키를 가져옵니다.
		/// </summary>
		public byte[] Key { get; private set; }

		/// <summary>
		/// AES 클래스를 초기화합니다.
		/// </summary>
		public Aes()
		{
			Key = Security.Key.GenerateBytes(32);
			byte[] iv = new byte[16];
			Array.Copy(Hash.SHA1(Key), iv, 16);

			memory1 = new MemoryStream();
			memory2 = new MemoryStream();
			memory3 = new MemoryStream();

			Encryptor = new RijndaelManaged()
			{
				KeySize = 256,
				BlockSize = 128,
				Mode = CipherMode.CBC,
				Padding = PaddingMode.PKCS7,
				Key = Key,
				IV = iv
			};
			Decryptor = new RijndaelManaged()
			{
				KeySize = 256,
				BlockSize = 128,
				Mode = CipherMode.CBC,
				Padding = PaddingMode.PKCS7,
				Key = Key,
				IV = iv
			};

			encrypt = new CryptoStream(memory1, Encryptor.CreateEncryptor(Encryptor.Key, Encryptor.IV), CryptoStreamMode.Write, true);
			decrypt = new CryptoStream(memory2, Decryptor.CreateDecryptor(Decryptor.Key, Decryptor.IV), CryptoStreamMode.Read, true);
		}
		/// <summary>
		/// AES 클래스를 초기화합니다.
		/// </summary>
		/// <param name="key">암호화에 사용할 키를 지정합니다.</param>
		public Aes(byte[] key)
		{
			if (key.Length > 32) throw new ArgumentException("키의 길이가 32바이트를 넘을 수 없습니다.");
			Key = key;
			byte[] iv = new byte[16];
			Array.Copy(Hash.SHA1(key), iv, 16);

			memory1 = new MemoryStream();
			memory2 = new MemoryStream();
			memory3 = new MemoryStream();

			Encryptor = new RijndaelManaged()
			{
				KeySize = 256,
				BlockSize = 128,
				Mode = CipherMode.CBC,
				Padding = PaddingMode.PKCS7,
				Key = key,
				IV = iv
			};
			Decryptor = new RijndaelManaged()
			{
				KeySize = 256,
				BlockSize = 128,
				Mode = CipherMode.CBC,
				Padding = PaddingMode.PKCS7,
				Key = key,
				IV = iv
			};

			encrypt = new CryptoStream(memory1, Encryptor.CreateEncryptor(Encryptor.Key, Encryptor.IV), CryptoStreamMode.Write, true);
			decrypt = new CryptoStream(memory2, Decryptor.CreateDecryptor(Decryptor.Key, Decryptor.IV), CryptoStreamMode.Read, true);
		}

		/// <summary>
		/// 새로운 키를 지정합니다.
		/// </summary>
		/// <param name="key">지정할 키입니다.</param>
		public void SetKey(byte[] key)
		{
			Key = key;
			byte[] iv = new byte[16];
			Array.Copy(Hash.SHA1(key), iv, 16);

			memory1 = new MemoryStream();
			memory2 = new MemoryStream();
			memory3 = new MemoryStream();

			Encryptor = new RijndaelManaged()
			{
				Key = key,
				IV = iv
			};
			Decryptor = new RijndaelManaged()
			{
				Key = key,
				IV = iv
			};

			encrypt = new CryptoStream(memory1, Encryptor.CreateEncryptor(Encryptor.Key, Encryptor.IV), CryptoStreamMode.Write, true);
			decrypt = new CryptoStream(memory2, Decryptor.CreateDecryptor(Decryptor.Key, Decryptor.IV), CryptoStreamMode.Read, true);
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
				memory1.SetLength(0);
				encrypt.Write(data, 0, data.Length);
				encrypted = memory1.ToArray();
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
				memory2.SetLength(0);
				memory3.SetLength(0);
				memory2.Write(data, 0, data.Length);
				memory2.Position = 0;
				decrypt.CopyTo(memory3);
				decrypted = memory3.ToArray();
				return true;
			}
			catch
			{
				decrypted = default;
				return false;
			}
		}

		/// <summary>
		/// 데이터를 암호화합니다.
		/// </summary>
		/// <param name="key">암호화에 사용할 키입니다.</param>
		/// <param name="data">암호화할 데이터입니다.</param>
		/// <param name="encrypted">암호화된 데이터입니다.</param>
		/// <returns>암호화 결과입니다.</returns>
		public static bool Encrypt(byte[] key, byte[] data, out byte[] encrypted)
		{
			if (key.Length > 32) throw new ArgumentException("키의 길이가 32바이트를 넘을 수 없습니다.");
			try
			{
				byte[] iv = new byte[16];
				Array.Copy(Hash.SHA1(key), iv, 16);

				s_Encryptor.Key = key;
				s_Encryptor.IV = iv;
				s_encrypt = new CryptoStream(s_memory1, s_Encryptor.CreateEncryptor(s_Encryptor.Key, s_Encryptor.IV), CryptoStreamMode.Write);

				s_memory1.SetLength(0);
				s_encrypt.Write(data, 0, data.Length);
				s_encrypt.FlushFinalBlock();
				encrypted = s_memory1.ToArray();
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
		/// <param name="key">복호화에 사용할 키입니다.</param>
		/// <param name="data">복호화할 데이터입니다.</param>
		/// <param name="decrypted">복호화된 데이터입니다.</param>
		/// <returns>복호화 결과입니다.</returns>
		public static bool Decrypt(byte[] key, byte[] data, out byte[] decrypted)
		{
			if (key.Length > 32) throw new ArgumentException("키의 길이가 32바이트를 넘을 수 없습니다.");
			try
			{
				byte[] iv = new byte[16];
				Array.Copy(Hash.SHA1(key), iv, 16);

				s_Decryptor.Key = key;
				s_Decryptor.IV = iv;
				s_decrypt = new CryptoStream(s_memory2, s_Decryptor.CreateDecryptor(s_Decryptor.Key, s_Decryptor.IV), CryptoStreamMode.Read);

				s_memory2.SetLength(0);
				s_decrypted.SetLength(0);
				s_memory2.Write(data, 0, data.Length);
				s_memory2.Position = 0;
				s_decrypt.CopyTo(s_decrypted);
				decrypted = s_decrypted.ToArray();
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
		/// AES 클래스를 제거합니다.
		/// </summary>
		~Aes()
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
					memory1?.Dispose();
					memory2?.Dispose();
					memory3?.Dispose();

					Encryptor?.Dispose();
					Decryptor?.Dispose();

					encrypt?.Dispose();
					decrypt?.Dispose();
				}
				Key = null;
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