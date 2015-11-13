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

	public class DashboardInfo {

		public DashboardInfo() { }

		public int Pages { get; set; }
		public int Posts { get; set; }
		public int Tags { get; set; }
		public int Categories { get; set; }

		public int Snippets { get; set; }
	}
}