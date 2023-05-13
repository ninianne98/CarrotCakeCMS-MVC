using Carrotware.CMS.Interface;
using System;

namespace CarrotCake.CMS.Plugins.CalendarModule {

	public class CalendarViewSettings : WidgetSettings {

		public CalendarViewSettings() {
			this.GenerateCss = true;
			this.SpecifiedCssFile = string.Empty;
		}

		public bool GenerateCss { get; set; }
		public string SpecifiedCssFile { get; set; }
	}
}