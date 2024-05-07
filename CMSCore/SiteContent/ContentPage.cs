using Carrotware.CMS.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
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

namespace Carrotware.CMS.Core {

	public class ContentPage : ISiteContent, IValidatableObject {

		public ContentPage() { }

		public ContentPage(Guid siteID, ContentPageType.PageType pageType) {
			this.Root_ContentID = Guid.Empty;
			this.ContentID = Guid.NewGuid();
			this.ContentType = pageType;
			this.SiteID = siteID;
			this.VersionCount = 0;
			this.NavOrder = -1;

			DateTime siteTime = SiteData.GetSiteByID(siteID).Now;

			this.CreateDate = siteTime;
			this.EditDate = siteTime;
			this.GoLiveDate = siteTime.AddMinutes(-5);
			this.RetireDate = siteTime.AddYears(200);

			//this.NavMenuText = "PAGE " + this.Root_ContentID.ToString().ToLowerInvariant();
			//this.FileName = "/" + this.Root_ContentID.ToString().ToLowerInvariant();
			this.NavMenuText = string.Empty;
			this.FileName = string.Empty;
			this.TemplateFile = SiteData.DefaultTemplateFilename;

			this.BlockIndex = false;
			this.PageActive = true;
			this.ShowInSiteMap = true;
			this.ShowInSiteNav = true;

			this.LeftPageText = PageContentEmpty;
			this.RightPageText = PageContentEmpty;
			this.PageText = PageContentEmpty;

			if (pageType != ContentPageType.PageType.ContentEntry) {
				this.Parent_ContentID = null;
				this.NavOrder = SiteData.BlogSortOrderNumber;
				this.ShowInSiteMap = false;
				this.ShowInSiteNav = false;
			}

			this.ContentCategories = new List<ContentCategory>();
			this.ContentTags = new List<ContentTag>();
		}

		internal ContentPage(vw_carrot_Content c) {
			if (c != null) {
				SiteData site = SiteData.GetSiteFromCache(c.SiteID);

				this.Root_ContentID = c.Root_ContentID;
				this.SiteID = c.SiteID;
				this.Heartbeat_UserId = c.Heartbeat_UserId;
				this.EditHeartbeat = c.EditHeartbeat;
				this.FileName = c.FileName;

				this.CreateUserId = c.CreateUserId;
				this.CreateDate = site.ConvertUTCToSiteTime(c.CreateDate);

				this.GoLiveDate = site.ConvertUTCToSiteTime(c.GoLiveDate);
				this.RetireDate = site.ConvertUTCToSiteTime(c.RetireDate);
				this.EditDate = site.ConvertUTCToSiteTime(c.EditDate);

				this.ShowInSiteMap = c.ShowInSiteMap;
				this.BlockIndex = c.BlockIndex;
				this.PageActive = c.PageActive;
				this.ShowInSiteNav = c.ShowInSiteNav;

				this.PageSlug = c.PageSlug;
				this.ContentType = ContentPageType.GetTypeByID(c.ContentTypeID);

				this.ContentID = c.ContentID;
				this.Parent_ContentID = c.Parent_ContentID;
				this.IsLatestVersion = c.IsLatestVersion;
				this.TitleBar = c.TitleBar;
				this.NavMenuText = c.NavMenuText;
				this.PageHead = c.PageHead;
				this.PageText = c.PageText;
				this.LeftPageText = c.LeftPageText;
				this.RightPageText = c.RightPageText;
				this.NavOrder = c.NavOrder;
				this.EditUserId = c.EditUserId;
				this.CreditUserId = c.CreditUserId;
				this.TemplateFile = c.TemplateFile;
				this.Thumbnail = c.PageThumbnail;

				if (string.IsNullOrEmpty(this.PageSlug) && this.ContentType == ContentPageType.PageType.BlogEntry) {
					this.PageSlug = c.FileName;
				}

				this.MetaDescription = c.MetaDescription;
				this.MetaKeyword = c.MetaKeyword;

				this.VersionCount = c.VersionCount.HasValue ? c.VersionCount.Value : 0;
			}
		}

		public static string PageContentEmpty {
			get { return "<p>  </p>"; }
		}

		public SiteNav GetSiteNav() {
			SiteNav sd = null;
			if (SiteData.IsPageSampler || !SiteData.IsWebView) {
				sd = SiteNavHelper.GetSamplerView();
			} else {
				using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
					sd = navHelper.GetLatestVersion(this.SiteID, this.Root_ContentID);
				}
				// in the case of stubbed data, which is not in the db, convert the content already present
				if (sd == null) {
					sd = new SiteNav();
					sd.Root_ContentID = this.Root_ContentID;
					sd.ContentID = this.ContentID;
					sd.ContentType = this.ContentType;
					sd.SiteID = this.SiteID;
					sd.NavOrder = this.NavOrder;
					sd.FileName = this.FileName;
					sd.NavMenuText = this.NavMenuText;
					sd.TitleBar = this.TitleBar;
					sd.TemplateFile = this.TemplateFile;
					sd.CreateDate = this.CreateDate;
					sd.RetireDate = this.RetireDate;
					sd.PageActive = this.PageActive;
					sd.PageText = this.PageText;
				}
			}
			return sd;
		}

		public void ResetHeartbeatLock() {
			using (var db = CarrotCMSDataContext.Create()) {
				carrot_RootContent rc = CompiledQueries.cqGetRootContentTbl(db, this.SiteID, this.Root_ContentID);

				if (rc != null) {
					rc.EditHeartbeat = DateTime.UtcNow.AddHours(-2);
					rc.Heartbeat_UserId = null;
					db.SubmitChanges();
				}
			}
		}

		public void RecordHeartbeatLock(Guid currentUserID) {
			using (var db = CarrotCMSDataContext.Create()) {
				carrot_RootContent rc = CompiledQueries.cqGetRootContentTbl(db, this.SiteID, this.Root_ContentID);

				if (rc != null) {
					rc.Heartbeat_UserId = currentUserID;
					rc.EditHeartbeat = DateTime.UtcNow == rc.EditHeartbeat ? DateTime.UtcNow.AddSeconds(-3) : DateTime.UtcNow;
					db.SubmitChanges();
				}
			}
		}

		public void LoadAttributes() {
			var lstC = this.ContentCategories;
			var lstT = this.ContentTags;
		}

		public List<Widget> GetWidgetList() {
			List<Widget> widgets = null;

			using (WidgetHelper pwh = new WidgetHelper()) {
				widgets = pwh.GetWidgets(this.Root_ContentID, false);
			}

			return widgets;
		}

		public List<Widget> GetAllWidgetsOrUnsaved() {
			List<Widget> widgets = new List<Widget>();
			using (CMSConfigHelper ch = new CMSConfigHelper()) {
				if (ch.cmsAdminContent == null) {
					widgets = this.GetWidgetList();
				} else {
					widgets = (from w in ch.cmsAdminWidget
							   orderby w.WidgetOrder, w.EditDate
							   select w).ToList();
				}
			}
			return widgets;
		}

		private void FixMeta() {
			this.MetaKeyword = string.IsNullOrEmpty(this.MetaKeyword) ? string.Empty : this.MetaKeyword;
			this.MetaDescription = string.IsNullOrEmpty(this.MetaDescription) ? string.Empty : this.MetaDescription;
		}

		private void SaveKeywordsAndTags(CarrotCMSDataContext db) {
			IQueryable<carrot_TagContentMapping> oldContentTags = CannedQueries.GetContentTagMapByContentID(db, this.Root_ContentID);
			IQueryable<carrot_CategoryContentMapping> oldContentCategories = CannedQueries.GetContentCategoryMapByContentID(db, this.Root_ContentID);

			if (this.ContentType == ContentPageType.PageType.BlogEntry) {
				List<carrot_TagContentMapping> newContentTags = (from x in this.ContentTags
																 select new carrot_TagContentMapping {
																	 ContentTagID = x.ContentTagID,
																	 Root_ContentID = this.Root_ContentID,
																	 TagContentMappingID = Guid.NewGuid()
																 }).ToList();

				List<carrot_CategoryContentMapping> newContentCategories = (from x in this.ContentCategories
																			select new carrot_CategoryContentMapping {
																				ContentCategoryID = x.ContentCategoryID,
																				Root_ContentID = this.Root_ContentID,
																				CategoryContentMappingID = Guid.NewGuid()
																			}).ToList();

				foreach (carrot_TagContentMapping s in newContentTags) {
					db.carrot_TagContentMappings.InsertOnSubmit(s);
				}
				foreach (carrot_CategoryContentMapping s in newContentCategories) {
					db.carrot_CategoryContentMappings.InsertOnSubmit(s);
				}
			}

			db.carrot_TagContentMappings.BatchDelete(oldContentTags);
			db.carrot_CategoryContentMappings.BatchDelete(oldContentCategories);
		}

		private void PerformCommonSaveRoot(SiteData pageSite, carrot_RootContent rc) {
			rc.Root_ContentID = this.Root_ContentID;
			rc.PageActive = true;
			rc.BlockIndex = false;
			rc.ShowInSiteMap = true;

			rc.SiteID = this.SiteID;
			rc.ContentTypeID = ContentPageType.GetIDByType(this.ContentType);

			rc.CreateDate = DateTime.UtcNow;
			if (this.CreateUserId != Guid.Empty) {
				rc.CreateUserId = this.CreateUserId;
			} else {
				rc.CreateUserId = SecurityData.CurrentUserGuid;
			}
			rc.GoLiveDate = pageSite.ConvertSiteTimeToUTC(this.GoLiveDate);
			rc.RetireDate = pageSite.ConvertSiteTimeToUTC(this.RetireDate);

			if (this.CreateDate.Year > 1950) {
				rc.CreateDate = pageSite.ConvertSiteTimeToUTC(this.CreateDate);
			}
		}

		private void PerformCommonSave(SiteData pageSite, carrot_RootContent rc, carrot_Content c) {
			c.NavOrder = this.NavOrder;

			if (this.ContentType == ContentPageType.PageType.BlogEntry) {
				this.PageSlug = ContentPageHelper.ScrubFilename(this.Root_ContentID, this.PageSlug);
				this.FileName = ContentPageHelper.CreateFileNameFromSlug(this.SiteID, this.GoLiveDate, this.PageSlug);
				c.NavOrder = SiteData.BlogSortOrderNumber;
			}

			rc.GoLiveDate = pageSite.ConvertSiteTimeToUTC(this.GoLiveDate);
			rc.RetireDate = pageSite.ConvertSiteTimeToUTC(this.RetireDate);

			rc.GoLiveDateLocal = pageSite.ConvertUTCToSiteTime(rc.GoLiveDate);

			rc.PageSlug = this.PageSlug;
			rc.PageThumbnail = this.Thumbnail;

			c.Root_ContentID = this.Root_ContentID;

			rc.Heartbeat_UserId = this.Heartbeat_UserId;
			rc.EditHeartbeat = this.EditHeartbeat;

			rc.FileName = this.FileName;
			rc.PageActive = this.PageActive;
			rc.ShowInSiteNav = this.ShowInSiteNav;
			rc.BlockIndex = this.BlockIndex;
			rc.ShowInSiteMap = this.ShowInSiteMap;

			rc.FileName = ContentPageHelper.ScrubFilename(this.Root_ContentID, rc.FileName);

			c.Parent_ContentID = this.Parent_ContentID;
			c.IsLatestVersion = true;
			c.TitleBar = this.TitleBar;
			c.NavMenuText = this.NavMenuText;
			c.PageHead = this.PageHead;
			c.PageText = this.PageText;
			c.LeftPageText = this.LeftPageText;
			c.RightPageText = this.RightPageText;

			c.EditUserId = this.EditUserId;
			c.CreditUserId = this.CreditUserId;

			c.EditDate = DateTime.UtcNow;
			c.TemplateFile = this.TemplateFile;

			FixMeta();
			c.MetaKeyword = this.MetaKeyword.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("  ", " ");
			c.MetaDescription = this.MetaDescription.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("  ", " ");

			this.Root_ContentID = rc.Root_ContentID;
			this.ContentID = c.ContentID;
			this.FileName = rc.FileName;
			this.EditDate = pageSite.ConvertUTCToSiteTime(c.EditDate);
			this.CreateDate = pageSite.ConvertUTCToSiteTime(rc.CreateDate);
			this.GoLiveDate = pageSite.ConvertUTCToSiteTime(rc.GoLiveDate);
			this.RetireDate = pageSite.ConvertUTCToSiteTime(rc.RetireDate);
		}

		public void SaveTrackbacks() {
			SiteData site = SiteData.GetSiteFromCache(this.SiteID);

			if (!string.IsNullOrEmpty(this.NewTrackBackURLs)) {
				this.NewTrackBackURLs = this.NewTrackBackURLs.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n\n", "\n");
				string[] TBURLs = this.NewTrackBackURLs.Split('\n');
				foreach (string sURL in TBURLs) {
					List<TrackBackEntry> lstTB = TrackBackEntry.FindTrackbackByURL(this.Root_ContentID, sURL);
					if (lstTB.Count < 1) {
						TrackBackEntry tbe = new TrackBackEntry {
							Root_ContentID = this.Root_ContentID,
							TrackBackURL = sURL,
							TrackedBack = false
						};

						tbe.Save();
					}
				}
			}

			if (this.IsLatestVersion && site.SendTrackbacks) {
				List<TrackBackEntry> lstTBQ = TrackBackEntry.GetTrackBackQueue(this.Root_ContentID);
				foreach (TrackBackEntry t in lstTBQ) {
					if (t.CreateDate > site.Now.AddMinutes(-10)) {
						try {
							TrackBacker tb = new TrackBacker();
							t.TrackBackResponse = tb.SendTrackback(t.Root_ContentID, site.SiteID, t.TrackBackURL);
							t.TrackedBack = true;
							t.Save();
						} catch (Exception ex) { }
					}
				}
			}
		}

		public void SaveTrackbackTop() {
			SiteData site = SiteData.GetSiteFromCache(this.SiteID);

			if (this.IsLatestVersion && site.SendTrackbacks) {
				TrackBackEntry t = TrackBackEntry.GetTrackBackQueue(this.Root_ContentID).FirstOrDefault();
				if (t != null && t.CreateDate > site.Now.AddMinutes(-10)) {
					try {
						TrackBacker tb = new TrackBacker();
						t.TrackBackResponse = tb.SendTrackback(t.Root_ContentID, site.SiteID, t.TrackBackURL);
						t.TrackedBack = true;
						t.Save();
					} catch (Exception ex) { }
				}
			}
		}

		public void ApplyTemplate() {
			using (var db = CarrotCMSDataContext.Create()) {
				carrot_Content c = CompiledQueries.cqGetLatestContentTbl(db, this.SiteID, this.Root_ContentID);

				if (c != null) {
					c.TemplateFile = this.TemplateFile;

					db.SubmitChanges();
				}
			}
		}

		public void SavePageEdit() {
			using (var db = CarrotCMSDataContext.Create()) {
				SiteData site = SiteData.GetSiteFromCache(this.SiteID);

				if (this.Root_ContentID == Guid.Empty) {
					this.Root_ContentID = Guid.NewGuid();
				}
				if (this.ContentID == Guid.Empty) {
					this.ContentID = Guid.NewGuid();
				}
				if (this.Parent_ContentID == Guid.Empty) {
					this.Parent_ContentID = null;
				}

				carrot_RootContent rc = CompiledQueries.cqGetRootContentTbl(db, this.SiteID, this.Root_ContentID);

				carrot_Content oldC = CompiledQueries.cqGetLatestContentTbl(db, this.SiteID, this.Root_ContentID);

				bool bNew = false;

				if (rc == null) {
					rc = new carrot_RootContent();

					PerformCommonSaveRoot(site, rc);

					db.carrot_RootContents.InsertOnSubmit(rc);
					bNew = true;
				}

				carrot_Content c = new carrot_Content();
				c.ContentID = Guid.NewGuid();
				if (!bNew) {
					oldC.IsLatestVersion = false;
				}

				PerformCommonSave(site, rc, c);

				db.carrot_Contents.InsertOnSubmit(c);

				SaveKeywordsAndTags(db);

				db.SubmitChanges();

				SaveTrackbacks();
			}
		}

		public void SavePageAsDraft() {
			using (var db = CarrotCMSDataContext.Create()) {
				SiteData site = SiteData.GetSiteFromCache(this.SiteID);

				if (this.Root_ContentID == Guid.Empty) {
					this.Root_ContentID = Guid.NewGuid();
				}
				if (this.ContentID == Guid.Empty) {
					this.ContentID = Guid.NewGuid();
				}

				carrot_RootContent rc = CompiledQueries.cqGetRootContentTbl(db, this.SiteID, this.Root_ContentID);

				if (rc == null) {
					rc = new carrot_RootContent();

					PerformCommonSaveRoot(site, rc);

					db.carrot_RootContents.InsertOnSubmit(rc);
				}

				carrot_Content c = new carrot_Content();
				c.ContentID = Guid.NewGuid();

				PerformCommonSave(site, rc, c);

				c.IsLatestVersion = false; // draft, leave existing version latest

				db.carrot_Contents.InsertOnSubmit(c);

				SaveKeywordsAndTags(db);

				db.SubmitChanges();

				this.IsLatestVersion = c.IsLatestVersion;
				SaveTrackbacks();
			}
		}

		[Display(Name = "Content Type")]
		public ContentPageType.PageType ContentType { get; set; }

		[Display(Name = "Slug")]
		public string PageSlug { get; set; }

		public Guid ContentID { get; set; }
		public Guid Root_ContentID { get; set; }

		[Display(Name = "Edit Date")]
		public DateTime EditDate { get; set; }

		[Display(Name = "Date Created")]
		public DateTime CreateDate { get; set; }

		[Display(Name = "Go Live Date")]
		public DateTime GoLiveDate { get; set; }

		[Display(Name = "Retire Date")]
		public DateTime RetireDate { get; set; }

		public Guid? EditUserId { get; set; }
		public Guid? CreditUserId { get; set; }

		public Guid CreateUserId { get; set; }

		[Display(Name = "Latest")]
		public bool IsLatestVersion { get; set; }

		[Display(Name = "Template File")]
		[StringLength(256)]
		[Required]
		public string TemplateFile { get; set; }

		public string Thumbnail { get; set; }

		[Display(Name = "File Name")]
		[StringLength(256)]
		[Required]
		public string FileName { get; set; }

		[Display(Name = "Heading")]
		[StringLength(256)]
		public string PageHead { get; set; }

		[Display(Name = "Title")]
		[StringLength(256)]
		[Required]
		public string TitleBar { get; set; }

		[Display(Name = "Nav Text")]
		[StringLength(256)]
		[Required]
		public string NavMenuText { get; set; }

		[Display(Name = "Body")]
		public string PageText { get; set; }

		[Display(Name = "Left Body")]
		public string LeftPageText { get; set; }

		[Display(Name = "Right Body")]
		public string RightPageText { get; set; }

		public int NavOrder { get; set; }
		public Guid? Parent_ContentID { get; set; }

		public DateTime? EditHeartbeat { get; set; }
		public Guid? Heartbeat_UserId { get; set; }

		[Display(Name = "In Site Map")]
		public bool ShowInSiteMap { get; set; }

		[Display(Name = "Block")]
		public bool BlockIndex { get; set; }

		[Display(Name = "Public")]
		public bool PageActive { get; set; }

		[Display(Name = "In Site Nav")]
		public bool ShowInSiteNav { get; set; }

		public bool MadeSafe { get; set; }

		[Required]
		public Guid SiteID { get; set; }

		[Display(Name = "Meta Description")]
		[StringLength(1024)]
		public string MetaDescription { get; set; }

		[Display(Name = "Meta Keyword")]
		[StringLength(1024)]
		public string MetaKeyword { get; set; }

		[Display(Name = "Versions")]
		public int VersionCount { get; set; }

		public string NewTrackBackURLs { get; set; }

		[Display(Name = "Selected Item")]
		public bool Selected { get; set; }

		public bool IsPageLocked {
			get {
				bool bLock = false;
				if (this.Heartbeat_UserId != null) {
					if (this.Heartbeat_UserId != SecurityData.CurrentUserGuid
							&& this.EditHeartbeat.Value > DateTime.UtcNow.AddMinutes(-2)) {
						bLock = true;
					}
					if (this.Heartbeat_UserId == SecurityData.CurrentUserGuid
						|| this.Heartbeat_UserId == null) {
						bLock = false;
					}
				}
				return bLock;
			}
		}

		[Display(Name = "Retired")]
		public bool IsRetired {
			get {
				if (SiteData.CurrentSiteExists) {
					return this.RetireDate <= SiteData.CurrentSite.Now;
				} else {
					return this.RetireDate <= DateTime.UtcNow;
				}
			}
		}

		[Display(Name = "Unreleased")]
		public bool IsUnReleased {
			get {
				if (SiteData.CurrentSiteExists) {
					return this.GoLiveDate >= SiteData.CurrentSite.Now;
				} else {
					return this.GoLiveDate >= DateTime.UtcNow;
				}
			}
		}

		private int _commentCount = -1;

		[Display(Name = "Comment Count")]
		public int CommentCount {
			get {
				if (_commentCount < 0) {
					_commentCount = PostComment.GetCommentCountByContent(this.Root_ContentID, !SecurityData.IsAuthEditor);
				}
				return _commentCount;
			}
			set {
				_commentCount = value;
			}
		}

		private List<ContentTag> _contentTags = null;

		public List<ContentTag> ContentTags {
			get {
				if (_contentTags == null) {
					_contentTags = ContentTag.BuildTagList(this.Root_ContentID);
				}
				return _contentTags;
			}
			set {
				_contentTags = value;
			}
		}

		private List<ContentCategory> _contentCategories = null;

		public List<ContentCategory> ContentCategories {
			get {
				if (_contentCategories == null) {
					_contentCategories = ContentCategory.BuildCategoryList(this.Root_ContentID);
				}
				return _contentCategories;
			}
			set {
				_contentCategories = value;
			}
		}

		private ExtendedUserData _user = null;

		public ExtendedUserData GetUserInfo() {
			return this.EditUser;
		}

		public ExtendedUserData EditUser {
			get {
				if (_user == null && this.EditUserId.HasValue) {
					_user = new ExtendedUserData(this.EditUserId.Value);
				}
				return _user;
			}
		}

		private ExtendedUserData _crUser = null;

		public ExtendedUserData GetCreateUserInfo() {
			return this.CreateUser;
		}

		public ExtendedUserData CreateUser {
			get {
				if (_crUser == null) {
					_crUser = new ExtendedUserData(this.CreateUserId);
				}
				return _crUser;
			}
		}

		private ExtendedUserData _creditUser = null;

		public ExtendedUserData GetCreditUserInfo() {
			return this.CreditUser;
		}

		public ExtendedUserData CreditUser {
			get {
				if (_creditUser == null && this.CreditUserId.HasValue) {
					_creditUser = new ExtendedUserData(this.CreditUserId.Value);
				}
				return _creditUser;
			}
		}

		private ExtendedUserData _bylineUser = null;

		public ExtendedUserData BylineUser {
			get {
				if (_bylineUser == null) {
					_bylineUser = this.CreditUser;
				}
				if (_bylineUser == null) {
					_bylineUser = this.EditUser;
				}
				if (_bylineUser == null) {
					_bylineUser = this.CreateUser;
				}

				return _bylineUser;
			}
		}

		public List<TrackBackEntry> GetTrackbacks() {
			return TrackBackEntry.GetPageTrackBackList(this.Root_ContentID);
		}

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is ContentPage) {
				ContentPage p = (ContentPage)obj;
				return (this.ContentID == p.ContentID)
						&& (this.SiteID == p.SiteID)
						&& (this.Root_ContentID == p.Root_ContentID);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return this.ContentID.GetHashCode() ^ this.SiteID.GetHashCode() ^ this.Root_ContentID.GetHashCode();
		}

		public MvcHtmlString PageTextPlainSummaryMedium {
			get {
				string txt = this.PageText ?? string.Empty;
				txt = txt.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("&nbsp;", " ").Replace('\u00A0', ' '); //.Replace(".", "&#46;").Replace("@", " &#40;&#97;&#116;&#41; ");

				txt = Regex.Replace(txt, @"<!--(\n|.)*-->", " ");
				txt = Regex.Replace(txt, @"<(.|\n)*?>", " ");
				txt = txt.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ").Replace("  ", " ");

				txt = SiteData.CurrentSite.UpdateContent(txt);

				if (txt.Length > 4096) {
					txt = txt.Substring(0, 4096);
				}

				if (txt.Length > 800) {
					txt = txt.Substring(0, 768).Trim() + "[.....]";
				}

				return new MvcHtmlString(txt);
			}
		}

		public MvcHtmlString PageTextPlainSummary {
			get {
				string txt = this.PageTextPlainSummaryMedium.ToString();

				if (txt.Length > 300) {
					txt = txt.Substring(0, 256).Trim() + "[.....]";
				}

				return new MvcHtmlString(txt);
			}
		}

		public MvcHtmlString NavigationText { get { return new MvcHtmlString(this.NavMenuText); } }
		public MvcHtmlString HeadingText { get { return new MvcHtmlString(this.PageHead); } }

		public string RequestedFileName {
			get { return HttpContext.Current.Request.ServerVariables["script_name"].ToString(); }
		}

		private List<ValidationResult> _errors = null;

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			if (_errors == null) {
				_errors = new List<ValidationResult>();
				List<string> lst = new List<string>();

				if (this.ContentType == ContentPageType.PageType.ContentEntry) {
					if (!IsUniqueFilename()) {
						ValidationResult err = new ValidationResult("Filename must be unique", new string[] { "FileName" });
						_errors.Add(err);
					}
				}

				if (this.ContentType == ContentPageType.PageType.BlogEntry) {
					if (string.IsNullOrEmpty(this.PageSlug) || string.IsNullOrEmpty(this.FileName)) {
						ValidationResult err = new ValidationResult("File Name is required", new string[] { "PageSlug" });
						_errors.Add(err);
					}

					if (!IsUniqueBlog() || !IsUniqueFilename()) {
						ValidationResult err = new ValidationResult("Filename must be unique", new string[] { "PageSlug" });
						_errors.Add(err);
					}
				}
			}

			return _errors;
		}

		public bool IsUniqueBlog() {
			DateTime dateGoLive = Convert.ToDateTime(this.GoLiveDate);
			DateTime dateOrigGoLive = DateTime.MinValue;

			string thePageSlug = ContentPageHelper.ScrubFilename(this.Root_ContentID, this.PageSlug).ToLowerInvariant();

			string theFileName = thePageSlug;

			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				ContentPage cp = pageHelper.FindContentByID(SiteData.CurrentSite.SiteID, this.Root_ContentID);

				if (cp != null) {
					dateOrigGoLive = cp.GoLiveDate;
				}
				if (cp == null && this.Root_ContentID != Guid.Empty) {
					ContentPageExport cpe = ContentImportExportUtils.GetSerializedContentPageExport(this.Root_ContentID);
					if (cpe != null) {
						dateOrigGoLive = cpe.ThePage.GoLiveDate;
					}
				}

				theFileName = ContentPageHelper.CreateFileNameFromSlug(SiteData.CurrentSite, dateGoLive, thePageSlug);

				if (SiteData.IsPageSpecial(theFileName) || SiteData.IsLikelyHomePage(theFileName)) {
					return false;
				}

				ContentPage fn1 = pageHelper.FindByFilename(SiteData.CurrentSite.SiteID, theFileName);

				if (cp == null && this.Root_ContentID != Guid.Empty) {
					cp = pageHelper.GetVersion(SiteData.CurrentSite.SiteID, this.Root_ContentID);
				}

				if (fn1 == null || (fn1 != null && cp != null && fn1.Root_ContentID == cp.Root_ContentID)) {
					return true;
				}
			}

			return false;
		}

		public bool IsUniqueFilename() {
			string theFileName = this.FileName;

			theFileName = ContentPageHelper.ScrubFilename(this.Root_ContentID, theFileName);

			theFileName = theFileName.ToLowerInvariant();

			if (SiteData.IsPageSpecial(theFileName) || SiteData.IsLikelyHomePage(theFileName)) {
				return false;
			}

			if (SiteData.CurrentSite.GetSpecialFilePathPrefixes().Where(x => theFileName.StartsWith(x.ToLowerInvariant())).Count() > 0
				|| theFileName.StartsWith(SiteData.CurrentSite.BlogFolderPath.ToLowerInvariant())) {
				return false;
			}

			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				ContentPage fn = pageHelper.FindByFilename(SiteData.CurrentSite.SiteID, theFileName);

				ContentPage cp = pageHelper.FindContentByID(SiteData.CurrentSite.SiteID, this.Root_ContentID);

				if (cp == null && this.Root_ContentID != Guid.Empty) {
					cp = pageHelper.GetVersion(SiteData.CurrentSite.SiteID, this.Root_ContentID);
				}

				if (fn == null || (fn != null && cp != null && fn.Root_ContentID == cp.Root_ContentID)) {
					return true;
				}
			}

			return false;
		}
	}
}