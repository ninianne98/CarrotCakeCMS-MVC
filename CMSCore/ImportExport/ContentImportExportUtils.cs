using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

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

	public class ContentImportExportUtils {
		private static string _keyPageImport = "cmsContentPageExport";

		public static string ImportQueryKey {
			get { return "importid"; }
		}

		public static string ImportQuery(string uri, Guid id) {
			return string.Format("{0}?{1}={2}", uri, ImportQueryKey, id.ToString());
		}

		public static void AssignContentPageExportNewIDs(ContentPageExport cpe) {
			cpe.NewRootContentID = Guid.NewGuid();

			cpe.ThePage.Root_ContentID = cpe.NewRootContentID;
			cpe.ThePage.ContentID = Guid.NewGuid();
			cpe.ThePage.SiteID = SiteData.CurrentSiteID;
			cpe.ThePage.EditUserId = SecurityData.CurrentUserGuid;
			cpe.ThePage.EditDate = DateTime.UtcNow;

			cpe.ThePage.FileName = ContentPageHelper.ScrubFilename(cpe.NewRootContentID, cpe.ThePage.FileName);
			if (!string.IsNullOrEmpty(cpe.ParentFileName)) {
				cpe.ParentFileName = ContentPageHelper.ScrubFilename(cpe.OriginalParentContentID, cpe.ParentFileName);
			}

			foreach (var w in cpe.ThePageWidgets) {
				w.Root_ContentID = cpe.NewRootContentID;
				w.Root_WidgetID = Guid.NewGuid();
				w.WidgetDataID = Guid.NewGuid();
			}
		}

		public static void MapSiteCategoryTags(ContentPageExport cpe) {
			if (cpe.ThePage.ContentType == ContentPageType.PageType.BlogEntry) {
				var lstCategories = SiteData.CurrentSite.GetCategoryList();
				var lstTags = SiteData.CurrentSite.GetTagList();

				cpe.ThePage.ContentCategories = (from l in lstCategories
												 join o in cpe.ThePage.ContentCategories on l.CategorySlug.ToLowerInvariant() equals o.CategorySlug.ToLowerInvariant()
												 where !string.IsNullOrEmpty(o.CategorySlug)
												 select l).Distinct().ToList();

				cpe.ThePage.ContentTags = (from l in lstTags
										   join o in cpe.ThePage.ContentTags on l.TagSlug.ToLowerInvariant() equals o.TagSlug.ToLowerInvariant()
										   where !string.IsNullOrEmpty(o.TagSlug)
										   select l).Distinct().ToList();
			} else {
				cpe.ThePage.ContentCategories = new List<ContentCategory>();
				cpe.ThePage.ContentTags = new List<ContentTag>();
			}
		}

		public static ContentPageExport MapTemplate(ContentPageExport cpe) {
			var filePath = cpe.ThePage.TemplateFile ?? string.Empty;

			if (filePath.Length > 0) {
				cpe.ThePage.TemplateFile = MapTemplate(cpe.ThePage.TemplateFile);
			}

			return cpe;
		}

		public static string MapTemplate(string templateName) {
			var filePath = templateName ?? string.Empty;

			if (filePath.Length > 0) {
				filePath = filePath.ToLowerInvariant().FixPathSlashes();
				var fileName = Path.GetFileNameWithoutExtension(filePath);
				var rootFolder = new DirectoryInfo(Path.GetDirectoryName(filePath)).Name;
				var tplFilePath = (Path.Combine(rootFolder, fileName) + ".cshtml").FixPathSlashes().ToLowerInvariant();

				using (var cmsHelper = new CMSConfigHelper()) {
					var tp = cmsHelper.Templates.Where(x => x.TemplatePath.ToLowerInvariant() == filePath
										|| x.TemplatePath.ToLowerInvariant().EndsWith(tplFilePath)).FirstOrDefault();

					if (tp != null) {
						return tp.TemplatePath;
					}
				}
			}

			return SiteData.DefaultTemplateFilename;
		}

		public static void AssignSiteExportNewIDs(SiteExport se) {
			se.NewSiteID = Guid.NewGuid();

			se.TheSite.SiteID = se.NewSiteID;

			foreach (var p in se.ThePages) {
				AssignContentPageExportNewIDs(p);

				if (p.ThePage.ContentType == ContentPageType.PageType.BlogEntry) {
					p.ThePage.ContentCategories.ToList().ForEach(r => r.CategorySlug = se.TheCategories.Where(x => x.ContentCategoryID == r.ContentCategoryID).FirstOrDefault().CategorySlug);
					p.ThePage.ContentTags.ToList().ForEach(r => r.TagSlug = se.TheTags.Where(x => x.ContentTagID == r.ContentTagID).FirstOrDefault().TagSlug);
				}
			}

			se.ThePages.Where(p => p.ThePage.ContentType == ContentPageType.PageType.BlogEntry).ToList()
				.ForEach(r => r.ThePage.PageSlug = ContentPageHelper.ScrubFilename(r.ThePage.Root_ContentID, r.ThePage.PageSlug));

			se.ThePages.Where(p => p.ThePage.ContentType == ContentPageType.PageType.BlogEntry).ToList()
				.ForEach(q => q.ThePage.FileName = ContentPageHelper.ScrubFilename(q.ThePage.Root_ContentID, ContentPageHelper.CreateFileNameFromSlug(se.TheSite, q.ThePage.GoLiveDate, q.ThePage.PageSlug)));

			se.ThePages.ToList().ForEach(r => r.ThePage.FileName = ContentPageHelper.ScrubFilename(r.ThePage.Root_ContentID, r.ThePage.FileName));
		}

		public static void AssignWPExportNewIDs(SiteData site, WordPressSite wps) {
			wps.NewSiteID = Guid.NewGuid();

			wps.Content.Where(p => p.PostType == WordPressPost.WPPostType.BlogPost).ToList()
				.ForEach(q => q.ImportFileName = ContentPageHelper.ScrubFilename(q.ImportRootID, ContentPageHelper.CreateFileNameFromSlug(site, site.ConvertUTCToSiteTime(q.PostDateUTC), q.ImportFileSlug)));

			wps.Content.ToList().ForEach(r => r.ImportFileName = ContentPageHelper.ScrubFilename(r.ImportRootID, r.ImportFileName));
		}

		public static ContentPage CreateWPContentPage(WordPressSite wps, WordPressPost c, SiteData site) {
			ContentPage cont = null;

			ContentPageType.PageType contType = ContentPageType.PageType.Unknown;
			switch (c.PostType) {
				case WordPressPost.WPPostType.BlogPost:
					contType = ContentPageType.PageType.BlogEntry;
					break;

				case WordPressPost.WPPostType.Page:
					contType = ContentPageType.PageType.ContentEntry;
					break;
			}

			if (c != null) {
				cont = new ContentPage(site.SiteID, contType);
				cont.ContentID = Guid.NewGuid();

				cont.EditUserId = wps.FindImportUser(c.PostAuthor);
				cont.CreateUserId = wps.FindImportUser(c.PostAuthor);

				cont.Root_ContentID = c.ImportRootID;
				cont.FileName = ContentPageHelper.ScrubFilename(c.ImportRootID, c.ImportFileName);
				cont.PageSlug = null;
				cont.NavOrder = c.PostOrder;
				cont.Parent_ContentID = null;

				cont.CreateDate = site.ConvertUTCToSiteTime(c.PostDateUTC);
				cont.PageActive = c.IsPublished;
				cont.ContentType = ContentPageType.PageType.Unknown;

				if (c.PostType == WordPressPost.WPPostType.BlogPost) {
					cont.ContentType = ContentPageType.PageType.BlogEntry;
					cont.PageSlug = ContentPageHelper.ScrubFilename(c.ImportRootID, c.ImportFileSlug);
					cont.NavOrder = SiteData.BlogSortOrderNumber;
					cont.Parent_ContentID = null;
				}
				if (c.PostType == WordPressPost.WPPostType.Page) {
					cont.ContentType = ContentPageType.PageType.ContentEntry;
				}

				if (cont.ContentType == ContentPageType.PageType.ContentEntry) {
					cont.ShowInSiteMap = true;
					cont.ShowInSiteNav = true;
				} else {
					cont.ShowInSiteMap = false;
					cont.ShowInSiteNav = false;
				}

				cont.IsLatestVersion = true;

				cont.IsLatestVersion = true;
				cont.TitleBar = c.PostTitle;
				cont.NavMenuText = c.PostTitle;
				cont.PageHead = c.PostTitle;
				cont.PageText = c.PostContent;
				cont.LeftPageText = string.Empty;
				cont.RightPageText = string.Empty;

				cont.MetaDescription = string.Empty;
				cont.MetaKeyword = string.Empty;

				cont.ContentCategories = new List<ContentCategory>();
				cont.ContentTags = new List<ContentTag>();

				List<ContentTag> lstTags = site.GetTagList();
				List<ContentCategory> lstCategories = site.GetCategoryList();

				cont.ContentCategories = (from l in lstCategories
										  join o in c.Categories on l.CategorySlug.ToLowerInvariant() equals o.ToLowerInvariant()
										  select l).Distinct().ToList();

				cont.ContentTags = (from l in lstTags
									join o in c.Tags on l.TagSlug.ToLowerInvariant() equals o.ToLowerInvariant()
									select l).Distinct().ToList();
			}

			return cont;
		}

		public static SiteExport GetExportSite(Guid siteID) {
			SiteExport site = new SiteExport(siteID);

			return site;
		}

		public static SiteExport GetExportSite(Guid siteID, SiteExport.ExportType exportWhat) {
			SiteExport site = new SiteExport(siteID, exportWhat);

			return site;
		}

		public static List<ContentPageExport> ExportAllSiteContent(Guid siteID) {
			List<ContentPageExport> lst = new List<ContentPageExport>();

			List<ContentPageExport> lst1 = ExportPages(siteID);
			List<ContentPageExport> lst2 = ExportPosts(siteID);
			lst = lst1.Union(lst2).ToList();

			return lst;
		}

		public static List<ContentPageExport> ExportPages(Guid siteID) {
			List<ContentPageExport> lst = null;
			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				lst = (from c in pageHelper.GetAllLatestContentList(siteID)
					   select new ContentPageExport(c, c.GetWidgetList())).ToList();
			}

			return lst;
		}

		public static List<ContentPageExport> ExportPosts(Guid siteID) {
			List<ContentPageExport> lst = null;
			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				lst = (from c in pageHelper.GetAllLatestBlogList(siteID)
					   select new ContentPageExport(c, c.GetWidgetList())).ToList();
			}

			return lst;
		}

		public static ContentPageExport GetExportPage(Guid siteID, Guid rootContentID) {
			ContentPageExport cpe = new ContentPageExport(siteID, rootContentID);

			return cpe;
		}

		public static string GetExportXML<T>(T cpe) {
			var xmlSerializer = new XmlSerializer(typeof(T));
			string xml = string.Empty;
			using (var writer = new StringWriter()) {
				xmlSerializer.Serialize(writer, cpe);
				xml = writer.ToString();
			}

			return xml;
		}

		public static string GetContentPageExportXML(Guid siteID, Guid rootContentID) {
			ContentPageExport exp = GetExportPage(siteID, rootContentID);

			return GetExportXML(exp);
		}

		public static string GetContentPageExportXML(Guid siteID) {
			SiteExport exp = GetExportSite(siteID);

			return GetExportXML(exp);
		}

		public static void RemoveSerializedExportData(Guid rootContentID) {
			CMSConfigHelper.ClearSerialized(rootContentID, _keyPageImport);
		}

		public static string GetSerialized(Guid itemID) {
			return CMSConfigHelper.GetSerialized(itemID, _keyPageImport);
		}

		public static ContentPageExport GetSerializedContentPageExport(Guid rootContentID) {
			ContentPageExport c = null;
			try {
				string xml = GetSerialized(rootContentID);
				c = GetSerialData<ContentPageExport>(xml) as ContentPageExport;
			} catch (Exception ex) { }
			return c;
		}

		public static SiteExport GetSerializedSiteExport(Guid siteID) {
			SiteExport c = null;
			try {
				string xml = GetSerialized(siteID);
				c = GetSerialData<SiteExport>(xml) as SiteExport;
			} catch (Exception ex) { }
			return c;
		}

		public static WordPressSite GetSerializedWPExport(Guid siteID) {
			WordPressSite c = null;
			try {
				string xml = GetSerialized(siteID);
				c = GetSerialData<WordPressSite>(xml) as WordPressSite;
			} catch (Exception ex) { }
			return c;
		}

		public static ContentPageExport DeserializeContentPageExport(string xml) {
			ContentPageExport c = GetSerialData<ContentPageExport>(xml) as ContentPageExport;
			return c;
		}

		public static SiteExport DeserializeSiteExport(string xml) {
			SiteExport c = GetSerialData<SiteExport>(xml) as SiteExport;
			return c;
		}

		public static WordPressSite DeserializeWPExport(string xml) {
			WPBlogReader wbp = new WPBlogReader();
			XmlDocument doc = wbp.LoadText(xml);
			//WordPressSite site = wbp.GetContent(doc);
			WordPressSite site = wbp.GetAllData(doc);
			return site;
		}

		public static WordPressSite DeserializeWPExportAll(string xml) {
			WPBlogReader wbp = new WPBlogReader();
			XmlDocument doc = wbp.LoadText(xml);
			WordPressSite site = wbp.GetAllData(doc);
			return site;
		}

		public static void SaveSerializedDataExport<T>(Guid guidKey, T theData) {
			if (theData == null) {
				CMSConfigHelper.ClearSerialized(guidKey, _keyPageImport);
			} else {
				var xmlSerializer = new XmlSerializer(typeof(T));
				string xml = string.Empty;
				using (var writer = new StringWriter()) {
					xmlSerializer.Serialize(writer, theData);
					xml = writer.ToString();
				}
				CMSConfigHelper.SaveSerialized(guidKey, _keyPageImport, xml);
			}
		}

		public static object GetSerialData<T>(string xml) {
			object obj = null;
			try {
				var xmlSerializer = new XmlSerializer(typeof(T));

				using (StringReader stringReader = new StringReader(xml)) {
					obj = xmlSerializer.Deserialize(stringReader);
				}
			} catch (Exception ex) { }
			return obj;
		}

		public static object switch_on { get; set; }
	}
}