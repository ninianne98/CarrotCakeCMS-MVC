/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.Web.UI.Components {

	public class Bootstrap : BaseWebComponent {

		public Bootstrap() {
			this.SelectedSkin = BootstrapColorScheme.NotUsed;
		}

		public enum BootstrapColorScheme {
			ClassicBlue,
			ClassicGreen,
			ClassicOrange,
			Plum,
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
			Rust,
			Illuminating,
			FrenchBlue,
			GreenAsh,
			BurntCoral,
			Mint,
			AmethystOrchid,
			RaspberrySorbet,
			Inkwell,
			UltimateGray,
			Buttercream,
			DesertMist,
			Willow,
			NotUsed
		}

		public BootstrapColorScheme SelectedSkin { get; set; }

		private static string GetWebResourceUrl(string resource) {
			return CarrotWeb.GetWebResourceUrl(resource);
		}

		public override string GetHtml() {
			var bootstrapJS = GetWebResourceUrl("bootstrap.bootstrap.min.js");
			var bootstrapCSS = GetWebResourceUrl("bootstrap.bootstrap.min.css");
			var bootstrapColor = string.Empty;

			if (this.SelectedSkin != BootstrapColorScheme.NotUsed) {
				if (this.SelectedSkin == BootstrapColorScheme.Violet) {
					this.SelectedSkin = BootstrapColorScheme.ClassicViolet;
				}
				if (this.SelectedSkin == BootstrapColorScheme.Plum) {
					this.SelectedSkin = BootstrapColorScheme.ClassicPlum;
				}

				var colorUri = GetWebResourceUrl(string.Format("bootstrap.{0}.min.css", this.SelectedSkin.ToString().ToLowerInvariant()));

				bootstrapColor = UrlPaths.CreateCssTag(string.Format("Bootstrap CSS {0}", this.SelectedSkin), colorUri);
			}

			return (UrlPaths.CreateJavascriptTag("Bootstrap", bootstrapJS) + " \r\n"
							+ UrlPaths.CreateCssTag("Bootstrap CSS", bootstrapCSS) + "\r\n"
							+ bootstrapColor);
		}
	}
}