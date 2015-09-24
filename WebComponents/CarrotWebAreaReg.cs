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

namespace Carrotware.Web.UI.Components {

	public class CarrotWebAreaReg : AreaRegistration {
		protected string _areaName = String.Empty;
		private List<string> _namespaces = null;

		public CarrotWebAreaReg()
			: base() {
			Assembly asmbly = this.GetType().Assembly;

			_namespaces = asmbly.GetTypes().Select(t => t.Namespace)
								.Where(x => !String.IsNullOrEmpty(x))
								.Distinct().ToList();

			string assemblyName = asmbly.ManifestModule.Name;
			_areaName = assemblyName.Substring(0, assemblyName.Length - 4);
		}

		public CarrotWebAreaReg(string areaName)
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
						url: "carrotwarethumb.ashx/{id}",
						defaults: new { controller = "Home", action = "ImageThumb", id = UrlParameter.Optional },
						namespaces: _namespaces.ToArray()
					);
		}
	}
}