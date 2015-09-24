using Carrotware.CMS.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

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

	public class ContentLocalTime {
		public Guid Root_ContentID { get; set; }

		public Guid ContentTypeID { get; set; }

		public string FileName { get; set; }

		public string PageSlug { get; set; }

		public DateTime GoLiveDate { get; set; }

		public DateTime GoLiveDateLocal { get; set; }
	}

	//===============================
	public class BlogPostPageUrl {
		public Guid Root_ContentID { get; set; }

		public DateTime GoLiveDateLocal { get; set; }

		public string FileName { get; set; }
	}

	//===============================
	public class TimeZoneContent {
		public List<ContentLocalTime> ContentLocalDates { get; set; }

		public List<BlogPostPageUrl> BlogPostUrls { get; set; }

		public Guid SiteID { get; set; }

		public TimeZoneContent() {
			this.ContentLocalDates = new List<ContentLocalTime>();
			this.BlogPostUrls = new List<BlogPostPageUrl>();
		}

		public TimeZoneContent(Guid siteID) {
			// use C# libraries for timezones rather than pass in offset as some dates are +/- an hour off because of DST

			this.SiteID = siteID;

			this.ContentLocalDates = new List<ContentLocalTime>();

			this.BlogPostUrls = new List<BlogPostPageUrl>();

			SiteData site = SiteData.GetSiteFromCache(siteID);

			List<carrot_RootContent> queryAllContent = null;

			using (CarrotCMSDataContext db = CarrotCMSDataContext.Create()) {
				queryAllContent = CannedQueries.GetAllRootTbl(db, siteID).ToList();
			}

			this.ContentLocalDates = (from p in queryAllContent
									  select new ContentLocalTime {
										  Root_ContentID = p.Root_ContentID,
										  ContentTypeID = p.ContentTypeID,
										  GoLiveDate = p.GoLiveDate,
										  PageSlug = p.PageSlug,
										  FileName = p.FileName,
										  GoLiveDateLocal = site.ConvertUTCToSiteTime(p.GoLiveDate)
									  }).ToList();

			IEnumerable<ContentLocalTime> queryBlog = (from c in this.ContentLocalDates
													   where c.ContentTypeID == ContentPageType.GetIDByType(ContentPageType.PageType.BlogEntry)
													   select c);

			this.BlogPostUrls = (from p in queryBlog
								 select new BlogPostPageUrl {
									 Root_ContentID = p.Root_ContentID,
									 GoLiveDateLocal = p.GoLiveDateLocal,
									 FileName = ContentPageHelper.CreateFileNameFromSlug(siteID, p.GoLiveDateLocal, p.PageSlug)
								 }).ToList();
		}

		public void Save() {
			using (CarrotCMSDataContext db = CarrotCMSDataContext.Create()) {
				string xml = this.GetXml();

				xml = xml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");

				db.carrot_UpdateGoLiveLocal(this.SiteID, XElement.Parse(xml));
			}
		}

		public string GetXml() {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(TimeZoneContent));
			string sXML = "";
			using (StringWriter stringWriter = new StringWriter()) {
				xmlSerializer.Serialize(stringWriter, this);
				sXML = stringWriter.ToString();
			}
			return sXML;
		}
	}
}