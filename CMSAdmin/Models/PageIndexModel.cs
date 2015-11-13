using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;

using System.Linq;

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

	public class PageIndexModel {

		public PageIndexModel() {
			this.Page = new PagedData<ContentPage>();

			this.PageSizes = new List<int>();
			this.PageSizes.Add(10);
			this.PageSizes.Add(25);
			this.PageSizes.Add(50);
			this.PageSizes.Add(100);

			this.SelectedSearch = SearchBy.AllPages;
		}

		public enum SearchBy {
			AllPages,
			Filtered
		}

		public SearchBy SelectedSearch { get; set; }

		public PagedData<ContentPage> Page { get; set; }

		public List<int> PageSizes { get; set; }

		public Guid? ParentPageID { get; set; }
	}
}