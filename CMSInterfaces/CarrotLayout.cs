using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

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

	public static class CarrotLayout {

		public static HtmlHelper Html {
			get { return ((WebViewPage)WebPageContext.Current.Page).Html; }
		}

		public static string Main {
			get {
				return ConfigurationManager.AppSettings["LayoutMain"] != null
					? ConfigurationManager.AppSettings["LayoutMain"].ToString()
					: "~/Views/Shared/_LayoutModule.cshtml";
			}
		}

		public static string Popup {
			get {
				return ConfigurationManager.AppSettings["LayoutPopup"] != null
					? ConfigurationManager.AppSettings["LayoutPopup"].ToString()
					: "~/Views/CmsAdmin/_LayoutPopup.cshtml";
			}
		}

		public static string Public {
			get {
				return "~/Views/CmsAdmin/_LayoutPublic.cshtml";
			}
		}

		public static string PopupFunction {
			get {
				return ConfigurationManager.AppSettings["LayoutPopupOpenFunction"] != null
					? ConfigurationManager.AppSettings["LayoutPopupOpenFunction"].ToString()
					: "ShowWindowNoRefresh";
			}
		}

		public static HtmlString WritePopupLink(string Uri) {
			return new HtmlString(string.Format("{0}('{1}')", PopupFunction, Uri));
		}

		public static string CurrentScriptName {
			get {
				string sPath = "/";
				try { sPath = HttpContext.Current.Request.ServerVariables["script_name"].ToString(); } catch { }
				return sPath;
			}
		}
	}
}