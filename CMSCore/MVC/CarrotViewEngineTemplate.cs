using Carrotware.CMS.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
		protected string _filePath = "::filepath::";
		protected string _themePath = "::themepath::";

		public CarrotViewEngineTemplate()
			: base() {
			string[] viewBase = new[] { "/{0}.cshtml",
										"/{0}.vbhtml",
										"/Shared/{0}.cshtml",
										"/Shared/{0}.vbhtml" };

			string[] views = viewBase.Select(v => _filePath + v)
									.Union(viewBase.Select(v => _themePath + v))
									.ToArray();

			string[] areas = new[] { string.Empty };

			this.LoadPaths(views, areas);
		}

		protected string GetTemplateFile(ControllerContext ctrlCtx) {
			ControllerBase controller = ctrlCtx.Controller;
			string templateFile = string.Empty;

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

			if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(viewPath)) {
				string folderPath = Path.GetDirectoryName(fileName);
				string folderName = folderPath.Substring(folderPath.LastIndexOf("/") + 1);

				CarrotCakeConfig config = CarrotCakeConfig.GetConfig();
				string templatePath = config.ConfigFileLocation.TemplatePath;
				if (templatePath.StartsWith("/")) {
					templatePath = templatePath.Substring(1);
				}
				if (!templatePath.EndsWith("/")) {
					templatePath = templatePath + "/";
				}

				string themePath = string.Format("{0}{1}", templatePath, folderName).ToLowerInvariant();

#if DEBUG
				Debug.WriteLine(string.Format("CarrotViewEngineTemplate: n:{0}   c:{1}   v:{2}   f:{3}   t:{4}", ctrlCtx.Controller.GetType().Namespace, ctrlCtx.Controller.GetType().Name, viewPath, folderPath, themePath));
#endif
				newViewPath = viewPath.ToLowerInvariant()
								.Replace(_filePath, folderPath)
								.Replace(_themePath, themePath);
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