using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

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

	public class SiteMapHelper {

		public SiteMapHelper() { }

		public void RenderSiteMap(HttpContext context) {
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

			SiteData site = SiteData.CurrentSite;
			List<SiteNav> lstNav = new List<SiteNav>();

			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				//lstNav = navHelper.GetTwoLevelNavigation(SiteData.CurrentSiteID, true);
				lstNav = navHelper.GetLevelDepthNavigation(SiteData.CurrentSiteID, 4, true);
			}
			lstNav.RemoveAll(x => x.ShowInSiteMap == false);

			DateTime dtMax = lstNav.Min(x => x.EditDate);
			string DateFormat = "yyyy-MM-dd";

			response.Buffer = false;
			response.Clear();
			response.ContentType = "application/xml";

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.Encoding = Encoding.UTF8;
			settings.CheckCharacters = true;

			XmlWriter writer = XmlWriter.Create(response.Output, settings);

			//writer.WriteStartDocument();
			writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
			writer.WriteRaw("\n");
			writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
			writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
			writer.WriteAttributeString("xsi", "schemaLocation", null, "http://www.sitemaps.org/schemas/sitemap/0.9    http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

			writer.WriteRaw("\n");
			writer.WriteStartElement("url");
			writer.WriteElementString("loc", site.MainURL);
			writer.WriteElementString("lastmod", dtMax.ToString(DateFormat));
			writer.WriteElementString("priority", "1.0");
			writer.WriteEndElement();
			writer.WriteRaw("\n");

			// always, hourly, daily, weekly, monthly, yearly, never

			foreach (SiteNav n in lstNav) {
				writer.WriteStartElement("url");
				writer.WriteElementString("loc", site.ConstructedCanonicalURL(n));
				writer.WriteElementString("lastmod", n.EditDate.ToString(DateFormat));
				writer.WriteElementString("changefreq", "weekly");
				writer.WriteElementString("priority", n.Parent_ContentID.HasValue ? "0.60" : "0.80");
				writer.WriteEndElement();
				writer.WriteRaw("\n");
			}

			writer.WriteEndDocument();

			writer.Flush();
			writer.Close();
		}

		public static HtmlString GetSiteMap() {
			StringBuilder sb = new StringBuilder();

			SiteData site = SiteData.CurrentSite;
			List<SiteNav> lstNav = new List<SiteNav>();

			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				//lstNav = navHelper.GetTwoLevelNavigation(SiteData.CurrentSiteID, true);
				lstNav = navHelper.GetLevelDepthNavigation(SiteData.CurrentSiteID, 4, true);
			}
			lstNav.RemoveAll(x => x.ShowInSiteMap == false);

			DateTime dtMax = lstNav.Min(x => x.EditDate);
			string DateFormat = "yyyy-MM-dd";

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.Encoding = Encoding.UTF8;
			settings.CheckCharacters = true;

			XmlWriter writer = XmlWriter.Create(sb, settings);

			//writer.WriteStartDocument();
			writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
			writer.WriteRaw("\n");
			writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
			writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
			writer.WriteAttributeString("xsi", "schemaLocation", null, "http://www.sitemaps.org/schemas/sitemap/0.9    http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

			writer.WriteRaw("\n");
			writer.WriteStartElement("url");
			writer.WriteElementString("loc", site.MainURL);
			writer.WriteElementString("lastmod", dtMax.ToString(DateFormat));
			writer.WriteElementString("priority", "1.0");
			writer.WriteEndElement();
			writer.WriteRaw("\n");

			// always, hourly, daily, weekly, monthly, yearly, never

			foreach (SiteNav n in lstNav) {
				writer.WriteStartElement("url");
				writer.WriteElementString("loc", site.ConstructedCanonicalURL(n));
				writer.WriteElementString("lastmod", n.EditDate.ToString(DateFormat));
				writer.WriteElementString("changefreq", "weekly");
				writer.WriteElementString("priority", n.Parent_ContentID.HasValue ? "0.60" : "0.80");
				writer.WriteEndElement();
				writer.WriteRaw("\n");
			}

			writer.WriteEndDocument();

			writer.Flush();
			writer.Close();

			return new HtmlString(sb.ToString());
		}
	}
}