using System;
using System.Collections.Generic;
using cloudscribe.HtmlAgilityPack;
using Core.Network.Smtp;

namespace Middleware
{
	public class MailManager : IDisposable
	{
		internal class Info
		{
			public string Email;
			public bool Verified;
		}
		private static readonly Dictionary<string, Info> list = new Dictionary<string, Info>();
		private static readonly Mail mail = new Mail(true, "mail.thinkhub.ml", "no-reply@thinkhub.ml", "eSP$#418404");

		public static void ResetPassword(string targetUserName, string targetPassword, string targetEmail, string title, HtmlDocument document, params string[] args)
		{
			document.DocumentNode.SelectSingleNode(args[0]).InnerHtml = targetUserName;
			document.DocumentNode.SelectSingleNode(args[1]).InnerHtml = targetPassword;
			mail.Send(new Message()
			{
				Target = targetEmail,
				Title = title,
				Body = document.DocumentNode.OuterHtml
			});
		}
		public static bool CreateVerification(string targetUserName, string targetEmail, string title, HtmlDocument document, params string[] args)
		{
			string code = args[1];
			string link = args[0] + args[1];

			document.DocumentNode.SelectSingleNode(args[2]).InnerHtml = targetUserName;
			document.DocumentNode.SelectSingleNode(args[3]).SetAttributeValue("href", link);
			document.DocumentNode.SelectSingleNode(args[4]).SetAttributeValue("href", link);
			document.DocumentNode.SelectSingleNode(args[4]).InnerHtml = link;
			mail.Send(new Message()
			{
				Target = targetEmail,
				Title = title,
				Body = document.DocumentNode.OuterHtml
			});
			return list.TryAdd(code, new Info()
			{
				Email = targetEmail,
				Verified = false
			});
		}
		public static bool CheckVerification(string code)
		{
			if (list.ContainsKey(code) && list[code].Verified)
			{
				list.Remove(code);
				return true;
			}
			else if (list.ContainsKey(code) && !list[code].Verified)
			{
				list.Remove(code);
				return false;
			}
			else return false;
		}
		public static bool Verify(string code)
		{
			if (list.ContainsKey(code))
			{
				list[code].Verified = true;
				return true;
			}
			else return false;
		}
		#region IDisposable Support
		private bool disposedValue = false;
		/// <summary>
		/// EmailVerifier 클래스를 제거합니다.
		/// </summary>
		~MailManager()
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
					mail.Dispose();
				}
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