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

	public class SiteModel {

		public SiteModel() {
			this.Site = new SiteData();
			this.Users = new List<UserModel>();
		}

		public SiteModel(Guid siteId) {
			this.Site = SiteData.GetSiteByID(siteId);

			LoadUsers();
		}

		public void LoadUsers() {
			this.Users = this.Site.GetMappedUsers().OrderBy(x => x.UserName).Select(x => new UserModel(x)).ToList();
		}

		public SiteData Site { get; set; }
		public List<UserModel> Users { get; set; }

		[Display(Name = "New User")]
		public string NewUserId { get; set; }

		[Display(Name = "Add to Editor Role ")]
		public bool NewUserAsEditor { get; set; }
	}
}