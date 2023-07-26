using Carrotware.CMS.Interface;
using CarrotCake.CMS.Plugins.EventCalendarModule;
using System.Web.Mvc;
using System.Web.Routing;

namespace CarrotCake.CMS.Plugins.CalendarModule {
	public class MvcApplication : System.Web.HttpApplication {
		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();
			RouteConfig.RegisterRoutes(RouteTable.Routes);

			ControllerBuilder.Current.SetControllerFactory(CmsTestControllerFactory.GetFactory());

		}
	}
}