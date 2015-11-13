using Carrotware.CMS.Core;
using Carrotware.CMS.Security.Models;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class SiteImportWordpressModel {

		public SiteImportWordpressModel() {
			this.CreateUsers = true;
			this.MapUsers = true;
			this.FixHtmlBodies = true;
			this.HasLoaded = false;

			BuildFolderList();

			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				this.PageCount = pageHelper.GetSitePageCount(SiteData.CurrentSiteID, ContentPageType.PageType.ContentEntry);

				using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
					this.Templates = cmsHelper.Templates;

					float iThird = (float)(this.PageCount - 1) / (float)3;
					Dictionary<string, float> dictTemplates = null;

					dictTemplates = pageHelper.GetPopularTemplateList(SiteData.CurrentSiteID, ContentPageType.PageType.ContentEntry);
					if (dictTemplates.Any() && dictTemplates.First().Value >= iThird) {
						try { this.PageTemplate = dictTemplates.First().Key; } catch { }
					}

					dictTemplates = pageHelper.GetPopularTemplateList(SiteData.CurrentSiteID, ContentPageType.PageType.BlogEntry);
					if (dictTemplates.Any()) {
						try { this.PostTemplate = dictTemplates.First().Key; } catch { }
					}
				}
			}
		}

		protected void BuildFolderList() {
			List<FileData> lstFolders = new List<FileData>();

			string sRoot = HttpContext.Current.Server.MapPath("~/");

			string[] subdirs;
			try {
				subdirs = Directory.GetDirectories(sRoot);
			} catch {
				subdirs = null;
			}

			if (subdirs != null) {
				foreach (string theDir in subdirs) {
					string w = FileDataHelper.MakeWebFolderPath(theDir);
					lstFolders.Add(new FileData { FileName = w, FolderPath = w, FileDate = DateTime.Now });
				}
			}

			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith(SiteData.AdminFolderPath));
			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith("/app_code/"));
			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith("/app_data/"));
			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith("/app_start/"));
			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith("/bin/"));
			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith("/obj/"));
			lstFolders.RemoveAll(f => f.FileName.ToLower().StartsWith("/views/"));

			this.DownloadFolders = lstFolders.OrderBy(f => f.FileName).ToList();
		}

		public SiteImportWordpressModel(Guid importId)
			: this() {
			this.Site = ContentImportExportUtils.GetSerializedWPExport(importId);
			this.ImportID = importId;
		}

		private SiteNav _navHome = null;

		protected SiteNav GetHomePage(SiteData site) {
			if (_navHome == null) {
				using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
					_navHome = navHelper.FindHome(site.SiteID, false);
				}
			}
			return _navHome;
		}

		private Guid FindUser(Guid userId) {
			ExtendedUserData usr = new ExtendedUserData(userId);

			if (usr == null) {
				return SecurityData.CurrentUserGuid;
			} else {
				return userId;
			}
		}

		public bool HasLoaded { get; set; }

		public string Message { get; set; }

		private void SetMsg(string sMessage) {
			if (!String.IsNullOrEmpty(sMessage)) {
				this.HasLoaded = true;
				this.Message = sMessage;
			}
		}

		public void ImportStuff() {
			this.HasLoaded = false;
			this.Site = ContentImportExportUtils.GetSerializedWPExport(this.ImportID);

			SiteData.CurrentSite = null;

			SiteData site = SiteData.CurrentSite;

			this.Message = String.Empty;
			string sMsg = String.Empty;

			if (this.ImportSite || this.ImportPages || this.ImportPosts) {
				List<string> tags = site.GetTagList().Select(x => x.TagSlug.ToLower()).ToList();
				List<string> cats = site.GetCategoryList().Select(x => x.CategorySlug.ToLower()).ToList();

				this.Site.Tags.RemoveAll(x => tags.Contains(x.InfoKey.ToLower()));
				this.Site.Categories.RemoveAll(x => cats.Contains(x.InfoKey.ToLower()));

				sMsg += "<li>Imported Tags and Categories</li>";

				List<ContentTag> lstTag = (from l in this.Site.Tags.Distinct()
										   select new ContentTag {
											   ContentTagID = Guid.NewGuid(),
											   IsPublic = true,
											   SiteID = site.SiteID,
											   TagSlug = l.InfoKey,
											   TagText = l.InfoLabel
										   }).Distinct().ToList();

				List<ContentCategory> lstCat = (from l in this.Site.Categories.Distinct()
												select new ContentCategory {
													ContentCategoryID = Guid.NewGuid(),
													IsPublic = true,
													SiteID = site.SiteID,
													CategorySlug = l.InfoKey,
													CategoryText = l.InfoLabel
												}).Distinct().ToList();

				foreach (var v in lstTag) {
					v.Save();
				}
				foreach (var v in lstCat) {
					v.Save();
				}
			}
			SetMsg(sMsg);

			if (this.ImportSite) {
				sMsg += "<li>Updated Site Name</li>";
				site.SiteName = this.Site.SiteTitle;
				site.SiteTagline = this.Site.SiteDescription;
				site.Save();
			}
			SetMsg(sMsg);

			if (!this.MapUsers) {
				this.Site.Authors = new List<WordPressUser>();
			}

			//iterate author collection and find if in the system
			foreach (WordPressUser wpu in this.Site.Authors) {
				SecurityData sd = new SecurityData();

				ExtendedUserData usr = null;
				wpu.ImportUserID = Guid.Empty;

				//attempt to find the user in the userbase
				usr = ExtendedUserData.FindByEmail(wpu.Email);
				if (usr != null) {
					wpu.ImportUserID = usr.UserId;
				} else {
					usr = ExtendedUserData.FindByUsername(wpu.Login);
					if (usr != null) {
						wpu.ImportUserID = usr.UserId;
					}
				}

				if (this.CreateUsers) {
					if (wpu.ImportUserID == Guid.Empty) {
						ApplicationUser user = new ApplicationUser { UserName = wpu.Login, Email = wpu.Email };
						var result = sd.CreateApplicationUser(user, out usr);
						if (result.Succeeded) {
							usr = ExtendedUserData.FindByUsername(wpu.Login);
						}
						wpu.ImportUserID = usr.UserId;
					}

					if (wpu.ImportUserID != Guid.Empty) {
						ExtendedUserData ud = new ExtendedUserData(wpu.ImportUserID);
						if (!String.IsNullOrEmpty(wpu.FirstName) || !String.IsNullOrEmpty(wpu.LastName)) {
							ud.FirstName = wpu.FirstName;
							ud.LastName = wpu.LastName;
							ud.Save();
						}
					}
				}
			}

			this.Site.Comments.ForEach(r => r.ImportRootID = Guid.Empty);

			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				if (this.ImportPages) {
					sMsg += "<li>Imported Pages</li>";

					int iOrder = 0;
					SiteNav navHome = navHelper.FindHome(site.SiteID, false);
					if (navHome != null) {
						iOrder = 2;
					}

					foreach (var wpp in (from c in this.Site.Content
										 where c.PostType == WordPressPost.WPPostType.Page
										 orderby c.PostOrder, c.PostTitle
										 select c).ToList()) {
						GrabAttachments(wpp);
						RepairBody(wpp);

						ContentPage cp = ContentImportExportUtils.CreateWPContentPage(this.Site, wpp, site);
						cp.SiteID = site.SiteID;
						cp.ContentType = ContentPageType.PageType.ContentEntry;
						cp.EditDate = SiteData.CurrentSite.Now;
						cp.NavOrder = iOrder;
						cp.TemplateFile = this.PageTemplate;

						WordPressPost parent = (from c in this.Site.Content
												where c.PostType == WordPressPost.WPPostType.Page
												  && c.PostID == wpp.ParentPostID
												select c).FirstOrDefault();

						SiteNav navParent = null;

						SiteNav navData = navHelper.GetLatestVersion(site.SiteID, false, cp.FileName.ToLower());
						if (parent != null) {
							navParent = navHelper.GetLatestVersion(site.SiteID, false, parent.ImportFileName.ToLower());
						}

						//if URL exists already, make this become a new version in the current series
						if (navData != null) {
							cp.Root_ContentID = navData.Root_ContentID;
							if (navData.NavOrder == 0) {
								cp.NavOrder = 0;
							}
						}

						if (navParent != null) {
							cp.Parent_ContentID = navParent.Root_ContentID;
						} else {
							if (parent != null) {
								cp.Parent_ContentID = parent.ImportRootID;
							}
						}
						//preserve homepage
						if (navHome != null && navHome.FileName.ToLower() == cp.FileName.ToLower()) {
							cp.NavOrder = 0;
						}

						cp.RetireDate = CMSConfigHelper.CalcNearestFiveMinTime(cp.CreateDate).AddYears(200);
						cp.GoLiveDate = CMSConfigHelper.CalcNearestFiveMinTime(cp.CreateDate).AddMinutes(-5);

						//if URL exists already, make this become a new version in the current series
						if (navData != null) {
							cp.Root_ContentID = navData.Root_ContentID;
							cp.RetireDate = navData.RetireDate;
							cp.GoLiveDate = navData.GoLiveDate;
						}

						cp.SavePageEdit();

						this.Site.Comments.Where(x => x.PostID == wpp.PostID).ToList().ForEach(r => r.ImportRootID = cp.Root_ContentID);

						iOrder++;
					}
				}

				if (this.ImportPosts) {
					sMsg += "<li>Imported Posts</li>";

					foreach (var wpp in (from c in this.Site.Content
										 where c.PostType == WordPressPost.WPPostType.BlogPost
										 orderby c.PostOrder
										 select c).ToList()) {
						GrabAttachments(wpp);
						RepairBody(wpp);

						ContentPage cp = ContentImportExportUtils.CreateWPContentPage(this.Site, wpp, site);
						cp.SiteID = site.SiteID;
						cp.Parent_ContentID = null;
						cp.ContentType = ContentPageType.PageType.BlogEntry;
						cp.EditDate = SiteData.CurrentSite.Now;
						cp.NavOrder = SiteData.BlogSortOrderNumber;
						cp.TemplateFile = this.PostTemplate;

						SiteNav navData = navHelper.GetLatestVersion(site.SiteID, false, cp.FileName.ToLower());

						cp.RetireDate = CMSConfigHelper.CalcNearestFiveMinTime(cp.CreateDate).AddYears(200);
						cp.GoLiveDate = CMSConfigHelper.CalcNearestFiveMinTime(cp.CreateDate).AddMinutes(-5);

						//if URL exists already, make this become a new version in the current series
						if (navData != null) {
							cp.Root_ContentID = navData.Root_ContentID;
							cp.RetireDate = navData.RetireDate;
							cp.GoLiveDate = navData.GoLiveDate;
						}

						cp.SavePageEdit();

						this.Site.Comments.Where(x => x.PostID == wpp.PostID).ToList().ForEach(r => r.ImportRootID = cp.Root_ContentID);
					}

					using (ContentPageHelper cph = new ContentPageHelper()) {
						//cph.BulkBlogFileNameUpdateFromDate(site.SiteID);
						cph.ResolveDuplicateBlogURLs(site.SiteID);
						cph.FixBlogNavOrder(site.SiteID);
					}
				}
			}
			SetMsg(sMsg);

			this.Site.Comments.RemoveAll(r => r.ImportRootID == Guid.Empty);

			if (this.Site.Comments.Any()) {
				sMsg += "<li>Imported Comments</li>";
			}

			foreach (WordPressComment wpc in this.Site.Comments) {
				int iCommentCount = -1;

				iCommentCount = PostComment.GetCommentCountByContent(site.SiteID, wpc.ImportRootID, wpc.CommentDateUTC, wpc.AuthorIP, wpc.CommentContent);
				if (iCommentCount < 1) {
					iCommentCount = PostComment.GetCommentCountByContent(site.SiteID, wpc.ImportRootID, wpc.CommentDateUTC, wpc.AuthorIP);
				}

				if (iCommentCount < 1) {
					PostComment pc = new PostComment();
					pc.ContentCommentID = Guid.NewGuid();
					pc.Root_ContentID = wpc.ImportRootID;
					pc.CreateDate = site.ConvertUTCToSiteTime(wpc.CommentDateUTC);
					pc.IsApproved = false;
					pc.IsSpam = false;

					pc.CommenterIP = wpc.AuthorIP;
					pc.CommenterName = wpc.Author;
					pc.CommenterEmail = wpc.AuthorEmail;
					pc.PostCommentText = wpc.CommentContent;
					pc.CommenterURL = wpc.AuthorURL;

					if (wpc.Approved == "1") {
						pc.IsApproved = true;
					}
					if (wpc.Approved.ToLower() == "trash") {
						pc.IsSpam = true;
					}
					if (wpc.Type.ToLower() == "trackback" || wpc.Type.ToLower() == "pingback") {
						pc.CommenterEmail = wpc.Type;
					}

					pc.Save();
				}
			}
			SetMsg(sMsg);
		}

		protected void RepairBody(WordPressPost wpp) {
			wpp.CleanBody();

			if (this.FixHtmlBodies) {
				wpp.RepairBody();
			}
		}

		protected void GrabAttachments(WordPressPost wpPage) {
			if (this.DownloadImages) {
				wpPage.GrabAttachments(this.SelectedFolder, this.Site);
			}
		}

		public bool ImportPages { get; set; }
		public bool ImportPosts { get; set; }
		public bool ImportSite { get; set; }

		public bool DownloadImages { get; set; }
		public bool FixHtmlBodies { get; set; }
		public bool CreateUsers { get; set; }
		public bool MapUsers { get; set; }

		public int PageCount { get; set; }

		public string PageTemplate { get; set; }

		public string PostTemplate { get; set; }

		public string SelectedFolder { get; set; }

		public Guid ImportID { get; set; }

		public WordPressSite Site { get; set; }

		public List<FileData> DownloadFolders { get; set; }

		public List<CMSTemplate> Templates { get; set; }
	}
}