using System;
using System.Collections.Generic;

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

	public class ContentPageExport {

		public ContentPageExport() {
			this.CarrotCakeVersion = SiteData.CarrotCakeCMSVersion;
			this.ExportDate = DateTime.UtcNow;

			this.ThePage = new ContentPage();
			this.ThePageWidgets = new List<Widget>();
		}

		public ContentPageExport(Guid siteID, Guid rootContentID) {
			ContentPage cp = null;

			using (ContentPageHelper cph = new ContentPageHelper()) {
				cp = cph.FindContentByID(siteID, rootContentID);
			}

			List<Widget> widgets = cp.GetWidgetList();

			SetVals(cp, widgets);
		}

		public ContentPageExport(ContentPage cp, List<Widget> widgets) {
			SetVals(cp, widgets);
		}

		private void SetVals(ContentPage cp, List<Widget> widgets) {
			this.CarrotCakeVersion = SiteData.CarrotCakeCMSVersion;
			this.ExportDate = DateTime.UtcNow;
			Guid siteID = cp.SiteID;

			this.NewRootContentID = Guid.NewGuid();

			cp.LoadAttributes();

			this.ThePage = cp;
			this.ThePageWidgets = widgets;

			if (this.ThePage == null) {
				this.ThePage = new ContentPage();
				this.ThePage.Root_ContentID = Guid.NewGuid();
				this.ThePage.ContentID = ThePage.Root_ContentID;
			}
			if (this.ThePageWidgets == null) {
				this.ThePageWidgets = new List<Widget>();
			}

			this.OriginalRootContentID = ThePage.Root_ContentID;
			this.OriginalSiteID = ThePage.SiteID;
			this.OriginalParentContentID = Guid.Empty;
			this.ParentFileName = String.Empty;

			if (this.ThePage.Parent_ContentID != null) {
				ContentPage parent = new ContentPage();
				using (ContentPageHelper cph = new ContentPageHelper()) {
					parent = cph.FindContentByID(siteID, ThePage.Parent_ContentID.Value);
				}
				this.ParentFileName = parent.FileName;
				this.OriginalParentContentID = parent.Root_ContentID;
			}

			this.ThePage.Root_ContentID = this.NewRootContentID;
			this.ThePage.ContentID = this.NewRootContentID;

			foreach (var w in this.ThePageWidgets) {
				w.Root_ContentID = this.NewRootContentID;
				w.Root_WidgetID = Guid.NewGuid();
				w.WidgetDataID = Guid.NewGuid();
			}

			Guid userID1 = Guid.Empty;

			if (!cp.EditUserId.HasValue) {
				userID1 = cp.CreateUserId;
			} else {
				userID1 = cp.EditUserId.Value;
			}

			ExtendedUserData u1 = new ExtendedUserData(userID1);
			this.TheUser = new SiteExportUser(u1);

			Guid userID2 = Guid.Empty;

			if (cp.CreditUserId.HasValue) {
				userID2 = cp.CreditUserId.Value;
			}

			ExtendedUserData u2 = new ExtendedUserData(userID2);
			if (u2 != null) {
				this.CreditUser = new SiteExportUser(u2);
			} else {
				this.CreditUser = null;
			}
		}

		public string CarrotCakeVersion { get; set; }

		public DateTime ExportDate { get; set; }

		public Guid NewRootContentID { get; set; }

		public Guid OriginalRootContentID { get; set; }

		public Guid OriginalSiteID { get; set; }

		public Guid OriginalParentContentID { get; set; }

		public string ParentFileName { get; set; }

		public ContentPage ThePage { get; set; }

		public List<Widget> ThePageWidgets { get; set; }

		public SiteExportUser TheUser { get; set; }

		public SiteExportUser CreditUser { get; set; }
	}
}