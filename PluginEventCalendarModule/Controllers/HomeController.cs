using CarrotCake.CMS.Plugins.EventCalendarModule.Code;
using CarrotCake.CMS.Plugins.EventCalendarModule.Models;
using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
using System;
using System.Web.Mvc;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace CarrotCake.CMS.Plugins.EventCalendarModule.Controllers {

	public class HomeController : BaseController {
		private CalendarDataContext _db = CalendarDataContext.GetDataContext();

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			if (_db != null) {
				_db.Dispose();
			}
		}

		[WidgetActionSettingModel(typeof(CalendarUpcomingSettings))]
		public ActionResult CalendarUpcoming() {
			SiteData site = SiteData.CurrentSite;

			var payload = new CalendarUpcomingSettings();

			if (this.WidgetPayload is CalendarUpcomingSettings) {
				payload = (CalendarUpcomingSettings)this.WidgetPayload;
				payload.LoadData();
			}

			ViewBag.CalendarPageUri = payload.CalendarPageUri;

			DateTime dtStart = site.Now.Date.AddDays(payload.DaysInPast);
			DateTime dtEnd = site.Now.Date.AddDays(payload.DaysInFuture);

			var model = CalendarHelper.GetDisplayEvents(payload.SiteID, dtStart, dtEnd, payload.TakeTop, true);

			model = CalendarHelper.MassageDateTime(model);

			if (string.IsNullOrEmpty(payload.AlternateViewFile)) {
				return PartialView(model);
			} else {
				return PartialView(payload.AlternateViewFile, model);
			}
		}

		[HttpGet]
		[WidgetActionSettingModel(typeof(CalendarDisplaySettings))]
		public ActionResult EventCalendarDisplay() {
			CalendarViewModel model = new CalendarViewModel();

			return EventCalendarDisplay(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[WidgetActionSettingModel(typeof(CalendarDisplaySettings))]
		public ActionResult EventCalendarDisplay(CalendarViewModel model) {
			var settings = new CalendarViewSettings();
			var payload = new CalendarDisplaySettings();

			if (string.IsNullOrWhiteSpace(model.EncodedSettings)) {
				if (this.WidgetPayload is CalendarDisplaySettings) {
					payload = (CalendarDisplaySettings)this.WidgetPayload;
					payload.LoadData();
				}
				settings = model.ConvertSettings(payload);
			} else {
				settings = model.GetSettings();
			}

			model.AssignSettings(settings);

			ModelState.Clear();

			model.LoadData(settings.SiteID, true);

			if (string.IsNullOrEmpty(settings.AlternateViewFile)) {
				return PartialView(model);
			} else {
				return PartialView(settings.AlternateViewFile, model);
			}
		}

		[HttpGet]
		[WidgetActionSettingModel(typeof(CalendarSimpleSettings))]
		public ActionResult EventCalendarDisplay2() {
			CalendarViewModel model = new CalendarViewModel();

			return EventCalendarDisplay2(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[WidgetActionSettingModel(typeof(CalendarSimpleSettings))]
		public ActionResult EventCalendarDisplay2(CalendarViewModel model) {
			var settings = new CalendarViewSettings();
			var payload = new CalendarSimpleSettings();

			if (string.IsNullOrWhiteSpace(model.EncodedSettings)) {
				if (this.WidgetPayload is CalendarSimpleSettings) {
					payload = (CalendarSimpleSettings)this.WidgetPayload;
					payload.LoadData();
				}
				settings = model.ConvertSettings(payload);
			} else {
				settings = model.GetSettings();
			}

			model.AssignSettings(settings);

			ModelState.Clear();

			model.LoadData(settings.SiteID, true);

			if (string.IsNullOrEmpty(settings.AlternateViewFile)) {
				return PartialView(model);
			} else {
				return PartialView(settings.AlternateViewFile, model);
			}
		}

		public ActionResult Index() {
			return View();
		}
	}
}