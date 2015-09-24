using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Carrotware.CMS.Interface.Controllers {

	public class BaseWidgetController : Controller {
		protected string assemblyName = String.Empty;

		public BaseWidgetController()
			: base() {
			Assembly asmbly = this.GetType().Assembly;

			assemblyName = asmbly.ManifestModule.Name;
			assemblyName = assemblyName.Substring(0, assemblyName.Length - 4);

			List<CarrotViewEngine> lst = this.ViewEngineCollection
								.Where(x => x is CarrotViewEngine).Cast<CarrotViewEngine>()
								.Where(x => x.AssemblyKey == assemblyName).ToList();

			if (!lst.Any()) {
				CarrotViewEngine ve = new CarrotViewEngine(assemblyName);

				this.ViewEngineCollection.Add(ve);
			}
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			// only add the xtra lookup paths so long as needed to render the relative path partials from the template
			List<CarrotViewEngine> lst = this.ViewEngineCollection
								.Where(x => x is CarrotViewEngine).Cast<CarrotViewEngine>()
								.Where(x => x.AssemblyKey == assemblyName).ToList();

			if (lst.Count > 0) {
				for (int j = (lst.Count - 1); j >= 0; j--) {
					this.ViewEngineCollection.Remove(lst[j]);
				}
			}
		}
	}
}