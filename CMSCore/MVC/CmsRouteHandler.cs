using Carrotware.CMS.DBUpdater;
using Carrotware.Web.UI.Components;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

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

	public static class CmsRouteConstants {

		public static class CmsController {
			public static string Admin { get { return "CmsAdmin"; } }
			public static string AdminApi { get { return "CmsAdminApi"; } }
			public static string Home { get { return "Home"; } }
			public static string Content { get { return "CmsContent"; } }
			public static string AjaxForms { get { return "CmsAjaxForms"; } }
		}

		public static string IndexAction { get { return "Index"; } }
		public static string DefaultAction { get { return "Default"; } }
		public static string NotFoundAction { get { return "PageNotFound"; } }
		public static string RssAction { get { return "RSSFeed"; } }
		public static string SiteMapAction { get { return "SiteMap"; } }
	}

	//=====================

	public class CmsRouteHandler : MvcRouteHandler {
		public static string RouteKey { get { return "cmsRequestedUri"; } }
		public static string PageIdKey { get { return "cmsPageid"; } }

		protected override IHttpHandler GetHttpHandler(System.Web.Routing.RequestContext requestCtx) {
			string requestedUri = (string)requestCtx.RouteData.Values[RouteKey];

			requestedUri = string.IsNullOrEmpty(requestedUri) ? @"/" : requestedUri.ToLowerInvariant();
			requestedUri = requestedUri.FixPathSlashes();

			if (requestedUri.EndsWith(".ashx")
						|| requestedUri.ToLowerInvariant().Contains("rss.")
						|| requestedUri.ToLowerInvariant().Contains("sitemap.")) {
				if (UseDynamicFeed(SiteFilename.RssFeedUri, requestedUri)) {
					requestCtx.SetContextRoutevalues(CmsRouteConstants.CmsController.Content, CmsRouteConstants.RssAction);

					return base.GetHttpHandler(requestCtx);
				}
				if (UseDynamicFeed(SiteFilename.SiteMapUri, requestedUri)) {
					requestCtx.SetContextRoutevalues(CmsRouteConstants.CmsController.Content, CmsRouteConstants.SiteMapAction);

					return base.GetHttpHandler(requestCtx);
				}

				requestCtx.SetContextRoutevalues(CmsRouteConstants.CmsController.Content, CmsRouteConstants.NotFoundAction);

				SiteData.WriteDebugException("cmsroutehandler ashx not matched", new Exception(string.Format("RequestedUri: {0}", requestedUri)));

				return base.GetHttpHandler(requestCtx);
			} else if (requestedUri.EndsWith(".aspx")) {
				//since .aspx is not supported

				requestCtx.SetContextRoutevalues(CmsRouteConstants.CmsController.Content, CmsRouteConstants.NotFoundAction);
			} else {
				string sCurrentPage = SiteData.CurrentScriptName;

				try {
					string sScrubbedURL = SiteData.AlternateCurrentScriptName;

					if (sScrubbedURL.ToLowerInvariant() != sCurrentPage.ToLowerInvariant()) {
						requestedUri = sScrubbedURL;
					}

					SiteNav navData = null;
					bool bIsHomePage = false;
					bool bIgnorePublishState = SecurityData.AdvancedEditMode || SecurityData.IsAdmin || SecurityData.IsSiteEditor;

					using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
						if (SiteData.IsLikelyHomePage(requestedUri)) {
							navData = navHelper.FindHome(SiteData.CurrentSiteID, !bIgnorePublishState);

							if (navData != null) {
								requestedUri = navData.FileName;
								bIsHomePage = true;
							}
						}

						if (!bIsHomePage) {
							navData = navHelper.GetLatestVersion(SiteData.CurrentSiteID, !bIgnorePublishState, requestedUri);
						}

						if ((SiteData.IsLikelyHomePage(requestedUri)) && navData == null) {
							navData = SiteNavHelper.GetEmptyHome();
						}

						// use a fake search page when needed, but don't allow editing
						if (!SecurityData.AdvancedEditMode && SiteData.IsLikelyFakeSearch() && navData == null) {
							navData = SiteNavHelper.GetEmptySearch();
						}

						string actionRoute = string.Empty;
						if (navData != null) {
							SiteData.WriteDebugException("cmsroutehandler != null", new Exception(string.Format("Default: {0}", navData.FileName)));
							actionRoute = CmsRouteConstants.DefaultAction;
							requestCtx.RouteData.Values[PageIdKey] = navData.Root_ContentID;
						} else {
							SiteData.WriteDebugException("cmsroutehandler == null", new Exception(string.Format("_PageNotFound: {0}", sCurrentPage)));
							actionRoute = CmsRouteConstants.NotFoundAction;
						}

						requestCtx.SetContextRoutevalues(CmsRouteConstants.CmsController.Content, actionRoute);
					}
				} catch (Exception ex) {
					var du = new DatabaseUpdate();

					SiteData.WriteDebugException("cmsroutehandler_exception_uri", new Exception(string.Format("Exception: {0}", sCurrentPage)));

					if (ex.SystemNeedsChecking() || du.DatabaseNeedsUpdate()) {
						requestCtx.SetContextRoutevalues(CmsRouteConstants.CmsController.Content, CmsRouteConstants.DefaultAction);

						SiteData.WriteDebugException("cmsroutehandler_exception_systemneedschecking", ex);
					} else {
						//something bad has gone down, toss back the error
						SiteData.WriteDebugException("cmsroutehandler_exception", ex);
						throw;
					}
				}
			}

			return base.GetHttpHandler(requestCtx);
		}

		private bool UseDynamicFeed(string feedUri, string requestedUri) {
			var uri = feedUri.ToLowerInvariant();
			var reqUri = requestedUri.ToLowerInvariant();

			var pathMatch = reqUri == uri.ToLowerInvariant()
								|| reqUri == uri.Replace(".ashx", ".axd")
								|| reqUri == uri.Replace(".ashx", ".xml");

			// give precidence to actual xml
			if (pathMatch && reqUri.EndsWith(".xml")) {
				return File.Exists(HttpContext.Current.Server.MapPath(reqUri)) == false;
			}

			return pathMatch;
		}
	}
}