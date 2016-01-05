using Carrotware.CMS.DBUpdater;
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

	public class CmsRouteHandler : MvcRouteHandler {
		public const string ContentCtrlr = "CmsContent";

		protected override IHttpHandler GetHttpHandler(System.Web.Routing.RequestContext requestCtx) {
			string requestedUri = (string)requestCtx.RouteData.Values["RequestedUri"];

			requestedUri = String.IsNullOrEmpty(requestedUri) ? String.Empty : requestedUri.ToLower();
			if (!requestedUri.StartsWith("/")) {
				requestedUri = String.Format("/{0}", requestedUri);
			}
			if (requestedUri.EndsWith("/")) {
				requestedUri = requestedUri.Substring(0, requestedUri.Length - 1);
			}

			if (requestedUri.EndsWith(".ashx")) {
				if (requestedUri == "/rss.ashx") {
					requestCtx.RouteData.Values["controller"] = ContentCtrlr;
					requestCtx.RouteData.Values["action"] = "RSSFeed";
					return base.GetHttpHandler(requestCtx);
				}
				if (requestedUri == "/sitemap.ashx") {
					requestCtx.RouteData.Values["controller"] = ContentCtrlr;
					requestCtx.RouteData.Values["action"] = "SiteMap";
					return base.GetHttpHandler(requestCtx);
				}

				//if (requestedUri == "/trackback.ashx") {	// will be dead link
				//	requestCtx.RouteData.Values["controller"] = ContentCtrlr;
				//	requestCtx.RouteData.Values["action"] = "Trackback";
				//	return base.GetHttpHandler(requestCtx);
				//}

				requestCtx.RouteData.Values["controller"] = "Carrotware.CMS.Core.CmsHome";
				requestCtx.RouteData.Values["action"] = "Default_ashx";

				SiteData.WriteDebugException("cmsroutehandler ashx not matched", new Exception(String.Format("RequestedUri: {0}", requestedUri)));

				return base.GetHttpHandler(requestCtx);
			} else if (requestedUri.EndsWith(".aspx")) {
				//since .aspx is not supported
				requestCtx.RouteData.Values["controller"] = ContentCtrlr;
				requestCtx.RouteData.Values["action"] = "PageNotFound";
				requestCtx.RouteData.Values["id"] = null;
			} else {
				string sCurrentPage = SiteData.CurrentScriptName;

				try {
					string sScrubbedURL = SiteData.AlternateCurrentScriptName;

					if (sScrubbedURL.ToLower() != sCurrentPage.ToLower()) {
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

						requestCtx.RouteData.Values["controller"] = ContentCtrlr;
						if (navData != null) {
							SiteData.WriteDebugException("cmsroutehandler != null", new Exception(String.Format("Default: {0}", navData.FileName)));
							requestCtx.RouteData.Values["action"] = "Default";
						} else {
							SiteData.WriteDebugException("cmsroutehandler == null", new Exception(String.Format("_PageNotFound: {0}", sCurrentPage)));
							requestCtx.RouteData.Values["action"] = "PageNotFound";
						}
						requestCtx.RouteData.Values["id"] = null;
					}
				} catch (Exception ex) {
					SiteData.WriteDebugException("cmsroutehandler_exception_uri", new Exception(String.Format("Exception: {0}", sCurrentPage)));

					if (DatabaseUpdate.SystemNeedsChecking(ex) || DatabaseUpdate.AreCMSTablesIncomplete()) {
						requestCtx.RouteData.Values["controller"] = ContentCtrlr;
						requestCtx.RouteData.Values["action"] = "Default";
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