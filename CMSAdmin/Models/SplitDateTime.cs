using System;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class SplitDateTime {

		public SplitDateTime() {
			SetTimeStrings();
		}

		protected void SetTimeStrings() {
			DateTime dtModel = DateTime.MinValue;

			if (_combinedDateTime.HasValue) {
				dtModel = _combinedDateTime.Value;

				this.ValueDateString = dtModel.Date.ToString(Helper.ShortDatePattern);
				this.ValueTimeString = dtModel.ToString(Helper.ShortTimePattern);
				this.ValueDateAllString = String.Format("{0} {1}", this.ValueDateString, this.ValueTimeString);
			} else {
				this.ValueDateString = String.Empty;
				this.ValueTimeString = String.Empty;
				this.ValueDateAllString = String.Empty;
			}
		}

		public string FieldName { get; set; }
		public string TimeID { get { return String.Format("{0}_Time", this.FieldName); } }
		public string DateID { get { return String.Format("{0}_Date", this.FieldName); } }
		public string FieldID { get { return this.FieldName.Replace(".", "_").Replace("[", "_").Replace("]", "_"); } }

		private DateTime? _combinedDateTime = null;

		public DateTime? CombinedDateTime {
			get {
				SetTimeStrings();

				return _combinedDateTime;
			}

			set {
				_combinedDateTime = value;

				SetTimeStrings();
			}
		}

		public string ValueDateAllString { get; set; }
		public string ValueTimeString { get; set; }
		public string ValueDateString { get; set; }
	}
}