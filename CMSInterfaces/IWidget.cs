using System;
using System.Collections.Generic;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Interface {

	public interface IWidget {
		Guid PageWidgetID { get; set; }

		Guid RootContentID { get; set; }

		Guid SiteID { get; set; }

		string WidgetClientID { get; set; }

		bool EnableEdit { get; }

		bool IsBeingEdited { get; set; }

		bool IsDynamicInserted { get; set; }

		Dictionary<string, string> PublicParmValues { get; set; }

		Dictionary<string, string> JSEditFunctions { get; }

		void LoadData();
	}
}