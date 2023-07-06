using Carrotware.Web.UI.Components;
using System.IO;
using System.Xml.Serialization;

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

	public abstract class BaseWidgetModelSettings : IWidgetModelSettings {

		public BaseWidgetModelSettings() {
			this.EncodedSettings = null;
		}

		public string EncodedSettings { get; set; }

		public virtual void Persist<S>(S settings) {
			var xmlSerializer = new XmlSerializer(typeof(S));

			string xml = string.Empty;
			using (var sw = new StringWriter()) {
				xmlSerializer.Serialize(sw, settings);
				xml = sw.ToString();
			}

			this.EncodedSettings = xml.EncodeBase64();
		}

		public virtual object Restore<P>() {
			var settings = new object();

			if (!string.IsNullOrEmpty(this.EncodedSettings)) {
				string xml = this.EncodedSettings.DecodeBase64();

				var xmlSerializer = new XmlSerializer(typeof(P));
				using (var sr = new StringReader(xml)) {
					settings = xmlSerializer.Deserialize(sr);
				}
			}

			return settings;
		}
	}
}