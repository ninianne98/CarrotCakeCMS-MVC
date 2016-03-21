using System;
using System.ComponentModel.DataAnnotations;

namespace CarrotCake.CMS.Plugins.FAQ2 {

	public interface IFaqItem {

		[Required]
		[Display(Name = "A")]
		string Answer { get; set; }

		[Required]
		[Display(Name = "Caption")]
		string Caption { get; set; }

		Guid FaqCategoryID { get; set; }
		Guid FaqItemID { get; set; }

		[Required]
		[Display(Name = "Is Active")]
		bool IsActive { get; set; }

		[Required]
		[Display(Name = "Item Order")]
		int ItemOrder { get; set; }

		[Required]
		[Display(Name = "Q")]
		string Question { get; set; }
	}
}