using Carrotware.CMS.Core;
using Carrotware.CMS.Security.Models;
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

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class SiteImportNativeModel {

		public SiteImportNativeModel() {
			this.CreateUsers = true;
			this.MapUsers = true;
			this.HasLoaded = false;

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

		public SiteImportNativeModel(Guid importId)
			: this() {
			this.Site = ContentImportExportUtils.GetSerializedSiteExport(importId);
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
			this.Site = ContentImportExportUtils.GetSerializedSiteExport(this.ImportID);

			SiteData.CurrentSite = null;

			SiteData site = SiteData.CurrentSite;

			this.Message = String.Empty;
			string sMsg = String.Empty;

			if (this.ImportSite || this.ImportPages || this.ImportPosts) {
				List<string> tags = site.GetTagList().Select(x => x.TagSlug.ToLowerInvariant()).ToList();
				List<string> cats = site.GetCategoryList().Select(x => x.CategorySlug.ToLowerInvariant()).ToList();

				this.Site.TheTags.RemoveAll(x => tags.Contains(x.TagSlug.ToLowerInvariant()));
				this.Site.TheCategories.RemoveAll(x => cats.Contains(x.CategorySlug.ToLowerInvariant()));

				sMsg += "<li>Imported Tags and Categories</li>";

				List<ContentTag> lstTag = (from l in this.Site.TheTags.Distinct()
										   select new ContentTag {
											   ContentTagID = Guid.NewGuid(),
											   SiteID = site.SiteID,
											   IsPublic = l.IsPublic,
											   TagSlug = l.TagSlug,
											   TagText = l.TagText
										   }).ToList();

				List<ContentCategory> lstCat = (from l in this.Site.TheCategories.Distinct()
												select new ContentCategory {
													ContentCategoryID = Guid.NewGuid(),
													SiteID = site.SiteID,
													IsPublic = l.IsPublic,
													CategorySlug = l.CategorySlug,
													CategoryText = l.CategoryText
												}).ToList();

				foreach (var v in lstTag) {
					v.Save();
				}
				foreach (var v in lstCat) {
					v.Save();
				}
			}
			SetMsg(sMsg);

			if (this.ImportSnippets) {
				List<string> snippets = site.GetContentSnippetList().Select(x => x.ContentSnippetSlug.ToLowerInvariant()).ToList();

				this.Site.TheSnippets.RemoveAll(x => snippets.Contains(x.ContentSnippetSlug.ToLowerInvariant()));

				sMsg += "<li>Imported Content Snippets</li>";

				List<ContentSnippet> lstSnip = (from l in this.Site.TheSnippets.Distinct()
												select new ContentSnippet {
													SiteID = site.SiteID,
													Root_ContentSnippetID = Guid.NewGuid(),
													ContentSnippetID = Guid.NewGuid(),
													CreateUserId = SecurityData.CurrentUserGuid,
													CreateDate = site.Now,
													EditUserId = SecurityData.CurrentUserGuid,
													EditDate = site.Now,
													RetireDate = l.RetireDate,
													GoLiveDate = l.GoLiveDate,
													ContentSnippetActive = l.ContentSnippetActive,
													ContentBody = l.ContentBody,
													ContentSnippetSlug = l.ContentSnippetSlug,
													ContentSnippetName = l.ContentSnippetName
												}).ToList();

				foreach (var v in lstSnip) {
					v.Save();
				}
			}
			SetMsg(sMsg);

			if (this.ImportSite) {
				sMsg += "<li>Updated Site Name</li>";
				site.SiteName = this.Site.TheSite.SiteName;
				site.SiteTagline = this.Site.TheSite.SiteTagline;
				site.BlockIndex = this.Site.TheSite.BlockIndex;
				site.Save();
			}
			SetMsg(sMsg);

			if (!this.MapUsers) {
				this.Site.TheUsers = new List<SiteExportUser>();
			}

			//iterate author collection and find if in the system
			foreach (SiteExportUser seu in this.Site.TheUsers) {
				SecurityData sd = new SecurityData();

				ExtendedUserData usr = null;
				seu.ImportUserID = Guid.Empty;

				//attempt to find the user in the userbase
				usr = ExtendedUserData.FindByEmail(seu.Email);
				if (usr != null && usr.UserId != Guid.Empty) {
					seu.ImportUserID = usr.UserId;
				} else {
					usr = ExtendedUserData.FindByUsername(seu.Login);
					if (usr != null && usr.UserId != Guid.Empty) {
						seu.ImportUserID = usr.UserId;
					}
				}

				if (this.CreateUsers) {
					if (seu.ImportUserID == Guid.Empty) {
						ApplicationUser user = new ApplicationUser { UserName = seu.Login, Email = seu.Email };
						var result = sd.CreateApplicationUser(user, out usr);
						if (result.Succeeded) {
							usr = ExtendedUserData.FindByUsername(seu.Login);
						} else {
							throw new Exception(String.Format("Could not create user: {0} ({1}) \r\n{2}", seu.Login, seu.Email, String.Join("\r\n", result.Errors)));
						}
						seu.ImportUserID = usr.UserId;
					}

					if (seu.ImportUserID != Guid.Empty) {
						ExtendedUserData ud = new ExtendedUserData(seu.ImportUserID);
						if (!String.IsNullOrEmpty(seu.FirstName) || !String.IsNullOrEmpty(seu.LastName)) {
							ud.FirstName = seu.FirstName;
							ud.LastName = seu.LastName;
							ud.Save();
						}
					}
				}
			}

			if (this.ImportPages) {
				sMsg += "<li>Imported Pages</li>";
				this.Content = site.GetFullSiteFileList();

				int iOrder = 0;

				SiteNav navHome = GetHomePage(site);

				if (navHome != null) {
					iOrder = 2;
				}

				foreach (var impCP in (from c in this.Site.ThePages
									   where c.ThePage.ContentType == ContentPageType.PageType.ContentEntry
									   orderby c.ThePage.NavOrder, c.ThePage.NavMenuText
									   select c).ToList()) {
					ContentPage cp = impCP.ThePage;
					cp.Root_ContentID = impCP.NewRootContentID;
					cp.ContentID = Guid.NewGuid();
					cp.SiteID = site.SiteID;
					cp.ContentType = ContentPageType.PageType.ContentEntry;
					cp.EditDate = SiteData.CurrentSite.Now;
					cp.EditUserId = this.Site.FindImportUser(impCP.TheUser);
					cp.CreateUserId = this.Site.FindImportUser(impCP.TheUser);
					if (impCP.CreditUser != null) {
						cp.CreditUserId = this.Site.FindImportUser(impCP.CreditUser);
					}
					cp.NavOrder = iOrder;
					cp.TemplateFile = this.PageTemplate;

					ContentPageExport parent = (from c in this.Site.ThePages
												where c.ThePage.ContentType == ContentPageType.PageType.ContentEntry
												  && c.ThePage.FileName.ToLowerInvariant() == impCP.ParentFileName.ToLowerInvariant()
												select c).FirstOrDefault();

					BasicContentData navParent = null;
					BasicContentData navData = GetFileInfoFromList(site, cp.FileName);

					if (parent != null) {
						cp.Parent_ContentID = parent.NewRootContentID;
						navParent = GetFileInfoFromList(site, parent.ThePage.FileName);
					}

					//if URL exists already, make this become a new version in the current series
					if (navData != null) {
						cp.Root_ContentID = navData.Root_ContentID;

						impCP.ThePage.RetireDate = navData.RetireDate;
						impCP.ThePage.GoLiveDate = navData.GoLiveDate;

						if (navData.NavOrder == 0) {
							cp.NavOrder = 0;
						}
					}
					//preserve homepage
					if (navHome != null && navHome.FileName.ToLowerInvariant() == cp.FileName.ToLowerInvariant()) {
						cp.NavOrder = 0;
					}
					//if the file url in the upload has an existing ID, use that, not the ID from the queue
					if (navParent != null) {
						cp.Parent_ContentID = navParent.Root_ContentID;
					}

					cp.RetireDate = impCP.ThePage.RetireDate;
					cp.GoLiveDate = impCP.ThePage.GoLiveDate;

					cp.SavePageEdit();

					iOrder++;
				}
			}
			SetMsg(sMsg);

			if (this.ImportPosts) {
				sMsg += "<li>Imported Posts</li>";
				this.Content = site.GetFullSiteFileList();

				List<ContentTag> lstTags = site.GetTagList();
				List<ContentCategory> lstCategories = site.GetCategoryList();

				foreach (var impCP in (from c in this.Site.ThePages
									   where c.ThePage.ContentType == ContentPageType.PageType.BlogEntry
									   orderby c.ThePage.CreateDate
									   select c).ToList()) {
					ContentPage cp = impCP.ThePage;
					cp.Root_ContentID = impCP.NewRootContentID;
					cp.ContentID = Guid.NewGuid();
					cp.SiteID = site.SiteID;
					cp.Parent_ContentID = null;
					cp.ContentType = ContentPageType.PageType.BlogEntry;
					cp.EditDate = SiteData.CurrentSite.Now;
					cp.EditUserId = this.Site.FindImportUser(impCP.TheUser);
					cp.CreateUserId = this.Site.FindImportUser(impCP.TheUser);
					if (impCP.CreditUser != null) {
						cp.CreditUserId = this.Site.FindImportUser(impCP.CreditUser);
					}
					cp.NavOrder = SiteData.BlogSortOrderNumber;
					cp.TemplateFile = this.PostTemplate;

					cp.ContentCategories = (from l in lstCategories
											join o in impCP.ThePage.ContentCategories on l.CategorySlug.ToLowerInvariant() equals o.CategorySlug.ToLowerInvariant()
											select l).Distinct().ToList();

					cp.ContentTags = (from l in lstTags
									  join o in impCP.ThePage.ContentTags on l.TagSlug.ToLowerInvariant() equals o.TagSlug.ToLowerInvariant()
									  select l).Distinct().ToList();

					BasicContentData navData = GetFileInfoFromList(site, cp.FileName);

					//if URL exists already, make this become a new version in the current series
					if (navData != null) {
						cp.Root_ContentID = navData.Root_ContentID;

						impCP.ThePage.RetireDate = navData.RetireDate;
						impCP.ThePage.GoLiveDate = navData.GoLiveDate;
					}

					cp.RetireDate = impCP.ThePage.RetireDate;
					cp.GoLiveDate = impCP.ThePage.GoLiveDate;

					cp.SavePageEdit();
				}

				using (ContentPageHelper cph = new ContentPageHelper()) {
					//cph.BulkBlogFileNameUpdateFromDate(site.SiteID);
					cph.ResolveDuplicateBlogURLs(site.SiteID);
					cph.FixBlogNavOrder(site.SiteID);
				}
			}
			SetMsg(sMsg);

			if (this.ImportComments) {
				sMsg += "<li>Imported Comments</li>";
				this.Content = site.GetFullSiteFileList();

				foreach (var impCP in (from c in this.Site.TheComments
									   orderby c.TheComment.CreateDate
									   select c).ToList()) {
					int iCommentCount = -1;
					PostComment pc = impCP.TheComment;
					BasicContentData navData = GetFileInfoFromList(site, pc.FileName);
					if (navData != null) {
						pc.Root_ContentID = navData.Root_ContentID;
						pc.ContentCommentID = Guid.NewGuid();

						iCommentCount = PostComment.GetCommentCountByContent(site.SiteID, pc.Root_ContentID, pc.CreateDate, pc.CommenterIP, pc.PostCommentText);
						if (iCommentCount < 1) {
							iCommentCount = PostComment.GetCommentCountByContent(site.SiteID, pc.Root_ContentID, pc.CreateDate, pc.CommenterIP);
						}

						if (iCommentCount < 1) {
							pc.Save();
						}
					}
				}
			}

			SetMsg(sMsg);
		}

		private int iAccessCounter = 0;

		protected BasicContentData GetFileInfoFromList(SiteData site, string sFilename) {
			if (this.Content == null || !this.Content.Any() || iAccessCounter % 25 == 0) {
				this.Content = site.GetFullSiteFileList();
				iAccessCounter = 0;
			}
			iAccessCounter++;

			BasicContentData pageData = (from m in this.Content
										 where m.FileName.ToLowerInvariant() == sFilename.ToLowerInvariant()
										 select m).FirstOrDefault();

			if (pageData == null) {
				using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
					pageData = BasicContentData.CreateBasicContentDataFromSiteNav(navHelper.GetLatestVersion(site.SiteID, false, sFilename.ToLowerInvariant()));
				}
			}

			return pageData;
		}

		public bool ImportPages { get; set; }
		public bool ImportPosts { get; set; }
		public bool ImportSite { get; set; }
		public bool ImportComments { get; set; }
		public bool ImportSnippets { get; set; }
		public bool CreateUsers { get; set; }
		public bool MapUsers { get; set; }

		public int PageCount { get; set; }

		public string PageTemplate { get; set; }

		public string PostTemplate { get; set; }

		public Guid ImportID { get; set; }

		public SiteExport Site { get; set; }

		public List<BasicContentData> Content { get; set; }

		public List<CMSTemplate> Templates { get; set; }
	}
}