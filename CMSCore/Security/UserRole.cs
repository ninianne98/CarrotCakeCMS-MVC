using Carrotware.CMS.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

	public class UserRole {

		public UserRole() {
			this.RoleId = Guid.Empty.ToString();
		}

		public UserRole(string roleName) {
			this.RoleName = roleName;
			this.RoleId = Guid.Empty.ToString();
		}

		public UserRole(string roleName, string roleID) {
			this.RoleName = roleName;
			this.RoleId = roleID;
		}

		internal UserRole(membership_Role role) {
			if (role != null) {
				this.RoleId = role.Id;
				this.RoleName = role.Name;
			}
		}

		[Display(Name = "ID")]
		[Required]
		public string RoleId { get; set; }

		[Display(Name = "Name")]
		[Required]
		public string RoleName { get; set; }

		[Display(Name = "Lowercase Name")]
		public string LoweredRoleName { get { return (this.RoleName ?? String.Empty).ToLowerInvariant(); } }

		public void Save() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				membership_Role role = (from r in _db.membership_Roles
										where r.Name.ToLowerInvariant() == this.RoleName.ToLowerInvariant()
											|| r.Id == this.RoleId
										select r).FirstOrDefault();

				if (role == null) {
					role = new membership_Role();
					role.Id = Guid.NewGuid().ToString().ToLowerInvariant();
					_db.membership_Roles.InsertOnSubmit(role);
				}

				role.Name = this.RoleName.Trim();

				_db.SubmitChanges();

				this.RoleName = role.Name;
				this.RoleId = role.Id;
			}
		}

		public List<ExtendedUserData> GetMembers() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				return (from ur in _db.membership_UserRoles
						join r in _db.membership_Roles on ur.RoleId equals r.Id
						join ud in _db.vw_carrot_UserDatas on ur.UserId equals ud.UserKey
						where r.Id == this.RoleId
						orderby ud.UserName
						select new ExtendedUserData(ud)).ToList();
			}
		}
	}
}