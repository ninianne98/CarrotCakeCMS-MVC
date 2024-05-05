using Carrotware.CMS.Interface;
using System;
using System.ComponentModel;

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

	public class CalendarDisplaySettings : WidgetActionSettingModel {

		public CalendarDisplaySettings()
			: base() {
			this.GenerateCss = true;
		}

		public override bool EnableEdit {
			get {
				return true;
			}
		}

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		[Description("Specify CSS file to load")]
		public string SpecifiedCssFile { get; set; }

		[Widget(WidgetAttribute.FieldMode.CheckBox)]
		[Description("Auto generate CSS for calendar")]
		public bool GenerateCss { get; set; }

		public override void LoadData() {
			base.LoadData();

			this.GenerateCss = Convert.ToBoolean(this.GetParmValue("GenerateCss", "true"));
			this.SpecifiedCssFile = this.GetParmValue("SpecifiedCssFile", string.Empty);
		}
	}
}