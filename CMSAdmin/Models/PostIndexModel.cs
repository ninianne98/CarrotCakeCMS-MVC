using Carrotware.CMS.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

	public class PostIndexModel : PageIndexModel {

		public PostIndexModel()
			: base() {
			this.SearchDate = SiteData.CurrentSite.Now.Date;
			this.SelectedRange = 7;
			this.DateRanges = new Dictionary<int, string>();
			this.DateRanges.Add(1, "1 Days +/-");
			this.DateRanges.Add(7, "7 Days +/-");
			this.DateRanges.Add(30, "30 Days +/-");
			this.DateRanges.Add(60, "60 Days +/-");
			this.DateRanges.Add(90, "90 Days +/-");
			this.DateRanges.Add(120, "120 Days +/-");
		}

		[Required]
		public DateTime SearchDate { get; set; }

		public int SelectedRange { get; set; }

		public Dictionary<int, string> DateRanges { get; set; }
	}
}