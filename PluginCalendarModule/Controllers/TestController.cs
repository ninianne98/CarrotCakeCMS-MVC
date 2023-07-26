 using CarrotCake.CMS.Plugins.CalendarModule.Models;
using Carrotware.CMS.Interface;
using System;
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

namespace CarrotCake.CMS.Plugins.CalendarModule.Controllers {

	public class TestController : BaseController {
		private Guid _siteId = Guid.Empty;

		protected override void Initialize(RequestContext requestContext) {
			base.Initialize(requestContext);

			RouteValueDictionary vals = requestContext.RouteData.Values;

			// use the test id to build a fake payload so the widget can be loaded for dev
			string action = vals["action"].ToString().ToLowerInvariant();
			string controller = vals["controller"].ToString().ToLowerInvariant();

			if (this.TestSiteID != Guid.Empty.ToString()) {
				_siteId = new Guid(this.TestSiteID);
			}

			// since there are different models, set them up as needed to match the test

			if (action.ToLowerInvariant() == "testview1") {
				var settings = new CalendarUpcomingSettings();
				settings.SiteID = _siteId;
				settings.DaysInPast = -21;
				settings.DaysInFuture = 21;

				this.WidgetPayload = settings;
			}

			if (action.ToLowerInvariant() == "testview2") {
				var settings = new CalendarDisplaySettings();
				settings.SiteID = _siteId;

				this.WidgetPayload = settings;
			}

			if (action.ToLowerInvariant() == "testview3") {
				var settings = new CalendarSimpleSettings();
				settings.SiteID = _siteId;

				this.WidgetPayload = settings;
			}
		}

		public ActionResult Index() {
			return View();
		}

		public ActionResult TestView1() {
			var model = new TestModel();

			var ctrl = RenderWidgetHelper.CreateController<HomeController>(this, "CalendarUpcoming", this.AssemblyName, this.WidgetPayload);
			var result = RenderWidgetHelper.ExecuteAction(ctrl);
			model.RenderedContent = new HtmlString(RenderWidgetHelper.ResultToString(ctrl, result));

			ViewBag.WidgetTitle = "Test Widget Display 1";

			return View("TestView", model);
		}

		public ActionResult TestView2() {
			var model = new TestModel();

			var ctrl = RenderWidgetHelper.CreateController<HomeController>(this, "CalendarDisplay", this.AssemblyName, this.WidgetPayload);
			var result = RenderWidgetHelper.ExecuteAction(ctrl);
			model.RenderedContent = new HtmlString(RenderWidgetHelper.ResultToString(ctrl, result));

			ViewBag.WidgetTitle = "Test Widget Display 2";

			return View("TestView", model);
		}

		public ActionResult TestView3() {
			var model = new TestModel();

			var ctrl = RenderWidgetHelper.CreateController<HomeController>(this, "CalendarDisplaySimple", this.AssemblyName, this.WidgetPayload);
			var result = RenderWidgetHelper.ExecuteAction(ctrl);
			model.RenderedContent = new HtmlString(RenderWidgetHelper.ResultToString(ctrl, result));

			ViewBag.WidgetTitle = "Test Widget Display 3";

			return View("TestView", model);
		}
	}
}