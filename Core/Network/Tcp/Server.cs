using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Network.Tcp
{
	/// <summary>
	/// 서버의 소켓통신 기능을 제공합니다.
	/// </summary>
	public class Server : IDisposable
	{
		private bool started = false;
		private readonly Listener listener;
		private readonly Dictionary<string, Session> list;

		/// <summary>
		/// 서버의 로컬 IP주소입니다.
		/// </summary>
		public string IP => listener.IP;
		/// <summary>
		/// 연결된 클라이언트들의 리스트입니다.
		/// </summary>
		public string[] List => list.Keys.ToArray();

		/// <summary>
		/// 클라이언트 연결완료시 발생하는 이벤트입니다.
		/// </summary>
		public event EventHandler<ConnectEventArgs> Connected;
		/// <summary>
		/// 데이터 전송완료시 발생하는 이벤트입니다.
		/// </summary>
		public event EventHandler<SendEventArgs> Sended;
		/// <summary>
		/// 데이터 수신완료시 발생하는 이벤트입니다.
		/// </summary>
		public event EventHandler<ReceiveEventArgs> Received;
		/// <summary>
		/// 클라이언트 연결종료시 발생하는 이벤트입니다.
		/// </summary>
		public event EventHandler<DisconnectEventArgs> Disconnected;
		/// <summary>
		/// 에러발생시 발생하는 이벤트입니다.
		/// </summary>
		public event EventHandler<ExceptionEventArgs> ErrorOccurred;

		/// <summary>
		/// Server 클래스를 초기화합니다.
		/// </summary>
		public Server()
		{
			list = new Dictionary<string, Session>();
			listener = new Listener();
		}

		/// <summary>
		/// 서버를 시작합니다.
		/// </summary>
		/// <param name="port">바인딩할 포트 번호를 지정합니다.</param>
		/// <param name="backlog">접속 대기열의 길이를 지정합니다.</param>
		/// <param name="bufferSize">송수신에 사용할 버퍼의 크기를 지정합니다.</param>
		public void Open(int port, int backlog, int bufferSize)
		{
			if (!started)
			{
				listener.Connected += OnConnected;
				listener.ErrorOccurred += OnErrorOccurred;
				listener.Open(port, backlog, bufferSize);

				started = true;
			}
			else
			{
				ErrorOccurred?.Invoke(this, new ExceptionEventArgs(new InvalidOperationException("서버가 이미 구동 중입니다.")));
			}
		}
		/// <summary>
		/// 지정한 IP에 데이터를 전송합니다.
		/// </summary>
		/// <param name="ip">데이터를 전달할 클라이언트의 IP주소를 지정합니다.</param>
		/// <param name="command">명령어를 지정합니다.</param>
		/// <param name="data">전송할 데이터를 지정합니다.</param>
		public void Send(string ip, int command, byte[] data)
		{
			list[ip].Send(command, data);
		}
		/// <summary>
		/// 지정한 IP 목록에 데이터를 전송합니다.
		/// </summary>
		/// <param name="ip">데이터를 전달할 클라이언트의 IP 목록을 지정합니다.</param>
		/// <param name="command">명령어를 지정합니다.</param>
		/// <param name="data">전송할 데이터를 지정합니다.</param>
		public void Send(string[] ip, int command, byte[] data)
		{
			foreach (string i in ip)
			{
				list[i].Send(command, data);
			}
		}
		/// <summary>
		/// 연결 중인 세션 전체에 데이터를 전송합니다.
		/// </summary>
		/// <param name="command">명령어를 지정합니다.</param>
		/// <param name="data">전송할 데이터를 지정합니다.</param>
		public void Send(int command, byte[] data)
		{
			foreach (KeyValuePair<string, Session> session in list)
			{
				session.Value.Send(command, data);
			}
		}
		/// <summary>
		/// 지정한 클라이언트와의 연결을 해제합니다.
		/// </summary>
		/// <param name="ip">해제할 클라이언트의 IP주소를 지정합니다.</param>
		public void Disconnect(string ip)
		{
			try
			{
				list[ip].Close();
				list.Remove(ip);
			}
			catch { ErrorOccurred?.Invoke(this, new ExceptionEventArgs(new InvalidOperationException("현재 접속 중인 IP주소가 아닙니다."))); }
		}
		/// <summary>
		/// 서버를 정지합니다.
		/// </summary>
		public void Close()
		{
			if (started)
			{
				listener.Close();
				listener.Connected -= OnConnected;
				listener.ErrorOccurred -= OnErrorOccurred;
				Parallel.ForEach(list.Values.ToList(), (session) =>
				{
					session.Close();
					session.Sended -= OnSended;
					session.Received -= OnReceived;
					session.Disconnected -= OnDisconnected;
					session.ErrorOccurred -= OnErrorOccurred;
				});
				list.Clear();

				started = false;
			}
		}

		private void OnConnected(Session session)
		{
			try
			{
				session.Sended += OnSended;
				session.Received += OnReceived;
				session.Disconnected += OnDisconnected;
				session.ErrorOccurred += OnErrorOccurred;
				lock (list)
				{
					list.Add(session.IP, session);
				}

				Connected?.Invoke(this, new ConnectEventArgs(session.IP));
			}
			catch (Exception e) { ErrorOccurred?.Invoke(this, new ExceptionEventArgs(e)); }
		}
		private void OnSended(Session session, int command, int bytesSent)
		{
			try { Sended?.Invoke(this, new SendEventArgs(session.IP, command, bytesSent)); }
			catch (Exception e) { ErrorOccurred?.Invoke(this, new ExceptionEventArgs(e)); }
		}
		private void OnReceived(Session session, int command, int bytesRead, byte[] data)
		{
			try { Received?.Invoke(this, new ReceiveEventArgs(session.IP, command, bytesRead, data)); }
			catch (Exception e) { ErrorOccurred?.Invoke(this, new ExceptionEventArgs(e)); }
		}
		private void OnDisconnected(Session session)
		{
			try
			{
				session.Sended -= OnSended;
				session.Received -= OnReceived;
				session.Disconnected -= OnDisconnected;
				session.ErrorOccurred -= OnErrorOccurred;
				list[session.IP].Dispose();
				list.Remove(session.IP);
				Disconnected?.Invoke(this, new DisconnectEventArgs(session.IP));
			}
			catch (Exception e) { ErrorOccurred?.Invoke(this, new ExceptionEventArgs(e)); }
		}
		private void OnErrorOccurred(Exception e)
		{
			ErrorOccurred?.Invoke(this, new ExceptionEventArgs(e));
		}
		#region IDisposable Support
		private bool disposedValue = false;
		/// <summary>
		/// Server 클래스를 제거합니다.
		/// </summary>
		~Server()
		{
			Dispose(false);
		}
		/// <summary>
		/// IDisposable 패턴을 구현합니다.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					listener.Dispose();
				}
				Close();
				list.Clear();
				disposedValue = true;
			}
		}
		/// <summary>
		/// 클래스를 제거하고 리소스를 반환합니다.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}