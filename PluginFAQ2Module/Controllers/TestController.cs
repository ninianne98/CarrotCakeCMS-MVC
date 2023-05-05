using CarrotCake.CMS.Plugins.FAQ2.Models;
using Carrotware.CMS.Interface;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CarrotCake.CMS.Plugins.FAQ2.Controllers {

	public class TestController : BaseController {

		protected override void Initialize(RequestContext requestContext) {
			base.Initialize(requestContext);

			RouteValueDictionary vals = requestContext.RouteData.Values;

			// use the test id to build a fake payload so the widget can be loaded for dev
			string action = vals["action"].ToString().ToLowerInvariant();
			string controller = vals["controller"].ToString().ToLowerInvariant();

			// since there are different models, set them up as needed to match the test
			if (action.ToLowerInvariant() == "testview1" || action.ToLowerInvariant() == "testview2") {
				var settings = new FaqPublic();
				settings.SiteID = new Guid(this.TestSiteID);

				if (vals.ContainsKey("id")) {
					string id = vals["id"].ToString().ToLowerInvariant();
					settings.FaqCategoryID = new Guid(id);
					settings.WidgetClientID = "Widget_" + settings.FaqCategoryID.ToString().ToLowerInvariant().Substring(0, 5);
				}

				this.WidgetPayload = settings;
			}

			if (action.ToLowerInvariant() == "testview3") {
				var settings = new FaqPublicTop();
				settings.SiteID = new Guid(this.TestSiteID);

				if (vals.ContainsKey("id")) {
					string id = vals["id"].ToString().ToLowerInvariant();
					settings.FaqCategoryID = new Guid(id);
					settings.WidgetClientID = "Widget_" + settings.FaqCategoryID.ToString().ToLowerInvariant().Substring(0, 5);
				}

				int top = 3;

				if (HttpContext.Request.QueryString["top"] != null) {
					top = int.Parse(HttpContext.Request.QueryString["top"]);
				} else {
					if (vals.ContainsKey("top")) {
						top = int.Parse(vals["top"].ToString());
					}
				}

				settings.TakeTop = top;

				this.WidgetPayload = settings;
			}
		}

		// ViewBag.WidgetTitle

		public ActionResult Index() {
			List<carrot_FaqCategory> lst = null;

			using (FaqHelper fh = new FaqHelper(new Guid(this.TestSiteID))) {
				lst = fh.CategoryListGetBySiteID();
			}

			return View(lst);
		}

		public ActionResult TestView1(Guid id) {
			var model = new TestModel();

			var ctrl = RenderWidgetHelper.CreateController<HomeController>(this, "ShowFaqList", this.AssemblyName, this.WidgetPayload);
			var result = RenderWidgetHelper.ExecuteAction(ctrl);
			model.RenderedContent = new HtmlString(RenderWidgetHelper.ResultToString(ctrl, result));

			ViewBag.WidgetTitle = "Test Widget Display 1";

			return View("TestView", model);
		}

		public ActionResult TestView2(Guid id) {
			var model = new TestModel();

			var ctrl = RenderWidgetHelper.CreateController<HomeController>(this, "ShowRandomFaq", this.AssemblyName, this.WidgetPayload);
			var result = RenderWidgetHelper.ExecuteAction(ctrl);
			model.RenderedContent = new HtmlString(RenderWidgetHelper.ResultToString(ctrl, result));

			ViewBag.WidgetTitle = "Test Widget Display 2";

			return View("TestView", model);
		}

		public ActionResult TestView3(Guid id, int top) {
			var model = new TestModel();

			var ctrl = RenderWidgetHelper.CreateController<HomeController>(this, "ShowFaqTopList", this.AssemblyName, this.WidgetPayload);
			var result = RenderWidgetHelper.ExecuteAction(ctrl);
			model.RenderedContent = new HtmlString(RenderWidgetHelper.ResultToString(ctrl, result));

			ViewBag.WidgetTitle = "Test Widget Display 3";

			return View("TestView", model);
		}
	}
}