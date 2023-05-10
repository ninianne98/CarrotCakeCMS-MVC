using System;
using System.Collections.Generic;
using System.Linq;
using static Carrotware.Web.UI.Components.Bootstrap;

namespace Northwind.Models {

	public class SkinItem {

		public SkinItem() {
			this.Skin = BootstrapColorScheme.NotUsed;
			this.SkinName = this.Skin.ToString();
		}

		public SkinItem(BootstrapColorScheme skin) {
			this.Skin = skin;
			this.SkinName = this.Skin.ToString();
		}

		public BootstrapColorScheme Skin { get; set; }
		public string SkinName { get; set; }
	}
}