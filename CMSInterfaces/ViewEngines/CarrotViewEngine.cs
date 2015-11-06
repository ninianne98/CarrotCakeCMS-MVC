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

namespace Carrotware.CMS.Interface {

	public class CarrotViewEngine : RazorViewEngine {
		public Guid RequestKey { get; set; }

		public string AssemblyKey { get; set; }

		public CarrotViewEngine(string assemblyKey, Guid request) {
			this.AssemblyKey = assemblyKey;
			this.RequestKey = request;

			string[] partials = new[] {
					"~/Views/" + this.AssemblyKey + "/{0}.cshtml",
                    "~/Views/" + this.AssemblyKey + "/{0}.vbhtml",
                    "~/Views/" + this.AssemblyKey + "/{1}/{0}.cshtml",
                    "~/Views/" + this.AssemblyKey + "/{1}/{0}.vbhtml",
                    "~/Views/" + this.AssemblyKey + "/Shared/{0}.cshtml",
                    "~/Views/" + this.AssemblyKey + "/Shared/{0}.vbhtml"};

			string[] areas = new[] {
					"~/Areas/" + this.AssemblyKey + "/Views/{0}.cshtml",
                    "~/Areas/" + this.AssemblyKey + "/Views/{0}.vbhtml",
                    "~/Areas/" + this.AssemblyKey + "/Views/{1}/{0}.cshtml",
                    "~/Areas/" + this.AssemblyKey + "/Views/{1}/{0}.vbhtml",
                    "~/Areas/" + this.AssemblyKey + "/Views/Shared/{0}.cshtml",
                    "~/Areas/" + this.AssemblyKey + "/Views/Shared/{0}.vbhtml"};

			this.LoadPaths(partials, areas);
		}

		public void LoadPaths(string[] partials, string[] areas) {
			this.MasterLocationFormats = partials;
			this.ViewLocationFormats = partials;
			this.PartialViewLocationFormats = partials;

			this.AreaMasterLocationFormats = areas;
			this.AreaViewLocationFormats = areas;
			this.AreaPartialViewLocationFormats = areas;
		}
	}
}