using Carrotware.Web.UI.Components;
using System;
using System.Text;
using System.Web;

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

	public abstract class BaseTwoPartCmsComponent : BaseCmsComponent, ICmsComponent, ITwoPartWebComponent {

		public virtual string GetBody() {
			TweakData();

			StringBuilder output = new StringBuilder();
			return WriteTopLevel(output).ToString();
		}

		public virtual string GetHead() {
			return String.Empty;
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