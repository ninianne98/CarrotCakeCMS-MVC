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
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public class ContentLocalTime {
		public DateTime GoLiveDate { get; set; }

		public DateTime GoLiveDateLocal { get; set; }
	}

	//===============================
	public class BlogPostPageUrl {
		public string PostPrefix { get; set; }

		public DateTime GoLiveDate { get; set; }

		public DateTime GoLiveDateLocal { get; set; }
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
			SiteData site = SiteData.GetSiteFromCache(siteID);

			this.ContentLocalDates = new List<ContentLocalTime>();
			this.BlogPostUrls = new List<BlogPostPageUrl>();

			var allContentDates = new List<DateTime>();
			var blogDateList = new List<DateTime>();

			using (var db = CarrotCMSDataContext.Create()) {
				allContentDates = CannedQueries.GetAllDates(db, siteID).Distinct().ToList();
				blogDateList = CannedQueries.GetAllDatesByType(db, siteID, ContentPageType.PageType.BlogEntry).Distinct().ToList();
			}

			this.ContentLocalDates = (from d in allContentDates
									  select new ContentLocalTime() {
										  GoLiveDate = d,
										  GoLiveDateLocal = site.ConvertUTCToSiteTime(d)
									  }).ToList();

			this.BlogPostUrls = (from bd in blogDateList
								 join ld in this.ContentLocalDates on bd equals ld.GoLiveDate
								 select new BlogPostPageUrl() {
									 GoLiveDate = ld.GoLiveDate,
									 PostPrefix = ContentPageHelper.CreateFileNameFromSlug(siteID, ld.GoLiveDateLocal, string.Empty),
									 GoLiveDateLocal = ld.GoLiveDateLocal
								 }).ToList();
		}

		public void Save() {
			using (var db = CarrotCMSDataContext.Create()) {
				string xml = this.GetXml();

				xml = xml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", string.Empty);

				db.carrot_UpdateGoLiveLocal(this.SiteID, XElement.Parse(xml));
			}
		}

		public string GetXml() {
			var xmlSerializer = new XmlSerializer(typeof(TimeZoneContent));
			string xml = string.Empty;
			using (var sw = new StringWriter()) {
				xmlSerializer.Serialize(sw, this);
				xml = sw.ToString();
			}
			return xml;
		}
	}
}