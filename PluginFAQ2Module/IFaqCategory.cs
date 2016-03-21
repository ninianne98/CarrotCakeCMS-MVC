using System;
using System.ComponentModel.DataAnnotations;

namespace CarrotCake.CMS.Plugins.FAQ2 {

	public interface IFaqCategory {
		Guid FaqCategoryID { get; set; }

		[Required]
		[Display(Name = "Title")]
		string FAQTitle { get; set; }

		Guid? SiteID { get; set; }
	}
}