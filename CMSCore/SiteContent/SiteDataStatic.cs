using Carrotware.CMS.Data;
using Carrotware.CMS.DBUpdater;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Caching;
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

	[SecuritySafeCritical]
	public partial class SiteData {

		public static string DefaultPageTitlePattern {
			get {
				return "[[CARROT_SITE_NAME]] - [[CARROT_PAGE_TITLEBAR]]";
			}
		}

		public static string CurrentTitlePattern {
			get {
				string pattern = "{0} - {1}";
				SiteData s = CurrentSite;
				if (!string.IsNullOrEmpty(s.SiteTitlebarPattern)) {
					StringBuilder sb = new StringBuilder(s.SiteTitlebarPattern);
					sb.Replace("[[CARROT_SITENAME]]", "{0}");
					sb.Replace("[[CARROT_SITE_NAME]]", "{0}");
					sb.Replace("[[CARROT_SITE_SLOGAN]]", "{1}");
					sb.Replace("[[CARROT_PAGE_TITLEBAR]]", "{2}");
					sb.Replace("[[CARROT_PAGE_PAGEHEAD]]", "{3}");
					sb.Replace("[[CARROT_PAGE_NAVMENUTEXT]]", "{4}");
					sb.Replace("[[CARROT_PAGE_DATE_GOLIVE]]", "{5}");
					sb.Replace("[[CARROT_PAGE_DATE_EDIT]]", "{6}");

					// [[CARROT_SITE_NAME]]: [[CARROT_PAGE_TITLEBAR]] ([[CARROT_PAGE_DATE_GOLIVE:MMMM d, yyyy]])
					var p5 = ParsePlaceholder(s.SiteTitlebarPattern, "[[CARROT_PAGE_DATE_GOLIVE:*]]", 5);
					if (!string.IsNullOrEmpty(p5.Key)) {
						sb.Replace(p5.Key, p5.Value);
					}

					// [[CARROT_SITE_NAME]]: [[CARROT_PAGE_TITLEBAR]] ([[CARROT_PAGE_DATE_EDIT:MMMM d, yyyy]])
					var p6 = ParsePlaceholder(s.SiteTitlebarPattern, "[[CARROT_PAGE_DATE_EDIT:*]]", 6);
					if (!string.IsNullOrEmpty(p6.Key)) {
						sb.Replace(p6.Key, p6.Value);
					}

					pattern = sb.ToString();
				}

				return pattern;
			}
		}

		private static KeyValuePair<string, string> ParsePlaceholder(string titleString, string placeHolder, int posNum) {
			var pair = new KeyValuePair<string, string>(string.Empty, string.Empty);

			if (placeHolder.Contains(":")) {
				string fragTest = placeHolder.Substring(0, placeHolder.IndexOf(":") + 1);

				string formatPattern = string.Format("{{{0}}}", posNum);

				if (titleString.Contains(fragTest)) {
					int idx1 = titleString.IndexOf(fragTest);
					int idx2 = titleString.IndexOf("]]", idx1 + 4);
					int len = idx2 - idx1 - fragTest.Length;

					if (idx1 >= 0 && idx2 > 0 && titleString.Contains(fragTest)) {
						string format = "M/d/yyyy"; // default date format

						if (len > 0) {
							format = titleString.Substring(idx1 + fragTest.Length, len);
						}
						placeHolder = placeHolder.Replace("*", format);

						formatPattern = string.Format("{{{0}:{1}}}", posNum, format);
						pair = new KeyValuePair<string, string>(placeHolder, formatPattern);
					}
				}
			}

			return pair;
		}

		public static List<SiteData> GetSiteList() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				return (from l in _db.carrot_Sites orderby l.SiteName select new SiteData(l)).ToList();
			}
		}

		public static SiteData GetSiteByID(Guid siteID) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				carrot_Site s = CompiledQueries.cqGetSiteByID(_db, siteID);

				if (s != null) {
#if DEBUG
					Debug.WriteLine(" ================ " + DateTime.UtcNow.ToString() + " ================");
					Debug.WriteLine("Grabbed site : GetSiteByID(Guid siteID) " + siteID.ToString());
#endif
					return new SiteData(s);
				} else {
					return null;
				}
			}
		}

		public static int BlogSortOrderNumber { get { return 10; } }

		public static bool IsWebView {
			get { return (HttpContext.Current != null); }
		}

		public static bool IsCurrentLikelyHomePage {
			get {
				if (!IsWebView) {
					return false;
				}
				return IsLikelyHomePage(CurrentScriptName);
			}
		}

		public static bool IsLikelyHomePage(string filePath) {
			if (!IsWebView) {
				return false;
			}

			return string.Format("{0}", filePath).Length < 2;
		}

		private static string SiteKeyPrefix = "cms_SiteData_";

		public static void RemoveSiteFromCache(Guid siteID) {
			string ContentKey = SiteKeyPrefix + siteID.ToString();
			try {
				HttpContext.Current.Cache.Remove(ContentKey);
			} catch { }
		}

		public static SiteData GetSiteFromCache(Guid siteID) {
			string ContentKey = SiteKeyPrefix + siteID.ToString();
			SiteData currentSite = null;
			if (IsWebView) {
				try { currentSite = (SiteData)HttpContext.Current.Cache[ContentKey]; } catch { }
				if (currentSite == null) {
					currentSite = GetSiteByID(siteID);
					if (currentSite != null) {
						HttpContext.Current.Cache.Insert(ContentKey, currentSite, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
					} else {
						HttpContext.Current.Cache.Remove(ContentKey);
					}
				}
			} else {
				currentSite = new SiteData();
				currentSite.SiteID = Guid.Empty;
				currentSite.SiteName = "MOCK SITE";
				currentSite.SiteTagline = "MOCK SITE TAGLINE";
				currentSite.MainURL = "http://localhost";
				currentSite.Blog_FolderPath = "archive";
				currentSite.Blog_CategoryPath = "category";
				currentSite.Blog_TagPath = "tag";
				currentSite.Blog_DatePath = "date";
				currentSite.Blog_EditorPath = "author";
				currentSite.TimeZoneIdentifier = "UTC";
				currentSite.Blog_DatePattern = "yyyy/MM/dd";
			}
			return currentSite;
		}

		public static SiteData CurrentSite {
			get {
				return GetSiteFromCache(CurrentSiteID);
			}
			set {
				string ContentKey = SiteKeyPrefix + CurrentSiteID.ToString();
				if (value == null) {
					HttpContext.Current.Cache.Remove(ContentKey);
				} else {
					HttpContext.Current.Cache.Insert(ContentKey, value, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
				}
			}
		}

		public static bool CurretSiteExists {
			get {
				return CurrentSite != null ? true : false;
			}
		}

		public static ContentPage GetCurrentPage() {
			ContentPage pageContents = null;

			if (IsWebView) {
				using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
					if (SecurityData.AdvancedEditMode) {
						if (cmsHelper.cmsAdminContent == null) {
							pageContents = GetCurrentLivePage();
							pageContents.LoadAttributes();
							cmsHelper.cmsAdminContent = pageContents;
						} else {
							pageContents = cmsHelper.cmsAdminContent;
						}
					} else {
						pageContents = GetCurrentLivePage();
						if (pageContents == null && (!SiteData.CurretSiteExists || DatabaseUpdate.AreCMSTablesIncomplete())) {
							pageContents = ContentPageHelper.GetEmptyHome();
						}
						if (SecurityData.CurrentUserGuid != Guid.Empty) {
							cmsHelper.cmsAdminContent = null;
						}
					}
				}
			} else {
				pageContents = ContentPageHelper.GetSamplerView();
			}

			return pageContents;
		}

		public static ContentPage GetPage(string sCurrentPage) {
			ContentPage pageContents = null;

			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				if (SecurityData.IsAdmin || SecurityData.IsSiteEditor) {
					if (sCurrentPage.Length <= 1 || sCurrentPage == SiteData.DefaultDirectoryFilename) {
						pageContents = pageHelper.FindHome(SiteData.CurrentSiteID);
					} else {
						pageContents = pageHelper.FindByFilename(SiteData.CurrentSiteID, sCurrentPage);
					}
				} else {
					if (sCurrentPage.Length <= 1 || sCurrentPage == SiteData.DefaultDirectoryFilename) {
						pageContents = pageHelper.FindHome(SiteData.CurrentSiteID, true);
					} else {
						pageContents = pageHelper.GetLatestContentByURL(SiteData.CurrentSiteID, true, sCurrentPage);
					}
				}
			}

			return pageContents;
		}

		public static ContentPage GetPage(Guid guidContentID) {
			ContentPage pageContents = null;
			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				pageContents = pageHelper.FindContentByID(CurrentSiteID, guidContentID);
			}
			return pageContents;
		}

		public static List<Widget> GetCurrentPageWidgets(Guid guidContentID) {
			List<Widget> pageWidgets = new List<Widget>();

			if (IsWebView) {
				using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
					if (SecurityData.AdvancedEditMode) {
						if (cmsHelper.cmsAdminWidget == null) {
							pageWidgets = GetCurrentPageLiveWidgets(guidContentID);
							cmsHelper.cmsAdminWidget = (from w in pageWidgets
														orderby w.WidgetOrder, w.EditDate
														select w).ToList();
						} else {
							pageWidgets = (from w in cmsHelper.cmsAdminWidget
										   orderby w.WidgetOrder, w.EditDate
										   select w).ToList();
						}
					} else {
						pageWidgets = GetCurrentPageLiveWidgets(guidContentID);
						if (SecurityData.CurrentUserGuid != Guid.Empty) {
							cmsHelper.cmsAdminWidget = null;
						}
					}
				}
			}

			return pageWidgets;
		}

		public static ContentPage GetCurrentLivePage() {
			ContentPage pageContents = null;

			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				bool IsPageTemplate = false;
				string sCurrentPage = SiteData.CurrentScriptName;
				string sScrubbedURL = SiteData.AlternateCurrentScriptName;

				if (sScrubbedURL.ToLowerInvariant() != sCurrentPage.ToLowerInvariant()) {
					sCurrentPage = sScrubbedURL;
				}

				if (SecurityData.IsAdmin || SecurityData.IsSiteEditor) {
					if (sCurrentPage.Length <= 1 || sCurrentPage == SiteData.DefaultDirectoryFilename) {
						pageContents = pageHelper.FindHome(SiteData.CurrentSiteID);
					} else {
						pageContents = pageHelper.FindByFilename(SiteData.CurrentSiteID, sCurrentPage);
					}
				} else {
					if (sCurrentPage.Length <= 1 || sCurrentPage == SiteData.DefaultDirectoryFilename) {
						pageContents = pageHelper.FindHome(SiteData.CurrentSiteID, true);
					} else {
						pageContents = pageHelper.GetLatestContentByURL(SiteData.CurrentSiteID, true, sCurrentPage);
					}
				}

				if (pageContents == null && SiteData.IsPageReal) {
					IsPageTemplate = true;
				}

				if ((SiteData.IsPageSampler || IsPageTemplate || !IsWebView) && pageContents == null) {
					pageContents = ContentPageHelper.GetSamplerView();
				}

				if (IsPageTemplate) {
					pageContents.TemplateFile = sCurrentPage;
				}
			}

			return pageContents;
		}

		public static List<Widget> GetCurrentPageLiveWidgets(Guid guidContentID) {
			List<Widget> pageWidgets = new List<Widget>();

			using (WidgetHelper widgetHelper = new WidgetHelper()) {
				pageWidgets = widgetHelper.GetWidgets(guidContentID, !SecurityData.AdvancedEditMode);
			}

			return pageWidgets;
		}

		public static Guid CurrentSiteID {
			get {
				Guid _site = Guid.Empty;
				if (IsWebView) {
					CarrotCakeConfig config = CarrotCakeConfig.GetConfig();
					if (config.MainConfig != null
						&& config.MainConfig.SiteID != null) {
						_site = config.MainConfig.SiteID.Value;
					}

					if (_site == Guid.Empty) {
						try {
							DynamicSite s = CMSConfigHelper.DynSite;
							if (s != null) {
								_site = s.SiteID;
							}
						} catch { }
					}
				}
				return _site;
			}
		}

		public static SiteData InitNewSite(Guid siteID) {
			SiteData site = new SiteData();
			site.SiteID = siteID;
			site.BlockIndex = true;

			site.MainURL = "http://" + CMSConfigHelper.DomainName;
			site.SiteName = CMSConfigHelper.DomainName;

			site.SiteTitlebarPattern = SiteData.DefaultPageTitlePattern;

			site.Blog_FolderPath = "archive";
			site.Blog_CategoryPath = "category";
			site.Blog_TagPath = "tag";
			site.Blog_DatePath = "date";
			site.Blog_EditorPath = "author";
			site.Blog_DatePattern = "yyyy/MM/dd";

			site.TimeZoneIdentifier = TimeZoneInfo.Local.Id;

			return site;
		}

		public static AspNetHostingPermissionLevel CurrentTrustLevel {
			get {
				foreach (AspNetHostingPermissionLevel trustLevel in
					new AspNetHostingPermissionLevel[] {
						AspNetHostingPermissionLevel.Unrestricted,
						AspNetHostingPermissionLevel.High,
						AspNetHostingPermissionLevel.Medium,
						AspNetHostingPermissionLevel.Low,
						AspNetHostingPermissionLevel.Minimal
					  }) {
					try {
						new AspNetHostingPermission(trustLevel).Demand();
					} catch (System.Security.SecurityException) {
						continue;
					}

					return trustLevel;
				}

				return AspNetHostingPermissionLevel.None;
			}
		}

		public static string SiteSearchPageName {
			get { return "/search"; }
		}

		public static string ReadEmbededScript(string sResouceName) {
			string sReturn = null;

			Assembly _assembly = Assembly.GetExecutingAssembly();
			using (var stream = new StreamReader(_assembly.GetManifestResourceStream(sResouceName))) {
				sReturn = stream.ReadToEnd();
			}

			return sReturn;
		}

		public static void ManuallyWriteDefaultFile(HttpContext context, Exception objErr) {
			Assembly _assembly = Assembly.GetExecutingAssembly();

			string sBody = ReadEmbededScript("Carrotware.CMS.Core.SiteContent.Default.htm");

			try {
				if (CurretSiteExists) {
					sBody = sBody.Replace("{TIME_STAMP}", CurrentSite.Now.ToString());
				}
			} catch { }
			sBody = sBody.Replace("{TIME_STAMP}", DateTime.Now.ToString());

			if (objErr != null) {
				sBody = sBody.Replace("{LONG_NAME}", FormatToHTML(" [" + objErr.GetType().ToString() + "] " + objErr.Message));

				if (objErr.StackTrace != null) {
					sBody = sBody.Replace("{STACK_TRACE}", FormatToHTML(objErr.StackTrace));
				}
				if (objErr.InnerException != null) {
					sBody = sBody.Replace("{CONTENT_DETAIL}", FormatToHTML(objErr.InnerException.Message));
				}
			}

			sBody = sBody.Replace("{STACK_TRACE}", "");
			sBody = sBody.Replace("{CONTENT_DETAIL}", "");

			sBody = sBody.Replace("{SITE_ROOT_PATH}", SiteData.AdminFolderPath);

			context.Response.ContentType = "text/html";
			context.Response.Clear();
			context.Response.BufferOutput = true;

			context.Response.Write(sBody);
			context.Response.Flush();
			context.Response.End();
		}

		private static string FormatToHTML(string inputString) {
			string outputString = string.Empty;
			if (!string.IsNullOrEmpty(inputString)) {
				StringBuilder sb = new StringBuilder(inputString);
				sb.Replace("\r\n", " <br \\> \r\n");
				sb.Replace("   ", "&nbsp;&nbsp;&nbsp;");
				sb.Replace("  ", "&nbsp;&nbsp;");
				sb.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
				outputString = sb.ToString();
			}
			return outputString;
		}

		public static string FormatErrorOutput(Exception objErr) {
			string sBody = ReadEmbededScript("Carrotware.CMS.Core.SiteContent.ErrorFormat.htm");

			if (objErr is HttpException) {
				HttpException httpEx = (HttpException)objErr;

				sBody = sBody.Replace("{PAGE_TITLE}", httpEx.Message);
				sBody = sBody.Replace("{SHORT_NAME}", httpEx.Message);
				sBody = sBody.Replace("{LONG_NAME}", "HTTP " + httpEx.GetHttpCode() + " - " + FormatToHTML(httpEx.Message));
			} else {
				sBody = sBody.Replace("{PAGE_TITLE}", objErr.Message);
				sBody = sBody.Replace("{SHORT_NAME}", objErr.Message);
				sBody = sBody.Replace("{LONG_NAME}", FormatToHTML(" [" + objErr.GetType().ToString() + "] " + objErr.Message));
			}

			if (objErr.StackTrace != null) {
				sBody = sBody.Replace("{STACK_TRACE}", FormatToHTML(objErr.StackTrace));
			}

			if (objErr.InnerException != null) {
				sBody = sBody.Replace("{CONTENT_DETAIL}", FormatToHTML(objErr.InnerException.Message));
			}

			if (CurretSiteExists) {
				sBody = sBody.Replace("{TIME_STAMP}", CurrentSite.Now.ToString());
			}
			sBody = sBody.Replace("{TIME_STAMP}", DateTime.Now.ToString());

			sBody = sBody.Replace("{CONTENT_DETAIL}", "");
			sBody = sBody.Replace("{STACK_TRACE}", "");

			return sBody;
		}

		public static void Show404MessageFull(bool bResponseEnd) {
			HttpContext context = HttpContext.Current;
			context.Response.StatusCode = 404;
			context.Response.AppendHeader("Status", "HTTP/1.1 404 Object Not Found");
			context.Response.Cache.SetLastModified(DateTime.Today.Date);
			//context.Response.Write("<h2>404 Not Found</h2><p>HTTP 404. The resource you are looking for (or one of its dependencies) could have been removed, had its name changed, or is temporarily unavailable.  Please review the following URL and make sure that it is spelled correctly. </p>");

			Exception errInner = new Exception("The resource you are looking for (or one of its dependencies) could have been removed, had its name changed, or is temporarily unavailable. Please review the following URL and make sure that it is spelled correctly.");
			HttpException err = new HttpException(404, "File or directory not found.", errInner);

			context.Response.Write(FormatErrorOutput(err));

			if (bResponseEnd) {
				context.Response.End();
			}
		}

		public static void Show404MessageShort() {
			HttpContext context = HttpContext.Current;
			context.Response.StatusCode = 404;
			context.Response.StatusDescription = "Not Found";
		}

		public static void Show301Message(string sFileRequested) {
			HttpContext context = HttpContext.Current;
			context.Response.StatusCode = 301;
			context.Response.AppendHeader("Status", "301 Moved Permanently");
			context.Response.AppendHeader("Location", sFileRequested);
			context.Response.Cache.SetLastModified(DateTime.Today.Date);
			//context.Response.Write("<h2>301 Moved Permanently</h2>");

			HttpException ex = new HttpException(301, "301 Moved Permanently");
			context.Response.Write(FormatErrorOutput(ex));
		}

		public static void WriteDebugException(string sSrc, Exception objErr) {
			bool bWriteError = false;

			CarrotCakeConfig config = CarrotCakeConfig.GetConfig();

			if (config.ExtraOptions != null && config.ExtraOptions.WriteErrorLog) {
				bWriteError = config.ExtraOptions.WriteErrorLog;
			}
#if DEBUG
			bWriteError = true; // always write errors when debug build
#endif
			DatabaseUpdate.WriteDebugException(bWriteError, sSrc, objErr);
		}

		public static void Perform404Redirect(string sReqURL) {
			PerformRedirectToErrorPage(404, sReqURL);
		}

		public static void PerformRedirectToErrorPage(int ErrorKey, string sReqURL) {
			PerformRedirectToErrorPage(ErrorKey.ToString(), sReqURL);
		}

		public static void PerformRedirectToErrorPage(string sErrorKey, string sReqURL) {
			//parse web.config as XML because of medium trust issues
			HttpContext context = HttpContext.Current;

			XmlDocument xDoc = new XmlDocument();
			xDoc.Load(context.Server.MapPath("~/Web.config"));

			XmlElement xmlCustomErrors = xDoc.SelectSingleNode("//system.web/customErrors") as XmlElement;

			if (xmlCustomErrors != null) {
				string redirectPage = string.Empty;

				if (xmlCustomErrors.Attributes["mode"] != null && xmlCustomErrors.Attributes["mode"].Value.ToLowerInvariant() != "off") {
					if (xmlCustomErrors.Attributes["defaultRedirect"] != null) {
						redirectPage = xmlCustomErrors.Attributes["defaultRedirect"].Value;
					}

					if (xmlCustomErrors.HasChildNodes) {
						XmlNode xmlErrNode = xmlCustomErrors.SelectSingleNode("//system.web/customErrors/error[@statusCode='" + sErrorKey + "']");
						if (xmlErrNode != null) {
							redirectPage = xmlErrNode.Attributes["redirect"].Value;
						}
					}
					string sQS = string.Empty;
					if (context.Request.QueryString != null) {
						if (!string.IsNullOrEmpty(context.Request.QueryString.ToString())) {
							sQS = HttpUtility.UrlEncode(string.Format("?{0}", context.Request.QueryString));
						}
					}

					if (!string.IsNullOrEmpty(redirectPage) && !sQS.ToLowerInvariant().Contains("aspxerrorpath")) {
						context.Response.Redirect(string.Format("{0}?aspxerrorpath={1}{2}", redirectPage, sReqURL, sQS));
					}
				}
			}

			/*
			Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
			CustomErrorsSection section = (CustomErrorsSection)config.GetSection("system.web/customErrors");

			if (section != null) {
				if (section.Mode != CustomErrorsMode.Off) {
					CustomError configuredError = section.Errors[sErrorKey];
					if (configuredError != null) {
						if (!string.IsNullOrEmpty(configuredError.Redirect)) {
							context.Response.Redirect(configuredError.Redirect + "?aspxerrorpath=" + sReqURL);
						}
					} else {
						if (!string.IsNullOrEmpty(section.DefaultRedirect)) {
							context.Response.Redirect(section.DefaultRedirect + "?aspxerrorpath=" + sReqURL);
						}
					}
				}
			}
			*/
		}

		public static bool IsFilenameCurrentPage(string sCurrentFile) {
			if (string.IsNullOrEmpty(sCurrentFile)) {
				return false;
			}

			if (sCurrentFile.Contains("?")) {
				sCurrentFile = sCurrentFile.Substring(0, sCurrentFile.IndexOf("?"));
			}

			if (sCurrentFile.ToLowerInvariant() == SiteData.CurrentScriptName.ToLowerInvariant()
				|| sCurrentFile.ToLowerInvariant() == SiteData.AlternateCurrentScriptName.ToLowerInvariant()) {
				return true;
			}
			return false;
		}

		public static string StarterHomePageSample {
			get {
				string sBody = ReadEmbededScript("Carrotware.CMS.Core.SiteContent.FirstPage.txt");

				return sBody;
			}
		}

		public static SiteNav SiteBlogPage {
			get {
				if (CurrentSite.Blog_Root_ContentID.HasValue) {
					using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
						return navHelper.GetLatestVersion(CurrentSite.SiteID, CurrentSite.Blog_Root_ContentID.Value);
					}
				}
				return null;
			}
		}

		public static string SearchQueryParameter {
			get { return "query".ToLowerInvariant(); }
		}

		public static string AdvancedEditParameter {
			get { return "carrotedit".ToLowerInvariant(); }
		}

		public static string TemplatePreviewParameter {
			get { return "carrot_templatepreview".ToLowerInvariant(); }
		}

		public static string DefaultDirectoryFilename {
			get { return "/"; }
		}

		public static string DefaultTemplateFilename {
			get { return "~/Views/CmsContent/PlainPage/PlainPageView.cshtml".ToLowerInvariant(); }
		}

		public static string PreviewTemplateFilePage {
			get { return AdminFolderPath + "TemplatePreview"; }
		}

		public static bool IsPageSampler {
			get {
				return CurrentScriptName.ToLowerInvariant().StartsWith(PreviewTemplateFilePage.ToLowerInvariant())
						&& HttpContext.Current.Request.QueryString[TemplatePreviewParameter] != null;
			}
		}

		public static bool IsPageReal {
			get {
				if (IsWebView
					&& CurrentScriptName.ToLowerInvariant() != DefaultDirectoryFilename.ToLowerInvariant()
					&& File.Exists(HttpContext.Current.Server.MapPath(CurrentScriptName))) {
					return true;
				} else {
					return false;
				}
			}
		}

		private static List<string> _specialFiles = null;

		public static List<string> SpecialFiles {
			get {
				if (_specialFiles == null) {
					_specialFiles = new List<string>();
					//_specialFiles.Add(DefaultTemplateFilename);
					//_specialFiles.Add(DefaultDirectoryFilename);
					//_specialFiles.Add("/feed/rss.ashx");
					//_specialFiles.Add("/feed/sitemap.ashx");
					//_specialFiles.Add("/feed/xmlrpc.ashx");
				}

				return _specialFiles;
			}
		}

		public static bool IsCurrentPageSpecial {
			get {
				return SiteData.SpecialFiles.Contains(CurrentScriptName.ToLowerInvariant());
			}
		}

		public static bool IsPageSpecial(string sPageName) {
			return SiteData.SpecialFiles.Contains(sPageName.ToLowerInvariant()) || sPageName.ToLowerInvariant().StartsWith(AdminFolderPath);
		}

		public static string PreviewTemplateFile {
			get {
				string _preview = DefaultTemplateFilename;

				if (IsWebView) {
					if (HttpContext.Current.Request.QueryString[TemplatePreviewParameter] != null) {
						_preview = HttpContext.Current.Request.QueryString[TemplatePreviewParameter].ToString();
						_preview = CMSConfigHelper.DecodeBase64(_preview);
					}
				}

				return _preview;
			}
		}

		private static Version CurrentVersion {
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}

		public static string CurrentDLLVersion {
			get { return CurrentVersion.ToString(); }
		}

		public static string CurrentDLLMajorMinorVersion {
			get {
				Version v = CurrentVersion;
				return v.Major.ToString() + "." + v.Minor.ToString();
			}
		}

		public static string CarrotCakeCMSVersion {
			get {
#if DEBUG
				return string.Format("CarrotCake CMS MVC {0} DEBUG MODE", CurrentDLLVersion);
#endif
				return string.Format("CarrotCake CMS MVC {0}", CurrentDLLVersion);
			}
		}

		public static string CarrotCakeCMSVersionMM {
			get {
#if DEBUG
				return string.Format("CarrotCake CMS {0} (debug)", CurrentDLLMajorMinorVersion);
#endif
				return string.Format("CarrotCake CMS {0}", CurrentDLLMajorMinorVersion);
			}
		}

		public static string CurrentScriptName {
			get {
				string sPath = "/";
				try { sPath = HttpContext.Current.Request.ServerVariables["script_name"].ToString(); } catch { }
				return sPath;
			}
		}

		public static string RefererScriptName {
			get {
				string sPath = string.Empty;
				try { sPath = HttpContext.Current.Request.ServerVariables["http_referer"].ToString(); } catch { }
				return sPath;
			}
		}

		public static string AdminDefaultFile {
			get {
				return (AdminFolderPath + DefaultDirectoryFilename).Replace("//", "/");
			}
		}

		private static string _adminFolderPath = null;

		public static string AdminFolderPath {
			get {
				if (_adminFolderPath == null) {
					string _defPath = "/c3-admin/";
					CarrotCakeConfig config = CarrotCakeConfig.GetConfig();
					if (config.MainConfig != null && !string.IsNullOrEmpty(config.MainConfig.AdminFolderPath)) {
						_adminFolderPath = config.MainConfig.AdminFolderPath;
						_adminFolderPath = string.Format("/{0}/", _adminFolderPath).Replace(@"\", "/").Replace("///", "/").Replace("//", "/").Replace("//", "/").Trim();
					} else {
						_adminFolderPath = _defPath;
					}
					if (string.IsNullOrEmpty(_adminFolderPath) || _adminFolderPath.Length < 2) {
						_adminFolderPath = _defPath;
					}
				}
				return _adminFolderPath;
			}
		}

		public static string AlternateCurrentScriptName {
			get {
				string sCurrentPage = CurrentScriptName;

				if (IsWebView) {
					if (!CurrentScriptName.ToLowerInvariant().StartsWith(AdminFolderPath)) {
						string sScrubbedURL = CheckForSpecialURL(CurrentSite);

						//if (sScrubbedURL.ToLowerInvariant() == sCurrentPage.ToLowerInvariant()) {
						//	sCurrentPage = AppendDefaultPath(sCurrentPage);
						//}

						if (sCurrentPage.EndsWith("/")) {
							sCurrentPage = sCurrentPage.Substring(0, sCurrentPage.Length - 1);
						}

						if (!sScrubbedURL.ToLowerInvariant().StartsWith(sCurrentPage.ToLowerInvariant())
							&& !sCurrentPage.ToLowerInvariant().EndsWith(DefaultDirectoryFilename)) {
							if (sScrubbedURL.ToLowerInvariant() != sCurrentPage.ToLowerInvariant()) {
								sCurrentPage = sScrubbedURL;
							}
						}
					}
				}

				return sCurrentPage;
			}
		}

		public static string CheckForSpecialURL(SiteData site) {
			string sRequestedURL = "/";

			if (IsWebView) {
				sRequestedURL = CurrentScriptName;
				string sFileRequested = sRequestedURL;

				if (!sRequestedURL.ToLowerInvariant().StartsWith(AdminFolderPath) && site != null) {
					if (sFileRequested.ToLowerInvariant().StartsWith(site.BlogFolderPath.ToLowerInvariant())) {
						if (site.GetSpecialFilePathPrefixes().Where(x => sFileRequested.ToLowerInvariant().StartsWith(x)).Count() > 0) {
							if (site.Blog_Root_ContentID.HasValue) {
								using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
									SiteNav blogNavPage = navHelper.GetLatestVersion(site.SiteID, site.Blog_Root_ContentID.Value);
									if (blogNavPage != null) {
										sRequestedURL = blogNavPage.FileName;
									}
								}
							}
						}
					}
				}
			}

			return sRequestedURL;
		}

		public static string ReferringPage {
			get {
				string r = SiteData.CurrentScriptName;
				try { r = HttpContext.Current.Request.ServerVariables["http_referer"].ToString(); } catch { }
				if (string.IsNullOrEmpty(r))
					r = DefaultDirectoryFilename;
				return r;
			}
		}

		public static string RssDocType { get { return "application/rss+xml"; } }


		public static string RawMode { get { return "raw"; } }
		public static string HtmlMode { get { return "html"; } }

		public static string EditMode(string mode) {
			return (string.IsNullOrEmpty(mode) || mode.Trim().ToLowerInvariant() != RawMode) ? HtmlMode.ToLowerInvariant() : RawMode.ToLowerInvariant();
		}
	}
}