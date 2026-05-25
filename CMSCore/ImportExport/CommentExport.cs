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

	public class CommentExport {

		public CommentExport() {
			this.CarrotCakeVersion = SiteData.CarrotCakeCMSVersion;
			this.ExportDate = DateTime.UtcNow;

			this.TheComment = new PostComment();
		}

		public static List<CommentExport> GetPageCommentExport(Guid rootContentID) {
			List<CommentExport> lst = PostComment.GetCommentsByContentPage(rootContentID, false).Select(x => new CommentExport(x)).ToList();

			return lst;
		}

		public CommentExport(PostComment pc) {
			SetVals(pc);
		}

		private void SetVals(PostComment pc) {
			this.CarrotCakeVersion = SiteData.CarrotCakeCMSVersion;
			this.ExportDate = DateTime.UtcNow;

			this.NewContentCommentID = Guid.NewGuid();

			this.TheComment = pc;

			if (this.TheComment == null) {
				this.TheComment = new PostComment();
				this.TheComment.Root_ContentID = Guid.NewGuid();
			}

			this.OriginalRootContentID = this.TheComment.Root_ContentID;
			this.OriginalContentCommentID = this.TheComment.ContentCommentID;

			this.TheComment.ContentCommentID = this.NewContentCommentID;
		}

		public string CarrotCakeVersion { get; set; } = SiteData.CarrotCakeCMSVersion;

		public DateTime ExportDate { get; set; } = DateTime.UtcNow;

		public Guid NewContentCommentID { get; set; }

		public Guid OriginalContentCommentID { get; set; }

		public Guid OriginalRootContentID { get; set; }

		public PostComment TheComment { get; set; }
	}
}