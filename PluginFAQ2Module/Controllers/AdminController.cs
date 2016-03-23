using CarrotCake.CMS.Plugins.FAQ2.Models;
using Carrotware.CMS.DBUpdater;
using Carrotware.CMS.Interface.Controllers;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CarrotCake.CMS.Plugins.FAQ2.Controllers {

	public class AdminController : BaseAdminWidgetController {

		public ActionResult Index() {
			PagedData<carrot_FaqCategory> model = new PagedData<carrot_FaqCategory>();
			model.InitOrderBy(x => x.FAQTitle, true);
			model.PageSize = 25;
			model.PageNumber = 1;

			return Index(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Index(PagedData<carrot_FaqCategory> model) {
			model.ToggleSort();
			var srt = model.ParseSort();

			List<carrot_FaqCategory> lst = null;

			using (FaqHelper fh = new FaqHelper(this.SiteID)) {
				lst = fh.CategoryListGetBySiteID();
			}

			IQueryable<carrot_FaqCategory> query = lst.AsQueryable();
			query = query.SortByParm<carrot_FaqCategory>(srt.SortField, srt.SortDirection);

			model.DataSource = query.Skip(model.PageSize * model.PageNumberZeroIndex).Take(model.PageSize).ToList();

			model.TotalRecords = lst.Count();

			ModelState.Clear();

			return View(model);
		}

		public ActionResult CreateFaq() {
			carrot_FaqCategory model = new carrot_FaqCategory();
			model.SiteID = this.SiteID;

			return View("EditFaq", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CreateFaq(carrot_FaqCategory model) {
			return EditFaq(model);
		}

		public ActionResult EditFaq(Guid id) {
			using (FaqHelper fh = new FaqHelper(this.SiteID)) {
				return View("EditFaq", fh.CategoryGetByID(id));
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditFaq(carrot_FaqCategory model) {
			if (ModelState.IsValid) {
				using (FaqHelper fh = new FaqHelper(this.SiteID)) {
					var fc = fh.CategoryGetByID(model.FaqCategoryID);

					if (fc == null || model.FaqCategoryID == Guid.Empty) {
						model.FaqCategoryID = Guid.NewGuid();
						fc = new carrot_FaqCategory();
						fc.SiteID = this.SiteID;
						fc.FaqCategoryID = model.FaqCategoryID;
					}

					fc.FAQTitle = model.FAQTitle;

					fh.Save(fc);
				}

				return RedirectToAction("Index");
			} else {
				return View("EditFaq", model);
			}
		}

		public ActionResult ListFaqItems(Guid id) {
			FaqListing model = new FaqListing();
			model.Faq.FaqCategoryID = id;
			model.Faq.SiteID = this.SiteID;

			return ListFaqItems(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ListFaqItems(FaqListing model) {
			model.Items.ToggleSort();
			var srt = model.Items.ParseSort();

			List<carrot_FaqItem> lst = null;

			using (FaqHelper fh = new FaqHelper(this.SiteID)) {
				model.Faq = fh.CategoryGetByID(model.Faq.FaqCategoryID);
				lst = fh.FaqItemListGetByFaqCategoryID(model.Faq.FaqCategoryID);
			}

			IQueryable<carrot_FaqItem> query = lst.AsQueryable();
			query = query.SortByParm<carrot_FaqItem>(srt.SortField, srt.SortDirection);

			model.Items.DataSource = query.Skip(model.Items.PageSize * model.Items.PageNumberZeroIndex).Take(model.Items.PageSize).ToList();

			model.Items.TotalRecords = lst.Count();

			ModelState.Clear();

			return View(model);
		}

		public ActionResult CreateFaqItem(Guid parent) {
			carrot_FaqItem model = new carrot_FaqItem();
			model.FaqCategoryID = parent;

			return View("EditFaqItem", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)]
		public ActionResult CreateFaqItem(carrot_FaqItem model) {
			return EditFaqItem(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)]
		public ActionResult DeleteFaqItem(carrot_FaqItem model) {
			using (FaqHelper fh = new FaqHelper(this.SiteID)) {
				fh.DeleteItem(model.FaqItemID);
			}

			return RedirectToAction("ListFaqItems", new { @id = model.FaqCategoryID });
		}

		public ActionResult EditFaqItem(Guid id) {
			using (FaqHelper fh = new FaqHelper(this.SiteID)) {
				return View("EditFaqItem", fh.FaqItemGetByID(id));
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)]
		public ActionResult EditFaqItem(carrot_FaqItem model) {
			if (ModelState.IsValid) {
				using (FaqHelper fh = new FaqHelper(this.SiteID)) {
					var fc = fh.FaqItemGetByID(model.FaqItemID);

					if (fc == null || model.FaqCategoryID == Guid.Empty) {
						model.FaqItemID = Guid.NewGuid();
						fc = new carrot_FaqItem();
						fc.FaqCategoryID = model.FaqCategoryID;
						fc.FaqItemID = model.FaqItemID;
					}

					fc.Caption = model.Caption;
					fc.Question = model.Question;
					fc.Answer = model.Answer;
					fc.ItemOrder = model.ItemOrder;
					fc.IsActive = model.IsActive;

					fh.Save(fc);
				}

				return RedirectToAction("ListFaqItems", new { @id = model.FaqCategoryID });
			} else {
				return View("EditFaqItem", model);
			}
		}

		public ActionResult FaqDatabase() {
			List<string> lst = new List<string>();

			DatabaseUpdate du = new DatabaseUpdate();
			DatabaseUpdateResponse dbRes = new DatabaseUpdateResponse();
			string sqlUpdate = "";
			string sqlTest = "";
			try {
				sqlUpdate = FaqHelper.ReadEmbededScript("CarrotCake.CMS.Plugins.FAQ2.carrot_FaqItem.sql");

				sqlTest = "select * from [INFORMATION_SCHEMA].[COLUMNS] where table_name in('carrot_FaqCategory')";
				dbRes = du.ApplyUpdateIfNotFound(sqlTest, sqlUpdate, false);

				if (dbRes.LastException != null && !string.IsNullOrEmpty(dbRes.LastException.Message)) {
					lst.Add(dbRes.LastException.Message);
				} else {
					lst.Add(dbRes.Response);
				}
			} catch (Exception ex) {
				lst.Add(ex.ToString());
			}

			return View(lst);
		}
	}
}