using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Carrotware.CMS.Interface {

	public static class ParmParser {

		public static string GetParmValue(this Dictionary<string, string> parmDictionary, string sKey) {
			string ret = null;

			if (parmDictionary.Any()) {
				ret = (from c in parmDictionary
					   where c.Key.ToLowerInvariant() == sKey.ToLowerInvariant()
					   select c.Value).FirstOrDefault();
			}

			return ret;
		}

		public static string GetParmValue(this Dictionary<string, string> parmDictionary, string sKey, string sDefault) {
			string ret = null;

			if (parmDictionary.Any()) {
				ret = (from c in parmDictionary
					   where c.Key.ToLowerInvariant() == sKey.ToLowerInvariant()
					   select c.Value).FirstOrDefault();
			}

			ret = ret == null ? sDefault : ret;

			return ret;
		}

		public static string GetParmValueDefaultEmpty(this Dictionary<string, string> parmDictionary, string sKey, string sDefault) {
			string ret = GetParmValue(parmDictionary, sKey, sDefault);

			ret = String.IsNullOrEmpty(ret) ? sDefault : ret;

			return ret;
		}

		public static List<string> GetParmValueList(this Dictionary<string, string> parmDictionary, string sKey) {
			sKey = sKey.EndsWith("|") ? sKey : sKey + "|";
			sKey = sKey.ToLowerInvariant();

			List<string> ret = new List<string>();

			if (parmDictionary.Any()) {
				ret = (from c in parmDictionary
					   where c.Key.ToLowerInvariant().StartsWith(sKey)
					   select c.Value).ToList();
			}

			return ret;
		}

		#region QueryString Parsers

		public static bool IsWebView {
			get { return (HttpContext.Current != null); }
		}

		public static Guid GetGuidParameterFromQuery(string ParmName) {
			Guid id = Guid.Empty;
			if (IsWebView) {
				if (HttpContext.Current.Request.QueryString[ParmName] != null
					&& !String.IsNullOrEmpty(HttpContext.Current.Request.QueryString[ParmName].ToString())) {
					id = new Guid(HttpContext.Current.Request.QueryString[ParmName].ToString());
				}
			}
			return id;
		}

		public static string GetStringParameterFromQuery(string ParmName) {
			string id = String.Empty;
			if (IsWebView) {
				if (HttpContext.Current.Request.QueryString[ParmName] != null
					&& !String.IsNullOrEmpty(HttpContext.Current.Request.QueryString[ParmName].ToString())) {
					id = HttpContext.Current.Request.QueryString[ParmName].ToString();
				}
			}
			return id;
		}

		#endregion QueryString Parsers
	}
}