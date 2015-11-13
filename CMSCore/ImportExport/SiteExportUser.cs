using System;

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

	public class SiteExportUser {

		public SiteExportUser() { }

		public SiteExportUser(ExtendedUserData user) {
			if (user != null) {
				this.ExportUserID = user.UserId;
				this.Email = user.Email;
				this.Login = user.UserName;
				this.FirstName = user.FirstName;
				this.LastName = user.LastName;
				this.UserNickname = user.UserNickName;
			}
		}

		public Guid ExportUserID { get; set; }
		public Guid ImportUserID { get; set; }

		public string Login { get; set; }
		public string Email { get; set; }

		public string FirstName { get; set; }
		public string LastName { get; set; }

		public string UserNickname { get; set; }
	}
}