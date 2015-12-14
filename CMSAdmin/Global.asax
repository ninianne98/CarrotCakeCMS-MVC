<%@ Application Language="C#" %>

<%@ Import Namespace="System" %>
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
		Helper.RegisterCmsComponents();
	}

	/*
	// if the developer wants to handle special redirects based on 404/500 etc.
	protected void Application_Error(object sender, EventArgs e) {
		try {
			Exception objErr = Server.GetLastError().GetBaseException();
			HttpContext cxt = HttpContext.Current;

			if (cxt != null) {
				cxt.Items["ErrorMessage"] = objErr.Message;
				cxt.Items["ErrorSource"] = objErr.Source;

				//Session["session_error"] = objErr;
				HttpContext.Current.Cache.Insert("CarrotSiteError", objErr, null, DateTime.Now.AddMinutes(2), Cache.NoSlidingExpiration);

				Server.ClearError();

				HttpException httpEx = null;

				if (objErr is HttpException) {
					httpEx = (HttpException)objErr;
					SiteData.PerformRedirectToErrorPage(httpEx.GetHttpCode().ToString(), cxt.Request.Path);
				}

				string sErr = "500";
				string sErrTitle = "Internal Server Error";

				if (httpEx != null && httpEx.GetHttpCode() != 500) {
					sErr = httpEx.GetHttpCode().ToString();
					sErrTitle = httpEx.Message.ToString();
				}

				SiteData.PerformRedirectToErrorPage(sErr, cxt.Request.Path);

				cxt.Response.AppendHeader("Status", "HTTP/1.1 " + sErr + " " + sErrTitle);
				cxt.Response.Cache.SetLastModified(DateTime.Today.Date);

				cxt.Response.Write(SiteData.FormatErrorOutput(objErr));

				cxt.Response.End();
			}
		} catch (Exception ex) { }
	}
	*/
</script>