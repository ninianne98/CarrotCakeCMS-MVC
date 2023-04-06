﻿using System;

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
				return "1.11";
			}
		}

		public string JQUIVersion { get; set; }

		private static string _generalUri = null;

		public static string GeneralUri {
			get {
				if (string.IsNullOrEmpty(_generalUri)) {
					_generalUri = CarrotWeb.GetWebResourceUrl("jquery.jqueryui-1-11-4.js");
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

				case "1.9":
					jqVer = "1.9.2";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jqueryui-1-9-2.js");
					break;

				case "1.8":
					jqVer = "1.8.24";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jqueryui-1-8-24.js");
					break;

				case "1.7":
					jqVer = "1.7.3";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jqueryui-1-7-3.js");
					break;

				case "1.11":
				default:
					jqVer = "1.11.4";
					sJQFile = CarrotWeb.GetWebResourceUrl("jquery.jqueryui-1-11-4.js");
					break;
			}

			return ("<!-- JQuery UI v. " + jqVer + " --> <script src=\"" + sJQFile + "\" type=\"text/javascript\"></script> ").Trim();
		}
	}
}