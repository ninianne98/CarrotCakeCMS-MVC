using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

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

	public class PagePayload {
		public static string ViewDataKey = "CmsWebViewPage_PagePayload_ViewData";

		public static PagePayload GetCurrentContent() {
			Guid guidContentID = Guid.Empty;

			PagePayload page = new PagePayload();
			page.ThePage = SiteData.GetCurrentPage();

			page.Load();
			return page;
		}

		public static PagePayload GetContent(string uri) {
			PagePayload page = new PagePayload();
			page.ThePage = SiteData.GetPage(uri);

			page.Load();
			return page;
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

		public void HandleTemplatePath(Controller controller) {
			string templateFile = this.ThePage.TemplateFile;
			string folderPath = templateFile.Substring(0, templateFile.LastIndexOf("/"));

			List<CmsTemplateViewEngine> lst = controller.ViewEngineCollection
				.Where(x => x is CmsTemplateViewEngine).Cast<CmsTemplateViewEngine>()
				.Where(x => x.ThemeFile.ToLower() == templateFile.ToLower()
					|| x.ThemeFile.ToLower().StartsWith(folderPath.ToLower())).ToList();

			if (!lst.Any()) {
				CmsTemplateViewEngine ve = new CmsTemplateViewEngine(templateFile);
				controller.ViewEngineCollection.Add(ve);
			}
		}

		public PagePayload() {
			this.TypeLabelPrefixes = new List<TypeHeadingOption>();
			this.TheWidgets = new List<Widget>();
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

		private List<SiteNav> _top2nav = null;

		public List<SiteNav> Top2Nav {
			get {
				if (_top2nav == null) {
					using (SiteNavHelper navHelper = new SiteNavHelper()) {
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
					using (SiteNavHelper navHelper = new SiteNavHelper()) {
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
					using (SiteNavHelper navHelper = new SiteNavHelper()) {
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
					using (SiteNavHelper navHelper = new SiteNavHelper()) {
						_parentnav = navHelper.GetParentPageNavigation(this.TheSite.SiteID, this.ThePage.Root_ContentID);
					}
					if (_parentnav != null) {
						_parentnav = IdentifyLinkAsInactive(_parentnav);
					}
				}

				return _parentnav;
			}
		}

		private SiteNav _blogidxnav = null;

		public SiteNav SearchNav {
			get {
				if (_blogidxnav == null) {
					if (this.TheSite.Blog_Root_ContentID.HasValue) {
						using (SiteNavHelper navHelper = new SiteNavHelper()) {
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
					using (SiteNavHelper navHelper = new SiteNavHelper()) {
						SiteNav pageNav = this.ThePage.GetSiteNav();

						if (SiteData.CurretSiteExists && SiteData.CurrentSite.Blog_Root_ContentID.HasValue &&
							pageNav.ContentType == ContentPageType.PageType.BlogEntry) {
							_breadcrumbs = navHelper.GetPageCrumbNavigation(SiteData.CurrentSiteID, SiteData.CurrentSite.Blog_Root_ContentID.Value, !SecurityData.IsAuthEditor);

							if (_breadcrumbs != null && _breadcrumbs.Any()) {
								pageNav.NavOrder = _breadcrumbs.Max(x => x.NavOrder) + 100;
								_breadcrumbs.Add(pageNav);
							}
						} else {
							_breadcrumbs = navHelper.GetPageCrumbNavigation(SiteData.CurrentSiteID, pageNav.Root_ContentID, !SecurityData.IsAuthEditor);
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
					using (SiteNavHelper navHelper = new SiteNavHelper()) {
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

		public List<ContentTag> GetPageTags(int iTakeTop) {
			if (iTakeTop < 0) {
				iTakeTop = 100000;
			}

			using (SiteNavHelper navHelper = new SiteNavHelper()) {
				return navHelper.GetTagListForPost(this.TheSite.SiteID, iTakeTop, this.ThePage.Root_ContentID);
			}
		}

		public List<ContentCategory> GetPageCategories(int iTakeTop) {
			if (iTakeTop < 0) {
				iTakeTop = 100000;
			}
			using (SiteNavHelper navHelper = new SiteNavHelper()) {
				return navHelper.GetCategoryListForPost(this.TheSite.SiteID, iTakeTop, this.ThePage.Root_ContentID);
			}
		}

		public List<ContentTag> GetSiteTags(int iTakeTop, bool ShowNonZeroCountOnly) {
			List<ContentTag> lstNav = new List<ContentTag>();
			if (iTakeTop < 0) {
				iTakeTop = 100000;
			}
			using (SiteNavHelper navHelper = new SiteNavHelper()) {
				lstNav = navHelper.GetTagList(this.TheSite.SiteID, iTakeTop);
			}

			lstNav.RemoveAll(x => x.Count < 1 && ShowNonZeroCountOnly);
			lstNav = lstNav.OrderByDescending(x => x.Count).ToList();

			return lstNav;
		}

		public List<ContentCategory> GetSiteCategories(int iTakeTop, bool ShowNonZeroCountOnly) {
			List<ContentCategory> lstNav = new List<ContentCategory>();
			if (iTakeTop < 0) {
				iTakeTop = 100000;
			}
			using (SiteNavHelper navHelper = new SiteNavHelper()) {
				lstNav = navHelper.GetCategoryList(this.TheSite.SiteID, iTakeTop);
			}

			lstNav.RemoveAll(x => x.Count < 1 && ShowNonZeroCountOnly);
			lstNav = lstNav.OrderByDescending(x => x.Count).ToList();

			return lstNav;
		}

		public List<ContentDateTally> GetSiteDates(int iTakeTop) {
			List<ContentDateTally> lstNav = new List<ContentDateTally>();
			if (iTakeTop < 0) {
				iTakeTop = 100000;
			}
			using (SiteNavHelper navHelper = new SiteNavHelper()) {
				lstNav = navHelper.GetMonthBlogUpdateList(SiteData.CurrentSiteID, iTakeTop, !SecurityData.IsAuthEditor);
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
}