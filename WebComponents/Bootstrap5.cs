/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: May 2024
*/

namespace Carrotware.Web.UI.Components {

	public class Bootstrap5 : BaseWebComponent {

		public Bootstrap5() {
			this.SelectedSkin = Bootstrap5ColorScheme.NotUsed;
		}

		public enum Bootstrap5ColorScheme {
			Seafoam,
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

			ClassicBlue,
			ClassicGreen,
			ClassicOrange,
			ClassicPlum,
			ClassicPurple,
			ClassicRed,
			ClassicTeal,
			Violet,
			ClassicViolet,
			Grey,
			LightGreen,
			Green,
			DarkGreen,
			Magenta,
			LightPurple,
			Purple,
			DarkPurple,
			Darken,
			Teal,
			LightBlue,
			Blue,
			DarkBlue,
			Yellow,
			Orange,
			DarkOrange,
			Red,
			DarkRed,
			Marigold,
			Cerulean,
			Illuminating,
			GreenAsh,
			BurntCoral,
			Mint,
			RaspberrySorbet,
			Inkwell,
			UltimateGray,
			Buttercream,
			DesertMist,
			Willow,
			NotUsed
		}

		public Bootstrap5ColorScheme SelectedSkin { get; set; }
		public bool ExcludeIcons { get; set; }

		private static string GetWebResourceUrl(string resource) {
			return CarrotWeb.GetWebResourceUrl(resource);
		}

		public override string GetHtml() {
			if (this.SelectedSkin == Bootstrap5ColorScheme.Violet) {
				this.SelectedSkin = Bootstrap5ColorScheme.ClassicViolet;
			}

			var bootstrapJSUri = GetWebResourceUrl("bootstrap5.bootstrap.min.js");
			var bootstrapCss = "bootstrap5.bootstrap.min.css";

			var bootstrapIconCssUri = this.ExcludeIcons == false ? UrlPaths.CreateCssTag("Bootstrap Icons", GetWebResourceUrl("bootstrap5.bootstrap-icons.min.css")) + " \n" : string.Empty;
			var bootstrapColorUri = string.Empty;

			if (this.SelectedSkin != Bootstrap5ColorScheme.NotUsed) {
				bootstrapCss = string.Format("bootstrap5.bootstrap.{0}.min.css", this.SelectedSkin.ToString().ToLowerInvariant());
				var colorUri = GetWebResourceUrl(string.Format("bootstrap5.{0}.min.css", this.SelectedSkin.ToString().ToLowerInvariant()));
				bootstrapColorUri = UrlPaths.CreateCssTag(string.Format("Bootstrap 5 CSS {0}", this.SelectedSkin), colorUri);
			}

			var bootstrapCssUri = GetWebResourceUrl(bootstrapCss);

			return (UrlPaths.CreateJavascriptTag("Bootstrap 5 JS", bootstrapJSUri) + " \n"
							+ UrlPaths.CreateCssTag(string.Format("Bootstrap 5 CSS Main {0}", this.SelectedSkin), bootstrapCssUri) + " \n"
							+ bootstrapColorUri
							+ bootstrapIconCssUri).Trim();
		}
	}
}