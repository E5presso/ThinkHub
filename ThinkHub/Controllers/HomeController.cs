using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ThinkHub.Controllers
{
	public class HomeController : Controller
	{
		private bool IsLoggedIn => HttpContext.Session.GetInt32(Session.SessionID) != null;
		private int SessionID
		{
			get => (int)HttpContext.Session.GetInt32(Session.SessionID);
			set => HttpContext.Session.SetInt32(Session.SessionID, value);
		}

		public IActionResult Index()
		{
			if (!IsLoggedIn) return View();
			else
			{
				if (SessionID == 0) return RedirectToAction("DashBoard", "Admin");
				else return RedirectToAction("Explorer", "File");
			}
		}
	}
}