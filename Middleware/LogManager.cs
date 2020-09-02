using System;
using System.IO;
using System.Threading;
using Core.Collection;

namespace Middleware
{
	public class LogManager
	{
		private static readonly RingBuffer<(DateTime, string)> queue = new RingBuffer<(DateTime, string)>();
		private static readonly Thread WriteLog;

		static LogManager()
		{
			WriteLog = new Thread(new ThreadStart(() =>
			{
				while (true)
				{
					if (queue.Count > 0)
					{
						(var time, var message) = queue.Read();
						var path = $@"{XmlConfiguration.LogDirectory}log-{time.ToShortDateString()}.log";
						using (FileStream file = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
						using (StreamWriter writer = new StreamWriter(file))
						{
							writer.WriteLine(message);
						}
					}
					Thread.Sleep(100);
				}
			}));
			WriteLog.Start();
		}
		public static void Log(string client, string type, string url, string query = "")
		{
			lock (queue)
			{
				var now = DateTime.UtcNow;
				queue.Write((now, $"[LOG]\t{now.ToString()}\t{client}\t{type}\t{url}{(query == "" ? "" : $"?{query}")}"));
			}
		}
		public static void Log(string client, string type, string username, string url, string query = "")
		{
			lock (queue)
			{
				var now = DateTime.UtcNow;
				queue.Write((now, $"[LOG]\t{now.ToString()}\t{client}\t{type}\t{username}\t{url}{(query == "" ? "" : $"?{query}")}"));
			}
		}
		public static void Error(Exception e)
		{
			lock (queue)
			{
				var now = DateTime.UtcNow;
				queue.Write((now, $"[ERROR]\t{now.ToString()}\t{e.Message}\t{e.StackTrace}"));
			}
		}
	}
}