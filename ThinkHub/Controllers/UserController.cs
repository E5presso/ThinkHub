using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using cloudscribe.HtmlAgilityPack;
using ThinkHub.Models;
using Core.Security;
using Middleware;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using ThinkHub.ViewModels;

namespace ThinkHub.Controllers
{
	public class UserController : Controller
	{
		private string RequestClient => HttpContext.Connection.RemoteIpAddress.ToString();
		private string RequestType => HttpContext.Request.Method;
		private string RequestPath => HttpContext.Request.Path;

		private int SessionID
		{
			get => (int)HttpContext.Session.GetInt32(Session.SessionID);
			set => HttpContext.Session.SetInt32(Session.SessionID, value);
		}
		private string UserName
		{
			get => HttpContext.Session.GetString(Session.UserName);
			set => HttpContext.Session.SetString(Session.UserName, value);
		}
		private string UserImage
		{
			get => HttpContext.Session.GetString(Session.UserImage);
			set => HttpContext.Session.SetString(Session.UserImage, value);
		}
		private bool IsLoggedIn => HttpContext.Session.GetInt32(Session.SessionID) != null;

		public IActionResult Register()
		{
			if (!IsLoggedIn) return View();
			else return RedirectToAction("Index", "Home");
		}
		public IActionResult MyPage()
		{
			if (IsLoggedIn)
			{
				using (var db = new ThinkhubDbContext())
				{
					if (SessionID != 0)
					{
						var user = db.User.SingleOrDefault(x => x.Id == SessionID);
						user.Password = string.Empty;
						user.HashSalt = string.Empty;
						user.Shared = null;
						user.Permission = null;
						user.Access = null;
						return View(user);
					}
					else return RedirectToAction("DashBoard", "Admin");
				}
			}
			else return RedirectToAction("Index", "Home");
		}
		public IActionResult Profile()
		{
			if (IsLoggedIn)
			{
				if (SessionID == 0) return RedirectToAction("DashBoard", "Admin");
				else
				{
					using (var db = new ThinkhubDbContext())
					{
						var user = db.User.SingleOrDefault(u => u.Id == SessionID);
						byte[] imgData = user.Profile.Image;
						string image = imgData != null ? Base64.GetString(imgData) : string.Empty;
						string imageUrl = $@"data:image/jpg;base64,{image}";
						UserImage = imageUrl;
						return View(new ProfileVM()
						{
							Name = user.Profile.Name,
							Phone = user.Profile.Phone,
							Birthday = user.Profile.Birthday
						});
					}

				}
			}
			else return RedirectToAction("Index", "Home");
		}
		public IActionResult Login()
		{
			if (!IsLoggedIn) return View();
			else return RedirectToAction("Index", "Home");
		}
		public IActionResult ForgotPassword()
		{
			if (!IsLoggedIn) return View();
			else return RedirectToAction("Index", "Home");
		}
		public IActionResult Logout()
		{
			if (IsLoggedIn)
			{
				LogManager.Log(RequestClient, RequestType, UserName, RequestPath, "");
				HttpContext.Session.Clear();
				SystemMonitor.UserDisconnected();
			}
			return RedirectToAction("Index", "Home");
		}

		[HttpGet] public IActionResult VerifyEmail()
		{
			try
			{
				LogManager.Log(RequestClient, RequestType, RequestPath);
				string code = HttpContext.Request.Query["Code"];
				ViewBag.Result = MailManager.Verify(code);
				return View();
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpGet] public IActionResult CheckVerification()
		{
			try
			{
				LogManager.Log(RequestClient, RequestType, RequestPath);
				string code = HttpContext.Request.Query["Code"];
				return Json(new { verified = MailManager.CheckVerification(code) });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> TryVerifyEmail(User user)
		{
			try
			{
				LogManager.Log(RequestClient, RequestType, RequestPath);
				using (var db = new ThinkhubDbContext())
				{
					var email = await db.User.SingleOrDefaultAsync(u => u.Email == user.Email);
					if (email == null)
					{
						var code = Hash.SHA256(user.Email, Key.GenerateString(64));
						var path = AppDomain.CurrentDomain.BaseDirectory + XmlConfiguration.VerifyTemplate;
						var document = new HtmlDocument();
						document.Load(path);

						var sended = MailManager.CreateVerification
						(
							user.UserName, user.Email, XmlConfiguration.VerifyTitle,
							document, XmlConfiguration.VerifyLink, code, XmlConfiguration.VerifyFilter[0],
							XmlConfiguration.VerifyFilter[1], XmlConfiguration.VerifyFilter[2]
						);
						if (sended) return Json(new { result = "SENDED", code });
						else return Json(new { result = "DOUBLE_REQUEST" });
					}
					else return Json(new { result = "EMAIL_CONFLICT" });

				}
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> ResetPassword(string email)
		{
			try
			{
				LogManager.Log(RequestClient, RequestType, RequestPath);
				using (var db = new ThinkhubDbContext())
				{
					var user = await db.User.SingleOrDefaultAsync(x => x.Email == email);
					if (user != null)
					{
						var password = Key.GenerateString(16);
						var salt = Key.GenerateString(32);
						user.Password = Hash.SHA256(password, salt);
						user.HashSalt = salt;
						db.User.Update(user);
						await db.SaveChangesAsync();

						var path = AppDomain.CurrentDomain.BaseDirectory + XmlConfiguration.ResetTemplate;
						var document = new HtmlDocument();
						document.Load(path);

						MailManager.ResetPassword
						(
							user.UserName, password, user.Email, XmlConfiguration.ResetTitle,
							document, XmlConfiguration.ResetFilter[0], XmlConfiguration.ResetFilter[1]
						);
						return Json(new { result = "SUCCESS" });
					}
					else return Json(new { result = "NOT_REGISTERED" });

				}
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> CheckUserName(User user)
		{
			try
			{
				using (var db = new ThinkhubDbContext())
				{
					var sample = await db.User.SingleOrDefaultAsync(u => u.UserName == user.UserName);
					return Json(new { result = sample == null ? user.UserName != XmlConfiguration.AdminUserName ? true : false : false });
				}
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> CreateAccount(User user)
		{
			try
			{
				LogManager.Log(RequestClient, RequestType, RequestPath);
				string salt = Key.GenerateString(32);
				user.Password = Hash.SHA256(user.Password, salt);
				user.HashSalt = salt;
				user.RegistrationDate = DateTime.UtcNow;

				var homePath = $@"{XmlConfiguration.UserDirectory}{user.UserName}{XmlConfiguration.HomeDirectory}";
				var uploadPath = $@"{XmlConfiguration.UserDirectory}{user.UserName}{XmlConfiguration.UploadDirectory}";

				FileManager.Create(homePath);
				FileManager.Create(uploadPath);

				using (var db = new ThinkhubDbContext())
				{
					if (await db.User.SingleOrDefaultAsync(x => x.UserName == user.UserName) == null)
					{
						db.User.Add(user);
						db.Profile.Add(user.Profile);
						await db.SaveChangesAsync();
					}
				}
				return RedirectToAction("Login", "User");
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> CheckAccount(User user)
		{
			try
			{
				LogManager.Log(RequestClient, RequestType, RequestPath);
				if (user.UserName == XmlConfiguration.AdminUserName && user.Password == XmlConfiguration.AdminPassword)
				{
					SessionID = 0;
					UserName = user.UserName;
					UserImage = $@"data:image/jpg;base64,";
					SystemMonitor.UserConnected();
					return Json(new { result = true });
				}
				else
				{
					using (var db = new ThinkhubDbContext())
					{
						var result = await db.User.SingleOrDefaultAsync(
							u => (u.UserName == user.UserName || u.Email == user.UserName)
							&& u.Password == Hash.SHA256(user.Password, u.HashSalt));
						if (result != null)
						{
							byte[] imgData = result.Profile.Image;
							string imageUrl = $@"data:image/jpg;base64,{(imgData != null ? Base64.GetString(imgData) : string.Empty)}";

							SessionID = result.Id;
							UserName = result.UserName;
							UserImage = imageUrl;
							SystemMonitor.UserConnected();
							return Json(new { result = true });
						}
						else return Json(new { result = false });
					}
				}
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> UpdateProfile(ProfileVM user)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, RequestPath);
					byte[] imgData = new byte[] { };
					using (var memory = new MemoryStream())
					{
						if (user.Image != null)
						{
							await user.Image.CopyToAsync(memory);
							int ThumbnailSize = 600;
							using (Image temp = Image.FromStream(memory))
							{
								int width = temp.Width;
								int height = temp.Height;
								if (width > height)
								{
									using (Image thumb = temp.GetThumbnailImage(ThumbnailSize, ThumbnailSize * height / width, null, IntPtr.Zero))
									using (MemoryStream result = new MemoryStream())
									{
										thumb.Save(result, ImageFormat.Png);
										imgData = result.ToArray();
									}
								}
								else
								{
									using (Image thumb = temp.GetThumbnailImage(ThumbnailSize * width / height, ThumbnailSize, null, IntPtr.Zero))
									using (MemoryStream result = new MemoryStream())
									{
										thumb.Save(result, ImageFormat.Png);
										imgData = result.ToArray();
									}
								}
							}
						}
						using (var db = new ThinkhubDbContext())
						{
							var selected = await db.User.SingleOrDefaultAsync(x => x.Id == SessionID);
							var profile = selected.Profile;
							profile.Name = user.Name;
							profile.Birthday = user.Birthday;
							profile.Phone = user.Phone;
							profile.Image = imgData.Length != 0 ? imgData : profile.Image;
							db.Profile.Update(profile);
							db.SaveChanges();
						}
						string image = imgData.Length != 0 ? Base64.GetString(imgData) : string.Empty;
						string imageUrl = $@"data:image/jpg;base64,{image}";
						if (image != string.Empty) UserImage = imageUrl;
						return RedirectToAction("MyPage", "User");
					}
				}
				else return RedirectToAction("Index", "Home");
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> ChangeEmail(string email)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, RequestPath);
					using (var db = new ThinkhubDbContext())
					{
						var user = await db.User.SingleOrDefaultAsync(x => x.Id == SessionID);
						user.Email = email;
						db.User.Update(user);
						await db.SaveChangesAsync();
						return Json(new { result = "SUCCESS" });
					}
				}
				else return Json(new { result = "NOT_LOGGEDIN" });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> ChangePassword(string oldpassword, string newpassword)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, RequestPath);
					using (var db = new ThinkhubDbContext())
					{
						var user = await db.User.SingleOrDefaultAsync(x => x.Id == SessionID);
						if (user.Password == Hash.SHA256(oldpassword, user.HashSalt))
						{
							string salt = Key.GenerateString(32);
							user.Password = Hash.SHA256(newpassword, salt);
							user.HashSalt = salt;
							db.User.Update(user);
							await db.SaveChangesAsync();
							return Json(new { result = "SUCCESS" });
						}
						else return Json(new { result = "WRONG_PASSWORD" });
					}
				}
				else return Json(new { result = "NOT_LOGGEDIN" });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> DeleteAccount(string username, string password)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, RequestPath);
					using (var db = new ThinkhubDbContext())
					{
						var user = await db.User.SingleOrDefaultAsync(u => (u.UserName == username || u.Email == username) && u.Password == Hash.SHA256(password, u.HashSalt));
						if (user != null)
						{
							var basePath = $@"{XmlConfiguration.UserDirectory}{username}";
							FileManager.Delete(basePath, "DIR");

							db.Access.RemoveRange(user.Access);
							db.Permission.RemoveRange(user.Permission);
							db.Shared.RemoveRange(user.Shared);
							db.Profile.Remove(user.Profile);
							db.User.Remove(user);
							await db.SaveChangesAsync();
							HttpContext.Session.Clear();
							return Json(new { result = "SUCCESS" });
						}
						else return Json(new { result = "WRONG_USERDATA" });
					}
				}
				else return Json(new { result = "NOT_LOGGEDIN" });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
	}
}