using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System.Drawing;
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

	public class CmsSkin : BaseWebComponent {
		private string _rootThemePathSkin = "/Assets/Admin/skins/";
		private string _rootThemePathEdit = "/Assets/Admin/editor/";
		private jquerybasic _jqb = new jquerybasic();

		public enum SkinOption {
			None,
			Classic,
			AmethystOrchid,
			BlueIzis,
			FrenchBlue,
			Mauve,
			MosaicBlue,
			Plum,
			QuetzalGreen,
			Rust,
			Sandstone,
			SugarAlmond,
		}

		public enum SkinMode {
			Main,
			AdvEdit,
			Popup,
			Logon,
			Notify,
		}

		private bool _useEditor = false;

		public CmsSkin() {
			this.SelectedColor = SkinOption.Classic;
			this.WindowMode = SkinMode.Main;
		}

		public CmsSkin(SkinOption color) {
			this.SelectedColor = color;
			this.WindowMode = SkinMode.Main;
		}

		public CmsSkin(SkinOption color, SkinMode mode) {
			this.SelectedColor = color;
			this.WindowMode = mode;
		}

		protected void GetEditState() {
			var editModes = new SkinMode[] { SkinMode.Notify, SkinMode.AdvEdit };
			_useEditor = editModes.Contains(this.WindowMode);
		}

		public SkinOption SelectedColor { get; set; }
		public SkinMode WindowMode { get; set; }

		public static string GetPrimaryColorCode(SkinOption color) {
			string colorCode = "#cccccc";

			switch (color) {
				case SkinOption.Classic:
					colorCode = "#6f997d";
					break;

				case SkinOption.AmethystOrchid:
					colorCode = "#926aa6";
					break;

				case SkinOption.BlueIzis:
					colorCode = "#5b5ea6";
					break;

				case SkinOption.FrenchBlue:
					colorCode = "#0072b5";
					break;

				case SkinOption.Mauve:
					colorCode = "#76608a";
					break;

				case SkinOption.MosaicBlue:
					colorCode = "#00758f";
					break;

				case SkinOption.Plum:
					colorCode = "#854085";
					break;

				case SkinOption.QuetzalGreen:
					colorCode = "#006e6d";
					break;

				case SkinOption.Rust:
					colorCode = "#b55a30";
					break;

				case SkinOption.Sandstone:
					colorCode = "#c48a69";
					break;

				case SkinOption.SugarAlmond:
					colorCode = "#935529";
					break;
			}

			return colorCode;
		}

		public static string DarkenColor(string color, double adjustment) {
			var main = CarrotWeb.DecodeColor(color);

			return ColorTranslator.ToHtml(DarkenColor(main, adjustment));
		}

		public static Color DarkenColor(Color color, double adjustment) {
			if (adjustment > 0) {
				// needs to be negative
				adjustment = adjustment * -1;
			}
			if (adjustment < -1) {
				// convert to a percentage
				adjustment = adjustment / 100;
			}

			return ShiftColor(color, adjustment);
		}

		public static string LightenColor(string color, double adjustment) {
			var main = CarrotWeb.DecodeColor(color);

			return ColorTranslator.ToHtml(LightenColor(main, adjustment));
		}

		public static Color LightenColor(Color color, double adjustment) {
			if (adjustment < 0) {
				// needs to be positive
				adjustment = adjustment * -1;
			}
			if (adjustment > 1) {
				// convert to a percentage
				adjustment = adjustment / 100;
			}

			return ShiftColor(color, adjustment);
		}

		protected static Color ShiftColor(Color color, double adjustment) {
			var adjust = (float)adjustment;

			float rr = color.R;
			float gg = color.G;
			float bb = color.B;

			if (adjust >= 0) {
				//lighten
				rr = ((255 - rr) * adjust) + rr;
				gg = ((255 - gg) * adjust) + gg;
				bb = ((255 - bb) * adjust) + bb;
			} else {
				//darken
				adjust = 1 + adjust;
				rr = rr * adjust;
				gg = gg * adjust;
				bb = bb * adjust;
			}

			return Color.FromArgb(color.A, (int)rr, (int)gg, (int)bb);
		}

		public override string GetHtml() {
			GetEditState();

			var sb = new StringBuilder();
			_jqb.SelectedSkin = jquerybasic.jQueryTheme.Silver;

			switch (this.SelectedColor) {
				case SkinOption.Classic:
					_jqb.SelectedSkin = jquerybasic.jQueryTheme.LightGreen;
					break;

				case SkinOption.Mauve:
				case SkinOption.AmethystOrchid:
					_jqb.SelectedSkin = jquerybasic.jQueryTheme.Purple;
					break;

				case SkinOption.Rust:
				case SkinOption.Sandstone:
				case SkinOption.SugarAlmond:
				case SkinOption.MosaicBlue:
					_jqb.SelectedSkin = jquerybasic.jQueryTheme.GlossyBlack;
					break;

				case SkinOption.FrenchBlue:
					_jqb.SelectedSkin = jquerybasic.jQueryTheme.Blue;
					break;

				case SkinOption.QuetzalGreen:
				case SkinOption.BlueIzis:
				case SkinOption.Plum:
					_jqb.SelectedSkin = jquerybasic.jQueryTheme.Silver;
					break;

				default:
					_jqb.SelectedSkin = jquerybasic.jQueryTheme.Silver;
					break;
			}

			var rootPath = _useEditor ? _rootThemePathEdit : _rootThemePathSkin;

			var versionKey = string.Format("?cms={0}&ts={1}", SiteData.CurrentDLLVersion, CarrotWeb.DateKey());

			var csstag = new HtmlTag(HtmlTag.EasyTag.Css);

			csstag.Uri = string.Format("{0}{1}.{2}.min.css{3}", rootPath,
						this.WindowMode.ToString().ToLowerInvariant(),
						this.SelectedColor.ToString().ToLowerInvariant(), versionKey);

			sb.AppendLine(string.Format("<!-- BEGIN {0} Theme   -->", this.SelectedColor));

			if (!_useEditor) {
				sb.AppendLine(_jqb.GetHtml().Trim());
			}

			sb.AppendLine(string.Format("<!-- CMS {0} Theme   -->", this.SelectedColor) + csstag.RenderSelfClosingTag());

			sb.AppendLine(string.Format("<!-- END {0} Theme   -->", this.SelectedColor));

			return "\r\n" + sb.ToString() + "\r\n";
		}
	}
}