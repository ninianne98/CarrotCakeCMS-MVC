﻿using Carrotware.CMS.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace CarrotCake.CMS.Plugins.LoremIpsum {

	public class MvcApplication : System.Web.HttpApplication {

		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();
			RouteConfig.RegisterRoutes(RouteTable.Routes);

			ControllerBuilder.Current.SetControllerFactory(CmsTestControllerFactory.GetFactory());
		}
	}
}