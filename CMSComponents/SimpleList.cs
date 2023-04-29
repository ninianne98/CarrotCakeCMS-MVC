using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

	public abstract class SimpleList : BaseToolboxComponent {

		public SimpleList()
			: base() {
			this.CssClass = string.Empty;
			this.CssSelected = "selected";
			this.ElementId = "list";

			this.CmsPage = PagePayload.GetContentFromViewData();
			this.NavigationData = new List<SiteNav>();
		}

		public override bool EnableEdit {
			get {
				return true;
			}
		}

		public PagePayload CmsPage { get; set; }

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public string ElementId { get; set; }

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public string CssClass { get; set; }

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public string CssItem { get; set; }

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public string CssAnchor { get; set; }

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public string CssSelected { get; set; }

		public List<SiteNav> NavigationData { get; set; }

		public override void LoadData() {
			base.LoadData();

			this.NavigationData = new List<SiteNav>();

			this.ElementId = this.WidgetClientID;

			try {
				string sFoundVal = this.GetParmValue("ElementId", this.WidgetClientID);

				if (!string.IsNullOrEmpty(sFoundVal)) {
					this.ElementId = sFoundVal;
				}
			} catch (Exception ex) { }

			try {
				string sFoundVal = this.GetParmValue("CssClass", string.Empty);

				if (!string.IsNullOrEmpty(sFoundVal)) {
					this.CssClass = sFoundVal;
				}
			} catch (Exception ex) { }

			try {
				string sFoundVal = this.GetParmValue("CssItem", string.Empty);

				if (!string.IsNullOrEmpty(sFoundVal)) {
					this.CssItem = sFoundVal;
				}
			} catch (Exception ex) { }

			try {
				string sFoundVal = this.GetParmValue("CssAnchor", string.Empty);

				if (!string.IsNullOrEmpty(sFoundVal)) {
					this.CssAnchor = sFoundVal;
				}
			} catch (Exception ex) { }

			try {
				string sFoundVal = this.GetParmValue("CssSelected", "selected");

				if (!string.IsNullOrEmpty(sFoundVal)) {
					this.CssSelected = sFoundVal;
				}
			} catch (Exception ex) { }
		}

		protected virtual void TweakData() {
			this.NavigationData = ControlUtilities.TweakData(this.NavigationData);
		}

		public override string ToHtmlString() {
			var output = new StringBuilder();
			LoadData();
			TweakData();

			var list = new HtmlTag("ul");
			list.SetAttribute("id", this.ElementId);
			list.MergeAttribute("class", this.CssClass);
			output.AppendLine(list.OpenTag());

			if (this.NavigationData != null && this.NavigationData.Any()) {
				foreach (var n in this.NavigationData) {
					var item = new HtmlTag("li");
					var link = new HtmlTag("a");

					item.MergeAttribute("class", this.CssItem);
					link.MergeAttribute("class", this.CssAnchor);

					if (SiteData.IsFilenameCurrentPage(n.FileName)
								|| (n.NavOrder == 0 && SiteData.IsCurrentLikelyHomePage)
								|| ControlUtilities.AreFilenamesSame(n.FileName, this.CmsPage.ThePage.FileName)) {
						item.MergeAttribute("class", this.CssSelected);
						link.MergeAttribute("class", this.CssSelected);
					}

					link.Uri = n.FileName;
					link.InnerHtml = n.NavMenuText;

					item.InnerHtml = link.RenderTag();

					output.AppendLine(item.RenderTag());
				}
			}

			output.AppendLine(list.CloseTag());

			return ControlUtilities.HtmlFormat(output);
		}
	}

	//==================================
	public abstract class SimpleListSortable : SimpleList {

		public SimpleListSortable()
			: base() {
			this.SortNavBy = SortOrder.SortAsc;
		}

		[Widget(WidgetAttribute.FieldMode.DictionaryList)]
		public Dictionary<string, string> lstSortOrder {
			get {
				Dictionary<string, string> _dict = new Dictionary<string, string>();

				_dict = EnumHelper.ToList<SortOrder>().ToDictionary(k => k.Text, v => v.Description);

				return _dict;
			}
		}

		[Widget(WidgetAttribute.FieldMode.DropDownList, "lstSortOrder")]
		public SortOrder SortNavBy { get; set; }

		public enum SortOrder {

			[Description("Link Order Ascending")]
			SortAsc,

			[Description("Link Order Descending")]
			SortDesc,

			[Description("Go Live Date Ascending")]
			DateAsc,

			[Description("Go Live Date Descending")]
			DateDesc,

			[Description("Link Text Ascending")]
			TitleAsc,

			[Description("Link Text Descending")]
			TitleDesc,
		}

		protected override void TweakData() {
			base.TweakData();

			ToggleSort();
		}

		public override void LoadData() {
			base.LoadData();

			try {
				string sFoundVal = this.GetParmValue("SortNavBy", SortOrder.SortAsc.ToString());

				if (!string.IsNullOrEmpty(sFoundVal)) {
					this.SortNavBy = (SortOrder)Enum.Parse(typeof(SortOrder), sFoundVal, true);
				}
			} catch (Exception ex) { }
		}

		protected void ToggleSort() {
			if (this.NavigationData != null && this.NavigationData.Any()) {
				switch (this.SortNavBy) {
					case SortOrder.TitleAsc:
						this.NavigationData = this.NavigationData.OrderBy(ct => ct.NavMenuText).ToList();
						break;

					case SortOrder.TitleDesc:
						this.NavigationData = this.NavigationData.OrderByDescending(ct => ct.NavMenuText).ToList();
						break;

					case SortOrder.DateAsc:
						this.NavigationData = this.NavigationData.OrderBy(ct => ct.NavMenuText).OrderBy(ct => ct.GoLiveDate).ToList();
						break;

					case SortOrder.DateDesc:
						this.NavigationData = this.NavigationData.OrderBy(ct => ct.NavMenuText).OrderByDescending(ct => ct.GoLiveDate).ToList();
						break;

					case SortOrder.SortDesc:
						this.NavigationData = this.NavigationData.OrderBy(ct => ct.NavMenuText).OrderByDescending(ct => ct.NavOrder).ToList();
						break;

					case SortOrder.SortAsc:
					default:
						this.NavigationData = this.NavigationData.OrderBy(ct => ct.NavMenuText).OrderBy(ct => ct.NavOrder).ToList();
						break;
				}
			} else {
				this.NavigationData = new List<SiteNav>();
			}
		}
	}
}