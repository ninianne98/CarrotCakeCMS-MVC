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

namespace Carrotware.CMS.Core {

	public class SiteNavHelperMock : ISiteNavHelper {

		public SiteNavHelperMock() { }

		public List<SiteNav> GetMasterNavigation(Guid siteID, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerFakeNav();
		}

		public List<SiteNav> GetTopNavigation(Guid siteID, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerFakeNav();
		}

		public List<SiteNav> GetTwoLevelNavigation(Guid siteID, bool bActiveOnly) {
			List<SiteNav> lstNav = SiteNavHelper.GetSamplerFakeNav();
			List<SiteNav> lstNav2 = new List<SiteNav>();

			foreach (SiteNav l in lstNav) {
				lstNav2 = lstNav2.Union(SiteNavHelper.GetSamplerFakeNav(l.Root_ContentID)).ToList();
			}

			lstNav = lstNav.Union(lstNav2).ToList();
			return lstNav;
		}

		public List<SiteNav> GetLevelDepthNavigation(Guid siteID, int iDepth, bool bActiveOnly) {
			List<SiteNav> lstNav = SiteNavHelper.GetSamplerFakeNav();
			List<SiteNav> lstNav2 = new List<SiteNav>();

			foreach (SiteNav l1 in lstNav) {
				List<SiteNav> lst = SiteNavHelper.GetSamplerFakeNav(l1.Root_ContentID);
				lstNav2 = lstNav2.Union(lst).ToList();

				foreach (SiteNav l2 in lst) {
					List<SiteNav> lst2 = SiteNavHelper.GetSamplerFakeNav(l2.Root_ContentID);
					lstNav2 = lstNav2.Union(lst2).ToList();
				}
			}

			lstNav = lstNav.Union(lstNav2).ToList();
			return lstNav;
		}

		public List<ContentCategory> GetCategoryList(Guid siteID, int iUpdates) {
			List<int> pagelist = Enumerable.Range(1, iUpdates).ToList();

			List<ContentCategory> lstContent = (from ct in pagelist
												orderby ct descending
												select new ContentCategory {
													SiteID = Guid.NewGuid(),
													CategoryURL = String.Format("#/archive/keyword/cat{0}", ct),
													CategoryText = String.Format("Meta Info Cat {0}", ct),
													UseCount = ct + 2,
													PublicUseCount = ct + 3
												}).ToList();

			return lstContent;
		}

		public List<ContentTag> GetTagList(Guid siteID, int iUpdates) {
			List<int> pagelist = Enumerable.Range(1, iUpdates).ToList();

			List<ContentTag> lstContent = (from ct in pagelist
										   orderby ct descending
										   select new ContentTag {
											   SiteID = Guid.NewGuid(),
											   TagURL = String.Format("#/archive/keyword/tag{0}", ct),
											   TagText = String.Format("Meta Info Tag {0}", ct),
											   UseCount = ct + 2,
											   PublicUseCount = ct + 3
										   }).ToList();

			return lstContent;
		}

		public List<ContentTag> GetTagListForPost(Guid siteID, int iUpdates, string urlFileName) {
			return GetTagList(siteID, 3);
		}

		public List<ContentCategory> GetCategoryListForPost(Guid siteID, int iUpdates, string urlFileName) {
			return GetCategoryList(siteID, 5);
		}

		public List<ContentTag> GetTagListForPost(Guid siteID, int iUpdates, Guid rootContentID) {
			return GetTagList(siteID, 3);
		}

		public List<ContentCategory> GetCategoryListForPost(Guid siteID, int iUpdates, Guid rootContentID) {
			return GetCategoryList(siteID, 5);
		}

		public List<ContentDateLinks> GetSingleMonthBlogUpdateList(SiteData currentSite, DateTime monthDate, bool bActiveOnly) {
			List<ContentDateLinks> lstContent = new List<ContentDateLinks>();
			int n = 0;
			monthDate = monthDate.AddDays(0 - monthDate.Day).AddDays(1);
			DateTime dateNow = monthDate;

			while (n < 28) {
				dateNow = monthDate.AddDays(n);

				ContentDateLinks cc = new ContentDateLinks();
				cc.TheSite = currentSite;
				cc.UseCount = n;
				cc.PostDate = dateNow;
				lstContent.Add(cc);

				n = n + 3;
			}

			return lstContent;
		}

		public List<ContentDateTally> GetMonthBlogUpdateList(Guid siteID, int iUpdates, bool bActiveOnly) {
			List<ContentDateTally> lstContent = new List<ContentDateTally>();
			int n = 0;
			DateTime dateNow = DateTime.UtcNow.Date;

			while (n < iUpdates) {
				dateNow = DateTime.UtcNow.Date.AddMonths(0 - n);

				ContentDateTally cc = new ContentDateTally {
					DateCaption = dateNow.ToString("MMMM yyyy"),
					TallyDate = Convert.ToDateTime(dateNow),
					UseCount = Convert.ToInt32(n * 2),
					TheSite = SiteData.CurrentSite
				};

				lstContent.Add(cc);
				n++;
			}

			return lstContent;
		}

		public List<SiteNav> GetPageCrumbNavigation(Guid siteID, Guid rootContentID, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerFakeNav(rootContentID);
		}

		public List<SiteNav> GetPageCrumbNavigation(Guid siteID, string sPage, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerFakeNav(Guid.NewGuid());
		}

		public List<SiteNav> GetChildNavigation(Guid siteID, string sparentPageID, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerFakeNav(Guid.NewGuid());
		}

		public List<SiteNav> GetSiblingNavigation(Guid siteID, string sPage, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerFakeNav(Guid.NewGuid());
		}

		public int GetChildNavigationCount(Guid siteID, Guid? parentPageID, bool bActiveOnly) {
			return 15;
		}

		public int GetChildNavigationCount(Guid siteID, string sParentPage, bool bActiveOnly) {
			return 15;
		}

		public SiteNav GetPageNavigation(Guid siteID, string sPage) {
			return SiteNavHelper.GetSamplerView();
		}

		public SiteNav GetPageNavigation(Guid siteID, Guid rootContentID) {
			return SiteNavHelper.GetSamplerView(rootContentID);
		}

		public SiteNav GetParentPageNavigation(Guid siteID, string sPage) {
			return SiteNavHelper.GetSamplerView();
		}

		public SiteNav GetParentPageNavigation(Guid siteID, Guid rootContentID) {
			return SiteNavHelper.GetSamplerView();
		}

		public SiteNav GetPrevPost(Guid siteID, Guid rootContentID, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerView();
		}

		public SiteNav GetNextPost(Guid siteID, Guid rootContentID, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerView();
		}

		public List<SiteNav> GetChildNavigation(Guid siteID, Guid? parentPageID, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerFakeNav(parentPageID);
		}

		public List<SiteNav> GetSiblingNavigation(Guid siteID, Guid PageID, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerFakeNav(PageID);
		}

		public List<SiteNav> GetLatest(Guid siteID, int iUpdates, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerFakeNav(iUpdates);
		}

		public List<SiteNav> GetLatestPosts(Guid siteID, int iUpdates, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerFakeNav(iUpdates);
		}

		public List<SiteNav> GetLatestUpdates(Guid siteID, int iUpdates, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerFakeNav(iUpdates);
		}

		public List<SiteNav> GetLatestPostUpdates(Guid siteID, int iUpdates, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerFakeNav(iUpdates);
		}

		public SiteNav GetLatestVersion(Guid siteID, Guid rootContentID) {
			return SiteNavHelper.GetSamplerView();
		}

		public SiteNav GetLatestVersion(Guid siteID, bool bActiveOnly, string sPage) {
			return SiteNavHelper.GetSamplerView();
		}

		public SiteNav FindHome(Guid siteID) {
			return SiteNavHelper.GetSamplerView();
		}

		public SiteNav FindByFilename(Guid siteID, string urlFileName) {
			return SiteNavHelper.GetSamplerView();
		}

		public SiteNav FindHome(Guid siteID, bool bActiveOnly) {
			return SiteNavHelper.GetSamplerView();
		}

		public int GetFilteredContentPagedCount(SiteData currentSite, string sFilterPath, bool bActiveOnly) {
			return 50;
		}

		public int GetFilteredContentByIDPagedCount(SiteData currentSite, List<Guid> lstCategories, bool bActiveOnly) {
			return 50;
		}

		public int GetFilteredContentByIDPagedCount(SiteData currentSite, List<Guid> lstCategoryGUIDs, List<string> lstCategorySlugs, bool bActiveOnly) {
			return 50;
		}

		public int GetSiteSearchCount(Guid siteID, string searchTerm, bool bActiveOnly) {
			return 50;
		}

		public List<SiteNav> GetLatestContentSearchList(Guid siteID, string searchTerm, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return SiteNavHelper.GetSamplerFakeNav(pageSize);
		}

		public string GetBlogHeadingFromURL(SiteData currentSite, string sFilterPath) {
			string sTitle = String.Empty;

			if (currentSite.CheckIsBlogCategoryPath(sFilterPath)) {
				sTitle = "Category 1";
			}
			if (currentSite.CheckIsBlogTagPath(sFilterPath)) {
				sTitle = "Tag 1";
			}
			if (currentSite.CheckIsBlogEditorFolderPath(sFilterPath)) {
				sTitle = "Editor 1";
			}
			if (currentSite.CheckIsBlogDateFolderPath(sFilterPath)) {
				sTitle = DateTime.UtcNow.ToString("MMMM yyyy");
			}
			if (currentSite.CheckIsSiteSearchPath(sFilterPath)) {
				sTitle = "Search Results";
			}

			return sTitle;
		}

		public List<SiteNav> GetFilteredContentPagedList(SiteData currentSite, string sFilterPath, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return SiteNavHelper.GetSamplerFakeNav(pageSize);
		}

		public List<SiteNav> GetFilteredContentByIDPagedList(SiteData currentSite, List<Guid> lstCategories, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return SiteNavHelper.GetSamplerFakeNav(pageSize);
		}

		public List<SiteNav> GetFilteredContentByIDPagedList(SiteData currentSite, List<Guid> lstCategoryGUIDs, List<string> lstCategorySlugs, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return SiteNavHelper.GetSamplerFakeNav(pageSize);
		}

		public List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageNumber) {
			return SiteNavHelper.GetSamplerFakeNav(10);
		}

		public List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageNumber, string sortField, string sortDir) {
			return SiteNavHelper.GetSamplerFakeNav(10);
		}

		public List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber) {
			return SiteNavHelper.GetSamplerFakeNav(pageSize);
		}

		public List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return SiteNavHelper.GetSamplerFakeNav(pageSize);
		}

		public List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageNumber) {
			return SiteNavHelper.GetSamplerFakeNav(10);
		}

		public List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageNumber, string sortField, string sortDir) {
			return SiteNavHelper.GetSamplerFakeNav(10);
		}

		public List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageSize, int pageNumber) {
			return SiteNavHelper.GetSamplerFakeNav(pageSize);
		}

		public List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return SiteNavHelper.GetSamplerFakeNav(pageSize);
		}

		public List<SiteNav> GetLatestChildContentPagedList(Guid siteID, Guid? parentContentID, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return SiteNavHelper.GetSamplerFakeNav(pageSize, parentContentID);
		}

		public List<SiteNav> GetLatestChildContentPagedList(Guid siteID, string parentPage, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return SiteNavHelper.GetSamplerFakeNav(pageSize, Guid.NewGuid());
		}

		public int GetSiteContentCount(Guid siteID) {
			return 50;
		}

		public int GetSitePageCount(Guid siteID, ContentPageType.PageType entryType) {
			return 50;
		}

		public int GetSitePageCount(Guid siteID, ContentPageType.PageType entryType, bool bActiveOnly) {
			return 50;
		}

		public List<SiteNav> PerformDataPagingQueryableContent(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir, IQueryable<Data.vw_carrot_Content> QueryInput) {
			return SiteNavHelper.GetSamplerFakeNav(pageSize);
		}

		public void Dispose() { }
	}
}