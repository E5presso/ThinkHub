using System;
using System.Net;
using System.Net.Sockets;

namespace Core.Network.Tcp
{
	internal class Connector : IDisposable
	{
		private IPEndPoint point;
		private int size;
		private Socket socket;
		public string LocalIP;

		public event ConnectEvent Connected;
		public event ExceptionEvent ErrorOccurred;

		public void Start(string address, int port, int buffersize)
		{
			try
			{
				size = buffersize;
				IPAddress ip = Dns.GetHostAddresses(address)[0];
				point = new IPEndPoint(ip, port);
				socket = new Socket(AddressFamily.InterNetwork,
					SocketType.Stream, ProtocolType.Tcp);
				socket.BeginConnect(point,
					new AsyncCallback(ConnectCallback), null);
			}
			catch (Exception e) { ErrorOccurred?.BeginInvoke(e, new AsyncCallback((ar) => ErrorOccurred?.EndInvoke(ar)), null); }
		}
		private void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				socket.EndConnect(ar);
				LocalIP = socket.LocalEndPoint.ToString();
				Connected?.BeginInvoke(new Session(socket, size), new AsyncCallback((iar) => Connected?.EndInvoke(iar)), null);
			}
			catch (Exception e) { ErrorOccurred?.BeginInvoke(e, new AsyncCallback((iar) => ErrorOccurred?.EndInvoke(iar)), null); }
		}
		#region IDisposable Support
		private bool disposedValue = false;
		~Connector()
		{
			Dispose(false);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					socket.Dispose();
				}
				point = null;
				LocalIP = null;
				disposedValue = true;
			}
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}