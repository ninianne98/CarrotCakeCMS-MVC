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
			this.UseJqueryMigrate = false;
		}

		public static string DefaultJQVersion {
			get {
				return "1.11";
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
				case "2":
				case "2.0":
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

				case "1.9":
					jqVer = "1.9.1";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jquery-1-9-1.js");
					break;

				case "1.8":
					jqVer = "1.8.3";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jquery-1-8-3.js");
					break;

				case "1.7":
					jqVer = "1.7.2";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jquery-1-7-2.js");
					break;

				case "1":
				case "1.3":
				case "1.4":
				case "1.5":
				case "1.6":
					jqVer = "1.6.4";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jquery-1-6-4.js");
					break;

				default:
					jqVer = "1.11.3";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jquery-1-11-3.js");
					break;
			}

			sb.AppendLine("<!-- JQuery v. " + jqVer + " --> <script src=\"" + sJQFile + "\" type=\"text/javascript\"></script> ");

			if (this.UseJqueryMigrate) {
				string sJQFileMig = string.Empty;

				if (jqVer.StartsWith("1.9") || jqVer.StartsWith("1.10") || jqVer.StartsWith("1.11")) {
					sJQFileMig = CarrotWeb.GetWebResourceUrl("jquery.jquery-1-2-1-mig.js");
				}

				if (jqVer.StartsWith("1.12") || jqVer.StartsWith("1.13")) {
					sJQFileMig = CarrotWeb.GetWebResourceUrl("jquery.jquery-1-3-0-mig.js");
				}

				if (!string.IsNullOrEmpty(sJQFileMig)) {
					sb.AppendLine("<!-- jQuery Migrate Plugin --> <script src=\"" + sJQFileMig + "\" type=\"text/javascript\"></script> ");
				}
			}

			//string sVAjax = CarrotWeb.GetWebResourceUrl("jquery.validate-unobtrusive.min.js");
			//sb.AppendLine("<!-- Unobtrusive Validate --> <script src=\"" + sVAjax + "\" type=\"text/javascript\"></script> ");

			string sJqAjax = CarrotWeb.GetWebResourceUrl("jquery.jquery-unobtrusive-ajax.min.js");
			sb.AppendLine("<!-- Unobtrusive Ajax --> <script src=\"" + sJqAjax + "\" type=\"text/javascript\"></script> ");

			string key = CarrotWeb.DateKey();

			sb.AppendLine("<!-- Carrot Helpers --> <script src=\"/carrotwarehelper.ashx?ts=" + key + "\" type=\"text/javascript\"></script> ");

			return sb.ToString().Trim();
		}
	}
}