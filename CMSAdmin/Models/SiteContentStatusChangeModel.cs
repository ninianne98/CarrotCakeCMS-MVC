using Carrotware.CMS.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

	public class SiteContentStatusChangeModel {

		public SiteContentStatusChangeModel() {
			this.PerformSave = false;
			this.SearchDate = SiteData.CurrentSite.Now.Date;
			this.SelectedRange = 3;

			this.DateRanges = new Dictionary<int, string>();
			this.DateRanges.Add(1, "1 Days +/-");
			this.DateRanges.Add(3, "3 Days +/-");
			this.DateRanges.Add(7, "7 Days +/-");
			this.DateRanges.Add(30, "30 Days +/-");
			this.DateRanges.Add(60, "60 Days +/-");
			this.DateRanges.Add(90, "90 Days +/-");
			this.DateRanges.Add(120, "120 Days +/-");

			this.FilterOptions = Helper.CreateBoolFilter();

			this.ActionOptions = new Dictionary<string, string>();
			this.ActionOptions.Add("active", "show publicly");
			this.ActionOptions.Add("inactive", "do NOT show publicly **");
			this.ActionOptions.Add("navigation", "include in site navigation");
			this.ActionOptions.Add("navigation-no", "EXCLUDE from site navigation **");
			this.ActionOptions.Add("sitemap", "include in sitemap");
			this.ActionOptions.Add("sitemap-no", "EXCLUDE from sitemap **");
			this.ActionOptions.Add("searchengine", "allow search engine indexing");
			this.ActionOptions.Add("searchengine-no", "BLOCK search engine indexing **");

			this.UseDate = true;
			this.PageType = ContentPageType.PageType.Unknown;

			this.Pages = new List<ContentPage>();
		}

		public DateTime SearchDate { get; set; }

		public int SelectedRange { get; set; }
		public Dictionary<int, string> DateRanges { get; set; }

		public string SelectedAction { get; set; }
		public Dictionary<string, string> ActionOptions { get; set; }

		public bool UseDate { get; set; }

		[Display(Name = "In Site Map")]
		public bool? ShowInSiteMap { get; set; }

		[Display(Name = "Block")]
		public bool? BlockIndex { get; set; }

		[Display(Name = "Active")]
		public bool? PageActive { get; set; }

		[Display(Name = "In Site Nav")]
		public bool? ShowInSiteNav { get; set; }

		public Dictionary<bool, string> FilterOptions { get; set; }

		public ContentPageType.PageType PageType { get; set; }

		public List<ContentPage> Pages { get; set; }

		public bool PerformSave { get; set; }
	}
}