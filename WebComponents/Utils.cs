using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.Web.UI.Components {

	public class Utils {

		public static string DecodeBase64(string ValIn) {
			string val = String.Empty;
			if (!String.IsNullOrEmpty(ValIn)) {
				ASCIIEncoding encoding = new ASCIIEncoding();
				val = encoding.GetString(Convert.FromBase64String(ValIn));
			}
			return val;
		}

		public static string EncodeBase64(string ValIn) {
			string val = String.Empty;
			if (!String.IsNullOrEmpty(ValIn)) {
				ASCIIEncoding encoding = new ASCIIEncoding();
				byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes(ValIn);
				val = System.Convert.ToBase64String(toEncodeAsBytes);
			}
			return val;
		}
	}
}