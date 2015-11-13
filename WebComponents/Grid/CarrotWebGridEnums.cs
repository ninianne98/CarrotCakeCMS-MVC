using System;
using System.Collections.Generic;
using System.Linq;

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

	public enum CarrotGridColumnType {
		Template,
		Standard,
		ImageEnum,
		BooleanImage,
	}

	public enum GridFormFieldType {
		Checkbox,
		TextBox,
		RadioButton,
		TextArea,
		Hidden,
	}
}