using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
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

namespace Carrotware.CMS.UI.Components {

	public class PagedCommentModel {

		public PagedCommentModel() { }

		public PagedCommentModel(PagedCommentSettings data) {
			data.LoadData();

			this.SiteID = data.SiteID;
			this.RootContentID = data.RootContentID;
			this.WidgetClientID = data.WidgetClientID;

			this.ShowPager = data.ShowPager;
			this.PageSize = data.PageSize;
			this.MaxPage = data.MaxPage;

			this.CSSPageFooter = data.CSSPageFooter;
			this.CSSPageListing = data.CSSPageListing;
			this.CSSSelectedPage = data.CSSSelectedPage;

			using (var navHelper = SiteNavFactory.GetSiteNavHelper()) {
				this.SiteNav = navHelper.GetPageNavigation(this.SiteID, this.RootContentID);
			}
		}

		public int PageSize { get; set; } = 10;
		public int MaxPage { get; set; } = 50;
		public bool ShowPager { get; set; } = true;

		public string CSSSelectedPage { get; set; }
		public string CSSPageListing { get; set; }
		public string CSSPageFooter { get; set; }

		public Guid SiteID { get; set; }
		public Guid RootContentID { get; set; } = Guid.Empty;
		public SiteNav SiteNav { get; set; } = new SiteNav();
		public string WidgetClientID { get; set; }
	}

	//========================
	public class PagedCommentSettings : WidgetActionSettingModel {

		public override void LoadData() {
			base.LoadData();

			try {
				var foundVal = this.GetValue(x => x.PageSize, this.PageSize);
				this.SetIntValue(x => x.PageSize, foundVal);
			} catch (Exception ex) { }
			try {
				var foundVal = this.GetValue(x => x.MaxPage, this.MaxPage);
				this.SetIntValue(x => x.MaxPage, foundVal);
			} catch (Exception ex) { }
			try {
				var foundVal = this.GetValue(x => x.ShowPager, this.ShowPager);
				this.SetBoolValue(x => x.ShowPager, foundVal);
			} catch (Exception ex) { }

			try {
				var foundVal = this.GetValue(x => x.CSSSelectedPage, this.CSSSelectedPage);
				this.SetStringValue(x => x.CSSSelectedPage, foundVal);
			} catch (Exception ex) { }
			try {
				var foundVal = this.GetValue(x => x.CSSPageListing, this.CSSPageListing);
				this.SetStringValue(x => x.CSSPageListing, foundVal);
			} catch (Exception ex) { }
			try {
				var foundVal = this.GetValue(x => x.CSSPageFooter, this.CSSPageFooter);
				this.SetStringValue(x => x.CSSPageFooter, foundVal);
			} catch (Exception ex) { }
		}

		[Widget]
		public int PageSize { get; set; } = 10;

		[Widget]
		public int MaxPage { get; set; } = -1;

		[Widget]
		public bool ShowPager { get; set; } = true;

		[Widget]
		public string CSSSelectedPage { get; set; } = "SelectedCurrentPager";

		[Widget]
		public string CSSPageListing { get; set; } = string.Empty;

		[Widget]
		public string CSSPageFooter { get; set; } = string.Empty;

		public override bool EnableEdit {
			get {
				return true;
			}
		}
	}
}