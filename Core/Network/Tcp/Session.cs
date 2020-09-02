using Core.Collection;
using System;
using System.ComponentModel;
using System.Net.Sockets;

namespace Core.Network.Tcp
{
	internal class Session : IDisposable
	{
		private bool isClosed = false;
		private readonly int buffersize;

		private struct Status
		{
			public enum Phase
			{
				GET_HEADER,
				GET_DATA
			}
			public Phase phase;
			public int command;
			public int length;
		}
		private Status status;
		private readonly Socket socket;
		private readonly RingBuffer<byte> messageQueue;
		private readonly byte[] buffer;

		public string IP { get; }

		public event SendEvent Sended;
		public event ReceiveEvent Received;
		public event DisconnectEvent Disconnected;
		public event ExceptionEvent ErrorOccurred;

		public Session(Socket socket, int buffersize)
		{
			this.socket = socket;
			this.buffersize = buffersize;

			IP = socket.RemoteEndPoint.ToString();
			buffer = new byte[buffersize];
			messageQueue = new RingBuffer<byte>();
			if (socket.Connected)
			{
				try
				{
					isClosed = false;
					socket.BeginReceive(buffer, 0, buffersize, 0, new AsyncCallback(OnReceived), null);
				}
				catch (Exception e) { ErrorOccurred?.Invoke(e); }
			}
		}

		public void Send(int command, byte[] data)
		{
			if (socket.Connected)
			{
				byte[] packet = new byte[8 + data.Length];
				byte[] cmd = BitConverter.GetBytes(command);
				byte[] length = BitConverter.GetBytes(data.Length);

				Array.Reverse(cmd);
				Array.Reverse(length);

				Buffer.BlockCopy(cmd, 0, packet, 0, 4);
				Buffer.BlockCopy(length, 0, packet, 4, 4);
				Buffer.BlockCopy(data, 0, packet, 8, data.Length);
				try
				{
					socket.BeginSend(packet, 0, packet.Length, 0, new AsyncCallback((ar) =>
					{
						int bytesSent = socket.EndSend(ar) - 8;
						Sended?.Invoke(this, command, bytesSent);
					}), null);
				}
				catch (Exception e) { ErrorOccurred?.Invoke(e); }
			}
			else
			{
				ErrorOccurred?.Invoke(new InvalidOperationException("현재 소켓이 연결되어있지 않습니다."));
			}
		}
		public void Close()
		{
			if (socket.Connected && !isClosed)
			{
				try
				{
					isClosed = true;
					socket.Shutdown(SocketShutdown.Both);
					socket.BeginDisconnect(false, new AsyncCallback((ar) =>
					{
						socket.EndDisconnect(ar);
						Disconnected?.Invoke(this);
					}), null);
				}
				catch (Exception e) { ErrorOccurred?.Invoke(e); }
			}
			else
			{
				ErrorOccurred?.Invoke(new InvalidOperationException("현재 소켓이 연결되어있지 않습니다."));
			}
		}

		private void OnReceived(IAsyncResult ar)
		{
			try
			{
				int bytesRead = socket.EndReceive(ar);
				if (bytesRead > 0)
				{
					messageQueue.Write(buffer, 0, bytesRead);
					Array.Clear(buffer, 0, bytesRead);
					GetPacket();
				}
				else
				{
					Close();
				}
			}
			catch (Exception e)
			{
				if (e is Win32Exception w && w.ErrorCode == 10054)
				{
					Disconnected?.Invoke(this);
				}
				else
				{
					ErrorOccurred?.Invoke(e);
				}
			}
		}
		private void GetPacket()
		{
			if (status.phase == Status.Phase.GET_HEADER)
			{
				if (messageQueue.Count >= 8)
				{
					byte[] cmd = messageQueue.Read(4);
					byte[] length = messageQueue.Read(4);

					Array.Reverse(cmd);
					Array.Reverse(length);

					status.command = BitConverter.ToInt32(cmd, 0);
					status.length = BitConverter.ToInt32(length, 0);
					status.phase = Status.Phase.GET_DATA;
					if (messageQueue.Count > 0)
					{
						GetPacket();
					}
					else if (!isClosed)
					{
						socket.BeginReceive(buffer, 0, buffersize, 0, new AsyncCallback(OnReceived), null);
					}
				}
			}
			else if (status.phase == Status.Phase.GET_DATA)
			{
				if (messageQueue.Count >= status.length)
				{
					byte[] data = messageQueue.Read(status.length);
					status.phase = Status.Phase.GET_HEADER;
					if (messageQueue.Count > 0)
					{
						GetPacket();
					}
					else if (!isClosed)
					{
						socket.BeginReceive(buffer, 0, buffersize, 0, new AsyncCallback(OnReceived), null);
						Received?.Invoke(this, status.command, status.length, data);
					}
				}
			}
		}
		#region IDisposable Support
		private bool disposedValue = false;
		~Session()
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
				Close();
				messageQueue.Clear();
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