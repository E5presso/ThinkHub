using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using Middleware;

namespace Thinkhub
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Initializer.Initialize();
			CreateWebHostBuilder(args).Build().Run();
		}
		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>();
	}
}