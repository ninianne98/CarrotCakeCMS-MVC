using System;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class ContentSingleModel {

		public ContentSingleModel() {
			this.Mode = "raw";
		}

		public Guid PageId { get; set; }

		public Guid? WidgetId { get; set; }

		public string Mode { get; set; }

		public string Field { get; set; }

		public string PageText { get; set; }
	}
}