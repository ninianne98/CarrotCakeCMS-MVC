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

	public class CarrotViewEngineWidget : CarrotViewEngineBase {

		public CarrotViewEngineWidget()
			: base() {
			string[] views = GetPaths(ViewPathType.View);

			string[] areas = GetPaths(ViewPathType.Area);

			this.LoadPaths(views, areas);
		}

		public static string Key { get { return "CMS_Widget_AssemblyKey"; } }

		public string GetAssemblyKey(ControllerContext ctrlCtx) {
			string assemblyName = String.Empty;
			var controller = ctrlCtx.Controller;

			//if (!(controller is IAdminModule)) {
			if (controller is IWidgetController) {
				assemblyName = (controller as IWidgetController).AssemblyName;
			} else {
				if (controller.ViewData[CarrotViewEngineWidget.Key] != null) {
					assemblyName = controller.ViewData[CarrotViewEngineWidget.Key].ToString();
				}
			}
			//}

			return assemblyName;
		}

		protected override string ReplacePath(ControllerContext ctrlCtx, string viewPath) {
			string newViewPath = "~/Views/Shared/__";
			string assemblyName = GetAssemblyKey(ctrlCtx);

#if DEBUG
			Debug.WriteLine(String.Format("CarrotViewEngineWidget: n:{0}   c:{1}   v:{2}   a:{3}", ctrlCtx.Controller.GetType().Namespace, ctrlCtx.Controller.GetType().Name, viewPath, assemblyName));
#endif

			if (!String.IsNullOrEmpty(viewPath) && !String.IsNullOrEmpty(assemblyName)) {
				newViewPath = viewPath.ToLowerInvariant().Replace("::key::", assemblyName);
			}

			return newViewPath;
		}
	}
}