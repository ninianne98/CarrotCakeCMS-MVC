using Carrotware.CMS.Data;
using System;
using System.Data.Linq;
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

	public static class CompiledQueries {
		//internal static CarrotCMSDataContext dbConn = new CarrotCMSDataContext();

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid, carrot_RootContent> cqGetRootContentTbl =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID, Guid rootContentID) =>
					  (from r in ctx.carrot_RootContents
					   where r.SiteID == siteID
						   && r.Root_ContentID == rootContentID
					   select r).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, carrot_Site> cqGetSiteFromRootContentID =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid rootContentID) =>
					  (from r in ctx.carrot_RootContents
					   join s in ctx.carrot_Sites on r.SiteID equals s.SiteID
					   where r.Root_ContentID == rootContentID
					   select s).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid, carrot_Content> cqGetLatestContentTbl =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID, Guid rootContentID) =>
					  (from ct in ctx.carrot_Contents
					   join r in ctx.carrot_RootContents on ct.Root_ContentID equals r.Root_ContentID
					   where r.SiteID == siteID
						   && r.Root_ContentID == rootContentID
						   && ct.IsLatestVersion == true
					   select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid, IQueryable<vw_carrot_Content>> cqGetVersionHistory =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, Guid siteID, Guid rootContentID) =>
					(from ct in ctx.vw_carrot_Contents
					 orderby ct.EditDate descending
					 where ct.SiteID == siteID
						&& ct.Root_ContentID == rootContentID
					 select ct));

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid, vw_carrot_Content> cqGetContentByContentID =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, Guid siteID, Guid contentID) =>
					(from ct in ctx.vw_carrot_Contents
					 orderby ct.EditDate descending
					 where ct.SiteID == siteID
						&& ct.ContentID == contentID
					 select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, int> cqGetMaxOrderID =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, Guid siteID) =>
					(from ct in ctx.vw_carrot_Contents.Where(c => c.SiteID == siteID && c.IsLatestVersion == true)
					 select ct.NavOrder).Max());

		internal static IQueryable<vw_carrot_Content> TopLevelPages(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				DateCompare = DateTime.UtcNow,
				ContentTypeID = ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry),
				ContentType = ContentPageType.PageType.ContentEntry,
				ActiveOnly = bActiveOnly
			};

			return cqTopLevelPages(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, IQueryable<vw_carrot_Content>> cqTopLevelPages =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					(from ct in ctx.vw_carrot_Contents
					 orderby ct.NavOrder, ct.NavMenuText
					 where ct.SiteID == sp.SiteID
						 && ct.Parent_ContentID == null
						 && ct.IsLatestVersion == true
						 && ct.ContentTypeID == sp.ContentTypeID
						 && (ct.PageActive == true || sp.ActiveOnly == false)
						 && (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
						 && (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					 select ct));

		internal static IQueryable<vw_carrot_Content> PostsByDateRange(CarrotCMSDataContext ctx, Guid siteID, DateTime dateBegin, DateTime dateEnd, bool bActiveOnly) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				DateCompare = DateTime.UtcNow,
				ContentTypeID = ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry),
				ContentType = ContentPageType.PageType.BlogEntry,
				DateBegin = dateBegin,
				DateEnd = dateEnd,
				ActiveOnly = bActiveOnly
			};

			return cqPostsByDateRange(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, IQueryable<vw_carrot_Content>> cqPostsByDateRange =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					(from ct in ctx.vw_carrot_Contents
					 orderby ct.NavOrder, ct.NavMenuText
					 where ct.SiteID == sp.SiteID
						&& ct.IsLatestVersion == true
						&& (ct.GoLiveDate >= sp.DateBegin && ct.GoLiveDate <= sp.DateEnd)
						&& ct.ContentTypeID == sp.ContentTypeID
						&& (ct.PageActive == true || sp.ActiveOnly == false)
						&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
						&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					 select ct));

		internal static vw_carrot_Content GetLatestContentByURL(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, string sPage) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				DateCompare = DateTime.UtcNow,
				ActiveOnly = bActiveOnly,
				FileName = sPage
			};
			return cqGetLatestContentByURL(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, vw_carrot_Content> cqGetLatestContentByURL =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					  (from ct in ctx.vw_carrot_Contents
					   where ct.SiteID == sp.SiteID
							&& ct.FileName.ToLower() == sp.FileName.ToLower()
							&& ct.IsLatestVersion == true
							&& (ct.PageActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					   select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, DateTime, string, vw_carrot_Content> cqGetLatestContentBySlug =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID, DateTime datePublished, string sPageSlug) =>
					  (from ct in ctx.vw_carrot_Contents
					   where ct.SiteID == siteID
							&& ct.PageSlug.ToLower() == sPageSlug.ToLower()
							&& (ct.GoLiveDate.Date == datePublished.Date)
							&& ct.IsLatestVersion == true
					   select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, string, vw_carrot_CategoryURL> cqGetCategoryByURL =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID, string sPage) =>
					  (from ct in ctx.vw_carrot_CategoryURLs
					   where ct.SiteID == siteID
							&& ct.CategoryUrl.ToLower() == sPage.ToLower()
					   select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, string, vw_carrot_TagURL> cqGetTagByURL =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID, string sPage) =>
					  (from ct in ctx.vw_carrot_TagURLs
					   where ct.SiteID == siteID
							&& ct.TagUrl.ToLower() == sPage.ToLower()
					   select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, string, vw_carrot_EditorURL> cqGetEditorByURL =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID, string sPage) =>
					  (from ct in ctx.vw_carrot_EditorURLs
					   where ct.SiteID == siteID
							&& ct.UserUrl.ToLower() == sPage.ToLower()
					   select ct).FirstOrDefault());

		internal static vw_carrot_Content GetLatestContentByID(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, Guid rootContentID) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				RootContentID = rootContentID,
				DateCompare = DateTime.UtcNow,
				ActiveOnly = bActiveOnly
			};

			return cqGetLatestContentByID(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, vw_carrot_Content> cqGetLatestContentByID =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					(from ct in ctx.vw_carrot_Contents
					 where ct.SiteID == sp.SiteID
							&& ct.Root_ContentID == sp.RootContentID
							&& ct.IsLatestVersion == true
							&& (ct.PageActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					 select ct).FirstOrDefault());

		internal static IQueryable<vw_carrot_Content> GetLatestContentByParent(CarrotCMSDataContext ctx, Guid siteID, Guid? parentContentID, bool bActiveOnly) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				ParentContentID = parentContentID,
				DateCompare = DateTime.UtcNow,
				ActiveOnly = bActiveOnly
			};

			return cqGetLatestContentByParent1(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, IQueryable<vw_carrot_Content>> cqGetLatestContentByParent1 =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					(from ct in ctx.vw_carrot_Contents
					 orderby ct.NavOrder, ct.NavMenuText
					 where ct.SiteID == sp.SiteID
							&& ct.Parent_ContentID == sp.ParentContentID
							&& ct.IsLatestVersion == true
							&& (ct.PageActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					 select ct));

		internal static IQueryable<vw_carrot_Content> GetLatestContentByParent(CarrotCMSDataContext ctx, Guid siteID, string parentPage, bool bActiveOnly) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				ParentFileName = parentPage,
				DateCompare = DateTime.UtcNow,
				ActiveOnly = bActiveOnly
			};

			return cqGetLatestContentByParent2(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, IQueryable<vw_carrot_Content>> cqGetLatestContentByParent2 =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					(from ct in ctx.vw_carrot_Contents
					 join cp in ctx.vw_carrot_ContentChilds on ct.Root_ContentID equals cp.Root_ContentID
					 orderby ct.NavOrder, ct.NavMenuText
					 where ct.SiteID == sp.SiteID
							&& cp.ParentFileName.ToLower() == sp.ParentFileName.ToLower()
							&& ct.IsLatestVersion == true
							&& (ct.PageActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					 select ct));

		internal static IQueryable<vw_carrot_Content> GetLatestContentBySibling(CarrotCMSDataContext ctx, Guid siteID, Guid rootContentID, bool bActiveOnly) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				RootContentID = rootContentID,
				DateCompare = DateTime.UtcNow,
				ActiveOnly = bActiveOnly
			};

			return cqGetLatestContentBySibling1(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, IQueryable<vw_carrot_Content>> cqGetLatestContentBySibling1 =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					(from cc1 in ctx.vw_carrot_ContentChilds
					 join cc2 in ctx.vw_carrot_ContentChilds on cc1.Parent_ContentID equals cc2.Parent_ContentID
					 join ct in ctx.vw_carrot_Contents on cc2.Root_ContentID equals ct.Root_ContentID
					 orderby ct.NavOrder, ct.NavMenuText
					 where ct.SiteID == sp.SiteID
							&& cc1.Root_ContentID == sp.RootContentID
							&& cc1.SiteID == sp.SiteID
							&& cc2.SiteID == sp.SiteID
							&& ct.IsLatestVersion == true
							&& (ct.PageActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					 select ct));

		internal static IQueryable<vw_carrot_Content> GetLatestContentBySibling(CarrotCMSDataContext ctx, Guid siteID, string sPage, bool bActiveOnly) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				FileName = sPage,
				DateCompare = DateTime.UtcNow,
				ActiveOnly = bActiveOnly
			};

			return cqGetLatestContentBySibling2(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, IQueryable<vw_carrot_Content>> cqGetLatestContentBySibling2 =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					(from cc1 in ctx.vw_carrot_ContentChilds
					 join cc2 in ctx.vw_carrot_ContentChilds on cc1.Parent_ContentID equals cc2.Parent_ContentID
					 join ct in ctx.vw_carrot_Contents on cc2.Root_ContentID equals ct.Root_ContentID
					 orderby ct.NavOrder, ct.NavMenuText
					 where ct.SiteID == sp.SiteID
							&& cc1.FileName.ToLower() == sp.FileName.ToLower()
							&& cc1.SiteID == sp.SiteID
							&& cc2.SiteID == sp.SiteID
							&& ct.IsLatestVersion == true
							&& (ct.PageActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					 select ct));

		internal static int GetContentCountByParent(CarrotCMSDataContext ctx, Guid siteID, Guid? parentContentID, bool bActiveOnly) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				ParentContentID = parentContentID,
				DateCompare = DateTime.UtcNow,
				ActiveOnly = bActiveOnly
			};

			return cqGetContentCountByParent1(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, int> cqGetContentCountByParent1 =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					(from ct in ctx.vw_carrot_Contents
					 orderby ct.NavOrder, ct.NavMenuText
					 where ct.SiteID == sp.SiteID
							&& ct.Parent_ContentID == sp.ParentContentID
							&& ct.IsLatestVersion == true
							&& (ct.PageActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					 select ct).Count());

		internal static int GetContentCountByParent(CarrotCMSDataContext ctx, Guid siteID, string parentPage, bool bActiveOnly) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				ParentFileName = parentPage,
				DateCompare = DateTime.UtcNow,
				ActiveOnly = bActiveOnly
			};

			return cqGetContentCountByParent2(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, int> cqGetContentCountByParent2 =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					(from ct in ctx.vw_carrot_Contents
					 join cp in ctx.vw_carrot_ContentChilds on ct.Root_ContentID equals cp.Root_ContentID
					 orderby ct.NavOrder, ct.NavMenuText
					 where ct.SiteID == sp.SiteID
							&& cp.ParentFileName.ToLower() == sp.ParentFileName.ToLower()
							&& ct.IsLatestVersion == true
							&& (ct.PageActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					 select ct).Count());

		internal static IQueryable<vw_carrot_Content> GetLatestContentWithParent(CarrotCMSDataContext ctx, Guid siteID, Guid? parentContentID, bool bActiveOnly) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				ParentContentID = parentContentID,
				ContentTypeID = ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry),
				ContentType = ContentPageType.PageType.ContentEntry,
				DateCompare = DateTime.UtcNow,
				ActiveOnly = bActiveOnly
			};

			return cqGetLatestContentWithParent(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, IQueryable<vw_carrot_Content>> cqGetLatestContentWithParent =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					(from ct in ctx.vw_carrot_Contents
					 orderby ct.NavOrder, ct.NavMenuText
					 where ct.SiteID == sp.SiteID
							&& (ct.Parent_ContentID == sp.ParentContentID || ct.Root_ContentID == sp.ParentContentID)
							&& ct.IsLatestVersion == true
							&& ct.ContentTypeID == sp.ContentTypeID
							&& (ct.PageActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					 select ct));

		internal static IQueryable<vw_carrot_Content> ContentNavAll(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				DateCompare = DateTime.UtcNow,
				ContentTypeID = ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry),
				ContentType = ContentPageType.PageType.ContentEntry,
				ActiveOnly = bActiveOnly
			};

			return cqContentNavAll(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, IQueryable<vw_carrot_Content>> cqContentNavAll =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					  (from ct in ctx.vw_carrot_Contents
					   orderby ct.NavOrder, ct.NavMenuText
					   where ct.SiteID == sp.SiteID
							&& ct.IsLatestVersion == true
							&& ct.ContentTypeID == sp.ContentID
							&& (ct.PageActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					   select ct));

		internal static IQueryable<vw_carrot_Content> BlogNavAll(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				DateCompare = DateTime.UtcNow,
				ContentTypeID = ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry),
				ContentType = ContentPageType.PageType.BlogEntry,
				ActiveOnly = bActiveOnly
			};

			return cqBlogNavAll(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, IQueryable<vw_carrot_Content>> cqBlogNavAll =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					  (from ct in ctx.vw_carrot_Contents
					   orderby ct.NavOrder, ct.NavMenuText
					   where ct.SiteID == sp.SiteID
							&& ct.IsLatestVersion == true
							&& ct.ContentTypeID == sp.ContentTypeID
							&& (ct.PageActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					   select ct));

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<carrot_Content>> cqBlogAllContentTbl =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID) =>
					  (from ct in ctx.carrot_RootContents
					   join c in ctx.carrot_Contents on ct.Root_ContentID equals c.Root_ContentID
					   where ct.SiteID == siteID
							&& c.IsLatestVersion == true
							&& ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry)
					   select c));

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<carrot_Content>> cqContentAllContentTbl =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID) =>
					  (from ct in ctx.carrot_RootContents
					   join c in ctx.carrot_Contents on ct.Root_ContentID equals c.Root_ContentID
					   where ct.SiteID == siteID
							&& c.IsLatestVersion == true
							&& ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry)
					   select c));

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<carrot_RootContent>> cqBlogAllRootTbl =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID) =>
					  (from ct in ctx.carrot_RootContents
					   where ct.SiteID == siteID
							&& ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry)
					   select ct));

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<string>> cqBlogDupFileNames =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID) =>
					  (ctx.carrot_RootContents.Where(c => c.SiteID == siteID && c.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry))
						   .GroupBy(g => g.FileName)
						   .Where(g => g.Count() > 1)
						   .Select(g => g.Key)));

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid, string, IQueryable<carrot_RootContent>> cqGetRootContentListByURLTbl =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID, Guid entryType, string sPage) =>
					  (from r in ctx.carrot_RootContents
					   orderby r.GoLiveDate descending
					   where r.SiteID == siteID
							&& r.ContentTypeID == entryType
							&& r.FileName.ToLower() == sPage.ToLower()
					   select r));

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<vw_carrot_Content>> cqGetAllContent =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID) =>
					  (from r in ctx.vw_carrot_Contents
					   where r.SiteID == siteID
							&& r.IsLatestVersion == true
					   select r));

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid, string, IQueryable<vw_carrot_Content>> cqGetRootContentListNoMatchByURL =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID, Guid rootContentID, string sPage) =>
					  (from ct in ctx.vw_carrot_Contents
					   where ct.SiteID == siteID
						   && ct.FileName.ToLower() == sPage.ToLower()
						   && ct.Root_ContentID != rootContentID
					   select ct));

		internal static vw_carrot_Content FindHome(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				DateCompare = DateTime.UtcNow,
				ActiveOnly = bActiveOnly
			};

			return cqFindHome(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, vw_carrot_Content> cqFindHome =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					(from ct in ctx.vw_carrot_Contents
					 orderby ct.NavOrder ascending
					 where ct.SiteID == sp.SiteID
							&& ct.NavOrder < 1
							&& ct.IsLatestVersion == true
							&& (ct.PageActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					 select ct).FirstOrDefault());

		internal static IQueryable<vw_carrot_Content> GetLatestContentList(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				DateCompare = DateTime.UtcNow,
				ContentTypeID = ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry),
				ContentType = ContentPageType.PageType.ContentEntry,
				ActiveOnly = bActiveOnly
			};

			return cqGetLatestContentList(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, IQueryable<vw_carrot_Content>> cqGetLatestContentList =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					  (from ct in ctx.vw_carrot_Contents
					   orderby ct.NavOrder, ct.NavMenuText
					   where ct.SiteID == sp.SiteID
							&& ct.IsLatestVersion == true
							&& ct.ContentTypeID == sp.ContentTypeID
							&& (ct.PageActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					   select ct));

		internal static IQueryable<vw_carrot_Content> GetLatestBlogList(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				DateCompare = DateTime.UtcNow,
				ContentTypeID = ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry),
				ContentType = ContentPageType.PageType.BlogEntry,
				ActiveOnly = bActiveOnly
			};

			return cqGetLatestBlogList(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, IQueryable<vw_carrot_Content>> cqGetLatestBlogList =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					  (from ct in ctx.vw_carrot_Contents
					   orderby ct.NavOrder, ct.NavMenuText
					   where ct.SiteID == sp.SiteID
							&& ct.IsLatestVersion == true
							&& ct.ContentTypeID == sp.ContentTypeID
							&& (ct.PageActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					   select ct));

		//===============================

		internal static readonly Func<CarrotCMSDataContext, Guid, carrot_ContentComment> cqGetContentCommentsTblByID =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid contentCommentID) =>
					  (from r in ctx.carrot_ContentComments
					   orderby r.CreateDate descending
					   where r.ContentCommentID == contentCommentID
					   select r).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, vw_carrot_Comment> cqGetContentCommentByID =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid contentCommentID) =>
					  (from r in ctx.vw_carrot_Comments
					   orderby r.CreateDate descending
					   where r.ContentCommentID == contentCommentID
					   select r).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid, carrot_ContentComment> cqGetContentCommentsTbl =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid rootContentID, Guid contentCommentID) =>
					  (from r in ctx.carrot_ContentComments
					   orderby r.CreateDate descending
					   where r.ContentCommentID == contentCommentID
						   && r.Root_ContentID == rootContentID
					   select r).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<carrot_ContentComment>> cqGetContentCommentsListTbl =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid rootContentID) =>
					  (from r in ctx.carrot_ContentComments
					   orderby r.CreateDate descending
					   where r.Root_ContentID == rootContentID
					   select r));

		//===============================

		internal static readonly Func<CarrotCMSDataContext, Guid, carrot_Widget> cqGetRootWidget =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid rootWidgetID) =>
				(from r in ctx.carrot_Widgets
				 where r.Root_WidgetID == rootWidgetID
				 select r).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, vw_carrot_Widget> cqGetLatestWidget =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid rootWidgetID) =>
				(from r in ctx.vw_carrot_Widgets
				 where r.Root_WidgetID == rootWidgetID
					&& r.IsLatestVersion == true
				 select r).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, vw_carrot_Widget> cqGetWidgetDataByID_VW =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid widgetDataID) =>
				(from r in ctx.vw_carrot_Widgets
				 where r.WidgetDataID == widgetDataID
				 select r).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, carrot_WidgetData> cqGetWidgetDataByID_TBL =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid widgetDataID) =>
				(from r in ctx.carrot_WidgetDatas
				 where r.WidgetDataID == widgetDataID
				 select r).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, carrot_WidgetData> cqGetWidgetDataByRootID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid rootWidgetID) =>
				(from r in ctx.carrot_WidgetDatas
				 where r.Root_WidgetID == rootWidgetID
					&& r.IsLatestVersion == true
				 select r).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<carrot_WidgetData>> cqGetWidgetDataByRootAll =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid rootWidgetID) =>
				(from r in ctx.carrot_WidgetDatas
				 where r.Root_WidgetID == rootWidgetID
				 select r));

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<vw_carrot_Widget>> cqGetWidgetVersionHistory_VW =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid rootWidgetID) =>
					  (from r in ctx.vw_carrot_Widgets
					   orderby r.EditDate descending
					   where r.Root_WidgetID == rootWidgetID
					   select r));

		internal static readonly Func<CarrotCMSDataContext, Guid, bool, IQueryable<vw_carrot_Widget>> cqGetLatestWidgets =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid rootContentID, bool bActiveOnly) =>
					  (from r in ctx.vw_carrot_Widgets
					   orderby r.WidgetOrder
					   where r.Root_ContentID == rootContentID
						  && r.IsLatestVersion == true
						  && (r.WidgetActive == true || bActiveOnly == false)
					   select r));

		internal static readonly Func<CarrotCMSDataContext, SearchParameterObject, carrot_SerialCache> cqGetSerialCacheTbl =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, SearchParameterObject searchParm) =>
			(from c in ctx.carrot_SerialCaches
			 where c.ItemID == searchParm.ItemID
					&& c.KeyType == searchParm.KeyType
					&& c.SiteID == searchParm.SiteID
					&& c.EditUserId == searchParm.UserId
			 select c).FirstOrDefault());

		internal static carrot_SerialCache SearchSeriaCache(CarrotCMSDataContext ctx, Guid siteID, Guid userID, Guid itemID, string keyType) {
			SearchParameterObject searchParm = new SearchParameterObject {
				SiteID = siteID,
				DateCompare = DateTime.UtcNow,
				UserId = userID,
				ItemID = itemID,
				KeyType = keyType
			};

			return cqGetSerialCacheTbl(ctx, searchParm);
		}

		internal static carrot_SerialCache SearchSeriaCache(CarrotCMSDataContext ctx, Guid itemID, string keyType) {
			return SearchSeriaCache(ctx, SiteData.CurrentSiteID, SecurityData.CurrentUserGuid, itemID, keyType);
		}

		//=====================

		internal static readonly Func<CarrotCMSDataContext, Guid, carrot_Site> cqGetSiteByID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID) =>
				(from r in ctx.carrot_Sites
				 where r.SiteID == siteID
				 select r).FirstOrDefault());

		//=====================

		internal static readonly Func<CarrotCMSDataContext, string, carrot_ContentType> cqGetContentTypeByName =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, string contentTypeValue) =>
				(from r in ctx.carrot_ContentTypes
				 where r.ContentTypeValue.ToLower() == contentTypeValue.ToLower()
				 select r).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, carrot_ContentType> cqGetContentTypeByID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid contentTypeID) =>
				(from r in ctx.carrot_ContentTypes
				 where r.ContentTypeID == contentTypeID
				 select r).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, IQueryable<carrot_ContentType>> cqGetContentTypes =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx) =>
				(from r in ctx.carrot_ContentTypes
				 select r));

		//==========================

		internal static readonly Func<CarrotCMSDataContext, Guid, string, vw_carrot_TagURL> cqGetContentTagByURL =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID, string slugURL) =>
				(from c in ctx.vw_carrot_TagURLs
				 where c.SiteID == siteID
					 && c.TagUrl.ToLower() == slugURL.ToLower()
				 select c).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, string, vw_carrot_CategoryURL> cqGetContentCategoryByURL =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID, string slugURL) =>
				(from c in ctx.vw_carrot_CategoryURLs
				 where c.SiteID == siteID
					 && c.CategoryUrl.ToLower() == slugURL.ToLower()
				 select c).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, string, vw_carrot_EditorURL> cqGetContentEditorURL =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID, string slugURL) =>
				(from c in ctx.vw_carrot_EditorURLs
				 where c.SiteID == siteID
					 && c.UserUrl.ToLower() == slugURL.ToLower()
				 select c).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, carrot_ContentTag> cqGetContentTagByID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid contentTagID) =>
				(from c in ctx.carrot_ContentTags
				 where c.ContentTagID == contentTagID
				 select c).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, carrot_ContentCategory> cqGetContentCategoryByID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid contentCategoryID) =>
				(from c in ctx.carrot_ContentCategories
				 where c.ContentCategoryID == contentCategoryID
				 select c).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid, vw_carrot_EditorURL> cqGetContentEditorByID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID, Guid userId) =>
				(from c in ctx.vw_carrot_EditorURLs
				 where c.SiteID == siteID
					 && c.UserId == userId
				 select c).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, int> cqGetContentTagCountBySiteID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID) =>
				(from c in ctx.carrot_ContentTags
				 where c.SiteID == siteID
				 select c).Count());

		internal static readonly Func<CarrotCMSDataContext, Guid, int> cqGetContentCategoryCountBySiteID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID) =>
				(from c in ctx.carrot_ContentCategories
				 where c.SiteID == siteID
				 select c).Count());

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<vw_carrot_TagCounted>> cqGetContentTagBySiteID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID) =>
				(from c in ctx.vw_carrot_TagCounteds
				 where c.SiteID == siteID
				 select c));

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<vw_carrot_CategoryCounted>> cqGetContentCategoryBySiteID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID) =>
				(from c in ctx.vw_carrot_CategoryCounteds
				 where c.SiteID == siteID
				 select c));

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid, string, IQueryable<carrot_ContentTag>> cqGetContentTagNoMatch =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID, Guid contentTagID, string slug) =>
				(from r in ctx.carrot_ContentTags
				 where r.SiteID == siteID
					&& r.ContentTagID != contentTagID
					&& r.TagSlug.ToLower() == slug.ToLower()
				 select r));

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid, string, IQueryable<carrot_ContentCategory>> cqGetContentCategoryNoMatch =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID, Guid contentCategoryID, string slug) =>
				(from r in ctx.carrot_ContentCategories
				 where r.SiteID == siteID
					&& r.ContentCategoryID != contentCategoryID
					&& r.CategorySlug.ToLower() == slug.ToLower()
				 select r));

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<carrot_ContentTag>> cqGetContentTagByContentID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid rootContentID) =>
				(from r in ctx.carrot_ContentTags
				 join c in ctx.carrot_TagContentMappings on r.ContentTagID equals c.ContentTagID
				 where c.Root_ContentID == rootContentID
				 select r));

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<carrot_ContentCategory>> cqGetContentCategoryByContentID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid rootContentID) =>
				(from r in ctx.carrot_ContentCategories
				 join c in ctx.carrot_CategoryContentMappings on r.ContentCategoryID equals c.ContentCategoryID
				 where c.Root_ContentID == rootContentID
				 select r));

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<carrot_TagContentMapping>> cqGetContentTagMapByContentID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid rootContentID) =>
				(from r in ctx.carrot_ContentTags
				 join c in ctx.carrot_TagContentMappings on r.ContentTagID equals c.ContentTagID
				 where c.Root_ContentID == rootContentID
				 select c));

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<carrot_CategoryContentMapping>> cqGetContentCategoryMapByContentID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid rootContentID) =>
				(from r in ctx.carrot_ContentCategories
				 join c in ctx.carrot_CategoryContentMappings on r.ContentCategoryID equals c.ContentCategoryID
				 where c.Root_ContentID == rootContentID
				 select c));

		internal static IQueryable<vw_carrot_Content> GetContentByTagURL(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, string sTagURL) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				DateCompare = DateTime.UtcNow,
				FileName = sTagURL,
				ActiveOnly = bActiveOnly
			};

			return cqGetContentByTagURL(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, IQueryable<vw_carrot_Content>> cqGetContentByTagURL =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
				(from r in ctx.vw_carrot_TagURLs
				 join m in ctx.carrot_TagContentMappings on r.ContentTagID equals m.ContentTagID
				 join ct in ctx.vw_carrot_Contents on m.Root_ContentID equals ct.Root_ContentID
				 where r.SiteID == sp.SiteID
						&& ct.SiteID == sp.SiteID
						&& r.TagUrl.ToLower() == sp.FileName.ToLower()
						&& (ct.PageActive == true || sp.ActiveOnly == false)
						&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
						&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
						&& ct.IsLatestVersion == true
				 select ct));

		internal static IQueryable<vw_carrot_Content> GetContentByCategoryURL(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, string sCatURL) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				DateCompare = DateTime.UtcNow,
				FileName = sCatURL,
				ActiveOnly = bActiveOnly
			};

			return cqGetContentByCategoryURL(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, IQueryable<vw_carrot_Content>> cqGetContentByCategoryURL =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
				(from r in ctx.vw_carrot_CategoryURLs
				 join m in ctx.carrot_CategoryContentMappings on r.ContentCategoryID equals m.ContentCategoryID
				 join ct in ctx.vw_carrot_Contents on m.Root_ContentID equals ct.Root_ContentID
				 where r.SiteID == sp.SiteID
						&& ct.SiteID == sp.SiteID
						&& r.CategoryUrl.ToLower() == sp.FileName.ToLower()
						&& (ct.PageActive == true || sp.ActiveOnly == false)
						&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
						&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
						&& ct.IsLatestVersion == true
				 select ct));

		//=====================

		internal static IQueryable<vw_carrot_Content> GetOtherNotPage(CarrotCMSDataContext ctx, Guid siteID, Guid rootContentID, Guid? parentContentID) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				RootContentID = rootContentID,
				ParentContentID = parentContentID,
				ContentTypeID = ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry),
				DateCompare = DateTime.UtcNow
			};

			return cqGetOtherNotPage(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, IQueryable<vw_carrot_Content>> cqGetOtherNotPage =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					  (from ct in ctx.vw_carrot_Contents
					   orderby ct.NavOrder, ct.NavMenuText
					   where ct.SiteID == sp.SiteID
							&& ct.IsLatestVersion == true
							&& ct.ContentTypeID == sp.ContentTypeID
							&& ct.Root_ContentID != sp.RootContentID
							&& (ct.Parent_ContentID == sp.ParentContentID
								 || (ct.Parent_ContentID == null && sp.ParentContentID == Guid.Empty))
					   select ct));

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid?, IQueryable<vw_carrot_Content>> cqGetLatestContentPages =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID, Guid? rootContentID) =>
					  (from ct in ctx.vw_carrot_Contents
					   orderby ct.NavOrder, ct.NavMenuText
					   where ct.SiteID == siteID
						   && ct.IsLatestVersion == true
						   && ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.ContentEntry)
						   && ct.Root_ContentID == rootContentID
					   select ct));

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid?, IQueryable<vw_carrot_Content>> cqGetLatestBlogPages =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID, Guid? rootContentID) =>
					  (from ct in ctx.vw_carrot_Contents
					   orderby ct.NavOrder, ct.NavMenuText
					   where ct.SiteID == siteID
						   && ct.IsLatestVersion == true
						   && ct.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry)
						   && ct.Root_ContentID == rootContentID
					   select ct));

		internal static vw_carrot_Content GetPreviousPost(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, Guid rootContentID) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				RootContentID = rootContentID,
				DateCompare = DateTime.UtcNow,
				ActiveOnly = bActiveOnly
			};

			return cqGetPreviousPost(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, vw_carrot_Content> cqGetPreviousPost =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					(from ct1 in ctx.vw_carrot_Contents
					 join ct2 in ctx.vw_carrot_Contents on ct1.SiteID equals ct2.SiteID
					 orderby ct1.GoLiveDate descending
					 where ct1.SiteID == sp.SiteID
							&& ct2.GoLiveDate >= ct1.GoLiveDate
							&& ct2.ContentTypeID == ct1.ContentTypeID
							&& ct2.Root_ContentID != ct1.Root_ContentID
							&& ct2.Root_ContentID == sp.RootContentID
							&& ct1.IsLatestVersion == true
							&& ct2.IsLatestVersion == true
							&& (ct1.PageActive == true || sp.ActiveOnly == false)
							&& (ct1.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct1.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					 select ct1).FirstOrDefault());

		internal static vw_carrot_Content GetNextPost(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, Guid rootContentID) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				RootContentID = rootContentID,
				DateCompare = DateTime.UtcNow,
				ActiveOnly = bActiveOnly
			};

			return cqGetNextPost(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, vw_carrot_Content> cqGetNextPost =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					(from ct1 in ctx.vw_carrot_Contents
					 join ct2 in ctx.vw_carrot_Contents on ct1.SiteID equals ct2.SiteID
					 orderby ct1.GoLiveDate ascending
					 where ct1.SiteID == sp.SiteID
							&& ct2.GoLiveDate <= ct1.GoLiveDate
							&& ct2.ContentTypeID == ct1.ContentTypeID
							&& ct2.Root_ContentID != ct1.Root_ContentID
							&& ct2.Root_ContentID == sp.RootContentID
							&& ct1.IsLatestVersion == true
							&& ct2.IsLatestVersion == true
							&& (ct1.PageActive == true || sp.ActiveOnly == false)
							&& (ct1.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct1.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					 select ct1).FirstOrDefault());

		//=========================================

		internal static readonly Func<CarrotCMSDataContext, Guid, carrot_UserData> cqFindUserTblByID =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, Guid UserId) =>
					(from ct in ctx.carrot_UserDatas
					 where ct.UserId == UserId
					 select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, vw_carrot_UserData> cqFindUserByID =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, Guid UserId) =>
					(from ct in ctx.vw_carrot_UserDatas
					 where ct.UserId == UserId
					 select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, string, vw_carrot_UserData> cqFindUserByName =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, string userName) =>
					(from ct in ctx.vw_carrot_UserDatas
					 where ct.UserName.ToLower() == userName.ToLower()
					 select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, string, vw_carrot_UserData> cqFindUserByEmail =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, string emailAddy) =>
					(from ct in ctx.vw_carrot_UserDatas
					 where ct.Email.ToLower() == emailAddy.ToLower()
					 select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, IQueryable<vw_carrot_UserData>> cqGetUserList =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx) =>
					(from ct in ctx.vw_carrot_UserDatas
					 select ct));

		//======================

		internal static readonly Func<CarrotCMSDataContext, Guid, carrot_TrackbackQueue> cqGetTrackbackTblByID =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, Guid trackbackQueueID) =>
					(from ct in ctx.carrot_TrackbackQueues
					 orderby ct.CreateDate descending
					 where ct.TrackbackQueueID == trackbackQueueID
					 select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, vw_carrot_TrackbackQueue> cqGetTrackbackByID =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, Guid trackbackQueueID) =>
					(from ct in ctx.vw_carrot_TrackbackQueues
					 orderby ct.CreateDate descending
					 where ct.TrackbackQueueID == trackbackQueueID
					 select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<vw_carrot_TrackbackQueue>> cqGetTrackbackByRootID =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, Guid rootContentID) =>
					(from ct in ctx.vw_carrot_TrackbackQueues
					 orderby ct.CreateDate descending
					 where ct.Root_ContentID == rootContentID
					 select ct));

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<vw_carrot_TrackbackQueue>> cqGetTrackbackByRootIDUnTracked =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, Guid rootContentID) =>
					(from ct in ctx.vw_carrot_TrackbackQueues
					 orderby ct.CreateDate descending
					 where ct.Root_ContentID == rootContentID
						&& ct.TrackedBack == false
					 select ct));

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<vw_carrot_TrackbackQueue>> cqGetTrackbackBySiteIDUnTracked =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, Guid siteID) =>
					(from ct in ctx.vw_carrot_TrackbackQueues
					 orderby ct.CreateDate descending
					 where ct.SiteID == siteID
						&& ct.TrackedBack == false
						&& ct.PageActive == true
					 select ct).Take(20));

		internal static readonly Func<CarrotCMSDataContext, Guid, string, IQueryable<vw_carrot_TrackbackQueue>> cqGetTrackbackByRootIDAndUrl =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, Guid rootContentID, string sURL) =>
					(from ct in ctx.vw_carrot_TrackbackQueues
					 orderby ct.CreateDate descending
					 where ct.Root_ContentID == rootContentID
						&& ct.TrackBackURL.ToLower() == sURL.ToLower()
					 select ct));

		//===============

		internal static readonly Func<CarrotCMSDataContext, Guid, carrot_TextWidget> cqTextWidgetByID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid textWidgetID) =>
				(from w in ctx.carrot_TextWidgets
				 where w.TextWidgetID == textWidgetID
				 select w).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<carrot_TextWidget>> cqTextWidgetBySiteID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID) =>
				(from w in ctx.carrot_TextWidgets
				 where w.SiteID == siteID
				 select w));

		//==============

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid, carrot_RootContentSnippet> cqGetSnippetDataTbl =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID, Guid rootSnippetID) =>
			  (from r in ctx.carrot_RootContentSnippets
			   where r.SiteID == siteID
				   && r.Root_ContentSnippetID == rootSnippetID
			   select r).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid, carrot_ContentSnippet> cqGetLatestSnippetContentTbl =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, Guid siteID, Guid rootSnippetID) =>
					  (from ct in ctx.carrot_ContentSnippets
					   join r in ctx.carrot_RootContentSnippets on ct.Root_ContentSnippetID equals r.Root_ContentSnippetID
					   where r.SiteID == siteID
						   && r.Root_ContentSnippetID == rootSnippetID
						   && ct.IsLatestVersion == true
					   select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<vw_carrot_ContentSnippet>> cqGetSnippetVersionHistory =
		CompiledQuery.Compile(
				(CarrotCMSDataContext ctx, Guid rootSnippetID) =>
					(from ct in ctx.vw_carrot_ContentSnippets
					 orderby ct.EditDate descending
					 where ct.Root_ContentSnippetID == rootSnippetID
					 select ct));

		internal static readonly Func<CarrotCMSDataContext, Guid, IQueryable<vw_carrot_ContentSnippet>> cqGetSnippetsBySiteID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID) =>
				(from ct in ctx.vw_carrot_ContentSnippets
				 orderby ct.EditDate descending
				 where ct.SiteID == siteID
					&& ct.IsLatestVersion == true
				 select ct));

		internal static readonly Func<CarrotCMSDataContext, Guid, vw_carrot_ContentSnippet> cqGetLatestSnippetVersion =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid rootSnippetID) =>
				(from ct in ctx.vw_carrot_ContentSnippets
				 orderby ct.EditDate descending
				 where ct.Root_ContentSnippetID == rootSnippetID
					&& ct.IsLatestVersion == true
				 select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, vw_carrot_ContentSnippet> cqGetSnippetVersionByID =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid snippetDataID) =>
				(from ct in ctx.vw_carrot_ContentSnippets
				 where ct.ContentSnippetID == snippetDataID
				 select ct).FirstOrDefault());

		internal static readonly Func<CarrotCMSDataContext, Guid, Guid, string, IQueryable<vw_carrot_ContentSnippet>> cqGetContentSnippetNoMatch =
		CompiledQuery.Compile(
			(CarrotCMSDataContext ctx, Guid siteID, Guid rootSnippetID, string slug) =>
				(from r in ctx.vw_carrot_ContentSnippets
				 where r.SiteID == siteID
					&& r.Root_ContentSnippetID != rootSnippetID
					&& r.ContentSnippetSlug.ToLower() == slug.ToLower()
				 select r));

		internal static vw_carrot_ContentSnippet GetLatestContentSnippetBySlug(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, string sItemSlug) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				DateCompare = DateTime.UtcNow,
				ActiveOnly = bActiveOnly,
				ItemSlug = sItemSlug
			};
			return cqGetLatestContentSnippetBySlug(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, vw_carrot_ContentSnippet> cqGetLatestContentSnippetBySlug =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					  (from ct in ctx.vw_carrot_ContentSnippets
					   where ct.SiteID == sp.SiteID
							&& ct.ContentSnippetSlug.ToLower() == sp.ItemSlug.ToLower()
							&& ct.IsLatestVersion == true
							&& (ct.ContentSnippetActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					   select ct).FirstOrDefault());

		internal static vw_carrot_ContentSnippet GetLatestContentSnippetByID(CarrotCMSDataContext ctx, Guid siteID, bool bActiveOnly, Guid itemID) {
			SearchParameterObject sp = new SearchParameterObject {
				SiteID = siteID,
				DateCompare = DateTime.UtcNow,
				ActiveOnly = bActiveOnly,
				ItemSlugID = itemID
			};
			return cqGetLatestContentSnippetByID(ctx, sp);
		}

		private static readonly Func<CarrotCMSDataContext, SearchParameterObject, vw_carrot_ContentSnippet> cqGetLatestContentSnippetByID =
		CompiledQuery.Compile(
					(CarrotCMSDataContext ctx, SearchParameterObject sp) =>
					  (from ct in ctx.vw_carrot_ContentSnippets
					   where ct.SiteID == sp.SiteID
							&& ct.Root_ContentSnippetID == sp.ItemSlugID
							&& ct.IsLatestVersion == true
							&& (ct.ContentSnippetActive == true || sp.ActiveOnly == false)
							&& (ct.GoLiveDate < sp.DateCompare || sp.ActiveOnly == false)
							&& (ct.RetireDate > sp.DateCompare || sp.ActiveOnly == false)
					   select ct).FirstOrDefault());
	}
}