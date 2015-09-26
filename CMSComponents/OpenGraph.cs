using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.UI.Components {

	public class OpenGraph : IHtmlString {

		public OpenGraph() {
			this.OpenGraphType = OpenGraphTypeDef.Default;
			this.ShowExpirationDate = false;

			WebViewPage page = ((WebViewPage)WebPageContext.Current.Page);
			if (page != null) {
				if (page.ViewData[PagePayload.ViewDataKey] != null) {
					this.CmsPage = (PagePayload)page.ViewData[PagePayload.ViewDataKey];
				}
				if (page is CmsWebViewPage) {
					this.CmsPage = ((CmsWebViewPage)page).CmsPage;
				}
			}
		}

		public OpenGraph(PagePayload pp)
			: this() {
			this.CmsPage = pp;
		}

		public enum OpenGraphTypeDef {
			Default,
			Article,
			Blog,
			Website,
			Book,
			Video,
			Movie,
			Profile
		}

		public bool ShowExpirationDate { get; set; }

		public OpenGraphTypeDef OpenGraphType { get; set; }

		public PagePayload CmsPage { get; set; }

		public override string ToString() {
			return this.ToHtmlString();
		}

		public string ToHtmlString() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(String.Empty);

			try {
				if (this.CmsPage != null) {
					if (!String.IsNullOrEmpty(this.CmsPage.ThePage.MetaDescription)) {
						sb.AppendLine(CarrotWeb.MetaTag("og:description", this.CmsPage.ThePage.MetaDescription).ToString());
					}
					sb.AppendLine(CarrotWeb.MetaTag("og:url", this.CmsPage.TheSite.DefaultCanonicalURL).ToString());

					string contType = OpenGraphTypeDef.Default.ToString();

					if (this.OpenGraphType == OpenGraphTypeDef.Default) {
						if (this.CmsPage.ThePage.ContentType == ContentPageType.PageType.BlogEntry) {
							contType = OpenGraphTypeDef.Blog.ToString().ToLower();
						} else {
							contType = OpenGraphTypeDef.Article.ToString().ToLower();
						}
						if (this.CmsPage.TheSite.Blog_Root_ContentID.HasValue && this.CmsPage.ThePage.Root_ContentID == this.CmsPage.TheSite.Blog_Root_ContentID) {
							contType = OpenGraphTypeDef.Website.ToString().ToLower();
						}
					} else {
						contType = this.OpenGraphType.ToString().ToLower();
					}

					sb.AppendLine(CarrotWeb.MetaTag("og:type", contType).ToString());

					if (!String.IsNullOrEmpty(this.CmsPage.ThePage.TitleBar)) {
						sb.AppendLine(CarrotWeb.MetaTag("og:title", this.CmsPage.ThePage.TitleBar).ToString());
					}

					if (!String.IsNullOrEmpty(this.CmsPage.TheSite.SiteName)) {
						sb.AppendLine(CarrotWeb.MetaTag("og:site_name", this.CmsPage.TheSite.SiteName).ToString());
					}

					sb.AppendLine(CarrotWeb.MetaTag("article:published_time", this.CmsPage.TheSite.ConvertSiteTimeToISO8601(this.CmsPage.ThePage.GoLiveDate)).ToString());
					sb.AppendLine(CarrotWeb.MetaTag("article:modified_time", this.CmsPage.TheSite.ConvertSiteTimeToISO8601(this.CmsPage.ThePage.EditDate)).ToString());

					if (this.ShowExpirationDate) {
						sb.AppendLine(CarrotWeb.MetaTag("article:expiration_time", this.CmsPage.TheSite.ConvertSiteTimeToISO8601(this.CmsPage.ThePage.RetireDate)).ToString());
					}
				}
			} catch (Exception ex) { }

			return sb.ToString();
		}
	}
}