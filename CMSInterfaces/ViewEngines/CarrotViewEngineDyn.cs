using System;
using System.Diagnostics;
using System.Reflection;
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

	public class CarrotViewEngineDyn : CarrotViewEngineBase {

		public string AssemblyKey {
			get {
				Assembly asmbly = this.GetType().Assembly;

				string assemblyName = asmbly.ManifestModule.Name;
				assemblyName = assemblyName.Substring(0, assemblyName.Length - 4);

				return assemblyName;
			}
		}

		public CarrotViewEngineDyn()
			: base() {
			string[] views = GetPaths(ViewPathType.View);

			string[] areas = GetPaths(ViewPathType.Area);

			this.LoadPaths(views, areas);
		}

		protected override string ReplacePath(ControllerContext ctrlCtx, string viewPath) {
			string newViewPath = "~/Views/Shared/__";

#if DEBUG
			Debug.WriteLine(String.Format("CarrotViewEngineDyn: n:{0}   c:{1}   v:{2}   a:{3}", ctrlCtx.Controller.GetType().Namespace, ctrlCtx.Controller.GetType().Name, viewPath, this.AssemblyKey));
#endif

			if (!String.IsNullOrEmpty(viewPath)) {
				newViewPath = viewPath.ToLowerInvariant().Replace("::key::", this.AssemblyKey);
			}

			return newViewPath;
		}
	}
}