using System;
using System.Collections.Generic;
using System.Linq;
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

	public class CmsAdminAuthorizeAttribute : AuthorizeAttribute {

		protected override bool AuthorizeCore(HttpContextBase httpContext) {
			if (!SecurityData.IsAuthenticated) {
				return false;
			}

			if (SecurityData.GetIsAdminFromCache()) {
				return true;
			}

			return false;
		}

		public override void OnAuthorization(AuthorizationContext filterContext) {
			base.OnAuthorization(filterContext);

			bool skipAuth = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
								|| filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true);

			if (skipAuth) {
				return;
			} else {
				if (!filterContext.HttpContext.User.Identity.IsAuthenticated) {
					if (filterContext.HttpContext.Request.Path.ToLowerInvariant() != SiteFilename.LoginURL.ToLowerInvariant()) {
						filterContext.Result = new RedirectResult(String.Format("{0}?returnUrl={1}", SiteFilename.LoginURL, HttpUtility.UrlEncode(filterContext.HttpContext.Request.Path)));
					} else {
						filterContext.Result = new RedirectResult(SiteFilename.LoginURL);
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