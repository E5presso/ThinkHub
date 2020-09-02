using System.IO;

namespace Middleware
{
	public class Initializer
	{
		public static void Initialize()
		{
			if (!Directory.Exists(XmlConfiguration.RootDirectory)) Directory.CreateDirectory(XmlConfiguration.RootDirectory);
			if (!Directory.Exists(XmlConfiguration.UserDirectory)) Directory.CreateDirectory(XmlConfiguration.UserDirectory);
			if (!Directory.Exists(XmlConfiguration.TempDirectory)) Directory.CreateDirectory(XmlConfiguration.TempDirectory);
			if (!Directory.Exists(XmlConfiguration.LogDirectory)) Directory.CreateDirectory(XmlConfiguration.LogDirectory);
		}
	}
}