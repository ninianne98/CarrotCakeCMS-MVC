using CarrotCake.CMS.Plugins.EventCalendarModule.Code;
using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarrotCake.CMS.Plugins.EventCalendarModule.Models {

	public class CalendarViewModel : BaseWidgetModelSettings {

		public CalendarViewModel() : base() {
			this.EncodedSettings = string.Empty;
			this.GenerateCss = true;
			this.MonthSelected = SiteData.CurrentSite.Now.Date;
			this.MonthNext = CalendarHelper.GetEndOfMonthByDate(this.MonthSelected).AddDays(7);
			this.MonthPrior = CalendarHelper.GetFirstOfMonthByDate(this.MonthSelected).AddDays(-7);
			this.MonthDates = new List<vw_carrot_CalendarEvent>();
		}

		public string StyleSheetPath { get; set; }
		public bool GenerateCss { get; set; }
		public DateTime MonthNext { get; set; }
		public DateTime MonthPrior { get; set; }
		public DateTime MonthSelected { get; set; }
		public List<vw_carrot_CalendarEvent> MonthDates { get; set; }
		public List<DateTime> SelectedDates { get; set; }

		public List<carrot_CalendarEventCategory> Colors { get; set; }

		public void LoadData(Guid siteid, bool activeOnly) {
			var first = CalendarHelper.GetFirstOfMonthByDate(this.MonthSelected).Date;

			DateTime dtStart = CalendarHelper.GetFirstOfMonthByDate(first).AddMinutes(-15);
			DateTime dtEnd = CalendarHelper.GetEndOfMonthByDate(first).AddMinutes(15);

			this.MonthSelected = first.Date;
			this.MonthNext = dtEnd.AddDays(3);
			this.MonthPrior = dtStart.AddDays(-3);

			var events = CalendarHelper.GetDisplayEvents(siteid, dtStart, dtEnd, -1, activeOnly).ToList();

			this.SelectedDates = (from dd in events select dd.EventDate.Date).Distinct().ToList();

			events = CalendarHelper.MassageDateTime(events);

			this.MonthDates = events;

			var catIds = events.Select(x => x.CalendarEventCategoryID).Distinct();

			this.Colors = CalendarHelper.GetCalendarCategories(siteid).Where(x => catIds.Contains(x.CalendarEventCategoryID)).ToList();
		}

		public void SetSettings(CalendarDisplaySettings obj) {
			if (obj != null) {
				CalendarViewSettings settings = ConvertSettings(obj);
				base.Persist(settings);
			}
		}

		public void SetSettings(CalendarSimpleSettings obj) {
			if (obj != null) {
				CalendarViewSettings settings = ConvertSettings(obj);
				base.Persist(settings);
			}
		}

		public void AssignSettings(CalendarViewSettings settings) {
			if (settings != null) {
				this.GenerateCss = settings.GenerateCss;
				this.StyleSheetPath = settings.SpecifiedCssFile;
			}

			this.Persist(settings);
		}

		public CalendarViewSettings ConvertSettings(CalendarDisplaySettings obj) {
			var settings = new CalendarViewSettings();

			settings.SettingsFromWidget(obj);

			return settings;
		}

		public CalendarViewSettings ConvertSettings(CalendarSimpleSettings obj) {
			var settings = new CalendarViewSettings();

			settings.SettingsFromWidget(obj);

			settings.GenerateCss = false;
			settings.SpecifiedCssFile = string.Empty;

			return settings;
		}

		public CalendarViewSettings GetSettings() {
			var settings = new CalendarViewSettings();
			var tmp = base.Restore<CalendarViewSettings>();

			if (tmp is CalendarViewSettings) {
				settings = (CalendarViewSettings)tmp;
			}

			this.AssignSettings(settings);

			return settings;
		}
	}
}