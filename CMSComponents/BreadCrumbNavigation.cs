using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

	public class BreadCrumbNavigation : BaseWebComponent, ICmsMainComponent {

		public BreadCrumbNavigation() {
			this.DisplayAsList = false;
			this.TextDivider = "&gt;";
			this.CssSelected = "selected";
			this.CssWrapper = string.Empty;
			this.CssClass = string.Empty;
			this.ElementId = "breadcrumb";
		}

		public string ElementId { get; set; }
		public string CssClass { get; set; }
		public string CssSelected { get; set; }
		public string CssWrapper { get; set; }
		public bool DisplayAsList { get; set; }
		public string TextDivider { get; set; }

		public ContentPage ContentPage { get; set; }

		public override string GetHtml() {
			var output = new StringBuilder();
			var lstNav = new List<SiteNav>();

			var pageNav = this.ContentPage.GetSiteNav();
			string currentPageFile = pageNav.FileName.ToLowerInvariant();

			using (var navHelper = SiteNavFactory.GetSiteNavHelper()) {
				if (SiteData.CurrentSiteExists && SiteData.CurrentSite.Blog_Root_ContentID.HasValue &&
					pageNav.ContentType == ContentPageType.PageType.BlogEntry) {
					lstNav = navHelper.GetPageCrumbNavigation(SiteData.CurrentSiteID, SiteData.CurrentSite.Blog_Root_ContentID.Value, !SecurityData.IsAuthEditor);

					if (lstNav != null && lstNav.Any()) {
						pageNav.NavOrder = lstNav.Max(x => x.NavOrder) + 100;
						lstNav.Add(pageNav);
					}
				} else {
					lstNav = navHelper.GetPageCrumbNavigation(SiteData.CurrentSiteID, pageNav.Root_ContentID, !SecurityData.IsAuthEditor);
				}
			}

			lstNav = ControlUtilities.TweakData(lstNav);

			var outerTag = new HtmlTag("ul");
			outerTag.SetAttribute("id", this.ElementId);
			outerTag.MergeAttribute("class", this.CssClass);

			if (this.DisplayAsList) {
				outerTag = new HtmlTag("ul");

				output.AppendLine(outerTag.OpenTag());

				foreach (SiteNav c in lstNav) {
					var item = new HtmlTag("li");
					var link = new HtmlTag("a");

					item.MergeAttribute("class", this.CssWrapper);
					link.Uri = c.FileName;
					link.InnerHtml = c.NavMenuText;

					if (SiteData.IsFilenameCurrentPage(c.FileName) || ControlUtilities.AreFilenamesSame(c.FileName, currentPageFile)) {
						item.MergeAttribute("class", this.CssSelected);
						item.InnerHtml = c.NavMenuText;
					} else {
						item.InnerHtml = link.RenderTag();
					}

					output.AppendLine(item.RenderTag());
				}

				output.AppendLine(outerTag.CloseTag());
			} else {
				outerTag = new HtmlTag("div");

				output.AppendLine(outerTag.OpenTag());

				foreach (SiteNav c in lstNav) {
					var item = new HtmlTag("span");
					var link = new HtmlTag("a");

					item.MergeAttribute("class", this.CssWrapper);
					link.Uri = c.FileName;
					link.InnerHtml = c.NavMenuText;

					if (SiteData.IsFilenameCurrentPage(c.FileName) || ControlUtilities.AreFilenamesSame(c.FileName, currentPageFile)) {
						item.MergeAttribute("class", this.CssSelected);
						item.InnerHtml = c.NavMenuText;
					} else {
						item.InnerHtml = link.RenderTag() + string.Format(" {0} ", this.TextDivider);
					}

					output.AppendLine(item.RenderTag());
				}

				output.AppendLine(outerTag.CloseTag());
			}

			return ControlUtilities.HtmlFormat(output);
		}
	}
}