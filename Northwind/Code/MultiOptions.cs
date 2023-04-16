using Carrotware.CMS.Interface;
using Northwind.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Northwind {

	public class MultiOptions : WidgetActionSettingModel {

		public MultiOptions()
			: base() {
			this.CategoryIDs = new List<int>();
		}

		public override bool EnableEdit {
			get {
				return true;
			}
		}

		[Widget(WidgetAttribute.FieldMode.CheckBoxList, "lstCategoryID")]
		public List<int> CategoryIDs { get; set; }

		[Widget(WidgetAttribute.FieldMode.DictionaryList)]
		public Dictionary<string, string> lstCategoryID {
			get {
				Dictionary<string, string> _dict = null;

				using (var db = new NorthwindDataContext()) {
					_dict = (from c in db.Categories.ToList()
							 orderby c.CategoryName
							 select c).ToList().ToDictionary(k => k.CategoryID.ToString(), v => v.CategoryName);
				}

				return _dict;
			}
		}

		public override void LoadData() {
			base.LoadData();

			try {
				List<string> foundValues = this.GetParmValueList("CategoryIDs");

				if (foundValues.Any()) {
					this.CategoryIDs = foundValues.Select(x => int.Parse(x)).ToList();
				}
			} catch (Exception ex) { }
		}

		public ProductSearch GetData() {
			ProductSearch model = new ProductSearch();
			LoadData();

			using (var db = new NorthwindDataContext()) {
				if (this.CategoryIDs.Any()) {
					model.Options = (from c in db.Categories
									 where this.CategoryIDs.Contains(c.CategoryID)
									 select c).ToList();

					model.Results = (from p in db.Products
									 where this.CategoryIDs.Contains(p.CategoryID.Value)
									 select p).ToList();
				}
			}
			return model;
		}
	}
}