using Carrotware.CMS.Data;
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

	public class UserProfile {

		public UserProfile() {
			this.Id = string.Empty;
			this.UserId = Guid.Empty;
			this.UserName = string.Empty;
		}

		internal UserProfile(membership_User mu, carrot_UserData ud) {
			if (mu != null) {
				this.Id = mu.Id;
				this.Email = mu.Email;
				this.EmailConfirmed = mu.EmailConfirmed;
				this.PasswordHash = mu.PasswordHash;
				this.SecurityStamp = mu.SecurityStamp;
				this.PhoneNumber = mu.PhoneNumber;
				this.PhoneNumberConfirmed = mu.PhoneNumberConfirmed;
				this.TwoFactorEnabled = mu.TwoFactorEnabled;
				this.LockoutEndDateUtc = mu.LockoutEndDateUtc;
				this.LockoutEnabled = mu.LockoutEnabled;
				this.AccessFailedCount = mu.AccessFailedCount;
				this.UserName = mu.UserName;

				this.UserId = Guid.Empty;
				this.UserKey = mu.Id;
			}

			if (ud != null) {
				this.UserId = ud.UserId;
				this.UserNickName = ud.UserNickName;
				this.FirstName = ud.FirstName;
				this.LastName = ud.LastName;
				this.UserBio = ud.UserBio;
				this.UserKey = ud.UserKey;
			} else {
				this.UserId = Guid.Empty;
				this.UserKey = string.Empty;
			}
		}

		public string Id { get; set; }
		public string Email { get; set; }
		public bool EmailConfirmed { get; set; }
		public string PasswordHash { get; set; }
		public string SecurityStamp { get; set; }
		public string PhoneNumber { get; set; }
		public bool PhoneNumberConfirmed { get; set; }
		public bool TwoFactorEnabled { get; set; }
		public DateTime? LockoutEndDateUtc { get; set; }
		public bool LockoutEnabled { get; set; }
		public int AccessFailedCount { get; set; }
		public string UserName { get; set; }
		public Guid UserId { get; set; }
		public string UserNickName { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string UserBio { get; set; }
		public string UserKey { get; set; }
	}
}