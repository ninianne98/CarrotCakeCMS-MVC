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

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class SitePageDrillDownModel {

		public SitePageDrillDownModel() {
			this.SelectedPageID = Guid.Empty;
		}

		public Guid? SelectedPageID { get; set; }

		public Guid CurrentPageID { get; set; }

		public string FieldName { get; set; }

		public string FieldID { get { return this.FieldName.Replace(".", "_").Replace("[", "_").Replace("]", "_"); } }
	}
}