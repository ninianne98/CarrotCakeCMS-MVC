using Carrotware.CMS.DBUpdater;
using Carrotware.Web.UI.Components;
using System;
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

		protected override IHttpHandler GetHttpHandler(System.Web.Routing.RequestContext requestCtx) {
			string requestedUri = (string)requestCtx.RouteData.Values["RequestedUri"];

			requestedUri = string.IsNullOrEmpty(requestedUri) ? @"/" : requestedUri.ToLowerInvariant();
			requestedUri = requestedUri.FixPathSlashes();

			if (requestedUri.EndsWith(".ashx")) {
				if (requestedUri == SiteFilename.RssFeedUri) {
					requestCtx.RouteData.Values["controller"] = CmsRouteConstants.CmsController.Content;
					requestCtx.RouteData.Values["action"] = CmsRouteConstants.RssAction;
					return base.GetHttpHandler(requestCtx);
				}
				if (requestedUri == SiteFilename.SiteMapUri) {
					requestCtx.RouteData.Values["controller"] = CmsRouteConstants.CmsController.Content;
					requestCtx.RouteData.Values["action"] = CmsRouteConstants.SiteMapAction;
					return base.GetHttpHandler(requestCtx);
				}

				requestCtx.RouteData.Values["controller"] = CmsRouteConstants.CmsController.Content;
				requestCtx.RouteData.Values["action"] = CmsRouteConstants.NotFoundAction;
				requestCtx.RouteData.Values["id"] = null;

				SiteData.WriteDebugException("cmsroutehandler ashx not matched", new Exception(string.Format("RequestedUri: {0}", requestedUri)));

				return base.GetHttpHandler(requestCtx);
			} else if (requestedUri.EndsWith(".aspx")) {
				//since .aspx is not supported
				requestCtx.RouteData.Values["controller"] = CmsRouteConstants.CmsController.Content;
				requestCtx.RouteData.Values["action"] = CmsRouteConstants.NotFoundAction;
				requestCtx.RouteData.Values["id"] = null;
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

						requestCtx.RouteData.Values["controller"] = CmsRouteConstants.CmsController.Content;
						if (navData != null) {
							SiteData.WriteDebugException("cmsroutehandler != null", new Exception(string.Format("Default: {0}", navData.FileName)));
							requestCtx.RouteData.Values["action"] = CmsRouteConstants.DefaultAction;
						} else {
							SiteData.WriteDebugException("cmsroutehandler == null", new Exception(string.Format("_PageNotFound: {0}", sCurrentPage)));
							requestCtx.RouteData.Values["action"] = CmsRouteConstants.NotFoundAction;
						}
						requestCtx.RouteData.Values["id"] = null;
					}
				} catch (Exception ex) {
					SiteData.WriteDebugException("cmsroutehandler_exception_uri", new Exception(string.Format("Exception: {0}", sCurrentPage)));

					if (DatabaseUpdate.SystemNeedsChecking(ex) || DatabaseUpdate.AreCMSTablesIncomplete()) {
						requestCtx.RouteData.Values["controller"] = CmsRouteConstants.CmsController.Content;
						requestCtx.RouteData.Values["action"] = CmsRouteConstants.DefaultAction;
						requestCtx.RouteData.Values["id"] = null;
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
	}
}