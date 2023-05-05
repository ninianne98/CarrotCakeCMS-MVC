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

namespace Carrotware.CMS.Interface.Controllers {

	public class BaseDataWidgetController : BaseWidgetController, IWidgetDataObject {
		public virtual Object WidgetPayload { get; set; }
	}
}