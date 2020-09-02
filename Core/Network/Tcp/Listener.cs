using System;
using System.Net;
using System.Net.Sockets;

namespace Core.Network.Tcp
{
	internal class Listener : IDisposable
	{
		private bool flag = false;
		private readonly Socket listener;

		public string IP => listener.LocalEndPoint.ToString();

		public event ConnectEvent Connected;
		public event ExceptionEvent ErrorOccurred;

		public Listener()
		{
			listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public void Open(int port, int backlog, int buffersize)
		{
			var localEndPoint = new IPEndPoint(IPAddress.Any, port);
			listener.Bind(localEndPoint);
			listener.Listen(backlog);
			void Callback(IAsyncResult ar)
			{
				try
				{
					var client = listener.EndAccept(ar);
					if (flag)
					{
						listener.BeginAccept(new AsyncCallback(Callback), null);
					}

					Connected(new Session(client, buffersize));
				}
				catch (Exception e) { ErrorOccurred(e); }
			}
			try
			{
				flag = true;
				listener.BeginAccept(new AsyncCallback(Callback), null);
			}
			catch (Exception e) { ErrorOccurred(e); }
		}
		public void Close()
		{
			flag = false;
		}
		#region IDisposable Support
		private bool disposedValue = false;
		~Listener()
		{
			Dispose(false);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					listener.Dispose();
				}
				Close();
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