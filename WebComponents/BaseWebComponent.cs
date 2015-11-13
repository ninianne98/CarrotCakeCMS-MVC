using System;
using System.Web;

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

	public abstract class BaseWebComponent : IWebComponent, IHtmlString {

		public static string GetWebResourceUrl(Type type, string resource) {
			return CarrotWeb.GetWebResourceUrl(type, resource);
		}

		protected string CurrentScriptName {
			get { return HttpContext.Current.Request.ServerVariables["script_name"].ToString(); }
		}

		public bool IsPostBack {
			get {
				string requestMethod = "POST";
				try { requestMethod = HttpContext.Current.Request.ServerVariables["request_method"].ToString().ToUpper(); } catch { }
				return requestMethod == "POST" ? true : false;
			}
		}

		public bool IsWebView {
			get { return (HttpContext.Current != null); }
		}

		public virtual string GetHtml() {
			return String.Empty;
		}

		public virtual HtmlString RenderHtml() {
			return new HtmlString(GetHtml());
		}

		public virtual string ToHtmlString() {
			return GetHtml();
		}
	}
}