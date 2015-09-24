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

namespace Carrotware.Web.UI.Components {

	public interface ITwoPartWebComponent {

		string GetBody();

		string GetHead();

		HtmlString RenderBody();

		HtmlString RenderHead();
	}
}