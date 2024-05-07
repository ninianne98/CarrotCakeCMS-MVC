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
				return (this.SiteID == p.SiteID && this.ContentTagID == p.ContentTagID);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return this.ContentTagID.GetHashCode() ^ this.SiteID.GetHashCode();
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
				this.TagSlug = ContentPageHelper.ScrubSlug(c.TagSlug);
				this.TagText = c.TagText;
				this.UseCount = c.UseCount;
				this.PublicUseCount = 1;
				this.IsPublic = c.IsPublic;

				SiteData site = SiteData.GetSiteFromCache(c.SiteID);
				if (site != null) {
					this.TagURL = ContentPageHelper.ScrubFilename(c.ContentTagID, String.Format("/{0}/{1}", site.BlogTagPath, c.TagSlug));
				}
			}
		}

		internal ContentTag(vw_carrot_TagURL c) {
			if (c != null) {
				SiteData site = SiteData.GetSiteFromCache(c.SiteID);

				this.ContentTagID = c.ContentTagID;
				this.SiteID = c.SiteID;
				this.TagURL = ContentPageHelper.ScrubFilename(c.ContentTagID, c.TagUrl);
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
				this.TagSlug = ContentPageHelper.ScrubSlug(c.TagSlug);
				this.TagText = c.TagText;
				this.IsPublic = c.IsPublic;
				this.UseCount = 1;
				this.PublicUseCount = 1;

				SiteData site = SiteData.GetSiteFromCache(c.SiteID);
				if (site != null) {
					this.TagURL = ContentPageHelper.ScrubFilename(c.ContentTagID, String.Format("/{0}/{1}", site.BlogTagPath, c.TagSlug));
				}
			}
		}

		public static ContentTag Get(Guid TagID) {
			ContentTag item = null;
			using (var db = CarrotCMSDataContext.Create()) {
				carrot_ContentTag query = CompiledQueries.cqGetContentTagByID(db, TagID);
				if (query != null) {
					item = new ContentTag(query);
				}
			}

			return item;
		}

		public static ContentTag GetByURL(Guid SiteID, string requestedURL) {
			ContentTag item = null;
			using (var db = CarrotCMSDataContext.Create()) {
				vw_carrot_TagURL query = CompiledQueries.cqGetContentTagByURL(db, SiteID, requestedURL);
				if (query != null) {
					item = new ContentTag(query);
				}
			}

			return item;
		}

		public static int GetSimilar(Guid SiteID, Guid TagID, string tagSlug) {
			using (var db = CarrotCMSDataContext.Create()) {
				IQueryable<carrot_ContentTag> query = CompiledQueries.cqGetContentTagNoMatch(db, SiteID, TagID, tagSlug);

				return query.Count();
			}
		}

		public static int GetSiteCount(Guid siteID) {
			int iCt = -1;

			using (var db = CarrotCMSDataContext.Create()) {
				iCt = CompiledQueries.cqGetContentTagCountBySiteID(db, siteID);
			}

			return iCt;
		}

		public static List<ContentTag> BuildTagList(Guid rootContentID) {
			List<ContentTag> types = null;

			using (var db = CarrotCMSDataContext.Create()) {
				IQueryable<carrot_ContentTag> query = CompiledQueries.cqGetContentTagByContentID(db, rootContentID);

				types = (from d in query.ToList()
						 select new ContentTag(d)).ToList();
			}

			return types;
		}

		public void Save() {
			using (var db = CarrotCMSDataContext.Create()) {
				bool bNew = false;
				carrot_ContentTag s = CompiledQueries.cqGetContentTagByID(db, this.ContentTagID);

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
					db.carrot_ContentTags.InsertOnSubmit(s);
				}

				db.SubmitChanges();

				this.ContentTagID = s.ContentTagID;
			}
		}

		public void Delete() {
			using (var db = CarrotCMSDataContext.Create()) {
				carrot_ContentTag s = CompiledQueries.cqGetContentTagByID(db, this.ContentTagID);

				if (s != null) {
					IQueryable<carrot_TagContentMapping> lst = (from m in db.carrot_TagContentMappings
																where m.ContentTagID == s.ContentTagID
																select m);

					db.carrot_TagContentMappings.BatchDelete(lst);
					db.carrot_ContentTags.DeleteOnSubmit(s);
					db.SubmitChanges();
				}
			}
		}
	}
}