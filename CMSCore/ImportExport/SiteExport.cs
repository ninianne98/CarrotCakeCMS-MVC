using System;
using System.Collections.Generic;
using System.Linq;

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

	public class SiteExport {

		public enum ExportType {
			BlogData,
			ContentData,
			AllData,
		}

		public SiteExport() {
			this.CarrotCakeVersion = SiteData.CarrotCakeCMSVersion;
			this.ExportDate = DateTime.UtcNow;

			this.TheSite = new SiteData();
			this.ThePages = new List<ContentPageExport>();

			this.TheCategories = new List<ContentCategory>();
			this.TheTags = new List<ContentTag>();
			this.TheSnippets = new List<ContentSnippet>();
		}

		public SiteExport(Guid siteID) {
			SiteData s = null;
			List<ContentPageExport> pages = null;

			s = SiteData.GetSiteByID(siteID);

			pages = ContentImportExportUtils.ExportAllSiteContent(siteID);

			SetVals(s, pages);
		}

		public SiteExport(Guid siteID, ExportType exportWhat) {
			SiteData s = null;
			List<ContentPageExport> pages = new List<ContentPageExport>();

			s = SiteData.GetSiteByID(siteID);

			if (exportWhat == ExportType.AllData) {
				pages = ContentImportExportUtils.ExportAllSiteContent(siteID);
			} else {
				if (exportWhat == ExportType.ContentData) {
					List<ContentPageExport> lst = ContentImportExportUtils.ExportPages(siteID);
					pages = pages.Union(lst).ToList();
				}
				if (exportWhat == ExportType.BlogData) {
					List<ContentPageExport> lst = ContentImportExportUtils.ExportPosts(siteID);
					pages = pages.Union(lst).ToList();
				}
			}

			this.TheUsers = (from u in ExtendedUserData.GetUserList()
							 select new SiteExportUser {
								 Email = u.Email,
								 Login = u.UserName,
								 FirstName = u.FirstName,
								 LastName = u.LastName,
								 UserNickname = u.UserNickName
							 }).ToList();

			SetVals(s, pages);
		}

		public SiteExport(SiteData s, List<ContentPageExport> pages) {
			SetVals(s, pages);
		}

		private void SetVals(SiteData s, List<ContentPageExport> pages) {
			this.CarrotCakeVersion = SiteData.CarrotCakeCMSVersion;
			this.ExportDate = DateTime.UtcNow;

			this.NewSiteID = Guid.NewGuid();

			this.TheSite = s;
			this.ThePages = pages;

			if (this.TheSite == null) {
				this.TheSite = new SiteData();
				this.TheSite.SiteID = Guid.NewGuid();
			}
			if (this.ThePages == null) {
				this.ThePages = new List<ContentPageExport>();
			}

			this.OriginalSiteID = TheSite.SiteID;

			foreach (var w in ThePages) {
				w.OriginalSiteID = NewSiteID;
			}

			this.TheCategories = s.GetCategoryList();
			this.TheTags = s.GetTagList();
			this.TheSnippets = s.GetContentSnippetList();
		}

		public void LoadComments() {
			if (this.ThePages != null) {
				this.TheComments = new List<CommentExport>();
				foreach (ContentPageExport cpe in this.ThePages) {
					this.TheComments = this.TheComments.Union(CommentExport.GetPageCommentExport(cpe.OriginalRootContentID)).ToList();
				}
			}
		}

		public string CarrotCakeVersion { get; set; }

		public DateTime ExportDate { get; set; }

		public Guid NewSiteID { get; set; }

		public Guid OriginalSiteID { get; set; }

		public SiteData TheSite { get; set; }

		public List<ContentPageExport> ThePages { get; set; }

		public List<ContentPageExport> TheContentPages {
			get {
				return (from c in this.ThePages
						where c.ThePage.ContentType == ContentPageType.PageType.ContentEntry
						orderby c.ThePage.NavOrder ascending
						select c).ToList();
			}
		}

		public List<ContentPageExport> TheBlogPages {
			get {
				return (from c in this.ThePages
						where c.ThePage.ContentType == ContentPageType.PageType.BlogEntry
						orderby c.ThePage.CreateDate descending
						select c).ToList();
			}
		}

		public Guid FindImportUser(SiteExportUser u) {
			SiteExportUser usr = (from t in this.TheUsers
								  where t.Login.ToString() == u.Login.ToString()
										  || t.Email.ToString() == u.Email.ToString()
								  select t).FirstOrDefault();

			if (usr == null || (usr != null && usr.ImportUserID != Guid.Empty)) {
				return SecurityData.CurrentUserGuid;
			} else {
				return usr.ImportUserID;
			}
		}

		public List<CommentExport> TheComments { get; set; }

		public List<ContentCategory> TheCategories { get; set; }

		public List<ContentTag> TheTags { get; set; }

		public List<ContentSnippet> TheSnippets { get; set; }

		public List<SiteExportUser> TheUsers { get; set; }
	}
}