using System;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Models {
	public class MathModel {

		public MathModel() {
		}

		[Required]
		public double Number1 { get; set; } = 25;

		[Required]
		public double Number2 { get; set; } = 5;

		public double? Number3 { get; set; }

		public double GetResult() {
			if (this.Number2 > -0.2 && this.Number2 < 0.2) {
				this.Number3 = Convert.ToInt32(this.Number1) / Convert.ToInt32(this.Number2);
			} else {
				this.Number3 = this.Number1 / this.Number2;
			}

			return this.Number3.Value;
		}

	}
}
