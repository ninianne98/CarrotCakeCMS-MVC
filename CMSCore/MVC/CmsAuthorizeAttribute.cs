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

			bool skipAuth = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
								|| filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true);

			if (skipAuth) {
				return;
			} else {
				if (!filterContext.HttpContext.User.Identity.IsAuthenticated) {
					filterContext.Result = new RedirectResult(SiteFilename.LoginURL);
					return;
				}
			}

			if (filterContext.Result is HttpUnauthorizedResult) {
				filterContext.Result = new RedirectResult(SiteFilename.NotAuthorizedURL);
			}
		}
	}
}