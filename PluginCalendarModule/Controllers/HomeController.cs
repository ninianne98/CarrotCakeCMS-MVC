using CarrotCake.CMS.Plugins.CalendarModule.Code;
using CarrotCake.CMS.Plugins.CalendarModule.Models;
using Carrotware.CMS.Interface;
using Carrotware.CMS.Interface.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CarrotCake.CMS.Plugins.CalendarModule.Controllers {

	public class HomeController : BaseDataWidgetController {
		private dbCalendarDataContext db = dbCalendarDataContext.GetDataContext();

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			if (db != null) {
				db.Dispose();
			}
		}

		public ActionResult Index() {
			return View();
		}

		[HttpGet]
		public ActionResult CalendarDisplay() {
			CalendarViewModel model = new CalendarViewModel();

			return CalendarDisplay(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[WidgetActionSettingModel("CarrotCake.CMS.Plugins.CalendarModule.CalendarDisplaySettings, CarrotCake.CMS.Plugins.CalendarModule")]
		public ActionResult CalendarDisplay(CalendarViewModel model) {
			CalendarViewSettings settings = new CalendarViewSettings();

			if (!String.IsNullOrEmpty(model.SerialSettings)) {
				settings = model.GetSettings();

				model.AssignSettings(settings);
			} else {
				CalendarDisplaySettings payload = new CalendarDisplaySettings();

				if (this.WidgetPayload is CalendarDisplaySettings) {
					payload = (CalendarDisplaySettings)this.WidgetPayload;
					payload.LoadData();

					settings = model.ConvertSettings(payload);
					model.AssignSettings(settings);

					model.SetSettings(payload);
				}
			}

			ModelState.Clear();

			model.LoadData(settings.SiteID, true);

			if (String.IsNullOrEmpty(settings.AlternateViewFile)) {
				return PartialView(model);
			} else {
				return PartialView(settings.AlternateViewFile, model);
			}
		}

		[HttpGet]
		public ActionResult CalendarDisplaySimple() {
			CalendarViewModel model = new CalendarViewModel();

			return CalendarDisplaySimple(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[WidgetActionSettingModel("CarrotCake.CMS.Plugins.CalendarModule.CalendarSimpleSettings, CarrotCake.CMS.Plugins.CalendarModule")]
		public ActionResult CalendarDisplaySimple(CalendarViewModel model) {
			CalendarSimpleSettings settings = new CalendarSimpleSettings();

			if (this.WidgetPayload is CalendarSimpleSettings) {
				settings = (CalendarSimpleSettings)this.WidgetPayload;
				settings.LoadData();

				model.SiteID = settings.SiteID;
				model.AlternateViewFile = settings.AlternateViewFile;
			}

			ModelState.Clear();

			model.LoadData(model.SiteID, true);

			if (String.IsNullOrEmpty(model.AlternateViewFile)) {
				return PartialView(model);
			} else {
				return PartialView(model.AlternateViewFile, model);
			}
		}

		[WidgetActionSettingModel("CarrotCake.CMS.Plugins.CalendarModule.CalendarUpcomingSettings, CarrotCake.CMS.Plugins.CalendarModule")]
		public ActionResult CalendarUpcoming() {
			CalendarUpcomingSettings payload = new CalendarUpcomingSettings();

			if (this.WidgetPayload is CalendarUpcomingSettings) {
				payload = (CalendarUpcomingSettings)this.WidgetPayload;
				payload.LoadData();
			}

			ViewBag.CalendarPageUri = payload.CalendarPageUri;

			DateTime dtStart = DateTime.Now.Date.AddDays(payload.DaysInPast);
			DateTime dtEnd = DateTime.Now.Date.AddDays(payload.DaysInFuture);

			List<tblCalendar> model = (from c in db.tblCalendars
									   where c.EventDate >= dtStart
										&& c.EventDate < dtEnd
										&& c.IsActive == true
										&& c.SiteID == payload.SiteID
									   orderby c.EventDate ascending
									   select c).ToList();

			if (String.IsNullOrEmpty(payload.AlternateViewFile)) {
				return PartialView(model);
			} else {
				return PartialView(payload.AlternateViewFile, model);
			}
		}

		[WidgetActionSettingModel("CarrotCake.CMS.Plugins.CalendarModule.CalendarSimpleSettings, CarrotCake.CMS.Plugins.CalendarModule")]
		public ActionResult CalendarDateInfo(DateTime? calendardate) {
			DateTime theEventDate = calendardate ?? DateTime.Now.Date;

			CalendarSimpleSettings payload = new CalendarSimpleSettings();

			if (this.WidgetPayload is CalendarSimpleSettings) {
				payload = (CalendarSimpleSettings)this.WidgetPayload;
				payload.LoadData();
			}

			DateModel model = new DateModel(theEventDate, payload.SiteID);

			if (String.IsNullOrEmpty(payload.AlternateViewFile)) {
				return PartialView(model);
			} else {
				return PartialView(payload.AlternateViewFile, model);
			}
		}
	}
}