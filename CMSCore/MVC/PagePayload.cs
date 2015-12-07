using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.WebPages;

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

	public class PagePayload {
		public static string ViewDataKey = "CmsWebViewPage_PagePayload_ViewData";

		public PagePayload() {
			this.ThePage = new ContentPage();
			this.TypeLabelPrefixes = new List<TypeHeadingOption>();
			this.TheWidgets = new List<Widget>();

			if (SiteData.CurretSiteExists) {
				this.TheSite = SiteData.CurrentSite;
			}
		}

		public static PagePayload GetCurrentContent() {
			PagePayload page = new PagePayload();
			page.ThePage = SiteData.GetCurrentPage();

			page.Load();
			return page;
		}

		public static PagePayload GetContent(Guid id) {
			PagePayload page = new PagePayload();
			page.ThePage = SiteData.GetPage(id);

			page.Load();
			return page;
		}

		public static PagePayload GetContent(SiteNav nav) {
			PagePayload page = new PagePayload();
			page.ThePage = nav.GetContentPage();

			page.Load();
			return page;
		}

		public static PagePayload GetContent(ContentPage cp) {
			PagePayload page = new PagePayload();
			page.ThePage = cp;

			page.Load();
			return page;
		}

		public static PagePayload GetContent(string uri) {
			PagePayload page = new PagePayload();

			if (SecurityData.AdvancedEditMode) {
				using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
					cmsHelper.OverrideKey(uri);
					if (cmsHelper.cmsAdminContent == null) {
						page.ThePage = SiteData.GetPage(uri);
						if (page.ThePage.ContentType == ContentPageType.PageType.BlogEntry) {
							var c = page.ThePage.ContentCategories;
							var t = page.ThePage.ContentTags;
						}

						cmsHelper.cmsAdminContent = page.ThePage;
					} else {
						page.ThePage = cmsHelper.cmsAdminContent;
					}
				}
			} else {
				page.ThePage = SiteData.GetPage(uri);
			}

			page.Load();
			return page;
		}

		public static PagePayload GetContentFromViewData() {
			if (WebPageContext.Current != null) {
				WebViewPage page = ((WebViewPage)WebPageContext.Current.Page);
				if (page != null) {
					if (page.ViewData[PagePayload.ViewDataKey] != null) {
						return (PagePayload)page.ViewData[PagePayload.ViewDataKey];
					}
					if (page is CmsWebViewPage) {
						return ((CmsWebViewPage)page).CmsPage;
					}
				}
			} else {
				// because this is probably a widget edit context
				return new PagePayload();
			}

			return null;
		}

		public void Load() {
			this.TheSite = SiteData.CurrentSite;

			CMSConfigHelper.IdentifyLinkAsInactive(this.ThePage);
			Guid guidContentID = Guid.Empty;

			if (this.ThePage != null) {
				guidContentID = this.ThePage.Root_ContentID;
				this.TheWidgets = SiteData.GetCurrentPageWidgets(guidContentID);
			} else {
				this.ThePage = new ContentPage();
				this.TheWidgets = new List<Widget>();
			}
		}

		public ContentPage ThePage { get; set; }
		public SiteData TheSite { get; set; }
		public List<Widget> TheWidgets { get; set; }

		private string _pageTitle = String.Empty;

		public MvcHtmlString Titlebar {
			get {
				LoadHeadCaption();

				if (String.IsNullOrEmpty(_pageTitle)) {
					string sPrefix = String.Empty;

					if (!this.ThePage.PageActive) {
						sPrefix = "* UNPUBLISHED * ";
					}
					if (this.ThePage.RetireDate < this.TheSite.Now) {
						sPrefix = "* RETIRED * ";
					}
					if (this.ThePage.GoLiveDate > this.TheSite.Now) {
						sPrefix = "* UNRELEASED * ";
					}
					string sPattern = sPrefix + SiteData.CurrentTitlePattern;

					_pageTitle = String.Format(sPattern, this.TheSite.SiteName, this.TheSite.SiteTagline, this.ThePage.TitleBar, this.ThePage.PageHead, this.ThePage.NavMenuText, this.ThePage.GoLiveDate, this.ThePage.EditDate);
				}

				return new MvcHtmlString(_pageTitle);
			}
		}

		public MvcHtmlString Heading {
			get {
				LoadHeadCaption();

				return new MvcHtmlString(this.ThePage.PageHead);
			}
		}

		private string _headingText = null;

		private void LoadHeadCaption() {
			if (this.TheSite.Blog_Root_ContentID == this.ThePage.Root_ContentID
				&& _headingText == null && this.TypeLabelPrefixes.Any()) {
				_headingText = String.Empty;
				using (ContentPageHelper pageHelper = new ContentPageHelper()) {
					PageViewType pvt = pageHelper.GetBlogHeadingFromURL(this.TheSite, SiteData.CurrentScriptName);
					_headingText = pvt.ExtraTitle;

					TypeHeadingOption titleOpts = this.TypeLabelPrefixes.Where(x => x.KeyValue == pvt.CurrentViewType).FirstOrDefault();

					if (titleOpts == null
						&& (pvt.CurrentViewType == PageViewType.ViewType.DateDayIndex
						|| pvt.CurrentViewType == PageViewType.ViewType.DateMonthIndex
						|| pvt.CurrentViewType == PageViewType.ViewType.DateYearIndex)) {
						titleOpts = this.TypeLabelPrefixes.Where(x => x.KeyValue == PageViewType.ViewType.DateIndex).FirstOrDefault();
					}

					if (titleOpts != null) {
						if (!String.IsNullOrEmpty(titleOpts.FormatText)) {
							pvt.ExtraTitle = string.Format(titleOpts.FormatText, pvt.RawValue);
							_headingText = pvt.ExtraTitle;
						}
						if (!String.IsNullOrEmpty(titleOpts.LabelText)) {
							this.ThePage.PageHead = String.Format("{0} {1}", titleOpts.LabelText, _headingText);
							this.ThePage.TitleBar = this.ThePage.PageHead;
							_headingText = this.ThePage.PageHead;
						}
					}
				}
			}
		}

		public List<TypeHeadingOption> TypeLabelPrefixes { get; set; }

		private List<SiteNav> _topnav = null;

		public List<SiteNav> TopNav {
			get {
				if (_topnav == null) {
					using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
						_topnav = navHelper.GetTopNavigation(this.TheSite.SiteID, !SecurityData.IsAuthEditor);
					}
					_topnav = TweakData(_topnav);
				}

				return _topnav;
			}
		}

		private List<SiteNav> _top2nav = null;

		public List<SiteNav> Top2Nav {
			get {
				if (_top2nav == null) {
					using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
						_top2nav = navHelper.GetTwoLevelNavigation(this.TheSite.SiteID, !SecurityData.IsAuthEditor);
					}
					_top2nav = TweakData(_top2nav);
				}

				return _top2nav;
			}
		}

		private List<SiteNav> _childnav = null;

		public List<SiteNav> ChildNav {
			get {
				if (_childnav == null) {
					using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
						_childnav = navHelper.GetChildNavigation(this.TheSite.SiteID, this.ThePage.Root_ContentID, !SecurityData.IsAuthEditor);
					}
					_childnav = TweakData(_childnav);
				}

				return _childnav;
			}
		}

		private List<SiteNav> _sibnav = null;

		public List<SiteNav> SiblingNav {
			get {
				if (_sibnav == null) {
					using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
						_sibnav = navHelper.GetSiblingNavigation(this.TheSite.SiteID, this.ThePage.Root_ContentID, !SecurityData.IsAuthEditor);
					}
					_sibnav = TweakData(_sibnav);
				}

				return _sibnav;
			}
		}

		private SiteNav _parentnav = null;

		public SiteNav ParentNav {
			get {
				if (_parentnav == null) {
					using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
						_parentnav = navHelper.GetParentPageNavigation(this.TheSite.SiteID, this.ThePage.Root_ContentID);
					}
					if (_parentnav != null) {
						_parentnav = IdentifyLinkAsInactive(_parentnav);
					}
				}

				return _parentnav;
			}
		}

		private SiteNav _hometnav = null;

		public SiteNav HomeNav {
			get {
				if (_hometnav == null) {
					using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
						_hometnav = navHelper.FindHome(this.TheSite.SiteID);
					}
					if (_hometnav != null) {
						_hometnav = IdentifyLinkAsInactive(_hometnav);
					}
				}

				return _hometnav;
			}
		}

		private SiteNav _blogidxnav = null;

		public SiteNav SearchNav {
			get {
				if (_blogidxnav == null) {
					if (this.TheSite.Blog_Root_ContentID.HasValue) {
						using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
							_blogidxnav = navHelper.GetLatestVersion(this.TheSite.SiteID, this.TheSite.Blog_Root_ContentID.Value);
						}
						_blogidxnav = IdentifyLinkAsInactive(_blogidxnav);
					}
				}

				return _blogidxnav;
			}
		}

		private List<SiteNav> _breadcrumbs = null;

		public List<SiteNav> BreadCrumbs {
			get {
				if (_breadcrumbs == null) {
					using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
						SiteNav pageNav = this.ThePage.GetSiteNav();

						if (SiteData.CurretSiteExists && SiteData.CurrentSite.Blog_Root_ContentID.HasValue &&
							pageNav.ContentType == ContentPageType.PageType.BlogEntry) {
							_breadcrumbs = navHelper.GetPageCrumbNavigation(this.TheSite.SiteID, SiteData.CurrentSite.Blog_Root_ContentID.Value, !SecurityData.IsAuthEditor);

							if (_breadcrumbs != null && _breadcrumbs.Any()) {
								pageNav.NavOrder = _breadcrumbs.Max(x => x.NavOrder) + 100;
								_breadcrumbs.Add(pageNav);
							}
						} else {
							_breadcrumbs = navHelper.GetPageCrumbNavigation(this.TheSite.SiteID, pageNav.Root_ContentID, !SecurityData.IsAuthEditor);
						}
						_breadcrumbs.RemoveAll(x => x.ShowInSiteNav == false && x.ContentType == ContentPageType.PageType.ContentEntry);
					}
					if (_breadcrumbs != null) {
						_breadcrumbs = TweakData(_breadcrumbs);
					}
				}

				return _breadcrumbs;
			}
		}

		public bool NavIsCurrentPage(SiteNav nav) {
			return this.ThePage.Root_ContentID == nav.Root_ContentID;
		}

		public List<SiteNav> GetTopNav(List<SiteNav> nav) {
			return nav.Where(ct => ct.Parent_ContentID == null).OrderBy(ct => ct.NavMenuText).OrderBy(ct => ct.NavOrder).ToList();
		}

		public List<SiteNav> GetChildren(List<SiteNav> nav, Guid rootContentID) {
			return nav.Where(ct => ct.Parent_ContentID == rootContentID).OrderBy(ct => ct.NavMenuText).OrderBy(ct => ct.NavOrder).ToList();
		}

		private List<SiteNav> _childeditnav = null;

		public List<SiteNav> ChildEditNav {
			get {
				if (_childeditnav == null) {
					using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
						_childeditnav = navHelper.GetChildNavigation(this.TheSite.SiteID, this.ThePage.Root_ContentID, !SecurityData.IsAuthEditor);
					}
				}

				return _childeditnav;
			}
		}

		private List<CMSTemplate> _templates = null;

		public List<CMSTemplate> Templates {
			get {
				if (_templates == null) {
					using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
						_templates = cmsHelper.Templates;
					}
				}

				return _templates;
			}
		}

		private List<CMSPlugin> _plugins = null;

		public List<CMSPlugin> Plugins {
			get {
				if (_plugins == null) {
					using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
						_plugins = cmsHelper.ToolboxPlugins;
					}
				}

				return _plugins;
			}
		}

		public string GeneratedFileName {
			get {
				if (this.ThePage.ContentType == ContentPageType.PageType.BlogEntry) {
					return ContentPageHelper.CreateFileNameFromSlug(this.TheSite.SiteID, this.ThePage.GoLiveDate, this.ThePage.PageSlug);
				} else {
					return this.ThePage.FileName;
				}
			}
		}

		public bool IsPageLocked {
			get {
				using (ContentPageHelper pageHelper = new ContentPageHelper()) {
					return pageHelper.IsPageLocked(this.ThePage);
				}
			}
		}

		public UserProfile LockUser {
			get {
				if (IsPageLocked && this.ThePage.Heartbeat_UserId.HasValue) {
					return SecurityData.GetProfileByUserID(this.ThePage.Heartbeat_UserId.Value);
				}
				return null;
			}
		}

		public List<SiteNav> GetSiteUpdates(ListContentType contentType, List<string> lstCategorySlugs) {
			return GetSiteUpdates(contentType, -1, null, lstCategorySlugs);
		}

		public List<SiteNav> GetSiteUpdates(ListContentType contentType, List<Guid> lstCategories) {
			return GetSiteUpdates(contentType, -1, lstCategories, null);
		}

		public List<SiteNav> GetSiteUpdates(ListContentType contentType, int takeTop, List<string> lstCategorySlugs) {
			return GetSiteUpdates(contentType, takeTop, null, lstCategorySlugs);
		}

		public List<SiteNav> GetSiteUpdates(ListContentType contentType, int takeTop, List<Guid> lstCategories) {
			return GetSiteUpdates(contentType, takeTop, lstCategories, null);
		}

		public List<SiteNav> GetSiteUpdates(ListContentType contentType, int takeTop) {
			return GetSiteUpdates(contentType, takeTop, null, null);
		}

		public List<SiteNav> GetSiteUpdates(int takeTop) {
			return GetSiteUpdates(ListContentType.Blog, takeTop, null, null);
		}

		internal List<SiteNav> GetSiteUpdates(ListContentType contentType, int takeTop, List<Guid> lstCategories, List<string> lstCategorySlugs) {
			List<SiteNav> _siteUpdates = new List<SiteNav>();
			if (lstCategories == null) {
				lstCategories = new List<Guid>();
			}
			if (lstCategorySlugs == null) {
				lstCategorySlugs = new List<string>();
			}

			if (lstCategories.Any() || lstCategorySlugs.Any()) {
				contentType = ListContentType.SpecifiedCategories;
			}

			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				switch (contentType) {
					case ListContentType.Blog:
						_siteUpdates = navHelper.GetLatestPosts(this.TheSite.SiteID, takeTop, !SecurityData.IsAuthEditor);
						break;

					case ListContentType.ContentPage:
						_siteUpdates = navHelper.GetLatest(this.TheSite.SiteID, takeTop, !SecurityData.IsAuthEditor);
						break;

					case ListContentType.SpecifiedCategories:
						if (takeTop > 0) {
							_siteUpdates = navHelper.GetFilteredContentByIDPagedList(SiteData.CurrentSite, lstCategories, lstCategorySlugs, !SecurityData.IsAuthEditor, takeTop, 0, "GoLiveDate", "DESC");
						} else {
							_siteUpdates = navHelper.GetFilteredContentByIDPagedList(SiteData.CurrentSite, lstCategories, lstCategorySlugs, !SecurityData.IsAuthEditor, 250000, 0, "NavMenuText", "ASC");
						}
						break;
				}
			}

			if (_siteUpdates == null) {
				_siteUpdates = new List<SiteNav>();
			}

			_siteUpdates = TweakData(_siteUpdates);

			return _siteUpdates;
		}

		public List<ContentTag> GetPageTags(int takeTop) {
			if (takeTop < 0) {
				takeTop = 100000;
			}

			if (SecurityData.AdvancedEditMode) {
				using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
					cmsHelper.OverrideKey(this.ThePage.FileName);
					if (cmsHelper.cmsAdminContent != null) {
						return cmsHelper.cmsAdminContent.ContentTags.Take(takeTop).ToList();
					}
				}
			} else {
				using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
					return navHelper.GetTagListForPost(this.TheSite.SiteID, takeTop, this.ThePage.Root_ContentID);
				}
			}

			return new List<ContentTag>();
		}

		private int _pageCt = -10;

		public int SitePageCount {
			get {
				if (_pageCt < 0) {
					using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
						_pageCt = navHelper.GetSitePageCount(this.TheSite.SiteID, ContentPageType.PageType.ContentEntry);
					}
				}

				return _pageCt;
			}
		}

		private int _postCt = -10;

		public int SitePostCount {
			get {
				if (_postCt < 0) {
					using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
						_postCt = navHelper.GetSitePageCount(this.TheSite.SiteID, ContentPageType.PageType.BlogEntry);
					}
				}

				return _postCt;
			}
		}

		public double GetRoundedMetaPercentage(IMetaDataLinks meta) {
			return GetRoundedMetaPercentage(meta, 5);
		}

		public double GetRoundedMetaPercentage(IMetaDataLinks meta, int nearestNumber) {
			if (nearestNumber > 100 || nearestNumber < 1) {
				nearestNumber = 5;
			}

			double percUsed = Math.Ceiling(100 * (float)meta.Count / (((float)this.SitePostCount + 0.000001)));
			percUsed = Math.Round(percUsed / nearestNumber) * nearestNumber;
			if (percUsed < 1 && meta.Count > 0) {
				percUsed = 1;
			}

			return percUsed;
		}

		public List<ContentCategory> GetPageCategories(int takeTop) {
			if (takeTop < 0) {
				takeTop = 300000;
			}

			if (SecurityData.AdvancedEditMode) {
				using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
					cmsHelper.OverrideKey(this.ThePage.FileName);
					if (cmsHelper.cmsAdminContent != null) {
						return cmsHelper.cmsAdminContent.ContentCategories.Take(takeTop).ToList();
					}
				}
			} else {
				using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
					return navHelper.GetCategoryListForPost(this.TheSite.SiteID, takeTop, this.ThePage.Root_ContentID);
				}
			}
			return new List<ContentCategory>();
		}

		public List<ContentTag> GetSiteTags(int takeTop, bool ShowNonZeroCountOnly) {
			List<ContentTag> lstNav = new List<ContentTag>();
			if (takeTop < 0) {
				takeTop = 100000;
			}

			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				lstNav = navHelper.GetTagList(this.TheSite.SiteID, takeTop);
			}

			lstNav.RemoveAll(x => x.Count < 1 && ShowNonZeroCountOnly);
			lstNav = lstNav.OrderByDescending(x => x.Count).ToList();

			return lstNav;
		}

		public List<ContentCategory> GetSiteCategories(int takeTop, bool ShowNonZeroCountOnly) {
			List<ContentCategory> lstNav = new List<ContentCategory>();
			if (takeTop < 0) {
				takeTop = 100000;
			}
			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				lstNav = navHelper.GetCategoryList(this.TheSite.SiteID, takeTop);
			}

			lstNav.RemoveAll(x => x.Count < 1 && ShowNonZeroCountOnly);
			lstNav = lstNav.OrderByDescending(x => x.Count).ToList();

			return lstNav;
		}

		public List<ContentDateTally> GetSiteDates(int takeTop, string dateFormat) {
			var lst = GetSiteDates(takeTop);

			if (String.IsNullOrEmpty(dateFormat)) {
				dateFormat = "MMMM yyyy";
			}

			lst.ForEach(x => x.DateCaption = x.TallyDate.ToString(dateFormat));

			return lst;
		}

		public List<ContentDateTally> GetSiteDates(int takeTop) {
			List<ContentDateTally> lstNav = new List<ContentDateTally>();
			if (takeTop < 0) {
				takeTop = 100000;
			}
			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				lstNav = navHelper.GetMonthBlogUpdateList(this.TheSite.SiteID, takeTop, !SecurityData.IsAuthEditor);
			}

			lstNav.RemoveAll(x => x.Count < 1);
			lstNav = lstNav.OrderByDescending(x => x.TallyDate).ToList();

			return lstNav;
		}

		// ======================================
		private SiteNav IdentifyLinkAsInactive(SiteNav nav) {
			return CMSConfigHelper.IdentifyLinkAsInactive(nav);
		}

		private List<SiteNav> TweakData(List<SiteNav> nav) {
			if (nav != null) {
				nav.RemoveAll(x => x.ShowInSiteNav == false && x.ContentType == ContentPageType.PageType.ContentEntry);
				nav.RemoveAll(x => x.ShowInSiteMap == false && x.ContentType == ContentPageType.PageType.ContentEntry);

				nav.ToList().ForEach(q => IdentifyLinkAsInactive(q));
			}

			return nav;
		}
	}

	//==================
	public enum ListContentType {
		Unknown,
		Blog,
		ContentPage,
		SpecifiedCategories
	}
}