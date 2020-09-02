using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Middleware
{
	public class XmlConfiguration
	{
		private static readonly string ConfigPath = "configuration.xml";
		private static readonly XmlDocument config = new XmlDocument();

		public static string Domain => config["configuration"]["domain"].InnerText;

		public static string AdminUserName => config["configuration"]["admin"]["username"].InnerText;
		public static string AdminPassword => config["configuration"]["admin"]["password"].InnerText;

		public static string FFMpeg => config["configuration"]["ffmpeg"]["path"].InnerText;

		public static string HomeMask => config["configuration"]["masks"]["home"].InnerText;
		public static string SharedMask => config["configuration"]["masks"]["shared"].InnerText;
		public static string UploadMask => config["configuration"]["masks"]["upload"].InnerText;

		public static string RootDirectory => config["configuration"]["directories"]["root"].InnerText;
		public static string UserDirectory => config["configuration"]["directories"]["users"].InnerText;
		public static string TempDirectory => config["configuration"]["directories"]["temp"].InnerText;
		public static string LogDirectory => config["configuration"]["directories"]["log"].InnerText;

		public static string HomeDirectory => config["configuration"]["subdirectories"]["home"].InnerText;
		public static string UploadDirectory => config["configuration"]["subdirectories"]["upload"].InnerText;

		public static string VerifyTemplate => config["configuration"]["email"]["verify"]["template"].InnerText;
		public static string VerifyTitle => config["configuration"]["email"]["verify"]["title"].InnerText;
		public static string VerifyLink => config["configuration"]["email"]["verify"]["link"].InnerText;
		public static string[] VerifyFilter
		{
			get
			{
				var result = new List<string>();
				var list = config["configuration"]["email"]["verify"].GetElementsByTagName("filter");
				for (int i = 0; i < list.Count; i++) result.Add(list.Item(i).InnerText);
				return result.ToArray();
			}
		}

		public static string ResetTemplate => config["configuration"]["email"]["reset"]["template"].InnerText;
		public static string ResetTitle => config["configuration"]["email"]["reset"]["title"].InnerText;
		public static string ResetPassword => config["configuration"]["email"]["reset"]["password"].InnerText;
		public static string[] ResetFilter
		{
			get
			{
				var result = new List<string>();
				var list = config["configuration"]["email"]["reset"].GetElementsByTagName("filter");
				for (int i = 0; i < list.Count; i++) result.Add(list.Item(i).InnerText);
				return result.ToArray();
			}
		}

		static XmlConfiguration()
		{
			using (FileStream stream = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				config.Load(stream);
			}
		}
	}
}