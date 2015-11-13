using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

	public class PagedDataSummary : PagedData<SiteNav>, IPagedContent {

		public PagedDataSummary() {
			this.SelectedCategorySlugs = new List<string>();
			this.InitOrderBy(x => x.GoLiveDate, false);
		}

		public enum SummaryContentType {
			Unknown,
			Blog,
			ContentPage,
			ChildContentPage,
			SpecifiedCategories,
			SiteSearch
		}

		public bool IgnoreSitePath { get; set; }

		public List<string> SelectedCategorySlugs { get; set; }

		public SummaryContentType ContentType { get; set; }

		public string GetSearchTerm() {
			string sSearchTerm = String.Empty;

			if (HttpContext.Current.Request.QueryString[SiteData.SearchQueryParameter] != null) {
				sSearchTerm = HttpContext.Current.Request.QueryString[SiteData.SearchQueryParameter].ToString();
			}

			return sSearchTerm;
		}

		public string GetUrl(int pageNbr) {
			if (this.ContentType == SummaryContentType.SiteSearch) {
				string sSearchTerm = GetSearchTerm();
				return String.Format("{0}?{1}={2}&{3}={4}", SiteData.CurrentScriptName, SiteData.SearchQueryParameter, HttpUtility.UrlEncode(sSearchTerm), this.PageNumbParm, pageNbr);
			}

			return String.Format("{0}?{1}={2}", SiteData.CurrentScriptName, this.PageNumbParm, pageNbr);
		}

		public void FetchData() {
			base.ReadPageNbr();

			string sPagePath = SiteData.CurrentScriptName;

			if (String.IsNullOrEmpty(this.OrderBy)) {
				this.InitOrderBy(x => x.GoLiveDate, false);
			}

			List<SiteNav> lstContents = new List<SiteNav>();

			string sSearchTerm = String.Empty;

			ContentPageType.PageType viewContentType = ContentPageType.PageType.BlogEntry;

			if (this.IgnoreSitePath) {
				sPagePath = String.Format("/siteid-{0}", SiteData.CurrentSiteID);
			}

			if (SiteData.IsWebView) {
				if (SiteData.CurrentSite.IsSiteSearchPath && !this.IgnoreSitePath) {
					this.ContentType = SummaryContentType.SiteSearch;
					sSearchTerm = GetSearchTerm();
				}
			}

			switch (this.ContentType) {
				case SummaryContentType.Blog:
				case SummaryContentType.ContentPage:
				case SummaryContentType.SiteSearch:
					this.InitOrderBy(x => x.GoLiveDate, false);
					break;
			}

			SortParm sp = this.ParseSort();
			string sSortFld = sp.SortField;
			string sSortDir = sp.SortDirection;

			if (this.PageNumber <= 0) {
				this.PageNumber = 1;
			}

			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				if (SiteData.IsWebView) {
					switch (this.ContentType) {
						case SummaryContentType.Blog:
							viewContentType = ContentPageType.PageType.BlogEntry;
							this.TotalRecords = navHelper.GetFilteredContentPagedCount(SiteData.CurrentSite, sPagePath, !SecurityData.IsAuthEditor);
							lstContents = navHelper.GetFilteredContentPagedList(SiteData.CurrentSite, sPagePath, !SecurityData.IsAuthEditor, this.PageSize, this.PageNumberZeroIndex, sSortFld, sSortDir);
							break;

						case SummaryContentType.ChildContentPage:
							viewContentType = ContentPageType.PageType.ContentEntry;
							this.TotalRecords = navHelper.GetChildNavigationCount(SiteData.CurrentSiteID, sPagePath, !SecurityData.IsAuthEditor);
							lstContents = navHelper.GetLatestChildContentPagedList(SiteData.CurrentSiteID, sPagePath, !SecurityData.IsAuthEditor, this.PageSize, this.PageNumberZeroIndex, sSortFld, sSortDir);
							break;

						case SummaryContentType.ContentPage:
							viewContentType = ContentPageType.PageType.ContentEntry;
							this.TotalRecords = navHelper.GetSitePageCount(SiteData.CurrentSiteID, viewContentType, !SecurityData.IsAuthEditor);
							lstContents = navHelper.GetLatestContentPagedList(SiteData.CurrentSiteID, viewContentType, !SecurityData.IsAuthEditor, this.PageSize, this.PageNumberZeroIndex, sSortFld, sSortDir);
							break;

						case SummaryContentType.SpecifiedCategories:
							viewContentType = ContentPageType.PageType.BlogEntry;
							this.TotalRecords = navHelper.GetFilteredContentByIDPagedCount(SiteData.CurrentSite, null, SelectedCategorySlugs, !SecurityData.IsAuthEditor);
							lstContents = navHelper.GetFilteredContentByIDPagedList(SiteData.CurrentSite, null, SelectedCategorySlugs, !SecurityData.IsAuthEditor, this.PageSize, this.PageNumberZeroIndex, sSortFld, sSortDir);
							break;

						case SummaryContentType.SiteSearch:
							this.TotalRecords = navHelper.GetSiteSearchCount(SiteData.CurrentSiteID, sSearchTerm, !SecurityData.IsAuthEditor);
							lstContents = navHelper.GetLatestContentSearchList(SiteData.CurrentSiteID, sSearchTerm, !SecurityData.IsAuthEditor, this.PageSize, this.PageNumberZeroIndex, sSortFld, sSortDir);
							break;
					}
				} else {
					viewContentType = ContentPageType.PageType.ContentEntry;
					this.TotalRecords = navHelper.GetSitePageCount(SiteData.CurrentSiteID, viewContentType, false);
					lstContents = navHelper.GetLatestContentPagedList(Guid.NewGuid(), viewContentType, false, this.PageSize, this.PageNumberZeroIndex, sSortFld, sSortDir);
				}
			}

			lstContents.ToList().ForEach(q => CMSConfigHelper.IdentifyLinkAsInactive(q));

			this.DataSource = lstContents;
		}
	}
}