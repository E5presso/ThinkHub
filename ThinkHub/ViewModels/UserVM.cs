using System;

namespace ThinkHub.ViewModels
{
	public class UserVM
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public DateTime RegistrationDate { get; set; }
	}
}