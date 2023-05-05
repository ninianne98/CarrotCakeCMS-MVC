using CarrotCake.CMS.Plugins.FAQ2.Models;
using Carrotware.CMS.Interface;
using System;
using System.Web.Mvc;

namespace CarrotCake.CMS.Plugins.FAQ2.Controllers {

	public class HomeController : BaseController {

		public ActionResult Index() {
			return View();
		}

		[WidgetActionSettingModel(typeof(FaqPublic))]
		public ActionResult ShowFaqList() {
			var model = new FaqItems();
			var payload = new FaqPublic();

			if (this.WidgetPayload is FaqPublic) {
				payload = (FaqPublic)this.WidgetPayload;
				payload.LoadData();
			}

			ModelState.Clear();

			if (payload.FaqCategoryID != Guid.Empty) {
				model.Faq = payload.GetFaq();
				model.Items = payload.GetList();
			}

			if (model == null) {
				model = new FaqItems();
			}

			if (string.IsNullOrEmpty(payload.AlternateViewFile)) {
				return PartialView("FaqList", model);
			} else {
				return PartialView(payload.AlternateViewFile, model);
			}
		}

		[WidgetActionSettingModel(typeof(FaqPublic))]
		public ActionResult ShowRandomFaq() {
			var model = new carrot_FaqItem();
			var payload = new FaqPublic();

			if (this.WidgetPayload is FaqPublic) {
				payload = (FaqPublic)this.WidgetPayload;
				payload.LoadData();
			}

			ModelState.Clear();

			if (payload.FaqCategoryID != Guid.Empty) {
				model = payload.GetRandomItem();
			}

			if (model == null) {
				model = new carrot_FaqItem();
				model.FaqCategoryID = payload.FaqCategoryID;
			}

			if (string.IsNullOrEmpty(payload.AlternateViewFile)) {
				return PartialView("FaqItem", model);
			} else {
				return PartialView(payload.AlternateViewFile, model);
			}
		}

		[WidgetActionSettingModel(typeof(FaqPublicTop))]
		public ActionResult ShowFaqTopList() {
			var model = new FaqItems();
			var payload = new FaqPublicTop();

			if (this.WidgetPayload is FaqPublicTop) {
				payload = (FaqPublicTop)this.WidgetPayload;
				payload.LoadData();
			}

			ModelState.Clear();

			if (payload.FaqCategoryID != Guid.Empty) {
				model.Faq = payload.GetFaq();
				model.Items = payload.GetListTop(payload.TakeTop);
			}

			if (string.IsNullOrEmpty(payload.AlternateViewFile)) {
				return PartialView("FaqList", model);
			} else {
				return PartialView(payload.AlternateViewFile, model);
			}
		}
	}
}