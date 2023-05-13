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

	public interface IWidgetSettings {
		Guid PageWidgetID { get; set; }

		Guid RootContentID { get; set; }

		Guid SiteID { get; set; }

		string WidgetClientID { get; set; }

		bool IsBeingEdited { get; set; }

		bool IsDynamicInserted { get; set; }

		string AlternateViewFile { get; set; }
	}
}