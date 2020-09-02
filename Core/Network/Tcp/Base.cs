using System;
using System.ComponentModel;

namespace Core.Network.Tcp
{
	/// <summary>
	/// 소켓 연결시 발생하는 이벤트에 대한 정보입니다.
	/// </summary>
	public class ConnectEventArgs : EventArgs
	{
		/// <summary>
		/// 클라이언트의 IP주소를 가져옵니다.
		/// </summary>
		public string IP { get; private set; }

		/// <summary>
		/// AcceptEventArgs 클래스를 초기화합니다.
		/// </summary>
		/// <param name="ip">클라이언트의 IP주소를 지정합니다.</param>
		public ConnectEventArgs(string ip) => IP = ip;
	}
	/// <summary>
	/// 데이터 전송완료시 발생하는 이벤트에 대한 정보입니다.
	/// </summary>
	public class SendEventArgs : EventArgs
	{
		/// <summary>
		/// 클라이언트의 IP주소를 가져옵니다.
		/// </summary>
		public string IP { get; private set; }
		/// <summary>
		/// 전송한 명령어를 가져옵니다.
		/// </summary>
		public int Command { get; private set; }
		/// <summary>
		/// 전송된 데이터의 크기를 가져옵니다.
		/// </summary>
		public int BytesSent { get; private set; }

		/// <summary>
		/// SendEventArgs 클래스를 초기화합니다.
		/// </summary>
		/// <param name="ip">클라이언트의 IP주소를 지정합니다.</param>
		/// <param name="command">전송된 명령어를 지정합니다.</param>
		/// <param name="bytessent">전송된 데이터의 크기를 지정합니다.</param>
		public SendEventArgs(string ip, int command, int bytessent)
		{
			IP = ip;
			Command = command;
			BytesSent = bytessent;
		}
	}
	/// <summary>
	/// 데이터 수신완료시 발생하는 이벤트에 대한 정보입니다.
	/// </summary>
	public class ReceiveEventArgs : EventArgs
	{
		/// <summary>
		/// 클라이언트의 IP주소를 가져옵니다.
		/// </summary>
		public string IP { get; private set; }
		/// <summary>
		/// 수신된 명령어를 가져옵니다.
		/// </summary>
		public int Command { get; private set; }
		/// <summary>
		/// 수신된 데이터의 크기를 가져옵니다.
		/// </summary>
		public int BytesRead { get; private set; }
		/// <summary>
		/// 수신된 데이터를 가져옵니다.
		/// </summary>
		public byte[] Data { get; private set; }

		/// <summary>
		/// ReceiveEventArgs 클래스를 초기화합니다.
		/// </summary>
		/// <param name="ip">클라이언트의 IP주소를 지정합니다.</param>
		/// <param name="command">수신된 명령어를 지정합니다.</param>
		/// <param name="bytesread">수신된 데이터의 크기를 지정합니다.</param>
		/// <param name="data">수신된 데이터를 지정합니다.</param>
		public ReceiveEventArgs(string ip, int command, int bytesread, byte[] data)
		{
			IP = ip;
			Command = command;
			BytesRead = bytesread;
			Data = data;
		}
	}
	/// <summary>
	/// 소켓의 연결해제시 발생하는 이벤트에 대한 정보입니다.
	/// </summary>
	public class DisconnectEventArgs : EventArgs
	{
		/// <summary>
		/// 클라이언트의 IP주소를 가져옵니다.
		/// </summary>
		public string IP { get; private set; }

		/// <summary>
		/// DisconnectEventArgs 클래스를 초기화합니다.
		/// </summary>
		/// <param name="ip">클라이언트의 IP주소를 지정합니다.</param>
		public DisconnectEventArgs(string ip) => IP = ip;
	}
	/// <summary>
	/// 예외발생 이벤트에 대한 정보를 정의합니다.
	/// </summary>
	public class ExceptionEventArgs : EventArgs
	{
		/// <summary>
		/// 예외발생에 대한 정보를 담고 있는 개체를 가져옵니다.
		/// </summary>
		public Exception Exception { get; private set; }
		/// <summary>
		/// 에러 메세지를 가져옵니다.
		/// </summary>
		public string Message => Exception.Message;
		/// <summary>
		/// 에러 코드를 가져옵니다. 시스템 에러가 아니라면 -1을 반환합니다.
		/// </summary>
		public int ErrorCode => (Exception is Win32Exception e) ? e.ErrorCode : -1;

		/// <summary>
		/// 지정한 예외발생 정보를 이용해 ExceptionEventArgs 클래스를 초기화합니다.
		/// </summary>
		/// <param name="e">예외발생에 대한 정보를 지정합니다.</param>
		public ExceptionEventArgs(Exception e) => Exception = e;
		/// <summary>
		/// 예외발생 정보를 문자열 형식으로 반환합니다.
		/// </summary>
		/// <returns>문자열로 변환된 예외발생 정보입니다.</returns>
		public override string ToString() => Exception.ToString();
	}

	internal delegate void ConnectEvent(Session session);
	internal delegate void SendEvent(Session session, int command, int bytesSent);
	internal delegate void ReceiveEvent(Session session, int command, int bytesRead, byte[] data);
	internal delegate void DisconnectEvent(Session session);
	internal delegate void ExceptionEvent(Exception e);
}