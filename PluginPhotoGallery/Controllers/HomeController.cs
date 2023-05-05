using Carrotware.CMS.Interface;
using System.Web.Mvc;
using System.Web.Routing;

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
			GallerySettings settings = new GallerySettings();

			if (WidgetPayload is GallerySettings) {
				settings = (GallerySettings)WidgetPayload;
				settings.LoadData();
			}

			var model = BuildModel(settings);

			if (string.IsNullOrEmpty(settings.AlternateViewFile)) {
				return PartialView(model);
			} else {
				return PartialView(settings.AlternateViewFile, model);
			}
		}
	}
}