using CarrotCake.CMS.Plugins.PhotoGallery.Models;
using Carrotware.CMS.Interface;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CarrotCake.CMS.Plugins.PhotoGallery.Controllers {

	public class TestController : BasePublicController {
		private GalleryHelper helper = new GalleryHelper();

		public TestController() {
			helper = new GalleryHelper(new Guid(this.TestSiteID));
		}

		protected override void Initialize(RequestContext requestContext) {
			base.Initialize(requestContext);

			RouteValueDictionary vals = requestContext.RouteData.Values;

			// use the test id to build a fake payload so the widget can be loaded for dev
			string action = vals["action"].ToString().ToLowerInvariant();
			string controller = vals["controller"].ToString().ToLowerInvariant();

			var settings = new GallerySettings();
			settings.SiteID = new Guid(this.TestSiteID);
			settings.ShowHeading = true;

			if (vals.ContainsKey("id")) {
				string id = vals["id"].ToString().ToLowerInvariant();
				settings.GalleryID = new Guid(id);
				settings.WidgetClientID = "Widget_" + settings.GalleryID.ToString().ToLowerInvariant().Substring(0, 5);
			}

			settings.PublicParmValues.Add("SiteID", this.TestSiteID);
			settings.PublicParmValues.Add("WidgetClientID", settings.WidgetClientID);
			settings.PublicParmValues.Add("GalleryID", settings.GalleryID.ToString().ToLowerInvariant());
			settings.PublicParmValues.Add("ShowHeading", settings.ShowHeading.ToString());

			WidgetPayload = settings;
		}

		protected override void OnAuthorization(AuthorizationContext filterContext) {
			base.OnAuthorization(filterContext);

			RouteValueDictionary vals = filterContext.RouteData.Values;
		}

		//public ActionResult TestShowPrettyPhotoGallery(Guid id) {
		//	GallerySettings settings = new GallerySettings();
		//	settings.GalleryID = id;
		//	settings.SiteID = new Guid(this.TestSiteID);

		//	GalleryModel model = BuildModel(settings);

		//	return View(model);
		//}

		[HttpGet]
		public ActionResult Index() {
			var model = new PagedData<tblGalleryImage>();
			model.InitOrderBy(x => x.ImageOrder, true);

			using (PhotoGalleryDataContext db = PhotoGalleryDataContext.GetDataContext()) {
				model.DataSource = (from c in db.tblGalleryImages
									orderby c.ImageOrder ascending
									select c).Skip(model.PageSize * (model.PageNumber - 1)).Take(model.PageSize).ToList();

				model.TotalRecords = (from c in db.tblGalleryImages select c).Count();
			}

			return View(model);
		}

		[HttpPost]
		public ActionResult Index(PagedData<tblGalleryImage> model) {
			model.ToggleSort();
			var srt = model.ParseSort();

			using (PhotoGalleryDataContext db = PhotoGalleryDataContext.GetDataContext()) {
				IQueryable<tblGalleryImage> query = (from c in db.tblGalleryImages select c);

				query = query.SortByParm(srt.SortField, srt.SortDirection);

				model.DataSource = query.Skip(model.PageSize * (model.PageNumber - 1)).Take(model.PageSize).ToList();

				model.TotalRecords = (from c in db.tblGalleryImages select c).Count();
			}

			ModelState.Clear();

			return View(model);
		}

		public ActionResult Edit(Guid id) {
			return View(id);
		}

		public ActionResult View(Guid id) {
			tblGalleryImage model = new tblGalleryImage();

			using (PhotoGalleryDataContext db = PhotoGalleryDataContext.GetDataContext()) {
				model = (from c in db.tblGalleryImages
						 where c.GalleryImageID == id
						 select c).FirstOrDefault();
			}

			return View(model);
		}

		[HttpGet]
		public ActionResult GalleryList() {
			var model = new PagedData<tblGallery>();
			//model.PageSize = 10;
			//model.PageNumber = 1;
			model.InitOrderBy(x => x.GalleryTitle, true);

			using (PhotoGalleryDataContext db = PhotoGalleryDataContext.GetDataContext()) {
				model.DataSource = (from c in db.tblGalleries
									orderby c.GalleryTitle ascending
									select c).Skip(model.PageSize * (model.PageNumber - 1)).Take(model.PageSize).ToList();

				model.TotalRecords = (from c in db.tblGalleries select c).Count();
			}

			return View(model);
		}

		[HttpPost]
		public ActionResult GalleryList(PagedData<tblGallery> model) {
			model.ToggleSort();
			var srt = model.ParseSort();

			using (PhotoGalleryDataContext db = PhotoGalleryDataContext.GetDataContext()) {
				IQueryable<tblGallery> query = (from c in db.tblGalleries select c);

				query = query.SortByParm(srt.SortField, srt.SortDirection);

				model.DataSource = query.Skip(model.PageSize * (model.PageNumber - 1)).Take(model.PageSize).ToList();

				model.TotalRecords = (from c in db.tblGalleries select c).Count();
			}

			ModelState.Clear();

			return View(model);
		}

		public ActionResult GalleryView2(Guid id) {
			var model = new GalleryTest();
			model.Settings = (GallerySettings)this.WidgetPayload;

			var ctrl = RenderWidgetHelper.CreateController<HomeController>(this, "ShowPrettyPhotoGallery", this.AssemblyName, this.WidgetPayload);

			var result = ((HomeController)ctrl).ShowPrettyPhotoGallery();
			model.PartialResult = result;

			string viewName = model.Settings.AlternateViewFile ?? result.ViewName;

			if (string.IsNullOrEmpty(viewName)) {
				viewName = "ShowPrettyPhotoGallery";
			}

			model.RenderedContent = RenderWidgetHelper.RenderViewToString(ctrl, result, viewName);

			ViewBag.WidgetTitle = "Test Widget Display 2";

			return View(model);
		}

		public ViewResult GalleryView3(Guid id) {
			var model = new GalleryTest();
			model.Settings = (GallerySettings)this.WidgetPayload;

			Type type = typeof(HomeController);
			object obj = Activator.CreateInstance(type);

			var routeData = new RouteData();
			routeData.Values.Add("controller", "Home");
			routeData.Values.Add("area", AssemblyName);
			routeData.Values.Add("action", "ShowPrettyPhotoGallery");

			MethodInfo methodInfo = type.GetMethod("ShowPrettyPhotoGallery");

			Controller ctrl = null;

			if (methodInfo != null) {
				object result = null;
				ParameterInfo[] parameters = methodInfo.GetParameters();

				object classInstance = Activator.CreateInstance(type, null);

				if (classInstance is Controller) {
					ctrl = ((Controller)classInstance);
					var wrapper = new HttpContextWrapper(System.Web.HttpContext.Current);
					ctrl.ControllerContext = new ControllerContext(wrapper, routeData, ctrl);

					((HomeController)ctrl).WidgetPayload = this.WidgetPayload;
				}
				if (parameters.Length == 0) {
					result = methodInfo.Invoke(classInstance, null);

					model.PartialResult = (PartialViewResult)result;
				}

				// when action = view ViewName is blank, when alt view is used, populated
				model.RenderedContent = RenderWidgetHelper.RenderViewToString(ctrl, result, model.PartialResult.ViewName);
			}

			ViewBag.WidgetTitle = "Test Widget Display 3";

			return View("GalleryView2", model);
		}

		public ViewResult GalleryView4(Guid id) {
			var model = new GalleryTest();
			model.Settings = (GallerySettings)this.WidgetPayload;

			var ctrl = RenderWidgetHelper.CreateController<HomeController>(this, "ShowPrettyPhotoGallery", this.AssemblyName, this.WidgetPayload);
			model.PartialResult = RenderWidgetHelper.ExecuteAction(ctrl);
			model.RenderedContent = RenderWidgetHelper.ResultToString(ctrl, model.PartialResult);

			ViewBag.WidgetTitle = "Test Widget Display 4";

			return View("GalleryView2", model);
		}

		public ActionResult GalleryView(Guid id) {
			GallerySettings settings = new GallerySettings();

			if (WidgetPayload is GallerySettings) {
				settings = (GallerySettings)WidgetPayload;
				settings.LoadData();
			}

			var model = BuildModel(settings);

			return View(model);
		}

		public ActionResult Index2() {
			List<tblGalleryImage> lst = new List<tblGalleryImage>();

			using (PhotoGalleryDataContext db = PhotoGalleryDataContext.GetDataContext()) {
				lst = (from c in db.tblGalleryImages
					   select c).ToList();
			}

			return View(lst);
		}
	}
}