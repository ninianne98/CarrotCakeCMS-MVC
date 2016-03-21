using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using System.Linq;

namespace CarrotCake.CMS.Plugins.FAQ2.Models {

	public class FaqItems {

		public FaqItems() {
			this.Faq = new carrot_FaqCategory();
			this.Items = new List<carrot_FaqItem>();

			iFaq = 1;
		}

		protected int iFaq = 1;

		[Description("#")]
		[Display(Name = "Count")]
		public int FaqCount {
			get {
				return iFaq++;
			}
		}

		public string FaqCounter() {
			return this.FaqCount.ToString();
		}

		public carrot_FaqCategory Faq { get; set; }

		public List<carrot_FaqItem> Items { get; set; }
	}
}