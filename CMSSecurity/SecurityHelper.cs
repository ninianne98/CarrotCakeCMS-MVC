using Carrotware.CMS.Security.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using System;
using System.Web;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Security {

	public class SecurityHelper : IDisposable {
		private SecurityDbContext _db = null;
		private ApplicationUserManager _userManager;
		private ApplicationRoleManager _roleManager;
		private IAuthenticationManager _authnManager;
		private ApplicationSignInManager _signInManager;

		public SecurityHelper() {
			_db = SecurityDbContext.Create();

			var provider = new DpapiDataProtectionProvider("CarrotCake CMS");

			this.UserToken = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("UserToken")) {
				TokenLifespan = TimeSpan.FromDays(7)
			};
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
					_userManager.UserTokenProvider = this.UserToken;
					_userManager = ApplicationUserManager.Configure(_userManager);
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
				if (_authnManager == null) {
					_authnManager = HttpContext.Current.GetOwinContext().Authentication;
				}
				return _authnManager;
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

		public DataProtectorTokenProvider<ApplicationUser> UserToken { get; set; }

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
			if (_authnManager != null && _authnManager is IDisposable) {
				((IDisposable)_authnManager).Dispose();
			}

			if (_db != null) {
				_db.Dispose();
			}
		}
	}
}