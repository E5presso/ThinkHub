using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware;
using ThinkHub.Models;
using ThinkHub.ViewModels;

namespace ThinkHub.Controllers
{
	public class AdminController : Controller
	{
		private bool IsLoggedIn => HttpContext.Session.GetInt32(Session.SessionID) != null;
		private bool IsAdmin => HttpContext.Session.GetInt32(Session.SessionID) == 0;

		public IActionResult DashBoard()
		{
			if (IsLoggedIn && IsAdmin) return View();
			else return RedirectToAction("Index", "Home");
		}
		public IActionResult UserManager()
		{
			if (IsLoggedIn && IsAdmin) return View();
			else return RedirectToAction("Index", "Home");
		}

		[HttpPost] public IActionResult Monitor()
		{
			try
			{
				if (IsLoggedIn && IsAdmin)
				{
					long cpu = SystemMonitor.CpuUsage;
					long mem = SystemMonitor.MemUsage;
					long diskRead = SystemMonitor.DiskRead;
					long diskWrite = SystemMonitor.DiskWrite;
					long diskTotal = SystemMonitor.DiskTotal / 1024 / 1204; // MB 단위로 변환
					long diskUsed = SystemMonitor.DiskUsed / 1024 / 1204; // MB 단위로 변환
					long diskFree = SystemMonitor.DiskFree / 1024 / 1204; // MB 단위로 변환
					long netSent = SystemMonitor.NetSent / 1024; // KB 단위로 변환
					long netReceived = SystemMonitor.NetReceived / 1024; // KB 단위로 변환
					long connectedUser = SystemMonitor.ConnectedUser;
					return Json(new
					{
						result = FILE_RESULT.SUCCESS.ToString(),
						cpu, mem,
						diskRead, diskWrite,
						diskTotal, diskUsed, diskFree,
						netSent, netReceived,
						connectedUser
					});
				}
				else return Json(new { result = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public IActionResult GetUserList()
		{
			try
			{
				if (IsLoggedIn && IsAdmin)
				{
					using (var db = new ThinkhubDbContext())
					{
						var users = db.User.Where(x => x.Id > 0).ToArray();
						var vms = new List<UserVM>();
						foreach (var user in users)
						{
							vms.Add(new UserVM
							{
								Id = user.Id,
								UserName = user.UserName,
								Email = user.Email,
								Password = user.Password,
								RegistrationDate = user.RegistrationDate
							});
						}
						return Json(new { result = FILE_RESULT.SUCCESS.ToString(), users = vms.ToArray() });
					}
				}
				else return Json(new { result = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
	}
}