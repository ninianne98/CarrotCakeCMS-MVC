using Carrotware.CMS.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;

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

	public class SiteNavHelper : IDisposable, ISiteNavHelper {
		private ISiteNavHelper _navHelper = null;

		public SiteNavHelper() {
			if (SiteData.IsWebView) {
				if ((SiteData.IsPageSampler || SiteData.IsPageReal) && !SiteData.IsCurrentPageSpecial) {
					_navHelper = new SiteNavHelperMock();
				} else {
					_navHelper = new SiteNavHelperReal();
				}
			} else {
				_navHelper = new SiteNavHelperMock();
			}
		}

		public enum SiteNavMode {
			RealNav,
			MockupNav,
		}

		public SiteNavHelper(SiteNavMode navMode) {
			if (navMode == SiteNavMode.RealNav) {
				_navHelper = new SiteNavHelperReal();
			} else {
				_navHelper = new SiteNavHelperMock();
			}
		}

		#region ISiteNavHelper Members

		public SiteNav FindByFilename(Guid siteID, string urlFileName) {
			return _navHelper.FindByFilename(siteID, urlFileName);
		}

		public SiteNav FindHome(Guid siteID) {
			return _navHelper.FindHome(siteID);
		}

		public SiteNav FindHome(Guid siteID, bool bActiveOnly) {
			return _navHelper.FindHome(siteID, bActiveOnly);
		}

		public List<SiteNav> GetChildNavigation(Guid siteID, Guid? ParentID, bool bActiveOnly) {
			return _navHelper.GetChildNavigation(siteID, ParentID, bActiveOnly);
		}

		public List<SiteNav> GetChildNavigation(Guid siteID, string sParentID, bool bActiveOnly) {
			return _navHelper.GetChildNavigation(siteID, sParentID, bActiveOnly);
		}

		public int GetChildNavigationCount(Guid siteID, Guid? ParentID, bool bActiveOnly) {
			return _navHelper.GetChildNavigationCount(siteID, ParentID, bActiveOnly);
		}

		public int GetChildNavigationCount(Guid siteID, string sParentID, bool bActiveOnly) {
			return _navHelper.GetChildNavigationCount(siteID, sParentID, bActiveOnly);
		}

		public List<SiteNav> GetLatest(Guid siteID, int iUpdates, bool bActiveOnly) {
			return _navHelper.GetLatest(siteID, iUpdates, bActiveOnly);
		}

		public List<SiteNav> GetLatestPosts(Guid siteID, int iUpdates, bool bActiveOnly) {
			return _navHelper.GetLatestPosts(siteID, iUpdates, bActiveOnly);
		}

		public List<SiteNav> GetLatestUpdates(Guid siteID, int iUpdates, bool bActiveOnly) {
			return _navHelper.GetLatestUpdates(siteID, iUpdates, bActiveOnly);
		}

		public List<SiteNav> GetLatestPostUpdates(Guid siteID, int iUpdates, bool bActiveOnly) {
			return _navHelper.GetLatestPostUpdates(siteID, iUpdates, bActiveOnly);
		}

		public List<ContentDateTally> GetMonthBlogUpdateList(Guid siteID, int iUpdates, bool bActiveOnly) {
			return _navHelper.GetMonthBlogUpdateList(siteID, iUpdates, bActiveOnly);
		}

		public List<ContentDateLinks> GetSingleMonthBlogUpdateList(SiteData currentSite, DateTime monthDate, bool bActiveOnly) {
			return _navHelper.GetSingleMonthBlogUpdateList(currentSite, monthDate, bActiveOnly);
		}

		public List<ContentCategory> GetCategoryList(Guid siteID, int iUpdates) {
			return _navHelper.GetCategoryList(siteID, iUpdates);
		}

		public List<ContentTag> GetTagList(Guid siteID, int iUpdates) {
			return _navHelper.GetTagList(siteID, iUpdates);
		}

		public List<ContentTag> GetTagListForPost(Guid siteID, int iUpdates, string urlFileName) {
			return _navHelper.GetTagListForPost(siteID, iUpdates, urlFileName);
		}

		public List<ContentCategory> GetCategoryListForPost(Guid siteID, int iUpdates, string urlFileName) {
			return _navHelper.GetCategoryListForPost(siteID, iUpdates, urlFileName);
		}

		public List<ContentTag> GetTagListForPost(Guid siteID, int iUpdates, Guid rootContentID) {
			return _navHelper.GetTagListForPost(siteID, iUpdates, rootContentID);
		}

		public List<ContentCategory> GetCategoryListForPost(Guid siteID, int iUpdates, Guid rootContentID) {
			return _navHelper.GetCategoryListForPost(siteID, iUpdates, rootContentID);
		}

		public SiteNav GetLatestVersion(Guid siteID, Guid rootContentID) {
			return _navHelper.GetLatestVersion(siteID, rootContentID);
		}

		public SiteNav GetPrevPost(Guid siteID, Guid rootContentID, bool bActiveOnly) {
			return _navHelper.GetPrevPost(siteID, rootContentID, bActiveOnly);
		}

		public SiteNav GetNextPost(Guid siteID, Guid rootContentID, bool bActiveOnly) {
			return _navHelper.GetNextPost(siteID, rootContentID, bActiveOnly);
		}

		public SiteNav GetLatestVersion(Guid siteID, bool bActiveOnly, string sPage) {
			return _navHelper.GetLatestVersion(siteID, bActiveOnly, sPage);
		}

		public List<SiteNav> GetMasterNavigation(Guid siteID, bool bActiveOnly) {
			return _navHelper.GetMasterNavigation(siteID, bActiveOnly);
		}

		public SiteNav GetPageNavigation(Guid siteID, Guid rootContentID) {
			return _navHelper.GetPageNavigation(siteID, rootContentID);
		}

		public SiteNav GetPageNavigation(Guid siteID, string sPage) {
			return _navHelper.GetPageNavigation(siteID, sPage);
		}

		public SiteNav GetParentPageNavigation(Guid siteID, Guid rootContentID) {
			return _navHelper.GetParentPageNavigation(siteID, rootContentID);
		}

		public SiteNav GetParentPageNavigation(Guid siteID, string sPage) {
			return _navHelper.GetParentPageNavigation(siteID, sPage);
		}

		public List<SiteNav> GetSiblingNavigation(Guid siteID, Guid PageID, bool bActiveOnly) {
			return _navHelper.GetSiblingNavigation(siteID, PageID, bActiveOnly);
		}

		public List<SiteNav> GetSiblingNavigation(Guid siteID, string sPage, bool bActiveOnly) {
			return _navHelper.GetSiblingNavigation(siteID, sPage, bActiveOnly);
		}

		public List<SiteNav> GetTopNavigation(Guid siteID, bool bActiveOnly) {
			return _navHelper.GetTopNavigation(siteID, bActiveOnly);
		}

		public List<SiteNav> GetTwoLevelNavigation(Guid siteID, bool bActiveOnly) {
			return _navHelper.GetTwoLevelNavigation(siteID, bActiveOnly);
		}

		public List<SiteNav> GetLevelDepthNavigation(Guid siteID, int iDepth, bool bActiveOnly) {
			return _navHelper.GetLevelDepthNavigation(siteID, iDepth, bActiveOnly);
		}

		public List<SiteNav> GetPageCrumbNavigation(Guid siteID, Guid rootContentID, bool bActiveOnly) {
			return _navHelper.GetPageCrumbNavigation(siteID, rootContentID, bActiveOnly);
		}

		public List<SiteNav> GetPageCrumbNavigation(Guid siteID, string sPage, bool bActiveOnly) {
			return _navHelper.GetPageCrumbNavigation(siteID, sPage, bActiveOnly);
		}

		public int GetFilteredContentPagedCount(SiteData currentSite, string sFilterPath, bool bActiveOnly) {
			return _navHelper.GetFilteredContentPagedCount(currentSite, sFilterPath, bActiveOnly);
		}

		public int GetFilteredContentByIDPagedCount(SiteData currentSite, List<Guid> lstCategories, bool bActiveOnly) {
			return _navHelper.GetFilteredContentByIDPagedCount(currentSite, lstCategories, bActiveOnly);
		}

		public int GetFilteredContentByIDPagedCount(SiteData currentSite, List<Guid> lstCategoryGUIDs, List<string> lstCategorySlugs, bool bActiveOnly) {
			return _navHelper.GetFilteredContentByIDPagedCount(currentSite, lstCategoryGUIDs, lstCategorySlugs, bActiveOnly);
		}

		public string GetBlogHeadingFromURL(SiteData currentSite, string sFilterPath) {
			return _navHelper.GetBlogHeadingFromURL(currentSite, sFilterPath);
		}

		public List<SiteNav> GetFilteredContentPagedList(SiteData currentSite, string sFilterPath, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return _navHelper.GetFilteredContentPagedList(currentSite, sFilterPath, bActiveOnly, pageSize, pageNumber, sortField, sortDir);
		}

		public List<SiteNav> GetFilteredContentByIDPagedList(SiteData currentSite, List<Guid> lstCategories, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return _navHelper.GetFilteredContentByIDPagedList(currentSite, lstCategories, bActiveOnly, pageSize, pageNumber, sortField, sortDir);
		}

		public List<SiteNav> GetFilteredContentByIDPagedList(SiteData currentSite, List<Guid> lstCategoryGUIDs, List<string> lstCategorySlugs, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return _navHelper.GetFilteredContentByIDPagedList(currentSite, lstCategoryGUIDs, lstCategorySlugs, bActiveOnly, pageSize, pageNumber, sortField, sortDir);
		}

		public List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageNumber) {
			return _navHelper.GetLatestBlogPagedList(siteID, bActiveOnly, pageNumber);
		}

		public List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageNumber, string sortField, string sortDir) {
			return _navHelper.GetLatestBlogPagedList(siteID, bActiveOnly, pageNumber, sortField, sortDir);
		}

		public List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber) {
			return _navHelper.GetLatestBlogPagedList(siteID, bActiveOnly, pageSize, pageNumber);
		}

		public List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return _navHelper.GetLatestBlogPagedList(siteID, bActiveOnly, pageSize, pageNumber, sortField, sortDir);
		}

		public List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageNumber) {
			return _navHelper.GetLatestContentPagedList(siteID, postType, bActiveOnly, pageNumber);
		}

		public List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageNumber, string sortField, string sortDir) {
			return _navHelper.GetLatestContentPagedList(siteID, postType, bActiveOnly, pageNumber, sortField, sortDir);
		}

		public List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageSize, int pageNumber) {
			return _navHelper.GetLatestContentPagedList(siteID, postType, bActiveOnly, pageSize, pageNumber);
		}

		public List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return _navHelper.GetLatestContentPagedList(siteID, postType, bActiveOnly, pageSize, pageNumber, sortField, sortDir);
		}

		public List<SiteNav> GetLatestContentSearchList(Guid siteID, string searchTerm, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return _navHelper.GetLatestContentSearchList(siteID, searchTerm, bActiveOnly, pageSize, pageNumber, sortField, sortDir);
		}

		public List<SiteNav> GetLatestChildContentPagedList(Guid siteID, Guid? parentContentID, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return _navHelper.GetLatestChildContentPagedList(siteID, parentContentID, bActiveOnly, pageSize, pageNumber, sortField, sortDir);
		}

		public List<SiteNav> GetLatestChildContentPagedList(Guid siteID, string sParentPage, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return _navHelper.GetLatestChildContentPagedList(siteID, sParentPage, bActiveOnly, pageSize, pageNumber, sortField, sortDir);
		}

		public int GetSiteContentCount(Guid siteID) {
			return _navHelper.GetSiteContentCount(siteID);
		}

		public int GetSitePageCount(Guid siteID, ContentPageType.PageType entryType) {
			return _navHelper.GetSitePageCount(siteID, entryType);
		}

		public int GetSitePageCount(Guid siteID, ContentPageType.PageType entryType, bool bActiveOnly) {
			return _navHelper.GetSitePageCount(siteID, entryType, bActiveOnly);
		}

		public int GetSiteSearchCount(Guid siteID, string searchTerm, bool bActiveOnly) {
			return _navHelper.GetSiteSearchCount(siteID, searchTerm, bActiveOnly);
		}

		public List<SiteNav> PerformDataPagingQueryableContent(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir, IQueryable<vw_carrot_Content> QueryInput) {
			return _navHelper.PerformDataPagingQueryableContent(siteID, bActiveOnly, pageSize, pageNumber, sortField, sortDir, QueryInput);
		}

		#endregion ISiteNavHelper Members

		public static List<string> GetSiteDirectoryPaths() {
			List<string> lstContent = null;

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				lstContent = (from ct in _db.vw_carrot_Contents
							  where ct.IsLatestVersion == true
								  && ct.FileName.ToLower().EndsWith(SiteData.DefaultDirectoryFilename)
							  select ct.FileName.ToLower()).Distinct().ToList();
			}

			return lstContent;
		}

		public static SiteNav GetEmptyHome() {
			SiteNav navData = new SiteNav();
			navData.ContentID = Guid.Empty;
			navData.Root_ContentID = Guid.Empty;
			navData.SiteID = SiteData.CurrentSiteID;
			navData.TemplateFile = SiteData.DefaultDirectoryFilename;
			navData.FileName = SiteData.DefaultDirectoryFilename;
			navData.NavMenuText = "NONE";
			navData.PageHead = "NONE";
			navData.TitleBar = "NONE";
			navData.PageActive = false;
			navData.PageText = "NO PAGE CONTENT";
			navData.EditDate = DateTime.Now.Date.AddDays(-1);
			navData.CreateDate = DateTime.Now.Date.AddDays(-10);
			navData.GoLiveDate = DateTime.Now.Date.AddDays(1);
			navData.RetireDate = DateTime.Now.Date.AddDays(90);
			navData.ContentType = ContentPageType.PageType.ContentEntry;
			return navData;
		}

		public static List<SiteNav> GetSamplerFakeNav() {
			return GetSamplerFakeNav(4, null);
		}

		public static List<SiteNav> GetSamplerFakeNav(int iCount) {
			return GetSamplerFakeNav(iCount, null);
		}

		public static List<SiteNav> GetSamplerFakeNav(Guid? rootParentID) {
			return GetSamplerFakeNav(4, rootParentID);
		}

		public static List<SiteNav> GetSamplerFakeNav(int iCount, Guid? rootParentID) {
			List<SiteNav> navList = new List<SiteNav>();
			int n = 0;

			while (n < iCount) {
				SiteNav nav = GetSamplerView();
				nav.NavOrder = n;
				nav.NavMenuText = nav.NavMenuText + " " + n.ToString();
				nav.CreateDate = nav.CreateDate.AddHours((0 - n) * 25);
				nav.EditDate = nav.CreateDate.AddHours((0 - n) * 16);
				nav.GoLiveDate = DateTime.Now.Date.AddMinutes(-5);
				nav.RetireDate = DateTime.Now.Date.AddDays(90);
				nav.CommentCount = (n * 2) + 1;
				nav.ShowInSiteNav = true;
				nav.ShowInSiteMap = true;

				if (n > 0 || rootParentID != null) {
					nav.Root_ContentID = Guid.NewGuid();
					nav.ContentID = Guid.NewGuid();
					//nav.FileName = nav.FileName.Replace(".aspx", nav.NavOrder.ToString() + ".aspx");
					nav.FileName = "/#";
					if (rootParentID != null) {
						nav.NavMenuText = nav.NavMenuText + " - " + rootParentID.Value.ToString().Substring(0, 4);
					}
				}
				nav.Parent_ContentID = rootParentID;

				navList.Add(nav);
				n++;
			}

			return navList;
		}

		public static SiteNav GetSamplerView(Guid rootParentID) {
			var sn = GetSamplerView();

			sn.Parent_ContentID = rootParentID;

			return sn;
		}

		public static string GetSampleBody() {
			return GetSampleBody(null, "");
		}

		public static string GetSampleBody(string sContentSampleNumber) {
			return GetSampleBody(null, sContentSampleNumber);
		}

		public static string GetSampleBody(Control X, string sContentSampleNumber) { // SampleContent2
			if (String.IsNullOrEmpty(sContentSampleNumber)) {
				sContentSampleNumber = "SampleContent2";
			}

			string sFile2 = " <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus mi arcu, lacinia scelerisque blandit nec, mattis non nibh.</p> \r\n <p> Curabitur quis urna at massa placerat auctor. Quisque et mauris sapien, a consectetur nulla.</p>";

			try {
				Assembly _assembly = Assembly.GetExecutingAssembly();
				using (StreamReader oTextStream = new StreamReader(_assembly.GetManifestResourceStream("Carrotware.CMS.Core.SiteContent.Mock." + sContentSampleNumber + ".txt"))) {
					sFile2 = oTextStream.ReadToEnd();
				}

				List<string> imageNames = (from i in _assembly.GetManifestResourceNames()
										   where i.Contains("SiteContent.Mock.sample")
										   && i.EndsWith(".png")
										   select i).ToList();

				foreach (string img in imageNames) {
					var imgURL = CMSConfigHelper.GetWebResourceUrl(X, typeof(SiteNav), img);
					sFile2 = sFile2.Replace(img, imgURL);
				}
			} catch { }

			return sFile2;
		}

		public static SiteNav GetSamplerView() {
			string sFile2 = GetSampleBody();

			SiteNav navNew = new SiteNav();
			navNew.Root_ContentID = Guid.NewGuid();
			navNew.ContentID = Guid.NewGuid();

			navNew.NavOrder = -1;
			navNew.TitleBar = "Template Preview - TITLE";
			navNew.NavMenuText = "Template PV - NAV"; ;
			navNew.PageHead = "Template Preview - HEAD";
			navNew.PageActive = true;
			navNew.ShowInSiteNav = true;
			navNew.ShowInSiteMap = true;

			navNew.EditDate = DateTime.Now.Date.AddHours(-8);
			navNew.CreateDate = DateTime.Now.Date.AddHours(-38);
			navNew.GoLiveDate = navNew.EditDate.AddHours(-5);
			navNew.RetireDate = navNew.CreateDate.AddYears(5);
			navNew.PageText = "<h2>Content CENTER</h2>\r\n";

			navNew.TemplateFile = SiteData.PreviewTemplateFile;

			if (SiteData.IsWebView) {
				navNew.FileName = SiteData.PreviewTemplateFilePage + "?" + HttpContext.Current.Request.QueryString.ToString();
			} else {
				navNew.FileName = SiteData.PreviewTemplateFilePage + "?sampler=true";
			}

			navNew.PageText = "<h2>Content CENTER</h2>\r\n" + sFile2;

			navNew.SiteID = SiteData.CurrentSiteID;
			navNew.Parent_ContentID = null;
			navNew.ContentType = ContentPageType.PageType.ContentEntry;

			navNew.EditUserId = SecurityData.CurrentUserGuid;

			return navNew;
		}

		#region IDisposable Members

		public void Dispose() {
			if (_navHelper != null && _navHelper is IDisposable) {
				((IDisposable)_navHelper).Dispose();
			}
		}

		#endregion IDisposable Members
	}
}