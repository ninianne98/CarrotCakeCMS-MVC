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
			Blue,
			Green,
			Grey,
			Orange,
			Plum,
			Purple,
			Red,
			Teal,
			Violet,
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
				var colorUri = string.Empty;
				switch (this.SelectedSkin) {
					case BootstrapColorScheme.Blue:
						colorUri = GetWebResourceUrl("bootstrap.blue.min.css");
						break;

					case BootstrapColorScheme.Green:
						colorUri = GetWebResourceUrl("bootstrap.green.min.css");
						break;

					case BootstrapColorScheme.Grey:
						colorUri = GetWebResourceUrl("bootstrap.grey.min.css");
						break;

					case BootstrapColorScheme.Orange:
						colorUri = GetWebResourceUrl("bootstrap.orange.min.css");
						break;

					case BootstrapColorScheme.Plum:
						colorUri = GetWebResourceUrl("bootstrap.plum.min.css");
						break;

					case BootstrapColorScheme.Purple:
						colorUri = GetWebResourceUrl("bootstrap.purple.min.css");
						break;

					case BootstrapColorScheme.Red:
						colorUri = GetWebResourceUrl("bootstrap.red.min.css");
						break;

					case BootstrapColorScheme.Teal:
						colorUri = GetWebResourceUrl("bootstrap.teal.min.css");
						break;

					case BootstrapColorScheme.Violet:
						colorUri = GetWebResourceUrl("bootstrap.violet.min.css");
						break;
				}

				bootstrapColor = "<!-- Bootstrap CSS - " + this.SelectedSkin.ToString() + " --> <link href=\"" + colorUri + "\" type=\"text/css\" rel=\"stylesheet\" />\r\n";
			}

			return ("\r\n<!-- Bootstrap --> <script src=\"" + bootstrapJS + "\" type=\"text/javascript\"></script> \r\n"
					+ "<!-- Bootstrap CSS --> <link href=\"" + bootstrapCSS + "\" type=\"text/css\" rel=\"stylesheet\" />\r\n"
					+ bootstrapColor);
		}
	}
}