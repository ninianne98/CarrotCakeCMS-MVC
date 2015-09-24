<%@ Application Language="C#" %>

<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Web.Mvc" %>
<%@ Import Namespace="System.Web.Routing" %>
<%@ Import Namespace="System.Web.Configuration" %>
<%@ Import Namespace="System.Configuration" %>

<%@ Import Namespace="Carrotware.CMS.Mvc.UI.Admin" %>
<%@ Import Namespace="Carrotware.CMS.Core" %>


<script RunAt="server" Language="C#">
/*
	 use a code file vs compiled so folks can add stuff if needed
*/
	protected void Application_Start() {
		AreaRegistration.RegisterAllAreas();

		RouteConfig.RegisterRoutes(RouteTable.Routes);

		ControllerBuilder.Current.SetControllerFactory(CmsControllerFactory.GetFactory());
	}

		
</script>
