using Carrotware.CMS.Interface;
using Carrotware.Web.UI.Components;
using System.Web.Mvc;
using System.Web.Routing;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace CarrotCake.CMS.Plugins.PhotoGallery.Controllers {

	public class HomeController : BasePublicController {

		protected override void Initialize(RequestContext requestContext) {
			base.Initialize(requestContext);

			var vals = requestContext.RouteData.Values;
			var routeInfo = vals.GetRouteInfo();
			// use the test id to build a fake payload so the widget can be loaded for dev
			string action = routeInfo.Action;
			string controller = routeInfo.Controller;
			string id = routeInfo.Id;
		}

		public ActionResult Index() {
			return View(nameof(this.Index));
		}

		public ActionResult Index2() {
			return Index();
		}

		[WidgetActionSettingModel("CarrotCake.CMS.Plugins.PhotoGallery.GallerySettings, CarrotCake.CMS.Plugins.PhotoGallery")]
		public PartialViewResult ShowPrettyPhotoGallery() {
			var settings = new GallerySettings();

			if (this.WidgetPayload is GallerySettings) {
				settings = (GallerySettings)this.WidgetPayload;
				settings.LoadData();
			}

			var model = BuildModel(settings);

			if (string.IsNullOrWhiteSpace(settings.AlternateViewFile)) {
				return PartialView(model);
			} else {
				return PartialView(settings.AlternateViewFile, model);
			}
		}
	}
}