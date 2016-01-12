using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Drawing;

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

	public class TwoLevelNavigation : BaseTwoPartCmsComponent {

		public TwoLevelNavigation()
			: base() {
			this.ElementId = "TwoLevelNavigation";

			this.FontSize = new SizeUnit("14px");
			this.MenuFontSize = this.FontSize;

			this.BGColor = Color.Transparent;
			this.UnSelBGColor = Color.Transparent;
			this.ForeColor = ColorTranslator.FromHtml("#758569");
			this.BackColor = ColorTranslator.FromHtml("#DDDDDD");

			this.HoverBGColor = Color.Empty;
			this.HoverFGColor = Color.Empty;
			this.UnSelFGColor = Color.Empty;
			this.SelBGColor = Color.Empty;
			this.SelFGColor = Color.Empty;
			this.SubBGColor = Color.Empty;
			this.SubFGColor = Color.Empty;
		}

		private void FlipColor() {
			this.HoverBGColor = this.HoverBGColor == Color.Empty ? this.ForeColor : this.HoverBGColor;
			this.HoverFGColor = this.HoverFGColor == Color.Empty ? this.BackColor : this.HoverFGColor;
			this.UnSelFGColor = this.UnSelFGColor == Color.Empty ? this.ForeColor : this.UnSelFGColor;
			this.SelBGColor = this.SelBGColor == Color.Empty ? this.ForeColor : this.SelBGColor;
			this.SelFGColor = this.SelFGColor == Color.Empty ? this.BackColor : this.SelFGColor;
			this.SubBGColor = this.SubBGColor == Color.Empty ? this.BackColor : this.SubBGColor;
			this.SubFGColor = this.SubFGColor == Color.Empty ? this.ForeColor : this.SubFGColor;
		}

		public override bool MultiLevel {
			get {
				return true;
			}
		}

		public bool AutoStylingDisabled { get; set; }

		public SizeUnit FontSize { get; set; }

		public SizeUnit MenuFontSize { get; set; }

		public string CssTopBackground { get; set; }

		public Color ForeColor { get; set; }

		public Color BGColor { get; set; }

		public Color BackColor { get; set; }

		public Color HoverBGColor { get; set; }

		public Color HoverFGColor { get; set; }

		public Color UnSelBGColor { get; set; }

		public Color UnSelFGColor { get; set; }

		public Color SelBGColor { get; set; }

		public Color SelFGColor { get; set; }

		public Color SubBGColor { get; set; }

		public Color SubFGColor { get; set; }

		protected override void LoadData() {
			base.LoadData();

			this.NavigationData = navHelper.GetTwoLevelNavigation(SiteData.CurrentSiteID, !SecurityData.IsAuthEditor);
		}

		public override string GetHead() {
			FlipColor();

			string sCSSText = String.Empty;

			if (!this.AutoStylingDisabled) {
				sCSSText = ControlUtilities.GetManifestResourceStream("Carrotware.CMS.UI.Components.TopMenu.txt");

				if (!String.IsNullOrEmpty(sCSSText)) {
					sCSSText = sCSSText.Replace("{FORE_HEX}", ColorTranslator.ToHtml(this.ForeColor).ToLowerInvariant());
					sCSSText = sCSSText.Replace("{BG_HEX}", ColorTranslator.ToHtml(this.BGColor).ToLowerInvariant());

					sCSSText = sCSSText.Replace("{HOVER_FORE_HEX}", ColorTranslator.ToHtml(this.HoverFGColor).ToLowerInvariant());
					sCSSText = sCSSText.Replace("{HOVER_BG_HEX}", ColorTranslator.ToHtml(this.HoverBGColor).ToLowerInvariant());

					sCSSText = sCSSText.Replace("{SEL_FORE_HEX}", ColorTranslator.ToHtml(this.SelFGColor).ToLowerInvariant());
					sCSSText = sCSSText.Replace("{SEL_BG_HEX}", ColorTranslator.ToHtml(this.SelBGColor).ToLowerInvariant());

					sCSSText = sCSSText.Replace("{UNSEL_FORE_HEX}", ColorTranslator.ToHtml(this.UnSelFGColor).ToLowerInvariant());
					sCSSText = sCSSText.Replace("{UNSEL_BG_HEX}", ColorTranslator.ToHtml(this.UnSelBGColor).ToLowerInvariant());

					sCSSText = sCSSText.Replace("{SUB_FORE_HEX}", ColorTranslator.ToHtml(this.SubFGColor).ToLowerInvariant());
					sCSSText = sCSSText.Replace("{SUB_BG_HEX}", ColorTranslator.ToHtml(this.SubBGColor).ToLowerInvariant());

					if (this.FontSize.Value.HasValue) {
						sCSSText = sCSSText.Replace("{FONT_SIZE}", this.FontSize.ToString());
						if (this.FontSize.Type == SizeType.Pixel) {
							sCSSText = sCSSText.Replace("{MENU_HEIGHT}", String.Format("{0}px", Math.Floor(4 + (this.FontSize.Value.Value * 3))));
						}
					} else {
						sCSSText = sCSSText.Replace("{FONT_SIZE}", "inherit");
					}
					sCSSText = sCSSText.Replace("{MENU_HEIGHT}", "3em");

					if (this.MenuFontSize.Value.HasValue) {
						sCSSText = sCSSText.Replace("{MAIN_FONT_SIZE}", this.MenuFontSize.ToString());
					} else {
						sCSSText = sCSSText.Replace("{MAIN_FONT_SIZE}", "inherit");
					}

					sCSSText = sCSSText.Replace("{MOBILE_WIDTH}", "100%");

					sCSSText = sCSSText.Replace("{DESK_WIDTH}", "100%");

					sCSSText = sCSSText.Replace("{MENU_SELECT_CLASS}", this.CssSelected);
					sCSSText = sCSSText.Replace("{MENU_HASCHILD_CLASS}", this.CssHasChildren);

					if (!String.IsNullOrEmpty(this.CssTopBackground)) {
						this.CssTopBackground = this.CssTopBackground.Replace(";", "");
						sCSSText = sCSSText.Replace("{TOP_BACKGROUND_STYLE}", "background: " + this.CssTopBackground + ";");
					} else {
						sCSSText = sCSSText.Replace("{TOP_BACKGROUND_STYLE}", "");
					}

					sCSSText = sCSSText.Replace("{MENU_ID}", "#" + this.ElementId + "");
					sCSSText = sCSSText.Replace("{MENU_WRAPPER_ID}", "#" + this.ElementId + "-wrapper");
					sCSSText = "\r\n\t<style type=\"text/css\">\r\n" + sCSSText + "\r\n\t</style>\r\n";
				}
			}

			return sCSSText;
		}
	}
}