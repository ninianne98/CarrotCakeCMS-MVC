using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;

using System.Linq;

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class CommentIndexModel {

		public CommentIndexModel() {
			this.Comments = new PagedData<PostComment>();
			this.Comments.PageSize = 25;
			this.Comments.InitOrderBy(x => x.CreateDate, false);
		}

		public Guid? Root_ContentID { get; set; }
		public ContentPageType.PageType PageType { get; set; }
		public PagedData<PostComment> Comments { get; set; }
	}
}