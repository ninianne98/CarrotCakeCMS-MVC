using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

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

	public class CarrotViewEngine : RazorViewEngine {

		public CarrotViewEngine()
			: base() {
			this.DateLoaded = DateTime.UtcNow;
		}

		public Guid RequestKey { get; set; }

		public string AssemblyKey { get; set; }

		public DateTime DateLoaded { get; set; }

		public CarrotViewEngine(string assemblyKey, Guid request) {
			this.AssemblyKey = assemblyKey;
			this.RequestKey = request;
			this.DateLoaded = DateTime.UtcNow;

			string[] views = new[] {
					"~/Views/" + this.AssemblyKey + "/{0}.cshtml",
                    "~/Views/" + this.AssemblyKey + "/{0}.vbhtml",
                    "~/Views/" + this.AssemblyKey + "/{1}/{0}.cshtml",
                    "~/Views/" + this.AssemblyKey + "/{1}/{0}.vbhtml",
                    "~/Views/" + this.AssemblyKey + "/Shared/{0}.cshtml",
                    "~/Views/" + this.AssemblyKey + "/Shared/{0}.vbhtml"};

			string[] areas = new[] {
					"~/Areas/" + this.AssemblyKey + "/Views/{0}.cshtml",
                    "~/Areas/" + this.AssemblyKey + "/Views/{0}.vbhtml",
                    "~/Areas/" + this.AssemblyKey + "/Views/{1}/{0}.cshtml",
                    "~/Areas/" + this.AssemblyKey + "/Views/{1}/{0}.vbhtml",
                    "~/Areas/" + this.AssemblyKey + "/Views/Shared/{0}.cshtml",
                    "~/Areas/" + this.AssemblyKey + "/Views/Shared/{0}.vbhtml"};

			this.LoadPaths(views, areas);
		}

		public void LoadPaths(string[] views, string[] areas) {
			this.MasterLocationFormats = views;
			this.ViewLocationFormats = views;
			this.PartialViewLocationFormats = views;

			this.AreaMasterLocationFormats = areas;
			this.AreaViewLocationFormats = areas;
			this.AreaPartialViewLocationFormats = areas;
		}

		public static void EngineLoad(Controller ctrl, Guid requestKey) {
			if (ctrl is IWidgetController) {
				EngineLoad(ctrl, requestKey, (ctrl as IWidgetController).AssemblyName);
			}
		}

		public static void EngineLoad(Controller ctrl, Guid requestKey, string assemblyName) {
			List<CarrotViewEngine> lst = ctrl.ViewEngineCollection
								.Where(x => x is CarrotViewEngine).Cast<CarrotViewEngine>()
								.Where(x => x.RequestKey == requestKey).ToList();

			if (!lst.Any()) {
				CarrotViewEngine ve = new CarrotViewEngine(assemblyName, requestKey);

				ctrl.ViewEngineCollection.Add(ve);
			}
		}

		public static void EngineDispose(Controller ctrl, Guid requestKey) {
			if (ctrl is IWidgetController) {
				EngineDispose(ctrl, requestKey, (ctrl as IWidgetController).AssemblyName);
			}
		}

		public static void EngineDispose(Controller ctrl, Guid requestKey, string assemblyName) {
			// only add the xtra lookup paths so long as needed to render the relative path partials from the template
			List<CarrotViewEngine> lst = (from c in ctrl.ViewEngineCollection.Where(x => x is CarrotViewEngine).Cast<CarrotViewEngine>()
										  where c.RequestKey == requestKey
												|| c.DateLoaded < DateTime.UtcNow.AddMinutes(-15)
												|| (c.AssemblyKey == assemblyName && c.DateLoaded < DateTime.UtcNow.AddMinutes(-5))
										  select c).ToList();
			if (lst.Any()) {
				for (int j = (lst.Count - 1); j >= 0; j--) {
					ctrl.ViewEngineCollection.Remove(lst[j]);
				}
			}
		}

		public static void EngineDispose(Controller ctrl) {
			// only add the xtra lookup paths so long as needed to render the relative path partials from the template
			List<CarrotViewEngine> lst = (from c in ctrl.ViewEngineCollection.Where(x => x is CarrotViewEngine).Cast<CarrotViewEngine>()
										  where c.DateLoaded < DateTime.UtcNow.AddMinutes(-30)
										  select c).ToList();
			if (lst.Any()) {
				for (int j = (lst.Count - 1); j >= 0; j--) {
					ctrl.ViewEngineCollection.Remove(lst[j]);
				}
			}
		}
	}
}