using Carrotware.Web.UI.Components;

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