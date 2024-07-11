using System;
using System.Collections.Generic;

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

	public class WidgetBase : IWidget {

		#region IWidget Attributes

		public Guid PageWidgetID { get; set; }

		public Guid RootContentID { get; set; }

		public Guid SiteID { get; set; }

		public virtual bool EnableEdit {
			get { return false; }
		}

		public bool IsBeingEdited { get; set; }

		public bool IsDynamicInserted { get; set; }

		public string WidgetClientID { get; set; }

		public virtual Dictionary<string, string> PublicParmValues { get; set; }

		public virtual Dictionary<string, string> JSEditFunctions {
			get { return new Dictionary<string, string>(); }
		}

		public virtual void LoadData() {
			if (this.PublicParmValues == null) {
				this.PublicParmValues = new Dictionary<string, string>();
			}
		}

		#endregion IWidget Attributes

		#region Common Parser Routines

		public string GetParmValue(string sKey) {
			return this.PublicParmValues.GetParmValue(sKey);
		}

		public string GetParmValue(string sKey, string sDefault) {
			return this.PublicParmValues.GetParmValue(sKey, sDefault);
		}

		public string GetParmValue(string sKey, bool bDefault) {
			return this.PublicParmValues.GetParmValue(sKey, bDefault);
		}

		public string GetParmValue(string sKey, int iDefault) {
			return this.PublicParmValues.GetParmValue(sKey, iDefault);
		}

		public string GetParmValueDefaultEmpty(string sKey, string sDefault) {
			return this.PublicParmValues.GetParmValueDefaultEmpty(sKey, sDefault);
		}

		public List<string> GetParmValueList(string sKey) {
			return this.PublicParmValues.GetParmValueList(sKey);
		}

		#endregion Common Parser Routines
	}
}