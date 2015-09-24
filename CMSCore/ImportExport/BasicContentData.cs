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

	public class BasicContentData {

		public BasicContentData() { }

		public int NavOrder { get; set; }
		public Guid? Parent_ContentID { get; set; }
		public Guid Root_ContentID { get; set; }
		public Guid SiteID { get; set; }
		public string FileName { get; set; }
		public string TemplateFile { get; set; }
		public bool PageActive { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime GoLiveDate { get; set; }
		public DateTime RetireDate { get; set; }
		public ContentPageType.PageType ContentType { get; set; }

		internal BasicContentData(vw_carrot_Content c) {
			if (c != null) {
				SiteData site = SiteData.GetSiteFromCache(c.SiteID);

				this.SiteID = c.SiteID;
				this.Root_ContentID = c.Root_ContentID;
				this.PageActive = c.PageActive;
				this.Parent_ContentID = c.Parent_ContentID;
				this.FileName = c.FileName;
				this.TemplateFile = c.TemplateFile;
				this.NavOrder = c.NavOrder;

				this.ContentType = ContentPageType.GetTypeByID(c.ContentTypeID);
				this.CreateDate = site.ConvertUTCToSiteTime(c.CreateDate);
				this.GoLiveDate = site.ConvertUTCToSiteTime(c.GoLiveDate);
				this.RetireDate = site.ConvertUTCToSiteTime(c.RetireDate);
			}
		}

		public ContentPage GetContentPage() {
			ContentPage cp = null;
			if (SiteData.IsPageSampler) {
				cp = ContentPageHelper.GetSamplerView();
			} else {
				using (ContentPageHelper cph = new ContentPageHelper()) {
					cp = cph.FindContentByID(this.SiteID, this.Root_ContentID);
				}
			}
			return cp;
		}

		public static BasicContentData CreateBasicContentDataFromSiteNav(SiteNav c) {
			BasicContentData sn = null;
			if (c != null) {
				sn = new BasicContentData();
				sn.Root_ContentID = c.Root_ContentID;
				sn.Parent_ContentID = c.Parent_ContentID;
				sn.FileName = c.FileName;
				sn.TemplateFile = c.TemplateFile;
				sn.SiteID = c.SiteID;
				sn.PageActive = c.PageActive;
				sn.CreateDate = c.CreateDate;
				sn.GoLiveDate = c.GoLiveDate;
				sn.RetireDate = c.RetireDate;
				sn.ContentType = c.ContentType;
				sn.NavOrder = c.NavOrder;
			}

			return sn;
		}

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is BasicContentData) {
				BasicContentData p = (BasicContentData)obj;
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