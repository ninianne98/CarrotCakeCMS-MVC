using Carrotware.CMS.Security.Models;
using Microsoft.AspNet.Identity;

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

	public class NewUser {

		public NewUser() {
			this.ExtendedUserData = null;

			this.IdentityResult = IdentityResult.Failed();
		}

		public NewUser(ExtendedUserData exUser, ApplicationUser user, IdentityResult result) {
			this.ExtendedUserData = exUser;
			this.User = user;
			this.IdentityResult = result;
		}

		public ApplicationUser User { get; set; } = new ApplicationUser();

		public ExtendedUserData ExtendedUserData { get; set; } = new ExtendedUserData();

		public IdentityResult IdentityResult { get; set; }
	}
}