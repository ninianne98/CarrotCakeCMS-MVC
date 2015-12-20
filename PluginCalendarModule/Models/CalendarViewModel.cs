using CarrotCake.CMS.Plugins.CalendarModule.Code;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace CarrotCake.CMS.Plugins.CalendarModule.Models {

	public class CalendarViewModel {

		public CalendarViewModel() {
			this.SerialSettings = String.Empty;
			this.GenerateCss = true;
			this.AlternateViewFile = String.Empty;
			this.MonthSelected = DateTime.Now.Date;
			this.MonthNext = this.MonthSelected.AddMonths(1);
			this.MonthPrior = this.MonthSelected.AddMonths(-1);
			this.MonthDates = new List<tblCalendar>();
		}

		public Guid SiteID { get; set; }
		public string AlternateViewFile { get; set; }
		public string StyleSheetPath { get; set; }
		public bool GenerateCss { get; set; }

		public DateTime MonthNext { get; set; }
		public DateTime MonthPrior { get; set; }
		public DateTime MonthSelected { get; set; }
		public List<tblCalendar> MonthDates { get; set; }
		public List<DateTime> SelectedDates { get; set; }

		public void LoadData(Guid siteid, bool activeOnly) {
			this.SiteID = siteid;

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

		public string SerialSettings { get; set; }

		public void SetSettings(CalendarDisplaySettings obj) {
			string sXML = String.Empty;

			if (obj != null) {
				CalendarViewSettings settings = ConvertSettings(obj);

				XmlSerializer xmlSerializer = new XmlSerializer(typeof(CalendarViewSettings));

				using (StringWriter stringWriter = new StringWriter()) {
					xmlSerializer.Serialize(stringWriter, settings);
					sXML = stringWriter.ToString();
					sXML = Utils.EncodeBase64(sXML);
				}
			}

			this.SerialSettings = sXML;
		}

		public void AssignSettings(CalendarViewSettings settings) {
			if (settings != null) {
				this.SiteID = settings.SiteID;
				this.AlternateViewFile = settings.AlternateViewFile;
				this.GenerateCss = settings.GenerateCss;
				this.StyleSheetPath = settings.SpecifiedCssFile;
			}
		}

		public CalendarViewSettings ConvertSettings(CalendarDisplaySettings obj) {
			CalendarViewSettings settings = new CalendarViewSettings();

			if (obj != null) {
				settings.SiteID = obj.SiteID;
				settings.AlternateViewFile = obj.AlternateViewFile;
				settings.GenerateCss = obj.GenerateCss;
				settings.SpecifiedCssFile = obj.SpecifiedCssFile;
			}

			return settings;
		}

		public CalendarViewSettings GetSettings() {
			CalendarViewSettings settings = new CalendarViewSettings();

			if (!String.IsNullOrEmpty(this.SerialSettings)) {
				string sXML = Utils.DecodeBase64(this.SerialSettings);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(CalendarViewSettings));
				using (StringReader stringReader = new StringReader(sXML)) {
					settings = (CalendarViewSettings)xmlSerializer.Deserialize(stringReader);
				}
			}

			return settings;
		}
	}
}