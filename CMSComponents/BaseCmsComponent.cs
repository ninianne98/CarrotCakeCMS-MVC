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

	public abstract class BaseCmsComponent : BaseWebComponent, ICmsChildrenComponent, ICmsMainComponent {
		protected ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper();

		public BaseCmsComponent()
			: base() {
			LoadData();

			this.CssSelected = "selected";
			this.CssULClassTop = "parent";
			this.CssULClassLower = "children";
			this.CssHasChildren = "sub";
		}

		public virtual bool MultiLevel {
			get {
				return false;
			}
		}

		public string CssClass { get; set; }
		public string CssItem { get; set; }
		public string CssSelected { get; set; }
		public string CssULClassTop { get; set; }
		public string CssULClassLower { get; set; }
		public string CssHasChildren { get; set; }

		public string ElementId { get; set; }

		public List<SiteNav> NavigationData { get; set; }

		protected virtual void LoadData() {
			this.NavigationData = new List<SiteNav>();
		}

		protected virtual void TweakData() {
			if (this.NavigationData != null) {
				this.NavigationData.RemoveAll(x => x.ShowInSiteNav == false && x.ContentType == ContentPageType.PageType.ContentEntry);
				this.NavigationData.RemoveAll(x => x.ShowInSiteMap == false && x.ContentType == ContentPageType.PageType.ContentEntry);

				this.NavigationData.ForEach(q => ControlUtilities.IdentifyLinkAsInactive(q));
			}
		}

		public virtual List<SiteNav> GetTopNav() {
			if (this.MultiLevel) {
				return this.NavigationData.Where(ct => ct.Parent_ContentID == null).OrderBy(ct => ct.NavMenuText).OrderBy(ct => ct.NavOrder).ToList();
			} else {
				return this.NavigationData.OrderBy(ct => ct.NavMenuText).OrderBy(ct => ct.NavOrder).ToList();
			}
		}

		public virtual List<SiteNav> GetChildren(Guid rootContentID) {
			return this.NavigationData.Where(ct => ct.Parent_ContentID == rootContentID).OrderBy(ct => ct.NavMenuText).OrderBy(ct => ct.NavOrder).ToList();
		}

		protected SiteNav IsContained(List<SiteNav> navCrumbs, Guid rootContentID) {
			return navCrumbs.Where(ct => ct.Root_ContentID == rootContentID && ct.NavOrder > 0).FirstOrDefault();
		}

		protected StringBuilder WriteListPrefix(StringBuilder output) {
			string sCSS = (this.CssULClassTop + " " + this.CssClass).Trim();
			if (!String.IsNullOrEmpty(sCSS)) {
				output.AppendLine("<ul id=\"" + this.ElementId + "\" class=\"" + sCSS + "\">");
			} else {
				output.AppendLine("<ul id=\"" + this.ElementId + "\" >");
			}

			return output;
		}

		protected StringBuilder WriteListSuffix(StringBuilder output) {
			output.AppendLine("</ul>");
			return output;
		}

		protected string ParentFileName { get; set; }

		private int iItemNumber = 0;

		protected virtual StringBuilder WriteTopLevel(StringBuilder output) {
			List<SiteNav> lstNav = GetTopNav();
			SiteNav parentPageNav = ControlUtilities.GetParentPage();
			List<SiteNav> lstNavTree = ControlUtilities.GetPageNavTree().OrderByDescending(x => x.NavOrder).ToList();

			this.ParentFileName = parentPageNav.FileName.ToLower();

			if (lstNav != null && lstNav.Any()) {
				output.AppendLine();
				WriteListPrefix(output);

				string sItemCSS = String.Empty;
				if (!String.IsNullOrEmpty(this.CssItem)) {
					sItemCSS = String.Format(" {0} ", this.CssItem);
				}

				string sThis1CSS = sItemCSS;

				foreach (SiteNav c1 in lstNav) {
					List<SiteNav> cc = GetChildren(c1.Root_ContentID);

					string sChild = " ";
					if (this.MultiLevel) {
						if (cc != null && cc.Any()) {
							sChild = " level1-haschildren " + this.CssHasChildren + " ";
						}
						sThis1CSS = " level1 " + sItemCSS + sChild;
					} else {
						sThis1CSS = sItemCSS;
					}
					if (SiteData.IsFilenameCurrentPage(c1.FileName)
						|| (c1.NavOrder == 0 && SiteData.IsCurrentLikelyHomePage)
						|| (IsContained(lstNavTree, c1.Root_ContentID) != null)
						|| ControlUtilities.AreFilenamesSame(c1.FileName, this.ParentFileName)) {
						sThis1CSS = sThis1CSS + " " + this.CssSelected;
					}
					if (lstNav.Where(x => x.NavOrder < 0).Count() > 0) {
						if (c1.NavOrder < 0) {
							sThis1CSS = sThis1CSS + " parent-nav";
						} else {
							sThis1CSS = sThis1CSS + " child-nav";
						}
					}
					sThis1CSS = sThis1CSS.Replace("   ", " ").Replace("  ", " ").Trim();

					iItemNumber++;
					output.AppendLine("<li id=\"listitem" + iItemNumber.ToString() + "\" class=\"" + sThis1CSS + "\"><a href=\"" + c1.FileName + "\">" + c1.NavMenuText + "</a>");

					if (this.MultiLevel && cc != null && cc.Any()) {
						LoadChildren(output, c1.Root_ContentID, sItemCSS, iItemNumber, 2);
					}

					output.AppendLine("</li>");
					output.AppendLine();
				}
				WriteListSuffix(output);
			} else {
				output.AppendLine("<span style=\"display: none;\" id=\"" + this.ElementId + "\"></span>");
			}

			return output;
		}

		protected virtual StringBuilder LoadChildren(StringBuilder output, Guid rootContentID, string sItemCSS, int iParent, int iLevel) {
			List<SiteNav> lstNav = GetChildren(rootContentID);

			string sThis2CSS = sItemCSS;

			if (lstNav != null && lstNav.Any()) {
				output.AppendLine();
				output.AppendLine("<ul id=\"listitem" + iParent.ToString() + "-childlist\" class=\"childlist childlevel" + iLevel + " " + this.CssULClassLower + "\">");

				foreach (SiteNav c2 in lstNav) {
					List<SiteNav> cc = GetChildren(c2.Root_ContentID);

					if (this.MultiLevel) {
						string sChild = " ";
						if (cc != null && cc.Any()) {
							sChild = " level" + iLevel + "-haschildren " + this.CssHasChildren + " ";
						}
						sThis2CSS = " level" + iLevel + " " + sItemCSS + sChild;
					} else {
						sThis2CSS = sItemCSS;
					}

					if (SiteData.IsFilenameCurrentPage(c2.FileName)
							|| ControlUtilities.AreFilenamesSame(c2.FileName, this.ParentFileName)) {
						sThis2CSS = sThis2CSS + " " + this.CssSelected;
					}
					sThis2CSS = (sThis2CSS + " child-nav").Replace("   ", " ").Replace("  ", " ").Trim();

					iItemNumber++;
					output.AppendLine("<li id=\"listitem" + iItemNumber.ToString() + "\" class=\"" + sThis2CSS + "\"><a href=\"" + c2.FileName + "\">" + c2.NavMenuText + "</a>");

					if (cc != null && cc.Any()) {
						LoadChildren(output, c2.Root_ContentID, sItemCSS, iItemNumber, iLevel + 1);
					}

					output.Append("</li>");

					output.AppendLine();
				}

				output.AppendLine("</ul> ");
			}

			return output;
		}

		public override string GetHtml() {
			TweakData();

			StringBuilder output = new StringBuilder();
			return WriteTopLevel(output).ToString();
		}
	}
}