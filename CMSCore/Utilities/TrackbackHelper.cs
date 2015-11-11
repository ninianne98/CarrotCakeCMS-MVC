using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

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

	public class TrackbackHelper {

		public TrackbackHelper() { }

		public string PostRequest(Uri url, string postData) {
			if (postData == null) {
				throw new ArgumentNullException("postData");
			}

			Uri urlTB = FindTrackBackURL(url);
			if (urlTB == null && url != null) {
				urlTB = url;
			}

			string userAgent = SiteData.CarrotCakeCMSVersion + "; " + SiteData.CurrentSite.MainURL;

			ServicePointManager.Expect100Continue = false;
			HttpWebRequest request = WebRequest.Create(urlTB) as HttpWebRequest;

			request.UserAgent = userAgent;
			request.Timeout = 20000;
			request.Method = "POST";
			request.ContentLength = postData.Length;
			request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
			request.KeepAlive = true;

			using (StreamWriter myWriter = new StreamWriter(request.GetRequestStream())) {
				myWriter.Write(postData);
			}

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			if (response.StatusCode < HttpStatusCode.OK && response.StatusCode >= HttpStatusCode.Ambiguous) {
				throw new Exception(String.Format(response.StatusCode.ToString()));
			}

			var responseText = String.Empty;
			using (var reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII)) {
				responseText = reader.ReadToEnd();
			}

			return responseText;
		}

		public XmlDocument LoadText(string InputString) {
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(InputString);
			return doc;
		}

		public Uri FindTrackBackURL(string sURL) {
			return FindTrackBackURL(new Uri(sURL));
		}

		public Uri FindTrackBackURL(Uri url) {
			Uri urlTB = null;
			string sPageData = GetPageHtml(url);

			if (sPageData.Contains("<rdf:RDF") && sPageData.Contains("</rdf:RDF>")) {
				int iB = sPageData.IndexOf("<rdf:RDF");
				int iE = sPageData.IndexOf("</rdf:RDF>");

				string sRDF = sPageData.Substring(iB, (iE - iB) + "</rdf:RDF>".Length);
				XmlDocument doc = LoadText(sRDF);

				XmlNamespaceManager rssNamespace = new XmlNamespaceManager(doc.NameTable);
				rssNamespace.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
				rssNamespace.AddNamespace("trackback", "http://madskills.com/public/xml/rss/module/trackback/");
				rssNamespace.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");

				XmlNode node = doc.FirstChild.FirstChild;

				if (node.Attributes["trackback:ping"] != null) {
					string sTBURL = node.Attributes["trackback:ping"].InnerText;
					urlTB = new Uri(sTBURL);
				}
			}

			sPageData = null;

			return urlTB;
		}

		public void ProcessTrackback(HttpContext context, bool bRequireID) {
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

			response.Buffer = false;
			response.Clear();
			response.ContentType = "application/xml";

			XmlWriter writer = XmlWriter.Create(response.Output);

			if (request.Form.AllKeys.Count() < 6 && (!String.IsNullOrEmpty(request["id"]) || !bRequireID)
				&& !String.IsNullOrEmpty(request["excerpt"])
				&& !String.IsNullOrEmpty(request["blog_name"])) {
				string blogId = String.Empty;
				if (!String.IsNullOrEmpty(request["id"]))
					blogId = request["id"];

				if (!bRequireID && String.IsNullOrEmpty(blogId)) {
					blogId = request.Path;
				}

				string blogTitle = String.Empty;
				if (!String.IsNullOrEmpty(request["blog_name"]))
					blogTitle = request["blog_name"];

				string url = String.Empty;
				if (!String.IsNullOrEmpty(request["url"]))
					url = request["url"];

				string title = String.Empty;
				if (!String.IsNullOrEmpty(request["title"]))
					title = request["title"];

				string excerpt = String.Empty;
				if (!String.IsNullOrEmpty(request["excerpt"]))
					excerpt = request["excerpt"];

				TrackBackInfo tb = new TrackBackInfo {
					RequestSourceURL = url,
					BlogName = blogTitle,
					BlogPostID = blogId,
					BlogPostTitle = title,
					BlogExcerpt = excerpt
				};

				if (request.HttpMethod == "POST") {
					// Store trackback based on the id parameter
					GenerateSuccessResponse(request, writer, tb);
				} else {
					GenerateErrorResponse(2, "Only HTTP POST verb can be used to send trackbacks", writer);
				}
			} else {
				GenerateErrorResponse(1, "Item identifier is missing", writer);
			}
		}

		public void GenerateTrackBackDisabled(HttpContext context) {
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

			response.Buffer = false;
			response.Clear();
			response.ContentType = "application/xml";

			XmlWriter writer = XmlWriter.Create(response.Output);

			GenerateErrorResponse(1, "Sorry, trackbacks are not accepted for this site.", writer);
		}

		public void GenerateSuccessResponse(HttpRequest request, XmlWriter writer, TrackBackInfo tb) {
			SiteNav navData = null;
			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				navData = navHelper.FindByFilename(SiteData.CurrentSiteID, tb.BlogPostID);
			}
			string sReferer = null;
			string sIP = request.ServerVariables["REMOTE_ADDR"].ToString();
			try { sReferer = request.ServerVariables["HTTP_REFERER"].ToString(); } catch { }

			PostComment pc = new PostComment();
			pc.ContentCommentID = Guid.NewGuid();
			pc.Root_ContentID = navData.Root_ContentID;
			pc.CreateDate = SiteData.CurrentSite.Now;
			pc.IsApproved = false;
			pc.IsSpam = false;
			pc.CommenterIP = sIP;
			pc.CommenterEmail = "trackback";

			pc.CommenterName = tb.BlogName;
			pc.PostCommentText = tb.BlogExcerpt;

#if DEBUG
			pc.PostCommentText = request.UserAgent + "\r\n" + tb.RequestSourceURL + "\r\n" + tb.BlogExcerpt;
#endif

			pc.CommenterURL = tb.RequestSourceURL;
			if (!String.IsNullOrEmpty(sReferer)) {
				pc.CommenterURL = sReferer;
			}

			pc.Save();

			writer.WriteStartElement("response");
			writer.WriteElementString("error", "0");
			writer.WriteStartElement("rss");
			writer.WriteAttributeString("version", "0.91");
			writer.WriteStartElement("channel");

			if (navData != null) {
				writer.WriteElementString("title", navData.NavMenuText);
				writer.WriteElementString("link", SiteData.CurrentSite.ConstructedCanonicalURL(navData));
				writer.WriteElementString("description", navData.PageTextPlainSummary.ToString());
			}

			writer.WriteElementString("language", "");

			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.Flush();
			writer.Close();
		}

		public void GenerateErrorResponse(int number, string message, XmlWriter writer) {
			writer.WriteStartElement("response");

			if (number > 0) {
				writer.WriteElementString("error", number.ToString());
			}
			if (!String.IsNullOrEmpty(message)) {
				writer.WriteElementString("message", message);
			}

			writer.WriteEndElement();

			writer.Flush();
			writer.Close();
		}

		private string GetPageHtml(Uri uri) {
			string result = null;

			WebRequest request = WebRequest.Create(uri);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			string encodingName = response.ContentEncoding.Trim();

			if (String.IsNullOrEmpty(encodingName)) {
				encodingName = "utf-8";
			}

			Encoding encoding = Encoding.GetEncoding(encodingName);

			using (Stream stream = response.GetResponseStream()) {
				using (StreamReader reader = new StreamReader(stream, encoding))
					result = reader.ReadToEnd();
			}

			return result;
		}
	}

	//=================================
	public class TrackBackInfo {

		public TrackBackInfo() { }

		public string BlogPostID { get; set; }
		public string BlogName { get; set; }
		public string BlogPostTitle { get; set; }
		public string BlogExcerpt { get; set; }
		public string RequestSourceURL { get; set; }
	}

	//========================

	public class TrackBacker {
		public SiteData BlogSite { get; set; }
		public Uri DestinationUrl { get; set; }
		public SiteNav SourcePage { get; set; }

		public string SendTrackback(Guid sourceGuid, Guid siteID, string sTgtURL) {
			this.DestinationUrl = new Uri(sTgtURL);
			this.BlogSite = SiteData.GetSiteFromCache(siteID);

			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				this.SourcePage = navHelper.GetLatestVersion(siteID, sourceGuid);
			}

			return SendTrackback();
		}

		public string SendTrackback(string sourceURL, Guid siteID, string sTgtURL) {
			this.DestinationUrl = new Uri(sTgtURL);
			this.BlogSite = SiteData.GetSiteFromCache(siteID);

			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				this.SourcePage = navHelper.FindByFilename(siteID, sourceURL);
			}

			return SendTrackback();
		}

		public string SendTrackback(Uri targetURL, SiteNav pageNav) {
			this.DestinationUrl = targetURL;
			this.SourcePage = pageNav;
			this.BlogSite = SiteData.GetSiteFromCache(this.SourcePage.SiteID);

			return SendTrackback();
		}

		public string SendTrackback() {
			if (this.BlogSite.SendTrackbacks) {
				string sCanonicalURL = this.BlogSite.ConstructedCanonicalURL(this.SourcePage);

				StringBuilder formParams = new StringBuilder();
				formParams.AppendFormat("blog_name={0}", HttpUtility.UrlEncode(this.BlogSite.SiteName));
				formParams.AppendFormat("&url={0}", HttpUtility.UrlEncode(sCanonicalURL));
				formParams.AppendFormat("&title={0}", HttpUtility.UrlEncode(this.SourcePage.NavMenuText));
				formParams.AppendFormat("&excerpt={0}", HttpUtility.UrlEncode(this.SourcePage.PageTextPlainSummary.ToString()));

				TrackbackHelper client = new TrackbackHelper();

				return client.PostRequest(this.DestinationUrl, formParams.ToString());
			} else {
				return "site not currently configured to allow sending trackbacks";
			}
		}
	}
}