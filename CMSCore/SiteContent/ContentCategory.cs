using Carrotware.CMS.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

namespace Carrotware.CMS.Core {

	public class ContentCategory : IValidatableObject, IMetaDataLinks {

		public ContentCategory() { }

		public Guid ContentCategoryID { get; set; }
		public Guid SiteID { get; set; }

		[Display(Name = "Category")]
		[StringLength(256)]
		public string CategoryText { get; set; }

		[Display(Name = "Slug")]
		[StringLength(256)]
		public string CategorySlug { get; set; }

		[Display(Name = "URL")]
		public string CategoryURL { get; set; }

		public int? UseCount { get; set; }
		public int? PublicUseCount { get; set; }

		[Display(Name = "Public")]
		public bool IsPublic { get; set; }

		public DateTime? EditDate { get; set; }

		public IHtmlString Text { get { return new HtmlString(this.CategoryText); } }
		public string Uri { get { return this.CategoryURL; } }

		public int Count {
			get {
				if (SecurityData.IsAuthEditor) {
					return this.UseCount ?? 0;
				} else {
					return this.PublicUseCount ?? 0;
				}
			}
		}

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;

			if (obj is ContentCategory) {
				ContentCategory p = (ContentCategory)obj;
				return (this.SiteID == p.SiteID
						&& this.CategorySlug.ToLower() == p.CategorySlug.ToLower());
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return this.CategorySlug.GetHashCode() ^ this.SiteID.GetHashCode();
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			List<ValidationResult> errors = new List<ValidationResult>();
			List<string> lst = new List<string>();

			if (String.IsNullOrEmpty(this.CategorySlug)) {
				ValidationResult err = new ValidationResult("Slug is required", new string[] { "CategorySlug" });
				errors.Add(err);
			}
			if (String.IsNullOrEmpty(this.CategoryText)) {
				ValidationResult err = new ValidationResult("Text is required", new string[] { "CategoryText" });
				errors.Add(err);
			}

			if (ContentCategory.GetSimilar(SiteData.CurrentSite.SiteID, this.ContentCategoryID, this.CategorySlug) > 0) {
				ValidationResult err = new ValidationResult("Slug must be unique", new string[] { "CategorySlug" });
				errors.Add(err);
			}

			return errors;
		}

		internal ContentCategory(vw_carrot_CategoryCounted c) {
			if (c != null) {
				this.ContentCategoryID = c.ContentCategoryID;
				this.SiteID = c.SiteID;
				this.CategorySlug = c.CategorySlug;
				this.CategoryText = c.CategoryText;
				this.UseCount = c.UseCount;
				this.IsPublic = c.IsPublic;
			}
		}

		internal ContentCategory(vw_carrot_CategoryURL c) {
			if (c != null) {
				SiteData site = SiteData.GetSiteFromCache(c.SiteID);

				this.ContentCategoryID = c.ContentCategoryID;
				this.SiteID = c.SiteID;
				this.CategoryURL = c.CategoryUrl;
				this.CategoryText = c.CategoryText;
				this.UseCount = c.UseCount;
				this.PublicUseCount = c.PublicUseCount;
				this.IsPublic = c.IsPublic;

				if (c.EditDate.HasValue) {
					this.EditDate = site.ConvertUTCToSiteTime(c.EditDate.Value);
				}
			}
		}

		internal ContentCategory(carrot_ContentCategory c) {
			if (c != null) {
				this.ContentCategoryID = c.ContentCategoryID;
				this.SiteID = c.SiteID;
				this.CategorySlug = c.CategorySlug;
				this.CategoryText = c.CategoryText;
				this.IsPublic = c.IsPublic;
			}
		}

		public static ContentCategory Get(Guid CategoryID) {
			ContentCategory _item = null;
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				carrot_ContentCategory query = CompiledQueries.cqGetContentCategoryByID(_db, CategoryID);
				if (query != null) {
					_item = new ContentCategory(query);
				}
			}

			return _item;
		}

		public static ContentCategory GetByURL(Guid SiteID, string requestedURL) {
			ContentCategory _item = null;
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				vw_carrot_CategoryURL query = CompiledQueries.cqGetContentCategoryByURL(_db, SiteID, requestedURL);
				if (query != null) {
					_item = new ContentCategory(query);
				}
			}

			return _item;
		}

		public static int GetSimilar(Guid SiteID, Guid CategoryID, string categorySlug) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				IQueryable<carrot_ContentCategory> query = CompiledQueries.cqGetContentCategoryNoMatch(_db, SiteID, CategoryID, categorySlug);

				return query.Count();
			}
		}

		public static int GetSiteCount(Guid siteID) {
			int iCt = -1;

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				iCt = CompiledQueries.cqGetContentCategoryCountBySiteID(_db, siteID);
			}

			return iCt;
		}

		public static List<ContentCategory> BuildCategoryList(Guid rootContentID) {
			List<ContentCategory> _types = null;

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				IQueryable<carrot_ContentCategory> query = CompiledQueries.cqGetContentCategoryByContentID(_db, rootContentID);

				_types = (from d in query.ToList()
						  select new ContentCategory(d)).ToList();
			}

			return _types;
		}

		public void Save() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				bool bNew = false;
				carrot_ContentCategory s = CompiledQueries.cqGetContentCategoryByID(_db, this.ContentCategoryID);

				if (s == null || (s != null && s.ContentCategoryID == Guid.Empty)) {
					s = new carrot_ContentCategory();
					s.ContentCategoryID = Guid.NewGuid();
					s.SiteID = this.SiteID;
					bNew = true;
				}

				s.CategorySlug = ContentPageHelper.ScrubSlug(this.CategorySlug);
				s.CategoryText = this.CategoryText;
				s.IsPublic = this.IsPublic;

				if (bNew) {
					_db.carrot_ContentCategories.InsertOnSubmit(s);
				}

				_db.SubmitChanges();

				this.ContentCategoryID = s.ContentCategoryID;
			}
		}

		public void Delete() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				carrot_ContentCategory s = CompiledQueries.cqGetContentCategoryByID(_db, this.ContentCategoryID);

				if (s != null) {
					IQueryable<carrot_CategoryContentMapping> lst = (from m in _db.carrot_CategoryContentMappings
																	 where m.ContentCategoryID == s.ContentCategoryID
																	 select m);

					_db.carrot_CategoryContentMappings.DeleteBatch(lst);
					_db.carrot_ContentCategories.DeleteOnSubmit(s);
					_db.SubmitChanges();
				}
			}
		}
	}
}