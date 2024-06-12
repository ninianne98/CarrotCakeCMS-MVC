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

		public override void SettingsFromWidget(object widgetObject) {
			base.SettingsFromWidget(widgetObject);

			if (widgetObject != null) {
				if (widgetObject is CalendarDisplaySettings) {
					var widget = (CalendarDisplaySettings)widgetObject;

					this.GenerateCss = widget.GenerateCss;
					this.SpecifiedCssFile = widget.SpecifiedCssFile;
				}
			}
		}

		public bool GenerateCss { get; set; }
		public string SpecifiedCssFile { get; set; }
	}
}