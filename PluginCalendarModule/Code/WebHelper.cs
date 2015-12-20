using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CarrotCake.CMS.Plugins.CalendarModule {

	public class WebHelper {

		public static string ReadEmbededScript(string filePath) {
			string sFile = String.Empty;

			Assembly _assembly = Assembly.GetExecutingAssembly();

			using (StreamReader oTextStream = new StreamReader(_assembly.GetManifestResourceStream(filePath))) {
				sFile = oTextStream.ReadToEnd();
			}

			return sFile;
		}

		private static string _areaName = null;

		public static string AssemblyName {
			get {
				if (_areaName == null) {
					Assembly asmbly = Assembly.GetExecutingAssembly();

					string assemblyName = asmbly.ManifestModule.Name;
					_areaName = assemblyName.Substring(0, assemblyName.Length - 4);
				}

				return _areaName;
			}
		}
	}
}