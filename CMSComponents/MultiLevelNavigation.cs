using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
using System;
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

	public class MultiLevelNavigation : SimpleList {
		protected StringBuilder output = new StringBuilder();
		protected string sItemCss = String.Empty;
		protected string sItemCSS = String.Empty;

		public MultiLevelNavigation()
			: base() {
			this.LevelDepth = 3;
		}

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public int LevelDepth { get; set; }

		public override void LoadData() {
			base.LoadData();

			try {
				if (this.PublicParmValues.Any()) {
					this.LevelDepth = int.Parse(base.GetParmValue("LevelDepth", "3"));
				}
			} catch (Exception ex) { }

			if (this.LevelDepth <= 0) {
				this.LevelDepth = 1;
			}

			if (this.LevelDepth > 50) {
				this.LevelDepth = 50;
			}

			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				this.NavigationData = navHelper.GetLevelDepthNavigation(SiteData.CurrentSiteID, this.LevelDepth, !SecurityData.IsAuthEditor);
			}
		}

		public override string ToHtmlString() {
			LoadData();
			TweakData();

			InitList();

			return output.ToString();
		}

		protected void InitList() {
			output = new StringBuilder();
			sItemCss = String.Empty;

			if (!String.IsNullOrEmpty(this.CssClass)) {
				output.AppendLine("<ul id=\"" + this.ElementId + "\" class=\"" + this.CssClass + "\">");
			} else {
				output.AppendLine("<ul id=\"" + this.ElementId + "\" >");
			}

			if (this.NavigationData != null && this.NavigationData.Any()) {
				foreach (var n in this.NavigationData.OrderBy(x => x.NavOrder).Where(x => x.Parent_ContentID == null)) {
					if (SiteData.IsFilenameCurrentPage(n.FileName)
								|| (n.NavOrder == 0 && SiteData.IsCurrentLikelyHomePage)
								|| ControlUtilities.AreFilenamesSame(n.FileName, this.CmsPage.ThePage.FileName)) {
						sItemCss = String.Format(" {0} {1} ", this.CssItem, this.CssSelected).Trim();
					} else {
						sItemCss = String.Format(" {0} ", this.CssItem).Trim();
					}

					if (!String.IsNullOrEmpty(sItemCss)) {
						output.Append("<li class=\"" + sItemCss + "\">");
					} else {
						output.Append("<li>");
					}

					output.Append(" <a href=\"" + n.FileName + "\">" + n.NavMenuText + "</a> ");
					LoadChildLevels(n.Root_ContentID);
					output.Append(" </li>" + Environment.NewLine);
				}
			}

			output.AppendLine("</ul>");
		}

		protected void LoadChildLevels(Guid parentID) {
			sItemCss = String.Empty;

			output.AppendLine("<ul>");

			if (this.NavigationData != null && this.NavigationData.Any()) {
				foreach (var n in this.NavigationData.OrderBy(x => x.NavOrder).Where(x => x.Parent_ContentID == parentID)) {
					if (SiteData.IsFilenameCurrentPage(n.FileName)
								|| (n.NavOrder == 0 && SiteData.IsCurrentLikelyHomePage)
								|| ControlUtilities.AreFilenamesSame(n.FileName, this.CmsPage.ThePage.FileName)) {
						sItemCss = String.Format(" {0} {1} ", this.CssItem, this.CssSelected).Trim();
					} else {
						sItemCss = String.Format(" {0} ", this.CssItem).Trim();
					}

					if (!String.IsNullOrEmpty(sItemCss)) {
						output.Append("<li class=\"" + sItemCss + "\">");
					} else {
						output.Append("<li>");
					}

					output.Append(" <a href=\"" + n.FileName + "\">" + n.NavMenuText + "</a> ");

					LoadChildLevels(n.Root_ContentID);

					output.Append(" </li>" + Environment.NewLine);
				}
			}

			output.AppendLine("</ul>");
		}
	}
}