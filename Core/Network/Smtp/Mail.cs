using System;
using System.Net;
using System.Net.Mail;

namespace Core.Network.Smtp
{
	/// <summary>
	/// 전송할 메세지를 정의합니다.
	/// </summary>
	public struct Message
	{
		/// <summary>
		/// 메세지를 수신할 주소를 가져오거나 설정합니다.
		/// </summary>
		public string Target;
		/// <summary>
		/// 메세지의 제목을 가져오거나 설정합니다.
		/// </summary>
		public string Title;
		/// <summary>
		/// 메세지의 내용을 가져오거나 설정합니다.
		/// </summary>
		public string Body;
	}
	/// <summary>
	/// 메일 전송기능을 구현한 클래스입니다.
	/// </summary>
	public class Mail : IDisposable
	{
		private readonly MailAddress account;
		private readonly SmtpClient session;

		/// <summary>
		/// Mail 클래스를 초기화합니다.
		/// </summary>
		/// <param name="useSsl">SSL 사용여부를 지정합니다.</param>
		/// <param name="server">SMTP 메일 서버의 주소를 지정합니다.</param>
		/// <param name="account">메일 서버의 계정을 지정합니다.</param>
		/// <param name="password">계정의 비밀번호를 지정합니다.</param>
		public Mail(bool useSsl, string server, string account, string password)
		{
			try
			{
				this.account = new MailAddress(account);
				session = new SmtpClient()
				{
					Host = server,
					EnableSsl = useSsl,
					DeliveryMethod = SmtpDeliveryMethod.Network,
					Credentials = new NetworkCredential(this.account.Address, password),
					Timeout = 20000
				};
			}
			catch { throw; }
		}

		/// <summary>
		/// 메일을 전송합니다.
		/// </summary>
		/// <param name="message">전송할 메일을 지정합니다.</param>
		/// <returns>메일의 전송 결과입니다.</returns>
		public void Send(Message message)
		{
			session.Send(new MailMessage(account.Address, message.Target)
			{
				Subject = message.Title,
				Body = message.Body,
				IsBodyHtml = true
			});
		}
		#region IDisposable Support
		private bool disposedValue = false;
		/// <summary>
		/// Mail 클래스를 제거합니다.
		/// </summary>
		~Mail()
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