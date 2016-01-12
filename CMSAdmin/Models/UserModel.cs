using Carrotware.CMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

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

	public class UserModel {

		public UserModel() { }

		public UserModel(Guid UserId) {
			this.User = new ExtendedUserData(UserId);
		}

		public UserModel(ExtendedUserData user) {
			this.User = user;
		}

		public ExtendedUserData User { get; set; }

		public bool Selected { get; set; }

		private List<SelectListItem> _sites = null;

		public List<SelectListItem> SiteOptions {
			get {
				if (_sites == null) {
					_sites = (from l in this.AllSites
							  select new SelectListItem {
								  Text = l.SiteName,
								  Selected = this.User.GetSiteList().Where(x => x.SiteID == l.SiteID).Any(),
								  Value = l.SiteID.ToString().ToLowerInvariant()
							  }).ToList();
				}

				return _sites;
			}
		}

		private List<SelectListItem> _roles = null;

		public List<SelectListItem> RoleOptions {
			get {
				if (_roles == null) {
					_roles = (from l in SecurityData.GetRoleList()
							  select new SelectListItem {
								  Text = l.RoleName,
								  Selected = this.User.GetRoles().Where(x => x.RoleId == l.RoleId).Any(),
								  Value = l.RoleId.ToLowerInvariant()
							  }).ToList();
				}

				return _roles;
			}
		}

		private List<SiteData> _allSites = null;

		public List<SiteData> AllSites {
			get {
				if (_allSites == null) {
					_allSites = SiteData.GetSiteList();
				}
				return _allSites;
			}
		}
	}
}