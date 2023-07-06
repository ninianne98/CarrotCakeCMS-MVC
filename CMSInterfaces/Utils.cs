using System;
using System.IO;
using System.Reflection;

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

	public static class Utils {

		public static string GetAssemblyName(this Assembly assembly) {
			var assemblyName = assembly.ManifestModule.Name;
			return Path.GetFileNameWithoutExtension(assemblyName);
		}
	}
}