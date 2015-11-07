using System;
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

	public abstract class CarrotViewEngineBase : RazorViewEngine {

		protected virtual void LoadPaths(string[] partials, string[] areas) {
			this.MasterLocationFormats = partials;
			this.ViewLocationFormats = partials;
			this.PartialViewLocationFormats = partials;

			this.AreaMasterLocationFormats = areas;
			this.AreaViewLocationFormats = areas;
			this.AreaPartialViewLocationFormats = areas;
		}

		protected virtual string ReplacePath(ControllerContext controllerContext, string viewPath) {
			return "~/Views/Shared/__";
		}

		protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath) {
			string newPath = ReplacePath(controllerContext, partialPath);
			return base.CreatePartialView(controllerContext, newPath);
		}

		protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath) {
			string newPath = ReplacePath(controllerContext, viewPath);
			return base.CreateView(controllerContext, newPath, newPath);
		}

		protected override bool FileExists(ControllerContext controllerContext, string virtualPath) {
			string newPath = ReplacePath(controllerContext, virtualPath);
			return base.FileExists(controllerContext, newPath);
		}
	}
}