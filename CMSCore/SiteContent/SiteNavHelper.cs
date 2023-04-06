using Carrotware.CMS.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
			SeqGuid = null;

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
					nav.Root_ContentID = SeqGuid.NextGuid;
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
			return GetSampleBody(string.Empty);
		}

		public static string GetSampleBody(string sContentSampleNumber) { // SampleContent2
			if (string.IsNullOrEmpty(sContentSampleNumber)) {
				sContentSampleNumber = "SampleContent2";
			}

			string sFile2 = " <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus mi arcu, lacinia scelerisque blandit nec, mattis non nibh.</p> \r\n <p> Curabitur quis urna at massa placerat auctor. Quisque et mauris sapien, a consectetur nulla.</p>";

			try {
				Assembly _assembly = Assembly.GetExecutingAssembly();

				sFile2 = SiteData.ReadEmbededScript(string.Format("Carrotware.CMS.Core.SiteContent.Mock.{0}.txt", sContentSampleNumber));

				List<string> imageNames = (from i in _assembly.GetManifestResourceNames()
										   where i.Contains("SiteContent.Mock.sample")
												&& i.EndsWith(".png")
										   select i).ToList();

				foreach (string img in imageNames) {
					var imgURL = CMSConfigHelper.GetWebResourceUrl(typeof(SiteNav), img);
					sFile2 = sFile2.Replace(img, imgURL);
				}
			} catch { }

			return sFile2;
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

			SiteNav navNew = new SiteNav();
			navNew.Root_ContentID = SeqGuid.NextGuid;
			navNew.ContentID = Guid.NewGuid();

			navNew.NavOrder = -1;
			navNew.TitleBar = "Template Preview - TITLE";
			navNew.NavMenuText = "Template PV - NAV";
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

	//=====================
	internal class SequentialGuid {
		private int _b0 = 1;
		private int _b1 = 1;
		private int _b2 = 1;
		private int _b3 = 1;
		private int _b4 = 1;
		private int _b5 = 1;

		internal SequentialGuid() { }

		internal Guid NextGuid {
			get {
				var tempGuid = Guid.Empty;
				var bytes = tempGuid.ToByteArray();

				bytes[0] = (byte)_b0;
				bytes[1] = (byte)_b1;
				bytes[2] = (byte)_b2;
				bytes[3] = (byte)_b3;
				bytes[4] = (byte)_b4;
				bytes[5] = (byte)_b5;

				_b0 = _b0 + 3;
				_b1 = _b1 + 5;
				_b2 = _b2 + 11;
				_b3 = _b3 + 9;
				_b4 = _b4 + 7;
				_b5 = _b5 + 13;

				return new Guid(bytes);
			}
		}
	}
}