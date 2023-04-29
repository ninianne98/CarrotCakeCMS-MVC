using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
using Carrotware.Web.UI.Components;
using System;
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

	public class MultiLevelNavigation : SimpleListSortable {
		protected StringBuilder _output = new StringBuilder();

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

			return ControlUtilities.HtmlFormat(_output);
		}

		protected void InitList() {
			_output = new StringBuilder();

			var list = new HtmlTag("ul");
			list.SetAttribute("id", this.ElementId);
			list.MergeAttribute("class", this.CssClass);

			_output.AppendLine(list.OpenTag());

			if (this.NavigationData != null && this.NavigationData.Any()) {
				foreach (var n in this.NavigationData.OrderBy(x => x.NavOrder).Where(x => x.Parent_ContentID == null)) {
					var item = new HtmlTag("li");
					var link = new HtmlTag("a");

					item.MergeAttribute("class", this.CssItem);
					link.MergeAttribute("class", this.CssAnchor);

					link.Uri = n.FileName;
					link.InnerHtml = n.NavMenuText;

					if (SiteData.IsFilenameCurrentPage(n.FileName)
								|| (n.NavOrder == 0 && SiteData.IsCurrentLikelyHomePage)
								|| ControlUtilities.AreFilenamesSame(n.FileName, this.CmsPage.ThePage.FileName)) {
						item.MergeAttribute("class", this.CssSelected);
						link.MergeAttribute("class", this.CssSelected);
					}

					_output.Append(item.OpenTag());
					_output.Append(link.RenderTag());

					LoadChildLevels(n.Root_ContentID);

					_output.AppendLine(item.CloseTag());
				}
			}

			_output.AppendLine(list.CloseTag());
		}

		protected void LoadChildLevels(Guid parentID) {
			var list = new HtmlTag("ul");

			if (this.NavigationData != null && this.NavigationData.Where(x => x.Parent_ContentID == parentID).Any()) {
				_output.AppendLine(list.OpenTag());

				foreach (var n in this.NavigationData.OrderBy(x => x.NavOrder).Where(x => x.Parent_ContentID == parentID)) {
					var item = new HtmlTag("li");
					var link = new HtmlTag("a");

					item.MergeAttribute("class", this.CssItem);
					link.MergeAttribute("class", this.CssAnchor);

					link.Uri = n.FileName;
					link.InnerHtml = n.NavMenuText;

					if (SiteData.IsFilenameCurrentPage(n.FileName)
								|| ControlUtilities.AreFilenamesSame(n.FileName, this.CmsPage.ThePage.FileName)) {
						item.MergeAttribute("class", this.CssSelected);
						link.MergeAttribute("class", this.CssSelected);
					}

					_output.Append(item.OpenTag());
					_output.Append(link.RenderTag());

					LoadChildLevels(n.Root_ContentID);

					_output.AppendLine(item.CloseTag());
				}

				_output.AppendLine(list.CloseTag());
			}
		}
	}
}