using Carrotware.CMS.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml.Serialization;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin.Service {

	[WebService(Namespace = "http://carrotware.com/cms/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
	[System.Web.Script.Services.ScriptService]
	[CmsAuthorize]
	public class CMS : System.Web.Services.WebService {

		public CMS() {
			if (!this.User.Identity.IsAuthenticated) {
				throw new Exception("Not Authenticated!");
			}

			if (!(SecurityData.IsAdmin || SecurityData.IsSiteEditor)) {
				throw new Exception("Not Authorizeed!");
			}
		}

		private ContentPageHelper pageHelper = new ContentPageHelper();
		private WidgetHelper widgetHelper = new WidgetHelper();
		private SiteMapOrderHelper sitemapHelper = new SiteMapOrderHelper();

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			if (pageHelper != null) {
				pageHelper.Dispose();
			}
			if (widgetHelper != null) {
				widgetHelper.Dispose();
			}
			if (sitemapHelper != null) {
				sitemapHelper.Dispose();
			}
		}

		private Guid CurrentPageGuid = Guid.Empty;
		private ContentPage filePage = null;

		private List<ContentPage> _pages = null;

		protected List<ContentPage> lstActivePages {
			get {
				if (_pages == null) {
					_pages = pageHelper.GetLatestContentList(SiteData.CurrentSite.SiteID, true);
				}
				return _pages;
			}
		}

		public ContentPage cmsAdminContent {
			get {
				ContentPage c = null;
				try {
					string sXML = GetSerialized(CMSConfigHelper.keyAdminContent);
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(ContentPage));
					Object genpref = null;
					using (StringReader stringReader = new StringReader(sXML)) {
						genpref = xmlSerializer.Deserialize(stringReader);
					}
					c = genpref as ContentPage;
				} catch { }
				return c;
			}
			set {
				if (value == null) {
					ClearSerialized(CMSConfigHelper.keyAdminContent);
				} else {
					string sXML = String.Empty;
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(ContentPage));
					using (StringWriter stringWriter = new StringWriter()) {
						xmlSerializer.Serialize(stringWriter, value);
						sXML = stringWriter.ToString();
					}
					SaveSerialized(CMSConfigHelper.keyAdminContent, sXML);
				}
			}
		}

		public List<Widget> cmsAdminWidget {
			get {
				List<Widget> c = null;
				string sXML = GetSerialized(CMSConfigHelper.keyAdminWidget);
				//since a page may not have any widgets, initalize it and skip deserializing
				if (!String.IsNullOrEmpty(sXML)) {
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Widget>));
					Object genpref = null;
					using (StringReader stringReader = new StringReader(sXML)) {
						genpref = xmlSerializer.Deserialize(stringReader);
					}
					c = genpref as List<Widget>;
				} else {
					c = new List<Widget>();
				}
				return c;
			}
			set {
				if (value == null) {
					ClearSerialized(CMSConfigHelper.keyAdminWidget);
				} else {
					string sXML = String.Empty;
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Widget>));
					using (StringWriter stringWriter = new StringWriter()) {
						xmlSerializer.Serialize(stringWriter, value);
						sXML = stringWriter.ToString();
					}
					SaveSerialized(CMSConfigHelper.keyAdminWidget, sXML);
				}
			}
		}

		private void SaveSerialized(string sKey, string sData) {
			LoadGuids();

			CMSConfigHelper.SaveSerialized(CurrentPageGuid, sKey, sData);
		}

		private string GetSerialized(string sKey) {
			string sData = String.Empty;
			LoadGuids();

			sData = CMSConfigHelper.GetSerialized(CurrentPageGuid, sKey);

			return sData;
		}

		private bool ClearSerialized(string sKey) {
			LoadGuids();

			return CMSConfigHelper.ClearSerialized(CurrentPageGuid, sKey);
		}

		private void LoadGuids() {
			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				if (!String.IsNullOrEmpty(CurrentEditPage)) {
					filePage = pageHelper.FindByFilename(SiteData.CurrentSite.SiteID, CurrentEditPage);
					if (filePage != null) {
						CurrentPageGuid = filePage.Root_ContentID;
					}
				} else {
					if (CurrentPageGuid != Guid.Empty) {
						filePage = pageHelper.FindContentByID(SiteData.CurrentSite.SiteID, CurrentPageGuid);
						if (filePage != null) {
							CurrentEditPage = filePage.FileName;
						}
					} else {
						filePage = new ContentPage();
					}
				}
			}
		}

		private string CurrentEditPage = String.Empty;

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string RecordHeartbeat(string PageID) {
			try {
				CurrentPageGuid = new Guid(PageID);

				bool bRet = pageHelper.RecordPageLock(CurrentPageGuid, SiteData.CurrentSite.SiteID, SecurityData.CurrentUserGuid);

				if (bRet) {
					return SiteData.CurrentSite.Now.ToString();
				} else {
					return Convert.ToDateTime("12/31/1899").ToString();
				}
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return DateTime.MinValue.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string CancelEditing(string ThisPage) {
			try {
				CurrentPageGuid = new Guid(ThisPage);

				pageHelper.ResetHeartbeatLock(CurrentPageGuid, SiteData.CurrentSite.SiteID, SecurityData.CurrentUserGuid);

				GetSetUserEditStateAsEmpty();

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string SendTrackbackPageBatch(string ThisPage) {
			try {
				CurrentPageGuid = new Guid(ThisPage);
				if (CurrentPageGuid != Guid.Empty) {
					ContentPage cp = pageHelper.FindContentByID(SiteData.CurrentSite.SiteID, CurrentPageGuid);
					cp.SaveTrackbackTop();
				}
				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string SendTrackbackBatch() {
			try {
				if (SiteData.CurretSiteExists) {
					SiteData.CurrentSite.SendTrackbackQueue();
				}
				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string RecordEditorPosition(string ToolbarState, string ToolbarMargin, string ToolbarScroll, string WidgetScroll, string SelTabID) {
			try {
				GetSetUserEditState(ToolbarState, ToolbarMargin, ToolbarScroll, WidgetScroll, SelTabID);

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		private void GetSetUserEditStateAsEmpty() {
			GetSetUserEditState(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty);
		}

		private void GetSetUserEditState(string ToolbarState, string ToolbarMargin, string ToolbarScroll, string WidgetScroll, string SelTabID) {
			UserEditState editor = UserEditState.cmsUserEditState;

			if (editor == null) {
				editor = new UserEditState();
				editor.Init();
			}

			editor.EditorMargin = String.IsNullOrEmpty(ToolbarMargin) ? "L" : ToolbarMargin.ToUpperInvariant();
			editor.EditorOpen = String.IsNullOrEmpty(ToolbarState) ? "true" : ToolbarState.ToLowerInvariant();
			editor.EditorWidgetScrollPosition = String.IsNullOrEmpty(WidgetScroll) ? "0" : WidgetScroll.ToLowerInvariant();
			editor.EditorScrollPosition = String.IsNullOrEmpty(ToolbarScroll) ? "0" : ToolbarScroll.ToLowerInvariant();
			editor.EditorSelectedTabIdx = String.IsNullOrEmpty(SelTabID) ? "0" : SelTabID.ToLowerInvariant();

			if (String.IsNullOrEmpty(ToolbarMargin) && String.IsNullOrEmpty(ToolbarState)) {
				UserEditState.cmsUserEditState = null;
			} else {
				UserEditState.cmsUserEditState = editor;
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public List<SiteMapOrder> GetChildPages(string PageID, string CurrPageID) {
			Guid? ParentID = Guid.Empty;
			if (!String.IsNullOrEmpty(PageID)) {
				if (PageID.Length > 20) {
					ParentID = new Guid(PageID);
				}
			}

			Guid ContPageID = Guid.Empty;
			if (!String.IsNullOrEmpty(CurrPageID)) {
				if (CurrPageID.Length > 20) {
					ContPageID = new Guid(CurrPageID);
				}
			}

			List<SiteMapOrder> lstSiteMap = new List<SiteMapOrder>();

			try {
				if (SiteData.CurretSiteExists) {
					List<SiteMapOrder> lst = sitemapHelper.GetChildPages(SiteData.CurrentSite.SiteID, ParentID, ContPageID);

					lstSiteMap = (from l in lst
								  orderby l.NavOrder, l.NavMenuText
								  where l.Parent_ContentID != ContPageID || l.Parent_ContentID == null
								  select l).ToList();
				}
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				throw;
			}

			return lstSiteMap;
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public List<SiteMapOrder> GetPageCrumbs(string PageID, string CurrPageID) {
			Guid? ContentPageID = Guid.Empty;
			if (!String.IsNullOrEmpty(PageID)) {
				if (PageID.Length > 20) {
					ContentPageID = new Guid(PageID);
				}
			}

			Guid ContPageID = Guid.Empty;
			if (!String.IsNullOrEmpty(CurrPageID)) {
				if (CurrPageID.Length > 20) {
					ContPageID = new Guid(CurrPageID);
				}
			}

			List<SiteMapOrder> lstSiteMap = new List<SiteMapOrder>();

			int iLevel = 0;

			int iLenB = 0;
			int iLenA = 1;

			try {
				while (iLenB < iLenA && SiteData.CurretSiteExists) {
					iLenB = lstSiteMap.Count;

					SiteMapOrder cont = sitemapHelper.GetPageWithLevel(SiteData.CurrentSite.SiteID, ContentPageID, iLevel);

					iLevel++;
					if (cont != null) {
						ContentPageID = cont.Parent_ContentID;
						lstSiteMap.Add(cont);
					}

					iLenA = lstSiteMap.Count;
				}
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				throw;
			}

			return lstSiteMap.OrderByDescending(y => y.NavLevel).ToList();
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string UpdatePageTemplate(string TheTemplate, string ThisPage) {
			try {
				TheTemplate = CMSConfigHelper.DecodeBase64(TheTemplate);
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();

				ContentPage c = cmsAdminContent;

				c.TemplateFile = TheTemplate;

				cmsAdminContent = c;

				GetSetUserEditStateAsEmpty();

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string ValidateBlogFolders(string FolderPath, string DatePath, string CategoryPath, string TagPath, string EditorPath) {
			try {
				string sFolderPath = ContentPageHelper.ScrubSlug(CMSConfigHelper.DecodeBase64(FolderPath));
				string sCategoryPath = ContentPageHelper.ScrubSlug(CMSConfigHelper.DecodeBase64(CategoryPath));
				string sTagPath = ContentPageHelper.ScrubSlug(CMSConfigHelper.DecodeBase64(TagPath));
				string sDatePath = ContentPageHelper.ScrubSlug(CMSConfigHelper.DecodeBase64(DatePath));
				string sEditorPath = ContentPageHelper.ScrubSlug(CMSConfigHelper.DecodeBase64(EditorPath));

				if (String.IsNullOrEmpty(sFolderPath) || String.IsNullOrEmpty(sCategoryPath)
					|| String.IsNullOrEmpty(sTagPath) || String.IsNullOrEmpty(sDatePath)
					|| String.IsNullOrEmpty(sEditorPath)) {
					return "FAIL";
				}
				if (sFolderPath.Length < 1 || sCategoryPath.Length < 1 || sTagPath.Length < 1 || sDatePath.Length < 1 || sEditorPath.Length < 1) {
					return "FAIL";
				}

				List<string> lstParms = new List<string>();
				lstParms.Add(sCategoryPath);
				lstParms.Add(sTagPath);
				lstParms.Add(sDatePath);
				lstParms.Add(sEditorPath);

				Dictionary<string, int> ct = (from p in lstParms
											  group p by p into g
											  select new KeyValuePair<string, int>(g.Key, g.Count())).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

				bool bDuplicate = ct.Where(x => x.Value > 1).Any();

				if (SiteData.CurretSiteExists && !bDuplicate) {
					var exists = pageHelper.ExistingPagesBeginWith(SiteData.CurrentSite.SiteID, sFolderPath);

					if (!exists) {
						return "OK";
					}
				}

				if (!SiteData.CurretSiteExists) {
					return "OK";
				}

				return "FAIL";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public List<UserProfile> FindUsers(string searchTerm) {
			string search = CMSConfigHelper.DecodeBase64(searchTerm);

			List<UserProfile> lstUsers = SecurityData.GetUserProfileSearch(search);

			return lstUsers.OrderBy(x => x.UserName).ToList();
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public List<UserProfile> FindCreditUsers(string searchTerm) {
			string search = CMSConfigHelper.DecodeBase64(searchTerm);

			List<UserProfile> lstUsers = SecurityData.GetCreditUserProfileSearch(search);

			return lstUsers.OrderBy(x => x.UserName).ToList();
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string ValidateUniqueCategory(string TheSlug, string ItemID) {
			try {
				Guid CurrentItemGuid = new Guid(ItemID);
				TheSlug = CMSConfigHelper.DecodeBase64(TheSlug);
				TheSlug = ContentPageHelper.ScrubSlug(TheSlug);

				int iCount = ContentCategory.GetSimilar(SiteData.CurrentSite.SiteID, CurrentItemGuid, TheSlug);

				if (iCount < 1) {
					return "OK";
				}

				return "FAIL";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string ValidateUniqueTag(string TheSlug, string ItemID) {
			try {
				Guid CurrentItemGuid = new Guid(ItemID);
				TheSlug = CMSConfigHelper.DecodeBase64(TheSlug);
				TheSlug = ContentPageHelper.ScrubSlug(TheSlug);

				int iCount = ContentTag.GetSimilar(SiteData.CurrentSite.SiteID, CurrentItemGuid, TheSlug);

				if (iCount < 1) {
					return "OK";
				}

				return "FAIL";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string RecordSnippetHeartbeat(string ItemID) {
			try {
				Guid CurrentItemGuid = new Guid(ItemID);

				ContentSnippet item = GetSnippet(CurrentItemGuid);
				bool bRet = false;

				if (item != null && !item.IsLocked) {
					bRet = item.RecordSnippetLock(SecurityData.CurrentUserGuid);
				}

				if (bRet) {
					return SiteData.CurrentSite.Now.ToString();
				} else {
					return Convert.ToDateTime("12/31/1899").ToString();
				}
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return DateTime.MinValue.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string CancelSnippetEditing(string ItemID) {
			try {
				Guid CurrentItemGuid = new Guid(ItemID);
				ContentSnippet item = GetSnippet(CurrentItemGuid);

				if (item != null && !item.IsLocked) {
					item.ResetHeartbeatLock();
				}

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		private ContentSnippet GetSnippet(Guid snippetID) {
			ContentSnippet item = ContentSnippet.Get(snippetID);

			if (item == null) {
				item = ContentSnippet.GetVersion(snippetID);
			}

			return item;
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string ValidateUniqueSnippet(string TheSlug, string ItemID) {
			try {
				Guid CurrentItemGuid = new Guid(ItemID);
				TheSlug = CMSConfigHelper.DecodeBase64(TheSlug);
				TheSlug = ContentPageHelper.ScrubSlug(TheSlug);

				ContentSnippet item = GetSnippet(CurrentItemGuid);

				if (item != null) {
					CurrentItemGuid = item.Root_ContentSnippetID;
				}

				int iCount = ContentSnippet.GetSimilar(SiteData.CurrentSite.SiteID, CurrentItemGuid, TheSlug);

				if (iCount < 1) {
					return "OK";
				}

				return "FAIL";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string GetSnippetVersionText(string DBKey) {
			try {
				Guid guidSnippet = new Guid(DBKey);

				ContentSnippet cs = ContentSnippet.GetVersion(guidSnippet);

				if (cs != null) {
					if (String.IsNullOrEmpty(cs.ContentBody)) {
						return "No Data";
					} else {
						if (cs.ContentBody.Length < 768) {
							return cs.ContentBody;
						} else {
							return cs.ContentBody.Substring(0, 700) + "[.....]";
						}
					}
				}

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return "FAIL";
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string GenerateNewFilename(string ThePageTitle, string GoLiveDate, string PageID, string Mode) {
			try {
				CurrentPageGuid = new Guid(PageID);

				DateTime goLiveDate = Convert.ToDateTime(GoLiveDate);
				string sThePageTitle = CMSConfigHelper.DecodeBase64(ThePageTitle);
				if (String.IsNullOrEmpty(sThePageTitle)) {
					sThePageTitle = CurrentPageGuid.ToString();
				}
				sThePageTitle = sThePageTitle.Replace("/", "-");
				string sTheFileName = ContentPageHelper.ScrubFilename(CurrentPageGuid, sThePageTitle);

				if (Mode.ToLowerInvariant() == "page") {
					string sTestRes = ValidateUniqueFilename(CMSConfigHelper.EncodeBase64(sTheFileName), PageID);
					if (sTestRes != "OK") {
						for (int i = 1; i < 1000; i++) {
							string sTestFile = sThePageTitle + "-" + i.ToString();
							sTestRes = ValidateUniqueFilename(CMSConfigHelper.EncodeBase64(sTestFile), PageID);
							if (sTestRes == "OK") {
								sTheFileName = ContentPageHelper.ScrubFilename(CurrentPageGuid, sTestFile);
								break;
							} else {
								sTheFileName = String.Empty;
							}
						}
					}
				} else {
					string sTestRes = ValidateUniqueBlogFilename(CMSConfigHelper.EncodeBase64(sTheFileName), GoLiveDate, PageID);
					if (sTestRes != "OK") {
						for (int i = 1; i < 1000; i++) {
							string sTestFile = sThePageTitle + "-" + i.ToString();
							sTestRes = ValidateUniqueBlogFilename(CMSConfigHelper.EncodeBase64(sTestFile), GoLiveDate, PageID);
							if (sTestRes == "OK") {
								sTheFileName = ContentPageHelper.ScrubFilename(CurrentPageGuid, sTestFile);
								break;
							} else {
								sTheFileName = String.Empty;
							}
						}
					}
				}

				return ContentPageHelper.ScrubFilename(CurrentPageGuid, sTheFileName).ToLowerInvariant();
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);
				return "FAIL";
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string ValidateUniqueFilename(string TheFileName, string PageID) {
			try {
				CurrentPageGuid = new Guid(PageID);
				TheFileName = CMSConfigHelper.DecodeBase64(TheFileName);

				TheFileName = ContentPageHelper.ScrubFilename(CurrentPageGuid, TheFileName);

				TheFileName = TheFileName.ToLowerInvariant();

				if (SiteData.IsPageSpecial(TheFileName) || SiteData.IsLikelyHomePage(TheFileName)) {
					return "FAIL";
				}

				if (SiteData.CurrentSite.GetSpecialFilePathPrefixes().Where(x => TheFileName.StartsWith(x.ToLowerInvariant())).Any()
							|| TheFileName.StartsWith(SiteData.CurrentSite.BlogFolderPath.ToLowerInvariant())) {
					return "FAIL";
				}

				ContentPage fn = pageHelper.FindByFilename(SiteData.CurrentSite.SiteID, TheFileName);

				ContentPage cp = pageHelper.FindContentByID(SiteData.CurrentSite.SiteID, CurrentPageGuid);

				if (cp == null && CurrentPageGuid != Guid.Empty) {
					cp = pageHelper.GetVersion(SiteData.CurrentSite.SiteID, CurrentPageGuid);
				}

				if (fn == null || (fn != null && cp != null && fn.Root_ContentID == cp.Root_ContentID)) {
					return "OK";
				} else {
					return "FAIL";
				}
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string GenerateBlogFilePrefix(string ThePageSlug, string GoLiveDate) {
			try {
				DateTime goLiveDate = Convert.ToDateTime(GoLiveDate);
				ThePageSlug = CMSConfigHelper.DecodeBase64(ThePageSlug);

				return ContentPageHelper.CreateFileNameFromSlug(SiteData.CurrentSite.SiteID, goLiveDate, ThePageSlug);
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return "FAIL";
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string ValidateUniqueBlogFilename(string ThePageSlug, string GoLiveDate, string PageID) {
			try {
				CurrentPageGuid = new Guid(PageID);
				DateTime dateGoLive = Convert.ToDateTime(GoLiveDate);
				DateTime dateOrigGoLive = DateTime.MinValue;

				ThePageSlug = CMSConfigHelper.DecodeBase64(ThePageSlug);

				ThePageSlug = ContentPageHelper.ScrubFilename(CurrentPageGuid, ThePageSlug);
				ThePageSlug = ThePageSlug.ToLowerInvariant();

				string TheFileName = ThePageSlug;

				ContentPage cp = pageHelper.FindContentByID(SiteData.CurrentSite.SiteID, CurrentPageGuid);

				if (cp != null) {
					dateOrigGoLive = cp.GoLiveDate;
				}
				if (cp == null && CurrentPageGuid != Guid.Empty) {
					ContentPageExport cpe = ContentImportExportUtils.GetSerializedContentPageExport(CurrentPageGuid);
					if (cpe != null) {
						dateOrigGoLive = cpe.ThePage.GoLiveDate;
					}
				}

				TheFileName = ContentPageHelper.CreateFileNameFromSlug(SiteData.CurrentSite.SiteID, dateGoLive, ThePageSlug);

				if (SiteData.IsPageSpecial(TheFileName) || SiteData.IsLikelyHomePage(TheFileName)) {
					return "FAIL";
				}

				ContentPage fn1 = pageHelper.FindByFilename(SiteData.CurrentSite.SiteID, TheFileName);

				if (cp == null && CurrentPageGuid != Guid.Empty) {
					cp = pageHelper.GetVersion(SiteData.CurrentSite.SiteID, CurrentPageGuid);
				}

				if (fn1 == null || (fn1 != null && cp != null && fn1.Root_ContentID == cp.Root_ContentID)) {
					return "OK";
				} else {
					return "FAIL";
				}
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string GenerateCategoryTagSlug(string TheSlug, string Mode) {
			try {
				TheSlug = CMSConfigHelper.DecodeBase64(TheSlug).ToLowerInvariant().Trim();

				return ContentPageHelper.ScrubSlug(TheSlug);
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return "FAIL";
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string GenerateSnippetSlug(string TheSlug) {
			try {
				TheSlug = CMSConfigHelper.DecodeBase64(TheSlug).ToLowerInvariant().Trim();

				return ContentPageHelper.ScrubSlug(TheSlug);
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return "FAIL";
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string MoveWidgetToNewZone(string WidgetTarget, string WidgetDropped, string ThisPage) {
			try {
				//WidgetAddition = CMSConfigHelper.DecodeBase64(WidgetAddition);
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				string[] w = WidgetDropped.Split('\t');

				Guid guidWidget = Guid.Empty;
				if (w.Length > 2) {
					if (w[2].ToString().Length == Guid.Empty.ToString().Length) {
						guidWidget = new Guid(w[2]);
					}
				} else {
					if (w[0].ToString().Length == Guid.Empty.ToString().Length) {
						guidWidget = new Guid(w[0]);
					}
				}

				List<Widget> cacheWidget = cmsAdminWidget;

				Widget ww1 = (from w1 in cacheWidget
							  where w1.Root_WidgetID == guidWidget
							  select w1).FirstOrDefault();

				if (ww1 != null) {
					ww1.WidgetOrder = -1;
					ww1.PlaceholderName = WidgetTarget;
				}

				List<Widget> ww2 = (from w1 in cacheWidget
									where w1.PlaceholderName.ToLowerInvariant() == WidgetTarget.ToLowerInvariant()
									&& w1.WidgetOrder >= 0
									orderby w1.WidgetOrder, w1.EditDate
									select w1).ToList();

				int iW = 1;
				foreach (var w2 in ww2) {
					w2.WidgetOrder = iW++;
				}

				cmsAdminWidget = cacheWidget;
				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string CacheWidgetUpdate(string WidgetAddition, string ThisPage) {
			try {
				WidgetAddition = CMSConfigHelper.DecodeBase64(WidgetAddition);
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();

				List<Widget> cacheWidget = cmsAdminWidget;

				List<Widget> inputWid = new List<Widget>();
				Dictionary<Guid, int> dictOrder = new Dictionary<Guid, int>();
				int iW = 0;

				WidgetAddition = WidgetAddition.Replace("\r\n", "\n");
				WidgetAddition = WidgetAddition.Replace("\r", "\n");
				string[] arrWidgRows = WidgetAddition.Split('\n');

				foreach (string arrWidgCell in arrWidgRows) {
					if (!String.IsNullOrEmpty(arrWidgCell)) {
						bool bGoodWidget = false;
						string[] w = arrWidgCell.Split('\t');

						Widget rWidg = new Widget();
						if (w[2].ToLowerInvariant().EndsWith(".cshtml") || w[2].ToLowerInvariant().EndsWith(".vbhtml")
								|| w[2].ToLowerInvariant().Contains(":") || w[2].ToLowerInvariant().Contains("|")) {
							rWidg.ControlPath = w[2];
							rWidg.Root_WidgetID = Guid.NewGuid();

							DateTime dtSite = CMSConfigHelper.CalcNearestFiveMinTime(SiteData.CurrentSite.Now);
							rWidg.GoLiveDate = dtSite;
							rWidg.RetireDate = dtSite.AddYears(200);

							bGoodWidget = true;
						} else {
							if (w[2].ToString().Length == Guid.Empty.ToString().Length) {
								rWidg.Root_WidgetID = new Guid(w[2]);
								bGoodWidget = true;
							}
						}
						if (bGoodWidget) {
							dictOrder.Add(rWidg.Root_WidgetID, iW);

							rWidg.WidgetDataID = Guid.NewGuid();
							rWidg.IsPendingChange = true;
							rWidg.PlaceholderName = w[1].Substring(4);
							rWidg.WidgetOrder = int.Parse(w[0]);
							rWidg.Root_ContentID = CurrentPageGuid;
							rWidg.IsWidgetActive = true;
							rWidg.IsLatestVersion = true;
							rWidg.EditDate = SiteData.CurrentSite.Now;
							inputWid.Add(rWidg);
						}
						iW++;
					}
				}

				foreach (Widget wd1 in inputWid) {
					Widget wd2 = (from d in cacheWidget where d.Root_WidgetID == wd1.Root_WidgetID select d).FirstOrDefault();

					if (wd2 == null) {
						cacheWidget.Add(wd1);
					} else {
						wd2.EditDate = SiteData.CurrentSite.Now;
						wd2.PlaceholderName = wd1.PlaceholderName; // if moving zones

						int i = cacheWidget.IndexOf(wd2);
						cacheWidget[i].WidgetOrder = wd1.WidgetOrder;

						int? mainSort = (from entry in dictOrder
										 where entry.Key == wd1.Root_WidgetID
										 select entry.Value).FirstOrDefault();

						if (mainSort != null) {
							cacheWidget[i].WidgetOrder = Convert.ToInt32(mainSort);
						}
					}
				}

				cmsAdminWidget = cacheWidget;
				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string GetWidgetText(string DBKey, string ThisPage) {
			try {
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				Guid guidWidget = new Guid(DBKey);

				Widget ww = null;

				try {
					ww = (from w in cmsAdminWidget
						  where w.Root_WidgetID == guidWidget
						  select w).FirstOrDefault();
				} catch (Exception ex) { }

				if (ww == null) {
					ww = widgetHelper.Get(guidWidget);
				}

				if (ww != null) {
					if (String.IsNullOrEmpty(ww.ControlProperties)) {
						return "No Data";
					} else {
						if (ww.ControlProperties.Length < 768) {
							return ww.ControlProperties;
						} else {
							return ww.ControlProperties.Substring(0, 700) + "[.....]";
						}
					}
				}

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return "FAIL";
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string GetWidgetVersionText(string DBKey, string ThisPage) {
			try {
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				Guid guidWidget = new Guid(DBKey);

				Widget ww = null;

				try {
					ww = (from w in cmsAdminWidget
						  where w.WidgetDataID == guidWidget
						  select w).FirstOrDefault();
				} catch (Exception ex) { }

				if (ww == null) {
					ww = widgetHelper.GetWidgetVersion(guidWidget);
				}

				if (ww != null) {
					if (String.IsNullOrEmpty(ww.ControlProperties)) {
						return "No Data";
					} else {
						if (ww.ControlProperties.Length < 768) {
							return ww.ControlProperties;
						} else {
							return ww.ControlProperties.Substring(0, 700) + "[.....]";
						}
					}
				}

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return "FAIL";
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string GetWidgetLatestText(string DBKey, string ThisPage) {
			try {
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				Guid guidWidget = new Guid(DBKey);
				Widget ww = null;

				try {
					ww = (from w in cmsAdminWidget
						  where w.Root_WidgetID == guidWidget
						  select w).FirstOrDefault();
				} catch (Exception ex) { }

				if (ww == null) {
					ww = widgetHelper.Get(guidWidget);
				}

				if (ww != null) {
					if (String.IsNullOrEmpty(ww.ControlProperties)) {
						return "No Data";
					} else {
						if (ww.ControlProperties.Length < 768) {
							return ww.ControlProperties;
						} else {
							return ww.ControlProperties.Substring(0, 700) + "[.....]";
						}
					}
				}

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return "FAIL";
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string DeleteWidget(string DBKey, string ThisPage) {
			try {
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				Guid guidWidget = new Guid(DBKey);

				var cacheWidget = cmsAdminWidget;

				List<Widget> ww = (from w in cacheWidget
								   where w.Root_WidgetID == guidWidget
								   select w).ToList();

				if (ww != null) {
					foreach (var w in ww) {
						w.IsWidgetPendingDelete = true;
						w.IsWidgetActive = false;
						w.EditDate = SiteData.CurrentSite.Now;
					}
				}

				cmsAdminWidget = cacheWidget;

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string CopyWidget(string DBKey, string ThisPage) {
			try {
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();

				Guid guidWidget = new Guid(DBKey);

				List<Widget> cacheWidget = cmsAdminWidget;

				List<Widget> ww = (from w in cacheWidget
								   where w.Root_WidgetID == guidWidget
									&& w.IsLatestVersion == true
								   select w).ToList();

				if (ww != null) {
					foreach (var w in ww) {
						Guid newWidget = Guid.NewGuid();

						Widget wCpy = new Widget {
							Root_ContentID = w.Root_ContentID,
							Root_WidgetID = newWidget,
							WidgetDataID = Guid.NewGuid(),
							PlaceholderName = w.PlaceholderName,
							ControlPath = w.ControlPath,
							ControlProperties = w.ControlProperties,
							IsLatestVersion = true,
							IsPendingChange = true,
							IsWidgetActive = true,
							IsWidgetPendingDelete = false,
							WidgetOrder = w.WidgetOrder,
							GoLiveDate = w.GoLiveDate,
							RetireDate = w.RetireDate,
							EditDate = SiteData.CurrentSite.Now
						};

						cacheWidget.Add(wCpy);
					}
				}

				cmsAdminWidget = cacheWidget;

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string RemoveWidget(string DBKey, string ThisPage) {
			try {
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				Guid guidWidget = new Guid(DBKey);

				List<Widget> cacheWidget = cmsAdminWidget;

				List<Widget> ww = (from w in cacheWidget
								   where w.Root_WidgetID == guidWidget
								   select w).ToList();

				if (ww != null) {
					foreach (var w in ww) {
						w.IsWidgetActive = false;
						w.EditDate = SiteData.CurrentSite.Now;
					}
				}

				cmsAdminWidget = cacheWidget;

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string ActivateWidget(string DBKey, string ThisPage) {
			try {
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				Guid guidWidget = new Guid(DBKey);

				List<Widget> cacheWidget = cmsAdminWidget;

				List<Widget> ww = (from w in cacheWidget
								   where w.Root_WidgetID == guidWidget
								   select w).ToList();

				if (ww != null) {
					foreach (var w in ww) {
						w.IsWidgetActive = true;
						w.EditDate = SiteData.CurrentSite.Now;
					}
				}

				cmsAdminWidget = cacheWidget;

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string CacheGenericContent(string ZoneText, string DBKey, string ThisPage) {
			try {
				ZoneText = CMSConfigHelper.DecodeBase64(ZoneText);
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				Guid guidWidget = new Guid(DBKey);

				List<Widget> cacheWidget = cmsAdminWidget;

				Widget c = (from w in cacheWidget
							where w.Root_WidgetID == guidWidget
							select w).FirstOrDefault();

				c.ControlProperties = ZoneText;
				c.EditDate = SiteData.CurrentSite.Now;

				cmsAdminWidget = cacheWidget;

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string CacheContentZoneText(string ZoneText, string Zone, string ThisPage) {
			try {
				ZoneText = CMSConfigHelper.DecodeBase64(ZoneText);
				Zone = CMSConfigHelper.DecodeBase64(Zone);
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				CurrentEditPage = filePage.FileName.ToLowerInvariant();

				var c = cmsAdminContent;
				c.EditDate = SiteData.CurrentSite.Now;
				c.EditUserId = SecurityData.CurrentUserGuid;
				c.ContentID = Guid.NewGuid();

				if (Zone.ToLowerInvariant() == "c")
					c.PageText = ZoneText;

				if (Zone.ToLowerInvariant() == "l")
					c.LeftPageText = ZoneText;

				if (Zone.ToLowerInvariant() == "r")
					c.RightPageText = ZoneText;

				cmsAdminContent = c;

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string PublishChanges(string ThisPage) {
			try {
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				CurrentEditPage = filePage.FileName.ToLowerInvariant();

				bool bLock = pageHelper.IsPageLocked(CurrentPageGuid, SiteData.CurrentSite.SiteID, SecurityData.CurrentUserGuid);
				Guid guidUser = pageHelper.GetCurrentEditUser(CurrentPageGuid, SiteData.CurrentSite.SiteID);

				if (bLock || guidUser != SecurityData.CurrentUserGuid) {
					return "Cannot publish changes, not current editing user.";
				}

				List<Widget> pageWidgets = widgetHelper.GetWidgets(CurrentPageGuid, true);

				if (cmsAdminContent != null) {
					ContentPage oldContent = pageHelper.FindContentByID(SiteData.CurrentSiteID, CurrentPageGuid);

					ContentPage newContent = cmsAdminContent;
					newContent.ContentID = Guid.NewGuid();
					newContent.NavOrder = oldContent.NavOrder;
					newContent.Parent_ContentID = oldContent.Parent_ContentID;
					newContent.EditUserId = SecurityData.CurrentUserGuid;
					newContent.EditDate = SiteData.CurrentSite.Now;

					foreach (var wd in cmsAdminWidget) {
						wd.EditDate = SiteData.CurrentSite.Now;
						wd.Save();
					}

					newContent.SavePageEdit();

					if (newContent.ContentType == ContentPageType.PageType.BlogEntry) {
						pageHelper.ResolveDuplicateBlogURLs(newContent.SiteID);
					}

					cmsAdminWidget = new List<Widget>();
					cmsAdminContent = null;
				}

				GetSetUserEditStateAsEmpty();

				return "OK";
			} catch (Exception ex) {
				SiteData.WriteDebugException("webservice", ex);

				return ex.ToString();
			}
		}
	}
}