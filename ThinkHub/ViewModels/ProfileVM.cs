using Microsoft.AspNetCore.Http;
using System;

namespace ThinkHub.ViewModels
{
	public class ProfileVM
	{
		public string Name { get; set; }
		public IFormFile Image { get; set; }
		public string Phone { get; set; }
		public DateTime? Birthday { get; set; }
	}
}