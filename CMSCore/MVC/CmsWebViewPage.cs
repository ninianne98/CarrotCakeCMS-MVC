using System.Web.Mvc;

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

	public abstract class CmsWebViewPage : WebViewPage {
		public PagePayload CmsPage { get; set; }

		public override void InitHelpers() {
			base.InitHelpers();

			if (this.ViewData.Model != null && this.ViewData.Model is PagePayload) {
				this.CmsPage = (PagePayload)this.ViewData.Model;
			} else {
				if (this.ViewData[PagePayload.ViewDataKey] == null) {
					this.CmsPage = PagePayload.GetCurrentContent();
					this.ViewData[PagePayload.ViewDataKey] = this.CmsPage;
				} else {
					this.CmsPage = (PagePayload)this.ViewData[PagePayload.ViewDataKey];
				}
			}
		}

		public void OverridePage(PagePayload page) {
			this.CmsPage = page;
			this.ViewData[PagePayload.ViewDataKey] = this.CmsPage;
		}

		public void OverridePage(SiteNav nav) {
			this.CmsPage = PagePayload.GetContent(nav);
			this.ViewData[PagePayload.ViewDataKey] = this.CmsPage;
		}
	}

	//=====================

	public abstract class CmsWebViewPage<TModel> : CmsWebViewPage {
	}
}