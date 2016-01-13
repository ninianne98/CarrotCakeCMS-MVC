using Carrotware.CMS.Data;
using Carrotware.CMS.Security;
using Carrotware.CMS.Security.Models;
using Carrotware.Web.UI.Components;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public class SecurityData {

		public SecurityData() { }

		public static UserRole FindRole(string RoleName) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				return (from r in _db.membership_Roles
						where r.Name == RoleName
						select new UserRole(r)).FirstOrDefault();
			}
		}

		public static UserRole FindRoleByID(string roleID) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				return (from r in _db.membership_Roles
						where r.Id == roleID
						select new UserRole(r)).FirstOrDefault();
			}
		}

		public static List<UserRole> GetRoleList() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				return (from r in _db.membership_Roles
						orderby r.Name
						select new UserRole(r)).ToList();
			}
		}

		public static List<UserRole> GetRoleListRestricted() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				if (!SecurityData.IsAdmin) {
					return (from r in _db.membership_Roles
							where r.Name != SecurityData.CMSGroup_Users && r.Name != SecurityData.CMSGroup_Admins
							orderby r.Name
							select new UserRole(r)).ToList();
				} else {
					return (from r in _db.membership_Roles
							where r.Name != SecurityData.CMSGroup_Users
							orderby r.Name
							select new UserRole(r)).ToList();
				}
			}
		}

		public static List<ApplicationUser> GetUserSearch(string searchTerm) {
			using (var securityHelper = new SecurityHelper()) {
				return (from u in securityHelper.DataContext.Users
						where u.UserName.Contains(searchTerm)
								|| u.Email.Contains(searchTerm)
						select securityHelper.UserManager.FindByName(u.UserName)).Take(100).ToList();
			}
		}

		public static UserProfile GetProfileByUserID(Guid userId) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				return (from u in _db.membership_Users
						join ud1 in _db.carrot_UserDatas on u.Id equals ud1.UserKey into ud2
						from ud in ud2.DefaultIfEmpty()
						where ud.UserId == userId
						select new UserProfile(u, ud)).FirstOrDefault();
			}
		}

		public static UserProfile GetProfileByUserName(string userName) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				return (from u in _db.membership_Users
						join ud1 in _db.carrot_UserDatas on u.Id equals ud1.UserKey into ud2
						from ud in ud2.DefaultIfEmpty()
						where u.UserName == userName
						select new UserProfile(u, ud)).FirstOrDefault();
			}
		}

		public static List<UserProfile> GetUserProfileSearch(string searchTerm) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				return (from u in _db.membership_Users
						join ud1 in _db.carrot_UserDatas on u.Id equals ud1.UserKey into ud2
						from ud in ud2.DefaultIfEmpty()
						where u.UserName.Contains(searchTerm)
								   || u.Email.Contains(searchTerm)
						select new UserProfile(u, ud)).Take(100).ToList();
			}
		}

		public static List<ApplicationUser> GetCreditUserSearch(string searchTerm) {
			List<ApplicationUser> usrs = null;
			List<string> admins = null;
			List<string> editors = null;

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				admins = (from ur in _db.membership_UserRoles
						  join u in _db.membership_Users on ur.UserId equals u.Id
						  join r in _db.membership_Roles on ur.RoleId equals r.Id
						  join ud in _db.carrot_UserDatas on u.Id equals ud.UserKey
						  where r.Name == CMSGroup_Admins
						  select ud.UserKey).ToList();

				editors = (from sm in _db.carrot_UserSiteMappings
						   join ud in _db.carrot_UserDatas on sm.UserId equals ud.UserId
						   where sm.SiteID == SiteData.CurrentSiteID
						   select ud.UserKey).ToList();
			}

			using (var securityHelper = new SecurityHelper()) {
				usrs = (from u in securityHelper.DataContext.Users
						where (u.UserName.Contains(searchTerm)
									|| u.Email.Contains(searchTerm))
								&& admins.Union(editors).Contains(u.Id)
						select securityHelper.UserManager.FindByName(u.UserName)).Take(50).ToList();
			}

			return usrs;
		}

		public static List<UserProfile> GetCreditUserProfileSearch(string searchTerm) {
			List<UserProfile> usrs = null;
			List<string> admins = null;
			List<string> editors = null;

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				admins = (from ur in _db.membership_UserRoles
						  join u in _db.membership_Users on ur.UserId equals u.Id
						  join r in _db.membership_Roles on ur.RoleId equals r.Id
						  join ud in _db.carrot_UserDatas on u.Id equals ud.UserKey
						  where r.Name == CMSGroup_Admins
						  select ud.UserKey).ToList();

				editors = (from sm in _db.carrot_UserSiteMappings
						   join ud in _db.carrot_UserDatas on sm.UserId equals ud.UserId
						   where sm.SiteID == SiteData.CurrentSiteID
						   select ud.UserKey).ToList();

				usrs = (from u in _db.membership_Users
						join ud1 in _db.carrot_UserDatas on u.Id equals ud1.UserKey into ud2
						from ud in ud2.DefaultIfEmpty()
						where (u.UserName.Contains(searchTerm)
									|| u.Email.Contains(searchTerm))
								&& admins.Union(editors).Contains(u.Id)
						select new UserProfile(u, ud)).Take(50).ToList();
			}

			return usrs;
		}

		public static List<ApplicationUser> GetUserListByEmail(string email) {
			using (var securityHelper = new SecurityHelper()) {
				return (from u in securityHelper.DataContext.Users
						where u.Email.Contains(email)
						select securityHelper.UserManager.FindByName(u.UserName)).Take(50).ToList();
			}
		}

		public static List<ApplicationUser> GetUserListByName(string usrName) {
			using (var securityHelper = new SecurityHelper()) {
				return (from u in securityHelper.DataContext.Users
						where (u.UserName.Contains(usrName))
						select securityHelper.UserManager.FindByName(u.UserName)).Take(50).ToList();
			}
		}

		public static List<ApplicationUser> GetUserList() {
			using (var securityHelper = new SecurityHelper()) {
				return (from u in securityHelper.DataContext.Users
						select securityHelper.UserManager.FindByName(u.UserName)).Take(1000).ToList();
			}
		}

		public static List<ApplicationUser> GetUsersInRole(string groupName) {
			List<ApplicationUser> usrs = new List<ApplicationUser>();

			using (var securityHelper = new SecurityHelper()) {
				var role = (from r in securityHelper.DataContext.Roles
							where r.Name == groupName
							select r).FirstOrDefault();

				if (role != null) {
					usrs = (from ur in role.Users
							join u in securityHelper.DataContext.Users on ur.UserId equals u.Id
							select securityHelper.UserManager.FindByName(u.UserName)).Take(2500).ToList();
				}
			}

			return usrs;
		}

		public static string CMSGroup_Admins {
			get {
				return "CarrotCMS Administrators";
			}
		}

		public static string CMSGroup_Editors {
			get {
				return "CarrotCMS Editors";
			}
		}

		public static string CMSGroup_Users {
			get {
				return "CarrotCMS Users";
			}
		}

		private static string keyIsAdmin = "cms_IsAdmin";

		private static string keyIsSiteEditor = "cms_IsSiteEditor";

		public static bool GetIsAdminFromCache() {
			bool keyVal = false;

			if (SiteData.IsWebView && IsAuthenticated) {
				string key = String.Format("{0}_{1}", keyIsAdmin, SecurityData.CurrentUserIdentityName);
				if (HttpContext.Current.Cache[key] != null) {
					keyVal = Convert.ToBoolean(HttpContext.Current.Cache[key]);
				} else {
					keyVal = IsUserInRole(SecurityData.CMSGroup_Admins);
					HttpContext.Current.Cache.Insert(key, keyVal.ToString(), null, DateTime.Now.AddSeconds(30), Cache.NoSlidingExpiration);
				}
			}

			return keyVal;
		}

		public static bool GetIsSiteEditorFromCache() {
			bool keyVal = false;

			if (SiteData.IsWebView && IsAuthenticated) {
				string key = String.Format("{0}_{1}_{2}", keyIsSiteEditor, SecurityData.CurrentUserIdentityName, SiteData.CurrentSiteID);
				if (HttpContext.Current.Cache[key] != null) {
					keyVal = Convert.ToBoolean(HttpContext.Current.Cache[key]);
				} else {
					ExtendedUserData usrEx = SecurityData.CurrentExUser;

					keyVal = (IsEditor || usrEx.IsEditor) && usrEx.MemberSiteIDs.Contains(SiteData.CurrentSiteID);

					HttpContext.Current.Cache.Insert(key, keyVal.ToString(), null, DateTime.Now.AddSeconds(30), Cache.NoSlidingExpiration);
				}
			}

			return keyVal;
		}

		public static bool IsAdmin {
			get {
				return GetIsAdminFromCache();
			}
		}

		public static bool IsEditor {
			get {
				try {
					if (SiteData.IsWebView && IsAuthenticated) {
						return IsUserInRole(SecurityData.CMSGroup_Editors);
					}
					return false;
				} catch (Exception ex) {
					return false;
				}
			}
		}

		public static bool IsUsers {
			get {
				try {
					if (SiteData.IsWebView && IsAuthenticated) {
						return IsUserInRole(SecurityData.CMSGroup_Users);
					}
					return false;
				} catch (Exception ex) {
					return false;
				}
			}
		}

		public static IPrincipal UserPrincipal {
			get {
				return HttpContext.Current.User;
			}
		}

		public static bool IsAuthenticated {
			get {
				if (SiteData.IsWebView && UserPrincipal.Identity.IsAuthenticated) {
					return true;
				}

				return false;
			}
		}

		public static string GetUserName() {
			if (IsAuthenticated) {
				return UserPrincipal.Identity.GetUserName();
			}

			return String.Empty;
		}

		public static bool IsUserInRole(string groupName) {
			return IsUserInRole(SecurityData.CurrentUserIdentityName, groupName);
		}

		private static string keyIsUserInRole = "cms_IsUserInRole";

		public static bool IsUserInRole(string userName, string groupName) {
			bool keyVal = false;

			if (SiteData.IsWebView && IsAuthenticated) {
				string key = String.Format("{0}_{1}_{2}", keyIsUserInRole, userName.ToLowerInvariant(), groupName.ToLowerInvariant());

				if (HttpContext.Current.Cache[key] != null) {
					keyVal = Convert.ToBoolean(HttpContext.Current.Cache[key]);
				} else {
					using (var securityHelper = new SecurityHelper()) {
						var _user = securityHelper.UserManager.FindByName(userName);

						keyVal = securityHelper.UserManager.IsInRole(_user.Id, groupName);
					}
					HttpContext.Current.Cache.Insert(key, keyVal.ToString(), null, DateTime.Now.AddSeconds(15), Cache.NoSlidingExpiration);
				}
			}

			return keyVal;
		}

		public static bool IsSiteEditor {
			get {
				return GetIsSiteEditorFromCache();
			}
		}

		public static bool IsAuthEditor {
			get {
				if (SiteData.IsWebView && IsAuthenticated) {
					return AdvancedEditMode || IsAdmin || IsSiteEditor;
				} else {
					return false;
				}
			}
		}

		public static Guid CurrentUserGuid {
			get {
				Guid _currentUserGuid = Guid.Empty;
				if (CurrentUser != null) {
					_currentUserGuid = SecurityData.CurrentExUser.UserId;
				}
				return _currentUserGuid;
			}
		}

		public static UserProfile CurrentUser {
			get {
				UserProfile currentUser = null;
				if (SiteData.IsWebView && IsAuthenticated) {
					string userName = SecurityData.CurrentUserIdentityName;
					string key = String.Format("cms_CurrentUserProfile_{0}", userName);

					if (HttpContext.Current.Cache[key] != null) {
						currentUser = (UserProfile)HttpContext.Current.Cache[key];
					} else {
						using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
							currentUser = (from u in _db.membership_Users
										   join ud1 in _db.carrot_UserDatas on u.Id equals ud1.UserKey into ud2
										   from ud in ud2.DefaultIfEmpty()
										   where u.UserName == userName
										   select new UserProfile(u, ud)).FirstOrDefault();
						}

						if (currentUser != null) {
							HttpContext.Current.Cache.Insert(key, currentUser, null, DateTime.Now.AddSeconds(90), Cache.NoSlidingExpiration);
						}
					}
				} else {
					currentUser = new UserProfile();
					currentUser.UserId = Guid.Empty;
					currentUser.UserKey = Guid.Empty.ToString();
					currentUser.UserName = "anonymous-user-" + Guid.Empty.ToString();
				}

				return currentUser;
			}
		}

		public static ExtendedUserData CurrentExUser {
			get {
				ExtendedUserData currentUser = null;

				if (SiteData.IsWebView && IsAuthenticated) {
					string userName = SecurityData.CurrentUserIdentityName;
					string key = String.Format("cms_CurrentExUser_{0}", userName);

					if (HttpContext.Current.Cache[key] != null) {
						currentUser = (ExtendedUserData)HttpContext.Current.Cache[key];
					} else {
						using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
							currentUser = (from u in _db.vw_carrot_UserDatas
										   where u.UserName == userName
										   select new ExtendedUserData(u)).FirstOrDefault();
						}

						if (currentUser != null) {
							HttpContext.Current.Cache.Insert(key, currentUser, null, DateTime.Now.AddSeconds(90), Cache.NoSlidingExpiration);
						}
					}
				} else {
					currentUser = new ExtendedUserData();
					currentUser.UserId = Guid.Empty;
					currentUser.UserKey = Guid.Empty.ToString();
					currentUser.UserName = "anonymous-user-" + Guid.Empty.ToString();
				}

				return currentUser;
			}
		}

		public static ApplicationUser CurrentApplicationUser {
			get {
				ApplicationUser _currentUser = null;
				if (IsAuthenticated) {
					using (var securityHelper = new SecurityHelper()) {
						_currentUser = securityHelper.UserManager.FindByName(SecurityData.CurrentUserIdentityName);
					}
				}
				return _currentUser;
			}
		}

		public static ApplicationUser GetUserByID(string key) {
			using (var securityHelper = new SecurityHelper()) {
				return securityHelper.UserManager.FindById(key);
			}
		}

		public static ApplicationUser GetUserByName(string username) {
			using (var securityHelper = new SecurityHelper()) {
				return securityHelper.UserManager.FindByName(username);
			}
		}

		public static ApplicationUser GetUserByEmail(string email) {
			using (var securityHelper = new SecurityHelper()) {
				return securityHelper.UserManager.FindByEmail(email);
			}
		}

		public static bool AdvancedEditMode {
			get {
				bool _Advanced = false;
				if (SiteData.IsWebView && IsAuthenticated) {
					if (HttpContext.Current.Request.QueryString[SiteData.AdvancedEditParameter] != null && (SecurityData.IsAdmin || SecurityData.IsSiteEditor)) {
						_Advanced = true;
					} else {
						_Advanced = false;
					}
				}

				return _Advanced;
			}
		}

		private static string CurrentDLLVersion {
			get { return SiteData.CurrentDLLVersion; }
		}

		public static string CurrentUserIdentityName {
			get {
				if (SiteData.IsWebView && IsAuthenticated) {
					if (!String.IsNullOrEmpty(UserPrincipal.Identity.Name)) {
						return UserPrincipal.Identity.Name.ToLowerInvariant();
					}
				}
				return String.Empty;
			}
		}

		private static object newUsrLock = new Object();

		public IdentityResult CreateApplicationUser(ApplicationUser user, out ExtendedUserData newusr) {
			return AttemptCreateApplicationUser(user, SecurityData.GenerateSimplePassword(), out newusr);
		}

		public IdentityResult CreateApplicationUser(ApplicationUser user, string password, out ExtendedUserData newusr) {
			return AttemptCreateApplicationUser(user, password, out newusr);
		}

		private IdentityResult AttemptCreateApplicationUser(ApplicationUser user, string password, out ExtendedUserData newusr) {
			newusr = null;
			var result = new IdentityResult();

			lock (newUsrLock) {
				if (user != null && !String.IsNullOrEmpty(user.Id)) {
					using (var securityHelper = new SecurityHelper()) {
						result = securityHelper.UserManager.Create(user, password);

						if (result.Succeeded) {
							user = securityHelper.UserManager.FindByName(user.UserName);

							newusr = new ExtendedUserData();
							newusr.UserKey = user.Id;
							newusr.Id = user.Id;
							newusr.Save();

							newusr = ExtendedUserData.FindByUserID(newusr.UserId);
						}
					}
				}
			}

			return result;
		}

		public IdentityResult ResetPassword(ApplicationUser user, string code, string password) {
			IdentityResult result = new IdentityResult();

			if (user != null && !String.IsNullOrEmpty(user.Id)) {
				using (var securityHelper = new SecurityHelper()) {
					result = securityHelper.UserManager.ResetPassword(user.Id, code, password);

					return result;
				}
			}

			return result;
		}

		public bool ResetPassword(string email) {
			string adminFolder = SiteData.AdminFolderPath;
			if (adminFolder.StartsWith("/")) {
				adminFolder = adminFolder.Substring(1);
			}
			if (adminFolder.EndsWith("/")) {
				adminFolder = adminFolder.Substring(0, adminFolder.Length - 1);
			}

			string adminEmailPath = String.Format("{0}/ResetPassword", adminFolder);

			return ResetPassword(adminEmailPath, email);
		}

		public bool ResetPassword(string resetUri, string email) {
			HttpRequest request = HttpContext.Current.Request;
			ApplicationUser user = null;
			string code = String.Empty;

			if (resetUri.StartsWith("/")) {
				resetUri = resetUri.Substring(1);
			}

			if (!String.IsNullOrEmpty(email)) {
				using (var securityHelper = new SecurityHelper()) {
					user = securityHelper.UserManager.FindByEmail(email);

					if (user != null) {
						code = securityHelper.UserManager.GeneratePasswordResetToken(user.Id);
					}
				}
			}

			if (user != null) {
				string sBody = String.Empty;
				Assembly _assembly = Assembly.GetExecutingAssembly();

				using (StreamReader oTextStream = new StreamReader(_assembly.GetManifestResourceStream("Carrotware.CMS.Core.Security.EmailForgotPassMsg.txt"))) {
					sBody = oTextStream.ReadToEnd();
				}

				string strHTTPHost = String.Empty;
				try { strHTTPHost = request.ServerVariables["HTTP_HOST"].ToString().Trim(); } catch { strHTTPHost = String.Empty; }

				string hostName = strHTTPHost.ToLowerInvariant();

				string strHTTPPrefix = "http://";
				try {
					strHTTPPrefix = request.ServerVariables["SERVER_PORT_SECURE"] == "1" ? "https://" : "http://";
				} catch { strHTTPPrefix = "http://"; }

				strHTTPHost = String.Format("{0}{1}", strHTTPPrefix, strHTTPHost).ToLowerInvariant();

				var resetTokenUrl = String.Format("{0}/{1}?userId={2}&code={3}", strHTTPHost, resetUri, user.Id, HttpUtility.UrlEncode(code));

				sBody = sBody.Replace("{%%UserName%%}", user.UserName);
				sBody = sBody.Replace("{%%SiteURL%%}", strHTTPHost);
				sBody = sBody.Replace("{%%ResetURL%%}", resetTokenUrl);
				sBody = sBody.Replace("{%%Version%%}", CurrentDLLVersion);

				if (SiteData.CurretSiteExists) {
					sBody = sBody.Replace("{%%Time%%}", SiteData.CurrentSite.Now.ToString());
				} else {
					sBody = sBody.Replace("{%%Time%%}", DateTime.Now.ToString());
				}

				EmailHelper.SendMail(null, user.Email, String.Format("Reset Password {0}", hostName), sBody, false);

				return true;
			} else {
				return false;
			}
		}

		public static bool RemoveUserFromRole(string userName, string roleName) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				membership_UserRole usrRole = (from r in _db.membership_Roles
											   join ur in _db.membership_UserRoles on r.Id equals ur.RoleId
											   join u in _db.membership_Users on ur.UserId equals u.Id
											   where r.Name == roleName
													   && u.UserName == userName
											   select ur).FirstOrDefault();

				if (usrRole != null) {
					_db.membership_UserRoles.DeleteOnSubmit(usrRole);
					_db.SubmitChanges();

					return true;
				}
				return false;
			}
		}

		public static bool AddUserToRole(string userName, string roleName) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				membership_Role role = (from r in _db.membership_Roles
										where r.Name == roleName
										select r).FirstOrDefault();

				membership_User user = (from u in _db.membership_Users
										where u.UserName == userName
										select u).FirstOrDefault();

				membership_UserRole usrRole = (from r in _db.membership_Roles
											   join ur in _db.membership_UserRoles on r.Id equals ur.RoleId
											   join u in _db.membership_Users on ur.UserId equals u.Id
											   where r.Name == roleName
													   && u.UserName == userName
											   select ur).FirstOrDefault();

				if (usrRole == null && role != null && user != null) {
					usrRole = new membership_UserRole();
					usrRole.UserId = user.Id;
					usrRole.RoleId = role.Id;
					_db.membership_UserRoles.InsertOnSubmit(usrRole);
					_db.SubmitChanges();

					return true;
				}
				return false;
			}
		}

		public static bool AddUserToRole(Guid UserId, string roleName) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				membership_Role role = (from r in _db.membership_Roles
										where r.Name == roleName
										select r).FirstOrDefault();

				membership_User user = (from u in _db.membership_Users
										join ud in _db.carrot_UserDatas on u.Id equals ud.UserKey
										where ud.UserId == UserId
										select u).FirstOrDefault();

				membership_UserRole usrRole = (from r in _db.membership_Roles
											   join ur in _db.membership_UserRoles on r.Id equals ur.RoleId
											   join u in _db.membership_Users on ur.UserId equals u.Id
											   join ud in _db.carrot_UserDatas on u.Id equals ud.UserKey
											   where r.Name == roleName
													   && ud.UserId == UserId
											   select ur).FirstOrDefault();

				if (usrRole == null && role != null && user != null) {
					usrRole = new membership_UserRole();
					usrRole.UserId = user.Id;
					usrRole.RoleId = role.Id;
					_db.membership_UserRoles.InsertOnSubmit(usrRole);
					_db.SubmitChanges();

					return true;
				}
				return false;
			}
		}

		//create constant strings for each type of characters
		private static string alphaCaps = "QWERTYUIOPASDFGHJKLZXCVBNM";

		private static string alphaLow = "qwertyuiopasdfghjklzxcvbnm";
		private static string numerics = "1234567890";
		private static string special = "@#$";
		private static string nonalphanum = "&%=:";

		//create another string which is a concatenation of all above
		private static string allChars = alphaCaps + alphaLow + numerics + special;

		private static string specialChars = special + nonalphanum;

		public static string GenerateSimplePassword() {
			int length = 12;
			Random r = new Random();
			string generatedPassword = String.Empty;

			for (int i = 0; i < length; i++) {
				double rand = r.NextDouble();
				if (i == 0) {
					//First character is an upper case alphabet
					generatedPassword += alphaCaps.ToCharArray()[(int)Math.Floor(rand * alphaCaps.Length)];
				} else if (i == length - 3) {
					generatedPassword += specialChars.ToCharArray()[(int)Math.Floor(rand * specialChars.Length)];
				} else if (i == length - 5) {
					generatedPassword += numerics.ToCharArray()[(int)Math.Floor(rand * numerics.Length)];
				} else if (i == length - 7) {
					generatedPassword += alphaLow.ToCharArray()[(int)Math.Floor(rand * alphaLow.Length)];
				} else {
					generatedPassword += allChars.ToCharArray()[(int)Math.Floor(rand * allChars.Length)];
				}
			}

			return generatedPassword;
		}
	}
}