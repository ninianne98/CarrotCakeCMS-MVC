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
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Interface {

	public class BaseWidgetAreaReg : AreaRegistration {
		protected string _areaName = String.Empty;
		protected List<string> _namespaces = new List<string>();

		public BaseWidgetAreaReg()
			: base() {
			Assembly asmbly = this.GetType().Assembly;

			_namespaces = asmbly.GetTypes().Select(t => t.Namespace)
								.Where(x => !String.IsNullOrEmpty(x))
								.Distinct().ToList();

			string assemblyName = asmbly.ManifestModule.Name;
			_areaName = assemblyName.Substring(0, assemblyName.Length - 4);
		}

		public BaseWidgetAreaReg(string areaName)
			: base() {
			_areaName = areaName;
		}

		public override string AreaName {
			get {
				return _areaName;
			}
		}

		public override void RegisterArea(AreaRegistrationContext context) {
			context.MapRoute(
						name: this.AreaName + "_Default",
						url: this.AreaName + "/{controller}/{action}/{id}",
						defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
						namespaces: _namespaces.ToArray()
					);
		}
	}
}