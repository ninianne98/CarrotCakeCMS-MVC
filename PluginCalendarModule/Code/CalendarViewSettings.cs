using Carrotware.CMS.Interface;

namespace CarrotCake.CMS.Plugins.CalendarModule {

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