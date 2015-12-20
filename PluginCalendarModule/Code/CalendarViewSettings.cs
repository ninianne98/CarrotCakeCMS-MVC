using System;

namespace CarrotCake.CMS.Plugins.CalendarModule {

	public class CalendarViewSettings {

		public CalendarViewSettings() {
			this.GenerateCss = true;
		}

		public Guid SiteID { get; set; }
		public string AlternateViewFile { get; set; }
		public string SpecifiedCssFile { get; set; }
		public bool GenerateCss { get; set; }
	}
}