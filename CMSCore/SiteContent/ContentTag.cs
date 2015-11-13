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

	public class ContentTag : IValidatableObject, IMetaDataLinks {

		public ContentTag() { }

		public Guid ContentTagID { get; set; }
		public Guid SiteID { get; set; }

		[Display(Name = "Tag")]
		[StringLength(256)]
		public string TagText { get; set; }

		[Display(Name = "Slug")]
		[StringLength(256)]
		public string TagSlug { get; set; }

		[Display(Name = "URL")]
		public string TagURL { get; set; }

		public int? UseCount { get; set; }
		public int? PublicUseCount { get; set; }

		[Display(Name = "Public")]
		public bool IsPublic { get; set; }

		public DateTime? EditDate { get; set; }

		public IHtmlString Text { get { return new HtmlString(this.TagText); } }
		public string Uri { get { return this.TagURL; } }

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

			if (obj is ContentTag) {
				ContentTag p = (ContentTag)obj;
				return (this.SiteID == p.SiteID
						&& this.TagSlug.ToLower() == p.TagSlug.ToLower());
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return this.TagSlug.GetHashCode() ^ this.SiteID.GetHashCode();
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			List<ValidationResult> errors = new List<ValidationResult>();
			List<string> lst = new List<string>();

			if (String.IsNullOrEmpty(this.TagSlug)) {
				ValidationResult err = new ValidationResult("Slug is required", new string[] { "TagSlug" });
				errors.Add(err);
			}
			if (String.IsNullOrEmpty(this.TagText)) {
				ValidationResult err = new ValidationResult("Text is required", new string[] { "TagText" });
				errors.Add(err);
			}

			if (ContentTag.GetSimilar(SiteData.CurrentSite.SiteID, this.ContentTagID, this.TagSlug) > 0) {
				ValidationResult err = new ValidationResult("Slug must be unique", new string[] { "TagSlug" });
				errors.Add(err);
			}

			return errors;
		}

		internal ContentTag(vw_carrot_TagCounted c) {
			if (c != null) {
				this.ContentTagID = c.ContentTagID;
				this.SiteID = c.SiteID;
				this.TagSlug = c.TagSlug;
				this.TagText = c.TagText;
				this.UseCount = c.UseCount;
				this.IsPublic = c.IsPublic;
			}
		}

		internal ContentTag(vw_carrot_TagURL c) {
			if (c != null) {
				SiteData site = SiteData.GetSiteFromCache(c.SiteID);

				this.ContentTagID = c.ContentTagID;
				this.SiteID = c.SiteID;
				this.TagURL = c.TagUrl;
				this.TagText = c.TagText;
				this.UseCount = c.UseCount;
				this.PublicUseCount = c.PublicUseCount;
				this.IsPublic = c.IsPublic;

				if (c.EditDate.HasValue) {
					this.EditDate = site.ConvertUTCToSiteTime(c.EditDate.Value);
				}
			}
		}

		internal ContentTag(carrot_ContentTag c) {
			if (c != null) {
				this.ContentTagID = c.ContentTagID;
				this.SiteID = c.SiteID;
				this.TagSlug = c.TagSlug;
				this.TagText = c.TagText;
				this.IsPublic = c.IsPublic;
			}
		}

		public static ContentTag Get(Guid TagID) {
			ContentTag _item = null;
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				carrot_ContentTag query = CompiledQueries.cqGetContentTagByID(_db, TagID);
				if (query != null) {
					_item = new ContentTag(query);
				}
			}

			return _item;
		}

		public static ContentTag GetByURL(Guid SiteID, string requestedURL) {
			ContentTag _item = null;
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				vw_carrot_TagURL query = CompiledQueries.cqGetContentTagByURL(_db, SiteID, requestedURL);
				if (query != null) {
					_item = new ContentTag(query);
				}
			}

			return _item;
		}

		public static int GetSimilar(Guid SiteID, Guid TagID, string tagSlug) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				IQueryable<carrot_ContentTag> query = CompiledQueries.cqGetContentTagNoMatch(_db, SiteID, TagID, tagSlug);

				return query.Count();
			}
		}

		public static int GetSiteCount(Guid siteID) {
			int iCt = -1;

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				iCt = CompiledQueries.cqGetContentTagCountBySiteID(_db, siteID);
			}

			return iCt;
		}

		public static List<ContentTag> BuildTagList(Guid rootContentID) {
			List<ContentTag> _types = null;

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				IQueryable<carrot_ContentTag> query = CompiledQueries.cqGetContentTagByContentID(_db, rootContentID);

				_types = (from d in query.ToList()
						  select new ContentTag(d)).ToList();
			}

			return _types;
		}

		public void Save() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				bool bNew = false;
				carrot_ContentTag s = CompiledQueries.cqGetContentTagByID(_db, this.ContentTagID);

				if (s == null || (s != null && s.ContentTagID == Guid.Empty)) {
					s = new carrot_ContentTag();
					s.ContentTagID = Guid.NewGuid();
					s.SiteID = this.SiteID;
					bNew = true;
				}

				s.TagSlug = ContentPageHelper.ScrubSlug(this.TagSlug);
				s.TagText = this.TagText;
				s.IsPublic = this.IsPublic;

				if (bNew) {
					_db.carrot_ContentTags.InsertOnSubmit(s);
				}

				_db.SubmitChanges();

				this.ContentTagID = s.ContentTagID;
			}
		}

		public void Delete() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.Create()) {
				carrot_ContentTag s = CompiledQueries.cqGetContentTagByID(_db, this.ContentTagID);

				if (s != null) {
					IQueryable<carrot_TagContentMapping> lst = (from m in _db.carrot_TagContentMappings
																where m.ContentTagID == s.ContentTagID
																select m);

					_db.carrot_TagContentMappings.DeleteBatch(lst);
					_db.carrot_ContentTags.DeleteOnSubmit(s);
					_db.SubmitChanges();
				}
			}
		}
	}
}