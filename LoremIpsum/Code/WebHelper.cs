using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace CarrotCake.CMS.Plugins.LoremIpsum.Code {
	public class WebHelper {
		internal static string ReadEmbededScript(string resouceName) {
			var sb = new StringBuilder();

			var assembly = Assembly.GetExecutingAssembly();
			var a_name = assembly.GetName().Name;

			if (resouceName.ToLowerInvariant().StartsWith(a_name.ToLowerInvariant()) == false) {
				resouceName = string.Format("{0}.{1}", a_name, resouceName);
			}

			using (var stream = new StreamReader(assembly.GetManifestResourceStream(resouceName))) {
				sb.Append(stream.ReadToEnd());
			}

			return sb.ToString();
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