using System;
using System.Web.Compilation;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Interface {

	public class WidgetActionSettingModelAttribute : Attribute {

		public WidgetActionSettingModelAttribute(string className) {
			this._field = className;
		}

		private string _field;

		public string ClassName {
			get {
				return this._field;
			}
		}

		public Object SettingsModel {
			get {
				Type typ = null;
				if (!String.IsNullOrEmpty(this.ClassName)) {
					typ = Type.GetType(this.ClassName);

					if (typ == null && this.ClassName.IndexOf(",") < 1) {
						typ = BuildManager.GetType(this.ClassName, true);
					}
				}

				return Activator.CreateInstance(typ);
			}
		}
	}
}