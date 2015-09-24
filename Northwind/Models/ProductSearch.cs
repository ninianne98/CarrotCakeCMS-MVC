using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Northwind.Models {
	public class ProductSearch {

		public ProductSearch() {
			this.AltViewName = String.Empty;
		}

		public List<Category> Options { get; set; }

		public int? SelectedCat { get; set; }

		public List<Product> Results { get; set; }

		public string AltViewName { get; set; }

	}
}