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

namespace Carrotware.Web.UI.Components {

	public class jquery : BaseWebComponent {

		public jquery() {
			this.JQVersion = DefaultJQVersion;
			this.UseJqueryMigrate = DefaultJQVersion.StartsWith("3") ? true : false;
		}

		public static string DefaultJQVersion {
			get {
				return "3";
			}
		}

		public string JQVersion { get; set; }

		public bool UseJqueryMigrate { get; set; }

		private static string _generalUri = null;

		public static string GeneralUri {
			get {
				if (string.IsNullOrEmpty(_generalUri)) {
					_generalUri = CarrotWeb.GetWebResourceUrl("jquery.jquery-1-11-3.js");
				}

				return _generalUri;
			}
		}

		public override string GetHtml() {
			StringBuilder sb = new StringBuilder();

			string sJQFile = string.Empty;
			string jqVer = JQVersion;

			if (!string.IsNullOrEmpty(jqVer) && jqVer.Length > 2) {
				if (jqVer.LastIndexOf(".") != jqVer.IndexOf(".")) {
					jqVer = jqVer.Substring(0, jqVer.LastIndexOf("."));
				}
			}

			switch (jqVer) {
				case "3":
				case "3.0":
				case "3.6":
				case "3.7":
					jqVer = "3.7.1";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jquery-3-7-1.js");
					break;

				case "2":
				case "2.0":
				case "2.2":
					jqVer = "2.2.4";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jquery-2-2-4.js");
					break;

				case "1":
				case "1.0":
				case "1.11":
					jqVer = "1.11.3";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jquery-1-11-3.js");
					break;

				case "1.12":
					jqVer = "1.12.4";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jquery-1-12-4.js");
					break;

				case "1.10":
					jqVer = "1.10.2";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jquery-1-10-2.js");
					break;

				// older versions get dumped to 1.9
				case "1.1":
				case "1.2":
				case "1.3":
				case "1.4":
				case "1.5":
				case "1.6":
				case "1.7":
				case "1.8":
				case "1.9":
					jqVer = "1.9.1";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jquery-1-9-1.js");
					break;

				// if you didn't provide a version or a meaningful version, this is what you get
				default:
					jqVer = "3.7.1";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jquery-3-7-1.js");
					break;
			}

			sb.AppendLine(UrlPaths.CreateJavascriptTag(string.Format("JQuery v.{0}", jqVer), sJQFile));

			if (this.UseJqueryMigrate) {
				string sJQFileMig = string.Empty;

				if (jqVer.StartsWith("1.9") || jqVer.StartsWith("1.10") || jqVer.StartsWith("1.11")) {
					sJQFileMig = CarrotWeb.GetWebResourceUrl("jquery.jquery-mig-1-2-1.js");
				}

				if (jqVer.StartsWith("1.12") || jqVer.StartsWith("1.13")) {
					sJQFileMig = CarrotWeb.GetWebResourceUrl("jquery.jquery-mig-1-3-0.js");
				}

				if (jqVer.StartsWith("3")) {
					sJQFileMig = CarrotWeb.GetWebResourceUrl("jquery.jquery-mig-3-4-0.js");
				}

				if (!string.IsNullOrEmpty(sJQFileMig)) {
					sb.AppendLine(UrlPaths.CreateJavascriptTag("jQuery Migrate Plugin", sJQFileMig));
				}
			}

			//string sVAjax = CarrotWeb.GetWebResourceUrl("jquery.validate-unobtrusive.min.js");
			//sb.AppendLine(UrlPaths.CreateJavascriptTag("Unobtrusive Validate", sVAjax));

			string sJqAjax = CarrotWeb.GetWebResourceUrl("jquery.jquery-unobtrusive-ajax.min.js");
			sb.AppendLine(UrlPaths.CreateJavascriptTag("Unobtrusive Ajax", sJqAjax));

			string key = CarrotWeb.DateKey();

			sb.AppendLine(UrlPaths.CreateJavascriptTag("Carrot Helpers", string.Format("{0}?ts={1}&v={2}", UrlPaths.HelperPath, CarrotWeb.DateKey(), CarrotWeb.FileVersion.Replace(".", string.Empty))));

			return sb.ToString().Trim();
		}
	}
}