using Carrotware.CMS.Data;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
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

namespace Carrotware.CMS.Core {

	public class ContentPageHelper : IDisposable {
		private CarrotCMSDataContext db = CarrotCMSDataContext.Create();

		public ContentPageHelper() { }

		public void BulkUpdateTemplate(Guid siteID, List<Guid> lstUpd, string sTemplateFile) {
			IQueryable<carrot_Content> queryCont = (from ct in db.carrot_Contents
													join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
													where r.SiteID == siteID
														   && lstUpd.Contains(r.Root_ContentID)
														   && ct.IsLatestVersion == true
													select ct);

			db.carrot_Contents.BatchUpdate(queryCont, p => new carrot_Content { TemplateFile = sTemplateFile });

			db.SubmitChanges();
		}

		public void UpdateAllContentTemplates(Guid siteID, string sTemplateFile) {
			UpdateAllBlogTemplates(siteID, sTemplateFile);

			UpdateAllPageTemplates(siteID, sTemplateFile);
		}

		public void UpdateAllBlogTemplates(Guid siteID, string sTemplateFile) {
			IQueryable<carrot_Content> queryBlog = CannedQueries.GetBlogAllContentTbl(db, siteID);

			db.carrot_Contents.BatchUpdate(queryBlog, p => new carrot_Content { TemplateFile = sTemplateFile });

			db.SubmitChanges();
		}

		public void UpdateAllPageTemplates(Guid siteID, string sTemplateFile) {
			IQueryable<carrot_Content> queryContent = CannedQueries.GetContentAllContentTbl(db, siteID);

			db.carrot_Contents.BatchUpdate(queryContent, p => new carrot_Content { TemplateFile = sTemplateFile });

			db.SubmitChanges();
		}

		public void UpdateTopPageTemplates(Guid siteID, string sTemplateFile) {
			IQueryable<carrot_Content> queryContent = CannedQueries.GetContentTopContentTbl(db, siteID);

			db.carrot_Contents.BatchUpdate(queryContent, p => new carrot_Content { TemplateFile = sTemplateFile });

			db.SubmitChanges();
		}

		public void UpdateSubPageTemplates(Guid siteID, string sTemplateFile) {
			IQueryable<carrot_Content> queryContent = CannedQueries.GetContentSubContentTbl(db, siteID);

			db.carrot_Contents.BatchUpdate(queryContent, p => new carrot_Content { TemplateFile = sTemplateFile });

			db.SubmitChanges();
		}

		public List<Guid> GetPageHierarchy(Guid siteID, Guid rootContentID) {
			List<Guid> lstSub = new List<Guid>();
			int iDepth = 10000;

			List<Guid> lstFoundIDs = new List<Guid>();
			lstFoundIDs.Add(rootContentID);

			while (iDepth > 1) {
				lstSub = (from ct in CannedQueries.GetLatestContentList(db, siteID, false)
						  where ct.SiteID == siteID
								&& (!lstFoundIDs.Contains(ct.Root_ContentID) && lstFoundIDs.Contains(ct.Parent_ContentID.Value))
						  select ct.Root_ContentID).Distinct().ToList();

				lstFoundIDs = lstFoundIDs.Union(lstSub).ToList();

				if (lstSub.Count < 1) {
					break;
				}
				iDepth--;
			}

			return lstFoundIDs;
		}

		public static string CreateBlogDatePrefix(Guid siteID, DateTime goLiveDate) {
			var site = SiteData.GetSiteFromCache(siteID);

			return CreateBlogDatePrefix(site, goLiveDate);
		}

		public static string CreateBlogDatePrefix(SiteData site, DateTime goLiveDate) {
			string fileName = "";

			if (site.Blog_DatePattern.Length > 1) {
				fileName = "/" + ScrubSpecial(goLiveDate.ToString(site.Blog_DatePattern)) + "/";
			} else {
				fileName = "/";
			}

			return ScrubPath(fileName);
		}

		public static string CreateFileNameFromSlug(Guid siteID, DateTime goLiveDate, string pageSlug) {
			var site = SiteData.GetSiteFromCache(siteID);

			return CreateFileNameFromSlug(site, goLiveDate, pageSlug);
		}

		public static string CreateFileNameFromSlug(SiteData site, DateTime goLiveDate, string pageSlug) {
			string fileName = "";

			fileName = "/" + CreateBlogDatePrefix(site, goLiveDate) + "/" + pageSlug;

			return ScrubFilename(Guid.Empty, fileName);
		}

		public void BulkBlogFileNameUpdateFromDate(Guid siteID) {
			TimeZoneContent zone = new TimeZoneContent(siteID);

			zone.Save();

			ResolveDuplicateBlogURLs(siteID);
		}

		public void FixBlogNavOrder(Guid siteID) {
			IQueryable<carrot_Content> queryCont = CannedQueries.GetBlogAllContentTbl(db, siteID);
			IQueryable<carrot_RootContent> queryContRoot = CannedQueries.GetBlogAllRootTbl(db, siteID);

			db.carrot_Contents.BatchUpdate(queryCont, p => new carrot_Content { NavOrder = SiteData.BlogSortOrderNumber, Parent_ContentID = null });
			db.carrot_RootContents.BatchUpdate(queryContRoot, p => new carrot_RootContent { ShowInSiteMap = false, ShowInSiteNav = false });

			db.SubmitChanges();
		}

		public void ResolveDuplicateBlogURLs(Guid siteID) {
			SiteData site = SiteData.GetSiteFromCache(siteID);
			IQueryable<string> queryFindDups = CompiledQueries.cqBlogDupFileNames(db, siteID);

			Guid contentTypeID = ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry);

			foreach (string fileName in queryFindDups) {
				int iDupCtr = 1;
				IQueryable<carrot_RootContent> queryContentShareFilename = CompiledQueries.cqGetRootContentListByURLTbl(db, siteID, contentTypeID, fileName);

				foreach (carrot_RootContent item in queryContentShareFilename) {
					int c = -1;
					string newFilename = ScrubFilename(item.Root_ContentID, CreateFileNameFromSlug(site, item.GoLiveDate, item.PageSlug));
					string slug = item.PageSlug;

					c = CompiledQueries.cqGetRootContentListNoMatchByURL(db, siteID, item.Root_ContentID, newFilename).Count();

					if (c > 0) {
						newFilename = newFilename.Substring(0, newFilename.Length - 5) + "-" + item.CreateDate.ToString("yyyy-MM-dd");
						slug = slug.Substring(0, slug.Length - 5) + "-" + item.CreateDate.ToString("yyyy-MM-dd");

						c = CompiledQueries.cqGetRootContentListNoMatchByURL(db, siteID, item.Root_ContentID, newFilename).Count();

						if (c > 0) {
							newFilename = newFilename.Substring(0, newFilename.Length - 5) + "-" + iDupCtr.ToString();
							slug = slug.Substring(0, slug.Length - 5) + "-" + iDupCtr.ToString();
						}
					}

					item.PageSlug = slug;
					item.FileName = newFilename;
					iDupCtr++;
					db.SubmitChanges();
				}
			}
		}

		public enum UpdateField {
			MarkActive,
			MarkInactive,
			MarkIncludeInSiteMap,
			MarkIncludeInSiteMapNo,
			MarkIncludeInSiteNav,
			MarkIncludeInSiteNavNo,
			MarkAsIndexable,
			MarkAsIndexableNo,
		}

		public void MarkSelectedPublished(Guid siteID, List<Guid> lstUpd, UpdateField selField) {
			IQueryable<carrot_RootContent> queryCont = null;

			Guid gContent = ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry);

			if (selField == UpdateField.MarkActive
				|| selField == UpdateField.MarkAsIndexable
				|| selField == UpdateField.MarkInactive
				|| selField == UpdateField.MarkAsIndexableNo) {
				queryCont = (from r in db.carrot_RootContents
							 where r.SiteID == siteID
								&& lstUpd.Contains(r.Root_ContentID)
							 select r);
			} else {
				queryCont = (from r in db.carrot_RootContents
							 where r.SiteID == siteID
								&& r.ContentTypeID == gContent
								&& lstUpd.Contains(r.Root_ContentID)
							 select r);
			}

			switch (selField) {
				case UpdateField.MarkActive:
					db.carrot_RootContents.BatchUpdate(queryCont, p => new carrot_RootContent { PageActive = true });
					break;

				case UpdateField.MarkInactive:
					db.carrot_RootContents.BatchUpdate(queryCont, p => new carrot_RootContent { PageActive = false });
					break;

				case UpdateField.MarkAsIndexable:
					db.carrot_RootContents.BatchUpdate(queryCont, p => new carrot_RootContent { BlockIndex = false });
					break;

				case UpdateField.MarkAsIndexableNo:
					db.carrot_RootContents.BatchUpdate(queryCont, p => new carrot_RootContent { BlockIndex = true });
					break;

				case UpdateField.MarkIncludeInSiteMap:
					db.carrot_RootContents.BatchUpdate(queryCont, p => new carrot_RootContent { ShowInSiteMap = true });
					break;

				case UpdateField.MarkIncludeInSiteMapNo:
					db.carrot_RootContents.BatchUpdate(queryCont, p => new carrot_RootContent { ShowInSiteMap = false });
					break;

				case UpdateField.MarkIncludeInSiteNav:
					db.carrot_RootContents.BatchUpdate(queryCont, p => new carrot_RootContent { ShowInSiteNav = true });
					break;

				case UpdateField.MarkIncludeInSiteNavNo:
					db.carrot_RootContents.BatchUpdate(queryCont, p => new carrot_RootContent { ShowInSiteNav = false });
					break;
			}

			db.SubmitChanges();
		}

		public static string ScrubFilename(string fileName) {
			return ScrubFilename(Guid.Empty, fileName);
		}

		public static string ScrubFilename(Guid rootContentID, string fileName) {
			string newFileName = string.Format("{0}", fileName).Trim();

			if (string.IsNullOrEmpty(newFileName)) {
				newFileName = "/" + rootContentID.ToString().ToLowerInvariant();
			}

			newFileName = newFileName.Replace(@"//", @"/").Replace(@"//", @"/");

			if (newFileName.EndsWith(@"/")) {
				newFileName = newFileName.Substring(0, newFileName.Length - 1);
			}

			var ext = new string[] { ".aspx", ".cshtml", ".vbhtml", ".htm", ".html" };

			foreach (var x in ext) {
				if (newFileName.ToLowerInvariant().EndsWith(x)) {
					newFileName = newFileName.Substring(0, newFileName.Length - x.Length);
				}
			}

			newFileName = ScrubFilePath(newFileName).Trim();

			if (newFileName.EndsWith(@"/")) {
				newFileName = newFileName.Trim().Substring(0, newFileName.Length - 1);
			}

			return newFileName;
		}

		private static string ScrubSpecial(string sInput) {
			sInput = string.Format("{0}", sInput).Trim();

			Encoding iso = Encoding.GetEncoding("ISO-8859-8");  //use ISO-8859-8 to auto drop accent chars
			Encoding utf8 = Encoding.UTF8;
			byte[] utfBytes = utf8.GetBytes(sInput);
			byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
			string sOutput = iso.GetString(isoBytes);

			iso = Encoding.ASCII;  //once accents are dropped, turn ASCII
			utfBytes = utf8.GetBytes(sOutput);
			isoBytes = Encoding.Convert(utf8, iso, utfBytes);
			sOutput = iso.GetString(isoBytes);

			sOutput = sOutput.Replace("*", "-star-");
			sOutput = sOutput.Replace("%", "-percent-");
			sOutput = sOutput.Replace("&", "-n-");

			char[] badPathChars = Path.GetInvalidPathChars();  // if chars not valid for a path make into a dash
			sOutput = new String(sOutput.Select(x => badPathChars.Contains(x) ? '-' : x).ToArray());

			sOutput = sOutput.Replace("....", "-").Replace("...", "-").Replace("..", "-");
			sOutput = sOutput.Replace(" ", "-");
			sOutput = sOutput.Replace("'", "-");
			sOutput = sOutput.Replace("\"", "-");
			sOutput = sOutput.Replace(",", "-");
			sOutput = sOutput.Replace("+", "-");

			sOutput = sOutput.Replace("---", "-").Replace("--", "-");
			sOutput = sOutput.Replace(@"///", "/").Replace(@"//", "/");
			sOutput = sOutput.Trim();

			sOutput = Regex.Replace(sOutput, "[:\"*?<>|]+", "-");
			sOutput = Regex.Replace(sOutput, @"[^0-9a-zA-Z.-/_]+", "-");

			sOutput = sOutput.Replace(".", "-").Replace("--", "-");
			sOutput = sOutput.Replace("----", "-").Replace("---", "-").Replace("--", "-").Replace("--", "-");
			sOutput = sOutput.Replace(@"////", "/").Replace(@"///", "/").Replace(@"//", "/").Replace(@"//", "/");
			sOutput = sOutput.Trim();

			if (sOutput.EndsWith("-")) {
				sOutput = sOutput.Substring(0, sOutput.Length - 1);
			}

			return sOutput.Trim();
		}

		public static string ScrubSlug(string slugValue) {
			string newSlug = string.Format("{0}", slugValue).Trim();

			newSlug = newSlug.Replace(@"\", string.Empty);
			newSlug = newSlug.Replace(@"/", string.Empty);

			newSlug = ScrubSpecial(newSlug);

			return newSlug;
		}

		public static string ScrubPath(string filePath) {
			string newFilePath = ScrubFilePath(filePath);

			if (!newFilePath.EndsWith(@"/")) {
				newFilePath = string.Format("{0}/", newFilePath).Trim();
			}

			return newFilePath;
		}

		private static string ScrubFilePath(string filePath) {
			string newFilePath = string.Format("{0}", filePath).Trim().Replace(@"//", @"/").Replace(@"\", @"/");

			string[] newPaths = newFilePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			newPaths = newPaths.ToList().Select(x => ScrubSpecial(x)).ToArray();
			newFilePath = string.Join("/", newPaths.ToArray());

			if (!newFilePath.StartsWith(@"/")) {
				newFilePath = string.Format("/{0}", newFilePath).Trim();
			}

			return newFilePath;
		}

		public PageViewType GetBlogHeadingFromURL(SiteData currentSite, string sFilterPath) {
			Guid siteID = currentSite.SiteID;
			PageViewType pvt = new PageViewType { ExtraTitle = string.Empty, CurrentViewType = PageViewType.ViewType.SinglePage, RawValue = null };

			string sTitle = string.Empty;

			if (currentSite.CheckIsBlogCategoryPath(sFilterPath)) {
				pvt.CurrentViewType = PageViewType.ViewType.CategoryIndex;
				vw_carrot_CategoryURL query = CompiledQueries.cqGetCategoryByURL(db, siteID, sFilterPath);
				if (query != null) {
					sTitle = query.CategoryText;
					pvt.RawValue = query.CategoryText;
				}
			}
			if (currentSite.CheckIsBlogTagPath(sFilterPath)) {
				pvt.CurrentViewType = PageViewType.ViewType.TagIndex;
				vw_carrot_TagURL query = CompiledQueries.cqGetTagByURL(db, siteID, sFilterPath);
				if (query != null) {
					sTitle = query.TagText;
					pvt.RawValue = query.TagText;
				}
			}
			if (currentSite.CheckIsBlogEditorFolderPath(sFilterPath)) {
				pvt.CurrentViewType = PageViewType.ViewType.AuthorIndex;
				vw_carrot_EditorURL query = CompiledQueries.cqGetEditorByURL(db, siteID, sFilterPath);
				if (query != null) {
					ExtendedUserData usr = new ExtendedUserData(query.UserId);
					sTitle = usr.ToString();
					pvt.RawValue = usr;
				}
			}
			if (currentSite.CheckIsBlogDateFolderPath(sFilterPath)) {
				pvt.CurrentViewType = PageViewType.ViewType.DateIndex;

				BlogDatePathParser p = new BlogDatePathParser(currentSite, sFilterPath);
				TimeSpan ts = p.DateEndUTC - p.DateBeginUTC;

				pvt.RawValue = p.DateBegin;

				int daysDelta = ts.Days;
				if (daysDelta < 400 && daysDelta > 90) {
					sTitle = p.DateBegin.ToString("yyyy");
					pvt.CurrentViewType = PageViewType.ViewType.DateYearIndex;
				}
				if (daysDelta < 36 && daysDelta > 3) {
					sTitle = p.DateBegin.ToString("MMMM yyyy");
					pvt.CurrentViewType = PageViewType.ViewType.DateMonthIndex;
				}
				if (daysDelta < 5) {
					sTitle = p.DateBegin.ToString("MMMM d, yyyy");
					pvt.CurrentViewType = PageViewType.ViewType.DateDayIndex;
				}
			}

			if (currentSite.CheckIsSiteSearchPath(sFilterPath)) {
				pvt.CurrentViewType = PageViewType.ViewType.SearchResults;
				string sSearchTerm = "";

				if (HttpContext.Current.Request.QueryString[SiteData.SearchQueryParameter] != null) {
					sSearchTerm = HttpContext.Current.Request.QueryString[SiteData.SearchQueryParameter].ToString();
				}

				pvt.RawValue = sSearchTerm;
				sTitle = string.Format(" '{0}' ", sSearchTerm);
			}

			pvt.ExtraTitle = sTitle;

			return pvt;
		}

		public Dictionary<string, float> GetPopularTemplateList(Guid siteID, ContentPageType.PageType pageType) {
			Dictionary<string, float> lstTemps = CannedQueries.GetTemplateCounts(db, siteID, pageType);

			return lstTemps;
		}

		public List<ContentPage> GetAllLatestContentList(Guid siteID) {
			List<ContentPage> lstContent = CannedQueries.GetAllContentList(db, siteID).Select(ct => new ContentPage(ct)).ToList();

			return lstContent;
		}

		public List<ContentPage> GetAllLatestBlogList(Guid siteID) {
			List<ContentPage> lstContent = CannedQueries.GetAllBlogList(db, siteID).Select(ct => new ContentPage(ct)).ToList();

			return lstContent;
		}

		public List<ContentPage> GetPagedSortedContent(Guid siteID, ContentPageType.PageType entryType, bool bActiveOnly, int pageSize, int pageNumber, string sSortParm) {
			SortParm srt = new SortParm(sSortParm);

			IQueryable<vw_carrot_Content> query1 = CannedQueries.GetAllByTypeList(db, siteID, bActiveOnly, entryType);

			return PerformDataPagingQueryableContent(siteID, bActiveOnly, pageSize, pageNumber, srt.SortField, srt.SortDirection, query1);
		}

		public int GetSitePageCount(Guid siteID, ContentPageType.PageType entryType, bool bActiveOnly) {
			int iCount = CannedQueries.GetAllByTypeList(db, siteID, bActiveOnly, entryType).Count();
			return iCount;
		}

		public int GetSitePageCount(Guid siteID, ContentPageType.PageType entryType) {
			int iCount = CannedQueries.GetAllByTypeList(db, siteID, false, entryType).Count();
			return iCount;
		}

		public int GetSiteSnippetCount(Guid siteID) {
			int iCount = CompiledQueries.cqGetSnippetsBySiteID(db, siteID).Count();
			return iCount;
		}

		public int GetSiteContentCount(Guid siteID) {
			int iCount = CannedQueries.GetLatestContentList(db, siteID, false).Count();
			return iCount;
		}

		public int GetMaxNavOrder(Guid siteID) {
			int iCount = CompiledQueries.cqGetMaxOrderID(db, siteID);

			return iCount;
		}

		public List<ContentPage> GetLatest(Guid siteID, int iUpdates, bool bActiveOnly) {
			List<ContentPage> lstContent = (from ct in CannedQueries.GetLatestContentList(db, siteID, bActiveOnly)
											orderby ct.GoLiveDate descending
											select new ContentPage(ct)).Take(iUpdates).ToList();

			return lstContent;
		}

		public List<ContentPage> GetLatestPosts(Guid siteID, int iUpdates, bool bActiveOnly) {
			List<ContentPage> lstContent = (from ct in CannedQueries.GetLatestBlogList(db, siteID, bActiveOnly)
											orderby ct.GoLiveDate descending
											select new ContentPage(ct)).Take(iUpdates).ToList();

			return lstContent;
		}

		public List<ContentPage> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageNumber, string sortField, string sortDir) {
			return GetLatestContentPagedList(siteID, ContentPageType.PageType.BlogEntry, bActiveOnly, pageNumber, sortField, sortDir);
		}

		public List<ContentPage> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageNumber) {
			return GetLatestContentPagedList(siteID, ContentPageType.PageType.BlogEntry, bActiveOnly, pageNumber);
		}

		public List<ContentPage> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber) {
			return GetLatestContentPagedList(siteID, ContentPageType.PageType.BlogEntry, bActiveOnly, pageSize, pageNumber);
		}

		public List<ContentPage> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageNumber, string sortField, string sortDir) {
			return GetLatestContentPagedList(siteID, postType, bActiveOnly, 10, pageNumber, sortField, sortDir);
		}

		public List<ContentPage> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageNumber) {
			return GetLatestContentPagedList(siteID, postType, bActiveOnly, 10, pageNumber, "", "");
		}

		public List<ContentPage> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly, int pageSize, int pageNumber) {
			return GetLatestContentPagedList(siteID, postType, bActiveOnly, pageSize, pageNumber, "", "");
		}

		public List<ContentPage> GetLatestBlogPagedList(Guid siteID, bool bActiveOnly, int pageSize, int pageNumber, string sortField, string sortDir) {
			return GetLatestContentPagedList(siteID, ContentPageType.PageType.BlogEntry, bActiveOnly, pageSize, pageNumber, sortField, sortDir);
		}

		public List<ContentPage> GetLatestContentPagedList(Guid siteID, ContentPageType.PageType postType, bool bActiveOnly,
					int pageSize, int pageNumber, string sortField, string sortDir) {
			IQueryable<vw_carrot_Content> query1 = null;

			if (postType == ContentPageType.PageType.ContentEntry) {
				query1 = CannedQueries.GetLatestContentList(db, siteID, bActiveOnly);
			} else {
				query1 = CannedQueries.GetLatestBlogList(db, siteID, bActiveOnly);
			}

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

		public List<ContentPage> GetFilteredContentPagedList(SiteData currentSite, string sFilterPath, bool bActiveOnly,
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

		public List<ContentPage> PerformDataPagingQueryableContent(Guid siteID, bool bActiveOnly,
				int pageSize, int pageNumber, string sortField, string sortDir, IQueryable<vw_carrot_Content> queryable) {
			IEnumerable<ContentPage> lstContent = new List<ContentPage>();

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

			lstContent = (from q in queryable select new ContentPage(q)).PaginateList(pageNumber, pageSize);

			return lstContent.ToList();
		}

		public void ResetHeartbeatLock(Guid rootContentID, Guid siteID, Guid currentUserID) {
			carrot_RootContent rc = CompiledQueries.cqGetRootContentTbl(db, siteID, rootContentID);

			if (rc != null) {
				if (rc.Heartbeat_UserId.HasValue && rc.Heartbeat_UserId.Value == currentUserID) {
					rc.EditHeartbeat = DateTime.UtcNow.AddHours(-2);
					rc.Heartbeat_UserId = null;
					db.SubmitChanges();
				} else {
					if (!rc.Heartbeat_UserId.HasValue) {
						rc.EditHeartbeat = DateTime.UtcNow.AddHours(-4);
						rc.Heartbeat_UserId = null;
						db.SubmitChanges();
					}
				}
			}
		}

		public bool RecordHeartbeatLock(Guid rootContentID, Guid siteID, Guid currentUserID) {
			carrot_RootContent rc = CompiledQueries.cqGetRootContentTbl(db, siteID, rootContentID);

			if (rc != null) {
				rc.Heartbeat_UserId = currentUserID;
				rc.EditHeartbeat = DateTime.UtcNow;
				db.SubmitChanges();
				return true;
			}

			return false;
		}

		public bool IsPageLocked(Guid rootContentID, Guid siteID, Guid currentUserID) {
			carrot_RootContent rc = CompiledQueries.cqGetRootContentTbl(db, siteID, rootContentID);

			bool bLock = false;
			if (rc != null && rc.Heartbeat_UserId != null) {
				if (rc.Heartbeat_UserId != currentUserID
						&& rc.EditHeartbeat.Value > DateTime.UtcNow.AddMinutes(-2)) {
					bLock = true;
				}
				if (rc.Heartbeat_UserId == currentUserID
					|| rc.Heartbeat_UserId == null) {
					bLock = false;
				}
			}
			return bLock;
		}

		public bool IsPageLocked(Guid rootContentID, Guid siteID) {
			carrot_RootContent cp = CompiledQueries.cqGetRootContentTbl(db, siteID, rootContentID);

			bool bLock = false;
			if (cp.Heartbeat_UserId != null) {
				if (cp.Heartbeat_UserId != SecurityData.CurrentUserGuid
						&& cp.EditHeartbeat.Value > DateTime.UtcNow.AddMinutes(-2)) {
					bLock = true;
				}
				if (cp.Heartbeat_UserId == SecurityData.CurrentUserGuid
					|| cp.Heartbeat_UserId == null) {
					bLock = false;
				}
			}
			return bLock;
		}

		public bool IsPageLocked(ContentPage cp) {
			bool bLock = false;
			if (cp.Heartbeat_UserId != null) {
				if (cp.Heartbeat_UserId != SecurityData.CurrentUserGuid
						&& cp.EditHeartbeat.Value > DateTime.UtcNow.AddMinutes(-2)) {
					bLock = true;
				}
				if (cp.Heartbeat_UserId == SecurityData.CurrentUserGuid
					|| cp.Heartbeat_UserId == null) {
					bLock = false;
				}
			}
			return bLock;
		}

		public bool RecordPageLock(Guid rootContentID, Guid siteID, Guid currentUserID) {
			bool bLock = IsPageLocked(rootContentID, siteID, currentUserID);
			bool bRet = false;

			if (!bLock) {
				ExtendedUserData usr = new ExtendedUserData(currentUserID);

				//only allow admin/editors to record a lock
				if (usr.IsAdmin || usr.IsEditor) {
					bRet = RecordHeartbeatLock(rootContentID, siteID, currentUserID);
				}
			}

			return bRet;
		}

		public Guid GetCurrentEditUser(Guid rootContentID, Guid siteID) {
			carrot_RootContent rc = CompiledQueries.cqGetRootContentTbl(db, siteID, rootContentID);

			if (rc != null) {
				return (Guid)rc.Heartbeat_UserId;
			} else {
				return Guid.Empty;
			}
		}

		public static ContentPage GetEmptyHome() {
			ContentPage navData = new ContentPage();
			navData.ContentID = Guid.Empty;
			navData.Root_ContentID = Guid.Empty;
			navData.SiteID = SiteData.CurrentSiteID;
			navData.TemplateFile = "_EmptyHome";
			navData.FileName = SiteData.DefaultDirectoryFilename;
			navData.NavMenuText = "NONE";
			navData.PageHead = "NONE";
			navData.TitleBar = "NONE";
			navData.PageActive = false;
			navData.PageText = "<p>NO PAGE CONTENT</p>" + SiteNavHelper.SampleBody;
			navData.EditDate = DateTime.Now.Date.AddDays(-1);
			navData.CreateDate = DateTime.Now.Date.AddDays(-10);
			navData.GoLiveDate = DateTime.Now.Date.AddDays(1);
			navData.RetireDate = DateTime.Now.Date.AddDays(90);
			navData.ContentType = ContentPageType.PageType.ContentEntry;
			return navData;
		}

		public static ContentPage GetEmptySearch() {
			var link = new HtmlTag(HtmlTag.EasyTag.AnchorTag);
			link.Uri = SiteFilename.SiteInfoURL;
			link.InnerHtml = "Site Info";

			ContentPage navData = new ContentPage();
			navData.ContentID = Guid.Empty;
			navData.Root_ContentID = Guid.NewGuid();
			navData.SiteID = SiteData.CurrentSiteID;
			navData.TemplateFile = SiteData.DefaultTemplateFilename;
			navData.FileName = SiteData.CurrentSite.SiteSearchPath;
			navData.NavMenuText = "Search";
			navData.PageHead = "Search";
			navData.TitleBar = "Search";
			navData.PageActive = true;
			navData.PageText = SecurityData.IsAuthenticated == false ? "<p>Search Results</p>" :
							"<h2>This is a temporary search result page.  To assign an index page, please visit the"
							+ " " + link.RenderTag() + " page and select the page"
							+ " you want to be the search result page.</h2>";
			navData.EditDate = DateTime.Now.Date.AddDays(-30);
			navData.CreateDate = DateTime.Now.Date.AddDays(-30);
			navData.GoLiveDate = DateTime.Now.Date.AddDays(-30);
			navData.RetireDate = DateTime.Now.Date.AddDays(180);
			navData.ContentType = ContentPageType.PageType.ContentEntry;
			return navData;
		}

		public static ContentPage GetSamplerView() {
			var sbFile1 = new StringBuilder();
			var sbFile2 = new StringBuilder();

			SiteNavHelper.ResetCaption();
			var caption = SiteNavHelper.GetNextCaption();

			try {
				Assembly _assembly = Assembly.GetExecutingAssembly();

				sbFile1.Append(CoreHelper.ReadEmbededScript("Carrotware.CMS.Core.SiteContent.Mock.SampleContent1.txt"));
				sbFile2.Append(CoreHelper.ReadEmbededScript("Carrotware.CMS.Core.SiteContent.Mock.SampleContent2.txt"));

				List<string> imageNames = (from i in _assembly.GetManifestResourceNames()
										   where i.ToLowerInvariant().Contains("sitecontent.mock.sample")
													 && i.EndsWith(".png")
										   select i).ToList();

				foreach (string img in imageNames) {
					var imgURL = CoreHelper.GetWebResourceUrl(img);
					sbFile1.Replace(img, imgURL);
					sbFile2.Replace(img, imgURL);
				}
			} catch { }

			ContentPage pageNew = new ContentPage();
			pageNew.Root_ContentID = SiteData.CurrentSiteID;
			pageNew.ContentID = pageNew.Root_ContentID;
			pageNew.SiteID = SiteData.CurrentSiteID;
			pageNew.Parent_ContentID = null;

			pageNew.PageText = "<h2>Content CENTER</h2>\r\n" + SiteData.StarterHomePageSample + "\r\n" + sbFile1;
			pageNew.LeftPageText = "<h2>Content LEFT</h2>\r\n" + sbFile2;
			pageNew.RightPageText = "<h2>Content RIGHT</h2>\r\n" + sbFile2;

			pageNew.IsLatestVersion = true;
			pageNew.NavOrder = -1;
			pageNew.TitleBar = string.Format("{0} TT", caption);
			pageNew.NavMenuText = string.Format("{0} NN", caption);
			pageNew.PageHead = string.Format("{0} HH", caption);
			pageNew.PageActive = true;
			pageNew.ShowInSiteNav = true;
			pageNew.ShowInSiteMap = true;

			pageNew.EditUserId = SecurityData.CurrentUserGuid;

			pageNew.EditDate = DateTime.Now.Date.AddHours(-45);
			pageNew.CreateDate = DateTime.Now.Date.AddHours(-90);
			pageNew.GoLiveDate = pageNew.EditDate.AddHours(-48);
			pageNew.RetireDate = pageNew.CreateDate.AddYears(10);

			pageNew.TemplateFile = SiteData.PreviewTemplateFile;
			pageNew.FileName = SiteData.PreviewTemplateFilePage;
			pageNew.MetaDescription = "Meta Description";
			pageNew.MetaKeyword = "Meta Keyword";

			pageNew.ContentType = ContentPageType.PageType.BlogEntry;
			pageNew.PageSlug = "sampler-page-view";

			List<ContentCategory> lstK = new List<ContentCategory>();
			List<ContentTag> lstT = new List<ContentTag>();

			for (int i = 0; i < 5; i++) {
				ContentCategory k = new ContentCategory {
					ContentCategoryID = Guid.NewGuid(),
					CategoryText = "Keyword Text " + i.ToString(),
					CategorySlug = "keyword-slug-" + i.ToString()
				};
				ContentTag t = new ContentTag {
					ContentTagID = Guid.NewGuid(),
					TagText = "Tag Text " + i.ToString(),
					TagSlug = "tag-slug-" + i.ToString()
				};

				lstK.Add(k);
				lstT.Add(t);
			}

			pageNew.ContentCategories = lstK;
			pageNew.ContentTags = lstT;

			return pageNew;
		}

		public List<ContentPage> GetVersionHistory(Guid siteID, Guid rootContentID) {
			List<ContentPage> content = CompiledQueries.cqGetVersionHistory(db, siteID, rootContentID).Select(ct => new ContentPage(ct)).ToList();

			return content;
		}

		public ContentPage GetVersion(Guid siteID, Guid contentID) {
			ContentPage content = null;
			vw_carrot_Content cont = CompiledQueries.cqGetContentByContentID(db, siteID, contentID);
			if (cont != null) {
				content = new ContentPage(cont);
			}
			return content;
		}

		public List<ContentPage> GetLatestContentList(Guid siteID, bool bActiveOnly) {
			List<ContentPage> lstContent = CompiledQueries.GetLatestContentList(db, siteID, bActiveOnly).Select(ct => new ContentPage(ct)).ToList();

			return lstContent;
		}

		public void RemoveVersions(Guid siteID, List<Guid> lstDel) {
			//List<carrot_Content> lstContent = (from ct in db.carrot_Contents
			//                                   join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
			//                                   orderby ct.EditDate descending
			//                                   where r.SiteID == siteID
			//                                    && lstDel.Contains(ct.ContentID)
			//                                    && ct.IsLatestVersion != true
			//                                   select ct).ToList();

			//if (lstContent.Count > 0) {
			//    db.carrot_Contents.DeleteAllOnSubmit(lstContent);
			//    db.SubmitChanges();
			//}

			IQueryable<carrot_Content> queryCont = (from ct in db.carrot_Contents
													join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
													orderby ct.EditDate descending
													where r.SiteID == siteID
													 && lstDel.Contains(ct.ContentID)
													 && ct.IsLatestVersion != true
													select ct);

			if (lstDel.Any()) {
				db.carrot_Contents.BatchDelete(queryCont);
				db.SubmitChanges();
			}
		}

		public void RemoveContent(Guid siteID, Guid rootContentID) {
			using (var transaction = new TransactionScope()) {
				try {
					IQueryable<carrot_Content> queryCont = (from ct in db.carrot_Contents
															where ct.Root_ContentID == rootContentID
															select ct);

					IQueryable<carrot_RootContent> queryRootContent = (from r in db.carrot_RootContents
																	   where r.SiteID == siteID
																		   && r.Root_ContentID == rootContentID
																	   select r);

					IQueryable<carrot_ContentComment> queryComment = (from r in db.carrot_ContentComments
																	  where r.Root_ContentID == rootContentID
																	  select r);

					IQueryable<carrot_TrackbackQueue> queryTrack = (from r in db.carrot_TrackbackQueues
																	where r.Root_ContentID == rootContentID
																	select r);

					IQueryable<carrot_Widget> queryWidget = (from r in db.carrot_Widgets
															 where r.Root_ContentID == rootContentID
															 select r);

					List<Guid> widgIDs = queryWidget.Select(x => x.Root_WidgetID).Distinct().ToList();

					IQueryable<carrot_WidgetData> queryWidgetData = (from r in db.carrot_WidgetDatas
																	 where widgIDs.Contains(r.Root_WidgetID)
																	 select r);

					IQueryable<carrot_Content> queryChildPages = (from ct in db.carrot_Contents
																  join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
																  where r.SiteID == siteID
																		&& ct.Parent_ContentID == rootContentID
																  select ct);

					IQueryable<carrot_TagContentMapping> oldContentTags = CannedQueries.GetContentTagMapByContentID(db, rootContentID);
					IQueryable<carrot_CategoryContentMapping> oldContentCategories = CannedQueries.GetContentCategoryMapByContentID(db, rootContentID);

					db.carrot_Contents.BatchUpdate(queryChildPages, p => new carrot_Content { Parent_ContentID = null });

					db.carrot_TagContentMappings.BatchDelete(oldContentTags);
					db.carrot_CategoryContentMappings.BatchDelete(oldContentCategories);

					db.carrot_WidgetDatas.BatchDelete(queryWidgetData);
					db.carrot_Widgets.BatchDelete(queryWidget);

					db.carrot_ContentComments.BatchDelete(queryComment);
					db.carrot_TrackbackQueues.BatchDelete(queryTrack);

					db.carrot_Contents.BatchDelete(queryCont);
					db.carrot_RootContents.BatchDelete(queryRootContent);

					Guid? newHomeID = (from ct in db.carrot_Contents
									   join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
									   orderby ct.NavOrder ascending
									   where r.SiteID == siteID
											 && ct.Root_ContentID != rootContentID
											 && ct.IsLatestVersion == true
											 && ct.Parent_ContentID == null
											 && r.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry)
									   select ct.Root_ContentID).FirstOrDefault();

					if (newHomeID == null) {
						newHomeID = (from ct in db.carrot_Contents
									 join r in db.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
									 orderby ct.NavOrder ascending
									 where r.SiteID == siteID
											&& ct.Root_ContentID != rootContentID
											&& ct.IsLatestVersion == true
											&& r.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry)
									 select ct.Root_ContentID).FirstOrDefault();
					}

					if (newHomeID.HasValue) {
						IQueryable<carrot_Content> queryContNH = (from ct in db.carrot_Contents
																  where ct.Root_ContentID == newHomeID.Value
																		&& ct.IsLatestVersion == true
																  select ct);

						db.carrot_Contents.BatchUpdate(queryContNH, p => new carrot_Content { NavOrder = 0 });
					}

					db.SubmitChanges();

					transaction.Complete();
				} catch (Exception ex) {
					throw;
				}
			}

			FixBlogNavOrder(siteID);
		}

		public ContentPage FindContentByID(Guid siteID, Guid rootContentID) {
			ContentPage content = null;
			vw_carrot_Content cont = CompiledQueries.GetLatestContentByID(db, siteID, false, rootContentID);
			if (cont != null) {
				content = new ContentPage(cont);
			}
			return content;
		}

		public ContentPage GetLatestContentByURL(Guid siteID, bool bActiveOnly, string sPage) {
			ContentPage content = null;
			vw_carrot_Content cont = CompiledQueries.GetLatestContentByURL(db, siteID, bActiveOnly, sPage);
			if (cont != null) {
				content = new ContentPage(cont);
			}
			if (content == null && sPage == SiteData.DefaultDirectoryFilename) {
				content = FindHome(siteID, bActiveOnly);
			}
			return content;
		}

		public ContentPage FindByFilename(Guid siteID, string urlFileName) {
			ContentPage content = null;
			vw_carrot_Content cont = CompiledQueries.GetLatestContentByURL(db, siteID, false, urlFileName);

			if (cont != null) {
				content = new ContentPage(cont);
			}
			if (content == null && urlFileName == SiteData.DefaultDirectoryFilename) {
				content = FindHome(siteID, false);
			}

			return content;
		}

		public ContentPage FindByPageSlug(Guid siteID, DateTime datePublished, string urlPageSlug) {
			ContentPage content = null;
			vw_carrot_Content cont = CompiledQueries.cqGetLatestContentBySlug(db, siteID, datePublished, urlPageSlug);
			if (cont != null) {
				content = new ContentPage(cont);
			}
			return content;
		}

		public List<ContentPage> FindPagesBeginingWith(Guid siteID, string sFolderPath) {
			List<ContentPage> lstContent = (from ct in GetPagesBeginingWith(siteID, sFolderPath).ToList()
											select new ContentPage(ct)).ToList();

			return lstContent;
		}

		public bool ExistingPagesBeginWith(Guid siteID, string sFolderPath) {
			string p1 = ("/" + sFolderPath.ToLowerInvariant() + "/").Replace("//", "/");
			int i1 = GetPagesBeginingWith(siteID, p1).Count();

			string p2 = ("/" + sFolderPath.ToLowerInvariant()).Replace("//", "/");
			int i2 = GetPagesEquals(siteID, p2).Count();

			return (i1 + i2) > 0;
		}

		public bool ExistingPagesBeginWith(SiteData site) {
			int iTot = 0;

			string p1 = string.Format("/{0}/{1}", site.Blog_FolderPath, site.Blog_CategoryPath).Replace("//", "/");
			iTot += GetPagesBeginingWith(site.SiteID, p1).Count();
			if (iTot > 0) {
				return true;
			}

			p1 = string.Format("/{0}/{1}", site.Blog_FolderPath, site.Blog_TagPath).Replace("//", "/");
			iTot += GetPagesBeginingWith(site.SiteID, p1).Count();
			if (iTot > 0) {
				return true;
			}

			p1 = string.Format("/{0}/{1}", site.Blog_FolderPath, site.Blog_DatePath).Replace("//", "/");
			iTot += GetPagesBeginingWith(site.SiteID, p1).Count();
			if (iTot > 0) {
				return true;
			}

			p1 = string.Format("/{0}/{1}", site.Blog_FolderPath, site.Blog_EditorPath).Replace("//", "/");
			iTot += GetPagesBeginingWith(site.SiteID, p1).Count();
			if (iTot > 0) {
				return true;
			}

			string p2 = string.Format("/{0}", site.Blog_FolderPath).ToLowerInvariant().Replace("//", "/");
			iTot += GetPagesEquals(site.SiteID, p2).Count();

			return (iTot) > 0;
		}

		private IQueryable<vw_carrot_Content> GetPagesBeginingWith(Guid siteID, string sFolderPath) {
			sFolderPath = sFolderPath.ToLowerInvariant().Trim().Replace("//", "/");

			IQueryable<vw_carrot_Content> query = (from ct in db.vw_carrot_Contents
												   where ct.SiteID == siteID
														&& ct.FileName.StartsWith(sFolderPath)
														&& ct.IsLatestVersion == true
												   select ct);

			return query;
		}

		private IQueryable<vw_carrot_Content> GetPagesEquals(Guid siteID, string sFolderPath) {
			sFolderPath = sFolderPath.ToLowerInvariant().Trim().Replace("//", "/");

			IQueryable<vw_carrot_Content> query = (from ct in db.vw_carrot_Contents
												   where ct.SiteID == siteID
														&& ct.FileName == sFolderPath
														&& ct.IsLatestVersion == true
												   select ct);

			return query;
		}

		public ContentPage FindHome(Guid siteID) {
			ContentPage content = null;
			vw_carrot_Content cont = CompiledQueries.FindHome(db, siteID, true);
			if (cont != null) {
				content = new ContentPage(cont);
			}
			return content;
		}

		public ContentPage FindHome(Guid siteID, bool bActiveOnly) {
			ContentPage content = null;
			vw_carrot_Content cont = CompiledQueries.FindHome(db, siteID, bActiveOnly);
			if (cont != null) {
				content = new ContentPage(cont);
			}
			return content;
		}

		public List<ContentPage> FindPageByTitleAndDate(Guid siteID, string sTitle, string sFileNameFrag, DateTime dateCreate) {
			SiteData site = SiteData.GetSiteFromCache(siteID);

			//DateTime dateUTC = site.ConvertSiteTimeToUTC(dateCreate);

			List<ContentPage> lstContent = (from ct in CannedQueries.FindPageByTitleAndDate(db, siteID, sTitle, sFileNameFrag, dateCreate).ToList()
											select new ContentPage(ct)).ToList();

			return lstContent;
		}

		public List<ContentPage> GetChildNavigation(Guid siteID, string sParentPage, bool bActiveOnly) {
			List<ContentPage> lstContent = (from ct in CompiledQueries.GetLatestContentByParent(db, siteID, sParentPage, bActiveOnly).ToList()
											select new ContentPage(ct)).ToList();

			return lstContent;
		}

		public List<ContentPage> GetChildNavigation(Guid siteID, Guid? ParentID, bool bActiveOnly) {
			List<ContentPage> lstContent = (from ct in CompiledQueries.GetLatestContentByParent(db, siteID, ParentID, bActiveOnly).ToList()
											select new ContentPage(ct)).ToList();

			return lstContent;
		}

		public List<ContentPage> GetParentWithChildNavigation(Guid siteID, Guid? ParentID, bool bActiveOnly) {
			List<ContentPage> lstContent = (from ct in CompiledQueries.GetLatestContentWithParent(db, siteID, ParentID, bActiveOnly).ToList()
											select new ContentPage(ct)).ToList();

			return lstContent;
		}

		public List<ContentPage> GetTopNavigation(Guid siteID, bool bActiveOnly) {
			List<ContentPage> lstContent = CompiledQueries.TopLevelPages(db, siteID, bActiveOnly).Select(ct => new ContentPage(ct)).ToList();

			return lstContent;
		}

		public List<ContentPage> GetPostsByDateRange(Guid siteID, DateTime dateMidpoint, int iDayRange, bool bActiveOnly) {
			DateTime dateBegin = dateMidpoint.AddDays(0 - iDayRange);
			DateTime dateEnd = dateMidpoint.AddDays(iDayRange);

			List<ContentPage> lstContent = CompiledQueries.PostsByDateRange(db, siteID, dateBegin, dateEnd, bActiveOnly).Select(ct => new ContentPage(ct)).ToList();

			return lstContent;
		}

		public List<ContentPage> GetContentByDateRange(Guid siteID, DateTime dateMidpoint, int iDayRange, ContentPageType.PageType pageType, bool? bActive, bool? bSiteMap, bool? bSiteNav, bool? bBlock) {
			//assiging SQL min dates because of 1753 vs 0001 year issues
			DateTime dateBegin = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue; //DateTime.MinValue;
			DateTime dateEnd = (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue; //DateTime.MaxValue;

			if (iDayRange > 0) {
				dateBegin = dateMidpoint.AddDays(0 - iDayRange);
				dateEnd = dateMidpoint.AddDays(iDayRange);
			}

			List<ContentPage> lstContent = CannedQueries.GetContentByStatusAndDateRange(db, siteID, pageType, dateBegin, dateEnd, bActive, bSiteMap, bSiteNav, bBlock).Select(ct => new ContentPage(ct)).ToList();

			return lstContent;
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