using System;
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

namespace Carrotware.CMS.UI.Components {

	public class LiteralMessage : IHtmlString {

		public LiteralMessage() { }

		public LiteralMessage(Exception ex, string key, string path) {
			string msg = string.Empty;

			if (ex != null) {
				msg = string.Format("<pre>{0}</pre>", ex);

				if (ex.InnerException != null) {
					msg = string.Format("<pre>\r\n{0}\r\n{1}\r\n</pre>", ex, ex.InnerException);
				}
			} else {
				msg = "<p>There was an error loading the widget.</p>";
			}

			this.Message = string.Format("<div>\r\n<p><b>{0}:</b>  {1}</p>\r\n{2}</div>", key, path, msg);
		}

		public LiteralMessage(string message, string key, string path) {
			string msg = "<p>There was an error loading the widget.</p>";

			if (!string.IsNullOrWhiteSpace(message)) {
				msg = message;
			}

			this.Message = string.Format("<div>\r\n<p><b>{0}:</b>  {1}</p>\r\n{2}</div>", key, path, msg);
		}

		public string Message { get; set; }

		public string ToHtmlString() {
			return this.Message;
		}
	}
}