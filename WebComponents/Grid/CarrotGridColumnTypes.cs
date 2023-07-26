using System;
using System.Collections.Generic;
using System.Data;
using System.Web;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.Web.UI.Components {

	public class CarrotGridTemplateColumn<T> : ICarrotGridColumn, ICarrotGridColumnTemplate<T> where T : class {

		public CarrotGridTemplateColumn() {
			this.Order = 0;
			this.Mode = CarrotGridColumnType.Template;
			this.HeaderText = string.Empty;
			this.HasHeadingText = true;
		}

		public int Order { get; set; }
		public CarrotGridColumnType Mode { get; set; }
		public bool HasHeadingText { get; set; }
		public string HeaderText { get; set; }
		public bool PrettifyHeading { get; set; }
		public object HeadAttributes { get; set; }
		public object BodyAttributes { get; set; }
		public Func<T, IHtmlString> FormatTemplate { get; set; }
	}

	public class CarrotGridTableTemplateColumn : CarrotGridTemplateColumn<DataRow> {
	}

	public class CarrotGridColumn : ICarrotGridColumn, ICarrotGridColumnExt {

		public CarrotGridColumn() {
			this.Order = 0;
			this.Sortable = false;
			this.HasHeadingText = true;
			this.ColumnName = string.Empty;
			this.HeaderText = string.Empty;
			this.CellFormatString = " {0} ";

			this.HeadAttributes = null;
			this.HeadLinkAttributes = null;

			this.BodyAttributes = null;

			this.Mode = CarrotGridColumnType.Standard;
		}

		public int Order { get; set; }
		public CarrotGridColumnType Mode { get; set; }
		public bool Sortable { get; set; }
		public string ColumnName { get; set; }
		public bool HasHeadingText { get; set; }
		public string HeaderText { get; set; }
		public bool PrettifyHeading { get; set; }
		public string CellFormatString { get; set; }
		public object HeadAttributes { get; set; }
		public object HeadLinkAttributes { get; set; }
		public object BodyAttributes { get; set; }
	}

	//=========================

	public class CarrotGridImageColumn : CarrotGridColumn {
		protected string IconResourcePaperclip = "Grid.attach.png";

		public CarrotGridImageColumn()
			: base() {
			this.ImagePairs = new List<CarrotImageColumnData>();
			this.DefaultImagePath = HttpUtility.HtmlEncode(CarrotWeb.GetWebResourceUrl(IconResourcePaperclip));
			this.Mode = CarrotGridColumnType.ImageEnum;
		}

		public string DefaultImagePath { get; set; }
		public IList<CarrotImageColumnData> ImagePairs { get; set; }
		public object ImageAttributes { get; set; }
	}

	public class CarrotImageColumnData {

		public CarrotImageColumnData()
			: this(string.Empty, string.Empty, string.Empty) {
		}

		public CarrotImageColumnData(string key, string image, string label) {
			this.KeyValue = key;
			this.ImagePath = image;
			this.ImageAltText = label;
		}

		public CarrotImageColumnData(string key, string image)
			: this(key, image, key) {
		}

		public CarrotImageColumnData(object key, string image, string label)
			: this(key.ToString(), image, label) {
		}

		public CarrotImageColumnData(object key, string image)
			: this(key.ToString(), image, key.ToString()) {
		}

		public string KeyValue { get; set; }

		public string ImagePath { get; set; }

		public string ImageAltText { get; set; }
	}

	//=========================

	public class CarrotGridBooleanImageColumn : CarrotGridColumn {
		protected string IconResourceAffirm = "Grid.accept.png";
		protected string IconResourceNegative = "Grid.cancel.png";

		public CarrotGridBooleanImageColumn()
			: base() {
			this.ImageAttributes = null;

			this.ImagePathTrue = CarrotWeb.GetWebResourceUrl(IconResourceAffirm);
			this.ImagePathFalse = CarrotWeb.GetWebResourceUrl(IconResourceNegative);
			this.AlternateTextTrue = "True";
			this.AlternateTextFalse = "False";

			this.Mode = CarrotGridColumnType.BooleanImage;
		}

		public string ImagePathTrue { get; set; }
		public string ImagePathFalse { get; set; }
		public object ImageAttributes { get; set; }

		public string AlternateTextTrue { get; set; }
		public string AlternateTextFalse { get; set; }
	}
}