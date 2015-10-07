using Carrotware.CMS.Interface;
using Carrotware.CMS.Interface.Controllers;
using Northwind.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Northwind.Controllers {

	public class HomeController : BaseDataWidgetController {

		public ActionResult Index() {
			return View();
		}

		public ActionResult About() {
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact() {
			ViewBag.Message = "Your contact page.";

			return View();
		}

		[HttpGet]
		[WidgetActionSettingModel("Carrotware.CMS.Interface.WidgetActionSettingModel, Carrotware.CMS.Interface")]
		public PartialViewResult ProductSearch() {
			WidgetActionSettingModel settings = new WidgetActionSettingModel();

			if (WidgetPayload is WidgetActionSettingModel) {
				settings = (WidgetActionSettingModel)WidgetPayload;
				settings.LoadData();
			}

			ProductSearch model = null;
			model = InitProductSearch(model);

			if (String.IsNullOrEmpty(settings.AlternateViewFile)) {
				return PartialView(model);
			} else {
				model.AltViewName = settings.AlternateViewFile;
				return PartialView(settings.AlternateViewFile, model);
			}
		}

		[HttpPost]
		[WidgetActionSettingModel("Carrotware.CMS.Interface.WidgetActionSettingModel, Carrotware.CMS.Interface")]
		public PartialViewResult ProductSearch(ProductSearch model) {
			WidgetActionSettingModel settings = new WidgetActionSettingModel();

			if (WidgetPayload is WidgetActionSettingModel) {
				settings = (WidgetActionSettingModel)WidgetPayload;
				settings.LoadData();
			}

			model = InitProductSearch(model);

			if (String.IsNullOrEmpty(model.AltViewName)) {
				return PartialView(model);
			} else {
				return PartialView(model.AltViewName, model);
			}
		}

		public ProductSearch InitProductSearch(ProductSearch model) {
			if (model == null) {
				model = new ProductSearch();
				model.SelectedCat = -1;
			}

			using (var db = new NorthwindDataContext()) {
				model.Options = db.Categories.ToList();
				model.Results = new List<Product>();

				if (model.SelectedCat.HasValue) {
					model.Results = db.Products.Where(x => x.CategoryID == model.SelectedCat.Value).ToList();
				}
			}

			return model;
		}
	}
}