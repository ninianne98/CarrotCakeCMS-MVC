using Carrotware.CMS.Interface;
using System;
using System.Collections.Generic;
using System.Web;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.UI.Components {

	public abstract class BaseToolboxComponent : IWidget, IHtmlString {

		public virtual string ToHtmlString() {
			return String.Empty;
		}

		public Guid PageWidgetID { get; set; }

		public Guid RootContentID { get; set; }

		public Guid SiteID { get; set; }

		public virtual bool EnableEdit {
			get { return false; }
		}

		public bool IsBeingEdited { get; set; }

		public string WidgetClientID { get; set; }

		public virtual string AlternateViewFile { get; set; }

		public bool IsDynamicInserted { get; set; }

		public Dictionary<string, string> PublicParmValues { get; set; }

		public virtual Dictionary<string, string> JSEditFunctions {
			get {
				return new Dictionary<string, string>();
			}
		}

		public virtual void LoadData() {
			if (this.PublicParmValues == null) {
				this.PublicParmValues = new Dictionary<string, string>();
			}

			if (!String.IsNullOrEmpty(this.AlternateViewFile)) {
				throw new Exception("AlternateViewFile is not a valid assignment for this widget.");
			}
		}

		#region Common Parser Routines

		public string GetParmValue(string sKey) {
			return ParmParser.GetParmValue(this.PublicParmValues, sKey);
		}

		public string GetParmValue(string sKey, string sDefault) {
			return ParmParser.GetParmValue(this.PublicParmValues, sKey, sDefault);
		}

		public string GetParmValueDefaultEmpty(string sKey, string sDefault) {
			return ParmParser.GetParmValueDefaultEmpty(this.PublicParmValues, sKey, sDefault);
		}

		public List<string> GetParmValueList(string sKey) {
			return ParmParser.GetParmValueList(this.PublicParmValues, sKey);
		}

		#endregion Common Parser Routines
	}
}