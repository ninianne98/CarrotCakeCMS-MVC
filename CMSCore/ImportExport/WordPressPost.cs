using Carrotware.Web.UI.Components;
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

	public class WordPressPost {

		public enum WPPostType {
			Unknown,
			Attachment,
			BlogPost,
			Page
		}

		public WordPressPost() { }

		public string PostTitle { get; set; }
		public string PostName { get; set; }
		public string PostContent { get; set; }
		public DateTime PostDateUTC { get; set; }
		public bool IsPublished { get; set; }

		public int PostOrder { get; set; }
		public int PostID { get; set; }
		public int ParentPostID { get; set; }

		public WPPostType PostType { get; set; }

		public List<string> Categories { get; set; }
		public List<string> Tags { get; set; }

		public string PostAuthor { get; set; }

		public Guid ImportRootID { get; set; }
		public string ImportFileSlug { get; set; }
		public string ImportFileName { get; set; }

		public string AttachmentURL { get; set; }

		public override string ToString() {
			return this.PostTitle + " : " + this.PostType.ToString() + " , #" + this.PostID;
		}

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is WordPressPost) {
				WordPressPost p = (WordPressPost)obj;
				return (this.PostID == p.PostID)
						&& (this.ImportFileName == p.ImportFileName)
						&& (this.PostDateUTC == p.PostDateUTC);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return this.PostID.GetHashCode() ^ this.ImportFileName.GetHashCode() ^ this.PostDateUTC.GetHashCode();
		}

		public void CleanBody() {
			if (String.IsNullOrEmpty(this.PostContent)) {
				this.PostContent = "";
			}

			this.PostContent = this.PostContent.Replace("\r\n", "\n");
			this.PostContent = this.PostContent.Replace('\u00A0', ' ').Replace("\n\n\n\n", "\n\n\n").Replace("\n\n\n\n", "\n\n\n");
			this.PostContent = this.PostContent.Trim();
		}

		public void RepairBody() {
			this.CleanBody();

			this.PostContent = "<p>" + this.PostContent.Replace("\n\n", "</p><p>") + "</p>";
			this.PostContent = this.PostContent.Replace("\n", "<br />\n");
			this.PostContent = this.PostContent.Replace("</p><p>", "</p>\n<p>");
		}

		public void GrabAttachments(string folderName, WordPressSite wpSite) {
			int iPost = this.PostID;

			List<WordPressPost> lstA = (from a in wpSite.Content
										where a.PostType == WPPostType.Attachment
										&& a.ParentPostID == iPost
										select a).Distinct().ToList();

			lstA.ToList().ForEach(q => q.ImportFileSlug = q.ImportFileSlug.NormalizeFilename());

			using (CMSConfigHelper cmsHelper = new CMSConfigHelper()) {
				foreach (var img in lstA) {
					img.ImportFileSlug = ("~" + folderName + "/" + img.ImportFileSlug).CleanDuplicateSlashes();

					cmsHelper.GetFile(img.AttachmentURL, img.ImportFileSlug);
					var imgPath = img.ImportFileSlug.Replace("~", "");

					this.PostContent = this.PostContent.Replace(img.AttachmentURL, imgPath);
				}
			}
		}
	}
}