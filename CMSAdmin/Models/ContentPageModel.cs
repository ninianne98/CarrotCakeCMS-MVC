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
* Dual licensed under the MIT or GPL Version 2 licenses.
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
						_usr = ContentPage.GetCreditUserInfo();
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
								 Value = l.ContentCategoryID.ToString().ToLower()
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
								 Value = l.ContentTagID.ToString().ToLower()
							 }).ToList();
				}

				return _tags;
			}
		}

		public string OriginalFileName { get; set; }

		public void SetPage(ContentPage page) {
			this.ContentPage = page;

			this.OriginalFileName = page.FileName;
			this.SelectedCategories = this.ContentPage.ContentCategories.Select(x => x.ContentCategoryID.ToString().ToLower()).ToList();
			this.SelectedTags = this.ContentPage.ContentTags.Select(x => x.ContentTagID.ToString().ToLower()).ToList();
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
							this.ContentPage.NavOrder = pageHelper.GetSitePageCount(this.ContentPage.SiteID, this.ContentPage.ContentType) + 1;
						} else {
							this.ContentPage.Parent_ContentID = null;
							this.ContentPage.NavOrder = SiteData.BlogSortOrderNumber;
						}

						DateTime dtSite = CMSConfigHelper.CalcNearestFiveMinTime(SiteData.CurrentSite.Now);
						this.ContentPage.GoLiveDate = dtSite;
						this.ContentPage.RetireDate = dtSite.AddYears(200);

						float iThird = (float)(this.ContentPage.NavOrder - 1) / (float)3;

						Dictionary<string, float> dictTemplates = pageHelper.GetPopularTemplateList(this.ContentPage.SiteID, this.ContentPage.ContentType);
						if (dictTemplates.Any() && dictTemplates.First().Value >= iThird) {
							try {
								this.ContentPage.TemplateFile = dictTemplates.First().Key;
							} catch { }
						}
					}
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