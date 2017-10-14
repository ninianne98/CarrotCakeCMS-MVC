using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;

namespace CarrotCake.CMS.Plugins.PhotoGallery {

	public class RouteConfig {

		public static void RegisterRoutes(RouteCollection routes) {
			routes.MapMvcAttributeRoutes();
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			Assembly _assembly = Assembly.GetExecutingAssembly();

			List<string> _namespaces = _assembly.GetTypes().Select(t => t.Namespace)
					.Where(x => !String.IsNullOrEmpty(x))
					.Distinct().ToList();

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
				namespaces: _namespaces.ToArray()
			);
		}
	}
}