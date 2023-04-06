using Carrotware.Web.UI.Components;
using Northwind.Code;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Northwind.Models {

	public class SelectSkin {

		public SelectSkin() {
			this.SelectedItem = Helper.BootstrapColorScheme;

			this.Options = new Dictionary<string, string>();

			foreach (Bootstrap.BootstrapColorScheme enumValue in Enum.GetValues(typeof(Bootstrap.BootstrapColorScheme))) {
				this.Options.Add(enumValue.ToString(), enumValue.ToString());
			}
		}

		[Display(Name = "Option List")]
		public Dictionary<string, string> Options { get; set; }

		[Display(Name = "Selected Skin")]
		public Bootstrap.BootstrapColorScheme SelectedItem { get; set; }
	}
}