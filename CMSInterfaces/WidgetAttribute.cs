using System;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Interface {

	public class WidgetAttribute : Attribute {

		public enum FieldMode {
			Unknown,
			DictionaryList,
			DropDownList,
			CheckBoxList,
			TextBox,
			MultiLineTextBox,
			RichHTMLTextBox,
			CheckBox
		}

		public WidgetAttribute() {
			this._mode = FieldMode.Unknown;
		}

		public WidgetAttribute(FieldMode mode) {
			this._mode = mode;
		}

		public WidgetAttribute(FieldMode mode, string field) {
			this._mode = mode;
			this._field = field;
		}

		private FieldMode _mode;

		public FieldMode Mode {
			get {
				return this._mode;
			}
		}

		private string _field;

		public string SelectFieldSource {
			get {
				return this._field;
			}
		}
	}
}