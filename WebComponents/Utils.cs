using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

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
			//var assemblyName = assembly.ManifestModule.Name;
			//return Path.GetFileNameWithoutExtension(assemblyName);
			return assembly.GetName().Name;
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

		// =======================

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

		// =======================

		public static string GetRouteValue(this RouteValueDictionary routeValues, string key) {
			if (routeValues != null) {
				var dict = routeValues as IDictionary<string, object>;
				return routeValues.GetRouteValue(key);
			}

			return string.Empty;
		}

		public static string GetRouteValue(this RouteData routeValues, string key) {
			if (routeValues != null) {
				var dict = routeValues as IDictionary<string, object>;
				return routeValues.GetRouteValue(key);
			}

			return string.Empty;
		}

		public static string GetRouteValue(this IDictionary<string, object> routeValues, string key) {
			if (routeValues.TryGetValue(key, out var keyValue) && keyValue != null) {
				return keyValue.ToString() ?? string.Empty;
			}

			return string.Empty;
		}

		public static RouteInfo GetRouteInfo(this RouteValueDictionary routeValues) {
			return new RouteInfo(routeValues);
		}

		public static string GetRouteAction(this RouteValueDictionary routeValues) {
			return new RouteInfo(routeValues).Action;
		}

		public static string GetRouteController(this RouteValueDictionary routeValues) {
			return new RouteInfo(routeValues).Controller;
		}

		public static string GetRouteArea(this RouteValueDictionary routeValues) {
			return new RouteInfo(routeValues).Area;
		}

		public static RouteInfo GetRouteInfo(this RouteData routeValues) {
			return new RouteInfo(routeValues);
		}

		public static string GetRouteAction(this RouteData routeValues) {
			return new RouteInfo(routeValues).Action;
		}

		public static string GetRouteController(this RouteData routeValues) {
			return new RouteInfo(routeValues).Controller;
		}

		public static string GetRouteArea(this RouteData routeValues) {
			return new RouteInfo(routeValues).Area;
		}

		public static RouteInfo GetRouteInfo(this IDictionary<string, object> routeValues) {
			if (routeValues == null) {
				return null;
			} else {
				return new RouteInfo(routeValues);
			}
		}

		public static string GetRouteAction(this IDictionary<string, object> routeValues) {
			return new RouteInfo(routeValues).Action;
		}

		public static string GetRouteController(this IDictionary<string, object> routeValues) {
			return new RouteInfo(routeValues).Controller;
		}

		public static string GetRouteArea(this IDictionary<string, object> routeValues) {
			return new RouteInfo(routeValues).Area;
		}

		public static RouteData AddUpdateRouting(this RouteData routeData, string key, string value) {
			string keyLower = key.ToLowerInvariant();
			if (routeData.Values.ContainsKey(keyLower)) {
				routeData.Values[keyLower] = value;
			} else {
				routeData.Values.Add(keyLower, value);
			}

			return routeData;
		}

		public static RouteData SetRouteValues(this RouteData routeData, string areaValue, string controllerValue, string actionValue, string idValue) {
			routeData.AddUpdateRouting(RouteInfo.Keys.Area, areaValue);
			routeData.AddUpdateRouting(RouteInfo.Keys.Controller, controllerValue);
			routeData.AddUpdateRouting(RouteInfo.Keys.Action, actionValue);
			routeData.AddUpdateRouting(RouteInfo.Keys.Id, idValue);

			return routeData;
		}

		public static RouteData SetRouteValues(this RouteData routeData, string controllerValue, string actionValue, string idValue) {
			return routeData.SetRouteValues(null, controllerValue, actionValue, idValue);
		}

		public static RouteData SetRouteValues(this RouteData routeData, string controllerValue, string actionValue) {
			return routeData.SetRouteValues(null, controllerValue, actionValue, null);
		}

		public static RequestContext SetContextRoutevalues(this RequestContext requestCtx, string areaValue, string controllerValue, string actionValue, string idValue) {
			//requestCtx.RouteData.Values[RouteInfo.Keys.Area] = areaValue;
			//requestCtx.RouteData.Values[RouteInfo.Keys.Controller] = controllerValue;
			//requestCtx.RouteData.Values[RouteInfo.Keys.Action] = actionValue;
			//requestCtx.RouteData.Values[RouteInfo.Keys.Id] = idValue;

			requestCtx.RouteData.AddUpdateRouting(RouteInfo.Keys.Area, areaValue);
			requestCtx.RouteData.AddUpdateRouting(RouteInfo.Keys.Controller, controllerValue);
			requestCtx.RouteData.AddUpdateRouting(RouteInfo.Keys.Action, actionValue);
			requestCtx.RouteData.AddUpdateRouting(RouteInfo.Keys.Id, idValue);

			return requestCtx;
		}

		public static RequestContext SetContextRoutevalues(this RequestContext requestCtx, string controllerValue, string actionValue, string idValue) {
			return requestCtx.SetContextRoutevalues(null, controllerValue, actionValue, idValue);
		}

		public static RequestContext SetContextRoutevalues(this RequestContext requestCtx, string controllerValue, string actionValue) {
			return requestCtx.SetContextRoutevalues(null, controllerValue, actionValue, null);
		}

		public static RequestContext SetContextRoutevalues(this RequestContext requestCtx, Dictionary<string, string> dictValues) {
			foreach (var kvp in dictValues) {
				requestCtx.RouteData.AddUpdateRouting(kvp.Key, kvp.Value);
			}

			return requestCtx;
		}

		public static string GetControllerName(this Controller controller) {
			return (controller == null) ? string.Empty : controller.GetType().Name.ToLowerInvariant().Replace(RouteInfo.Keys.Controller, string.Empty);
		}

		public static string GetControllerName(this Type type) {
			if (typeof(ControllerBase).IsAssignableFrom(type)) {
				return (type == null) ? string.Empty : type.Name.ToLowerInvariant().Replace(RouteInfo.Keys.Controller, string.Empty);
			}
			return string.Empty;
		}
	}

	//========================

	public class RouteInfo {
		private RouteValueDictionary _routeValueDictionary;
		private IDictionary<string, object> _dict;

		//===============
		public static class Keys {
			public static string Id { get { return "id"; } }
			public static string Area { get { return "area"; } }
			public static string Controller { get { return "controller"; } }
			public static string Action { get { return "action"; } }

			public static List<string> GetStandardKeys() {
				return new List<string> { Keys.Area.ToLowerInvariant(), Keys.Controller.ToLowerInvariant(), Keys.Action.ToLowerInvariant() };
			}
		}

		//===============

		public RouteInfo() { }

		public RouteInfo(RouteValueDictionary routeValues) {
			if (routeValues != null) {
				_dict = null;
				_routeValueDictionary = routeValues;
				var dict = routeValues as IDictionary<string, object>;
				Assign(dict);
			}
		}

		public RouteInfo(RouteData routeValues) {
			if (routeValues != null) {
				_dict = null;
				_routeValueDictionary = routeValues.Values;
				var dict = routeValues.Values as IDictionary<string, object>;
				Assign(dict);
			}
		}

		public RouteInfo(IDictionary<string, object> routeValues) {
			_routeValueDictionary = null;
			_dict = routeValues;
			Assign(routeValues);
		}

		private void Assign(IDictionary<string, object> routeValues) {
			if (routeValues != null) {
				if (routeValues.ContainsKey(Keys.Area)) {
					this.Area = routeValues.GetRouteValue(Keys.Area);
				}
				if (routeValues.ContainsKey(Keys.Controller)) {
					this.Controller = routeValues.GetRouteValue(Keys.Controller);
				}
				if (routeValues.ContainsKey(Keys.Action)) {
					this.Action = routeValues.GetRouteValue(Keys.Action);
				}
				if (routeValues.ContainsKey(Keys.Id)) {
					this.Id = routeValues.GetRouteValue(Keys.Id);
				}
			}
		}

		private void AssignValue(string key, string value) {
			if (_routeValueDictionary != null) {
				if (_routeValueDictionary.ContainsKey(key) == false) {
					_routeValueDictionary.Add(key, value);
				} else {
					_routeValueDictionary[key] = value;
				}
			}
			if (_dict != null) {
				if (_dict.ContainsKey(key) == false) {
					_dict.Add(key, value);
				} else {
					_dict[key] = value;
				}
			}
		}

		private void Remove(string key) {
			if (_routeValueDictionary != null) {
				if (_routeValueDictionary.ContainsKey(key) == false) {
					_routeValueDictionary.Remove(key);
				}
			}
			if (_dict != null) {
				if (_dict.ContainsKey(key) == false) {
					_dict.Remove(key);
				}
			}
		}

		public void SetArea(string value) {
			this.Area = value;
			AssignValue(Keys.Area, value);
		}

		public void SetController(string value) {
			this.Controller = value;
			AssignValue(Keys.Controller, value);
		}

		public void SetAction(string value) {
			this.Action = value;
			AssignValue(Keys.Action, value);
		}

		public void SetId(string value) {
			this.Id = value;
			AssignValue(Keys.Id, value);
		}

		public void SetArea() {
			var value = string.Empty;
			this.Area = value;
			AssignValue(Keys.Area, value);
		}

		public void SetController() {
			var value = string.Empty;
			this.Controller = value;
			AssignValue(Keys.Controller, value);
		}

		public void SetAction() {
			var value = string.Empty;
			this.Action = value;
			AssignValue(Keys.Action, value);
		}

		public void SetId() {
			var value = string.Empty;
			this.Id = value;
			AssignValue(Keys.Id, value);
		}

		public void RemoveArea() {
			var value = string.Empty;
			this.Area = value;
			Remove(Keys.Area);
		}

		public string Id { get; private set; } = string.Empty;

		public string Area { get; private set; } = string.Empty;

		public string Controller { get; private set; } = string.Empty;

		public string Action { get; private set; } = string.Empty;
	}
}