using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Carrotware.CMS.UI.Components {

	public class PagedComments : PagedData<PostComment> {

		public PagedComments() {
			this.InitOrderBy(x => x.CreateDate, false);
		}

		public string GetUrl(int pageNbr) {
			return String.Format("{0}?{1}={2}", SiteData.CurrentScriptName, this.PageNumbParm, pageNbr);
		}

		public void FetchData() {
			base.ReadPageNbr();
			List<PostComment> lstContents = new List<PostComment>();

			using (SiteNavHelper navHelper = new SiteNavHelper()) {
				if (SiteData.IsWebView) {
					SiteNav sn = navHelper.FindByFilename(SiteData.CurrentSiteID, SiteData.CurrentScriptName);

					if (sn != null) {
						TotalRecords = PostComment.GetCommentCountByContent(sn.Root_ContentID, !SecurityData.IsAuthEditor);
						lstContents = PostComment.GetCommentsByContentPageNumber(sn.Root_ContentID, this.PageNumberZeroIndex, this.PageSize, this.OrderBy, !SecurityData.IsAuthEditor);
					}
				} else {
					TotalRecords = 0;
					lstContents = new List<PostComment>();
				}
			}

			this.DataSource = lstContents;
		}
	}
}