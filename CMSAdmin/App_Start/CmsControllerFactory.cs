using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
using Carrotware.CMS.Mvc.UI.Admin.Controllers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin {

	public class CmsControllerFactory : DefaultControllerFactory {

		public CmsControllerFactory()
			: base() {
		}

		public override IController CreateController(RequestContext requestContext, string controllerName) {
			IController ctrl = base.CreateController(requestContext, controllerName);

			if (ctrl is IAdminModule) {
				var principal = requestContext.HttpContext.User;

				if (principal.Identity.IsAuthenticated) {
					if (ctrl is IAdminModule) {
						IAdminModule m = (ctrl as IAdminModule);
						m.SiteID = SiteData.CurrentSiteID;
						m.ModuleID = Guid.Empty;
					}
				} else {
					SiteData.WriteDebugException("cmscontrollerfactory_createcontroller", new Exception(String.Format("Anonymous: {0} - {1}", ctrl.GetType(), controllerName)));

					requestContext.RouteData.Values["action"] = "Index";
					requestContext.RouteData.Values["id"] = null;

					ctrl = new CmsAdminController();
				}
			}

			if (ctrl is IWidget) {
				IWidget w = ctrl as IWidget;
				w.SiteID = SiteData.CurrentSiteID;
			}

			return ctrl;
		}

		public static IControllerFactory GetFactory() {
			return new CmsControllerFactory();
		}
	}
}