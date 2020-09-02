using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Middleware
{
	public class SystemMonitor
	{
		private static readonly DriveInfo drive;
		private static readonly PerformanceCounter cpu;
		private static readonly PerformanceCounter mem;
		private static readonly PerformanceCounter disk_read;
		private static readonly PerformanceCounter disk_write;
		private static readonly PerformanceCounter net_sent;
		private static readonly PerformanceCounter net_received;

		public static long CpuUsage => (long)cpu.NextValue();
		public static long MemUsage => (long)mem.NextValue();
		public static long DiskRead => (long)disk_read.NextValue();
		public static long DiskWrite => (long)disk_write.NextValue();
		public static long DiskTotal => drive.TotalSize;
		public static long DiskUsed => drive.TotalSize - drive.TotalFreeSpace;
		public static long DiskFree => drive.TotalFreeSpace;
		public static long NetSent => (long)net_sent.NextValue();
		public static long NetReceived => (long)net_received.NextValue();
		public static long ConnectedUser { get; private set; } = 0;

		static SystemMonitor()
		{
			string diskname = XmlConfiguration.RootDirectory.Substring(0, 2).ToUpper();
			string[] disks = new PerformanceCounterCategory("PhysicalDisk").GetInstanceNames();
			string disk = disks.FirstOrDefault(s => s.IndexOf(diskname) > -1);

			string[] nets = new PerformanceCounterCategory("Network Interface").GetInstanceNames();
			string net = nets.FirstOrDefault();

			drive = new DriveInfo(diskname.Substring(0, 1));
			cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
			mem = new PerformanceCounter("Memory", "% Committed Bytes In Use");
			disk_read = new PerformanceCounter("PhysicalDisk", "% Disk Read Time", disk);
			disk_write = new PerformanceCounter("PhysicalDisk", "% Disk Write Time", disk);
			net_sent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", net);
			net_received = new PerformanceCounter("Network Interface", "Bytes Received/sec", net);
		}

		public static void UserConnected()
		{
			ConnectedUser++;
		}
		public static void UserDisconnected()
		{
			ConnectedUser--;
		}
	}
}