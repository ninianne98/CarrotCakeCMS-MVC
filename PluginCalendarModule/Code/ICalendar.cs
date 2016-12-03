using System;
using System.ComponentModel.DataAnnotations;

namespace CarrotCake.CMS.Plugins.CalendarModule.Code {

	public interface ICalendar {

		[Required]
		Guid? SiteID { get; set; }

		[Required]
		Guid CalendarID { get; set; }

		[Required]
		[Display(Name = "date")]
		DateTime? EventDate { get; set; }

		[Required]
		[Display(Name = "event")]
		string EventTitle { get; set; }

		[Display(Name = "details")]
		string EventDetail { get; set; }

		[Display(Name = "is active")]
		bool? IsActive { get; set; }
	}
}