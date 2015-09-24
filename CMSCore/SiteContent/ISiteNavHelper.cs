using Carrotware.CMS.Data;
using System;
using System.Collections.Generic;
using System.Linq;

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

	public interface ISiteNavHelper {

		SiteNav FindByFilename(Guid siteID, string urlFileName);

		SiteNav FindHome(Guid siteID);

		SiteNav FindHome(Guid siteID, bool bActiveOnly);

		List<SiteNav> GetChildNavigation(Guid siteID, Guid? parentPageID, bool bActiveOnly);

		List<SiteNav> GetChildNavigation(Guid siteID, string parentPage, bool bActiveOnly);

		int GetChildNavigationCount(Guid siteID, Guid? parentPageID, bool bActiveOnly);

		int GetChildNavigationCount(Guid siteID, string parentPage, bool bActiveOnly);

		int GetFilteredContentPagedCount(SiteData currentSite, string sFilterPath, bool bActiveOnly);

		List<SiteNav> GetFilteredContentPagedList(SiteData currentSite, string sFilterPath, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir);

		int GetFilteredContentByIDPagedCount(SiteData currentSite, List<Guid> lstCategories, bool bActiveOnly);

		List<SiteNav> GetFilteredContentByIDPagedList(SiteData currentSite, List<Guid> lstCategories, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir);

		int GetFilteredContentByIDPagedCount(SiteData currentSite, List<Guid> lstCategoryGUIDs, List<string> lstCategorySlugs, bool bActiveOnly);

		List<SiteNav> GetFilteredContentByIDPagedList(SiteData currentSite, List<Guid> lstCategoryGUIDs, List<string> lstCategorySlugs, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir);

		string GetBlogHeadingFromURL(SiteData currentSite, string sFilterPath);

		List<SiteNav> GetLatest(Guid siteID, int iUpdates, bool bActiveOnly);

		List<SiteNav> GetLatestPosts(Guid siteID, int iUpdates, bool bActiveOnly);

		List<SiteNav> GetLatestUpdates(Guid siteID, int iUpdates, bool bActiveOnly);

		List<SiteNav> GetLatestPostUpdates(Guid siteID, int iUpdates, bool bActiveOnly);

		List<ContentTag> GetTagList(Guid siteID, int iUpdates);

		List<ContentCategory> GetCategoryList(Guid siteID, int iUpdates);

		List<ContentDateTally> GetMonthBlogUpdateList(Guid siteID, int iUpdates, bool bActiveOnly);

		List<ContentDateLinks> GetSingleMonthBlogUpdateList(SiteData currentSite, DateTime monthDate, bool bActiveOnly);

		List<ContentTag> GetTagListForPost(Guid siteID, int iUpdates, string urlFileName);

		List<ContentCategory> GetCategoryListForPost(Guid siteID, int iUpdates, string urlFileName);

		List<ContentTag> GetTagListForPost(Guid siteID, int iUpdates, Guid rootContentID);

		List<ContentCategory> GetCategoryListForPost(Guid siteID, int iUpdates, Guid rootContentID);

		SiteNav GetPrevPost(Guid siteID, Guid rootContentID, bool bActiveOnly);

		SiteNav GetNextPost(Guid siteID, Guid rootContentID, bool bActiveOnly);

		List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageNumber);

		List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageNumber, string sortField, string sortDir);

		List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber);

		List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir);

		List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageNumber);

		List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageNumber, string sortField, string sortDir);

		List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageSize, int pageNumber);

		List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir);

		List<SiteNav> GetLatestChildContentPagedList(Guid siteID, Guid? parentPageID, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir);

		List<SiteNav> GetLatestChildContentPagedList(Guid siteID, string parentPage, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir);

		List<SiteNav> GetLatestContentSearchList(Guid siteID, string searchTerm, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir);

		SiteNav GetLatestVersion(Guid siteID, bool bActiveOnly, string sPage);

		SiteNav GetLatestVersion(Guid siteID, Guid rootContentID);

		List<SiteNav> GetLevelDepthNavigation(Guid siteID, int iDepth, bool bActiveOnly);

		List<SiteNav> GetMasterNavigation(Guid siteID, bool bActiveOnly);

		List<SiteNav> GetPageCrumbNavigation(Guid siteID, Guid rootContentID, bool bActiveOnly);

		List<SiteNav> GetPageCrumbNavigation(Guid siteID, string sPage, bool bActiveOnly);

		SiteNav GetPageNavigation(Guid siteID, Guid rootContentID);

		SiteNav GetPageNavigation(Guid siteID, string sPage);

		SiteNav GetParentPageNavigation(Guid siteID, Guid rootContentID);

		SiteNav GetParentPageNavigation(Guid siteID, string sPage);

		List<SiteNav> GetSiblingNavigation(Guid siteID, Guid PageID, bool bActiveOnly);

		List<SiteNav> GetSiblingNavigation(Guid siteID, string sPage, bool bActiveOnly);

		int GetSiteContentCount(Guid siteID);

		int GetSitePageCount(Guid siteID, ContentPageType.PageType entryType);

		int GetSitePageCount(Guid siteID, ContentPageType.PageType entryType, bool bActiveOnly);

		int GetSiteSearchCount(Guid siteID, string searchTerm, bool bActiveOnly);

		List<SiteNav> GetTopNavigation(Guid siteID, bool bActiveOnly);

		List<SiteNav> GetTwoLevelNavigation(Guid siteID, bool bActiveOnly);

		List<SiteNav> PerformDataPagingQueryableContent(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir, IQueryable<vw_carrot_Content> QueryInput);
	}
}