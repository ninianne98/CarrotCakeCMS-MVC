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
			string[] views = GetPaths(ViewPathType.View);

			string[] areas = GetPaths(ViewPathType.Area);

			this.LoadPaths(views, areas);
		}

		public string GetAssemblyKey(ControllerContext ctrlCtx) {
			string assemblyName = string.Empty;
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
			Debug.WriteLine(string.Format("CarrotViewEngineWidgetAdmin: n:{0}   c:{1}   v:{2}   a:{3}", ctrlCtx.Controller.GetType().Namespace, ctrlCtx.Controller.GetType().Name, viewPath, assemblyName));
#endif

			if (!string.IsNullOrEmpty(viewPath) && !string.IsNullOrEmpty(assemblyName)) {
				newViewPath = viewPath.ToLowerInvariant().Replace(AreaRouteKey, assemblyName);
			}

			return newViewPath;
		}
	}
}