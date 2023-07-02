using Carrotware.Web.UI.Components;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

		public static T Clone<T>(this T source) {
			if (object.ReferenceEquals(source, null)) {
				return default(T);
			}

			var bf = new BinaryFormatter();
			using (var ms = new MemoryStream()) {
				bf.Serialize(ms, source);
				ms.Seek(0, SeekOrigin.Begin);
				return (T)bf.Deserialize(ms);
			}
		}
	}
}