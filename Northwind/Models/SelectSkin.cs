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

			var options = new List<SkinItem>();

			foreach (Bootstrap.BootstrapColorScheme enumValue in Enum.GetValues(typeof(Bootstrap.BootstrapColorScheme))) {
				options.Add(new SkinItem(enumValue));
			}

			this.Options = options.OrderBy(x => x.SkinName).ToList();
		}

		[Display(Name = "Option List")]
		public List<SkinItem> Options { get; set; }

		[Display(Name = "Selected Skin")]
		public Bootstrap.BootstrapColorScheme SelectedItem { get; set; }
	}
}