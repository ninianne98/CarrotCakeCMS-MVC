﻿using System;
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

namespace Carrotware.Web.UI.Components {

	public abstract class BaseTwoPartWebComponent : BaseWebComponent, ITwoPartWebComponent {

		public virtual string GetBody() {
			return string.Empty;
		}

		public virtual string GetHead() {
			return string.Empty;
		}

		public virtual HtmlString RenderBody() {
			return new HtmlString(GetBody());
		}

		public virtual HtmlString RenderHead() {
			return new HtmlString(GetHead());
		}

		public override string GetHtml() {
			return GetHead() + Environment.NewLine + GetBody();
		}
	}
}