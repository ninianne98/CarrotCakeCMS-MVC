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

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class SiteTemplateUpdateModel {
		public string HomePageLink { get; set; }
		public string HomePageTitle { get; set; }
		public string HomePage { get; set; }
		public string AllContent { get; set; }
		public string TopPages { get; set; }
		public string SubPages { get; set; }

		public Guid? IndexPageID { get; set; }
		public string IndexPage { get; set; }
		public string BlogPages { get; set; }

		public string AllPages { get; set; }
	}
}