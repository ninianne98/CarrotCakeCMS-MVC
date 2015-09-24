using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace Carrotware.CMS.Interface {

	public class CmsTestControllerFactory : DefaultControllerFactory {
		private Dictionary<string, Func<RequestContext, IController>> controllers;

		public string TestSiteID {
			get {
				return ConfigurationManager.AppSettings["TestSiteID"] != null
					? ConfigurationManager.AppSettings["TestSiteID"].ToString()
					: Guid.NewGuid().ToString();
			}
		}

		public CmsTestControllerFactory() {
			controllers = new Dictionary<string, Func<RequestContext, IController>>();
		}

		public override IController CreateController(RequestContext requestContext, string controllerName) {
			if (controllers != null && controllers.ContainsKey(controllerName)) {
				return controllers[controllerName](requestContext);
			} else {
				IController ctrl = base.CreateController(requestContext, controllerName);

				if (ctrl is IAdminModule || ctrl is IAdminModuleLoad) {
					if (ctrl is IAdminModule) {
						IAdminModule m = ((IAdminModule)ctrl);
						m.SiteID = new Guid(this.TestSiteID);
						m.ModuleID = Guid.Empty;
					}

					if (ctrl is IAdminModuleLoad) {
						IAdminModule data = new AdminModuleData();
						data.SiteID = new Guid(this.TestSiteID);
						data.ModuleID = Guid.Empty;

						((IAdminModuleLoad)ctrl).LoadData(data);
					}
				}

				if (ctrl is IWidget) {
					IWidget w = (IWidget)ctrl;
					w.SiteID = new Guid(this.TestSiteID);
				}

				return ctrl;
			}
		}

		public static IControllerFactory GetFactory() {
			return new CmsTestControllerFactory();
		}
	}
}