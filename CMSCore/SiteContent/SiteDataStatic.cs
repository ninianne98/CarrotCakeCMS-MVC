using Carrotware.CMS.Data;
using Carrotware.CMS.DBUpdater;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
			if (!IsWebView || filePath == null) {
				return false;
			}

			return string.Format("{0}", filePath).Length <= 1
				|| filePath.ToLowerInvariant() == SiteData.DefaultDirectoryFilename;
		}

		public static bool IsLikelyFakeSearch() {
			if (!IsWebView) {
				return false;
			}
			if (CurrentSite == null) {
				return false;
			}
			// no blog index is set, but the URL looks like a search is happening
			return !CurrentSite.Blog_Root_ContentID.HasValue
						&& (CurrentSite.IsBlogDateFolderPath
								|| CurrentSite.IsBlogCategoryPath
								|| CurrentSite.IsBlogTagPath
								|| CurrentSite.IsBlogEditorFolderPath
								|| CurrentSite.IsSiteSearchPath);
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

		public static bool IsUniqueFilename(string theFileName, Guid pageId) {
			try {
				if (theFileName.Length < 2) {
					return false;
				}

				theFileName = ContentPageHelper.ScrubFilename(pageId, theFileName);
				theFileName = theFileName.ToLowerInvariant();

				if (SiteData.IsPageSpecial(theFileName) || SiteData.IsLikelyHomePage(theFileName)) {
					return false;
				}

				if (SiteData.CurrentSite.GetSpecialFilePathPrefixes().Where(x => theFileName.StartsWith(x.ToLowerInvariant())).Any()
							|| theFileName.StartsWith(SiteData.CurrentSite.BlogFolderPath.ToLowerInvariant())) {
					return false;
				}

				using (var pageHelper = new ContentPageHelper()) {
					ContentPage fn = pageHelper.FindByFilename(SiteData.CurrentSite.SiteID, theFileName);
					ContentPage cp = pageHelper.FindContentByID(SiteData.CurrentSite.SiteID, pageId);

					if (cp == null && pageId != Guid.Empty) {
						cp = pageHelper.GetVersion(SiteData.CurrentSite.SiteID, pageId);
					}

					if (fn == null || (fn != null && cp != null && fn.Root_ContentID == cp.Root_ContentID)) {
						return true;
					} else {
						return false;
					}
				}
			} catch (Exception ex) {
				SiteData.WriteDebugException("isuniquefilename", ex);

				throw;
			}
		}

		public static bool IsUniqueBlogFilename(string pageSlug, DateTime dateGoLive, Guid pageId) {
			try {
				if (pageSlug.Length < 2) {
					return false;
				}

				DateTime dateOrigGoLive = DateTime.MinValue;

				pageSlug = ContentPageHelper.ScrubFilename(pageId, pageSlug);
				pageSlug = pageSlug.ToLowerInvariant();

				string theFileName = pageSlug;

				using (var pageHelper = new ContentPageHelper()) {
					ContentPage cp = pageHelper.FindContentByID(SiteData.CurrentSite.SiteID, pageId);

					if (cp != null) {
						dateOrigGoLive = cp.GoLiveDate;
					}
					if (cp == null && pageId != Guid.Empty) {
						ContentPageExport cpe = ContentImportExportUtils.GetSerializedContentPageExport(pageId);
						if (cpe != null) {
							dateOrigGoLive = cpe.ThePage.GoLiveDate;
						}
					}

					theFileName = ContentPageHelper.CreateFileNameFromSlug(SiteData.CurrentSite, dateGoLive, pageSlug);

					if (SiteData.IsPageSpecial(theFileName) || SiteData.IsLikelyHomePage(theFileName)) {
						return false;
					}

					ContentPage fn1 = pageHelper.FindByFilename(SiteData.CurrentSite.SiteID, theFileName);

					if (cp == null && pageId != Guid.Empty) {
						cp = pageHelper.GetVersion(SiteData.CurrentSite.SiteID, pageId);
					}

					if (fn1 == null || (fn1 != null && cp != null && fn1.Root_ContentID == cp.Root_ContentID)) {
						return true;
					} else {
						return false;
					}
				}
			} catch (Exception ex) {
				SiteData.WriteDebugException("isuniqueblogfilename", ex);

				throw;
			}
		}

		public static string GenerateNewFilename(Guid pageId, string pageTitle, DateTime goLiveDate,
					ContentPageType.PageType pageType) {
			try {
				if (string.IsNullOrEmpty(pageTitle)) {
					pageTitle = pageId.ToString();
				}
				pageTitle = pageTitle.Replace("/", "-");
				string theFileName = ContentPageHelper.ScrubFilename(pageId, pageTitle);
				string testFile = string.Empty;

				if (pageType == ContentPageType.PageType.ContentEntry) {
					var resp = IsUniqueFilename(theFileName, pageId);
					if (resp == false) {
						for (int i = 1; i < 2500; i++) {
							testFile = string.Format("{0}-{1}", pageTitle, i);
							resp = IsUniqueFilename(testFile, pageId);
							if (resp) {
								theFileName = testFile;
								break;
							} else {
								theFileName = string.Empty;
							}
						}
					}
				} else {
					var resp = IsUniqueBlogFilename(theFileName, goLiveDate, pageId);
					if (resp == false) {
						for (int i = 1; i < 2500; i++) {
							testFile = string.Format("{0}-{1}", pageTitle, i);
							resp = IsUniqueBlogFilename(testFile, goLiveDate, pageId);
							if (resp) {
								theFileName = testFile;
								break;
							} else {
								theFileName = string.Empty;
							}
						}
					}
				}

				return ContentPageHelper.ScrubFilename(pageId, theFileName).ToLowerInvariant();
			} catch (Exception ex) {
				SiteData.WriteDebugException("generatenewfilename", ex);
				throw;
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

			using (var pageHelper = new ContentPageHelper()) {
				var requireActivePage = !(SecurityData.IsAdmin || SecurityData.IsSiteEditor);

				if (SiteData.IsLikelyHomePage(sCurrentPage)) {
					pageContents = pageHelper.FindHome(SiteData.CurrentSiteID, requireActivePage);
				} else {
					pageContents = pageHelper.GetLatestContentByURL(SiteData.CurrentSiteID, requireActivePage, sCurrentPage);
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

			using (var pageHelper = new ContentPageHelper()) {
				bool IsPageTemplate = false;
				string sCurrentPage = SiteData.CurrentScriptName;
				string sScrubbedURL = SiteData.AlternateCurrentScriptName;

				if (sScrubbedURL.ToLowerInvariant() != sCurrentPage.ToLowerInvariant()) {
					sCurrentPage = sScrubbedURL;
				}

				var requireActivePage = !(SecurityData.IsAdmin || SecurityData.IsSiteEditor);

				if (SiteData.IsLikelyHomePage(sCurrentPage)) {
					pageContents = pageHelper.FindHome(SiteData.CurrentSiteID, requireActivePage);
				} else {
					pageContents = pageHelper.GetLatestContentByURL(SiteData.CurrentSiteID, requireActivePage, sCurrentPage);
				}

				if (pageContents == null && SiteData.IsPageReal) {
					IsPageTemplate = true;
				}

				if ((SiteData.IsPageSampler || IsPageTemplate || !IsWebView) && pageContents == null) {
					pageContents = ContentPageHelper.GetSamplerView();
				}

				if (pageContents == null && SiteData.IsLikelyFakeSearch()) {
					pageContents = ContentPageHelper.GetEmptySearch();
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

		public static void ManuallyWriteDefaultFile(HttpContext context, Exception objErr) {
			var sbBody = new StringBuilder();
			sbBody.Append(CoreHelper.ReadEmbededScript("Carrotware.CMS.Core.SiteContent.Default.htm"));

			try {
				if (CurretSiteExists) {
					sbBody.Replace("{TIME_STAMP}", CurrentSite.Now.ToString());
				}
			} catch { }
			sbBody.Replace("{TIME_STAMP}", DateTime.Now.ToString());

			if (objErr != null) {
				sbBody.Replace("{LONG_NAME}", FormatToHTML(" [" + objErr.GetType().ToString() + "] " + objErr.Message));

				if (objErr.StackTrace != null) {
					sbBody.Replace("{STACK_TRACE}", FormatToHTML(objErr.StackTrace));
				}
				if (objErr.InnerException != null) {
					sbBody.Replace("{CONTENT_DETAIL}", FormatToHTML(objErr.InnerException.Message));
				}
			}

			sbBody.Replace("{STACK_TRACE}", "");
			sbBody.Replace("{CONTENT_DETAIL}", "");

			sbBody.Replace("{SITE_ROOT_PATH}", SiteData.AdminFolderPath);

			context.Response.ContentType = "text/html";
			context.Response.Clear();
			context.Response.BufferOutput = true;

			context.Response.Write(sbBody.ToString());
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
			var sbBody = new StringBuilder();
			sbBody.Append(CoreHelper.ReadEmbededScript("Carrotware.CMS.Core.SiteContent.ErrorFormat.htm"));

			if (objErr is HttpException) {
				HttpException httpEx = (HttpException)objErr;

				sbBody.Replace("{PAGE_TITLE}", httpEx.Message);
				sbBody.Replace("{SHORT_NAME}", httpEx.Message);
				sbBody.Replace("{LONG_NAME}", "HTTP " + httpEx.GetHttpCode() + " - " + FormatToHTML(httpEx.Message));
			} else {
				sbBody.Replace("{PAGE_TITLE}", objErr.Message);
				sbBody.Replace("{SHORT_NAME}", objErr.Message);
				sbBody.Replace("{LONG_NAME}", FormatToHTML(" [" + objErr.GetType().ToString() + "] " + objErr.Message));
			}

			if (objErr.StackTrace != null) {
				sbBody.Replace("{STACK_TRACE}", FormatToHTML(objErr.StackTrace));
			}

			if (objErr.InnerException != null) {
				sbBody.Replace("{CONTENT_DETAIL}", FormatToHTML(objErr.InnerException.Message));
			}

			if (CurretSiteExists) {
				sbBody.Replace("{TIME_STAMP}", CurrentSite.Now.ToString());
			}
			sbBody.Replace("{TIME_STAMP}", DateTime.Now.ToString());

			sbBody.Replace("{CONTENT_DETAIL}", "");
			sbBody.Replace("{STACK_TRACE}", "");

			return sbBody.ToString();
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
				return CoreHelper.ReadEmbededScript("Carrotware.CMS.Core.SiteContent.FirstPage.txt");
			}
		}

		public static SiteNav SiteBlogPage {
			get {
				if (CurrentSite == null) {
					return null;
				}
				if (CurrentSite.Blog_Root_ContentID.HasValue) {
					using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
						return navHelper.GetLatestVersion(CurrentSite.SiteID, CurrentSite.Blog_Root_ContentID.Value);
					}
				} else {
					// fake / mockup of a search page
					return SiteNavHelper.GetEmptySearch();
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

		[Display(Name = "Default - Plain L-R-C Content")]
		public static string DefaultTemplateFilename {
			get { return "~/Views/CmsContent/PlainPage/PlainPageView.cshtml".ToLowerInvariant(); }
		}

		[Display(Name = "Black 'n White - Plain L-R-C Content")]
		public static string DefaultTemplateBWFilename {
			get { return "~/Views/CmsContent/PlainPage/PlainPageBWView.cshtml".ToLowerInvariant(); }
		}

		public static List<string> DefaultTemplates {
			get {
				string[] _defaultTemplates = new string[] { DefaultTemplateFilename, DefaultTemplateBWFilename };

				return _defaultTemplates.ToList();
			}
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
					try {
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
					} catch (Exception ex) {
						SiteData.WriteDebugException("adminfolderpath", ex);
						return _defPath;
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
									if (blogNavPage == null) {
										blogNavPage = SiteNavHelper.GetEmptySearch();
									}
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

		public static string RssDocType { get { return "text/xml"; } }

		public static string RawMode { get { return "raw"; } }
		public static string HtmlMode { get { return "html"; } }

		public static string EditMode(string mode) {
			return (string.IsNullOrEmpty(mode) || mode.Trim().ToLowerInvariant() != RawMode) ? HtmlMode.ToLowerInvariant() : RawMode.ToLowerInvariant();
		}
	}
}