using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System.Drawing;
using System.Text;
using System.Web;
using System;

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
			this.CssSelected = "selected";

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

		public TwoLevelNavigation(string f, string bg, string ubg, string fc, string bc,
				string hbc, string hfc, string uf, string sbg, string sfg,
				string bc2, string fc2) : this() {
			this.AutoStylingDisabled = false;
			this.FontSize = new SizeUnit(f);
			this.MenuFontSize = this.FontSize;

			this.BGColor = CarrotWeb.DecodeColor(bg);
			this.UnSelBGColor = CarrotWeb.DecodeColor(ubg);
			this.ForeColor = CarrotWeb.DecodeColor(fc);
			this.BackColor = CarrotWeb.DecodeColor(bc);

			this.HoverBGColor = CarrotWeb.DecodeColor(hbc);
			this.HoverFGColor = CarrotWeb.DecodeColor(hfc);

			this.UnSelFGColor = CarrotWeb.DecodeColor(uf);
			this.SelBGColor = CarrotWeb.DecodeColor(sbg);
			this.SelFGColor = CarrotWeb.DecodeColor(sfg);

			this.SubBGColor = CarrotWeb.DecodeColor(bc2);
			this.SubFGColor = CarrotWeb.DecodeColor(fc2);
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

		public static string NavigationStylePath {
			get {
				return "/carrotcakenavstyle.ashx";
			}
		}

		public string GenerateCSS() {
			string sCSSText = string.Empty;

			if (!this.AutoStylingDisabled) {
				FlipColor();

				var sb = new StringBuilder();
				sb.Append(ControlUtilities.ReadEmbededScript("Carrotware.CMS.UI.Components.TopMenu.txt"));

				if (sb != null && sb.Length > 1) {
					sb.Replace("[[TIMESTAMP]]", DateTime.UtcNow.ToString("u"));

					sb.Replace("{FORE_HEX}", ColorTranslator.ToHtml(this.ForeColor).ToLowerInvariant());
					sb.Replace("{BG_HEX}", ColorTranslator.ToHtml(this.BGColor).ToLowerInvariant());

					sb.Replace("{HOVER_FORE_HEX}", ColorTranslator.ToHtml(this.HoverFGColor).ToLowerInvariant());
					sb.Replace("{HOVER_BG_HEX}", ColorTranslator.ToHtml(this.HoverBGColor).ToLowerInvariant());

					sb.Replace("{SEL_FORE_HEX}", ColorTranslator.ToHtml(this.SelFGColor).ToLowerInvariant());
					sb.Replace("{SEL_BG_HEX}", ColorTranslator.ToHtml(this.SelBGColor).ToLowerInvariant());

					sb.Replace("{UNSEL_FORE_HEX}", ColorTranslator.ToHtml(this.UnSelFGColor).ToLowerInvariant());
					sb.Replace("{UNSEL_BG_HEX}", ColorTranslator.ToHtml(this.UnSelBGColor).ToLowerInvariant());

					sb.Replace("{SUB_FORE_HEX}", ColorTranslator.ToHtml(this.SubFGColor).ToLowerInvariant());
					sb.Replace("{SUB_BG_HEX}", ColorTranslator.ToHtml(this.SubBGColor).ToLowerInvariant());

					if (this.FontSize.Value.HasValue) {
						sb.Replace("{FONT_SIZE}", this.FontSize.ToString());
						if (this.FontSize.Type == SizeType.Pixel) {
							sb.Replace("{MENU_HEIGHT}", string.Format("{0}px", Math.Floor(4 + (this.FontSize.Value.Value * 3))));
						}
					} else {
						sb.Replace("{FONT_SIZE}", "inherit");
					}
					sb.Replace("{MENU_HEIGHT}", "3em");

					if (this.MenuFontSize.Value.HasValue) {
						sb.Replace("{MAIN_FONT_SIZE}", this.MenuFontSize.ToString());
					} else {
						sb.Replace("{MAIN_FONT_SIZE}", "inherit");
					}

					sb.Replace("{MOBILE_WIDTH}", "100%");

					sb.Replace("{DESK_WIDTH}", "100%");

					sb.Replace("{MENU_SELECT_CLASS}", this.CssSelected);
					sb.Replace("{MENU_HASCHILD_CLASS}", this.CssHasChildren);

					if (!string.IsNullOrEmpty(this.CssTopBackground)) {
						this.CssTopBackground = this.CssTopBackground.Replace(";", "");
						sb.Replace("{TOP_BACKGROUND_STYLE}", "background: " + this.CssTopBackground + ";");
					} else {
						sb.Replace("{TOP_BACKGROUND_STYLE}", "");
					}

					sb.Replace("{MENU_ID}", string.Format("#{0}", this.ElementId));
					sb.Replace("{MENU_WRAPPER_ID}", string.Format("#{0}-wrapper", this.ElementId));

					sCSSText = sb.ToString();
				}
			}

			return sCSSText;
		}

		public override string GetHead() {
			string sCSSText = string.Empty;

			if (!this.AutoStylingDisabled) {
				var sb = new StringBuilder();

				sb.Append(string.Format("{0}?el={1}&sel={2}&f={3}", NavigationStylePath, HttpUtility.HtmlEncode(Utils.EncodeBase64(ElementId)), HttpUtility.HtmlEncode(Utils.EncodeBase64(CssSelected)), this.FontSize));

				sb.Append(string.Format("&bg={0}&ubg={1}&fc={2}&bc={3}", CarrotWeb.EncodeColor(this.BGColor), CarrotWeb.EncodeColor(this.UnSelBGColor), CarrotWeb.EncodeColor(this.ForeColor), CarrotWeb.EncodeColor(this.BackColor)));
				sb.Append(string.Format("&hbc={0}&hfc={1}", CarrotWeb.EncodeColor(this.HoverBGColor), CarrotWeb.EncodeColor(this.HoverFGColor)));
				sb.Append(string.Format("&uf={0}&sbg={1}&sfg={2}", CarrotWeb.EncodeColor(this.UnSelFGColor), CarrotWeb.EncodeColor(this.SelBGColor), CarrotWeb.EncodeColor(this.SelFGColor)));
				sb.Append(string.Format("&bc2={0}&fc2={1}", CarrotWeb.EncodeColor(this.SubBGColor), CarrotWeb.EncodeColor(this.SubFGColor)));
				if (!string.IsNullOrWhiteSpace(CssTopBackground)) {
					sb.Append(string.Format("&tbg={0}", HttpUtility.HtmlEncode(CssTopBackground)));
				}
				sb.Append(string.Format("&ts={0}", CarrotWeb.DateKey()));

				sCSSText = UrlPaths.CreateCssTag(string.Format("Nav CSS: {0}", this.ElementId), sb.ToString());
			}

			//sCSSText = "\r\n\t<style type=\"text/css\">\r\n" + GenerateCSS() + "\r\n\t</style>\r\n";

			return sCSSText;
		}
	}
}