using System;
using System.Collections.Generic;

using System.Linq;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.Web.UI.Components {

	public interface IValidateHuman {

		bool ValidateValue(string testValue);

		string AltValidationFailText { get; set; }

		string Instructions { get; set; }
	}
}