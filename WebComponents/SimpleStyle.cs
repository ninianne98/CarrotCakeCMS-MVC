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

namespace Carrotware.Web.UI.Components {

	public class SimpleStyle {

		public SimpleStyle() { }

		[DefaultValue("")]
		public string CssClass { get; set; }

		[DefaultValue("")]
		public string Style { get; set; }

		public string StyleToString() {
			if (!String.IsNullOrEmpty(this.Style)) {
				return String.Format(" style=\"{0}\" ", this.Style);
			} else {
				return String.Empty;
			}
		}

		public string CssClassToString() {
			if (!String.IsNullOrEmpty(this.CssClass)) {
				return String.Format(" class=\"{0}\" ", this.CssClass);
			} else {
				return String.Empty;
			}
		}

		public virtual string GetHtml() {
			return this.StyleToString() + this.CssClassToString();
		}
	}
}