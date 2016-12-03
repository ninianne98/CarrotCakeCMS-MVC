using CarrotCake.CMS.Plugins.CalendarModule.Code;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarrotCake.CMS.Plugins.CalendarModule.Models {

	[MetadataType(typeof(ICalendar))]
	public class CalendarDetail : ICalendar {

		public CalendarDetail() { }

		public CalendarDetail(tblCalendar model) {
			if (model != null) {
				this.SiteID = model.SiteID;
				this.CalendarID = model.CalendarID;
				this.EventDate = model.EventDate ?? DateTime.Now.Date;
				this.EventTitle = model.EventTitle ?? "(No Title)";
				this.EventDetail = model.EventDetail;
				this.IsActive = model.IsActive ?? false;
			}
		}

		public Guid? SiteID { get; set; }

		public Guid CalendarID { get; set; }

		public DateTime? EventDate { get; set; }

		public string EventTitle { get; set; }

		public string EventDetail { get; set; }

		public bool? IsActive { get; set; }
	}
}