using Carrotware.CMS.Interface;
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

			RouteValueDictionary vals = requestContext.RouteData.Values;

			// use the test id to build a fake payload so the widget can be loaded for dev
			string action = vals["action"].ToString().ToLowerInvariant();
			string controller = vals["controller"].ToString().ToLowerInvariant();
		}

		public ActionResult Index() {
			return View("Index");
		}

		public ActionResult Index2() {
			return Index();
		}

		[WidgetActionSettingModel("CarrotCake.CMS.Plugins.PhotoGallery.GallerySettings, CarrotCake.CMS.Plugins.PhotoGallery")]
		public PartialViewResult ShowPrettyPhotoGallery() {
			var settings = new GallerySettings();

			if (WidgetPayload is GallerySettings) {
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