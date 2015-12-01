using System;
using System.Web.Mvc;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public interface IContentController {
		string TemplateFile { get; set; }

		ActionResult Default();

		ActionResult Default(FormCollection model);

		ActionResult RSSFeed(string type);

		ActionResult SiteMap();

		int WidgetCount { get; set; }
	}
}