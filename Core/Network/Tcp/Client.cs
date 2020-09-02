using System;

namespace Core.Network.Tcp
{
	/// <summary>
	/// 클라이언트 기능을 제공하는 클래스입니다.
	/// </summary>
	public class Client : IDisposable
	{
		private bool started = false;
		private readonly Connector connector;
		private Session session;

		/// <summary>
		/// 접속 중인 서버의 IP주소입니다.
		/// </summary>
		public string RemoteIP { get => session.IP; }
		/// <summary>
		/// 현재 클라이언트의 로컬 IP주소입니다.
		/// </summary>
		public string LocalIP { get => connector.LocalIP; }

		/// <summary>
		/// 서버 연결 시 발생하는 이벤트입니다.
		/// </summary>
		public event EventHandler<ConnectEventArgs> Connected;
		/// <summary>
		/// 데이터 전송 완료 시 발생하는 이벤트입니다.
		/// </summary>
		public event EventHandler<SendEventArgs> Sended;
		/// <summary>
		/// 데이터 수신 완료 시 발생하는 이벤트입니다.
		/// </summary>
		public event EventHandler<ReceiveEventArgs> Received;
		/// <summary>
		/// 클라이언트 연결 종료 시 발생하는 이벤트입니다.
		/// </summary>
		public event EventHandler<DisconnectEventArgs> Disconnected;
		/// <summary>
		/// 에러 시 발생하는 이벤트입니다.
		/// </summary>
		public event EventHandler<ExceptionEventArgs> ErrorOccurred;

		/// <summary>
		/// Client 클래스를 초기화합니다.
		/// </summary>
		public Client()
		{
			connector = new Connector();
		}
		/// <summary>
		/// 클라이언트를 시작합니다.
		/// </summary>
		/// <param name="ip">연결할 서버의 IP주소입니다.</param>
		/// <param name="port">연결에 사용할 포트번호입니다.</param>
		/// <param name="bufferSize">송수신에 사용할 버퍼의 크기입니다.</param>
		public void Open(string ip, int port, int bufferSize)
		{
			if (!started)
			{
				connector.Connected += OnConnected;
				connector.ErrorOccurred += OnErrorOccurred;
				connector.Start(ip, port, bufferSize);

				started = true;
			}
		}
		/// <summary>
		/// 데이터를 전송합니다.
		/// </summary>
		/// <param name="command">전송할 헤더입니다.</param>
		/// <param name="data">전송할 데이터입니다.</param>
		public void Send(int command, byte[] data)
		{
			session?.Send(command, data);
		}
		/// <summary>
		/// 클라이언트를 정지합니다.
		/// </summary>
		public void Close()
		{
			if (started)
			{
				connector.Connected -= OnConnected;
				connector.ErrorOccurred -= OnErrorOccurred;

				session.Close();
				session.Sended -= OnSended;
				session.Received -= OnReceived;
				session.Disconnected -= OnDisconnected;
				session.ErrorOccurred -= OnErrorOccurred;

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
				this.session = session;

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
				session.Dispose();

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
		/// Client 클래스를 제거합니다.
		/// </summary>
		~Client()
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
					connector.Dispose();
					session.Dispose();
				}
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