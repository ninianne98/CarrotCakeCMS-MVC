using Carrotware.CMS.Interface;
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

namespace Carrotware.CMS.Core {

	public class CarrotViewEngineTemplate : CarrotViewEngineBase {

		public CarrotViewEngineTemplate()
			: base() {
			string[] views = new[] {
					"::FILEPATH::/{0}.cshtml",
					"::FILEPATH::/{0}.vbhtml",
					"::FILEPATH::/Shared/{0}.cshtml",
					"::FILEPATH::/Shared/{0}.vbhtml",
					"::THEMEPATH::/{0}.cshtml",
					"::THEMEPATH::/{0}.vbhtml",
					"::THEMEPATH::/Shared/{0}.cshtml",
					"::THEMEPATH::/Shared/{0}.vbhtml"};

			string[] areas = new[] { String.Empty };

			this.LoadPaths(views, areas);
		}

		protected string GetTemplateFile(ControllerContext ctrlCtx) {
			ControllerBase controller = ctrlCtx.Controller;
			string templateFile = String.Empty;

			if (controller is IContentController) {
				templateFile = (controller as IContentController).TemplateFile;
			} else {
				if (controller.ViewData[PagePayload.ViewDataKey] != null) {
					PagePayload page = (PagePayload)controller.ViewData[PagePayload.ViewDataKey];
					templateFile = page.ThePage.TemplateFile;
				}
			}

			return templateFile;
		}

		protected override string ReplacePath(ControllerContext ctrlCtx, string viewPath) {
			string newViewPath = "~/Views/__";
			string fileName = GetTemplateFile(ctrlCtx);

			if (!String.IsNullOrEmpty(fileName) && !String.IsNullOrEmpty(viewPath)) {
				string folderPath = fileName.Substring(0, fileName.LastIndexOf("/")).ToLower();
				string folderName = folderPath.Substring(folderPath.LastIndexOf("/") + 1);

				CarrotCakeConfig config = CarrotCakeConfig.GetConfig();
				string templatePath = config.ConfigFileLocation.TemplatePath;
				if (templatePath.StartsWith("/")) {
					templatePath = templatePath.Substring(1);
				}
				if (!templatePath.EndsWith("/")) {
					templatePath = templatePath + "/";
				}

				string themePath = String.Format("{0}{1}", templatePath, folderName).ToLower();

#if DEBUG
				Debug.WriteLine(String.Format("CarrotViewEngineTemplate: n:{0}   c:{1}   v:{2}   f:{3}   t:{4}", ctrlCtx.Controller.GetType().Namespace, ctrlCtx.Controller.GetType().Name, viewPath, folderPath, themePath));
#endif

				newViewPath = viewPath.ToLower().Replace("::filepath::", folderPath).Replace("::themepath::", themePath);
			}

			return newViewPath;
		}

		public static void RegisterCmsViewEngines() {
			// since these all go together, register at the same time
			ViewEngines.Engines.Add(new CarrotViewEngineTemplate());
			ViewEngines.Engines.Add(new CarrotViewEngineWidget());
			// ViewEngines.Engines.Add(new CarrotViewEngineWidgetAdmin());
		}
	}
}