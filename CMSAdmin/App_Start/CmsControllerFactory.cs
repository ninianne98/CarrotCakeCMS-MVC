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
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin {

	public class CmsControllerFactory : DefaultControllerFactory {
		private Dictionary<string, Func<RequestContext, IController>> controllers;

		public CmsControllerFactory() {
			controllers = new Dictionary<string, Func<RequestContext, IController>>();
		}

		public CmsControllerFactory(IAdminModule moduleData)
			: base() { }

		public override IController CreateController(RequestContext requestContext, string controllerName) {
			if (controllers != null && controllers.ContainsKey(controllerName)) {
				return controllers[controllerName](requestContext);
			} else {
				IController ctrl = base.CreateController(requestContext, controllerName);

				if (ctrl is IAdminModule) {
					var principal = requestContext.HttpContext.User;

					if (principal.Identity.IsAuthenticated) {
						if (ctrl is IAdminModule) {
							IAdminModule m = ((IAdminModule)ctrl);
							m.SiteID = SiteData.CurrentSiteID;
							m.ModuleID = Guid.Empty;
						}
					} else {
						ctrl = new CmsAdminController();
					}
				}

				if (ctrl is IWidget) {
					IWidget w = (IWidget)ctrl;
					w.SiteID = SiteData.CurrentSiteID;
				}

				return ctrl;
			}
		}

		public static IControllerFactory GetFactory() {
			return new CmsControllerFactory();
		}
	}
}