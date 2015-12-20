using CarrotCake.CMS.Plugins.CalendarModule.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarrotCake.CMS.Plugins.CalendarModule.Models {

	public class DateModel {

		public DateModel() {
			this.SelectedDate = DateTime.Now.Date;
			this.Dates = new List<tblCalendar>();
		}

		public DateModel(DateTime date, Guid siteId)
			: this() {
			this.SelectedDate = date;

			using (dbCalendarDataContext db = dbCalendarDataContext.GetDataContext()) {
				this.Dates = (from c in db.tblCalendars
							  where c.EventDate == this.SelectedDate
						  && c.IsActive == true
						  && c.SiteID == siteId
							  orderby c.EventDate
							  select c).ToList();
			}
		}

		public DateTime SelectedDate { get; set; }
		public List<tblCalendar> Dates { get; set; }
	}
}