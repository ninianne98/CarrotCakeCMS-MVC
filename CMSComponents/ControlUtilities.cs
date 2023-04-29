using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;
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

namespace Carrotware.CMS.UI.Components {

	public static class ControlUtilities {

		public static string ReadEmbededScript(string sResouceName) {
			return CarrotWeb.GetManifestResourceText(typeof(ControlUtilities), sResouceName);
		}

		public static byte[] ReadEmbededBinary(string sResouceName) {
			return CarrotWeb.GetManifestResourceBytes(typeof(ControlUtilities), sResouceName);
		}

		public static string GetWebResourceUrl(string resource) {
			string sPath = string.Empty;

			try {
				sPath = CarrotWeb.GetWebResourceUrl(typeof(ControlUtilities), resource);
			} catch { }

			return sPath;
		}

		public static List<SiteNav> TweakData(List<SiteNav> navs) {
			return CMSConfigHelper.TweakData(navs);
		}

		public static SiteNav FixNavLinkText(SiteNav nav) {
			return CMSConfigHelper.FixNavLinkText(nav);
		}

		public static List<SiteNav> GetPageNavTree() {
			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				return navHelper.GetPageCrumbNavigation(SiteData.CurrentSiteID, SiteData.AlternateCurrentScriptName, !SecurityData.IsAuthEditor);
			}
		}

		public static SiteNav GetParentPage() {
			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				SiteNav pageNav = navHelper.GetParentPageNavigation(SiteData.CurrentSiteID, SiteData.AlternateCurrentScriptName);

				//assign bogus page name for comp purposes
				if (pageNav == null || pageNav.SiteID == Guid.Empty) {
					pageNav = new SiteNav();
					pageNav.Root_ContentID = Guid.Empty;
					pageNav.FileName = "/##/";
					pageNav.TemplateFile = "/##/";
				}

				return pageNav;
			}
		}

		public static SiteNav GetCurrentPage() {
			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				SiteNav pageNav = navHelper.FindByFilename(SiteData.CurrentSiteID, SiteData.AlternateCurrentScriptName);
				//assign bogus page name for comp purposes
				if (pageNav == null) {
					pageNav = new SiteNav();
					pageNav.Root_ContentID = Guid.Empty;
					pageNav.FileName = "/##/##/";
					pageNav.TemplateFile = "/##/##/";
				}

				pageNav.SiteID = SiteData.CurrentSiteID;

				return pageNav;
			}
		}

		public static string GetParentPageName() {
			SiteNav nav = ControlUtilities.GetParentPage();

			return nav.FileName.ToLowerInvariant();
		}

		public static bool AreFilenamesSame(string sParm1, string sParm2) {
			if (sParm1 == null || sParm2 == null) {
				return false;
			}

			return (sParm1.ToLowerInvariant() == sParm2.ToLowerInvariant()) ? true : false;
		}

		public static string HtmlFormat(StringBuilder input) {
			return CarrotWeb.HtmlFormat(input);
		}

		public static string HtmlFormat(string input) {
			return CarrotWeb.HtmlFormat(input);
		}
	}

	//===========================

	public class WrapperForHtmlHelper<T, U> : IViewDataContainer {
		public ViewDataDictionary ViewData { get; set; }

		public WrapperForHtmlHelper(T type, ViewDataDictionary<U> viewdata) {
			this.ViewData = new ViewDataDictionary<T>(type);

			foreach (var item in viewdata.ModelState) {
				if (!this.ViewData.ModelState.Keys.Contains(item.Key)) {
					this.ViewData.ModelState.Add(item);
				}
			}
		}
	}

	public class WrapperForHtmlHelper<T> : IViewDataContainer {
		public ViewDataDictionary ViewData { get; set; }

		public WrapperForHtmlHelper(T type) {
			this.ViewData = new ViewDataDictionary<T>(type);
		}

		public WrapperForHtmlHelper(T type, ViewDataDictionary viewdata) {
			this.ViewData = new ViewDataDictionary<T>(type);

			foreach (var item in viewdata.ModelState) {
				if (!this.ViewData.ModelState.Keys.Contains(item.Key)) {
					this.ViewData.ModelState.Add(item);
				}
			}
		}
	}
}