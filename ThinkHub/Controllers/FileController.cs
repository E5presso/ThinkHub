using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Middleware;
using System.Threading.Tasks;
using System.IO;
using System;
using ThinkHub.Models;
using Core.Security;
using Core.Utility;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text;

namespace ThinkHub.Controllers
{
	public class FileController : Controller
	{
		private string RequestClient => HttpContext.Connection.RemoteIpAddress.ToString();
		private string RequestType => HttpContext.Request.Method;
		private string RequestPath => HttpContext.Request.Path;

		private int SessionID => (int)HttpContext.Session.GetInt32(Session.SessionID);
		private bool IsLoggedIn => HttpContext.Session.GetInt32(Session.SessionID) != null;
		private string UserName => HttpContext.Session.GetString(Session.UserName);
		private string CurrentPath
		{
			get => HttpContext.Session.GetString(Session.CurrentPath);
			set => HttpContext.Session.SetString(Session.CurrentPath, value);
		}

		private string ClipboardType
		{
			get => HttpContext.Session.GetString(Session.Clipboard.Type);
			set => HttpContext.Session.SetString(Session.Clipboard.Type, value);
		}
		private string ClipboardFilePath
		{
			get => HttpContext.Session.GetString(Session.Clipboard.FilePath);
			set => HttpContext.Session.SetString(Session.Clipboard.FilePath, value);
		}
		private string ClipboardFileType
		{
			get => HttpContext.Session.GetString(Session.Clipboard.FileType);
			set => HttpContext.Session.SetString(Session.Clipboard.FileType, value);
		}
		private string ClipboardSharedCode
		{
			get => HttpContext.Session.GetString("ClipboardSharedCode");
			set => HttpContext.Session.SetString("ClipboardSharedCode", value);
		}
		private string ClipboardSharedSub
		{
			get => HttpContext.Session.GetString("ClipboardSharedSub");
			set { if (value != null) HttpContext.Session.SetString("ClipboardSharedSub", value); }
		}
		private bool ClipboardIsShared
		{
			get => HttpContext.Session.GetInt32("ClipboardIsShared") == 1 ? true : false;
			set { HttpContext.Session.SetInt32("ClipboardIsShared", value ? 1 : 0); }
		}

		private bool IsSharedPath(string path)
		{
			var splited = path.Split(":");
			return splited[0] == XmlConfiguration.SharedMask ? true : false;
		}
		private string GetNameOfShared(string code)
		{
			using (var db = new ThinkhubDbContext())
			{
				var shared = db.Shared.SingleOrDefault(x => x.Code == code);
				return Path.GetFileName(shared.Path);
			}
		}
		private string GetSharedPath(string code, string sub)
		{
			using (var db = new ThinkhubDbContext())
			{
				var shared = db.Shared.SingleOrDefault(x => x.Code == code);
				if (sub == null) return shared.Path;
				else return $@"{shared.Path}\{sub}";
			}
		}
		private string EncodePath(string basePath, string code, string sub)
		{
			if (sub == null) return $@"{XmlConfiguration.SharedMask}:{basePath}\Code:{code}";
			else return $@"{XmlConfiguration.SharedMask}:{basePath}\Code:{code}\{sub}";
		}
		private (string, string, string) DecodePath(string path)
		{
			var splited = path.Split(":");
			List<string> temp;

			temp = splited[1].Split(@"\").ToList();
			temp.Remove(temp.Last());
			var basePath = string.Join(@"\", temp);

			temp = splited[2].Split(@"\").ToList();
			var code = temp[0];

			string sub = null;
			temp.Remove(temp.First());
			if (temp.Count > 0) sub = string.Join(@"\", temp);

			return (basePath, code, sub);
		}
		private (bool, bool, bool, bool) GetSharedPermission(string code)
		{
			using (var db = new ThinkhubDbContext())
			{
				var shared = db.Shared.SingleOrDefault(x => x.Code == code);
				if (shared == null) return (false, false, false, false);
				else
				{
					if (shared.Type == 1) return (false, true, false, false);
					else
					{
						var permission = shared.Permission.SingleOrDefault(x => x.UserId == SessionID);
						if (permission == null) return (false, false, false, false);
						else
						{
							var create = permission.Create == 1 ? true : false;
							var read = permission.Read == 1 ? true : false;
							var write = permission.Write == 1 ? true : false;
							var remove = permission.Delete == 1 ? true : false;
							return (create, read, write, remove);
						}
					}
				}
			}
		}

		private string MaskPath(string realPath)
		{
			var username = realPath.Replace(XmlConfiguration.UserDirectory, "").Split(@"\")[0];
			var maskingPath = $"{XmlConfiguration.UserDirectory}{username}{XmlConfiguration.HomeDirectory}";
			var maskedPath = realPath.Replace(maskingPath, XmlConfiguration.HomeMask);
			return maskedPath;
		}
		private string UnmaskPath(string maskedPath)
		{
			var username = UserName;
			var maskingPath = $"{XmlConfiguration.UserDirectory}{username}{XmlConfiguration.HomeDirectory}";
			var realPath = maskedPath.Replace(XmlConfiguration.HomeMask, maskingPath);
			return realPath;
		}

		[HttpGet] public IActionResult Explorer()
		{
			if (IsLoggedIn)
			{
				if (SessionID == 0) return RedirectToAction("DashBoard", "Admin");
				else return View();
			}
			else return RedirectToAction("Index", "Home");
		}

		[HttpGet] public IActionResult Download(string path)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						if (read)
						{
							var realPath = GetSharedPath(code, sub);
							var success = FileManager.Exists(realPath, "FILE");
							if (success == FILE_RESULT.SUCCESS)
								return PhysicalFile(realPath, FileManager.GetMimeType(Path.GetExtension(realPath)), Path.GetFileName(realPath), false);
							else return Json(new { success = FILE_RESULT.NO_SUCH_DIRECTORY.ToString() });
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						var realPath = UnmaskPath(path);
						var success = FileManager.Exists(realPath, "FILE");
						if (success == FILE_RESULT.SUCCESS)
							return PhysicalFile(realPath, FileManager.GetMimeType(Path.GetExtension(realPath)), Path.GetFileName(realPath), false);
						else return Json(new { success = FILE_RESULT.NO_SUCH_DIRECTORY.ToString() });
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpGet] public IActionResult Stream(string path)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						if (read)
						{
							var realPath = GetSharedPath(code, sub);
							var success = FileManager.Exists(realPath, "FILE");
							if (success == FILE_RESULT.SUCCESS)
								return PhysicalFile(realPath, FileManager.GetMimeType(Path.GetExtension(realPath)), Path.GetFileName(realPath), true);
							else return Json(new { success = FILE_RESULT.NO_SUCH_DIRECTORY.ToString() });
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						var realPath = UnmaskPath(path);
						var success = FileManager.Exists(realPath, "FILE");
						if (success == FILE_RESULT.SUCCESS)
							return PhysicalFile(realPath, FileManager.GetMimeType(Path.GetExtension(realPath)), Path.GetFileName(realPath), true);
						else return Json(new { success = FILE_RESULT.NO_SUCH_DIRECTORY.ToString() });
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}

		[HttpGet] public async Task<IActionResult> Link(string code)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"code={code}");
					using (var db = new ThinkhubDbContext())
					{
						var folder = await db.Shared.SingleOrDefaultAsync(x => x.Code == code);
						if (folder.UserId != SessionID)
						{
							if (folder != null)
							{
								if (folder.Type == 1)
								{
									if (folder.Access.SingleOrDefault(x => x.UserId == SessionID) == null)
									{
										db.Access.Add(new Access()
										{
											UserId = SessionID,
											SharedId = folder.Id,
											Path = XmlConfiguration.HomeMask
										});
										await db.SaveChangesAsync();
										ViewData["success"] = FILE_RESULT.SUCCESS.ToString();
									}
									else ViewData["success"] = FILE_RESULT.ALREADY_EXISTS.ToString();
								}
								else
								{
									if (folder.Permission.SingleOrDefault(x => x.UserId == SessionID) == null)
									{
										ViewData["success"] = "ACCESS_DENIED";
									}
									else
									{
										if (folder.Access.SingleOrDefault(x => x.UserId == SessionID) == null)
										{
											db.Access.Add(new Access()
											{
												UserId = SessionID,
												SharedId = folder.Id,
												Path = XmlConfiguration.HomeMask
											});
											await db.SaveChangesAsync();
											ViewData["success"] = FILE_RESULT.SUCCESS.ToString();
										}
										else ViewData["success"] = FILE_RESULT.ALREADY_EXISTS.ToString();
									}
								}
							}
							else ViewData["success"] = FILE_RESULT.NO_SUCH_DIRECTORY.ToString();
						}
						else ViewData["success"] = "FOLDER_OWNER";
					}
				}
				else
				{
					HttpContext.Session.SetString("LINK_URL", HttpContext.Request.GetEncodedUrl());
					ViewData["success"] = FILE_RESULT.NOT_LOGGEDIN.ToString();
				}
				return View();
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> Unlink(string path)
		{
			try
			{
				if (IsLoggedIn)
				{
					if (path == null && CurrentPath != null) path = CurrentPath;
					else if (path == null && CurrentPath == null) path = XmlConfiguration.HomeMask;

					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						using (var db = new ThinkhubDbContext())
						{
							var shared = await db.Shared.SingleOrDefaultAsync(x => x.Code == code);
							var access = shared.Access.SingleOrDefault(x => x.UserId == SessionID);
							if (access == null) return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
							else
							{
								db.Access.Remove(access);
								await db.SaveChangesAsync();
								return Json(new { success = FILE_RESULT.SUCCESS.ToString() });
							}
						}
					}
					else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> Share(string path, bool isPublic)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					var realPath = UnmaskPath(path);
					using (var db = new ThinkhubDbContext())
					{
						if (db.Shared.Where(x => x.UserId == SessionID && x.Path.StartsWith(realPath)).Count() > 0)
						{
							return Json(new { success = FILE_RESULT.SHARED_EXISTS.ToString() });
						}
						else if (await db.Shared.SingleOrDefaultAsync(x => x.UserId == SessionID && x.Path == realPath) == null)
						{
							var code = Hash.SHA256(realPath);
							var shared = await db.Shared.SingleOrDefaultAsync(x => x.Code == code);
							while (shared != null)
							{
								code = Hash.SHA256(code + Key.GenerateString(8));
								shared = await db.Shared.SingleOrDefaultAsync(x => x.Code == code);
							}
							db.Shared.Add(new Shared()
							{
								UserId = SessionID,
								Type = isPublic ? 1 : 0,
								Path = realPath,
								Code = code
							});
							await db.SaveChangesAsync();
							return base.Json(new { success = FILE_RESULT.SUCCESS.ToString(), code });
						}
						else return Json(new { success = FILE_RESULT.ALREADY_EXISTS.ToString() });

					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> Unshare(string path)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					var realPath = UnmaskPath(path);
					using (var db = new ThinkhubDbContext())
					{
						var shared = db.Shared.SingleOrDefault(x => x.UserId == SessionID && x.Path == realPath);
						if (shared != null)
						{
							db.Permission.RemoveRange(shared.Permission);
							db.Access.RemoveRange(shared.Access);
							db.Shared.Remove(shared);
							await db.SaveChangesAsync();
							return Json(new { success = FILE_RESULT.SUCCESS.ToString() });
						}
						else return Json(new { success = FILE_RESULT.NO_SUCH_DIRECTORY.ToString() });
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> GetCode(string path)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					var realPath = UnmaskPath(path);
					using (var db = new ThinkhubDbContext())
					{
						var folder = await db.Shared.SingleOrDefaultAsync(x => x.UserId == SessionID && x.Path == realPath);
						if (folder != null)
						{
							var url = $@"{XmlConfiguration.Domain}File/Link?code={folder.Code}";
							var qr = QRGenerator.GetQRCode(url);
							return Json(new { success = FILE_RESULT.SUCCESS.ToString(), url, qr });
						}
						else return Json(new { success = FILE_RESULT.NO_SUCH_DIRECTORY.ToString() });
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> GetPermission(string path)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath);
					var realPath = UnmaskPath(path);
					using (var db = new ThinkhubDbContext())
					{
						var shared = await db.Shared.SingleOrDefaultAsync(x => x.UserId == SessionID && x.Path == realPath);
						if (shared != null)
						{
							var permission = shared.Permission;
							var list = permission.Select(x =>
							{
								var username = x.User.UserName;
								var Create = x.Create;
								var Read = x.Read;
								var Write = x.Write;
								var Delete = x.Delete;

								return new
								{
									username,
									create = Create == 1 ? true : false,
									read = Read == 1 ? true : false,
									write = Write == 1 ? true : false,
									remove = Delete == 1 ? true : false
								};
							}).ToList();
							return base.Json(new { success = FILE_RESULT.SUCCESS.ToString(), list });
						}
						else return base.Json(new { success = FILE_RESULT.NO_SUCH_DIRECTORY.ToString() });
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> SetPermission(string path, string json)
		{
			try
			{
				if (IsLoggedIn)
				{
					dynamic list = JsonSerializer.Deserialize(json);
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath);
					var realPath = UnmaskPath(path);
					using (var db = new ThinkhubDbContext())
					{
						var shared = await db.Shared.SingleOrDefaultAsync(x => x.UserId == SessionID && x.Path == realPath);
						if (shared != null)
						{
							db.Permission.RemoveRange(shared.Permission);
							foreach (var item in list)
							{
								string username = item.username;
								db.Permission.Add(new Permission()
								{
									UserId = db.User.SingleOrDefault(x => x.UserName == username).Id,
									SharedId = shared.Id,
									Create = (bool)item.create ? 1 : 0,
									Read = (bool)item.read ? 1 : 0,
									Write = (bool)item.write ? 1 : 0,
									Delete = (bool)item.remove ? 1 : 0
								});
							}
							await db.SaveChangesAsync();
							return base.Json(new { success = FILE_RESULT.SUCCESS.ToString() });
						}
						else return base.Json(new { success = FILE_RESULT.NO_SUCH_DIRECTORY.ToString() });
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}

		[HttpPost] public async Task<IActionResult> FindUser(string username)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath);
					using (var db = new ThinkhubDbContext())
					{
						if (await db.User.SingleOrDefaultAsync(x => x.UserName == username) != null)
						{
							if (username == UserName) return Json(new { success = false });
							else return Json(new { success = true });
						}
						else return Json(new { success = false });
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}

		[HttpPost] public async Task<IActionResult> Lock(string path, string password)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						if (create && read && write && remove)
						{
							var realPath = GetSharedPath(code, sub);
							var success = await FileManager.Lock(realPath, XmlConfiguration.TempDirectory, password);
							return Json(new { success = success.ToString() });
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						var realPath = UnmaskPath(path);
						var success = await FileManager.Lock(realPath, XmlConfiguration.TempDirectory, password);
						return Json(new { success = success.ToString() });
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> Unlock(string path, string password)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						if (create && read && write && remove)
						{
							var realPath = GetSharedPath(code, sub);
							var success = await FileManager.Unlock(realPath, XmlConfiguration.TempDirectory, password);
							return Json(new { success = success.ToString() });
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						var realPath = UnmaskPath(path);
						var success = await FileManager.Unlock(realPath, XmlConfiguration.TempDirectory, password);
						return Json(new { success = success.ToString() });
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}

		[HttpPost] public IActionResult List(string path)
		{
			try
			{
				if (IsLoggedIn)
				{
					if (path == null && CurrentPath != null) path = CurrentPath;
					else if (path == null && CurrentPath == null) path = XmlConfiguration.HomeMask;

					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						if (read)
						{
							var realPath = GetSharedPath(code, sub);
							var success = FileManager.List(realPath, out List<FILE_INFO> list);
							if (success == FILE_RESULT.SUCCESS) CurrentPath = path;
							string tempPath = string.Empty;
							if (sub == null) tempPath = ($@"{basePath}\{GetNameOfShared(code)}");
							else tempPath = ($@"{basePath}\{GetNameOfShared(code)}\{sub}");
							string[] pathList = tempPath.Split(@"\");

							List<string> pathLink = new List<string>();
							string stackPath = string.Empty;
							var basePathList = basePath.Split(@"\");
							for (int i = 0; i < basePathList.Length; i++)
							{
								if (i == 0) stackPath += basePathList[i];
								else stackPath += $@"\{basePathList[i]}";
								pathLink.Add(stackPath);
							}
							stackPath += $@"\Code:{code}";
							pathLink.Add($@"{XmlConfiguration.SharedMask}:{stackPath}");
							if (sub != null)
							{
								var subPathList = sub.Split(@"\");
								for (int i = 0; i < subPathList.Length; i++)
								{
									stackPath += $@"\{subPathList[i]}";
									pathLink.Add($@"{XmlConfiguration.SharedMask}:{stackPath}");
								}
							}

							using (var db = new ThinkhubDbContext())
							{
								list = list.Select(x =>
								{
									if (x.Type == "DIR") x.Type = "LINKED_SUB";
									x.Path = (EncodePath(basePath, code, $@"{sub}\{x.Name}")).Replace(@"\\", @"\");
									return x;
								}).ToList();
								return Json(new { success = success.ToString(), list = list.ToArray(), current = CurrentPath, pathList, pathLink = pathLink.ToArray() });
							}
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						var realPath = UnmaskPath(path);
						var success = FileManager.List(realPath, out List<FILE_INFO> list);
						if (success == FILE_RESULT.SUCCESS) CurrentPath = path;
						var pathList = path.Split(@"\").ToList();
						var pathLink = new List<string>();
						var stackPath = string.Empty;
						for (int i = 0; i < pathList.Count; i++)
						{
							if (i == 0) stackPath += pathList[i];
							else stackPath += $@"\{pathList[i]}";
							pathLink.Add(stackPath);
						}
						using (var db = new ThinkhubDbContext())
						{
							var folders = db.Shared.Where(x => x.UserId == SessionID);
							var linked = db.Access.Where(x => x.UserId == SessionID && x.Path == path);
							foreach (var i in linked)
							{
								var sharePath = EncodePath(i.Path, i.Shared.Code, null);
								var shareName = Path.GetFileName(i.Shared.Path);
								list.Add(new FILE_INFO()
								{
									Type = "LINKED",
									Path = sharePath,
									Name = shareName
								});
							}
							list = list.Select(x =>
							{
								var folder = folders.SingleOrDefault(y => y.Path == x.Path);
								if (folder != null) x.Type = folder.Type == 1 ? "PUBLIC_SHARED" : "PRIVATE_SHARED";
								if (x.Type != "LINKED") x.Path = MaskPath(x.Path);
								return x;
							}).ToList();
							return Json(new { success = success.ToString(), list = list.ToArray(), current = CurrentPath, pathList = pathList.ToArray(), pathLink = pathLink.ToArray() });
						}
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public IActionResult Search(string path, string keyword)
		{
			try
			{
				if (IsLoggedIn)
				{
					if (keyword == null) keyword = string.Empty;
					if (path == XmlConfiguration.SharedMask) path = XmlConfiguration.HomeMask;

					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}&keyword={keyword}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						if (read)
						{
							var realPath = GetSharedPath(code, sub);
							var success = FileManager.List(realPath, out List<FILE_INFO> list);
							if (success == FILE_RESULT.SUCCESS) CurrentPath = path;
							string tempPath = string.Empty;
							if (sub == null) tempPath = ($@"{basePath}\{GetNameOfShared(code)}");
							else tempPath = ($@"{basePath}\{GetNameOfShared(code)}\{sub}");
							string[] pathList = tempPath.Split(@"\");

							List<string> pathLink = new List<string>();
							string stackPath = string.Empty;
							var basePathList = basePath.Split(@"\");
							for (int i = 0; i < basePathList.Length; i++)
							{
								if (i == 0) stackPath += basePathList[i];
								else stackPath += $@"\{basePathList[i]}";
								pathLink.Add(stackPath);
							}
							stackPath += $@"\Code:{code}";
							pathLink.Add($@"{XmlConfiguration.SharedMask}:{stackPath}");
							if (sub != null)
							{
								var subPathList = sub.Split(@"\");
								for (int i = 0; i < subPathList.Length; i++)
								{
									stackPath += $@"\{subPathList[i]}";
									pathLink.Add($@"{XmlConfiguration.SharedMask}:{stackPath}");
								}
							}

							using (var db = new ThinkhubDbContext())
							{
								list = list.Where(x => x.Name.Contains(keyword)).Select(x =>
								{
									if (x.Type == "DIR") x.Type = "LINKED_SUB";
									x.Path = EncodePath(basePath, code, x.Name);
									return x;
								}).ToList();
								return Json(new { success = success.ToString(), list = list.ToArray(), current = CurrentPath, pathList, pathLink = pathLink.ToArray() });

							}
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						var realPath = UnmaskPath(path);
						var success = FileManager.List(realPath, out List<FILE_INFO> list);
						if (success == FILE_RESULT.SUCCESS) CurrentPath = path;
						var pathList = path.Split(@"\").ToList();
						var pathLink = new List<string>();
						var stackPath = string.Empty;
						for (int i = 0; i < pathList.Count; i++)
						{
							if (i == 0) stackPath += pathList[i];
							else stackPath += $@"\{pathList[i]}";
							pathLink.Add(stackPath);
						}
						using (var db = new ThinkhubDbContext())
						{
							var folders = db.Shared.Where(x => x.UserId == SessionID);
							var linked = db.Access.Where(x => x.UserId == SessionID && x.Path == path);
							foreach (var i in linked)
							{
								var sharePath = EncodePath(i.Path, i.Shared.Code, null);
								var shareName = Path.GetFileName(i.Shared.Path);
								list.Add(new FILE_INFO()
								{
									Type = "LINKED",
									Path = sharePath,
									Name = shareName
								});
							}
							list = list.Where(x => x.Name.Contains(keyword)).Select(x =>
							{
								var folder = folders.SingleOrDefault(y => y.Path == x.Path);
								if (folder != null) x.Type = folder.Type == 1 ? "PUBLIC_SHARED" : "PRIVATE_SHARED";
								if (x.Type != "LINKED") x.Path = MaskPath(x.Path);
								return x;
							}).ToList();
							return Json(new { success = success.ToString(), list = list.ToArray(), current = CurrentPath, pathList = pathList.ToArray(), pathLink = pathLink.ToArray() });
						}
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public IActionResult Cut(string path, string type)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						if (read && remove && type != "LINKED")
						{
							ClipboardType = "CUT";
							ClipboardIsShared = true;
							ClipboardFilePath = path;
							ClipboardFileType = type;
							ClipboardSharedCode = code;
							ClipboardSharedSub = sub;
							return Json(new { success = FILE_RESULT.SUCCESS.ToString() });
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						ClipboardType = "CUT";
						ClipboardIsShared = false;
						ClipboardFilePath = path;
						ClipboardFileType = type;
						return Json(new { success = FILE_RESULT.SUCCESS.ToString() });
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public IActionResult Copy(string path, string type)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						if (read)
						{
							ClipboardType = "CPY";
							ClipboardIsShared = true;
							ClipboardFilePath = path;
							ClipboardFileType = type;
							ClipboardSharedCode = code;
							ClipboardSharedSub = sub;
							return Json(new { success = FILE_RESULT.SUCCESS.ToString() });
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						ClipboardType = "CPY";
						ClipboardIsShared = false;
						ClipboardFilePath = path;
						ClipboardFileType = type;
						return Json(new { success = FILE_RESULT.SUCCESS.ToString() });
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public IActionResult Create(string path)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						if (create)
						{
							var realPath = GetSharedPath(code, sub);
							var success = FileManager.Create(realPath);
							return Json(new { success = success.ToString() });
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						var realPath = UnmaskPath(path);
						var success = FileManager.Create(realPath);
						return Json(new { success = success.ToString() });
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> Move(string srcPath, string dstPath, string type)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"srcPath={srcPath}&dstPath={dstPath}");
					if (IsSharedPath(srcPath))
					{
						(var srcBasePath, var srcCode, var srcSub) = DecodePath(srcPath);
						(var srcCreate, var srcRead, var srcWrite, var srcRemove) = GetSharedPermission(srcCode);
						if (type == "LINKED")
						{
							using (var db = new ThinkhubDbContext())
							{
								var shared = await db.Shared.SingleOrDefaultAsync(x => x.Code == srcCode);
								if (shared == null) return Json(new { success = FILE_RESULT.NO_SUCH_DIRECTORY.ToString() });
								else
								{
									var access = shared.Access.SingleOrDefault(x => x.UserId == SessionID);
									access.Path = dstPath;
									db.Access.Update(access);
									await db.SaveChangesAsync();
									return Json(new { type, success = FILE_RESULT.SUCCESS.ToString() });
								}

							}
						}
						else if (srcRead && srcRemove)
						{
							if (IsSharedPath(dstPath))
							{
								(var dstBasePath, var dstCode, var dstSub) = DecodePath(dstPath);
								(var dstCreate, var dstRead, var dstWrite, var dstRemove) = GetSharedPermission(dstCode);
								if (dstWrite)
								{
									var realSrcPath = GetSharedPath(srcCode, srcSub);
									var realDstPath = $@"{GetSharedPath(dstCode, dstSub)}\{Path.GetFileName(realSrcPath)}";
									type = type != "FILE" ? "DIR" : "FILE";
									var success = FileManager.Move(realSrcPath, realDstPath, type);
									return Json(new { type, success = success.ToString() });
								}
								else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
							}
							else
							{
								var realSrcPath = GetSharedPath(srcCode, srcSub);
								var realDstPath = $@"{UnmaskPath(dstPath)}\{Path.GetFileName(realSrcPath)}";
								type = type != "FILE" ? "DIR" : "FILE";
								var success = FileManager.Move(realSrcPath, realDstPath, type);
								return Json(new { type, success = success.ToString() });
							}
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						if (IsSharedPath(dstPath))
						{
							(var dstBasePath, var dstCode, var dstSub) = DecodePath(dstPath);
							(var dstCreate, var dstRead, var dstWrite, var dstRemove) = GetSharedPermission(dstCode);
							if (dstWrite)
							{
								var realSrcPath = UnmaskPath(srcPath);
								var realDstPath = $@"{GetSharedPath(dstCode, dstSub)}\{Path.GetFileName(realSrcPath)}";
								type = type != "FILE" ? "DIR" : "FILE";
								var success = FileManager.Move(realSrcPath, realDstPath, type);
								return Json(new { type, success = success.ToString() });
							}
							else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
						}
						else
						{
							var realSrcPath = UnmaskPath(srcPath);
							var realDstPath = $@"{UnmaskPath(dstPath)}\{Path.GetFileName(realSrcPath)}";
							type = type != "FILE" ? "DIR" : "FILE";
							var success = FileManager.Move(realSrcPath, realDstPath, type);
							if (success == FILE_RESULT.SUCCESS)
							{
								using (var db = new ThinkhubDbContext())
								{
									var shared = db.Shared.Where(x => x.UserId == SessionID && x.Path.StartsWith(realSrcPath));
									var access = db.Access.Where(x => x.UserId == SessionID && x.Path.StartsWith(srcPath));
									foreach (var i in shared) i.Path = i.Path.Replace(realSrcPath, realDstPath);
									foreach (var i in access) i.Path = i.Path.Replace(srcPath, $@"{dstPath}\{Path.GetFileName(srcPath)}");
									db.Shared.UpdateRange(shared);
									db.Access.UpdateRange(access);
									await db.SaveChangesAsync();

								}
							}
							return Json(new { type, success = success.ToString() });
						}
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> Delete(string path, string type)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						if (remove && type != "LINKED")
						{
							var realPath = GetSharedPath(code, sub);
							type = type != "FILE" ? "DIR" : "FILE";
							var success = FileManager.Delete(realPath, type);
							if (ClipboardFilePath == realPath && ClipboardFileType == type)
							{
								HttpContext.Session.Remove(Session.Clipboard.Type);
								HttpContext.Session.Remove(Session.Clipboard.FilePath);
								HttpContext.Session.Remove(Session.Clipboard.FileType);
								return Json(new { clipboard = false, success = success.ToString() });
							}
							else return Json(new { clipboard = true, success = success.ToString() });
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						var realPath = UnmaskPath(path);
						type = type != "FILE" ? "DIR" : "FILE";
						var success = FileManager.Delete(realPath, type);
						if (success == FILE_RESULT.SUCCESS)
						{
							using (var db = new ThinkhubDbContext())
							{
								var shared = db.Shared.Where(x => x.UserId == SessionID && x.Path.StartsWith(realPath));
								var access = db.Access.Where(x => x.UserId == SessionID && x.Path.StartsWith(path));
								foreach (var i in shared)
								{
									db.Permission.RemoveRange(i.Permission);
									db.Access.RemoveRange(i.Access);
								}
								db.Shared.RemoveRange(shared);
								db.Access.RemoveRange(access);
								await db.SaveChangesAsync();
							}
						}
						if (ClipboardFilePath == realPath && ClipboardFileType == type)
						{
							HttpContext.Session.Remove(Session.Clipboard.Type);
							HttpContext.Session.Remove(Session.Clipboard.FilePath);
							HttpContext.Session.Remove(Session.Clipboard.FileType);
							return Json(new { clipboard = false, success = success.ToString() });
						}
						else return Json(new { clipboard = true, success = success.ToString() });
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> Rename(string srcPath, string dstPath, string type)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"srcPath={srcPath}&dstPath={dstPath}");
					if (IsSharedPath(srcPath) && IsSharedPath(dstPath))
					{
						(var srcBasePath, var srcCode, var srcSub) = DecodePath(srcPath);
						(var dstBasePath, var dstCode, var dstSub) = DecodePath(dstPath);
						(var create, var read, var write, var remove) = GetSharedPermission(srcCode);
						if (create && read && write && remove && srcSub != null)
						{
							var realSrcPath = GetSharedPath(srcCode, srcSub);
							var realDstPath = GetSharedPath(dstCode, dstSub);
							var success = FileManager.Move(realSrcPath, realDstPath, type);
							if (ClipboardFilePath == realSrcPath && ClipboardFileType == type)
							{
								HttpContext.Session.Remove(Session.Clipboard.Type);
								HttpContext.Session.Remove(Session.Clipboard.FilePath);
								HttpContext.Session.Remove(Session.Clipboard.FileType);
								return Json(new { clipboard = false, success = success.ToString() });
							}
							else return Json(new { clipboard = true, success = success.ToString() });
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						var realSrcPath = UnmaskPath(srcPath);
						var realDstPath = UnmaskPath(dstPath);
						type = type != "FILE" ? "DIR" : "FILE";
						var success = FileManager.Move(realSrcPath, realDstPath, type);
						if (success == FILE_RESULT.SUCCESS)
						{
							using (var db = new ThinkhubDbContext())
							{
								var shared = db.Shared.Where(x => x.UserId == SessionID && x.Path.StartsWith(realSrcPath));
								var access = db.Access.Where(x => x.UserId == SessionID && x.Path.StartsWith(srcPath));
								foreach (var i in shared) i.Path = i.Path.Replace(realSrcPath, realDstPath);
								foreach (var i in access) i.Path = i.Path.Replace(srcPath, dstPath);
								db.Shared.UpdateRange(shared);
								db.Access.UpdateRange(access);
								await db.SaveChangesAsync();
							}
						}
						if (ClipboardFilePath == realSrcPath && ClipboardFileType == type)
						{
							HttpContext.Session.Remove(Session.Clipboard.Type);
							HttpContext.Session.Remove(Session.Clipboard.FilePath);
							HttpContext.Session.Remove(Session.Clipboard.FileType);
							return Json(new { clipboard = false, success = success.ToString() });
						}
						else return Json(new { clipboard = true, success = success.ToString() });
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> Paste(string path)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					var type = ClipboardType;
					if (type == "CUT")
					{
						if (IsSharedPath(path))
						{
							(var basePath, var code, var sub) = DecodePath(path);
							(var create, var read, var write, var remove) = GetSharedPermission(code);
							if (create && write)
							{
								var srcPath = ClipboardFilePath;
								string realSrcPath;
								if (ClipboardIsShared) realSrcPath = GetSharedPath(ClipboardSharedCode, ClipboardSharedSub);
								else realSrcPath = UnmaskPath(srcPath);

								var parentPath = GetSharedPath(code, sub);
								var realDstPath = $@"{parentPath}\{Path.GetFileName(realSrcPath)}";
								var fileType = ClipboardFileType != "FILE" ? "DIR" : "FILE";
								var success = FileManager.Move(realSrcPath, realDstPath, fileType);
								return Json(new { type, success = success.ToString() });
							}
							else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
						}
						else
						{
							var srcPath = ClipboardFilePath;
							string realSrcPath;
							if (ClipboardIsShared) realSrcPath = GetSharedPath(ClipboardSharedCode, ClipboardSharedSub);
							else realSrcPath = UnmaskPath(srcPath);

							var parentPath = UnmaskPath(path);
							var realDstPath = $@"{parentPath}\{Path.GetFileName(realSrcPath)}";
							var fileType = ClipboardFileType != "FILE" ? "DIR" : "FILE";
							var success = FileManager.Move(realSrcPath, realDstPath, fileType);
							if (success == FILE_RESULT.SUCCESS)
							{
								using (var db = new ThinkhubDbContext())
								{
									var shared = db.Shared.Where(x => x.UserId == SessionID && x.Path.StartsWith(realSrcPath));
									var access = db.Access.Where(x => x.UserId == SessionID && x.Path.StartsWith(srcPath));
									foreach (var i in shared) i.Path = i.Path.Replace(realSrcPath, realDstPath);
									foreach (var i in access) i.Path = i.Path.Replace(srcPath, $@"{path}\{Path.GetFileName(srcPath)}");
									db.Shared.UpdateRange(shared);
									db.Access.UpdateRange(access);
									await db.SaveChangesAsync();
								}
							}
							return Json(new { type, success = success.ToString() });
						}
					}
					else
					{
						if (IsSharedPath(path))
						{
							(var basePath, var code, var sub) = DecodePath(path);
							(var create, var read, var write, var remove) = GetSharedPermission(code);
							if (create && write)
							{
								var srcPath = ClipboardFilePath;
								string realSrcPath;
								if (ClipboardIsShared) realSrcPath = GetSharedPath(ClipboardSharedCode, ClipboardSharedSub);
								else realSrcPath = UnmaskPath(srcPath);

								var parentPath = GetSharedPath(code, sub);
								var realDstPath = $@"{parentPath}\{Path.GetFileName(realSrcPath)}";
								var fileType = ClipboardFileType != "FILE" ? "DIR" : "FILE";
								var success = FileManager.Copy(realSrcPath, realDstPath, fileType);
								return Json(new { type, success = success.ToString() });
							}
							else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
						}
						else
						{
							var srcPath = ClipboardFilePath;
							string realSrcPath;
							if (ClipboardIsShared) realSrcPath = GetSharedPath(ClipboardSharedCode, ClipboardSharedSub);
							else realSrcPath = UnmaskPath(srcPath);

							var parentPath = UnmaskPath(path);
							var realDstPath = $@"{parentPath}\{Path.GetFileName(realSrcPath)}";
							var fileType = ClipboardFileType != "FILE" ? "DIR" : "FILE";
							var success = FileManager.Copy(realSrcPath, realDstPath, fileType);
							return Json(new { type, success = success.ToString() });
						}
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public async Task<IActionResult> Thumbnail(string path, string media)
		{
			try
			{
				if (IsLoggedIn)
				{
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $"path={path}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						if (read)
						{
							var realPath = GetSharedPath(code, sub);
							var temp = XmlConfiguration.TempDirectory;
							if (media == "IMG")
							{
								var success = FileManager.ImageThumb(realPath, out FILE_INFO file);
								file.Path = EncodePath(basePath, code, sub);
								return Json(new { success = success.ToString(), file });
							}
							else
							{
								if (!Directory.Exists(XmlConfiguration.TempDirectory)) Directory.CreateDirectory(XmlConfiguration.TempDirectory);
								var (success, file) = await FileManager.VideoThumb(realPath, temp);
								file.Path = EncodePath(basePath, code, sub);
								return Json(new { success = success.ToString(), file });
							}
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						var realPath = UnmaskPath(path);
						var temp = XmlConfiguration.TempDirectory;
						if (media == "IMG")
						{
							var success = FileManager.ImageThumb(realPath, out FILE_INFO file);
							file.Path = MaskPath(file.Path);
							return Json(new { success = success.ToString(), file });
						}
						else
						{
							if (!Directory.Exists(XmlConfiguration.TempDirectory)) Directory.CreateDirectory(XmlConfiguration.TempDirectory);
							var (success, file) = await FileManager.VideoThumb(realPath, temp);
							file.Path = MaskPath(file.Path);
							return Json(new { success = success.ToString(), file });
						}
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}

		[HttpPost] public async Task<IActionResult> Upload()
		{
			try
			{
				if (IsLoggedIn)
				{
					var chunks = Request.Form.Files;
					if (chunks.Count > 0)
					{
						var path = CurrentPath;
						if (IsSharedPath(path))
						{
							(var basePath, var code, var sub) = DecodePath(path);
							(var create, var read, var write, var remove) = GetSharedPermission(code);
							if (create && write)
							{
								var realPath = GetSharedPath(code, sub);
								var name = chunks[0].FileName;
								var dst = $@"{realPath}\{name}";
								return Json(new
								{
									status = await FileManager.Upload(dst, chunks[0].OpenReadStream()),
									name
								});
							}
							else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
						}
						else
						{
							var realPath = UnmaskPath(path);
							var name = chunks[0].FileName;
							var dst = $@"{realPath}\{name}";
							return Json(new
							{
								status = await FileManager.Upload(dst, chunks[0].OpenReadStream()),
								name
							});
						}
					}
					else return Json(new { status = false });
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public IActionResult StartUpload(string path, string file, bool overwrite)
		{
			try
			{
				if (IsLoggedIn)
				{
					if (file.IsNormalized(NormalizationForm.FormD)) file = file.Normalize(NormalizationForm.FormC);
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $@"path={path}\{file}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						if (create && write)
						{
							var realPath = GetSharedPath(code, sub);
							var dst = $@"{realPath}\{file}";
							var uploadPath = $@"{XmlConfiguration.UserDirectory}{UserName}{XmlConfiguration.UploadDirectory}\{file}";
							return Json(new
							{
								name = file,
								success = FileManager.StartUpload(dst, uploadPath, overwrite).ToString()
							});
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						var realPath = UnmaskPath(path);
						var dst = $@"{realPath}\{file}";
						var uploadPath = $@"{XmlConfiguration.UserDirectory}{UserName}{XmlConfiguration.UploadDirectory}\{file}";
						return Json(new
						{
							name = file,
							success = FileManager.StartUpload(dst, uploadPath, overwrite).ToString()
						});
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public IActionResult CancelUpload(string path, string file)
		{
			try
			{
				if (IsLoggedIn)
				{
					if (file.IsNormalized(NormalizationForm.FormD)) file = file.Normalize(NormalizationForm.FormC);
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $@"path={path}\{file}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						if (create && write)
						{
							var realPath = GetSharedPath(code, sub);
							var dst = $@"{realPath}\{file}";
							return Json(new
							{
								name = file,
								success = FileManager.CancelUpload(dst).ToString()
							});
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						var realPath = UnmaskPath(path);
						var dst = $@"{realPath}\{file}";
						return Json(new
						{
							name = file,
							success = FileManager.CancelUpload(dst).ToString()
						});
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
		[HttpPost] public IActionResult FinishUpload(string path, string file)
		{
			try
			{
				if (IsLoggedIn)
				{
					if (file.IsNormalized(NormalizationForm.FormD)) file = file.Normalize(NormalizationForm.FormC);
					LogManager.Log(RequestClient, RequestType, UserName, RequestPath, $@"path={path}\{file}");
					if (IsSharedPath(path))
					{
						(var basePath, var code, var sub) = DecodePath(path);
						(var create, var read, var write, var remove) = GetSharedPermission(code);
						if (create && write)
						{
							var realPath = GetSharedPath(code, sub);
							var dst = $@"{realPath}\{file}";
							return Json(new
							{
								name = file,
								success = FileManager.FinishUpload(dst).ToString()
							});
						}
						else return Json(new { success = FILE_RESULT.ACCESS_DENIED.ToString() });
					}
					else
					{
						var realPath = UnmaskPath(path);
						var dst = $@"{realPath}\{file}";
						return Json(new
						{
							name = file,
							success = FileManager.FinishUpload(dst).ToString()
						});
					}
				}
				else return Json(new { success = FILE_RESULT.NOT_LOGGEDIN.ToString() });
			}
			catch (Exception e)
			{
				LogManager.Error(e);
				return null;
			}
		}
	}
}