﻿using Carrotware.CMS.Core;
using Carrotware.CMS.Security;
using Carrotware.Web.UI.Components;
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

		public static void RegisterRoutes(RouteCollection routes) {
			routes.MapMvcAttributeRoutes();
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			Assembly _assembly = Assembly.GetExecutingAssembly();

			List<string> _namespaces = _assembly.GetTypes().Select(t => t.Namespace)
					.Where(x => !string.IsNullOrEmpty(x))
					.Distinct().ToList();

			string adminFolder = SiteData.AdminFolderPath.TrimPathSlashes();

			routes.MapRoute(
				name: "C3_Admin_Default",
				url: adminFolder + "/{action}/{id}",
				defaults: new { controller = CmsRouteConstants.CmsController.Admin, action = SiteActions.Index, id = UrlParameter.Optional },
				namespaces: _namespaces.ToArray()
			);

			CarrotSecurityConfig config = CarrotSecurityConfig.GetConfig();
			string loginPath = config.AdditionalSettings.LoginPath;
			if (loginPath.ToLowerInvariant() != SiteFilename.LoginURL.ToLowerInvariant()) {
				loginPath = loginPath.TrimPathSlashes();

				routes.MapRoute(
					name: "C3_Admin_Login",
					url: loginPath + "/{id}",
					defaults: new { controller = CmsRouteConstants.CmsController.Admin, action = SiteActions.Login, id = UrlParameter.Optional },
					namespaces: _namespaces.ToArray()
				);
			}

			routes.MapRoute(
				name: "CmsContent_AjaxForms",
				url: "CmsAjaxForms/{action}.ashx",
				defaults: new { controller = CmsRouteConstants.CmsController.Content, action = CmsRouteConstants.IndexAction, id = UrlParameter.Optional },
				namespaces: _namespaces.ToArray()
			);

			routes.MapRoute(
				name: "CMS_Content_Default",
				url: "{*RequestedUri}").RouteHandler = new CmsRouteHandler();

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = CmsRouteConstants.CmsController.Home, action = CmsRouteConstants.IndexAction, id = UrlParameter.Optional },
				namespaces: _namespaces.ToArray()
			);
		}
	}
}