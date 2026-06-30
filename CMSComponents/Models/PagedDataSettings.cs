using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
using Carrotware.Web.UI.Components;
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

namespace Carrotware.CMS.UI.Components {

	public class PagedDataModel {

		public PagedDataModel() { }

		public PagedDataModel(PagedDataSettings data) {
			data.LoadData();

			this.SiteID = data.SiteID;
			this.RootContentID = data.RootContentID;
			this.WidgetClientID = data.WidgetClientID;

			this.ShowPager = data.ShowPager;
			this.PageSize = data.PageSize;
			this.MaxPage = data.MaxPage;
			this.ContentType = data.ContentType;

			this.CSSPageFooter = data.CSSPageFooter;
			this.CSSPageListing = data.CSSPageListing;
			this.CSSSelectedPage = data.CSSSelectedPage;

			this.SelectedCategories = data.SelectedCategories;

			if (this.SelectedCategories.Any()) {
				this.ContentType = PagedDataSummary.SummaryContentType.SpecifiedCategories;

				this.SelectedCategorySlugs = (from c in SiteData.CurrentSite.GetCategoryList()
											  orderby c.CategoryText
											  where c.SiteID == SiteData.CurrentSiteID
												 && this.SelectedCategories.Contains(c.ContentCategoryID)
											  select c.CategorySlug).Distinct().ToList();
			}

			using (var navHelper = SiteNavFactory.GetSiteNavHelper()) {
				this.SiteNav = navHelper.GetPageNavigation(this.SiteID, this.RootContentID);
			}
		}

		public int PageSize { get; set; }
		public int MaxPage { get; set; }
		public bool ShowPager { get; set; }

		public PagedDataSummary.SummaryContentType ContentType { get; set; } = PagedDataSummary.SummaryContentType.Blog;

		public List<Guid> SelectedCategories { get; set; } = new List<Guid>();

		public List<string> SelectedCategorySlugs { get; set; } = new List<string>();

		public string CSSSelectedPage { get; set; }
		public string CSSPageListing { get; set; }
		public string CSSPageFooter { get; set; }

		public Guid SiteID { get; set; }
		public Guid RootContentID { get; set; } = Guid.Empty;
		public SiteNav SiteNav { get; set; } = new SiteNav();
		public string WidgetClientID { get; set; }
	}

	//========================
	public class PagedDataSettings : WidgetActionSettingModel {

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
				var foundVal = this.GetValue(x => x.ContentType, this.ContentType);
				this.SetEnumValue(x => x.ContentType, foundVal);
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

			this.SelectedCategories = new List<Guid>();

			try {
				List<string> lstCat = this.GetParmValueList(nameof(this.SelectedCategories));

				foreach (string catV in lstCat) {
					if (!string.IsNullOrEmpty(catV)) {
						this.SelectedCategories.Add(new Guid(catV));
					}
				}
			} catch (Exception ex) { }

			if (this.SelectedCategories.Any()) {
				this.ContentType = PagedDataSummary.SummaryContentType.SpecifiedCategories;
			}
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

		[Widget(WidgetAttribute.FieldMode.DropDownList, nameof(lstContentType))]
		public PagedDataSummary.SummaryContentType ContentType { get; set; } = PagedDataSummary.SummaryContentType.Blog;

		[Widget(WidgetAttribute.FieldMode.CheckBoxList, nameof(lstCategories))]
		public List<Guid> SelectedCategories { get; set; } = new List<Guid>();

		[Widget(WidgetAttribute.FieldMode.DictionaryList)]
		public Dictionary<string, string> lstContentType {
			get {
				var _dict = typeof(PagedDataSummary.SummaryContentType).ToDescriptionDictionary()
						.Where(x => x.Key != PagedDataSummary.SummaryContentType.Unknown.ToString()
									&& x.Key != PagedDataSummary.SummaryContentType.SiteSearch.ToString())
						.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

				return _dict;
			}
		}

		[Widget(WidgetAttribute.FieldMode.DictionaryList)]
		public Dictionary<string, string> lstCategories {
			get {
				Dictionary<string, string> _dict = (from c in SiteData.CurrentSite.GetCategoryList()
													orderby c.CategoryText
													where c.SiteID == SiteData.CurrentSiteID
													select c).ToList()
														.ToDictionary(k => k.ContentCategoryID.ToString(),
																	v => v.CategoryText + " (" + v.CategorySlug + ")");

				return _dict;
			}
		}

		public override bool EnableEdit {
			get {
				return true;
			}
		}
	}
}