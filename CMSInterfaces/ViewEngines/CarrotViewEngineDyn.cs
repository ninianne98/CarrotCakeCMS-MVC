using System;
using System.Diagnostics;
using System.Reflection;
using System.Web.Mvc;

/*
* CarrotCake CMS (MVC5)
* http:/www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
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
			string[] partials = new[] {
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

			this.LoadPaths(partials, areas);
		}

		protected override string ReplacePath(ControllerContext ctrlCtx, string viewPath) {
			string newViewPath = "~/Views/Shared/__";

#if DEBUG
			Debug.WriteLine(String.Format("CarrotViewEngineDyn: n:{0}   c:{1}   v:{2}   a:{3}", ctrlCtx.Controller.GetType().Namespace, ctrlCtx.Controller.GetType().Name, viewPath, this.AssemblyKey));
#endif

			if (!String.IsNullOrEmpty(viewPath)) {
				newViewPath = viewPath.ToLower().Replace("::key::", this.AssemblyKey);
			}

			return newViewPath;
		}
	}
}