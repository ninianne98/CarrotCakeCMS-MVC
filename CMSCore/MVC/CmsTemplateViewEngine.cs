using System;
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

namespace Carrotware.CMS.Core {

	public class CmsTemplateViewEngine : RazorViewEngine {
		public string ThemeFile { get; set; }
		public string ThemeFolder { get; set; }

		public CmsTemplateViewEngine(string fileName) {
			string folderPath = fileName.Substring(0, fileName.LastIndexOf("/"));
			string folderName = folderPath.Substring(folderPath.LastIndexOf("/") + 1);
			this.ThemeFile = fileName;
			this.ThemeFolder = folderName;

			CarrotCakeConfig config = CarrotCakeConfig.GetConfig();
			string templatePath = config.ConfigFileLocation.TemplatePath;
			if (templatePath.StartsWith("/")) {
				templatePath = templatePath.Substring(1);
			}
			if (!templatePath.EndsWith("/")) {
				templatePath = templatePath + "/";
			}

			string[] partials = new[] {
					folderPath + "/{0}.cshtml",
                    folderPath + "/{0}.vbhtml",
					folderPath + "/Shared/{0}.cshtml",
                    folderPath + "/Shared/{0}.vbhtml",
					templatePath + this.ThemeFolder + "/{0}.cshtml",
                    templatePath + this.ThemeFolder + "/{0}.vbhtml",
                    templatePath + this.ThemeFolder + "/Shared/{0}.cshtml",
                    templatePath + this.ThemeFolder + "/Shared/{0}.vbhtml"};

			string[] areas = new[] { String.Empty };

			this.LoadPaths(partials, areas);
		}

		public CmsTemplateViewEngine(string key, string[] partials, string[] areas) {
			this.ThemeFolder = key;
			this.LoadPaths(partials, areas);
		}

		public void LoadPaths(string[] partials, string[] areas) {
			this.MasterLocationFormats = partials;
			this.ViewLocationFormats = partials;
			this.PartialViewLocationFormats = partials;

			this.AreaMasterLocationFormats = areas;
			this.AreaPartialViewLocationFormats = areas;
			this.AreaViewLocationFormats = areas;
		}
	}
}