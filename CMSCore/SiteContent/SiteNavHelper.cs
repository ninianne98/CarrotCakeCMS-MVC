using Carrotware.CMS.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public enum SiteNavMode {
		RealNav,
		MockupNav,
	}

	//=============
	public static class SiteNavFactory {

		public static ISiteNavHelper GetSiteNavHelper() {
			if (SiteData.IsWebView) {
				if ((SiteData.IsPageSampler || SiteData.IsPageReal) && !SiteData.IsCurrentPageSpecial) {
					return new SiteNavHelperMock();
				} else {
					return new SiteNavHelperReal();
				}
			} else {
				return new SiteNavHelperMock();
			}
		}

		public static ISiteNavHelper GetSiteNavHelper(SiteNavMode navMode) {
			if (navMode == SiteNavMode.RealNav) {
				return new SiteNavHelperReal();
			} else {
				return new SiteNavHelperMock();
			}
		}
	}

	//=============
	public class SiteNavHelper {

		internal static List<string> GetSiteDirectoryPaths() {
			List<string> lstContent = null;

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				lstContent = (from ct in _db.vw_carrot_Contents
							  where ct.IsLatestVersion == true
								  && ct.FileName.ToLower().EndsWith(SiteData.DefaultDirectoryFilename)
							  select ct.FileName.ToLower()).Distinct().ToList();
			}

			return lstContent;
		}

		internal static SiteNav GetEmptyHome() {
			SiteNav navData = new SiteNav();
			navData.ContentID = Guid.Empty;
			navData.Root_ContentID = Guid.Empty;
			navData.SiteID = SiteData.CurrentSiteID;
			navData.TemplateFile = SiteData.DefaultDirectoryFilename;
			navData.FileName = SiteData.DefaultDirectoryFilename;
			navData.NavMenuText = "NONE";
			navData.PageHead = "NONE";
			navData.TitleBar = "NONE";
			navData.PageActive = false;
			navData.PageText = "NO PAGE CONTENT";
			navData.EditDate = DateTime.Now.Date.AddDays(-1);
			navData.CreateDate = DateTime.Now.Date.AddDays(-10);
			navData.GoLiveDate = DateTime.Now.Date.AddDays(1);
			navData.RetireDate = DateTime.Now.Date.AddDays(90);
			navData.ContentType = ContentPageType.PageType.ContentEntry;
			return navData;
		}

		internal static List<SiteNav> GetSamplerFakeNav() {
			return GetSamplerFakeNav(4, null);
		}

		internal static List<SiteNav> GetSamplerFakeNav(int iCount) {
			return GetSamplerFakeNav(iCount, null);
		}

		internal static List<SiteNav> GetSamplerFakeNav(Guid? rootParentID) {
			return GetSamplerFakeNav(4, rootParentID);
		}

		internal static List<SiteNav> GetSamplerFakeNav(int iCount, Guid? rootParentID) {
			List<SiteNav> navList = new List<SiteNav>();
			int n = 0;

			while (n < iCount) {
				SiteNav nav = GetSamplerView();
				nav.NavOrder = n;
				nav.NavMenuText = nav.NavMenuText + " " + n.ToString();
				nav.CreateDate = nav.CreateDate.AddHours((0 - n) * 25);
				nav.EditDate = nav.CreateDate.AddHours((0 - n) * 16);
				nav.GoLiveDate = DateTime.Now.Date.AddMinutes(-5);
				nav.RetireDate = DateTime.Now.Date.AddDays(90);
				nav.CommentCount = (n * 2) + 1;
				nav.ShowInSiteNav = true;
				nav.ShowInSiteMap = true;

				if (n > 0 || rootParentID != null) {
					nav.Root_ContentID = Guid.NewGuid();
					nav.ContentID = Guid.NewGuid();
					//nav.FileName = nav.FileName.Replace(".aspx", nav.NavOrder.ToString() + ".aspx");
					nav.FileName = "/#";
					if (rootParentID != null) {
						nav.NavMenuText = nav.NavMenuText + " - " + rootParentID.Value.ToString().Substring(0, 4);
					}
				}
				nav.Parent_ContentID = rootParentID;

				navList.Add(nav);
				n++;
			}

			return navList;
		}

		internal static SiteNav GetSamplerView(Guid rootParentID) {
			var sn = GetSamplerView();

			sn.Parent_ContentID = rootParentID;

			return sn;
		}

		public static string GetSampleBody() {
			return GetSampleBody(null, "");
		}

		public static string GetSampleBody(string sContentSampleNumber) {
			return GetSampleBody(null, sContentSampleNumber);
		}

		public static string GetSampleBody(Control X, string sContentSampleNumber) { // SampleContent2
			if (String.IsNullOrEmpty(sContentSampleNumber)) {
				sContentSampleNumber = "SampleContent2";
			}

			string sFile2 = " <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus mi arcu, lacinia scelerisque blandit nec, mattis non nibh.</p> \r\n <p> Curabitur quis urna at massa placerat auctor. Quisque et mauris sapien, a consectetur nulla.</p>";

			try {
				Assembly _assembly = Assembly.GetExecutingAssembly();
				using (StreamReader oTextStream = new StreamReader(_assembly.GetManifestResourceStream("Carrotware.CMS.Core.SiteContent.Mock." + sContentSampleNumber + ".txt"))) {
					sFile2 = oTextStream.ReadToEnd();
				}

				List<string> imageNames = (from i in _assembly.GetManifestResourceNames()
										   where i.Contains("SiteContent.Mock.sample")
										   && i.EndsWith(".png")
										   select i).ToList();

				foreach (string img in imageNames) {
					var imgURL = CMSConfigHelper.GetWebResourceUrl(X, typeof(SiteNav), img);
					sFile2 = sFile2.Replace(img, imgURL);
				}
			} catch { }

			return sFile2;
		}

		internal static SiteNav GetSamplerView() {
			string sFile2 = GetSampleBody();

			SiteNav navNew = new SiteNav();
			navNew.Root_ContentID = Guid.NewGuid();
			navNew.ContentID = Guid.NewGuid();

			navNew.NavOrder = -1;
			navNew.TitleBar = "Template Preview - TITLE";
			navNew.NavMenuText = "Template PV - NAV"; ;
			navNew.PageHead = "Template Preview - HEAD";
			navNew.PageActive = true;
			navNew.ShowInSiteNav = true;
			navNew.ShowInSiteMap = true;

			navNew.EditDate = DateTime.Now.Date.AddHours(-8);
			navNew.CreateDate = DateTime.Now.Date.AddHours(-38);
			navNew.GoLiveDate = navNew.EditDate.AddHours(-5);
			navNew.RetireDate = navNew.CreateDate.AddYears(5);
			navNew.PageText = "<h2>Content CENTER</h2>\r\n";

			navNew.TemplateFile = SiteData.PreviewTemplateFile;

			if (SiteData.IsWebView) {
				navNew.FileName = SiteData.PreviewTemplateFilePage + "?" + HttpContext.Current.Request.QueryString.ToString();
			} else {
				navNew.FileName = SiteData.PreviewTemplateFilePage + "?sampler=true";
			}

			navNew.PageText = "<h2>Content CENTER</h2>\r\n" + sFile2;

			navNew.SiteID = SiteData.CurrentSiteID;
			navNew.Parent_ContentID = null;
			navNew.ContentType = ContentPageType.PageType.ContentEntry;

			navNew.EditUserId = SecurityData.CurrentUserGuid;

			return navNew;
		}
	}
}