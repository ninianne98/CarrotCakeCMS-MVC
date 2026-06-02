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
		protected string _areaName = string.Empty;
		protected List<string> _namespaces = new List<string>();

		public BaseWidgetAreaReg()
			: base() {
			Assembly assembly = this.GetType().Assembly;

			_namespaces = assembly.GetTypes().Select(t => t.Namespace)
								.Where(x => !string.IsNullOrEmpty(x))
								.Distinct().ToList();

			_areaName = assembly.GetAssemblyName();
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
			string nsp = typeof(BaseWidgetAreaReg).Namespace;

			if (this.AreaName.ToLowerInvariant() != nsp.ToLowerInvariant()) {
				context.MapRoute(
						name: string.Format("{0}_Default", this.AreaName),
						url: this.AreaName + "/{controller}/{action}/{id}",
						defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
						namespaces: _namespaces.ToArray()
					);
			}
		}
	}
}