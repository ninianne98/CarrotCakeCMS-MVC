using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

	public class BreadCrumbNavigation : BaseWebComponent, ICmsMainComponent {

		public BreadCrumbNavigation() {
			this.DisplayAsList = false;
			this.TextDivider = "&gt;";
			this.CssSelected = "selected";
			this.CssWrapper = String.Empty;
			this.CssClass = String.Empty;
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
			List<SiteNav> lstNav = new List<SiteNav>();
			StringBuilder sb = new StringBuilder();
			SiteNav pageNav = this.ContentPage.GetSiteNav();
			string sParent = pageNav.FileName.ToLower();

			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				if (SiteData.CurretSiteExists && SiteData.CurrentSite.Blog_Root_ContentID.HasValue &&
					pageNav.ContentType == ContentPageType.PageType.BlogEntry) {
					lstNav = navHelper.GetPageCrumbNavigation(SiteData.CurrentSiteID, SiteData.CurrentSite.Blog_Root_ContentID.Value, !SecurityData.IsAuthEditor);

					if (lstNav != null && lstNav.Any()) {
						pageNav.NavOrder = lstNav.Max(x => x.NavOrder) + 100;
						lstNav.Add(pageNav);
					}
				} else {
					lstNav = navHelper.GetPageCrumbNavigation(SiteData.CurrentSiteID, pageNav.Root_ContentID, !SecurityData.IsAuthEditor);
				}
				lstNav.RemoveAll(x => x.ShowInSiteNav == false && x.ContentType == ContentPageType.PageType.ContentEntry);
			}

			lstNav.ForEach(q => ControlUtilities.IdentifyLinkAsInactive(q));

			string sCSS = String.Empty;
			if (!String.IsNullOrEmpty(this.CssClass)) {
				sCSS = " class=\"" + this.CssClass + "\" ";
			}

			string sSelCSS = String.Format("{0} {1}", this.CssSelected, this.CssWrapper).Trim();

			string sWrapCSS = String.Empty;
			if (!String.IsNullOrEmpty(this.CssWrapper)) {
				sWrapCSS = " class=\"" + this.CssWrapper + "\" ";
			}

			if (this.DisplayAsList) {
				sb.AppendLine("<ul" + sCSS + " id=\"" + this.ElementId + "\">");
				foreach (SiteNav c in lstNav) {
					if (SiteData.IsFilenameCurrentPage(c.FileName) || ControlUtilities.AreFilenamesSame(c.FileName, sParent)) {
						sb.AppendLine("<li class=\"" + sSelCSS + "\">" + c.NavMenuText + "</li> ");
					} else {
						sb.AppendLine("<li" + sWrapCSS + "><a href=\"" + c.FileName + "\">" + c.NavMenuText + "</a></li> ");
					}
				}
				sb.AppendLine("</ul>");
			} else {
				string sDivider = String.Format(" {0} ", this.TextDivider);
				int iCtr = 1;
				int iMax = lstNav.Count;
				sb.AppendLine("<div" + sCSS + " id=\"" + this.ElementId + "\">");
				foreach (SiteNav c in lstNav) {
					if (SiteData.IsFilenameCurrentPage(c.FileName) || ControlUtilities.AreFilenamesSame(c.FileName, sParent)) {
						sb.AppendLine("<span class=\"" + sSelCSS + "\">" + c.NavMenuText + " " + sDivider + "</span> ");
					} else {
						sb.AppendLine("<span" + sWrapCSS + "><a href=\"" + c.FileName + "\">" + c.NavMenuText + "</a> " + sDivider + "</span> ");
					}
					iCtr++;

					if (iCtr == iMax) {
						sDivider = String.Empty;
					}
				}
				sb.AppendLine("</div>");
			}

			return sb.ToString();
		}
	}
}