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

	public class TestController : BasePublicController {
		private GalleryHelper _helper;

		private Guid _siteid = Guid.Empty;

		public TestController() {
			_siteid = new Guid(this.TestSiteID);
		}

		protected override void Initialize(RequestContext requestContext) {
			base.Initialize(requestContext);
			_siteid = new Guid(this.TestSiteID);

			_helper = new GalleryHelper(_siteid);

			RouteValueDictionary vals = requestContext.RouteData.Values;

			// use the test id to build a fake payload so the widget can be loaded for dev
			string action = vals["action"].ToString().ToLowerInvariant();
			string controller = vals["controller"].ToString().ToLowerInvariant();

			var settings = new GallerySettings();
			settings.SiteID = _siteid;
			settings.ShowHeading = true;

			if (vals.ContainsKey("id")) {
				string id = vals["id"].ToString().ToLowerInvariant();
				settings.GalleryID = new Guid(id);
				settings.WidgetClientID = "Widget_" + settings.GalleryID.ToString().ToLowerInvariant().Substring(0, 5);
			}

			settings.PublicParmValues.Add("SiteID", _siteid.ToString());
			settings.PublicParmValues.Add("WidgetClientID", settings.WidgetClientID);
			settings.PublicParmValues.Add("GalleryID", settings.GalleryID.ToString().ToLowerInvariant());
			settings.PublicParmValues.Add("ShowHeading", settings.ShowHeading.ToString());

			this.WidgetPayload = settings;
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			if (_helper != null) {
				_helper.Dispose();
			}
		}

		[HttpGet]
		public ActionResult Index() {
			var model = new PagedData<tblGalleryImage>();
			model.InitOrderBy(x => x.ImageOrder, true);

			using (var db = PhotoGalleryDataContext.GetDataContext()) {
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

			using (var db = PhotoGalleryDataContext.GetDataContext()) {
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
			var model = new tblGalleryImage();

			using (var db = PhotoGalleryDataContext.GetDataContext()) {
				model = (from c in db.tblGalleryImages
						 where c.GalleryImageID == id
						 select c).FirstOrDefault();
			}

			return View(model);
		}

		[HttpGet]
		public ActionResult GalleryList() {
			var siteid = _siteid;
			var model = new PagedData<tblGallery>();
			//model.PageSize = 10;
			//model.PageNumber = 1;
			model.InitOrderBy(x => x.GalleryTitle, true);

			using (var db = PhotoGalleryDataContext.GetDataContext()) {
				model.DataSource = (from c in db.tblGalleries
									where c.SiteID == siteid
									orderby c.GalleryTitle ascending
									select c).Skip(model.PageSize * (model.PageNumber - 1)).Take(model.PageSize).ToList();

				model.TotalRecords = (from c in db.tblGalleries select c).Count();
			}

			return View(model);
		}

		[HttpPost]
		public ActionResult GalleryList(PagedData<tblGallery> model) {
			var siteid = _siteid;

			model.ToggleSort();
			var srt = model.ParseSort();

			using (var db = PhotoGalleryDataContext.GetDataContext()) {
				IQueryable<tblGallery> query = (from c in db.tblGalleries
												where c.SiteID == siteid
												orderby c.GalleryTitle ascending
												select c);

				query = query.SortByParm(srt.SortField, srt.SortDirection);

				model.DataSource = query.Skip(model.PageSize * (model.PageNumber - 1)).Take(model.PageSize).ToList();

				model.TotalRecords = (from c in db.tblGalleries
									  where c.SiteID == siteid
									  orderby c.GalleryTitle ascending
									  select c).Count();
			}

			ModelState.Clear();

			return View(model);
		}

		public ActionResult GalleryView2(Guid id) {
			var model = new GalleryTest();
			model.Settings = (GallerySettings)this.WidgetPayload;

			var ctrl = this.CreateController<HomeController>("ShowPrettyPhotoGallery", this.AssemblyName, this.WidgetPayload);

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

			var ctrl = this.CreateController<HomeController>("ShowPrettyPhotoGallery", this.AssemblyName, this.WidgetPayload);
			model.PartialResult = ctrl.ExecuteAction();
			model.RenderedContent = ctrl.ResultToString(model.PartialResult);

			ViewBag.WidgetTitle = "Test Widget Display 4";

			return View("GalleryView2", model);
		}

		public ActionResult GalleryView(Guid id) {
			var settings = new GallerySettings();

			if (this.WidgetPayload is GallerySettings) {
				settings = (GallerySettings)this.WidgetPayload;
				settings.LoadData();
			}

			var model = BuildModel(settings);

			return View(model);
		}

		public ActionResult Index2() {
			var lst = new List<tblGalleryImage>();

			using (var db = PhotoGalleryDataContext.GetDataContext()) {
				lst = (from c in db.tblGalleryImages
					   select c).ToList();
			}

			return View(lst);
		}
	}
}