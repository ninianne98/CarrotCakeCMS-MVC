using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Carrotware.CMS.Interface.Controllers {

	public class BaseWidgetController : Controller, IWidgetController {
		protected string assemblyName = String.Empty;
		//protected Guid requestKey = Guid.NewGuid();

		public BaseWidgetController()
			: base() {
			Assembly asmbly = this.GetType().Assembly;

			assemblyName = asmbly.ManifestModule.Name;
			assemblyName = assemblyName.Substring(0, assemblyName.Length - 4);

			this.ViewData[CarrotViewEngineWidget.Key] = assemblyName;
			this.AssemblyName = assemblyName;

			//List<CarrotViewEngine> lst = this.ViewEngineCollection
			//					.Where(x => x is CarrotViewEngine).Cast<CarrotViewEngine>()
			//					.Where(x => x.RequestKey == requestKey).ToList();

			//if (!lst.Any()) {
			//	CarrotViewEngine ve = new CarrotViewEngine(assemblyName, requestKey);

			//	this.ViewEngineCollection.Add(ve);
			//}
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			//// only add the xtra lookup paths so long as needed to render the relative path partials from the template
			//List<CarrotViewEngine> lst = this.ViewEngineCollection
			//					.Where(x => x is CarrotViewEngine).Cast<CarrotViewEngine>()
			//					.Where(x => x.RequestKey == requestKey).ToList();

			//if (lst.Any()) {
			//	for (int j = (lst.Count - 1); j >= 0; j--) {
			//		this.ViewEngineCollection.Remove(lst[j]);
			//	}
			//}
		}

		public string AssemblyName { get; set; }
	}
}