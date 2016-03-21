using CarrotCake.CMS.Plugins.FAQ2.Models;
using Carrotware.CMS.Interface;
using Carrotware.CMS.Interface.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CarrotCake.CMS.Plugins.FAQ2.Controllers {

	public class HomeController : BaseDataWidgetController {

		public ActionResult Index() {
			return View();
		}

		[WidgetActionSettingModel(typeof(FaqPublic))]
		public ActionResult ShowFaqList() {
			FaqItems model = new FaqItems();
			FaqPublic payload = new FaqPublic();

			if (this.WidgetPayload is FaqPublic) {
				payload = (FaqPublic)this.WidgetPayload;
				payload.LoadData();
			}

			ModelState.Clear();

			if (payload.FaqCategoryID != Guid.Empty) {
				model.Faq = payload.GetFaq();
				model.Items = payload.GetList();
			}

			if (String.IsNullOrEmpty(payload.AlternateViewFile)) {
				return PartialView("FaqList", model);
			} else {
				return PartialView(payload.AlternateViewFile, model);
			}
		}

		[WidgetActionSettingModel(typeof(FaqPublic))]
		public ActionResult ShowRandomFaq() {
			carrot_FaqItem model = new carrot_FaqItem();
			FaqPublic payload = new FaqPublic();

			if (this.WidgetPayload is FaqPublic) {
				payload = (FaqPublic)this.WidgetPayload;
				payload.LoadData();
			}

			ModelState.Clear();

			if (payload.FaqCategoryID != Guid.Empty) {
				model = payload.GetRandomItem();
			}

			if (String.IsNullOrEmpty(payload.AlternateViewFile)) {
				return PartialView("FaqItem", model);
			} else {
				return PartialView(payload.AlternateViewFile, model);
			}
		}

		[WidgetActionSettingModel(typeof(FaqPublicTop))]
		public ActionResult ShowFaqTopList() {
			FaqItems model = new FaqItems();
			FaqPublicTop payload = new FaqPublicTop();

			if (this.WidgetPayload is FaqPublicTop) {
				payload = (FaqPublicTop)this.WidgetPayload;
				payload.LoadData();
			}

			ModelState.Clear();

			if (payload.FaqCategoryID != Guid.Empty) {
				model.Faq = payload.GetFaq();
				model.Items = payload.GetListTop(payload.TakeTop);
			}

			if (String.IsNullOrEmpty(payload.AlternateViewFile)) {
				return PartialView("FaqList", model);
			} else {
				return PartialView(payload.AlternateViewFile, model);
			}
		}
	}
}