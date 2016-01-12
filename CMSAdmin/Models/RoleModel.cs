using Carrotware.CMS.Core;
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

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class RoleModel {

		public RoleModel() {
			this.Role = new UserRole();
			this.Role.RoleId = Guid.Empty.ToString().ToLowerInvariant();
			this.Users = new List<UserModel>();
		}

		public RoleModel(string RoleId) {
			this.Role = SecurityData.FindRoleByID(RoleId);
			LoadUsers();
		}

		public void LoadUsers() {
			this.Users = this.Role.GetMembers().OrderBy(x => x.UserName).Select(x => new UserModel(x)).ToList();
		}

		public UserRole Role { get; set; }
		public List<UserModel> Users { get; set; }

		[Display(Name = "New User")]
		public string NewUserId { get; set; }

		public bool CanEditRoleName {
			get {
				return !(SecurityData.CMSGroup_Admins.ToLowerInvariant() == this.Role.LoweredRoleName
							|| SecurityData.CMSGroup_Editors.ToLowerInvariant() == this.Role.LoweredRoleName
							|| SecurityData.CMSGroup_Users.ToLowerInvariant() == this.Role.LoweredRoleName);
			}
		}
	}
}