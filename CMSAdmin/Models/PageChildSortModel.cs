using Carrotware.CMS.Core;
using System;
using System.Collections.Generic;
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

	public class PageChildSortModel {

		public PageChildSortModel() {
			this.SortOptions = new Dictionary<string, string>();
			this.SelectedSort = string.Empty;
			this.SortChild = false;

			this.SortOptions.Add("alpha", "Alphabetical (asc)");
			this.SortOptions.Add("datecreated", "Date Created (asc)");
			this.SortOptions.Add("dateupdated", "Date Last Updated (asc)");
			this.SortOptions.Add("alpha2", "Alphabetical (desc)");
			this.SortOptions.Add("datecreated2", "Date Created (desc)");
			this.SortOptions.Add("dateupdated2", "Date Last Updated (desc)");
		}

		public PageChildSortModel(Guid rootContentID)
			: this() {
			this.Root_ContentID = rootContentID;

			SortChildren();
		}

		public void SortChildren() {
			List<SiteNav> lstNav = null;
			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				lstNav = navHelper.GetChildNavigation(SiteData.CurrentSiteID, this.Root_ContentID, !SecurityData.IsAuthEditor);
			}

			if (!string.IsNullOrEmpty(this.SelectedSort)) {
				switch (this.SelectedSort) {
					case "alpha":
						lstNav = lstNav.OrderBy(x => x.NavMenuText).ToList();
						break;

					case "datecreated":
						lstNav = lstNav.OrderBy(x => x.CreateDate).ToList();
						break;

					case "dateupdated":
						lstNav = lstNav.OrderBy(x => x.EditDate).ToList();
						break;

					case "alpha2":
						lstNav = lstNav.OrderByDescending(x => x.NavMenuText).ToList();
						break;

					case "datecreated2":
						lstNav = lstNav.OrderByDescending(x => x.CreateDate).ToList();
						break;

					case "dateupdated2":
						lstNav = lstNav.OrderByDescending(x => x.EditDate).ToList();
						break;

					default:
						lstNav = lstNav.OrderBy(x => x.NavOrder).ToList();
						break;
				}
			}

			lstNav.ToList().ForEach(q => CMSConfigHelper.FixNavLinkText(q));

			this.Pages = lstNav;
		}

		public bool SortChild { get; set; }

		public Guid Root_ContentID { get; set; }

		public List<SiteNav> Pages { get; set; }

		public Dictionary<string, string> SortOptions { get; set; }

		public string SelectedSort { get; set; }

		public string Sort { get; set; }
	}
}