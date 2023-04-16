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

	public static class UrlPaths {

		public static string ResourcePath {
			get {
				return "/carrotwarewebresource.ashx";
			}
		}

		public static string CaptchaPath {
			get {
				return "/carrotwarecaptcha.ashx";
			}
		}

		public static string ThumbnailPath {
			get {
				return "/carrotwarethumb.ashx";
			}
		}

		public static string CalendarStylePath {
			get {
				return "/carrotwarecalendarstyle.ashx";
			}
		}

		public static string HelperPath {
			get {
				return "/carrotwarehelper.ashx";
			}
		}

		//==========================

		public static string CreateCssTag(string comment, string uri) {
			return "<!-- " + HttpUtility.HtmlEncode(comment) + " --> " + CreateCssTag(uri);
		}

		public static string CreateCssTag(string uri) {
			return "<link href=\"" + uri + "\" type=\"text/css\" rel=\"stylesheet\" />";
		}

		public static string CreateJavascriptTag(string comment, string uri) {
			return "<!-- " + HttpUtility.HtmlEncode(comment) + " --> " + CreateJavascriptTag(uri);
		}

		public static string CreateJavascriptTag(string uri) {
			return "<script src=\"" + uri + "\" type=\"text/javascript\"></script>";
		}
	}
}