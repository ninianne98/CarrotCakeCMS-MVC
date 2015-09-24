using Carrotware.CMS.Data;
using System;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public class SiteMapOrder {

		public SiteMapOrder() { }

		public int NavOrder { get; set; }
		public Guid? Parent_ContentID { get; set; }
		public Guid Root_ContentID { get; set; }
		public Guid SiteID { get; set; }
		public string NavMenuText { get; set; }
		public string FileName { get; set; }
		public bool PageActive { get; set; }
		public bool ShowInSiteNav { get; set; }
		public int NavLevel { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime GoLiveDate { get; set; }
		public DateTime RetireDate { get; set; }
		public ContentPageType.PageType ContentType { get; set; }

		internal SiteMapOrder(vw_carrot_Content c) {
			if (c != null) {
				SiteData site = SiteData.GetSiteFromCache(c.SiteID);

				this.SiteID = c.SiteID;
				this.Root_ContentID = c.Root_ContentID;
				this.PageActive = c.PageActive;
				this.ShowInSiteNav = c.ShowInSiteNav;
				this.Parent_ContentID = c.Parent_ContentID;
				this.NavMenuText = c.NavMenuText;
				this.FileName = c.FileName;
				this.NavOrder = c.NavOrder;

				if (this.Parent_ContentID.HasValue) {
					this.NavLevel = 0;
				} else {
					this.NavLevel = 10;
				}

				this.ContentType = ContentPageType.GetTypeByID(c.ContentTypeID);
				this.CreateDate = site.ConvertUTCToSiteTime(c.CreateDate);
				this.GoLiveDate = site.ConvertUTCToSiteTime(c.GoLiveDate);
				this.RetireDate = site.ConvertUTCToSiteTime(c.RetireDate);
			}
		}

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is SiteMapOrder) {
				SiteMapOrder p = (SiteMapOrder)obj;
				return (this.Root_ContentID == p.Root_ContentID);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return Root_ContentID.GetHashCode();
		}
	}
}