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

namespace Carrotware.CMS.UI.Components {

	public interface ICmsComponent {
		string CssClass { get; set; }
		string CssHasChildren { get; set; }
		string CssItem { get; set; }
		string CssSelected { get; set; }
		string CssULClassLower { get; set; }
		string CssULClassTop { get; set; }
		string ElementId { get; set; }

		System.Collections.Generic.List<Carrotware.CMS.Core.SiteNav> GetChildren(Guid rootContentID);

		string GetHtml();

		System.Collections.Generic.List<Carrotware.CMS.Core.SiteNav> GetTopNav();

		bool MultiLevel { get; }
		System.Collections.Generic.List<Carrotware.CMS.Core.SiteNav> NavigationData { get; set; }
	}
}