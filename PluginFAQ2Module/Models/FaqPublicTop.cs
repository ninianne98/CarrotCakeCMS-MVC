using Carrotware.CMS.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Linq;

namespace CarrotCake.CMS.Plugins.FAQ2.Models {

	public class FaqPublicTop : FaqPublic {

		public FaqPublicTop()
			: base() {
			this.TakeTop = 3;
		}

		[Description("Rows to return")]
		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public int TakeTop { get; set; }

		public override void LoadData() {
			base.LoadData();

			if (this.PublicParmValues.Count > 0) {
				this.TakeTop = 3;

				try {
					string sFoundVal = GetParmValue("TakeTop", String.Empty);

					if (!string.IsNullOrEmpty(sFoundVal)) {
						this.TakeTop = Convert.ToInt32(sFoundVal);
					}
				} catch (Exception ex) { }
			}
		}
	}
}