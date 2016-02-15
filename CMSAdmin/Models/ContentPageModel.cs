using Carrotware.CMS.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class ContentPageModel : IValidatableObject {

		public ContentPageModel() {
			this.InitSelections();
			this.Mode = "html";
			this.VisitPage = false;

			this.VersionHistory = new Dictionary<string, string>();
			this.WidgetListHtml = new List<Widget>();
			this.WidgetListText = new List<Widget>();

			using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
				this.SiteTemplates = cmsHelper.Templates;
			}
		}

		public ContentPage ContentPage { get; set; }

		public Guid? ImportID { get; set; }
		public Guid? VersionID { get; set; }
		public Guid? ParentID { get; set; }
		public string Mode { get; set; }
		public bool VisitPage { get; set; }

		public Dictionary<string, string> VersionHistory { get; set; }

		public List<CMSTemplate> SiteTemplates { get; set; }

		private ExtendedUserData _usr = null;

		public ExtendedUserData CreditUser {
			get {
				if (this.ContentPage.CreditUserId.HasValue) {
					if (_usr == null) {
						_usr = this.ContentPage.GetCreditUserInfo();
					}
				} else {
					_usr = null;
				}
				return _usr;
			}

			set {
				_usr = value;

				this.ContentPage.CreditUserId = null;
				if (_usr != null) {
					this.ContentPage.CreditUserId = _usr.UserId;
				}
			}
		}

		private List<SelectListItem> _cats = null;

		public List<string> SelectedCategories { get; set; }
		public List<string> SelectedTags { get; set; }

		protected void InitSelections() {
			if (this.SelectedCategories == null) {
				this.SelectedCategories = new List<string>();
			}
			if (this.SelectedTags == null) {
				this.SelectedTags = new List<string>();
			}
		}

		public List<SelectListItem> CategoryOptions {
			get {
				if (_cats == null) {
					_cats = (from l in SiteData.CurrentSite.GetCategoryList()
							 select new SelectListItem {
								 Text = l.CategoryText,
								 Selected = this.ContentPage.ContentCategories.Where(x => x.ContentCategoryID == l.ContentCategoryID).Any(),
								 Value = l.ContentCategoryID.ToString().ToLowerInvariant()
							 }).ToList();
				}

				return _cats;
			}
		}

		private List<SelectListItem> _tags = null;

		public List<SelectListItem> TagOptions {
			get {
				if (_tags == null) {
					_tags = (from l in SiteData.CurrentSite.GetTagList()
							 select new SelectListItem {
								 Text = l.TagText,
								 Selected = this.ContentPage.ContentTags.Where(x => x.ContentTagID == l.ContentTagID).Any(),
								 Value = l.ContentTagID.ToString().ToLowerInvariant()
							 }).ToList();
				}

				return _tags;
			}
		}

		public string OriginalFileName { get; set; }

		public List<Widget> WidgetListText { get; set; }

		public List<Widget> WidgetListHtml { get; set; }

		public void SetPage(ContentPage page) {
			this.ContentPage = page;

			this.OriginalFileName = page.FileName;
			this.SelectedCategories = this.ContentPage.ContentCategories.Select(x => x.ContentCategoryID.ToString().ToLowerInvariant()).ToList();
			this.SelectedTags = this.ContentPage.ContentTags.Select(x => x.ContentTagID.ToString().ToLowerInvariant()).ToList();
			this.InitSelections();

			if (this.ContentPage != null) {
				using (ContentPageHelper pageHelper = new ContentPageHelper()) {
					if (this.ContentPage.Root_ContentID != Guid.Empty) {
						this.VersionHistory = (from v in pageHelper.GetVersionHistory(this.ContentPage.SiteID, this.ContentPage.Root_ContentID)
											   join u in ExtendedUserData.GetUserList() on v.EditUserId equals u.UserId
											   orderby v.EditDate descending
											   select new KeyValuePair<string, string>(v.ContentID.ToString(),
																		String.Format("{0} ({1}) {2}", v.EditDate, u.UserName, (v.IsLatestVersion ? " [**] " : " ")))
																		).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
					} else {
						this.ContentPage.CreateDate = DateTime.UtcNow.Date;
						this.ContentPage.EditDate = DateTime.UtcNow.Date;
						this.ContentPage.ContentID = Guid.NewGuid();

						if (this.ContentPage.ContentType == ContentPageType.PageType.ContentEntry) {
							this.ContentPage.NavOrder = pageHelper.GetSitePageCount(this.ContentPage.SiteID, this.ContentPage.ContentType) * 2;
						} else {
							this.ContentPage.Parent_ContentID = null;
							this.ContentPage.NavOrder = SiteData.BlogSortOrderNumber;
						}

						DateTime dtSite = CMSConfigHelper.CalcNearestFiveMinTime(SiteData.CurrentSite.Now);
						this.ContentPage.GoLiveDate = dtSite;
						this.ContentPage.RetireDate = dtSite.AddYears(200);

						float iThird = (float)(this.ContentPage.NavOrder - 1) / (float)3;

						Dictionary<string, float> dictTemplates = pageHelper.GetPopularTemplateList(this.ContentPage.SiteID, this.ContentPage.ContentType);

						if (dictTemplates.Any()) {
							try {
								this.ContentPage.TemplateFile = dictTemplates.First().Key;

								if (dictTemplates.First().Value >= iThird) {
									this.ContentPage.TemplateFile = dictTemplates.First().Key;
								}
							} catch { }
						}
					}
				}

				RefreshWidgetList();
			}
		}

		protected void OverrideCache(ContentPage pageContents) {
			using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
				cmsHelper.OverrideKey(pageContents.Root_ContentID);
				cmsHelper.cmsAdminContent = pageContents;
				cmsHelper.cmsAdminWidget = pageContents.GetWidgetList();
			}
		}

		public void RefreshWidgetList() {
			this.WidgetListHtml = new List<Widget>();
			this.WidgetListText = new List<Widget>();

			if (this.ContentPage != null && this.ContentPage.Root_ContentID != Guid.Empty) {
				using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
					cmsHelper.OverrideKey(this.ContentPage.Root_ContentID);

					this.WidgetListHtml = (from w in cmsHelper.cmsAdminWidget
										   where w.IsLatestVersion == true
										   && w.ControlPath.StartsWith("CLASS:Carrotware.CMS.UI.Components.ContentRichText,")
										   select w).ToList();

					this.WidgetListText = (from w in cmsHelper.cmsAdminWidget
										   where w.IsLatestVersion == true
										   && w.ControlPath.StartsWith("CLASS:Carrotware.CMS.UI.Components.ContentPlainText,")
										   select w).ToList();
				}
			}
		}

		public ContentPage GetPost(Guid? id, Guid? versionid, Guid? importid, string mode) {
			ContentPage pageContents = null;
			this.ImportID = importid;
			this.VersionID = versionid;
			this.Mode = (String.IsNullOrEmpty(mode) || mode.Trim().ToLowerInvariant() != "raw") ? "html" : "raw";

			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				if (!id.HasValue && !versionid.HasValue && !importid.HasValue) {
					if (pageContents == null) {
						pageContents = new ContentPage(SiteData.CurrentSiteID, ContentPageType.PageType.BlogEntry);
					}

					pageContents.Root_ContentID = Guid.Empty;
				} else {
					if (importid.HasValue) {
						ContentPageExport cpe = ContentImportExportUtils.GetSerializedContentPageExport(importid.Value);
						if (cpe != null) {
							pageContents = cpe.ThePage;
							pageContents.EditDate = SiteData.CurrentSite.Now;

							var rp = pageHelper.GetLatestContentByURL(SiteData.CurrentSiteID, false, pageContents.FileName);
							if (rp != null) {
								pageContents.Root_ContentID = rp.Root_ContentID;
								pageContents.ContentID = rp.ContentID;
							} else {
								pageContents.Root_ContentID = Guid.Empty;
								pageContents.ContentID = Guid.Empty;
							}
							pageContents.Parent_ContentID = null;
							pageContents.NavOrder = SiteData.BlogSortOrderNumber;
						}
					}
					if (versionid.HasValue) {
						pageContents = pageHelper.GetVersion(SiteData.CurrentSiteID, versionid.Value);
					}
					if (id.HasValue && pageContents == null) {
						pageContents = pageHelper.FindContentByID(SiteData.CurrentSiteID, id.Value);
					}
				}
			}

			OverrideCache(pageContents);

			SetPage(pageContents);

			return pageContents;
		}

		public ContentPage SavePost() {
			ContentPage page = this.ContentPage;
			ContentPage pageContents = null;

			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				pageContents = pageHelper.FindContentByID(SiteData.CurrentSiteID, page.Root_ContentID);
			}
			if (pageContents == null) {
				pageContents = new ContentPage(SiteData.CurrentSiteID, ContentPageType.PageType.BlogEntry);
			}

			pageContents.GoLiveDate = page.GoLiveDate;
			pageContents.RetireDate = page.RetireDate;

			pageContents.IsLatestVersion = true;
			pageContents.Thumbnail = page.Thumbnail;

			pageContents.TemplateFile = page.TemplateFile;

			pageContents.TitleBar = page.TitleBar;
			pageContents.NavMenuText = page.NavMenuText;
			pageContents.PageHead = page.PageHead;
			pageContents.FileName = page.FileName;
			pageContents.PageSlug = page.PageSlug;

			pageContents.MetaDescription = page.MetaDescription;
			pageContents.MetaKeyword = page.MetaKeyword;

			pageContents.EditDate = SiteData.CurrentSite.Now;
			pageContents.NavOrder = SiteData.BlogSortOrderNumber;

			pageContents.PageText = page.PageText;
			pageContents.LeftPageText = page.LeftPageText;
			pageContents.RightPageText = page.RightPageText;

			pageContents.PageActive = page.PageActive;
			pageContents.ShowInSiteNav = false;
			pageContents.ShowInSiteMap = false;
			pageContents.BlockIndex = page.BlockIndex;

			pageContents.Parent_ContentID = page.Parent_ContentID;

			pageContents.CreditUserId = page.CreditUserId;

			pageContents.EditUserId = SecurityData.CurrentUserGuid;

			List<ContentCategory> lstCat = (from l in SiteData.CurrentSite.GetCategoryList()
											join cr in this.SelectedCategories on l.ContentCategoryID.ToString().ToLowerInvariant() equals cr.ToLowerInvariant()
											select l).ToList();
			List<ContentTag> lstTag = (from l in SiteData.CurrentSite.GetTagList()
									   join cr in this.SelectedTags on l.ContentTagID.ToString().ToLowerInvariant() equals cr.ToLowerInvariant()
									   select l).ToList();

			pageContents.ContentCategories = lstCat;
			pageContents.ContentTags = lstTag;

			pageContents.SavePageEdit();
			SaveTextWidgets();

			return pageContents;
		}

		public ContentPage GetPage(Guid? id, Guid? versionid, Guid? importid, string mode) {
			ContentPage pageContents = null;
			this.ImportID = importid;
			this.VersionID = versionid;
			this.Mode = (String.IsNullOrEmpty(mode) || mode.Trim().ToLowerInvariant() != "raw") ? "html" : "raw";

			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				if (!id.HasValue && !versionid.HasValue && !importid.HasValue) {
					if (pageContents == null) {
						pageContents = new ContentPage(SiteData.CurrentSiteID, ContentPageType.PageType.ContentEntry);
					}

					pageContents.Root_ContentID = Guid.Empty;
				} else {
					if (importid.HasValue) {
						ContentPageExport cpe = ContentImportExportUtils.GetSerializedContentPageExport(importid.Value);
						if (cpe != null) {
							pageContents = cpe.ThePage;
							pageContents.EditDate = SiteData.CurrentSite.Now;
							pageContents.Parent_ContentID = null;
							var rp = pageHelper.GetLatestContentByURL(SiteData.CurrentSiteID, false, pageContents.FileName);
							if (rp != null) {
								pageContents.Root_ContentID = rp.Root_ContentID;
								pageContents.ContentID = rp.ContentID;
								pageContents.Parent_ContentID = rp.Parent_ContentID;
								pageContents.NavOrder = rp.NavOrder;
							} else {
								pageContents.Root_ContentID = Guid.Empty;
								pageContents.ContentID = Guid.Empty;
								pageContents.NavOrder = pageHelper.GetSitePageCount(SiteData.CurrentSiteID, ContentPageType.PageType.ContentEntry);
							}
						}
					}
					if (versionid.HasValue) {
						pageContents = pageHelper.GetVersion(SiteData.CurrentSiteID, versionid.Value);
					}
					if (id.HasValue && pageContents == null) {
						pageContents = pageHelper.FindContentByID(SiteData.CurrentSiteID, id.Value);
					}
				}
			}

			OverrideCache(pageContents);

			SetPage(pageContents);

			return pageContents;
		}

		public ContentPage SavePage() {
			ContentPage page = this.ContentPage;
			ContentPage pageContents = null;

			using (ContentPageHelper pageHelper = new ContentPageHelper()) {
				pageContents = pageHelper.FindContentByID(SiteData.CurrentSiteID, page.Root_ContentID);
			}

			if (pageContents == null) {
				pageContents = new ContentPage(SiteData.CurrentSiteID, ContentPageType.PageType.ContentEntry);
			}

			pageContents.GoLiveDate = page.GoLiveDate;
			pageContents.RetireDate = page.RetireDate;

			pageContents.IsLatestVersion = true;
			pageContents.Thumbnail = page.Thumbnail;

			pageContents.TemplateFile = page.TemplateFile;

			pageContents.TitleBar = page.TitleBar;
			pageContents.NavMenuText = page.NavMenuText;
			pageContents.PageHead = page.PageHead;
			pageContents.FileName = page.FileName;
			pageContents.PageSlug = null;

			pageContents.MetaDescription = page.MetaDescription;
			pageContents.MetaKeyword = page.MetaKeyword;

			pageContents.EditDate = SiteData.CurrentSite.Now;
			pageContents.NavOrder = page.NavOrder;

			pageContents.PageText = page.PageText;
			pageContents.LeftPageText = page.LeftPageText;
			pageContents.RightPageText = page.RightPageText;

			pageContents.PageActive = page.PageActive;
			pageContents.ShowInSiteNav = page.ShowInSiteNav;
			pageContents.ShowInSiteMap = page.ShowInSiteMap;
			pageContents.BlockIndex = page.BlockIndex;

			pageContents.Parent_ContentID = page.Parent_ContentID;

			pageContents.CreditUserId = page.CreditUserId;

			pageContents.EditUserId = SecurityData.CurrentUserGuid;

			pageContents.SavePageEdit();

			SaveTextWidgets();

			return pageContents;
		}

		public void SaveTextWidgets() {
			if (this.ContentPage != null && this.ContentPage.Root_ContentID != Guid.Empty) {
				using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
					cmsHelper.OverrideKey(this.ContentPage.Root_ContentID);

					if (cmsHelper.cmsAdminWidget != null) {
						var ww = (from w in cmsHelper.cmsAdminWidget
								  where w.IsLatestVersion == true
								  && w.IsPendingChange == true
								  && (w.ControlPath.StartsWith("CLASS:Carrotware.CMS.UI.Components.ContentRichText,")
										|| w.ControlPath.StartsWith("CLASS:Carrotware.CMS.UI.Components.ContentPlainText,"))
								  select w);

						foreach (Widget w in ww) {
							w.Save();
						}
					}

					cmsHelper.cmsAdminContent = null;
					cmsHelper.cmsAdminWidget = null;
				}
			}
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			List<ValidationResult> errors = new List<ValidationResult>();
			IEnumerable<ValidationResult> oldErrors = this.ContentPage.Validate(validationContext);

			foreach (ValidationResult s in oldErrors) {
				List<string> mbrs = s.MemberNames.ToList().Select(m => String.Format("ContentPage.{0}", m)).ToList();

				errors.Add(new ValidationResult(s.ErrorMessage, mbrs));
			}

			return errors;
		}
	}
}