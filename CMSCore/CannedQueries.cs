using Carrotware.CMS.Data;
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

	static internal class CannedQueries {

		internal static IQueryable<vw_carrot_Content> GetAllByTypeList(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, ContentPageType.PageType entryType) {
			return (from ct in ctx.vw_carrot_Contents
					orderby ct.NavOrder, ct.NavMenuText
					where ct.SiteID == siteID
					&& ct.IsLatestVersion == true
					&& (ct.PageActive == true || bActiveOnly == false)
					&& (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
					&& (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
					&& ct.IsLatestVersion == true
					&& ct.ContentTypeID == ContentPageType.GetIDByType(entryType)
					select ct);
		}

		internal static IQueryable<vw_carrot_ContentSnippet> GetSnippets(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly) {
			return (from ct in ctx.vw_carrot_ContentSnippets
					orderby ct.ContentSnippetName
					where ct.SiteID == siteID
					&& ct.IsLatestVersion == true
					&& (ct.ContentSnippetActive == true || bActiveOnly == false)
					&& (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
					&& (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
					&& ct.IsLatestVersion == true
					select ct);
		}

		internal static IQueryable<vw_carrot_Content> GetAllContentList(CarrotCMSDataContext ctx, Guid siteID) {
			return (from ct in ctx.vw_carrot_Contents
					orderby ct.NavOrder, ct.NavMenuText
					where ct.SiteID == siteID
					 && ct.IsLatestVersion == true
					 && ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry)
					select ct);
		}

		internal static IQueryable<vw_carrot_Content> GetAllBlogList(CarrotCMSDataContext ctx, Guid siteID) {
			return (from ct in ctx.vw_carrot_Contents
					orderby ct.NavOrder, ct.NavMenuText
					where ct.SiteID == siteID
					 && ct.IsLatestVersion == true
					 && ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry)
					select ct);
		}

		internal static IQueryable<vw_carrot_Content> FindPageByTitleAndDate(CarrotCMSDataContext ctx, Guid siteID, string sTitle, string sFileNameFrag, DateTime dateCreate) {
			return (from ct in ctx.vw_carrot_Contents
					orderby ct.NavOrder, ct.NavMenuText
					where ct.SiteID == siteID
					 && ct.IsLatestVersion == true
					 && (ct.PageHead == sTitle || ct.TitleBar == sTitle)
					 && ct.FileName.Contains(sFileNameFrag)
					 && ct.CreateDate.Date == dateCreate.Date
					select ct);
		}

		internal static IQueryable<vw_carrot_Content> GetLatestContentList(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly) {
			return (from ct in ctx.vw_carrot_Contents
					orderby ct.NavOrder, ct.NavMenuText
					where ct.SiteID == siteID
					 && ct.IsLatestVersion == true
					 && ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry)
					 && (ct.PageActive == true || bActiveOnly == false)
					 && (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
					 && (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
					select ct);
		}

		internal static IQueryable<carrot_WidgetData> GetWidgetDataByRootAll(CarrotCMSDataContext ctx, Guid rootWidgetID) {
			return (from r in ctx.carrot_WidgetDatas
					where r.Root_WidgetID == rootWidgetID
					select r);
		}

		internal static IQueryable<carrot_Widget> GetWidgetsByRootContent(CarrotCMSDataContext ctx, Guid rootContentID) {
			return (from r in ctx.carrot_Widgets
					where r.Root_ContentID == rootContentID
					select r);
		}

		internal static IQueryable<vw_carrot_Content> GetContentByRoot(CarrotCMSDataContext ctx, Guid rootContentID) {
			return (from r in ctx.vw_carrot_Contents
					where r.Root_ContentID == rootContentID
					select r);
		}

		internal static IQueryable<vw_carrot_Content> GetContentByStatusAndDateRange(CarrotCMSDataContext ctx, Guid siteID, ContentPageType.PageType pageType,
			DateTime dateBegin, DateTime dateEnd, bool? bActive, bool? bSiteMap, bool? bSiteNav, bool? bBlock) {
			Guid gContent = ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry);
			Guid gBlog = ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry);
			Guid contentTypeID = ContentPageType.GetIDByType(pageType);

			return (from ct in ctx.vw_carrot_Contents
					orderby ct.ContentTypeValue, ct.NavMenuText
					where ct.SiteID == siteID
						&& ct.IsLatestVersion == true
						&& ct.GoLiveDate >= dateBegin
						&& ct.GoLiveDate <= dateEnd
						&& (ct.ContentTypeID == contentTypeID || pageType == ContentPageType.PageType.Unknown)
						&& (ct.PageActive == Convert.ToBoolean(bActive) || bActive == null)
						&& (ct.BlockIndex == Convert.ToBoolean(bBlock) || bBlock == null)
						&& ((ct.ShowInSiteMap == Convert.ToBoolean(bSiteMap) && ct.ContentTypeID == gContent) || bSiteMap == null)
						&& ((ct.ShowInSiteNav == Convert.ToBoolean(bSiteNav) && ct.ContentTypeID == gContent) || bSiteNav == null)
					select ct);
		}

		internal static IQueryable<vw_carrot_Content> GetLatestBlogListDateRange(CarrotCMSDataContext ctx, Guid siteID, DateTime dateBegin, DateTime dateEnd, bool bActiveOnly) {
			return (from ct in ctx.vw_carrot_Contents
					where ct.SiteID == siteID
					 && ct.IsLatestVersion == true
					 && ct.GoLiveDate >= dateBegin
					 && ct.GoLiveDate <= dateEnd
					 && ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry)
					 && (ct.PageActive == true || bActiveOnly == false)
					 && (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
					 && (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
					select ct);
		}

		internal static IQueryable<vw_carrot_Content> GetLatestBlogList(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly) {
			return (from ct in ctx.vw_carrot_Contents
					orderby ct.NavOrder, ct.NavMenuText
					where ct.SiteID == siteID
					 && ct.IsLatestVersion == true
					 && ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry)
					 && (ct.PageActive == true || bActiveOnly == false)
					 && (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
					 && (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
					select ct);
		}

		internal static Dictionary<string, float> GetTemplateCounts(CarrotCMSDataContext ctx, Guid siteID, ContentPageType.PageType pageType) {
			Guid contentTypeID = ContentPageType.GetIDByType(pageType);

			return (from ct in ctx.vw_carrot_Contents.Where(c => c.SiteID == siteID && c.ContentTypeID == contentTypeID && c.IsLatestVersion == true)
					group ct by ct.TemplateFile into grp
					orderby grp.Count() descending
					select new KeyValuePair<string, float>(grp.Key, (float)grp.Count()))
					.ToDictionary(t => t.Key, t => t.Value);
		}

		internal static IQueryable<vw_carrot_Content> GetContentByTagURL(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, string sTagURL) {
			return (from t in ctx.vw_carrot_TagURLs
					join m in ctx.carrot_TagContentMappings on t.ContentTagID equals m.ContentTagID
					join ct in ctx.vw_carrot_Contents on m.Root_ContentID equals ct.Root_ContentID
					where t.SiteID == siteID
						&& ct.SiteID == siteID
						&& t.TagUrl == sTagURL
						&& ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry)
						&& (ct.PageActive == true || bActiveOnly == false)
						&& (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
						&& (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
						&& ct.IsLatestVersion == true
					select ct);
		}

		internal static IQueryable<vw_carrot_Content> GetContentByCategoryURL(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, string sCatURL) {
			return (from c in ctx.vw_carrot_CategoryURLs
					join m in ctx.carrot_CategoryContentMappings on c.ContentCategoryID equals m.ContentCategoryID
					join ct in ctx.vw_carrot_Contents on m.Root_ContentID equals ct.Root_ContentID
					where c.SiteID == siteID
						&& ct.SiteID == siteID
						&& c.CategoryUrl == sCatURL
						&& ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry)
						&& (ct.PageActive == true || bActiveOnly == false)
						&& (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
						&& (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
						&& ct.IsLatestVersion == true
					select ct);
		}

		internal static IQueryable<vw_carrot_Content> GetContentByUserURL(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, string sUserURL) {
			return (from ed in ctx.vw_carrot_EditorURLs
					join ct in ctx.vw_carrot_Contents on ed.SiteID equals ct.SiteID
					where ed.SiteID == siteID
						&& ct.SiteID == siteID
						&& ed.UserUrl == sUserURL
						&& ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry)
						&& (ct.PageActive == true || bActiveOnly == false)
						&& (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
						&& (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
						&& ct.IsLatestVersion == true
						&& ((ed.UserId == ct.EditUserId && ct.CreditUserId == null)
									|| (ed.UserId == ct.CreditUserId && ct.CreditUserId != null))
					select ct);
		}

		internal static IQueryable<vw_carrot_Content> GetContentByCategoryIDs(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, List<Guid> lstCategories) {
			return GetContentByCategoryIDs(ctx, siteID, bActiveOnly, lstCategories, new List<string>());
		}

		internal static IQueryable<vw_carrot_Content> GetContentByCategoryIDs(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, List<Guid> lstCategoryGUIDs, List<string> lstCategorySlugs) {
			if (lstCategoryGUIDs == null) {
				lstCategoryGUIDs = new List<Guid>();
			}
			if (lstCategorySlugs == null) {
				lstCategorySlugs = new List<string>();
			}

			return (from ct in ctx.vw_carrot_Contents
					where ct.SiteID == siteID
						&& ((from m in ctx.carrot_CategoryContentMappings
							 join cc in ctx.carrot_ContentCategories on m.ContentCategoryID equals cc.ContentCategoryID
							 where cc.SiteID == siteID
									&& lstCategorySlugs.Contains(cc.CategorySlug)
							 select m.Root_ContentID).Contains(ct.Root_ContentID)
						|| (from m in ctx.carrot_CategoryContentMappings
							where lstCategoryGUIDs.Contains(m.ContentCategoryID)
							select m.Root_ContentID).Contains(ct.Root_ContentID))
						&& ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry)
						&& (ct.PageActive == true || bActiveOnly == false)
						&& (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
						&& (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
						&& ct.IsLatestVersion == true
					select ct);
		}

		internal static IQueryable<vw_carrot_Content> GetContentSiteSearch(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, string searchTerm) {
			return (from ct in ctx.vw_carrot_Contents
					where ct.SiteID == siteID
						&& (ct.PageText.Contains(searchTerm)
								|| ct.LeftPageText.Contains(searchTerm)
								|| ct.RightPageText.Contains(searchTerm)
								|| ct.TitleBar.Contains(searchTerm)
								|| ct.MetaDescription.Contains(searchTerm)
								|| ct.MetaKeyword.Contains(searchTerm)
							)
						&& (ct.PageActive == true || bActiveOnly == false)
						&& (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
						&& (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
						&& ct.IsLatestVersion == true
					select ct);
		}

		internal static IQueryable<vw_carrot_Content> GetLatestContentByParent(CarrotCMSDataContext ctx, Guid siteID, Guid? parentContentID, bool bActiveOnly) {
			return (from ct in ctx.vw_carrot_Contents
					orderby ct.NavOrder, ct.NavMenuText
					where ct.SiteID == siteID
						   && ct.Parent_ContentID == parentContentID
						   && ct.Parent_ContentID != null
						   && ct.IsLatestVersion == true
						   && (ct.PageActive == true || bActiveOnly == false)
						   && (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
						   && (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
					select ct);
		}

		internal static IQueryable<vw_carrot_Content> GetLatestContentByParent(CarrotCMSDataContext ctx, Guid siteID, string parentPage, bool bActiveOnly) {
			return (from ct in ctx.vw_carrot_Contents
					join cp in ctx.vw_carrot_ContentChilds on ct.Root_ContentID equals cp.Root_ContentID
					orderby ct.NavOrder, ct.NavMenuText
					where ct.SiteID == siteID
						   && cp.ParentFileName == parentPage
						   && ct.IsLatestVersion == true
						   && (ct.PageActive == true || bActiveOnly == false)
						   && (ct.GoLiveDate < DateTime.UtcNow || bActiveOnly == false)
						   && (ct.RetireDate > DateTime.UtcNow || bActiveOnly == false)
					select ct);
		}

		internal static IQueryable<carrot_CategoryContentMapping> GetContentCategoryMapByContentID(CarrotCMSDataContext ctx, Guid rootContentID) {
			return (from r in ctx.carrot_ContentCategories
					join c in ctx.carrot_CategoryContentMappings on r.ContentCategoryID equals c.ContentCategoryID
					where c.Root_ContentID == rootContentID
					select c);
		}

		internal static IQueryable<carrot_TagContentMapping> GetContentTagMapByContentID(CarrotCMSDataContext ctx, Guid rootContentID) {
			return (from r in ctx.carrot_ContentTags
					join c in ctx.carrot_TagContentMappings on r.ContentTagID equals c.ContentTagID
					where c.Root_ContentID == rootContentID
					select c);
		}

		internal static IQueryable<carrot_Content> GetBlogAllContentTbl(CarrotCMSDataContext ctx, Guid siteID) {
			return (from ct in ctx.carrot_RootContents
					join c in ctx.carrot_Contents on ct.Root_ContentID equals c.Root_ContentID
					where ct.SiteID == siteID
					where c.IsLatestVersion == true
						 && ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry)
					select c);
		}

		internal static IQueryable<carrot_RootContent> GetBlogAllRootTbl(CarrotCMSDataContext ctx, Guid siteID) {
			return (from ct in ctx.carrot_RootContents
					where ct.SiteID == siteID
					where ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry)
					select ct);
		}

		internal static IQueryable<carrot_RootContent> GetContentAllRootTbl(CarrotCMSDataContext ctx, Guid siteID) {
			return (from ct in ctx.carrot_RootContents
					where ct.SiteID == siteID
					where ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry)
					select ct);
		}

		internal static IQueryable<carrot_RootContent> GetAllRootTbl(CarrotCMSDataContext ctx, Guid siteID) {
			return (from ct in ctx.carrot_RootContents
					where ct.SiteID == siteID
					select ct);
		}

		internal static IQueryable<DateTime> GetAllDates(CarrotCMSDataContext ctx, Guid siteID) {
			return (from ct in ctx.carrot_RootContents
					where ct.SiteID == siteID
					select ct.GoLiveDate).Distinct();
		}

		internal static IQueryable<DateTime> GetAllDatesByType(CarrotCMSDataContext ctx, Guid siteID, ContentPageType.PageType pageType) {
			var pageTypeId = ContentPageType.GetIDByType(pageType);

			return (from ct in ctx.carrot_RootContents
					where ct.SiteID == siteID && ct.ContentTypeID == pageTypeId
					select ct.GoLiveDate).Distinct();
		}

		internal static IQueryable<carrot_Content> GetContentAllContentTbl(CarrotCMSDataContext ctx, Guid siteID) {
			return (from ct in ctx.carrot_RootContents
					join c in ctx.carrot_Contents on ct.Root_ContentID equals c.Root_ContentID
					where ct.SiteID == siteID
					where c.IsLatestVersion == true
						 && ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry)
					select c);
		}

		internal static IQueryable<carrot_Content> GetContentTopContentTbl(CarrotCMSDataContext ctx, Guid siteID) {
			return (from ct in ctx.carrot_RootContents
					join c in ctx.carrot_Contents on ct.Root_ContentID equals c.Root_ContentID
					where ct.SiteID == siteID
					where c.IsLatestVersion == true
						&& c.Parent_ContentID == null
						&& ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry)
					select c);
		}

		internal static IQueryable<carrot_Content> GetContentSubContentTbl(CarrotCMSDataContext ctx, Guid siteID) {
			return (from ct in ctx.carrot_RootContents
					join c in ctx.carrot_Contents on ct.Root_ContentID equals c.Root_ContentID
					where ct.SiteID == siteID
					where c.IsLatestVersion == true
						&& c.Parent_ContentID != null
						&& ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry)
					select c);
		}

		internal static IQueryable<vw_carrot_CategoryURL> GetCategoryURLs(CarrotCMSDataContext ctx, Guid siteID) {
			return (from ct in ctx.vw_carrot_CategoryURLs
					where ct.SiteID == siteID
					select ct);
		}

		internal static IQueryable<vw_carrot_TagURL> GetTagURLs(CarrotCMSDataContext ctx, Guid siteID) {
			return (from ct in ctx.vw_carrot_TagURLs
					where ct.SiteID == siteID
					select ct);
		}

		internal static IQueryable<vw_carrot_CategoryURL> GetPostCategoryURL(CarrotCMSDataContext ctx, Guid siteID, string urlFileName) {
			return (from ct in ctx.vw_carrot_CategoryURLs
					join m in ctx.carrot_CategoryContentMappings on ct.ContentCategoryID equals m.ContentCategoryID
					join c in ctx.carrot_RootContents on m.Root_ContentID equals c.Root_ContentID
					where c.FileName == urlFileName
						&& ct.SiteID == siteID
					select ct);
		}

		internal static IQueryable<vw_carrot_TagURL> GetPostTagURLs(CarrotCMSDataContext ctx, Guid siteID, string urlFileName) {
			return (from ct in ctx.vw_carrot_TagURLs
					join m in ctx.carrot_TagContentMappings on ct.ContentTagID equals m.ContentTagID
					join c in ctx.carrot_RootContents on m.Root_ContentID equals c.Root_ContentID
					where c.FileName == urlFileName
						&& ct.SiteID == siteID
					select ct);
		}

		internal static IQueryable<vw_carrot_CategoryURL> GetPostCategoryURL(CarrotCMSDataContext ctx, Guid siteID, Guid rootContentID) {
			return (from ct in ctx.vw_carrot_CategoryURLs
					join m in ctx.carrot_CategoryContentMappings on ct.ContentCategoryID equals m.ContentCategoryID
					where m.Root_ContentID == rootContentID
						&& ct.SiteID == siteID
					select ct);
		}

		internal static IQueryable<vw_carrot_TagURL> GetPostTagURLs(CarrotCMSDataContext ctx, Guid siteID, Guid rootContentID) {
			return (from ct in ctx.vw_carrot_TagURLs
					join m in ctx.carrot_TagContentMappings on ct.ContentTagID equals m.ContentTagID
					where m.Root_ContentID == rootContentID
						&& ct.SiteID == siteID
					select ct);
		}

		internal static IQueryable<vw_carrot_Comment> GetSiteContentComments(CarrotCMSDataContext ctx, Guid siteID) {
			return (from r in ctx.vw_carrot_Comments
					orderby r.CreateDate descending
					where r.SiteID == siteID
					select r);
		}

		internal static IQueryable<vw_carrot_Comment> GetContentPageComments(CarrotCMSDataContext ctx, Guid rootContentID, bool bActiveOnly) {
			return (from r in ctx.vw_carrot_Comments
					orderby r.CreateDate descending
					where r.Root_ContentID == rootContentID
						&& (r.IsApproved == true || bActiveOnly == false)
					select r);
		}

		internal static IQueryable<vw_carrot_Comment> GetContentPageComments(CarrotCMSDataContext ctx, Guid rootContentID, bool? approved, bool? spam) {
			return (from r in ctx.vw_carrot_Comments
					orderby r.CreateDate descending
					where r.Root_ContentID == rootContentID
						   && (spam == null || r.IsSpam == spam)
						   && (approved == null || r.IsApproved == approved)
					select r);
		}

		internal static IQueryable<vw_carrot_Comment> FindCommentsByDate(CarrotCMSDataContext ctx, Guid siteID, Guid rootContentID, DateTime postDate, string postIP, string sCommentText) {
			return (from r in ctx.vw_carrot_Comments
					orderby r.CreateDate descending
					where r.SiteID == siteID
						&& r.Root_ContentID == rootContentID
						&& r.CreateDate.Date == postDate.Date
						&& r.CommenterIP == postIP
						&& r.PostComment.Trim() == sCommentText.Trim()
					select r);
		}

		internal static IQueryable<vw_carrot_Comment> FindCommentsByDate(CarrotCMSDataContext ctx, Guid siteID, Guid rootContentID, DateTime postDate, string postIP) {
			return (from r in ctx.vw_carrot_Comments
					orderby r.CreateDate descending
					where r.SiteID == siteID
						&& r.Root_ContentID == rootContentID
						&& r.CreateDate.Date == postDate.Date
						&& r.CreateDate.Hour == postDate.Hour
						&& r.CreateDate.Minute == postDate.Minute
						&& r.CommenterIP == postIP
					select r);
		}

		internal static IQueryable<vw_carrot_Comment> GetSiteContentCommentsByPostType(CarrotCMSDataContext ctx, Guid siteID, ContentPageType.PageType contentEntry) {
			return (from r in ctx.vw_carrot_Comments
					orderby r.CreateDate descending
					where r.SiteID == siteID
						&& r.ContentTypeID == ContentPageType.GetIDByType(contentEntry)
					select r);
		}

		internal static IQueryable<vw_carrot_Comment> GetSiteContentCommentsByPostType(CarrotCMSDataContext ctx, Guid siteID, ContentPageType.PageType contentEntry, bool? approved, bool? spam) {
			return (from r in ctx.vw_carrot_Comments
					orderby r.CreateDate descending
					where r.SiteID == siteID
						   && (spam == null || r.IsSpam == spam)
						   && (approved == null || r.IsApproved == approved)
						&& r.ContentTypeID == ContentPageType.GetIDByType(contentEntry)
					select r);
		}
	}
}