using Carrotware.Web.UI.Components;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

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
			string path = string.Empty;

			try {
				path = CarrotWeb.GetWebResourceUrl(typeof(CoreHelper), resource);
			} catch { }

			return path;
		}

		public static XmlReaderSettings GetXmlReaderSettings() {
			var settings = new XmlReaderSettings {
				ConformanceLevel = ConformanceLevel.Fragment
			};

			return settings;
		}

		public static XmlWriterSettings GetXmlWriterSettings() {
			var settings = new XmlWriterSettings {
				OmitXmlDeclaration = true,
				Indent = true
			};

			return settings;
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