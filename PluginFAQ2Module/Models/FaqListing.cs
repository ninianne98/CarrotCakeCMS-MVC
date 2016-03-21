using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarrotCake.CMS.Plugins.FAQ2.Models {

	public class FaqListing {

		public FaqListing() {
			this.Faq = new carrot_FaqCategory();
			this.Items = new PagedData<carrot_FaqItem>();

			this.Items.InitOrderBy(x => x.ItemOrder);
			this.Items.PageSize = 20;
		}

		public carrot_FaqCategory Faq { get; set; }

		public PagedData<carrot_FaqItem> Items { get; set; }
	}
}