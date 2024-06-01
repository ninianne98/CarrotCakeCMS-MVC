using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
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

namespace CarrotCake.CMS.Plugins.EventCalendarModule {

	public static class WebHelper {

		public static List<CMSAdminModuleMenu> GetMenuEntries() {
			DataSet ds = new DataSet();
			ds.ReadXml(HttpContext.Current.Server.MapPath("~/Views/Admin.config"));

			var lst = (from d in ds.Tables[1].AsEnumerable()
					   select new CMSAdminModuleMenu {
						   Caption = d.Field<string>("pluginlabel"),
						   SortOrder = string.IsNullOrEmpty(d.Field<string>("menuorder")) ? -1 : int.Parse(d.Field<string>("menuorder")),
						   Action = d.Field<string>("action"),
						   Controller = d.Field<string>("controller"),
						   UsePopup = (d.Table.Columns.Contains("usepopup") == false || string.IsNullOrEmpty(d.Field<string>("usepopup"))) ? false : Convert.ToBoolean(d.Field<string>("usepopup")),
						   IsVisible = (d.Table.Columns.Contains("visible") == false || string.IsNullOrEmpty(d.Field<string>("visible"))) ? false : Convert.ToBoolean(d.Field<string>("visible")),
						   AreaKey = d.Field<string>("area")
					   }).OrderBy(x => x.Caption).OrderBy(x => x.SortOrder).ToList();

			return lst;
		}

		public static string ReadEmbededScript(string resouceName) {
			string ret = null;

			Assembly a = Assembly.GetExecutingAssembly();
			using (var stream = new StreamReader(a.GetManifestResourceStream(resouceName))) {
				ret = stream.ReadToEnd();
			}

			return ret;
		}

		private static string _areaName = null;

		public static string AssemblyName {
			get {
				if (_areaName == null) {
					Assembly a = Assembly.GetExecutingAssembly();

					_areaName = a.GetAssemblyName();
				}

				return _areaName;
			}
		}

		private static string _shortDatePattern = null;

		public static string ShortDatePattern {
			get {
				if (_shortDatePattern == null) {
					DateTimeFormatInfo _dtf = CultureInfo.CurrentCulture.DateTimeFormat;
					if (_dtf == null) {
						_dtf = CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat;
					}

					_shortDatePattern = _dtf.ShortDatePattern ?? "M/d/yyyy";
					_shortDatePattern = _shortDatePattern.Replace("MM", "M").Replace("dd", "d");
				}

				return _shortDatePattern;
			}
		}

		public static string ShortDateFormatPattern {
			get {
				return "{0:" + ShortDatePattern + "}";
			}
		}

		private static string _shortTimePattern = null;

		public static string ShortTimePattern {
			get {
				if (_shortTimePattern == null) {
					DateTimeFormatInfo dtf = CultureInfo.CurrentCulture.DateTimeFormat;
					if (dtf == null) {
						dtf = CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat;
					}
					_shortTimePattern = dtf.ShortTimePattern ?? "hh:mm tt";
				}

				return _shortTimePattern;
			}
		}

		public static string ShortTimeFormatPattern {
			get {
				return "{0:" + ShortTimePattern.Replace(":", "\\:") + "}";
			}
		}
	}
}