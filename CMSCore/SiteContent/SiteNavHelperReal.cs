using Carrotware.CMS.Data;
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

namespace Carrotware.CMS.Core {

	public class SiteNavHelperReal : IDisposable, ISiteNavHelper {
		private CarrotCMSDataContext db = CarrotCMSDataContext.Create();

		public SiteNavHelperReal() { }

		public List<SiteNav> GetMasterNavigation(Guid siteID, bool bActiveOnly) {
			List<SiteNav> lstContent = (from ct in CannedQueries.GetLatestContentList(db, siteID, bActiveOnly)
										select new SiteNav(ct)).ToList();

			return lstContent;
		}

		public List<SiteNav> GetTwoLevelNavigation(Guid siteID, bool bActiveOnly) {
			List<SiteNav> lstContent = null;

			List<Guid> lstTop = CompiledQueries.TopLevelPages(db, siteID, false).Select(z => z.Root_ContentID).ToList();

			lstContent = (from ct in CannedQueries.GetLatestContentList(db, siteID, bActiveOnly)
						  orderby ct.NavOrder, ct.NavMenuText
						  where ct.SiteID == siteID
								&& (ct.PageActive == true || bActiveOnly == false)
								&& (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
								&& (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
								&& ct.IsLatestVersion == true
								&& (lstTop.Contains(ct.Root_ContentID) || lstTop.Contains(ct.Parent_ContentID.Value))
						  select new SiteNav(ct)).ToList();

			return lstContent;
		}

		public List<SiteNav> GetLevelDepthNavigation(Guid siteID, int iDepth, bool bActiveOnly) {
			List<SiteNav> lstContent = null;
			List<Guid> lstSub = new List<Guid>();

			if (iDepth < 1) {
				iDepth = 1;
			}

			if (iDepth > 10) {
				iDepth = 10;
			}

			List<Guid> lstTop = CompiledQueries.TopLevelPages(db, siteID, false).Select(z => z.Root_ContentID).ToList();

			while (iDepth > 1) {
				lstSub = (from ct in CannedQueries.GetLatestContentList(db, siteID, bActiveOnly)
						  where ct.SiteID == siteID
								&& ct.ShowInSiteNav == true
								&& (ct.PageActive == true || bActiveOnly == false)
								&& ct.IsLatestVersion == true
								&& (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
								&& (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
								&& (!lstTop.Contains(ct.Root_ContentID) && lstTop.Contains(ct.Parent_ContentID.Value))
						  select ct.Root_ContentID).Distinct().ToList();

				lstTop = lstTop.Union(lstSub).ToList();

				iDepth--;
			}

			lstContent = (from ct in CannedQueries.GetLatestContentList(db, siteID, bActiveOnly)
						  orderby ct.NavOrder, ct.NavMenuText
						  where ct.SiteID == siteID
								&& ct.ShowInSiteNav == true
								&& (ct.PageActive == true || bActiveOnly == false)
								&& ct.IsLatestVersion == true
								&& (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
								&& (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
								&& lstTop.Contains(ct.Root_ContentID)
						  select new SiteNav(ct)).ToList();

			return lstContent;
		}

		public List<SiteNav> GetTopNavigation(Guid siteID, bool bActiveOnly) {
			List<SiteNav> lstContent = CompiledQueries.TopLevelPages(db, siteID, bActiveOnly).Select(ct => new SiteNav(ct)).ToList();

			return lstContent;
		}

		private SiteNav GetPageNavigation(Guid siteID, Guid rootContentID, bool bActiveOnly) {
			SiteNav content = null;
			vw_carrot_Content cont = CompiledQueries.GetLatestContentByID(db, siteID, bActiveOnly, rootContentID);
			if (cont != null) {
				content = new SiteNav(cont);
			}
			return content;
		}

		private List<SiteNav> GetPageCrumbNavByContent(vw_carrot_Content content, bool bActiveOnly) {
			Guid siteID = Guid.Empty;
			Guid rootContentID = Guid.Empty;
			Guid? parentContentID = null;
			int iOrder = 1000000;

			List<SiteNav> lstContent = new List<SiteNav>();

			if (content != null) {
				rootContentID = content.Root_ContentID;
				siteID = content.SiteID;
				parentContentID = content.Parent_ContentID;
				SiteNav sn = new SiteNav(content);
				sn.NavOrder = iOrder + 100;
				lstContent.Add(sn);
			}

			if (rootContentID != Guid.Empty) {
				Guid? gLast = parentContentID;

				while (gLast.HasValue) {
					SiteNav nav = GetPageNavigation(siteID, gLast.Value, false);
					gLast = null;

					if (nav != null) {
						nav.NavOrder = iOrder;
						lstContent.Add(nav);
						iOrder--;

						gLast = nav.Parent_ContentID;
					}
				}
			}

			SiteNav home = FindHome(siteID, false);
			if (home != null) {
				home.NavOrder = 0;

				if (!lstContent.Where(x => x.Root_ContentID == home.Root_ContentID).Any()) {
					lstContent.Add(home);
				}
			}

			return lstContent.OrderBy(x => x.NavOrder).Where(x => x.PageActive == true && x.IsRetired == false && x.IsUnReleased == false || bActiveOnly == false).ToList();
		}

		public List<SiteNav> GetPageCrumbNavigation(Guid siteID, Guid rootContentID, bool bActiveOnly) {
			vw_carrot_Content c = CompiledQueries.GetLatestContentByID(db, siteID, false, rootContentID);

			if (c != null) {
				return GetPageCrumbNavByContent(c, bActiveOnly);
			} else {
				return new List<SiteNav>();
			}
		}

		public List<SiteNav> GetPageCrumbNavigation(Guid siteID, string sPage, bool bActiveOnly) {
			vw_carrot_Content c = CompiledQueries.GetLatestContentByURL(db, siteID, false, sPage);

			if (c != null) {
				return GetPageCrumbNavByContent(c, bActiveOnly);
			} else {
				return new List<SiteNav>();
			}
		}

		public List<SiteNav> GetChildNavigation(Guid siteID, string parentPage, bool bActiveOnly) {
			List<SiteNav> lstContent = (from ct in CompiledQueries.GetLatestContentByParent(db, siteID, parentPage, bActiveOnly).ToList()
										select new SiteNav(ct)).ToList();

			return lstContent;
		}

		public List<SiteNav> GetChildNavigation(Guid siteID, Guid? parentPageID, bool bActiveOnly) {
			List<SiteNav> lstContent = (from ct in CompiledQueries.GetLatestContentByParent(db, siteID, parentPageID, bActiveOnly).ToList()
										select new SiteNav(ct)).ToList();

			return lstContent;
		}

		public int GetChildNavigationCount(Guid siteID, string parentPage, bool bActiveOnly) {
			return CompiledQueries.GetContentCountByParent(db, siteID, parentPage, bActiveOnly);
		}

		public int GetChildNavigationCount(Guid siteID, Guid? parentPageID, bool bActiveOnly) {
			return CompiledQueries.GetContentCountByParent(db, siteID, parentPageID, bActiveOnly);
		}

		public SiteNav GetPageNavigation(Guid siteID, string sPage) {
			SiteNav content = null;
			vw_carrot_Content cont = CompiledQueries.GetLatestContentByURL(db, siteID, false, sPage);
			if (cont != null) {
				content = new SiteNav(cont);
			}
			if (content == null && sPage == SiteData.DefaultDirectoryFilename) {
				content = FindHome(siteID, false);
			}
			return content;
		}

		public SiteNav GetPageNavigation(Guid siteID, Guid rootContentID) {
			SiteNav content = null;
			vw_carrot_Content cont = CompiledQueries.GetLatestContentByID(db, siteID, false, rootContentID);
			if (cont != null) {
				content = new SiteNav(cont);
			}
			return content;
		}

		public SiteNav GetParentPageNavigation(Guid siteID, string sPage) {
			SiteNav nav = GetPageNavigation(siteID, sPage);

			return GetParentPageNavigationByNav(siteID, nav);
		}

		public SiteNav GetParentPageNavigation(Guid siteID, Guid rootContentID) {
			SiteNav nav = GetPageNavigation(siteID, rootContentID);

			return GetParentPageNavigationByNav(siteID, nav);
		}

		private SiteNav GetParentPageNavigationByNav(Guid siteID, SiteNav navItem) {
			if (navItem != null) {
				if (navItem.ContentType == ContentPageType.PageType.BlogEntry) {
					Guid? parentPageID = SiteData.GetSiteFromCache(siteID).Blog_Root_ContentID;
					navItem.Parent_ContentID = parentPageID;
				}

				SiteNav content = null;
				if (navItem != null && navItem.Parent_ContentID.HasValue) {
					content = new SiteNav(CompiledQueries.GetLatestContentByID(db, siteID, false, navItem.Parent_ContentID.Value));
				}

				return content;
			} else {
				return null;
			}
		}

		public SiteNav GetPrevPost(Guid siteID, Guid rootContentID, bool bActiveOnly) {
			vw_carrot_Content c = CompiledQueries.GetPreviousPost(db, siteID, bActiveOnly, rootContentID);

			if (c != null) {
				return new SiteNav(c);
			} else {
				return null;
			}
		}

		public SiteNav GetNextPost(Guid siteID, Guid rootContentID, bool bActiveOnly) {
			vw_carrot_Content c = CompiledQueries.GetNextPost(db, siteID, bActiveOnly, rootContentID);

			if (c != null) {
				return new SiteNav(c);
			} else {
				return null;
			}
		}

		public List<SiteNav> GetSiblingNavigation(Guid siteID, Guid rootContentID, bool bActiveOnly) {
			vw_carrot_Content cont = CompiledQueries.GetLatestContentByID(db, siteID, false, rootContentID);

			return (from ct in CannedQueries.GetContentByParent(db, siteID, cont.Parent_ContentID, bActiveOnly)
					orderby ct.NavOrder ascending
					select new SiteNav(ct)).ToList();
		}

		public List<SiteNav> GetSiblingNavigation(Guid siteID, string sPage, bool bActiveOnly) {
			vw_carrot_Content cont = CompiledQueries.GetLatestContentByURL(db, siteID, false, sPage);

			return (from ct in CannedQueries.GetContentByParent(db, siteID, cont.Parent_ContentID, bActiveOnly)
					orderby ct.NavOrder ascending
					select new SiteNav(ct)).ToList();
		}

		public List<SiteNav> GetLatest(Guid siteID, int iUpdates, bool bActiveOnly) {
			List<SiteNav> lstContent = (from ct in CannedQueries.GetLatestContentList(db, siteID, bActiveOnly)
										orderby ct.GoLiveDate descending
										select new SiteNav(ct)).Take(iUpdates).ToList();

			return lstContent;
		}

		public List<SiteNav> GetLatestPosts(Guid siteID, int iUpdates, bool bActiveOnly) {
			List<SiteNav> lstContent = (from ct in CannedQueries.GetLatestBlogList(db, siteID, bActiveOnly)
										orderby ct.GoLiveDate descending
										select new SiteNav(ct)).Take(iUpdates).ToList();

			return lstContent;
		}

		public List<SiteNav> GetLatestUpdates(Guid siteID, int iUpdates, bool bActiveOnly) {
			List<SiteNav> lstContent = (from ct in CannedQueries.GetLatestContentList(db, siteID, bActiveOnly)
										orderby ct.EditDate descending
										select new SiteNav(ct)).Take(iUpdates).ToList();

			return lstContent;
		}

		public List<SiteNav> GetLatestPostUpdates(Guid siteID, int iUpdates, bool bActiveOnly) {
			List<SiteNav> lstContent = (from ct in CannedQueries.GetLatestBlogList(db, siteID, bActiveOnly)
										orderby ct.EditDate descending
										select new SiteNav(ct)).Take(iUpdates).ToList();

			return lstContent;
		}

		public List<ContentTag> GetTagList(Guid siteID, int iUpdates) {
			List<ContentTag> lstContent = (from ct in CannedQueries.GetTagURLs(db, siteID)
										   where ct.IsPublic == true
										   orderby ct.UseCount descending
										   select new ContentTag(ct)).Take(iUpdates).ToList();

			return lstContent;
		}

		public List<ContentCategory> GetCategoryList(Guid siteID, int iUpdates) {
			List<ContentCategory> lstContent = (from ct in CannedQueries.GetCategoryURLs(db, siteID)
												where ct.IsPublic == true
												orderby ct.UseCount descending
												select new ContentCategory(ct)).Take(iUpdates).ToList();

			return lstContent;
		}

		public List<ContentDateTally> GetMonthBlogUpdateList(Guid siteID, int iUpdates, bool bActiveOnly) {
			SiteData site = SiteData.GetSiteFromCache(siteID);

			List<carrot_ContentTally> lstContentTally = db.carrot_BlogMonthlyTallies(siteID, bActiveOnly, iUpdates).ToList();

			List<ContentDateTally> lstContent = (from ct in lstContentTally
												 orderby ct.DateMonth descending
												 select (new ContentDateTally {
													 DateCaption = ct.DateMonth.HasValue ? string.Format("{0:MMMM} {0:yyyy}", ct.DateMonth.Value) : ct.DateSlug,
													 TallyDate = ct.DateMonth ?? DateTime.Now,
													 UseCount = ct.ContentCount ?? 0,
													 TheSite = site
												 })).ToList();

			return lstContent;
		}

		public List<ContentTag> GetTagListForPost(Guid siteID, int iUpdates, string urlFileName) {
			List<ContentTag> lstContent = (from ct in CannedQueries.GetPostTagURLs(db, siteID, urlFileName)
										   where ct.IsPublic == true
										   orderby ct.TagText
										   select new ContentTag(ct)).Take(iUpdates).ToList();

			return lstContent;
		}

		public List<ContentCategory> GetCategoryListForPost(Guid siteID, int iUpdates, string urlFileName) {
			List<ContentCategory> lstContent = (from ct in CannedQueries.GetPostCategoryURL(db, siteID, urlFileName)
												where ct.IsPublic == true
												orderby ct.CategoryText
												select new ContentCategory(ct)).Take(iUpdates).ToList();

			return lstContent;
		}

		public List<ContentTag> GetTagListForPost(Guid siteID, int iUpdates, Guid rootContentID) {
			List<ContentTag> lstContent = (from ct in CannedQueries.GetPostTagURLs(db, siteID, rootContentID)
										   where ct.IsPublic == true
										   orderby ct.TagText
										   select new ContentTag(ct)).Take(iUpdates).ToList();

			return lstContent;
		}

		public List<ContentCategory> GetCategoryListForPost(Guid siteID, int iUpdates, Guid rootContentID) {
			List<ContentCategory> lstContent = (from ct in CannedQueries.GetPostCategoryURL(db, siteID, rootContentID)
												where ct.IsPublic == true
												orderby ct.CategoryText
												select new ContentCategory(ct)).Take(iUpdates).ToList();

			return lstContent;
		}

		public SiteNav GetLatestVersion(Guid siteID, Guid rootContentID) {
			SiteNav content = null;
			vw_carrot_Content cont = CompiledQueries.GetLatestContentByID(db, siteID, false, rootContentID);
			if (cont != null) {
				content = new SiteNav(cont);
			}
			return content;
		}

		public SiteNav GetLatestVersion(Guid siteID, bool bActiveOnly, string sPage) {
			SiteNav content = null;
			vw_carrot_Content cont = CompiledQueries.GetLatestContentByURL(db, siteID, bActiveOnly, sPage);
			if (cont != null) {
				content = new SiteNav(cont);
			}
			if (content == null && sPage == SiteData.DefaultDirectoryFilename) {
				content = FindHome(siteID, bActiveOnly);
			}
			return content;
		}

		public SiteNav FindByFilename(Guid siteID, string urlFileName) {
			SiteNav content = null;
			vw_carrot_Content cont = CompiledQueries.GetLatestContentByURL(db, siteID, false, urlFileName);
			if (cont != null) {
				content = new SiteNav(cont);
			}
			return content;
		}

		public SiteNav FindHome(Guid siteID) {
			SiteNav content = null;
			vw_carrot_Content cont = CompiledQueries.FindHome(db, siteID, true);
			if (cont != null) {
				content = new SiteNav(cont);
			}
			return content;
		}

		public SiteNav FindHome(Guid siteID, bool bActiveOnly) {
			SiteNav content = null;
			vw_carrot_Content cont = CompiledQueries.FindHome(db, siteID, bActiveOnly);
			if (cont != null) {
				content = new SiteNav(cont);
			}
			return content;
		}

		public List<ContentDateLinks> GetSingleMonthBlogUpdateList(SiteData currentSite, DateTime monthDate, bool bActiveOnly) {
			List<ContentDateLinks> lstContent = new List<ContentDateLinks>();
			DateTime dateBegin = monthDate.AddDays(0 - monthDate.Day).AddDays(1);
			DateTime dateEnd = dateBegin.AddMonths(1).AddMilliseconds(-1);

			if (currentSite != null) {
				dateBegin = currentSite.ConvertSiteTimeToUTC(dateBegin);
				dateEnd = currentSite.ConvertSiteTimeToUTC(dateEnd);
			}

			IQueryable<vw_carrot_Content> query1 = CannedQueries.GetLatestBlogListDateRange(db, currentSite.SiteID, dateBegin, dateEnd, bActiveOnly);

			lstContent = (from p in query1
						  group p by p.GoLiveDateLocal.Date into g
						  select new ContentDateLinks { PostDate = g.Key, UseCount = g.Count() }).ToList();

			lstContent.ToList().ForEach(q => q.TheSite = currentSite);

			return lstContent;
		}

		public int GetSitePageCount(Guid siteID, ContentPageType.PageType entryType, bool bActiveOnly) {
			int iCount = CannedQueries.GetAllByTypeList(db, siteID, bActiveOnly, entryType).Count();
			return iCount;
		}

		public int GetSitePageCount(Guid siteID, ContentPageType.PageType entryType) {
			int iCount = CannedQueries.GetAllByTypeList(db, siteID, false, entryType).Count();
			return iCount;
		}

		public int GetSiteContentCount(Guid siteID) {
			int iCount = CannedQueries.GetLatestContentList(db, siteID, false).Count();
			return iCount;
		}

		public List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageNumber, string sortField, string sortDir) {
			return GetLatestContentPagedList(siteID, ContentPageType.PageType.BlogEntry, bActiveOnly, pageNumber, sortField, sortDir);
		}

		public List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageNumber) {
			return GetLatestContentPagedList(siteID, ContentPageType.PageType.BlogEntry, bActiveOnly, pageNumber);
		}

		public List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber) {
			return GetLatestContentPagedList(siteID, ContentPageType.PageType.BlogEntry, bActiveOnly, pageSize, pageNumber);
		}

		public List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageNumber, string sortField, string sortDir) {
			return GetLatestContentPagedList(siteID, postType, bActiveOnly, 10, pageNumber, sortField, sortDir);
		}

		public List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageNumber) {
			return GetLatestContentPagedList(siteID, postType, bActiveOnly, 10, pageNumber, "", "");
		}

		public List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageSize, int pageNumber) {
			return GetLatestContentPagedList(siteID, postType, bActiveOnly, pageSize, pageNumber, "", "");
		}

		public List<SiteNav> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return GetLatestContentPagedList(siteID, ContentPageType.PageType.BlogEntry, bActiveOnly, pageSize, pageNumber, sortField, sortDir);
		}

		public List<SiteNav> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly,
					int pageSize, int pageNumber, string sortField, string sortDir) {
			IQueryable<vw_carrot_Content> query1 = null;

			if (postType == ContentPageType.PageType.ContentEntry) {
				query1 = CannedQueries.GetLatestContentList(db, siteID, bActiveOnly);
			} else {
				query1 = CannedQueries.GetLatestBlogList(db, siteID, bActiveOnly);
			}

			return PerformDataPagingQueryableContent(siteID, bActiveOnly, pageSize, pageNumber, sortField, sortDir, query1);
		}

		public string GetBlogHeadingFromURL(SiteData currentSite, string sFilterPath) {
			Guid siteID = currentSite.SiteID;

			string sTitle = string.Empty;

			if (currentSite.CheckIsBlogCategoryPath(sFilterPath)) {
				vw_carrot_CategoryURL query = CompiledQueries.cqGetCategoryByURL(db, siteID, sFilterPath);
				sTitle = query.CategoryText;
			}
			if (currentSite.CheckIsBlogTagPath(sFilterPath)) {
				vw_carrot_TagURL query = CompiledQueries.cqGetTagByURL(db, siteID, sFilterPath);
				sTitle = query.TagText;
			}
			if (currentSite.CheckIsBlogEditorFolderPath(sFilterPath)) {
				vw_carrot_EditorURL query = CompiledQueries.cqGetEditorByURL(db, siteID, sFilterPath);
				ExtendedUserData usr = new ExtendedUserData(query.UserId);
				sTitle = usr.ToString();
			}
			if (currentSite.CheckIsBlogDateFolderPath(sFilterPath)) {
				BlogDatePathParser p = new BlogDatePathParser(currentSite, sFilterPath);
				TimeSpan ts = p.DateEndUTC - p.DateBeginUTC;

				int daysDelta = ts.Days;
				if (daysDelta > 90) {
					sTitle = "Year " + p.DateBegin.ToString("yyyy");
				}
				if (daysDelta < 36) {
					sTitle = p.DateBegin.ToString("MMMM yyyy");
				}
				if (daysDelta < 5) {
					sTitle = p.DateBegin.ToString("MMMM d, yyyy");
				}
			}
			if (currentSite.CheckIsSiteSearchPath(sFilterPath)) {
				sTitle = "Search Results";
			}

			return sTitle;
		}

		public int GetSiteSearchCount(Guid siteID, string searchTerm, bool bActiveOnly) {
			IQueryable<vw_carrot_Content> query1 = CannedQueries.GetContentSiteSearch(db, siteID, bActiveOnly, searchTerm);

			return query1.Count();
		}

		public List<SiteNav> GetLatestContentSearchList(Guid siteID, string searchTerm, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			IQueryable<vw_carrot_Content> query1 = CannedQueries.GetContentSiteSearch(db, siteID, bActiveOnly, searchTerm);

			return PerformDataPagingQueryableContent(siteID, bActiveOnly, pageSize, pageNumber, sortField, sortDir, query1);
		}

		public int GetFilteredContentPagedCount(SiteData currentSite, string sFilterPath, bool bActiveOnly) {
			IQueryable<vw_carrot_Content> query1 = null;
			Guid siteID = currentSite.SiteID;
			bool bFound = false;

			if (currentSite.CheckIsBlogCategoryPath(sFilterPath)) {
				query1 = CannedQueries.GetContentByCategoryURL(db, siteID, bActiveOnly, sFilterPath);
				bFound = true;
			}
			if (currentSite.CheckIsBlogTagPath(sFilterPath)) {
				query1 = CannedQueries.GetContentByTagURL(db, siteID, bActiveOnly, sFilterPath);
				bFound = true;
			}
			if (currentSite.CheckIsBlogEditorFolderPath(sFilterPath)) {
				query1 = CannedQueries.GetContentByUserURL(db, siteID, bActiveOnly, sFilterPath);
				bFound = true;
			}
			if (currentSite.CheckIsBlogDateFolderPath(sFilterPath)) {
				BlogDatePathParser p = new BlogDatePathParser(currentSite, sFilterPath);
				query1 = CannedQueries.GetLatestBlogListDateRange(db, siteID, p.DateBeginUTC, p.DateEndUTC, bActiveOnly);
				bFound = true;
			}
			if (!bFound) {
				query1 = CannedQueries.GetLatestBlogList(db, siteID, bActiveOnly);
			}

			return query1.Count();
		}

		public int GetFilteredContentByIDPagedCount(SiteData currentSite, List<Guid> lstCategories, bool bActiveOnly) {
			Guid siteID = currentSite.SiteID;

			IQueryable<vw_carrot_Content> query1 = CannedQueries.GetContentByCategoryIDs(db, siteID, bActiveOnly, lstCategories);

			return query1.Count();
		}

		public int GetFilteredContentByIDPagedCount(SiteData currentSite, List<Guid> lstCategoryGUIDs, List<string> lstCategorySlugs, bool bActiveOnly) {
			Guid siteID = currentSite.SiteID;

			IQueryable<vw_carrot_Content> query1 = CannedQueries.GetContentByCategoryIDs(db, siteID, bActiveOnly, lstCategoryGUIDs, lstCategorySlugs);

			return query1.Count();
		}

		public List<SiteNav> GetFilteredContentPagedList(SiteData currentSite, string sFilterPath, bool bActiveOnly,
			int pageSize, int pageNumber, string sortField, string sortDir) {
			IQueryable<vw_carrot_Content> query1 = null;
			Guid siteID = currentSite.SiteID;
			bool bFound = false;

			if (currentSite.CheckIsBlogCategoryPath(sFilterPath)) {
				query1 = CannedQueries.GetContentByCategoryURL(db, siteID, bActiveOnly, sFilterPath);
				bFound = true;
			}
			if (currentSite.CheckIsBlogTagPath(sFilterPath)) {
				query1 = CannedQueries.GetContentByTagURL(db, siteID, bActiveOnly, sFilterPath);
				bFound = true;
			}
			if (currentSite.CheckIsBlogEditorFolderPath(sFilterPath)) {
				query1 = CannedQueries.GetContentByUserURL(db, siteID, bActiveOnly, sFilterPath);
				bFound = true;
			}
			if (currentSite.CheckIsBlogDateFolderPath(sFilterPath)) {
				BlogDatePathParser p = new BlogDatePathParser(currentSite, sFilterPath);
				query1 = CannedQueries.GetLatestBlogListDateRange(db, siteID, p.DateBeginUTC, p.DateEndUTC, bActiveOnly);
				bFound = true;
			}
			if (!bFound) {
				query1 = CannedQueries.GetLatestBlogList(db, siteID, bActiveOnly);
			}

			return PerformDataPagingQueryableContent(siteID, bActiveOnly, pageSize, pageNumber, sortField, sortDir, query1);
		}

		public List<SiteNav> GetFilteredContentByIDPagedList(SiteData currentSite, List<Guid> lstCategories, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			Guid siteID = currentSite.SiteID;

			IQueryable<vw_carrot_Content> query1 = CannedQueries.GetContentByCategoryIDs(db, siteID, bActiveOnly, lstCategories);

			return PerformDataPagingQueryableContent(siteID, bActiveOnly, pageSize, pageNumber, sortField, sortDir, query1);
		}

		public List<SiteNav> GetFilteredContentByIDPagedList(SiteData currentSite, List<Guid> lstCategoryGUIDs, List<string> lstCategorySlugs, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			Guid siteID = currentSite.SiteID;

			IQueryable<vw_carrot_Content> query1 = CannedQueries.GetContentByCategoryIDs(db, siteID, bActiveOnly, lstCategoryGUIDs, lstCategorySlugs);

			return PerformDataPagingQueryableContent(siteID, bActiveOnly, pageSize, pageNumber, sortField, sortDir, query1);
		}

		public List<SiteNav> GetLatestChildContentPagedList(Guid siteID, Guid? parentContentID, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			IQueryable<vw_carrot_Content> query1 = CannedQueries.GetLatestContentByParent(db, siteID, parentContentID, bActiveOnly);

			return PerformDataPagingQueryableContent(siteID, bActiveOnly, pageSize, pageNumber, sortField, sortDir, query1);
		}

		public List<SiteNav> GetLatestChildContentPagedList(Guid siteID, string parentPage, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			IQueryable<vw_carrot_Content> query1 = CannedQueries.GetLatestContentByParent(db, siteID, parentPage, bActiveOnly);

			return PerformDataPagingQueryableContent(siteID, bActiveOnly, pageSize, pageNumber, sortField, sortDir, query1);
		}

		public List<SiteNav> PerformDataPagingQueryableContent(Guid siteID, bool bActiveOnly,
				int pageSize, int pageNumber, string sortField, string sortDir, IQueryable<vw_carrot_Content> queryable) {
			IEnumerable<SiteNav> lstContent = new List<SiteNav>();

			int startRec = pageNumber * pageSize;

			if (pageSize < 0 || pageSize > 200) {
				pageSize = 25;
			}

			if (pageNumber < 0 || pageNumber > 10000) {
				pageNumber = 0;
			}

			if (string.IsNullOrEmpty(sortField)) {
				sortField = "CreateDate";
			}

			if (string.IsNullOrEmpty(sortDir)) {
				sortDir = "DESC";
			}

			bool IsContentProp = false;

			sortDir = sortDir.ToUpperInvariant();

			sortField = (from p in ReflectionUtilities.GetPropertyStrings(typeof(vw_carrot_Content))
						 where p.ToLowerInvariant().Trim() == sortField.ToLowerInvariant().Trim()
						 select p).FirstOrDefault();

			if (!string.IsNullOrEmpty(sortField)) {
				IsContentProp = ReflectionUtilities.DoesPropertyExist(typeof(vw_carrot_Content), sortField);
			}

			if (IsContentProp) {
				queryable = queryable.SortByParm(sortField, sortDir);
			} else {
				queryable = (from c in queryable
							 orderby c.CreateDate descending
							 where c.SiteID == siteID
								&& c.IsLatestVersion == true
								&& (c.PageActive == bActiveOnly || bActiveOnly == false)
							 select c).AsQueryable();
			}

			lstContent = (from q in queryable select new SiteNav(q)).PaginateListFromZero(pageNumber, pageSize);

			return lstContent.ToList();
		}

		#region IDisposable Members

		public void Dispose() {
			if (db != null) {
				db.Dispose();
			}
		}

		#endregion IDisposable Members
	}
}