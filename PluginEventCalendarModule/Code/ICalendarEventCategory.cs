using System;
using System.ComponentModel.DataAnnotations;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace CarrotCake.CMS.Plugins.EventCalendarModule.Code {

	public interface ICalendarEventCategory {

		[Required]
		string CategoryName { get; set; }

		Guid CalendarEventCategoryID { get; set; }

		[Required]
		string CategoryBGColor { get; set; }

		[Required]
		string CategoryFGColor { get; set; }

		Guid SiteID { get; set; }
	}
}