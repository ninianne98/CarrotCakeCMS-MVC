using Carrotware.CMS.Interface;
using System;
using System.ComponentModel;

namespace CarrotCake.CMS.Plugins.CalendarModule {

	public class CalendarUpcomingSettings : WidgetActionSettingModel {

		public CalendarUpcomingSettings() {
			this.DaysInPast = -3;
			this.DaysInFuture = 30;
			this.CalendarPageUri = String.Empty;
		}

		public override bool EnableEdit {
			get {
				return true;
			}
		}

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		[Description("Days in past")]
		public int DaysInPast { get; set; }

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		[Description("Days in future")]
		public int DaysInFuture { get; set; }

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		[Description("Calendar page uri")]
		public string CalendarPageUri { get; set; }

		public override void LoadData() {
			base.LoadData();

			this.DaysInPast = Convert.ToInt32(this.GetParmValue("DaysInPast", "-3"));
			this.DaysInFuture = Convert.ToInt32(this.GetParmValue("DaysInFuture", "30"));
			this.CalendarPageUri = this.GetParmValue("CalendarPageUri", String.Empty);
		}
	}
}