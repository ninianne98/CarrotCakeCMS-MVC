using System;
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

		public static string GetWebResourceUrl(string resource) {
			return BaseWebComponent.GetWebResourceUrl(typeof(jquery), resource);
		}

		public override string GetHtml() {
			StringBuilder sb = new StringBuilder();

			string sJQFile = String.Empty;
			string jqVer = JQVersion;

			if (!String.IsNullOrEmpty(jqVer) && jqVer.Length > 2) {
				if (jqVer.LastIndexOf(".") != jqVer.IndexOf(".")) {
					jqVer = jqVer.Substring(0, jqVer.LastIndexOf("."));
				}
			}

			switch (jqVer) {
				case "2":
				case "2.0":
				case "1.11":
					jqVer = "1.11.3";
					sJQFile = GetWebResourceUrl("Carrotware.Web.UI.Components.jquery-1-11-3.js");
					break;

				case "1.10":
					jqVer = "1.10.2";
					sJQFile = GetWebResourceUrl("Carrotware.Web.UI.Components.jquery-1-10-2.js");
					break;

				case "1.9":
					jqVer = "1.9.1";
					sJQFile = GetWebResourceUrl("Carrotware.Web.UI.Components.jquery-1-9-1.js");
					break;

				case "1.8":
					jqVer = "1.8.3";
					sJQFile = GetWebResourceUrl("Carrotware.Web.UI.Components.jquery-1-8-3.js");
					break;

				case "1.7":
					jqVer = "1.7.2";
					sJQFile = GetWebResourceUrl("Carrotware.Web.UI.Components.jquery-1-7-2.js");
					break;

				case "1":
				case "1.3":
				case "1.4":
				case "1.5":
				case "1.6":
					jqVer = "1.6.4";
					sJQFile = GetWebResourceUrl("Carrotware.Web.UI.Components.jquery-1-6-4.js");
					break;

				default:
					jqVer = "1.11.3";
					sJQFile = GetWebResourceUrl("Carrotware.Web.UI.Components.jquery-1-11-3.js");
					break;
			}

			sb.AppendLine("<!-- JQuery v. " + jqVer + " --> <script src=\"" + sJQFile + "\" type=\"text/javascript\"></script> ");

			if (this.UseJqueryMigrate) {
				if (jqVer.StartsWith("1.9") || jqVer.StartsWith("1.10") || jqVer.StartsWith("1.11")) {
					sJQFile = GetWebResourceUrl("Carrotware.Web.UI.Components.jquery-1-2-1-mig.js");

					sb.AppendLine("<!-- jQuery Migrate Plugin --> <script src=\"" + sJQFile + "\" type=\"text/javascript\"></script> ");
				}
			}

			string sAjax = GetWebResourceUrl("Carrotware.Web.UI.Components.unobtrusive-ajax.js");

			sb.AppendLine("<!-- Unobtrusive Ajax --> <script src=\"" + sAjax + "\" type=\"text/javascript\"></script> ");

			return sb.ToString().Trim();
		}
	}
}