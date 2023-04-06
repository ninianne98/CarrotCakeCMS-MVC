﻿using Carrotware.CMS.Interface;
using Carrotware.CMS.Interface.Controllers;
using Northwind.Code;
using Northwind.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Northwind.Controllers {

	public class HomeController : BaseDataWidgetController {

		public ActionResult Index() {
			return View();
		}

		[HttpGet]
		public ActionResult Sampler() {
			SelectSkin model = new SelectSkin();

			return View(model);
		}

		[HttpPost]
		public ActionResult Sampler(SelectSkin model) {
			var scheme = model.SelectedItem;

			Helper.SetBootstrapColor(scheme);

			return RedirectToAction("Sampler");
		}

		public ActionResult Contact() {
			ViewBag.Message = "Your contact page.";

			return View();
		}

		[HttpGet]
		[WidgetActionSettingModel("Carrotware.CMS.Interface.WidgetActionSettingModel, Carrotware.CMS.Interface")]
		public ActionResult ProductSearch() {
			WidgetActionSettingModel settings = new WidgetActionSettingModel();

			if (this.WidgetPayload is WidgetActionSettingModel) {
				settings = (WidgetActionSettingModel)this.WidgetPayload;
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
		[WidgetActionSettingModel(typeof(WidgetActionSettingModel))]
		public ActionResult ProductSearch(ProductSearch model) {
			WidgetActionSettingModel settings = new WidgetActionSettingModel();

			if (this.WidgetPayload is WidgetActionSettingModel) {
				settings = (WidgetActionSettingModel)this.WidgetPayload;
				settings.LoadData();
			}

			model = InitProductSearch(model);

			if (String.IsNullOrEmpty(model.AltViewName)) {
				return PartialView(model);
			} else {
				return PartialView(model.AltViewName, model);
			}
		}

		[HttpGet]
		public ActionResult TestProductSearch() {
			ProductSearch model = null;
			model = InitProductSearch(model);

			return View(model);
		}

		[HttpPost]
		public ActionResult TestProductSearch(ProductSearch model) {
			model = InitProductSearch(model);

			return View(model);
		}

		[HttpGet]
		[WidgetActionSettingModel(typeof(MultiOptions))]
		public ActionResult ProductSearchMulti() {
			MultiOptions settings = new MultiOptions();

			if (this.WidgetPayload is MultiOptions) {
				settings = (MultiOptions)this.WidgetPayload;
				settings.LoadData();
			}

			ProductSearch model = settings.GetData();

			if (String.IsNullOrEmpty(settings.AlternateViewFile)) {
				return PartialView(model);
			} else {
				model.AltViewName = settings.AlternateViewFile;
				return PartialView(settings.AlternateViewFile, model);
			}
		}

		[HttpGet]
		public ActionResult TestProductSearchMulti() {
			var model = new TestModel();

			return View(model);
		}

		[HttpPost]
		public ActionResult TestProductSearchMulti(TestModel model) {
			var opts = new MultiOptions();
			opts.AlternateViewFile = model.SelectedView;
			if (model.SelectedCategories != null) {
				opts.CategoryIDs = model.SelectedCategories.Select(x => Convert.ToInt32(x)).ToList();
			}
			this.WidgetPayload = opts;

			var settings = new MultiOptions();
			if (this.WidgetPayload is MultiOptions) {
				settings = (MultiOptions)this.WidgetPayload;
				settings.LoadData();
			}

			ProductSearch data = settings.GetData();
			data.AltViewName = settings.AlternateViewFile;
			model.ProductSearch = data;

			return View(model);
		}

		public ProductSearch InitProductSearch(ProductSearch model) {
			if (model == null) {
				model = new ProductSearch();
				model.SelectedCat = -1;
			}

			using (var db = new NorthwindDataContext()) {
				model.Options = db.Categories.ToList();

				if (model.SelectedCat.HasValue) {
					model.Results = db.Products.Where(x => x.CategoryID == model.SelectedCat.Value).ToList();
				}
			}

			return model;
		}
	}
}