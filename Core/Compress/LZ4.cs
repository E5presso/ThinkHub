using LZ4;

namespace Core.Compress
{
	/// <summary>
	/// LZ4 압축알고리즘을 구현한 클래스입니다.
	/// </summary>
	public class Lz4
	{
		/// <summary>
		/// 데이터를 압축합니다.
		/// </summary>
		/// <param name="data">압축할 데이터입니다.</param>
		/// <returns>압축된 데이터입니다.</returns>
		public static byte[] Compress(byte[] data)
		{
			return LZ4Codec.Wrap(data);
		}
		/// <summary>
		/// 데이터를 압축해제합니다.
		/// </summary>
		/// <param name="data">압축해제할 데이터입니다.</param>
		/// <returns>압축해제된 데이터입니다.</returns>
		public static byte[] Decompress(byte[] data)
		{
			return LZ4Codec.Unwrap(data);
		}
	}
}