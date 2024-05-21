using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
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

namespace Carrotware.CMS.UI.Components {

	public class AdminScriptInfo : BaseWebComponent {

		public AdminScriptInfo() { }

		public override string GetHtml() {
			var versionKey = string.Format("cms={0}", SiteData.CurrentDLLVersion);
			var tag = new HtmlTag(HtmlTag.EasyTag.JavaScript);
			var key = SecurityData.IsAuthenticated ? DateTime.UtcNow.Ticks.ToString().Substring(0, 8) : CarrotWeb.DateKey();

			tag.Uri = CarrotCakeHtml.AdminScriptValues + "?ts=" + key + (SecurityData.IsAuthenticated ? ("&a=true&" + versionKey) : string.Empty);

			return tag.ToString();
		}
	}
}