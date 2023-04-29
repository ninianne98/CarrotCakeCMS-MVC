using System.Text;
using System.Web;

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

	public class jquerybasic : BaseWebComponent {

		public jquerybasic() {
			this.JQVersion = jquery.DefaultJQVersion;
			this.UseJqueryMigrate = false;
			this.StylesheetOnly = false;
			this.SelectedSkin = jQueryTheme.GlossyBlack;
		}

		public enum jQueryTheme {
			GlossyBlack,
			Silver,
			Purple,
			Blue,
			Green,
			LightGreen,
			NotUsed
		}

		public jQueryTheme SelectedSkin { get; set; }

		public bool StylesheetOnly { get; set; }

		public string JQVersion { get; set; }

		public bool UseJqueryMigrate { get; set; }


		private jquery _jq = new jquery();
		private jqueryui _jqui = new jqueryui();

		public override string GetHtml() {
			string sJQFile = string.Empty;
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(string.Empty);

			if (!this.StylesheetOnly) {
				_jq.JQVersion = this.JQVersion;
				_jq.UseJqueryMigrate = this.UseJqueryMigrate;

				sb.AppendLine(_jq.GetHtml().Trim());
				sb.AppendLine(_jqui.GetHtml().Trim());
			}

			switch (this.SelectedSkin) {
				case jQueryTheme.GlossyBlack:
					sJQFile = CarrotWeb.GetWebResourceUrl("jquerybasic.jquery-ui-black.css");
					break;

				case jQueryTheme.Purple:
					sJQFile = CarrotWeb.GetWebResourceUrl("jquerybasic.jquery-ui-purple.css");
					break;

				case jQueryTheme.Green:
					sJQFile = CarrotWeb.GetWebResourceUrl("jquerybasic.jquery-ui-green.css");
					break;

				case jQueryTheme.Blue:
					sJQFile = CarrotWeb.GetWebResourceUrl("jquerybasic.jquery-ui-blue.css");
					break;

				case jQueryTheme.LightGreen:
					sJQFile = CarrotWeb.GetWebResourceUrl("jquerybasic.jquery-ui-lightgreen.css");
					break;

				case jQueryTheme.Silver:
				default:
					sJQFile = CarrotWeb.GetWebResourceUrl("jquerybasic.jquery-ui-silver.css");
					break;
			}

			if (this.SelectedSkin != jQueryTheme.NotUsed) {
				sb.AppendLine(UrlPaths.CreateCssTag(string.Format("JQuery UI CSS {0}", this.SelectedSkin), sJQFile));
			}

			return sb.ToString().Trim();
		}
	}
}