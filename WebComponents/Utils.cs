using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

	public class Utils {

		public static string DecodeBase64(string ValIn) {
			string val = String.Empty;
			if (!string.IsNullOrEmpty(ValIn)) {
				Encoding enc = Encoding.GetEncoding("ISO-8859-1"); //Western European (ISO)
				val = enc.GetString(Convert.FromBase64String(ValIn));
			}
			return val;
		}

		public static string EncodeBase64(string ValIn) {
			string val = String.Empty;
			if (!string.IsNullOrEmpty(ValIn)) {
				Encoding enc = Encoding.GetEncoding("ISO-8859-1"); //Western European (ISO)
				byte[] toEncodeAsBytes = enc.GetBytes(ValIn);
				val = Convert.ToBase64String(toEncodeAsBytes);
			}
			return val;
		}
	}
}