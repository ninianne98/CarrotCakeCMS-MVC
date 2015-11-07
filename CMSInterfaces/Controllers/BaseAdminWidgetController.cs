using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

	public class BaseAdminWidgetController : BaseWidgetController, IAdminModule {
		protected Guid requestKey = Guid.NewGuid();

		public BaseAdminWidgetController()
			: base() {

			List<CarrotViewEngine> lst = this.ViewEngineCollection
								.Where(x => x is CarrotViewEngine).Cast<CarrotViewEngine>()
								.Where(x => x.RequestKey == requestKey).ToList();

			if (!lst.Any()) {
				CarrotViewEngine ve = new CarrotViewEngine(this.AssemblyName, requestKey);

				this.ViewEngineCollection.Add(ve);
			}
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			// only add the xtra lookup paths so long as needed to render the relative path partials from the template
			List<CarrotViewEngine> lst = this.ViewEngineCollection
								.Where(x => x is CarrotViewEngine).Cast<CarrotViewEngine>()
								.Where(x => x.RequestKey == requestKey).ToList();

			if (lst.Any()) {
				for (int j = (lst.Count - 1); j >= 0; j--) {
					this.ViewEngineCollection.Remove(lst[j]);
				}
			}
		}

		public Guid SiteID { get; set; }

		public Guid ModuleID { get; set; }

		public string ModuleName { get; set; }
	}
}