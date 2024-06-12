using CarrotCake.CMS.Plugins.CalendarModule.Code;
using Carrotware.CMS.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarrotCake.CMS.Plugins.CalendarModule.Models {

	public class CalendarViewModel : BaseWidgetModelSettings {

		public CalendarViewModel() : base() {
			this.EncodedSettings = string.Empty;
			this.GenerateCss = true;
			this.MonthSelected = DateTime.Now.Date;
			this.MonthNext = this.MonthSelected.AddMonths(1);
			this.MonthPrior = this.MonthSelected.AddMonths(-1);
			this.MonthDates = new List<tblCalendar>();
		}

		public string StyleSheetPath { get; set; }
		public bool GenerateCss { get; set; }
		public DateTime MonthNext { get; set; }
		public DateTime MonthPrior { get; set; }
		public DateTime MonthSelected { get; set; }
		public List<tblCalendar> MonthDates { get; set; }
		public List<DateTime> SelectedDates { get; set; }

		public void LoadData(Guid siteid, bool activeOnly) {
			DateTime dtStart = this.MonthSelected.AddDays(1 - this.MonthSelected.Day);
			DateTime dtEnd = dtStart.AddMonths(1);

			this.MonthSelected = dtStart;
			this.MonthNext = this.MonthSelected.AddMonths(1);
			this.MonthPrior = this.MonthSelected.AddMonths(-1);

			using (dbCalendarDataContext db = dbCalendarDataContext.GetDataContext()) {
				var lst = (from c in db.tblCalendars
						   where c.EventDate >= dtStart
									&& c.EventDate < dtEnd
									&& c.SiteID == siteid
									&& ((c.IsActive ?? false == true) || !activeOnly)
						   orderby c.EventDate
						   select c).ToList();

				List<DateTime> dates = (from dd in lst
										select Convert.ToDateTime(dd.EventDate).Date).Distinct().ToList();

				this.SelectedDates = dates;

				this.MonthDates = lst;
			}
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