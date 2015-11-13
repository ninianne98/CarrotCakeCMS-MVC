using Carrotware.CMS.Security.Models;
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
		public static string keyPageImport = "cmsContentPageExport";

		public static void AssignContentPageExportNewIDs(ContentPageExport cpe) {
			cpe.NewRootContentID = Guid.NewGuid();

			cpe.ThePage.Root_ContentID = cpe.NewRootContentID;
			cpe.ThePage.ContentID = cpe.NewRootContentID;
			cpe.ThePage.SiteID = SiteData.CurrentSiteID;
			cpe.ThePage.EditUserId = SecurityData.CurrentUserGuid;
			cpe.ThePage.EditDate = DateTime.UtcNow;

			foreach (var w in cpe.ThePageWidgets) {
				w.Root_ContentID = cpe.NewRootContentID;
				w.Root_WidgetID = Guid.NewGuid();
				w.WidgetDataID = Guid.NewGuid();
			}
		}

		public static void AssignSiteExportNewIDs(SiteExport se) {
			se.NewSiteID = Guid.NewGuid();

			se.TheSite.SiteID = se.NewSiteID;

			foreach (var p in se.ThePages) {
				AssignContentPageExportNewIDs(p);
			}
		}

		public static void AssignWPExportNewIDs(SiteData sd, WordPressSite wps) {
			wps.NewSiteID = Guid.NewGuid();

			wps.Content.Where(p => p.PostType == WordPressPost.WPPostType.BlogPost).ToList()
				.ForEach(q => q.ImportFileName = ContentPageHelper.ScrubPath("/" + sd.ConvertUTCToSiteTime(q.PostDateUTC).ToString(sd.Blog_DatePattern) + "/" + q.ImportFileSlug));

			wps.Content.ToList().ForEach(r => r.ImportFileName = ContentPageHelper.ScrubPath(r.ImportFileName.Replace("//", "/")));
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

				cont.CreateUserId = SecurityData.CurrentUserGuid;
				cont.EditUserId = SecurityData.CurrentUserGuid;

				if (!String.IsNullOrEmpty(c.PostAuthor)) {
					WordPressUser wpu = wps.Authors.Where(x => x.Login.ToLower() == c.PostAuthor.ToLower()).FirstOrDefault();

					if (wpu != null && wpu.ImportUserID != Guid.Empty) {
						ApplicationUser usr = SecurityData.GetUserByID(wpu.ImportUserID.ToString());
						if (usr != null) {
							cont.CreateUserId = wpu.ImportUserID;
							cont.EditUserId = wpu.ImportUserID;
						}
					}
				}

				cont.Root_ContentID = c.ImportRootID;
				cont.FileName = c.ImportFileName.Replace("//", "/");
				cont.PageSlug = null;
				cont.NavOrder = c.PostOrder;
				cont.Parent_ContentID = null;

				cont.CreateDate = site.ConvertUTCToSiteTime(c.PostDateUTC);
				cont.PageActive = c.IsPublished;
				cont.ContentType = ContentPageType.PageType.Unknown;

				if (c.PostType == WordPressPost.WPPostType.BlogPost) {
					cont.ContentType = ContentPageType.PageType.BlogEntry;
					cont.PageSlug = c.ImportFileSlug.Replace("//", "/");
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
				cont.LeftPageText = String.Empty;
				cont.RightPageText = String.Empty;

				cont.MetaDescription = String.Empty;
				cont.MetaKeyword = String.Empty;

				cont.ContentCategories = new List<ContentCategory>();
				cont.ContentTags = new List<ContentTag>();

				List<ContentTag> lstTags = site.GetTagList();
				List<ContentCategory> lstCategories = site.GetCategoryList();

				cont.ContentCategories = (from l in lstCategories
										  join o in c.Categories on l.CategorySlug.ToLower() equals o.ToLower()
										  select l).Distinct().ToList();

				cont.ContentTags = (from l in lstTags
									join o in c.Tags on l.TagSlug.ToLower() equals o.ToLower()
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
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			string sXML = String.Empty;
			using (StringWriter stringWriter = new StringWriter()) {
				xmlSerializer.Serialize(stringWriter, cpe);
				sXML = stringWriter.ToString();
			}

			return sXML;
		}

		public static string GetContentPageExportXML(Guid siteID, Guid rootContentID) {
			ContentPageExport exp = GetExportPage(siteID, rootContentID);

			return GetExportXML<ContentPageExport>(exp);
		}

		public static string GetContentPageExportXML(Guid siteID) {
			SiteExport exp = GetExportSite(siteID);

			return GetExportXML<SiteExport>(exp);
		}

		public static void RemoveSerializedExportData(Guid rootContentID) {
			CMSConfigHelper.ClearSerialized(rootContentID, keyPageImport);
		}

		public static string GetSerialized(Guid itemID) {
			return CMSConfigHelper.GetSerialized(itemID, keyPageImport);
		}

		public static ContentPageExport GetSerializedContentPageExport(Guid rootContentID) {
			ContentPageExport c = null;
			try {
				string sXML = GetSerialized(rootContentID);
				c = GetSerialData<ContentPageExport>(sXML) as ContentPageExport;
			} catch (Exception ex) { }
			return c;
		}

		public static SiteExport GetSerializedSiteExport(Guid siteID) {
			SiteExport c = null;
			try {
				string sXML = GetSerialized(siteID);
				c = GetSerialData<SiteExport>(sXML) as SiteExport;
			} catch (Exception ex) { }
			return c;
		}

		public static WordPressSite GetSerializedWPExport(Guid siteID) {
			WordPressSite c = null;
			try {
				string sXML = GetSerialized(siteID);
				c = GetSerialData<WordPressSite>(sXML) as WordPressSite;
			} catch (Exception ex) { }
			return c;
		}

		public static ContentPageExport DeserializeContentPageExport(string sXML) {
			ContentPageExport c = GetSerialData<ContentPageExport>(sXML) as ContentPageExport;
			return c;
		}

		public static SiteExport DeserializeSiteExport(string sXML) {
			SiteExport c = GetSerialData<SiteExport>(sXML) as SiteExport;
			return c;
		}

		public static WordPressSite DeserializeWPExport(string sXML) {
			WPBlogReader wbp = new WPBlogReader();
			XmlDocument doc = wbp.LoadText(sXML);
			//WordPressSite site = wbp.GetContent(doc);
			WordPressSite site = wbp.GetAllData(doc);
			return site;
		}

		public static WordPressSite DeserializeWPExportAll(string sXML) {
			WPBlogReader wbp = new WPBlogReader();
			XmlDocument doc = wbp.LoadText(sXML);
			WordPressSite site = wbp.GetAllData(doc);
			return site;
		}

		public static void SaveSerializedDataExport<T>(Guid guidKey, T theData) {
			if (theData == null) {
				CMSConfigHelper.ClearSerialized(guidKey, keyPageImport);
			} else {
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
				string sXML = String.Empty;
				using (StringWriter stringWriter = new StringWriter()) {
					xmlSerializer.Serialize(stringWriter, theData);
					sXML = stringWriter.ToString();
				}
				CMSConfigHelper.SaveSerialized(guidKey, keyPageImport, sXML);
			}
		}

		public static Object GetSerialData<T>(string sXML) {
			Object obj = null;
			try {
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

				using (StringReader stringReader = new StringReader(sXML)) {
					obj = xmlSerializer.Deserialize(stringReader);
				}
			} catch (Exception ex) { }
			return obj;
		}

		public static object switch_on { get; set; }
	}
}