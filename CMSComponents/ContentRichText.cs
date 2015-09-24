using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
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

namespace Carrotware.CMS.UI.Components {

	public class ContentRichText : ContentPlainText {

		public override string ToHtmlString() {
			return SiteData.CurrentSite.UpdateContentRichText(this.RawWidgetData);
		}

		public override Dictionary<string, string> JSEditFunctions {
			get {
				Dictionary<string, string> lst = new Dictionary<string, string>();
				lst.Add("Edit HTML", "cmsShowEditWidgetForm('" + this.PageWidgetID + "', 'html');");
				lst.Add("Edit Text", "cmsShowEditWidgetForm('" + this.PageWidgetID + "', 'plain');");
				return lst;
			}
		}
	}
}