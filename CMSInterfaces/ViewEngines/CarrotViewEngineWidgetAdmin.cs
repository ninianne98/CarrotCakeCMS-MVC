using System;
using System.Diagnostics;
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

namespace Carrotware.CMS.Interface {

	public class CarrotViewEngineWidgetAdmin : CarrotViewEngineBase {

		public CarrotViewEngineWidgetAdmin()
			: base() {
			string[] views = new[] {
					"~/Views/::KEY::/{0}.cshtml",
                    "~/Views/::KEY::/{0}.vbhtml",
                    "~/Views/::KEY::/{1}/{0}.cshtml",
                    "~/Views/::KEY::/{1}/{0}.vbhtml",
                    "~/Views/::KEY::/Shared/{0}.cshtml",
                    "~/Views/::KEY::/Shared/{0}.vbhtml"};

			string[] areas = new[] {
					"~/Areas/::KEY::/Views/{0}.cshtml",
                    "~/Areas/::KEY::/Views/{0}.vbhtml",
                    "~/Areas/::KEY::/Views/{1}/{0}.cshtml",
                    "~/Areas/::KEY::/Views/{1}/{0}.vbhtml",
                    "~/Areas/::KEY::/Views/Shared/{0}.cshtml",
                    "~/Areas/::KEY::/Views/Shared/{0}.vbhtml"};

			this.LoadPaths(views, areas);
		}

		public string GetAssemblyKey(ControllerContext ctrlCtx) {
			string assemblyName = String.Empty;
			var controller = ctrlCtx.Controller;

			if (controller is IAdminModule && controller is IWidgetController) {
				assemblyName = (controller as IWidgetController).AssemblyName;
			}

			return assemblyName;
		}

		protected override string ReplacePath(ControllerContext ctrlCtx, string viewPath) {
			string newViewPath = "~/Views/Shared/__";
			string assemblyName = GetAssemblyKey(ctrlCtx);

#if DEBUG
			Debug.WriteLine(String.Format("CarrotViewEngineWidgetAdmin: n:{0}   c:{1}   v:{2}   a:{3}", ctrlCtx.Controller.GetType().Namespace, ctrlCtx.Controller.GetType().Name, viewPath, assemblyName));
#endif

			if (!String.IsNullOrEmpty(viewPath) && !String.IsNullOrEmpty(assemblyName)) {
				newViewPath = viewPath.ToLower().Replace("::key::", assemblyName);
			}

			return newViewPath;
		}
	}
}