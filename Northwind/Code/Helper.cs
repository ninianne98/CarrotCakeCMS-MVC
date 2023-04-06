using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Northwind.Code {

	public static class Helper {
		private static Bootstrap.BootstrapColorScheme? _colorScheme;

		public static void SetBootstrapColor(Bootstrap.BootstrapColorScheme color) {
			_colorScheme = color;
		}

		public static Bootstrap.BootstrapColorScheme BootstrapColorScheme {
			get {
				if (_colorScheme == null) {
					_colorScheme = Bootstrap.BootstrapColorScheme.NotUsed;
				}

				return _colorScheme.Value;
			}
		}
	}
}