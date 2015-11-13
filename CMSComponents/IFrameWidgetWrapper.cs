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

	public class IFrameWidgetWrapper : BaseToolboxComponent {

		public IFrameWidgetWrapper() {
			this.Hyperlink = String.Empty;
			this.CssStyle = "width: 300px; height: 100px;";
			this.ScrollingFrame = true;
			this.CssClass = String.Empty;
		}

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public string Hyperlink { get; set; }

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public string CssStyle { get; set; }

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public string CssClass { get; set; }

		[Widget(WidgetAttribute.FieldMode.CheckBox)]
		public bool ScrollingFrame { get; set; }

		public override void LoadData() {
			base.LoadData();

			try {
				string sFoundVal = this.GetParmValue("CssStyle", "width: 300px; height: 100px;");

				if (!String.IsNullOrEmpty(sFoundVal)) {
					this.CssStyle = sFoundVal;
				}
			} catch (Exception ex) { }

			try {
				string sFoundVal = this.GetParmValue("CssClass", String.Empty);

				if (!String.IsNullOrEmpty(sFoundVal)) {
					this.CssClass = sFoundVal;
				}
			} catch (Exception ex) { }
			try {
				string sFoundVal = this.GetParmValue("Hyperlink", String.Empty);

				if (!String.IsNullOrEmpty(sFoundVal)) {
					this.Hyperlink = sFoundVal;
				}
			} catch (Exception ex) { }
			try {
				string sFoundVal = this.GetParmValue("ScrollingFrame", "true");

				if (!String.IsNullOrEmpty(sFoundVal)) {
					this.ScrollingFrame = Convert.ToBoolean(sFoundVal);
				}
			} catch (Exception ex) { }
		}

		public override bool EnableEdit {
			get {
				return true;
			}
		}

		public override string ToHtmlString() {
			this.LoadData();

			StringBuilder sb = new StringBuilder();

			sb.AppendLine();

			string sCSS = String.Empty;
			if (!String.IsNullOrEmpty(this.CssClass)) {
				sCSS = " class=\"" + this.CssClass + "\" ";
			}
			string sStyle = String.Empty;
			if (!String.IsNullOrEmpty(this.CssStyle)) {
				sStyle = " style=\"" + this.CssStyle + "\" ";
			}
			string sHREF = String.Empty;
			if (!String.IsNullOrEmpty(this.Hyperlink)) {
				sHREF = " src=\"" + this.Hyperlink + "\" ";
			}
			string sScroll = String.Empty;
			if (this.ScrollingFrame) {
				sScroll = " scrolling=\"auto\" ";
			}

			sb.AppendLine("<div id=\"" + this.WidgetClientID + "\">");
			sb.AppendLine("\t<iframe id=\"" + this.WidgetClientID + "_frame\" " + sScroll + sStyle + sCSS + sHREF + " > </iframe>");
			sb.AppendLine("</div>");

			return sb.ToString();
		}
	}
}