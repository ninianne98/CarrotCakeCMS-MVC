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
	}
}