using Carrotware.CMS.Data;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
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
					SiteNavHelper.SeqGuid = null;
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
				SiteNavHelper.SeqGuid = null;
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
								  && ct.FileName.ToLowerInvariant().EndsWith(SiteData.DefaultDirectoryFilename)
							  select ct.FileName.ToLowerInvariant()).Distinct().ToList();
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
			navData.PageText = "<p>NO PAGE CONTENT</p>" + SiteNavHelperMock.SampleBody;
			navData.EditDate = DateTime.Now.Date.AddDays(-3);
			navData.CreateDate = DateTime.Now.Date.AddDays(-10);
			navData.GoLiveDate = DateTime.Now.Date.AddDays(2);
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
			SeqGuid = null;

			while (n < iCount) {
				SiteNav nav = GetSamplerView();
				nav.NavOrder = rootParentID.HasValue ? n * 100 : n;
				nav.NavMenuText = nav.NavMenuText;
				nav.CreateDate = nav.CreateDate.AddHours((0 - n) * 25);
				nav.EditDate = nav.CreateDate.AddHours((0 - n) * 16);
				nav.GoLiveDate = DateTime.Now.Date.AddDays((-2 * n) - 3);
				nav.RetireDate = DateTime.Now.Date.AddDays(45);
				nav.CommentCount = (n * 2) + 1;
				nav.ShowInSiteNav = true;
				nav.ShowInSiteMap = true;

				if (n > 0 || rootParentID != null) {
					nav.Root_ContentID = SeqGuid.NextGuid;
					nav.ContentID = Guid.NewGuid();
					//nav.FileName = nav.FileName.Replace(".aspx", nav.NavOrder.ToString() + ".aspx");
					nav.FileName = "/#";
					//if (rootParentID != null) {
					//	nav.NavMenuText = nav.NavMenuText + " - " + rootParentID.Value.ToString().Substring(0, 4);
					//}
				}
				nav.Parent_ContentID = rootParentID;

				var caption = string.Empty;
				if (rootParentID.HasValue) {
					caption = SiteNavHelperMock.GetRandomCaption();
					caption = string.Format("{0} {1}", caption, rootParentID.ToString().Substring(0, 3));
				} else {
					caption = SiteNavHelperMock.GetCaption(n);
				}

				nav.TitleBar = string.Format("{0} T", caption);
				nav.NavMenuText = string.Format("{0} N", caption);
				nav.PageHead = string.Format("{0} H", caption);

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
			return GetSampleBody(string.Empty);
		}

		public static string GetSampleBody(string sContentSampleNumber) { // SampleContent2
			if (string.IsNullOrWhiteSpace(sContentSampleNumber)) {
				sContentSampleNumber = "SampleContent2";
			}

			var sbFile = new StringBuilder();
			sbFile.Append(SiteNavHelperMock.SampleBody);

			try {
				var sFile = CoreHelper.ReadEmbededScript(string.Format("Carrotware.CMS.Core.SiteContent.Mock.{0}.txt", sContentSampleNumber));
				if (!string.IsNullOrWhiteSpace(sFile)) {
					sbFile.Clear();
					sbFile.Append(sFile);
				}
			} catch { }

			try {
				Assembly _assembly = Assembly.GetExecutingAssembly();

				List<string> imageNames = (from i in _assembly.GetManifestResourceNames()
										   where i.Contains("SiteContent.Mock.sample")
												&& i.EndsWith(".png")
										   select i).ToList();

				foreach (string img in imageNames) {
					var imgURL = CoreHelper.GetWebResourceUrl(img);
					sbFile.Replace(img, imgURL);
				}
			} catch { }

			return sbFile.ToString();
		}

		private static SequentialGuid _seq = new SequentialGuid();

		internal static SequentialGuid SeqGuid {
			get {
				if (_seq == null) {
					_seq = new SequentialGuid();
				}

				return _seq;
			}
			set {
				_seq = value;
			}
		}

		internal static SiteNav GetSamplerView() {
			string sFile2 = GetSampleBody();
			var caption = SiteNavHelperMock.GetRandomCaption();

			SiteNav navNew = new SiteNav();
			navNew.Root_ContentID = SeqGuid.NextGuid;
			navNew.ContentID = Guid.NewGuid();

			navNew.NavOrder = -1;
			navNew.TitleBar = string.Format("{0} T", caption);
			navNew.NavMenuText = string.Format("{0} N", caption);
			navNew.PageHead = string.Format("{0} H", caption);
			navNew.PageActive = true;
			navNew.ShowInSiteNav = true;
			navNew.ShowInSiteMap = true;

			navNew.EditDate = DateTime.Now.Date.AddHours(-8);
			navNew.CreateDate = DateTime.Now.Date.AddHours(-38);
			navNew.GoLiveDate = navNew.EditDate.AddHours(-5);
			navNew.RetireDate = navNew.CreateDate.AddYears(5);
			navNew.PageText = "<h2>Content CENTER</h2>\r\n" + SiteNavHelperMock.SampleBody;

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