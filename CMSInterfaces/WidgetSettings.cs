using System;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Interface {

	public class WidgetSettings : IWidgetSettings {

		public WidgetSettings() { }

		public Guid PageWidgetID { get; set; }

		public Guid RootContentID { get; set; }

		public Guid SiteID { get; set; }

		public virtual string WidgetClientID { get; set; }

		public bool IsBeingEdited { get; set; }

		public bool IsDynamicInserted { get; set; }

		public virtual string AlternateViewFile { get; set; }

		public virtual void SettingsFromWidget(object widgetObject) {
			if (widgetObject != null) {
				if (widgetObject is IWidget) {
					var widget = (IWidget)widgetObject;

					this.SiteID = widget.SiteID;
					this.WidgetClientID = widget.WidgetClientID;
					this.RootContentID = widget.RootContentID;
					this.PageWidgetID = widget.PageWidgetID;
					this.IsBeingEdited = widget.IsBeingEdited;
					this.IsDynamicInserted = widget.IsDynamicInserted;
				}

				if (widgetObject is IWidgetView) {
					var widget = (IWidgetView)widgetObject;
					this.AlternateViewFile = widget.AlternateViewFile;
				}
			}
		}
	}
}