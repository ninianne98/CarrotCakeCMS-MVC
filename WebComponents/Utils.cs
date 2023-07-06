using System;
using System.IO;
using System.Reflection;
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

	public static class Utils {

		internal static string GetAssemblyName(this Assembly assembly) {
			var assemblyName = assembly.ManifestModule.Name;
			return Path.GetFileNameWithoutExtension(assemblyName);
		}

		public static string ScrubQueryElement(this string text) {
			return text.Replace("{", "").Replace(">", "").Replace("<", "").Replace(">", "")
										.Replace("'", "").Replace("\\", "").Replace("//", "").Replace(":", "");
		}

		public static string SafeQueryString(this HttpContext context, string key) {
			return SafeQueryString(context, key, string.Empty);
		}

		public static string SafeQueryString(this HttpContext context, string key, string defaultVal) {
			if (context.Request.QueryString[key] != null) {
				return context.Request.QueryString[key].ToString();
			}
			return defaultVal;
		}

		public static string DecodeBase64(this string text) {
			string val = string.Empty;
			if (!string.IsNullOrEmpty(text)) {
				Encoding enc = Encoding.GetEncoding("ISO-8859-1"); //Western European (ISO)
				val = enc.GetString(Convert.FromBase64String(text));
			}
			return val;
		}

		public static string EncodeBase64(this string text) {
			string val = string.Empty;
			if (!string.IsNullOrEmpty(text)) {
				Encoding enc = Encoding.GetEncoding("ISO-8859-1"); //Western European (ISO)
				byte[] toEncodeAsBytes = enc.GetBytes(text);
				val = Convert.ToBase64String(toEncodeAsBytes);
			}
			return val;
		}
	}
}