using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace Carrotware.CMS.Interface.Controllers {

	public class BaseWidgetController : Controller, IWidgetController {

		public BaseWidgetController()
			: base() {
			Assembly asmbly = this.GetType().Assembly;

			string assemblyName = asmbly.ManifestModule.Name;
			assemblyName = assemblyName.Substring(0, assemblyName.Length - 4);

			this.ViewData[CarrotViewEngineWidget.Key] = assemblyName;
			this.AssemblyName = assemblyName;
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
		}

		public string AssemblyName { get; set; }
	}
}