using Carrotware.CMS.Core;
using System.Collections.Generic;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class WidgetProperties {

		public WidgetProperties() {
			this.Properties = new List<ObjectProperty>();
		}

		public WidgetProperties(Widget widget, List<ObjectProperty> properties) {
			this.Widget = widget;
			this.Properties = properties;
		}

		public Widget Widget { get; set; }

		public List<ObjectProperty> Properties { get; set; }
	}
}