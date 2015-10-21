using Carrotware.CMS.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web;
using System.Xml;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public partial class SiteData : IValidatableObject {

		public SiteData() { }

		public SiteData(carrot_Site s) {
			if (s != null && String.IsNullOrEmpty(s.TimeZone)) {
				s.TimeZone = SiteTimeZoneInfo.Id;
			}

			this.TimeZoneIdentifier = s.TimeZone;
			this.SiteID = s.SiteID;
			this.MetaKeyword = s.MetaKeyword;
			this.MetaDescription = s.MetaDescription;
			this.SiteName = s.SiteName;
			this.SiteTagline = s.SiteTagline;
			this.SiteTitlebarPattern = s.SiteTitlebarPattern;
			this.MainURL = s.MainURL;
			this.BlockIndex = s.BlockIndex;
			this.SendTrackbacks = s.SendTrackbacks;
			this.AcceptTrackbacks = s.AcceptTrackbacks;

			this.Blog_Root_ContentID = s.Blog_Root_ContentID;

			this.Blog_FolderPath = String.IsNullOrEmpty(s.Blog_FolderPath) ? "" : s.Blog_FolderPath;
			this.Blog_CategoryPath = String.IsNullOrEmpty(s.Blog_CategoryPath) ? "" : s.Blog_CategoryPath;
			this.Blog_TagPath = String.IsNullOrEmpty(s.Blog_TagPath) ? "" : s.Blog_TagPath;
			this.Blog_EditorPath = String.IsNullOrEmpty(s.Blog_TagPath) ? "" : s.Blog_EditorPath;
			this.Blog_DatePath = String.IsNullOrEmpty(s.Blog_DatePath) ? "" : s.Blog_DatePath;
			this.Blog_DatePattern = String.IsNullOrEmpty(s.Blog_DatePattern) ? "yyyy/MM/dd" : s.Blog_DatePattern;

			if (String.IsNullOrEmpty(this.SiteTitlebarPattern)) {
				this.SiteTitlebarPattern = DefaultPageTitlePattern;
			}

			this.LoadTextWidgets();
		}

		public void LoadTextWidgets() {
			try {
				this.SiteTextWidgets = TextWidget.GetSiteTextWidgets(this.SiteID);
			} catch (Exception ex) {
				this.SiteTextWidgets = new List<TextWidget>();
			}
		}

		public string UpdateContent(string textContent) {
			if (!String.IsNullOrEmpty(textContent)) {
				foreach (TextWidget o in this.SiteTextWidgets.Where(x => x.ProcessBody && x.TextProcessor != null)) {
					textContent = o.TextProcessor.UpdateContent(textContent);
				}
			}
			return textContent;
		}

		public string UpdateContentPlainText(string textContent) {
			if (!String.IsNullOrEmpty(textContent)) {
				foreach (TextWidget o in this.SiteTextWidgets.Where(x => x.ProcessPlainText && x.TextProcessor != null)) {
					textContent = o.TextProcessor.UpdateContentPlainText(textContent);
				}
			}
			return textContent;
		}

		public string UpdateContentRichText(string textContent) {
			if (!String.IsNullOrEmpty(textContent)) {
				foreach (TextWidget o in this.SiteTextWidgets.Where(x => x.ProcessHTMLText && x.TextProcessor != null)) {
					textContent = o.TextProcessor.UpdateContentRichText(textContent);
				}
			}
			return textContent;
		}

		public string UpdateContentComment(string textContent) {
			if (!String.IsNullOrEmpty(textContent)) {
				foreach (TextWidget o in this.SiteTextWidgets.Where(x => x.ProcessComment && x.TextProcessor != null)) {
					textContent = o.TextProcessor.UpdateContentComment(textContent);
				}
			}
			return textContent;
		}

		public string UpdateContentSnippet(string textContent) {
			if (!String.IsNullOrEmpty(textContent)) {
				foreach (TextWidget o in this.SiteTextWidgets.Where(x => x.ProcessSnippet && x.TextProcessor != null)) {
					textContent = o.TextProcessor.UpdateContentSnippet(textContent);
				}
			}
			return textContent;
		}

		public List<ContentCategory> GetCategoryList() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				List<ContentCategory> _types = (from d in CompiledQueries.cqGetContentCategoryBySiteID(_db, this.SiteID)
												select new ContentCategory(d)).ToList();

				return _types;
			}
		}

		public List<ContentTag> GetTagList() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				List<ContentTag> _types = (from d in CompiledQueries.cqGetContentTagBySiteID(_db, this.SiteID)
										   select new ContentTag(d)).ToList();

				return _types;
			}
		}

		public List<ContentSnippet> GetContentSnippetList() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				List<ContentSnippet> _types = (from d in CompiledQueries.cqGetSnippetsBySiteID(_db, this.SiteID)
											   select new ContentSnippet(d)).ToList();

				return _types;
			}
		}

		public void SendTrackbackQueue() {
			if (this.SendTrackbacks) {
				List<TrackBackEntry> lstTBQ = TrackBackEntry.GetTrackBackSiteQueue(this.SiteID);

				foreach (TrackBackEntry t in lstTBQ) {
					if (t.CreateDate > this.Now.AddMinutes(-30)) {
						try {
							TrackBacker tb = new TrackBacker();
							t.TrackBackResponse = tb.SendTrackback(t.Root_ContentID, this.SiteID, t.TrackBackURL);
							t.TrackedBack = true;
							t.Save();
						} catch (Exception ex) { }
					}
				}
			}
		}

		public SiteData GetCurrentSite() {
			//return Get(CurrentSiteID);
			return CurrentSite;
		}

		public static SiteData InitNewSite(Guid siteID) {
			SiteData site = new SiteData();
			site.SiteID = siteID;
			site.BlockIndex = true;

			site.MainURL = "http://" + CMSConfigHelper.DomainName;
			site.SiteName = CMSConfigHelper.DomainName;

			site.SiteTitlebarPattern = SiteData.DefaultPageTitlePattern;

			site.Blog_FolderPath = "archive";
			site.Blog_CategoryPath = "category";
			site.Blog_TagPath = "tag";
			site.Blog_DatePath = "date";
			site.Blog_EditorPath = "author";
			site.Blog_DatePattern = "yyyy/MM/dd";

			site.TimeZoneIdentifier = TimeZoneInfo.Local.Id;

			return site;
		}

		public void Save() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				carrot_Site s = CompiledQueries.cqGetSiteByID(_db, this.SiteID);

				bool bNew = false;
				if (s == null) {
					s = new carrot_Site();
					if (this.SiteID == Guid.Empty) {
						this.SiteID = Guid.NewGuid();
					}
					bNew = true;
				}

				// if updating the current site then blank out its cache
				if (CurrentSiteID == this.SiteID) {
					CurrentSite = null;
				}

				s.SiteID = this.SiteID;

				s.TimeZone = this.TimeZoneIdentifier;

				FixMeta();
				s.MetaKeyword = this.MetaKeyword.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("  ", " ");
				s.MetaDescription = this.MetaDescription.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("  ", " ");

				s.SiteName = this.SiteName;
				s.SiteTagline = this.SiteTagline;
				s.SiteTitlebarPattern = this.SiteTitlebarPattern;
				s.MainURL = this.MainURL;
				s.BlockIndex = this.BlockIndex;
				s.SendTrackbacks = this.SendTrackbacks;
				s.AcceptTrackbacks = this.AcceptTrackbacks;

				s.Blog_FolderPath = ContentPageHelper.ScrubSlug(this.Blog_FolderPath);
				s.Blog_CategoryPath = ContentPageHelper.ScrubSlug(this.Blog_CategoryPath);
				s.Blog_TagPath = ContentPageHelper.ScrubSlug(this.Blog_TagPath);
				s.Blog_EditorPath = ContentPageHelper.ScrubSlug(this.Blog_EditorPath);
				s.Blog_DatePath = ContentPageHelper.ScrubSlug(this.Blog_DatePath);

				s.Blog_Root_ContentID = this.Blog_Root_ContentID;
				s.Blog_DatePattern = String.IsNullOrEmpty(this.Blog_DatePattern) ? "yyyy/MM/dd" : this.Blog_DatePattern;

				if (bNew) {
					_db.carrot_Sites.InsertOnSubmit(s);
				}
				_db.SubmitChanges();
			}
		}

		private void FixMeta() {
			this.MetaKeyword = String.IsNullOrEmpty(this.MetaKeyword) ? String.Empty : this.MetaKeyword;
			this.MetaDescription = String.IsNullOrEmpty(this.MetaDescription) ? String.Empty : this.MetaDescription;
		}

		public List<ExtendedUserData> GetMappedUsers() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				return (from l in _db.carrot_UserSiteMappings
						join u in _db.vw_carrot_UserDatas on l.UserId equals u.UserId
						where l.SiteID == this.SiteID
						select new ExtendedUserData(u)).ToList();
			}
		}

		public bool VerifyUserHasSiteAccess(Guid siteID, Guid userID) {
			//all admins have rights to all sites
			if (SecurityData.IsAdmin) {
				return true;
			}

			// if user is neither admin nor editor, they should not be in the backend of the site
			if (!(SecurityData.IsSiteEditor || SecurityData.IsAdmin)) {
				return false;
			}

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				// by this point, the user is probably an editor, make sure they have rights to this site
				IQueryable<Guid> lstSiteIDs = (from l in _db.carrot_UserSiteMappings
											   where l.UserId == userID
													&& l.SiteID == siteID
											   select l.SiteID);

				return lstSiteIDs.Any();
			}

			return false;
		}

		public void CleanUpSerialData() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				IQueryable<carrot_SerialCache> lst = (from c in _db.carrot_SerialCaches
													  where c.EditDate < DateTime.UtcNow.AddHours(-6)
													  && c.SiteID == CurrentSiteID
													  select c);

				_db.carrot_SerialCaches.DeleteBatch(lst);
				_db.SubmitChanges();
			}
		}

		public int GetSitePageCount(ContentPageType.PageType entryType) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				int iCount = CannedQueries.GetAllByTypeList(_db, this.SiteID, false, entryType).Count();
				return iCount;
			}
		}

		public void MapUserToSite(Guid siteID, Guid userID) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				carrot_UserSiteMapping map = new carrot_UserSiteMapping();
				map.UserSiteMappingID = Guid.NewGuid();
				map.SiteID = siteID;
				map.UserId = userID;

				_db.carrot_UserSiteMappings.InsertOnSubmit(map);
				_db.SubmitChanges();
			}
		}

		public List<BasicContentData> GetFullSiteFileList() {
			List<BasicContentData> map = new List<BasicContentData>();

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				IQueryable<vw_carrot_Content> queryAllFiles = CompiledQueries.cqGetAllContent(_db, this.SiteID);
				map = queryAllFiles.Select(x => new BasicContentData(x)).ToList();
			}

			return map;
		}

		public DateTime Now {
			get {
				if (IsWebView && SiteData.CurrentSite != null) {
					return SiteData.CurrentSite.ConvertUTCToSiteTime(DateTime.UtcNow);
				} else {
					return DateTime.Now;
				}
			}
		}

		public TimeZoneInfo SiteTimeZoneInfo {
			get {
				TimeZoneInfo oTZ = TimeZoneInfo.Local;
				if (IsWebView) {
					if (!String.IsNullOrEmpty(this.TimeZoneIdentifier)) {
						try { oTZ = TimeZoneInfo.FindSystemTimeZoneById(this.TimeZoneIdentifier); } catch { }
					}
				}
				return oTZ;
			}
		}

		public DateTime ConvertUTCToSiteTime(DateTime dateUTC) {
			return TimeZoneInfo.ConvertTimeFromUtc(dateUTC, SiteTimeZoneInfo);
		}

		public DateTime ConvertSiteTimeToUTC(DateTime dateSite) {
			DateTime dateSiteSrc = DateTime.SpecifyKind(dateSite, DateTimeKind.Unspecified);
			return TimeZoneInfo.ConvertTimeToUtc(dateSiteSrc, SiteTimeZoneInfo);
		}

		public string ConvertSiteTimeToISO8601(DateTime dateSite) {
			return ConvertSiteTimeToUTC(dateSite).ToString("s") + "Z";
		}

		public DateTime ConvertSiteTimeToLocalServer(DateTime dateSite) {
			DateTime dateSiteSrc = DateTime.SpecifyKind(dateSite, DateTimeKind.Unspecified);
			DateTime utc = TimeZoneInfo.ConvertTimeToUtc(dateSiteSrc, SiteTimeZoneInfo);

			return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);
		}

		public DateTime ConvertUTCToLocalServer(DateTime dateUTC) {
			return TimeZoneInfo.ConvertTimeFromUtc(dateUTC, TimeZoneInfo.Local);
		}

		public bool SendTrackbacks { get; set; }
		public bool AcceptTrackbacks { get; set; }

		[Display(Name = "Block Index")]
		public bool BlockIndex { get; set; }

		[StringLength(128)]
		[Display(Name = "Site URL")]
		[Required]
		public string MainURL { get; set; }

		[StringLength(1024)]
		public string MetaDescription { get; set; }

		[StringLength(1024)]
		public string MetaKeyword { get; set; }

		[Display(Name = "Site ID")]
		[Required]
		public Guid SiteID { get; set; }

		[StringLength(256)]
		[Display(Name = "Site Name")]
		[Required]
		public string SiteName { get; set; }

		[StringLength(1024)]
		public string SiteTagline { get; set; }

		[StringLength(1024)]
		public string SiteTitlebarPattern { get; set; }

		[StringLength(128)]
		[Required]
		public string TimeZoneIdentifier { get; set; }

		private List<TextWidget> SiteTextWidgets { get; set; }

		public Guid? Blog_Root_ContentID { get; set; }

		[StringLength(64)]
		[Display(Name = "Folder Path")]
		[Required]
		public string Blog_FolderPath { get; set; }

		[StringLength(64)]
		[Display(Name = "Category Path")]
		[Required]
		public string Blog_CategoryPath { get; set; }

		[StringLength(64)]
		[Display(Name = "Tag Path")]
		[Required]
		public string Blog_TagPath { get; set; }

		[StringLength(32)]
		[Display(Name = "Date Pattern")]
		[Required]
		public string Blog_DatePattern { get; set; }

		[StringLength(64)]
		[Display(Name = "Editor Path")]
		[Required]
		public string Blog_EditorPath { get; set; }

		[StringLength(64)]
		[Display(Name = "Date Path")]
		[Required]
		public string Blog_DatePath { get; set; }

		public string BlogFolderPath {
			get { return RemoveDupeSlashes("/" + this.Blog_FolderPath + "/"); }
		}

		public string BlogCategoryPath {
			get { return RemoveDupeSlashes(BlogFolderPath + this.Blog_CategoryPath + "/"); }
		}

		public string BlogTagPath {
			get { return RemoveDupeSlashes(BlogFolderPath + this.Blog_TagPath + "/"); }
		}

		public string BlogDateFolderPath {
			get { return RemoveDupeSlashes(BlogFolderPath + this.Blog_DatePath + "/"); }
		}

		public string BlogEditorFolderPath {
			get { return RemoveDupeSlashes(BlogFolderPath + this.Blog_EditorPath + "/"); }
		}

		public string SiteSearchPath {
			get { return RemoveDupeSlashes(BlogFolderPath + SiteSearchPageName); }
		}

		public bool IsBlogCategoryPath {
			get { return SiteData.CurrentScriptName.ToLower().StartsWith(this.BlogCategoryPath); }
		}

		public bool IsBlogTagPath {
			get { return SiteData.CurrentScriptName.ToLower().StartsWith(this.BlogTagPath); }
		}

		public bool IsBlogDateFolderPath {
			get { return SiteData.CurrentScriptName.ToLower().StartsWith(this.BlogDateFolderPath); }
		}

		public bool IsBlogEditorFolderPath {
			get { return SiteData.CurrentScriptName.ToLower().StartsWith(this.BlogEditorFolderPath); }
		}

		public bool IsSiteSearchPath {
			get { return SiteData.CurrentScriptName.ToLower().StartsWith(this.SiteSearchPath); }
		}

		public bool CheckIsBlogCategoryPath(string sFilterPath) {
			return sFilterPath.ToLower().StartsWith(this.BlogCategoryPath);
		}

		public bool CheckIsBlogTagPath(string sFilterPath) {
			return sFilterPath.ToLower().StartsWith(this.BlogTagPath);
		}

		public bool CheckIsBlogDateFolderPath(string sFilterPath) {
			return sFilterPath.ToLower().StartsWith(this.BlogDateFolderPath);
		}

		public bool CheckIsBlogEditorFolderPath(string sFilterPath) {
			return sFilterPath.ToLower().StartsWith(this.BlogEditorFolderPath);
		}

		public bool CheckIsSiteSearchPath(string sFilterPath) {
			return sFilterPath.ToLower().StartsWith(this.SiteSearchPath);
		}

		public List<string> GetSpecialFilePathPrefixes() {
			List<string> lst = new List<string>();

			lst.Add(this.BlogCategoryPath.ToLower());
			lst.Add(this.BlogTagPath.ToLower());
			lst.Add(this.BlogDateFolderPath.ToLower());
			lst.Add(this.BlogEditorFolderPath.ToLower());
			lst.Add(this.SiteSearchPath.ToLower());

			return lst;
		}

		public string MainCanonicalURL {
			get { return RemoveDupeSlashesURL(this.MainURL + "/"); }
		}

		public string DefaultCanonicalURL {
			get { return RemoveDupeSlashesURL(this.MainCanonicalURL + CurrentScriptName); }
		}

		public string ConstructedCanonicalURL(string sFileName) {
			return RemoveDupeSlashesURL(this.MainCanonicalURL + sFileName);
		}

		public string ConstructedCanonicalURL(ContentPage cp) {
			return RemoveDupeSlashesURL(this.MainCanonicalURL + cp.FileName);
		}

		public string ConstructedCanonicalURL(SiteNav nav) {
			return RemoveDupeSlashesURL(this.MainCanonicalURL + nav.FileName);
		}

		public string BuildDateSearchLink(DateTime postDate) {
			return RemoveDupeSlashes(this.BlogDateFolderPath + postDate.ToString("/yyyy/MM/dd/") + SiteData.SiteSearchPageName);
		}

		public string BuildMonthSearchLink(DateTime postDate) {
			return RemoveDupeSlashes(this.BlogDateFolderPath + postDate.ToString("/yyyy/MM/") + SiteData.SiteSearchPageName);
		}

		private string RemoveDupeSlashes(string sInput) {
			if (!String.IsNullOrEmpty(sInput)) {
				return sInput.Replace("//", "/").Replace("//", "/");
			} else {
				return String.Empty;
			}
		}

		private string RemoveDupeSlashesURL(string sInput) {
			if (!String.IsNullOrEmpty(sInput)) {
				if (!sInput.ToLower().StartsWith("http")) {
					sInput = "http://" + sInput;
				}
				return RemoveDupeSlashes(sInput.Replace("://", "¤¤¤")).Replace("¤¤¤", "://");
			} else {
				return String.Empty;
			}
		}

		//==========BEGIN RSS=================

		public enum RSSFeedInclude {
			Unknown,
			BlogAndPages,
			BlogOnly,
			PageOnly
		}

		public void RenderRSSFeed(HttpContext context) {
			SiteData.RSSFeedInclude FeedType = SiteData.RSSFeedInclude.BlogAndPages;

			if (!String.IsNullOrEmpty(context.Request.QueryString["type"])) {
				string feedType = context.Request.QueryString["type"].ToString();

				FeedType = (SiteData.RSSFeedInclude)Enum.Parse(typeof(SiteData.RSSFeedInclude), feedType, true);
			}

			string sRSSXML = SiteData.CurrentSite.GetRSSFeed(FeedType);

			context.Response.ContentType = SiteData.RssDocType;

			context.Response.Write(sRSSXML);

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
		}

		public HtmlString GetRSSFeed(string feedType) {
			SiteData.RSSFeedInclude FeedType = SiteData.RSSFeedInclude.BlogAndPages;

			if (!String.IsNullOrEmpty(feedType)) {
				FeedType = (SiteData.RSSFeedInclude)Enum.Parse(typeof(SiteData.RSSFeedInclude), feedType, true);
			}

			string sRSSXML = SiteData.CurrentSite.GetRSSFeed(FeedType);

			return new HtmlString(sRSSXML);
		}

		public string GetRSSFeed(RSSFeedInclude feedData) {
			SyndicationFeed feed = CreateRecentItemFeed(feedData);

			StringBuilder sb = new StringBuilder();
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.Encoding = Encoding.UTF8;
			settings.CheckCharacters = true;

			using (XmlWriter xw = XmlWriter.Create(sb, settings)) {
				Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(feed);
				rssFormatter.WriteTo(xw);
			}

			string xml = sb.ToString();
			xml = xml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"", "<?xml version=\"1.0\" encoding=\"utf-8\"");

			return xml;
		}

		private SyndicationFeed CreateRecentItemFeed(RSSFeedInclude feedData) {
			List<SyndicationItem> syndicationItems = GetRecentPagesOrPosts(feedData);

			return new SyndicationFeed(syndicationItems) {
				Title = new TextSyndicationContent(this.SiteName),
				Description = new TextSyndicationContent(this.SiteTagline)
			};
		}

		private List<SyndicationItem> GetRecentPagesOrPosts(RSSFeedInclude feedData) {
			List<SyndicationItem> syndRSS = new List<SyndicationItem>();
			List<SiteNav> lst = new List<SiteNav>();

			ContentPageType PageType = new ContentPageType();

			using (SiteNavHelper navHelper = new SiteNavHelper()) {
				if (feedData == RSSFeedInclude.PageOnly || feedData == RSSFeedInclude.BlogAndPages) {
					List<SiteNav> lst1 = navHelper.GetLatest(this.SiteID, 8, true);
					lst = lst.Union(lst1).ToList();
					List<SiteNav> lst2 = navHelper.GetLatestUpdates(this.SiteID, 10, true);
					lst = lst.Union(lst2).ToList();
				}
				if (feedData == RSSFeedInclude.BlogOnly || feedData == RSSFeedInclude.BlogAndPages) {
					List<SiteNav> lst1 = navHelper.GetLatestPosts(this.SiteID, 8, true);
					lst = lst.Union(lst1).ToList();
					List<SiteNav> lst2 = navHelper.GetLatestPostUpdates(this.SiteID, 10, true);
					lst = lst.Union(lst2).ToList();
				}
			}

			lst.RemoveAll(x => x.ShowInSiteMap == false && x.ContentType == ContentPageType.PageType.ContentEntry);
			lst.RemoveAll(x => x.BlockIndex == true);

			foreach (SiteNav sn in lst) {
				SyndicationItem si = new SyndicationItem();

				string sPageURI = RemoveDupeSlashesURL(this.ConstructedCanonicalURL(sn));

				Uri PageURI = new Uri(sPageURI);

				si.Content = new TextSyndicationContent(sn.PageTextPlainSummaryMedium.ToString());
				si.Title = new TextSyndicationContent(sn.NavMenuText);
				si.Links.Add(SyndicationLink.CreateSelfLink(PageURI));
				si.AddPermalink(PageURI);

				si.LastUpdatedTime = sn.EditDate;
				si.PublishDate = sn.CreateDate;

				syndRSS.Add(si);
			}

			return syndRSS.OrderByDescending(p => p.PublishDate).ToList();
		}

		//==========END RSS=================

		private List<ValidationResult> _errors = null;

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			if (_errors == null) {
				_errors = new List<ValidationResult>();
				List<string> lst = new List<string>();

				if (!String.IsNullOrEmpty(this.Blog_CategoryPath)) {
					lst.Add(this.Blog_CategoryPath.ToLower().Trim());
				}
				if (!String.IsNullOrEmpty(this.Blog_TagPath)) {
					lst.Add(this.Blog_TagPath.ToLower().Trim());
				}
				if (!String.IsNullOrEmpty(this.Blog_DatePath)) {
					lst.Add(this.Blog_DatePath.ToLower().Trim());
				}
				if (!String.IsNullOrEmpty(this.Blog_EditorPath)) {
					lst.Add(this.Blog_EditorPath.ToLower().Trim());
				}

				List<string> duplicates = lst.GroupBy(s => s).SelectMany(grp => grp.Skip(1)).ToList();
				if (duplicates.Any()) {
					ValidationResult err = new ValidationResult("One or more paths are not unique.", new string[] { "Blog_CategoryPath", "Blog_TagPath", "Blog_DatePath", "Blog_EditorPath" });
					_errors.Add(err);
				}

				if (!FoldersAreValid()) {
					ValidationResult err = new ValidationResult("One or more paths are in conflict with existing site content.", new string[] { "Blog_FolderPath" });
					_errors.Add(err);
				}
			}

			return _errors;
		}

		protected bool FoldersAreValid() {
			string sFolderPath = this.Blog_FolderPath ?? String.Empty;

			if (SiteData.CurretSiteExists) {
				using (ContentPageHelper pageHelper = new ContentPageHelper()) {
					var exists = pageHelper.ExistingPagesBeginWith(this);

					return !exists;
				}
			} else {
				return true;
			}

			return false;
		}
	}
}