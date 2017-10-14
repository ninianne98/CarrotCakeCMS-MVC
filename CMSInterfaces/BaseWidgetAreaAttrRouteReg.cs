using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

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

	public class BaseWidgetAreaAttrRouteReg : BaseWidgetAreaReg {

		public override void RegisterArea(AreaRegistrationContext context) {
			context.Routes.MapMvcAttributeRoutes();

			base.RegisterArea(context);
		}
	}
}