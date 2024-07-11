using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

		public static void SetGuidValue<T>(this T widgetObject, Expression<Func<T, Guid>> widgetProperty, Guid? value) {
			SetGuidValue(widgetObject, widgetProperty, value, Guid.Empty);
		}

		public static void SetGuidValue<T>(this T widgetObject, Expression<Func<T, Guid>> widgetProperty, Guid? value, Guid defaultValue) {
			Guid val = defaultValue;
			if (value.HasValue) {
				val = value.Value;
			}

			SetValue(widgetObject, widgetProperty, val);
		}

		public static void SetStringValue<T>(this T widgetObject, Expression<Func<T, string>> widgetProperty, string value) {
			SetStringValue(widgetObject, widgetProperty, value, string.Empty);
		}

		public static void SetStringValue<T>(this T widgetObject, Expression<Func<T, string>> widgetProperty, string value, string defaultValue) {
			string val = defaultValue;
			if (!string.IsNullOrEmpty(value)) {
				val = value;
			}

			SetValue(widgetObject, widgetProperty, val);
		}

		public static void SetIntValue<T>(this T widgetObject, Expression<Func<T, int>> widgetProperty, int? value) {
			SetIntValue(widgetObject, widgetProperty, value, 0);
		}

		public static void SetIntValue<T>(this T widgetObject, Expression<Func<T, int>> widgetProperty, int? value, int defaultValue) {
			int val = defaultValue;
			if (value.HasValue) {
				val = value.Value;
			}

			SetValue(widgetObject, widgetProperty, val);
		}

		public static void SetBoolValue<T>(this T widgetObject, Expression<Func<T, bool>> widgetProperty, bool? value) {
			SetBoolValue(widgetObject, widgetProperty, value, false);
		}

		public static void SetBoolValue<T>(this T widgetObject, Expression<Func<T, bool>> widgetProperty, bool? value, bool defaultValue) {
			bool val = defaultValue;
			if (value.HasValue) {
				val = value.Value;
			}

			SetValue(widgetObject, widgetProperty, val);
		}

		public static void SetValue<T, TValue>(this T widgetObject, Expression<Func<T, TValue>> widgetProperty, TValue value) {
			var selector = widgetProperty.Body as MemberExpression;
			if (selector != null) {
				var property = selector.Member as PropertyInfo;
				if (property != null) {
					property.SetValue(widgetObject, value, null);
				}
			}
		}

		public static string GetValue<T, TValue>(this T widgetObject, Expression<Func<T, TValue>> widgetProperty) {
			return GetValue(widgetObject, widgetProperty, string.Empty);
		}

		public static string GetStringValue<T, TValue>(this T widgetObject, Expression<Func<T, TValue>> widgetProperty) {
			return GetValue(widgetObject, widgetProperty, string.Empty);
		}

		public static string GetValue<T, TValue>(this T widgetObject, Expression<Func<T, TValue>> widgetProperty, string defaultVal) {
			string foundVal = defaultVal;
			var selector = widgetProperty.Body as MemberExpression;

			if (selector != null && widgetObject is IWidget) {
				var property = selector.Member as PropertyInfo;
				foundVal = ((IWidget)widgetObject).PublicParmValues.GetParmValue(property.Name, defaultVal);
			}

			return foundVal;
		}

		public static int GetIntValue<T, TValue>(this T widgetObject, Expression<Func<T, TValue>> widgetProperty) {
			return GetValue(widgetObject, widgetProperty, 0);
		}

		public static int GetValue<T, TValue>(this T widgetObject, Expression<Func<T, TValue>> widgetProperty, int defaultVal) {
			int foundVal = defaultVal;
			var selector = widgetProperty.Body as MemberExpression;

			if (selector != null && widgetObject is IWidget) {
				var property = selector.Member as PropertyInfo;
				var foundString = ((IWidget)widgetObject).PublicParmValues.GetParmValue(property.Name, defaultVal);
				if (!string.IsNullOrEmpty(foundString)) {
					foundVal = Convert.ToInt32(foundString);
				}
			}

			return foundVal;
		}

		public static bool GetBoolValue<T, TValue>(this T widgetObject, Expression<Func<T, TValue>> widgetProperty) {
			return GetValue(widgetObject, widgetProperty, false);
		}

		public static bool GetValue<T, TValue>(this T widgetObject, Expression<Func<T, TValue>> widgetProperty, bool defaultVal) {
			bool foundVal = defaultVal;
			var selector = widgetProperty.Body as MemberExpression;

			if (selector != null && widgetObject is IWidget) {
				var property = selector.Member as PropertyInfo;
				var foundString = ((IWidget)widgetObject).PublicParmValues.GetParmValue(property.Name, defaultVal);
				if (!string.IsNullOrEmpty(foundString)) {
					foundVal = Convert.ToBoolean(foundString);
				}
			}

			return foundVal;
		}

		public static Guid GetGuidValue<T, TValue>(this T widgetObject, Expression<Func<T, TValue>> widgetProperty) {
			return GetValue(widgetObject, widgetProperty, Guid.Empty);
		}

		public static Guid GetValue<T, TValue>(this T widgetObject, Expression<Func<T, TValue>> widgetProperty, Guid defaultVal) {
			Guid foundVal = defaultVal;
			var selector = widgetProperty.Body as MemberExpression;

			if (selector != null && widgetObject is IWidget) {
				var property = selector.Member as PropertyInfo;
				var foundString = ((IWidget)widgetObject).PublicParmValues.GetParmValue(property.Name, defaultVal.ToString());
				if (!string.IsNullOrEmpty(foundString)) {
					foundVal = new Guid(foundString);
				}
			}

			return foundVal;
		}

		public static string GetParmValue(this Dictionary<string, string> parmDictionary, string key) {
			string ret = null;

			if (parmDictionary.Any()) {
				ret = (from c in parmDictionary
					   where c.Key.ToLowerInvariant() == key.ToLowerInvariant()
					   select c.Value).FirstOrDefault();
			}

			return ret;
		}

		public static string GetParmValue(this Dictionary<string, string> parmDictionary, string key, string defaultValue) {
			string ret = null;

			if (parmDictionary.Any()) {
				ret = (from c in parmDictionary
					   where c.Key.ToLowerInvariant() == key.ToLowerInvariant()
					   select c.Value).FirstOrDefault();
			}

			ret = ret == null ? defaultValue : ret;

			return ret;
		}

		public static string GetParmValue(this Dictionary<string, string> parmDictionary, string key, bool defaultValue) {
			return parmDictionary.GetParmValue(key, defaultValue.ToString().ToLowerInvariant());
		}

		public static string GetParmValue(this Dictionary<string, string> parmDictionary, string key, int defaultValue) {
			return parmDictionary.GetParmValue(key, defaultValue.ToString());
		}

		public static string GetParmValueDefaultEmpty(this Dictionary<string, string> parmDictionary, string key, string defaultValue) {
			string ret = GetParmValue(parmDictionary, key, defaultValue);

			ret = string.IsNullOrEmpty(ret) ? defaultValue : ret;

			return ret;
		}

		public static List<string> GetParmValueList(this Dictionary<string, string> parmDictionary, string key) {
			key = key.EndsWith("|") ? key : key + "|";
			key = key.ToLowerInvariant();

			List<string> ret = new List<string>();

			if (parmDictionary.Any()) {
				ret = (from c in parmDictionary
					   where c.Key.ToLowerInvariant().StartsWith(key)
					   select c.Value).ToList();
			}

			return ret;
		}

		#region QueryString Parsers

		public static bool IsWebView {
			get { return (HttpContext.Current != null); }
		}

		public static Guid GetGuidParameterFromQuery(string parmName) {
			Guid id = Guid.Empty;
			if (HttpContext.Current.Request.QueryString[parmName] != null
				&& !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString[parmName].ToString())) {
				id = new Guid(HttpContext.Current.Request.QueryString[parmName].ToString());
			}
			return id;
		}

		public static string GetStringParameterFromQuery(string parmName) {
			string id = string.Empty;
			if (HttpContext.Current.Request.QueryString[parmName] != null
				&& !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString[parmName].ToString())) {
				id = HttpContext.Current.Request.QueryString[parmName].ToString();
			}
			return id;
		}

		#endregion QueryString Parsers
	}
}