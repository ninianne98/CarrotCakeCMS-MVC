using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Carrotware.CMS.Mvc.UI.Admin.Models {

	public class ContentSnippetHistoryModel {

		public ContentSnippetHistoryModel() {
			this.Item = new ContentSnippet();
			this.History = new PagedData<ContentSnippet>();
		}

		public ContentSnippetHistoryModel(Guid id)
			: this() {
			this.Item = ContentSnippet.Get(id);

			this.History.SetData(this.Item.GetHistory());

			this.History.TotalRecords = this.History.DataSource.Count;
			this.History.PageSize = this.History.TotalRecords * 2;

			this.History.InitOrderBy(x => x.ContentSnippetName);
		}

		public ContentSnippet Item { get; set; }

		public PagedData<ContentSnippet> History { get; set; }
	}
}