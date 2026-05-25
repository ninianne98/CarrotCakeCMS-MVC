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

	public class WordPressSite {

		public WordPressSite() {
			this.Content = new List<WordPressPost>();
			this.Comments = new List<WordPressComment>();
			this.Authors = new List<WordPressUser>();
			this.Categories = new List<InfoKVP>();
			this.Tags = new List<InfoKVP>();
		}

		public Guid NewSiteID { get; set; }

		public string SiteTitle { get; set; }
		public string SiteDescription { get; set; }
		public string SiteURL { get; set; }

		public string ImportSource { get; set; }
		public string wxrVersion { get; set; }

		public DateTime ExtractDate { get; set; }

		public List<InfoKVP> Categories { get; set; } = new List<InfoKVP>();
		public List<InfoKVP> Tags { get; set; } = new List<InfoKVP>();

		public List<WordPressPost> Content { get; set; } = new List<WordPressPost>();
		public List<WordPressComment> Comments { get; set; } = new List<WordPressComment>();
		public List<WordPressUser> Authors { get; set; } = new List<WordPressUser>();

		public List<WordPressPost> ContentPages {
			get {
				return (from c in this.Content
						where c.PostType == WordPressPost.WPPostType.Page
						orderby c.PostOrder ascending
						select c).ToList();
			}
		}

		public List<WordPressPost> ContentPosts {
			get {
				return (from c in this.Content
						where c.PostType == WordPressPost.WPPostType.BlogPost
						orderby c.PostDateUTC descending
						select c).ToList();
			}
		}

		public Guid FindImportUser(string author) {
			if (!string.IsNullOrEmpty(author) && this.Authors != null && this.Authors.Any()) {
				var usr = (from t in this.Authors
						   where (t.Login.ToLowerInvariant() == author.ToLowerInvariant())
								   || (t.Email.ToLowerInvariant() == author.ToLowerInvariant())
						   select t).FirstOrDefault();

				if (usr != null && usr.ImportUserID != Guid.Empty) {
					return usr.ImportUserID;
				}
			}

			return SecurityData.CurrentUserGuid;
		}

		public Guid FindImportUser(WordPressUser u) {
			var usr = (from t in this.Authors
					   where (u.Login != null && t.Login.ToLowerInvariant() == u.Login.ToLowerInvariant())
							   || (u.Email != null && t.Email.ToLowerInvariant() == u.Email.ToLowerInvariant())
					   select t).FirstOrDefault();

			if (usr != null && usr.ImportUserID != Guid.Empty) {
				return usr.ImportUserID;
			}
			return SecurityData.CurrentUserGuid;
		}

		public void SavePageEdit(WordPressPost post, ContentPage cp) {
			cp.SavePageEdit();

			post.ImportRootID = cp.Root_ContentID;

			this.Comments.Where(c => c.PostID == post.PostID).ToList()
					.ForEach(r => r.ImportRootID = post.ImportRootID);
		}

		public override string ToString() {
			return this.SiteTitle + " : " + this.SiteDescription;
		}

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is WordPressSite) {
				WordPressSite p = (WordPressSite)obj;
				return (this.SiteTitle == p.SiteTitle)
						&& (this.SiteURL == p.SiteURL);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return (this.SiteTitle ?? string.Empty).GetHashCode() ^ (this.SiteURL ?? string.Empty).GetHashCode();
		}
	}
}