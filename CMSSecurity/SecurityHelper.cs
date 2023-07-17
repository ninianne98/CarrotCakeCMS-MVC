using Carrotware.CMS.Security.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Threading.Tasks;
using System.Web;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Security {

	public class SecurityHelper : IDisposable {
		private SecurityDbContext _db = null;
		private ApplicationUserManager _userManager = null;
		private ApplicationRoleManager _roleManager = null;
		private IAuthenticationManager _authManager = null;
		private ApplicationSignInManager _signInManager = null;

		public SecurityHelper() {
			if (_db == null) {
				_db = SecurityDbContext.Create();
			}

			//var provider = new DpapiDataProtectionProvider("CarrotCake CMS");

			//this.UserToken = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("UserToken")) {
			//	TokenLifespan = TimeSpan.FromDays(7)
			//};
		}

		public SecurityHelper(SecurityDbContext db)
			: this() {
			if (_db == null) {
				_db = db;
			}
		}

		public SecurityDbContext DataContext {
			get {
				return _db;
			}
		}

		public ApplicationUserManager UserManager {
			get {
				if (_userManager == null) {
					_userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(_db));
					//_userManager.UserTokenProvider = this.UserToken;
					//_userManager.Config();
				}
				return _userManager;
			}
		}

		public ApplicationRoleManager RoleManager {
			get {
				if (_roleManager == null) {
					_roleManager = new ApplicationRoleManager(new RoleStore<IdentityRole>(_db));
				}
				return _roleManager;
			}
		}

		public IAuthenticationManager AuthenticationManager {
			get {
				if (_authManager == null) {
					_authManager = HttpContext.Current.GetOwinContext().Authentication;
				}
				return _authManager;
			}
		}

		public ApplicationSignInManager SignInManager {
			get {
				if (_signInManager == null) {
					_signInManager = new ApplicationSignInManager(this.UserManager, this.AuthenticationManager);
				}
				return _signInManager;
			}
		}

		public void LogoutSession() {
			this.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
			HttpContext.Current.Session.Clear();
		}

		public async Task<ApplicationUser> FindByNameAsync(string userName) {
			return await this.UserManager.FindByNameAsync(userName);
		}

		public async Task<bool> SimpleLogInAsync(string userName, string password, bool rememberMe) {
			var user = await this.UserManager.FindByNameAsync(userName);

			if (user == null) {
				return false;
			}

			var result = await this.SignInManager.PasswordSignInAsync(userName, password, rememberMe, shouldLockout: true);

			return result == Microsoft.AspNet.Identity.Owin.SignInStatus.Success;
		}

		public void Dispose() {
			if (_userManager != null) {
				_userManager.Dispose();
			}
			if (_roleManager != null) {
				_roleManager.Dispose();
			}
			if (_signInManager != null) {
				_signInManager.Dispose();
			}
			if (_authManager != null && _authManager is IDisposable) {
				((IDisposable)_authManager).Dispose();
			}

			if (_db != null) {
				_db.Dispose();
			}
		}
	}
}