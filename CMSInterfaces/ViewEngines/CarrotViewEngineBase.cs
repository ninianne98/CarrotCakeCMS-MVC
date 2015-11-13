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

	public abstract class CarrotViewEngineBase : RazorViewEngine {

		protected virtual void LoadPaths(string[] views, string[] areas) {
			this.MasterLocationFormats = views;
			this.ViewLocationFormats = views;
			this.PartialViewLocationFormats = views;

			this.AreaMasterLocationFormats = areas;
			this.AreaViewLocationFormats = areas;
			this.AreaPartialViewLocationFormats = areas;
		}

		protected virtual string ReplacePath(ControllerContext controllerContext, string viewPath) {
			return "~/Views/Shared/__";
		}

		protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath) {
			string newPath = ReplacePath(controllerContext, partialPath);

#if DEBUG
			Debug.WriteLine(String.Format("CarrotViewEngineBase.CreatePartialView: n:{0}   c:{1}   pp:{2}   np:{3} ", controllerContext.Controller.GetType().Namespace, controllerContext.Controller.GetType().Name, partialPath, newPath));
#endif

			return base.CreatePartialView(controllerContext, newPath);
		}

		protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath) {
			string newPath = ReplacePath(controllerContext, viewPath);

			string newMaster = String.Empty;
			if (!String.IsNullOrEmpty(masterPath)) {
				newMaster = ReplacePath(controllerContext, masterPath);
			}

#if DEBUG
			Debug.WriteLine(String.Format("CarrotViewEngineBase.CreateView: n:{0}   c:{1}   pp:{2}   np:{3}   mp:{3}", controllerContext.Controller.GetType().Namespace, controllerContext.Controller.GetType().Name, viewPath, newPath, masterPath));
#endif

			return base.CreateView(controllerContext, newPath, newMaster);
		}

		protected override bool FileExists(ControllerContext controllerContext, string virtualPath) {
			string newPath = ReplacePath(controllerContext, virtualPath);

#if DEBUG
			Debug.WriteLine(String.Format("CarrotViewEngineBase.FileExists: n:{0}   c:{1}   vp:{2}   np:{3} ", controllerContext.Controller.GetType().Namespace, controllerContext.Controller.GetType().Name, virtualPath, newPath));
#endif
			return base.FileExists(controllerContext, newPath);
		}
	}
}