using Carrotware.Web.UI.Components;
using System.Net;
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

	public class CmsAuthorizeAttribute : AuthorizeAttribute {

		protected override bool AuthorizeCore(HttpContextBase httpContext) {
			if (!SecurityData.IsAuthenticated) {
				return false;
			}

			if (SecurityData.GetIsAdminFromCache() || SecurityData.GetIsSiteEditorFromCache()) {
				return true;
			}

			return false;
		}

		public override void OnAuthorization(AuthorizationContext filterContext) {
			base.OnAuthorization(filterContext);

			var routeInfo = filterContext.RouteData.GetRouteInfo();
			var isApi = CmsRouteConstants.CmsController.AdminApi.ToLowerInvariant() == routeInfo.Controller.ToLowerInvariant();

			bool skipAuth = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
								|| filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true);

			if (skipAuth) {
				return;
			} else {
				if (!filterContext.HttpContext.User.Identity.IsAuthenticated) {
					if (isApi) {
						filterContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
						filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
						filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
						filterContext.HttpContext.Response.StatusDescription = HttpStatusCode.Forbidden.ToString();
						filterContext.Result = new HttpUnauthorizedResult();
					} else {
						if (filterContext.HttpContext.Request.Path.ToLowerInvariant() != SiteFilename.LoginURL.ToLowerInvariant()) {
							filterContext.Result = new RedirectResult(string.Format("{0}?returnUrl={1}", SiteFilename.LoginURL, HttpUtility.UrlEncode(filterContext.HttpContext.Request.Path)));
						} else {
							filterContext.Result = new RedirectResult(SiteFilename.LoginURL);
						}
					}
					return;
				}
			}

			if (filterContext.Result is HttpUnauthorizedResult) {
				filterContext.Result = new RedirectResult(SiteFilename.NotAuthorizedURL);
			}
		}
	}
}