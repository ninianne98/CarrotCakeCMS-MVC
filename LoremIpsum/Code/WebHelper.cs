using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CarrotCake.CMS.Plugins.LoremIpsum.Code {
	public class WebHelper {
		public static string ReadEmbededScript(string sResouceName) {
			string sReturn = null;

			Assembly _assembly = Assembly.GetExecutingAssembly();
			using (var stream = new StreamReader(_assembly.GetManifestResourceStream(sResouceName))) {
				sReturn = stream.ReadToEnd();
			}

			return sReturn;
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
					DateTimeFormatInfo _dtf = CultureInfo.CurrentCulture.DateTimeFormat;
					if (_dtf == null) {
						_dtf = CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat;
					}
					_shortTimePattern = _dtf.ShortTimePattern ?? "hh:mm tt";
				}

				return _shortTimePattern;
			}
		}

	}
}