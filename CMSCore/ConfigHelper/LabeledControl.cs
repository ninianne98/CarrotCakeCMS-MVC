using System;
using System.Web.UI;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public class LabeledControl {

		public LabeledControl() { }

		public string ControlLabel { get; set; }
		public Control KeyControl { get; set; }

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is LabeledControl) {
				LabeledControl p = (LabeledControl)obj;
				return (ControlLabel.ToLower() == p.ControlLabel.ToLower());
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return ControlLabel.GetHashCode() ^ KeyControl.GetHashCode();
		}
	}
}