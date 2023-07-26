using Carrotware.CMS.Interface;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace CarrotCake.CMS.Plugins.EventCalendarModule {

	public class CalendarViewSettings : WidgetSettings {

		public CalendarViewSettings() {
			this.GenerateCss = true;
			this.SpecifiedCssFile = string.Empty;
		}

		public bool GenerateCss { get; set; }
		public string SpecifiedCssFile { get; set; }
	}
}