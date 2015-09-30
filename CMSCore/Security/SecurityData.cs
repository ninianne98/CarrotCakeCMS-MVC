﻿using Carrotware.CMS.Data;
using Carrotware.CMS.Security.Models;
using Carrotware.Web.UI.Components;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Caching;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public class SecurityData {

		public SecurityData() { }

		public static UserRole FindRole(string RoleName) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				return (from r in _db.membership_Roles
						where r.Name.ToLower() == RoleName.ToLower()
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
			using (var db = SecurityDbContext.Create()) {
				using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db))) {
					return (from u in db.Users
							where u.UserName.ToLower().Contains(searchTerm.ToLower())
									|| u.Email.ToLower().Contains(searchTerm.ToLower())
							select userManager.FindByName(u.UserName)).Take(100).ToList();
				}
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
						where u.UserName.ToLower() == userName.ToLower()
						select new UserProfile(u, ud)).FirstOrDefault();
			}
		}

		public static List<UserProfile> GetUserProfileSearch(string searchTerm) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				return (from u in _db.membership_Users
						join ud1 in _db.carrot_UserDatas on u.Id equals ud1.UserKey into ud2
						from ud in ud2.DefaultIfEmpty()
						where u.UserName.ToLower().Contains(searchTerm.ToLower())
								   || u.Email.ToLower().Contains(searchTerm.ToLower())
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
						  select ud.UserKey.ToLower()).ToList();

				editors = (from sm in _db.carrot_UserSiteMappings
						   join ud in _db.carrot_UserDatas on sm.UserId equals ud.UserId
						   where sm.SiteID == SiteData.CurrentSiteID
						   select ud.UserKey.ToLower()).ToList();
			}

			using (var db = SecurityDbContext.Create()) {
				using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db))) {
					usrs = (from u in db.Users
							where (u.UserName.ToLower().Contains(searchTerm.ToLower())
										|| u.Email.ToLower().Contains(searchTerm.ToLower()))
									&& admins.Union(editors).Contains(u.Id.ToLower())
							select userManager.FindByName(u.UserName)).Take(50).ToList();
				}
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
						  select ud.UserKey.ToLower()).ToList();

				editors = (from sm in _db.carrot_UserSiteMappings
						   join ud in _db.carrot_UserDatas on sm.UserId equals ud.UserId
						   where sm.SiteID == SiteData.CurrentSiteID
						   select ud.UserKey.ToLower()).ToList();

				usrs = (from u in _db.membership_Users
						join ud1 in _db.carrot_UserDatas on u.Id equals ud1.UserKey into ud2
						from ud in ud2.DefaultIfEmpty()
						where (u.UserName.ToLower().Contains(searchTerm.ToLower())
									|| u.Email.ToLower().Contains(searchTerm.ToLower()))
								&& admins.Union(editors).Contains(u.Id.ToLower())
						select new UserProfile(u, ud)).Take(50).ToList();
			}

			return usrs;
		}

		public static List<ApplicationUser> GetUserListByEmail(string email) {
			using (var db = SecurityDbContext.Create()) {
				using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db))) {
					return (from u in db.Users
							where u.Email.ToLower().Contains(email.ToLower())
							select userManager.FindByName(u.UserName)).Take(50).ToList();
				}
			}
		}

		public static List<ApplicationUser> GetUserListByName(string usrName) {
			using (var db = SecurityDbContext.Create()) {
				using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db))) {
					return (from u in db.Users
							where (u.UserName.ToLower().Contains(usrName.ToLower()))
							select userManager.FindByName(u.UserName)).Take(50).ToList();
				}
			}
		}

		public static List<ApplicationUser> GetUserList() {
			using (var db = SecurityDbContext.Create()) {
				using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db))) {
					return (from u in db.Users
							select userManager.FindByName(u.UserName)).Take(1000).ToList();
				}
			}
		}

		public static List<ApplicationUser> GetUsersInRole(string groupName) {
			List<ApplicationUser> usrs = new List<ApplicationUser>();

			using (var db = SecurityDbContext.Create()) {
				using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db))) {
					var role = (from r in db.Roles
								where r.Name.ToLower() == groupName.ToLower()
								select r).FirstOrDefault();

					if (role != null) {
						usrs = (from ur in role.Users
								join u in db.Users on ur.UserId equals u.Id
								select userManager.FindByName(u.UserName)).Take(2500).ToList();
					}
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

		public static bool IsAdmin {
			get {
				try {
					if (SiteData.IsWebView && HttpContext.Current.User.Identity.IsAuthenticated) {
						return IsUserInRole(SecurityData.CMSGroup_Admins);
					}
					return false;
				} catch (Exception ex) {
					return false;
				}
			}
		}

		public static bool IsEditor {
			get {
				try {
					if (SiteData.IsWebView && HttpContext.Current.User.Identity.IsAuthenticated) {
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
					if (SiteData.IsWebView && HttpContext.Current.User.Identity.IsAuthenticated) {
						return IsUserInRole(SecurityData.CMSGroup_Users);
					}
					return false;
				} catch (Exception ex) {
					return false;
				}
			}
		}

		public static bool IsUserInRole(string groupName) {
			return IsUserInRole(SecurityData.CurrentUserIdentityName, groupName);
		}

		//using (var db = SecurityDbContext.Create()) {
		//using (var roleMgr = new  RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db))) {
		//}
		//}

		public static bool IsUserInRole(string userName, string groupName) {
			if (SiteData.IsWebView && HttpContext.Current.User.Identity.IsAuthenticated) {
				using (var db = SecurityDbContext.Create()) {
					//using (var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db))) {
					using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db))) {
						//var _role = roleManager.FindById(groupName);
						var _user = userManager.FindByName(userName);

						return userManager.IsInRole(_user.Id, groupName);
					}
					//}
				}
			}
			return false;
		}

		public static bool IsSiteEditor {
			get {
				if (SiteData.IsWebView && HttpContext.Current.User.Identity.IsAuthenticated) {
					ExtendedUserData usrEx = SecurityData.CurrentExUser;

					return usrEx.IsEditor && usrEx.MemberSiteIDs.Contains(SiteData.CurrentSiteID);
				} else {
					return false;
				}
			}
		}

		public static bool IsAuthEditor {
			get {
				if (SiteData.IsWebView && HttpContext.Current.User.Identity.IsAuthenticated) {
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
				if (!String.IsNullOrEmpty(SecurityData.CurrentUserIdentityName)) {
					string userName = SecurityData.CurrentUserIdentityName;
					string cacheKey = String.Format("cms_UserProfile_{0}", userName);
					try { currentUser = (UserProfile)HttpContext.Current.Cache[cacheKey]; } catch (Exception ex) { }

					if (currentUser == null) {
						using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
							currentUser = (from u in _db.membership_Users
										   join ud1 in _db.carrot_UserDatas on u.Id equals ud1.UserKey into ud2
										   from ud in ud2.DefaultIfEmpty()
										   where u.UserName.ToLower() == userName.ToLower()
										   select new UserProfile(u, ud)).FirstOrDefault();
						}
						if (currentUser != null) {
							HttpContext.Current.Cache.Insert(cacheKey, currentUser, null, DateTime.Now.AddSeconds(90), Cache.NoSlidingExpiration);
						} else {
							HttpContext.Current.Cache.Remove(cacheKey);
						}
					}
				} else {
					currentUser = new UserProfile();
					currentUser.UserId = Guid.Empty;
					currentUser.UserName = "anonymous-user-" + Guid.Empty.ToString();
				}

				return currentUser;
			}
		}

		public static ExtendedUserData CurrentExUser {
			get {
				ExtendedUserData currentUser = null;
				if (!String.IsNullOrEmpty(SecurityData.CurrentUserIdentityName)) {
					string userName = SecurityData.CurrentUserIdentityName;
					string cacheKey = String.Format("cms_ExUserProfile_{0}", userName);
					try { currentUser = (ExtendedUserData)HttpContext.Current.Cache[cacheKey]; } catch (Exception ex) { }

					if (currentUser == null) {
						using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
							currentUser = (from u in _db.vw_carrot_UserDatas
										   where u.UserName.ToLower() == userName.ToLower()
										   select new ExtendedUserData(u)).FirstOrDefault();
						}
						if (currentUser != null) {
							HttpContext.Current.Cache.Insert(cacheKey, currentUser, null, DateTime.Now.AddSeconds(90), Cache.NoSlidingExpiration);
						} else {
							HttpContext.Current.Cache.Remove(cacheKey);
						}
					}
				} else {
					currentUser = new ExtendedUserData();
					currentUser.UserId = Guid.Empty;
					currentUser.UserName = "anonymous-user-" + Guid.Empty.ToString();
				}

				return currentUser;
			}
		}

		public static ApplicationUser CurrentApplicationUser {
			get {
				ApplicationUser _currentUser = null;
				if (!String.IsNullOrEmpty(SecurityData.CurrentUserIdentityName)) {
					using (var db = SecurityDbContext.Create()) {
						using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db))) {
							_currentUser = userManager.FindByName(SecurityData.CurrentUserIdentityName);
						}
					}
				}
				return _currentUser;
			}
		}

		public static ApplicationUser GetUserByID(string key) {
			using (var db = SecurityDbContext.Create()) {
				using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db))) {
					return userManager.FindById(key);
				}
			}
		}

		public static ApplicationUser GetUserByName(string username) {
			using (var db = SecurityDbContext.Create()) {
				using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db))) {
					return userManager.FindByName(username);
				}
			}
		}

		public static ApplicationUser GetUserByEmail(string email) {
			using (var db = SecurityDbContext.Create()) {
				using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db))) {
					return userManager.FindByEmail(email);
				}
			}
		}

		public static bool AdvancedEditMode {
			get {
				bool _Advanced = false;
				if (SiteData.IsWebView && HttpContext.Current.User.Identity.IsAuthenticated) {
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
				if (SiteData.IsWebView && HttpContext.Current.User.Identity.IsAuthenticated) {
					if (!String.IsNullOrEmpty(HttpContext.Current.User.Identity.Name)) {
						return HttpContext.Current.User.Identity.Name.ToLower();
					}
				}
				return String.Empty;
			}
		}

		public IdentityResult CreateApplicationUser(ApplicationUser user) {
			return CreateApplicationUser(user, SecurityData.GenerateSimplePassword());
		}

		public IdentityResult CreateApplicationUser(ApplicationUser user, string password) {
			IdentityResult result = new IdentityResult();

			if (user != null && !String.IsNullOrEmpty(user.Id)) {
				using (var db = SecurityDbContext.Create()) {
					using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db))) {
						result = userManager.Create(user, password);
						if (result.Succeeded) {
							user = userManager.FindByName(user.UserName);

							ExtendedUserData newusr = new ExtendedUserData();
							newusr.UserKey = user.Id;
							newusr.Id = user.Id;
							newusr.Save();
						}
					}
				}
			}

			return result;
		}

		public IdentityResult ResetPassword(ApplicationUser user, string code, string password) {
			IdentityResult result = new IdentityResult();

			if (user != null && !String.IsNullOrEmpty(user.Id)) {
				using (var db = SecurityDbContext.Create()) {
					var provider = new DpapiDataProtectionProvider("CarrotCake CMS");
					using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db))) {
						userManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("UserToken")) {
							TokenLifespan = TimeSpan.FromDays(7)
						};

						result = userManager.ResetPassword(user.Id, code, password);

						return result;
					}
				}
			}

			return result;
		}

		public bool ResetPassword(string Email) {
			ApplicationUser user = null;
			string code = String.Empty;

			if (!String.IsNullOrEmpty(Email)) {
				using (var db = SecurityDbContext.Create()) {
					var provider = new DpapiDataProtectionProvider("CarrotCake CMS");
					using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db))) {
						userManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("UserToken")) {
							TokenLifespan = TimeSpan.FromDays(7)
						};

						user = userManager.FindByEmail(Email);
						if (user != null) {
							code = userManager.GeneratePasswordResetToken(user.Id);
						}
					}
				}
			}

			if (user != null) {
				string sBody = String.Empty;
				Assembly _assembly = Assembly.GetExecutingAssembly();

				using (StreamReader oTextStream = new StreamReader(_assembly.GetManifestResourceStream("Carrotware.CMS.Core.Security.EmailForgotPassMsg.txt"))) {
					sBody = oTextStream.ReadToEnd();
				}

				string strHTTPHost = "";
				try { strHTTPHost = HttpContext.Current.Request.ServerVariables["HTTP_HOST"] + ""; } catch (Exception ex) { strHTTPHost = ""; }

				string strHTTPProto = "http://";
				try {
					strHTTPProto = HttpContext.Current.Request.ServerVariables["SERVER_PORT_SECURE"] + "";
					if (strHTTPProto == "1") {
						strHTTPProto = "https://";
					} else {
						strHTTPProto = "http://";
					}
				} catch (Exception ex) { }

				strHTTPHost = strHTTPProto + strHTTPHost.ToLower();

				string adminFolder = SiteData.AdminFolderPath;
				if (adminFolder.StartsWith("/")) {
					adminFolder = adminFolder.Substring(1);
				}
				if (adminFolder.EndsWith("/")) {
					adminFolder = adminFolder.Substring(0, adminFolder.Length - 1);
				}

				var callbackUrl = String.Format("{0}/{1}/ResetPassword?userId={2}&code={3}", strHTTPHost, adminFolder, user.Id, HttpUtility.UrlEncode(code));

				sBody = sBody.Replace("{%%UserName%%}", user.UserName);
				sBody = sBody.Replace("{%%SiteURL%%}", strHTTPHost);
				sBody = sBody.Replace("{%%ResetURL%%}", callbackUrl);
				sBody = sBody.Replace("{%%Version%%}", CurrentDLLVersion);

				if (SiteData.CurretSiteExists) {
					sBody = sBody.Replace("{%%Time%%}", SiteData.CurrentSite.Now.ToString());
				} else {
					sBody = sBody.Replace("{%%Time%%}", DateTime.Now.ToString());
				}

				EmailHelper.SendMail(null, user.Email, "Reset Password", sBody, false);

				return true;
			} else {
				return false;
			}
		}

		public static void RemoveUserFromRole(string userName, string roleName) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				membership_UserRole usrRole = (from r in _db.membership_Roles
											   join ur in _db.membership_UserRoles on r.Id equals ur.RoleId
											   join u in _db.membership_Users on ur.UserId equals u.Id
											   where r.Name.ToLower() == roleName.ToLower()
													   && u.UserName.ToLower() == userName.ToLower()
											   select ur).FirstOrDefault();

				if (usrRole != null) {
					_db.membership_UserRoles.DeleteOnSubmit(usrRole);
					_db.SubmitChanges();
				}
			}
		}

		public static void AddUserToRole(string userName, string roleName) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				membership_Role role = (from r in _db.membership_Roles
										where r.Name.ToLower() == roleName.ToLower()
										select r).FirstOrDefault();

				membership_User user = (from u in _db.membership_Users
										where u.UserName.ToLower() == userName.ToLower()
										select u).FirstOrDefault();

				membership_UserRole usrRole = (from r in _db.membership_Roles
											   join ur in _db.membership_UserRoles on r.Id equals ur.RoleId
											   join u in _db.membership_Users on ur.UserId equals u.Id
											   where r.Name.ToLower() == roleName.ToLower()
													   && u.UserName.ToLower() == userName.ToLower()
											   select ur).FirstOrDefault();

				if (usrRole == null && role != null && user != null) {
					usrRole = new membership_UserRole();
					usrRole.UserId = user.Id;
					usrRole.RoleId = role.Id;
					_db.membership_UserRoles.InsertOnSubmit(usrRole);
					_db.SubmitChanges();
				}
			}
		}

		public static void AddUserToRole(Guid UserId, string roleName) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				membership_Role role = (from r in _db.membership_Roles
										where r.Name.ToLower() == roleName.ToLower()
										select r).FirstOrDefault();

				membership_User user = (from u in _db.membership_Users
										join ud in _db.carrot_UserDatas on u.Id equals ud.UserKey
										where ud.UserId == UserId
										select u).FirstOrDefault();

				membership_UserRole usrRole = (from r in _db.membership_Roles
											   join ur in _db.membership_UserRoles on r.Id equals ur.RoleId
											   join u in _db.membership_Users on ur.UserId equals u.Id
											   join ud in _db.carrot_UserDatas on u.Id equals ud.UserKey
											   where r.Name.ToLower() == roleName.ToLower()
													   && ud.UserId == UserId
											   select ur).FirstOrDefault();

				if (usrRole == null && role != null && user != null) {
					usrRole = new membership_UserRole();
					usrRole.UserId = user.Id;
					usrRole.RoleId = role.Id;
					_db.membership_UserRoles.InsertOnSubmit(usrRole);
					_db.SubmitChanges();
				}
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
			String generatedPassword = "";

			for (int i = 0; i < length; i++) {
				double rand = r.NextDouble();
				if (i == 0) {
					//First character is an upper case alphabet
					generatedPassword += alphaCaps.ToCharArray()[(int)Math.Floor(rand * alphaCaps.Length)];
				} else if (i == length - 2) {
					generatedPassword += specialChars.ToCharArray()[(int)Math.Floor(rand * specialChars.Length)];
				} else {
					generatedPassword += allChars.ToCharArray()[(int)Math.Floor(rand * allChars.Length)];
				}
			}

			return generatedPassword;
		}
	}
}