using Carrotware.CMS.Core;
using Carrotware.CMS.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

	public class RouteConfig {
		public const string AdminCtrlr = "CmsAdmin";

		public static void RegisterRoutes(RouteCollection routes) {
			routes.MapMvcAttributeRoutes();
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			Assembly _assembly = Assembly.GetExecutingAssembly();

			List<string> _namespaces = _assembly.GetTypes().Select(t => t.Namespace)
					.Where(x => !String.IsNullOrEmpty(x))
					.Distinct().ToList();

			string adminFolder = SiteData.AdminFolderPath;
			if (adminFolder.StartsWith("/")) {
				adminFolder = adminFolder.Substring(1);
			}
			if (adminFolder.EndsWith("/")) {
				adminFolder = adminFolder.Substring(0, adminFolder.Length - 1);
			}

			//routes.MapRoute(
			//	name: "C3_Admin_Module",
			//	url: adminFolder + "/Module/{pi}/{pf}/{id}", // plugin guid, plugin key, id
			//	defaults: new { controller = AdminCtrlr, action = "Module", id = UrlParameter.Optional },
			//	namespaces: _namespaces.ToArray()
			//);

			routes.MapRoute(
				name: "C3_Admin_Default",
				url: adminFolder + "/{action}/{id}",
				defaults: new { controller = AdminCtrlr, action = "Index", id = UrlParameter.Optional },
				namespaces: _namespaces.ToArray()
			);

			CarrotSecurityConfig config = CarrotSecurityConfig.GetConfig();
			string loginPath = config.AdditionalSettings.LoginPath;
			if (loginPath.ToLowerInvariant() != SiteFilename.LoginURL.ToLowerInvariant()) {
				if (loginPath.StartsWith("/")) {
					loginPath = loginPath.Substring(1);
				}

				routes.MapRoute(
					name: "C3_Admin_Login",
					url: loginPath + "/{id}",
					defaults: new { controller = AdminCtrlr, action = "Login", id = UrlParameter.Optional },
					namespaces: _namespaces.ToArray()
				);
			}

			routes.MapRoute(
				name: "CmsContent_AjaxForms",
				url: "CmsAjaxForms/{action}.ashx",
				defaults: new { controller = CmsRouteHandler.ContentCtrlr, action = "Index", id = UrlParameter.Optional },
				namespaces: _namespaces.ToArray()
			);

			routes.MapRoute(
				name: "CMS_Content_Default",
				url: "{*RequestedUri}").RouteHandler = new CmsRouteHandler();

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
				namespaces: _namespaces.ToArray()
			);
		}
	}
}