using System;
using System.Configuration;
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

namespace Carrotware.CMS.Interface.Controllers {

	public class BaseWidgetController : Controller, IWidgetController {

		public string TestSiteID {
			get {
				return ConfigurationManager.AppSettings["TestSiteID"] != null
					? ConfigurationManager.AppSettings["TestSiteID"].ToString()
					: Guid.Empty.ToString();
			}
		}

		public BaseWidgetController()
			: base() {
			Assembly a = this.GetType().Assembly;

			string assemblyName = a.GetAssemblyName();

			this.ViewData[CarrotViewEngineWidget.Key] = assemblyName;
			this.AssemblyName = assemblyName;

			ViewBag.WidgetAssemblyName = assemblyName;
			ViewBag.WidgetAreaAlias = string.Format("{0}_Default", assemblyName);
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
		}

		public string AssemblyName { get; set; }
	}
}