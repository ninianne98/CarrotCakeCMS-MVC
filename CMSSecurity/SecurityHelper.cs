using Carrotware.CMS.Security.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using System;
using System.Collections.Generic;

using System.Linq;

namespace Carrotware.CMS.Security {

	public class SecurityHelper : IDisposable {
		private SecurityDbContext _db = null;

		private ApplicationUserManager _userManager;
		private ApplicationRoleManager _roleManager;

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
					//_userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_db));
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
					//_roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_db));
					_roleManager = new ApplicationRoleManager(new RoleStore<IdentityRole>(_db));
				}
				return _roleManager;
			}
		}

		public DataProtectorTokenProvider<ApplicationUser> UserToken { get; set; }

		public void Dispose() {
			if (this.UserManager != null) {
				this.UserManager.Dispose();
			}
			if (this.RoleManager != null) {
				this.RoleManager.Dispose();
			}
			if (_db != null) {
				_db.Dispose();
			}
		}
	}
}