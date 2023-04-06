using Carrotware.CMS.Interface;
using System;
using System.Web;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.UI.Components {

	public abstract class BaseToolboxComponent : WidgetBase, IHtmlString {

		public virtual string ToHtmlString() {
			return String.Empty;
		}

		public override bool EnableEdit {
			get { return false; }
		}
	}
}