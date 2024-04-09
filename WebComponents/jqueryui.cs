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

	public class jqueryui : BaseWebComponent {

		public jqueryui() {
			this.JQUIVersion = DefaultJQUIVersion;
		}

		public static string DefaultJQUIVersion {
			get {
				return "1.13";
			}
		}

		public string JQUIVersion { get; set; }

		private static string _generalUri = null;

		public static string GeneralUri {
			get {
				if (string.IsNullOrEmpty(_generalUri)) {
					_generalUri = CarrotWeb.GetWebResourceUrl("jquery.jqueryui-1-13-2.js");
				}

				return _generalUri;
			}
		}

		public override string GetHtml() {
			string sJQFile = string.Empty;
			string jqVer = JQUIVersion;

			if (!string.IsNullOrEmpty(jqVer) && jqVer.Length > 2) {
				if (jqVer.LastIndexOf(".") != jqVer.IndexOf(".")) {
					jqVer = jqVer.Substring(0, jqVer.LastIndexOf("."));
				}
			}

			switch (jqVer) {
				case "1.10":
					jqVer = "1.10.2";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jqueryui-1-10-2.js");
					break;

				case "1.11":
					jqVer = "1.11.4";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jqueryui-1-11-4.js");
					break;

				case "1.12":
					jqVer = "1.12.1";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jqueryui-1-12-1.js");
					break;

				case "1.13":
				default:
					jqVer = "1.13.2";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jqueryui-1-13-2.js");
					break;
			}

			return UrlPaths.CreateJavascriptTag(string.Format("JQuery UI v.{0}", jqVer), sJQFile).Trim();
		}
	}
}