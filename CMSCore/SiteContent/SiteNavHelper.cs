using Carrotware.CMS.Data;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

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

	public enum SearchContentPortion {
		All,
		Title,
		FileName,
		ContentBody
	}

	//=============
	public static class SiteNavFactory {

		public static ISiteNavHelper GetSiteNavHelper() {
			if ((SiteData.IsPageSampler || SiteData.IsPageReal) && !SiteData.IsCurrentPageSpecial) {
				return new SiteNavHelperMock();
			} else {
				return new SiteNavHelperReal();
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
			navData.PageText = "<p>NO PAGE CONTENT</p>" + SampleBody;
			navData.EditDate = DateTime.Now.Date.AddDays(-3);
			navData.CreateDate = DateTime.Now.Date.AddDays(-10);
			navData.GoLiveDate = DateTime.Now.Date.AddDays(2);
			navData.RetireDate = DateTime.Now.Date.AddDays(90);
			navData.ContentType = ContentPageType.PageType.ContentEntry;
			return navData;
		}

		public static SiteNav GetEmptySearch() {
			var link = new HtmlTag(HtmlTag.EasyTag.AnchorTag);
			link.Uri = SiteFilename.SiteInfoURL;
			link.InnerHtml = "Site Info";

			SiteNav navData = new SiteNav();
			navData.ContentID = Guid.Empty;
			navData.Root_ContentID = Guid.NewGuid();
			navData.SiteID = SiteData.CurrentSiteID;
			navData.TemplateFile = SiteData.DefaultTemplateFilename;
			navData.FileName = SiteData.CurrentSite.SiteSearchPath;
			navData.NavMenuText = "Search";
			navData.PageHead = "Search";
			navData.TitleBar = "Search";
			navData.PageActive = true;
			navData.PageText = SecurityData.IsAuthenticated == false ? "<p>Search Results</p>" :
							"<h2>This is a temporary search result page.  To assign an index page, please visit the"
							+ " " + link.RenderTag() + " page and select the page"
							+ " you want to be the search result page.</h2>";
			navData.EditDate = DateTime.Now.Date.AddDays(-30);
			navData.CreateDate = DateTime.Now.Date.AddDays(-30);
			navData.GoLiveDate = DateTime.Now.Date.AddDays(-30);
			navData.RetireDate = DateTime.Now.Date.AddDays(180);
			navData.ContentType = ContentPageType.PageType.ContentEntry;
			return navData;
		}

		internal static List<SiteNav> _content = null;
		internal static List<SiteNav> _pages = new List<SiteNav>();
		internal static List<SiteNav> _posts = new List<SiteNav>();
		internal static List<ContentCategory> _cats = new List<ContentCategory>();
		internal static List<ContentTag> _tags = new List<ContentTag>();
		internal static List<PostComment> _comments = new List<PostComment>();
		internal static ContentPage _home = new ContentPage();

		internal static object _lockReset = new object();

		public static void ResetFakeData() {
			lock (_lockReset) {
				_content = null;
				_pages = new List<SiteNav>();
				_posts = new List<SiteNav>();
				//BuildFakeData();
			}
		}

		internal static object _lockBuild = new object();

		internal static void BuildFakeData() {
			lock (_lockBuild) {
				var siteId = SiteData.CurrentSiteID;
				if (_pages == null || _content == null) {
					ResetCaption();
					_comments = new List<PostComment>();

					_pages = BuildFakeLevelDepthNavigation(siteId, 3, false);
					_posts = BuildSamplerFakeNav(25, null);

					_pages.ForEach(c => c.ContentType = ContentPageType.PageType.ContentEntry);
					_posts.ForEach(c => c.ContentType = ContentPageType.PageType.BlogEntry);

					_cats = BuildFakeCategoryList(siteId, 12);
					_tags = BuildFakeTagList(siteId, 12);

					_content = _pages.Union(_posts).ToList();
				}

				_pages = _content.OrderBy(x => x.NavOrder)
							.Where(x => x.ContentType == ContentPageType.PageType.ContentEntry)
							.Select(x => x.ShallowCopy()).ToList();
				_posts = _content.OrderByDescending(x => x.GoLiveDate)
							.Where(x => x.ContentType == ContentPageType.PageType.BlogEntry)
							.Select(x => x.ShallowCopy()).ToList();

				var homeNav = _pages.Where(x => x.NavOrder == 0).First();
				var oldId = homeNav.Root_ContentID;
				var newId = siteId;

				if (oldId != newId) {
					_pages.Where(x => x.Parent_ContentID == oldId).ToList().ForEach(c => c.Parent_ContentID = newId);
					homeNav.Root_ContentID = newId;

					_home = homeNav.GetContentPage();
					_home.TemplateFile = SiteData.PreviewTemplateFile;
				}
			}
		}

		internal static string SampleBody {
			get {
				return "<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus mi arcu, lacinia scelerisque blandit nec, mattis non nibh.</p> \r\n <p> Curabitur quis urna at massa placerat auctor. Quisque et mauris sapien, a consectetur nulla.</p>\r\n" +
							"<p>Etiam a quam lacus. Etiam urna sapien, porttitor at rhoncus sed, tristique sed sapien. Nam felis nulla, sodales a tincidunt ac, fermentum eu nisi.</p>\r\n";
			}
		}

		internal static string[] _captions = new string[] { "Felis eget velit", "Dui accumsan", "Sed tempus urna",
							"Consectetur", "Adipiscing", "Leo urna molestie", "Nulla facilisi", "Lorem sed risus",
							"Vitae nunc sed", "Convallis convallis", "Morbi leo urna", "Nunc consequat" };

		private static int _captionPos = -1;
		private static List<string> _captionsUsed = new List<string>();

		internal static void ResetCaption() {
			_captionsUsed = new List<string>();
			_captionPos = -1;
		}

		internal static string GetCaption(int idx) {
			if (idx >= _captions.Length || idx < 0) {
				idx = 0;
			}

			var caption = _captions[idx];

			_captionPos = idx + 1;

			return string.Format("{0} {1}", caption, idx);
		}

		internal static string GetRandomCaption() {
			var caption = _captions.OrderBy(x => Guid.NewGuid())
							.Where(x => !_captionsUsed.Contains(x))
							.FirstOrDefault();

			if (_captionsUsed.Count >= 7) {
				_captionsUsed.RemoveAt(0);
				_captionsUsed.RemoveAt(0);
				_captionsUsed.RemoveAt(0);
				_captionsUsed.RemoveAt(0);
			}

			_captionsUsed.Add(caption);

			return string.Format("{0}", caption);
		}

		internal static string GetNextCaption() {
			Interlocked.Increment(ref _captionPos);
			if (_captionPos >= _captions.Length || _captionPos < 0) {
				_captionPos = 0;
			}

			var caption = _captions[_captionPos];
			_captionsUsed.Add(caption);

			return string.Format("{0} {1}", caption, _captionPos);
		}

		internal static List<ContentCategory> GetFakeCategoryList(Guid siteID, int iUpdates) {
			return _cats.Take(iUpdates).ToList();
		}

		internal static List<ContentTag> GetFakeTagList(Guid siteID, int iUpdates) {
			return _tags.Take(iUpdates).ToList();
		}

		internal static List<SiteNav> GetFakeLevelDepthNavigation(Guid siteID, int iDepth, bool bActiveOnly) {
			List<SiteNav> lstContent = null;
			List<Guid> lstSub = new List<Guid>();

			if (iDepth < 1) {
				iDepth = 1;
			}

			if (iDepth > 10) {
				iDepth = 10;
			}

			List<Guid> lstTop = _pages.Where(x => x.Parent_ContentID == null).Select(x => x.Root_ContentID).ToList();

			while (iDepth > 1) {
				lstSub = _pages.Where(x => x.Parent_ContentID != null)
							.Where(x => lstTop.Contains(x.Parent_ContentID.Value))
							.Select(x => x.Root_ContentID).ToList();

				lstTop = lstTop.Union(lstSub).ToList();

				iDepth--;
			}

			return _pages.Where(x => lstTop.Contains(x.Root_ContentID)).OrderBy(x => x.NavOrder).ToList();
		}

		internal static List<ContentCategory> BuildFakeCategoryList(Guid siteID, int iUpdates) {
			List<int> pagelist = Enumerable.Range(1, iUpdates).ToList();

			List<ContentCategory> lstContent = (from ct in pagelist
												orderby ct descending
												select new ContentCategory {
													SiteID = Guid.NewGuid(),
													CategoryURL = string.Format("javascript:void(0);", ct),
													CategoryText = string.Format("Meta Info Cat {0}", ct),
													UseCount = ct + 2,
													PublicUseCount = ct + 3
												}).ToList();

			return lstContent;
		}

		internal static List<ContentTag> BuildFakeTagList(Guid siteID, int iUpdates) {
			List<int> pagelist = Enumerable.Range(1, iUpdates).ToList();

			List<ContentTag> lstContent = (from ct in pagelist
										   orderby ct descending
										   select new ContentTag {
											   SiteID = Guid.NewGuid(),
											   TagURL = string.Format("javascript:void(0);", ct),
											   TagText = string.Format("Meta Info Tag {0}", ct),
											   UseCount = ct + 2,
											   PublicUseCount = ct + 3
										   }).ToList();

			return lstContent;
		}

		internal static List<SiteNav> BuildFakeLevelDepthNavigation(Guid siteID, int iDepth, bool bActiveOnly) {
			List<SiteNav> lstNav = BuildSamplerFakeNav(4, null);
			var lstNav2 = new List<SiteNav>();

			if (iDepth >= 2) {
				foreach (SiteNav l1 in lstNav) {
					List<SiteNav> lst = BuildSamplerFakeNav(4, l1.Root_ContentID);
					lstNav2 = lstNav2.Union(lst).ToList();
					if (iDepth >= 3) {
						foreach (SiteNav l2 in lst) {
							List<SiteNav> lst2 = BuildSamplerFakeNav(3, l2.Root_ContentID);
							lstNav2 = lstNav2.Union(lst2).ToList();
							if (iDepth >= 4) {
								foreach (SiteNav l3 in lst2) {
									List<SiteNav> lst3 = BuildSamplerFakeNav(3, l2.Root_ContentID);
									lstNav2 = lstNav2.Union(lst3).ToList();
									if (iDepth >= 5) {
										foreach (SiteNav l4 in lst3) {
											List<SiteNav> lst4 = BuildSamplerFakeNav(3, l2.Root_ContentID);
											lstNav2 = lstNav2.Union(lst4).ToList();
										}
									}
								}
							}
						}
					}
				}
			}

			lstNav = lstNav.Union(lstNav2).ToList();
			return lstNav;
		}

		internal static List<SiteNav> BuildSamplerFakeNav(int iCount, Guid? rootParentID) {
			var navList = new List<SiteNav>();
			int n = 0;

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
					nav.Root_ContentID = Guid.NewGuid();
					nav.ContentID = nav.Root_ContentID;
					nav.FileName = "javascript:void(0);";
				}
				nav.Parent_ContentID = rootParentID;

				var caption = string.Empty;
				if (rootParentID.HasValue) {
					caption = GetRandomCaption();
					caption = string.Format("{0} {1}", caption, rootParentID.ToString().Substring(0, 3));
				} else {
					caption = GetCaption(n);
				}

				nav.TitleBar = string.Format("{0} T", caption);
				nav.NavMenuText = string.Format("{0} N", caption);
				nav.PageHead = string.Format("{0} H", caption);

				navList.Add(nav);
				n++;
			}

			return navList;
		}

		internal static List<SiteNav> GetSamplerFakeBlog(int iCount) {
			return _posts.OrderByDescending(x => x.GoLiveDate).Take(iCount).ToList();
		}

		internal static List<SiteNav> GetSamplerFakeNav() {
			return GetSamplerFakeNav(4, null);
		}

		internal static List<SiteNav> GetSamplerFakeNav(string page) {
			return _pages.Where(x => x.Parent_ContentID != null)
						.Take(4).OrderBy(x => x.NavOrder).ToList();
		}

		internal static List<SiteNav> GetSamplerFakeNav(int iCount) {
			return GetSamplerFakeNav(iCount, null);
		}

		internal static List<SiteNav> GetSamplerFakeNav(Guid? rootParentID) {
			return GetSamplerFakeNav(5, rootParentID);
		}

		internal static List<SiteNav> GetSamplerFakeNav(int iCount, Guid? rootParentID) {
			if (rootParentID.HasValue) {
				return _pages.Where(x => x.Parent_ContentID == rootParentID.Value)
						.Take(iCount).OrderBy(x => x.NavOrder).ToList();
			}

			return _pages.Where(x => x.Parent_ContentID == null)
					.Take(iCount).OrderBy(x => x.NavOrder).ToList();
		}

		internal static List<SiteNav> GetSamplerFakeSearch(int iCount) {
			return _pages.Where(x => x.Parent_ContentID == null)
					.Take(iCount).OrderByDescending(x => x.GoLiveDate).ToList();
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
			sbFile.Append(SampleBody);

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

		internal static SiteNav GetSamplerHome() {
			return _home.GetSiteNav();
		}

		internal static SiteNav GetSamplerView() {
			string sFile2 = GetSampleBody();
			var caption = GetRandomCaption();

			SiteNav navNew = new SiteNav();
			navNew.Root_ContentID = Guid.NewGuid();
			navNew.ContentID = navNew.Root_ContentID;
			navNew.FileName = "javascript:void(0);";

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
			navNew.PageText = "<h2>Content CENTER</h2>\r\n" + SampleBody;

			navNew.TemplateFile = SiteData.PreviewTemplateFile;
			navNew.FileName = SiteData.PreviewTemplateFilePage;

			navNew.PageText = "<h2>Content CENTER</h2>\r\n" + sFile2;

			navNew.SiteID = SiteData.CurrentSiteID;
			navNew.Parent_ContentID = null;
			navNew.ContentType = ContentPageType.PageType.ContentEntry;

			navNew.EditUserId = SecurityData.CurrentUserGuid;

			return navNew;
		}
	}
}