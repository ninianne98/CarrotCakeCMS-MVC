using Carrotware.CMS.Interface;
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

namespace Carrotware.CMS.UI.Components {

	public class CarrotCakeAreaReg : AreaRegistration {
		protected string _areaName = string.Empty;
		private List<string> _namespaces = null;

		public CarrotCakeAreaReg()
			: base() {
			Assembly asmbly = this.GetType().Assembly;

			_namespaces = asmbly.GetTypes().Select(t => t.Namespace)
								.Where(x => !string.IsNullOrEmpty(x))
								.Distinct().ToList();

			_areaName = asmbly.GetAssemblyName();
		}

		public CarrotCakeAreaReg(string areaName)
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
					name: this.AreaName + "_GetNavigationCss",
					url: TwoLevelNavigation.NavigationStylePath.Replace("/", "") + "/{id}",
					defaults: new { controller = "Home", action = "GetNavigationCss", id = UrlParameter.Optional },
					namespaces: _namespaces.ToArray());

			context.MapRoute(
				name: this.AreaName + "_GetAdminScriptValues",
				url: CarrotCakeHtml.AdminScriptValues.Replace("/", "") + "/{id}",
				defaults: new { controller = "Home", action = "GetAdminScriptValues", id = UrlParameter.Optional },
				namespaces: _namespaces.ToArray());
		}
	}
}