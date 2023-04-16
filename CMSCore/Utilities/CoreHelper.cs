using Carrotware.Web.UI.Components;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public static class CoreHelper {

		internal static string ReadEmbededScript(string sResouceName) {
			return CarrotWeb.GetManifestResourceText(typeof(CoreHelper), sResouceName);
		}

		internal static byte[] ReadEmbededBinary(string sResouceName) {
			return CarrotWeb.GetManifestResourceBytes(typeof(CoreHelper), sResouceName);
		}

		internal static string GetWebResourceUrl(string resource) {
			string sPath = string.Empty;

			try {
				sPath = CarrotWeb.GetWebResourceUrl(typeof(CoreHelper), resource);
			} catch { }

			return sPath;
		}
	}
}