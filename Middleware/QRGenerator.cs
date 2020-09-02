using System.Drawing.Imaging;
using System.IO;
using Core.Security;
using QRCoder;

namespace Middleware
{
	public class QRGenerator
	{
		private static readonly QRCodeGenerator generator = new QRCodeGenerator();
		public static string GetQRCode(string data)
		{
			var code = generator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
			using (var qrCode = new QRCode(code))
			using (var image = qrCode.GetGraphic(20))
			using (var stream = new MemoryStream())
			{
				image.Save(stream, ImageFormat.Png);
				byte[] buffer = stream.ToArray();
				string test = "12341234";
				return $@"data:image/jpg;base64,{Base64.GetString(buffer)}";
			}
		}
	}
}