using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

	//========================================
	public class ContentPageNext : BaseCmsComponent {

		public ContentPageNext() {
			this.CaptionDataField = CaptionSource.NavMenuText;
			this.NavigationDirection = NavDirection.Unknown;
			this.UseDefaultText = true;
			this.ContentPage = null;
		}

		public enum CaptionSource {
			TitleBar,
			NavMenuText,
			PageHead,
		}

		public enum NavDirection {
			Unknown,
			Prev,
			Next,
		}

		public CaptionSource CaptionDataField { get; set; }

		public NavDirection NavigationDirection { get; set; }

		public bool UseDefaultText { get; set; }

		public string NavigateUrl { get; set; }

		public string NavigateText { get; set; }

		public object linkAttributes { get; set; }

		public ContentPage ContentPage { get; set; }

		public override string GetHtml() {
			string sFieldValue = string.Empty;

			if (this.ContentPage == null) {
				this.ContentPage = SiteData.GetCurrentPage();
			}

			SiteNav navNext = new SiteNav();

			if (this.NavigationDirection != NavDirection.Unknown) {
				using (SiteNavHelper navHelper = new SiteNavHelper()) {
					if (NavigationDirection == NavDirection.Prev) {
						navNext = navHelper.GetPrevPost(SiteData.CurrentSiteID, this.ContentPage.Root_ContentID, !SecurityData.IsAuthEditor);
					}
					if (NavigationDirection == NavDirection.Next) {
						navNext = navHelper.GetNextPost(SiteData.CurrentSiteID, this.ContentPage.Root_ContentID, !SecurityData.IsAuthEditor);
					}

					if (navNext != null) {
						if (this.UseDefaultText) {
							string sField = this.CaptionDataField.ToString();

							object objData = ReflectionUtilities.GetPropertyValue(navNext, sField);
							if (objData != null) {
								sFieldValue = String.Format("{0}", objData);
							}

							this.NavigateText = sFieldValue;
						}

						this.NavigateUrl = navNext.FileName;
					} else {
						this.NavigateUrl = String.Empty;
					}
				}
			} else {
				this.NavigateUrl = String.Empty;
			}

			if (!String.IsNullOrEmpty(this.NavigateUrl)) {
				var lnkBuilder = new TagBuilder("a");
				lnkBuilder.MergeAttribute("href", this.NavigateUrl);

				lnkBuilder.InnerHtml = this.NavigateUrl;
				if (!String.IsNullOrEmpty(this.NavigateText)) {
					lnkBuilder.InnerHtml = this.NavigateText;
				}

				var lnkAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(linkAttributes);
				lnkBuilder.MergeAttributes(lnkAttribs);

				return lnkBuilder.ToString(TagRenderMode.Normal);
			} else {
				return String.Empty;
			}
		}
	}

	//========================================
	public class ContentPageImageThumb : BaseCmsComponent {

		public ContentPageImageThumb() {
			this.ThumbSize = 100;
			this.ScaleImage = true;
			this.PerformURLResize = false;
			this.ImageUrl = String.Empty;
			this.ImgSrc = String.Empty;
			this.ContentPage = null;
		}

		public ContentPageImageThumb(string imageUrl)
			: this() {
			this.ImageUrl = imageUrl;
			this.ImgSrc = imageUrl;
		}

		public string ImageUrl { get; set; }

		public string ImgSrc { get; protected set; }

		public string Title { get; set; }

		public int ThumbSize { get; set; }

		public bool ScaleImage { get; set; }

		public bool PerformURLResize { get; set; }

		public object imageAttributes { get; set; }

		public ContentPage ContentPage { get; set; }

		public override string GetHtml() {
			if (this.ContentPage == null) {
				this.ContentPage = SiteData.GetCurrentPage();
			}

			this.ImgSrc = this.ContentPage.Thumbnail;
			this.Title = this.ContentPage.NavMenuText;

			if (String.IsNullOrEmpty(this.ImgSrc)) {
				// if page itself has no image, see if the image had been specified directly
				this.ImgSrc = this.ImageUrl;
			}

			if (!String.IsNullOrEmpty(this.ImgSrc)) {
				if (this.PerformURLResize) {
					ImageSizer imgSzr = new ImageSizer();
					imgSzr.ImageUrl = this.ImgSrc;
					imgSzr.ThumbSize = this.ThumbSize;
					imgSzr.ScaleImage = this.ScaleImage;

					this.ImgSrc = imgSzr.ImageThumbUrl;
				}
			} else {
				this.ImgSrc = String.Empty;
			}

			if (!String.IsNullOrEmpty(this.ImgSrc)) {
				var imgBuilder = new TagBuilder("img");
				imgBuilder.MergeAttribute("src", this.ImgSrc);

				if (!String.IsNullOrEmpty(this.Title)) {
					imgBuilder.MergeAttribute("alt", this.Title);
					imgBuilder.MergeAttribute("title", this.Title);
				}

				var imgAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(imageAttributes);
				imgBuilder.MergeAttributes(imgAttribs);

				return imgBuilder.ToString(TagRenderMode.SelfClosing);
			} else {
				return String.Empty;
			}
		}
	}

	//========================================
	public class SiteCanonicalURL : BaseCmsComponent {

		public SiteCanonicalURL() {
			this.Enable301Redirect = false;
			this.ContentPage = null;
		}

		public bool Enable301Redirect { get; set; }

		public ContentPage ContentPage { get; set; }

		public override string GetHtml() {
			string pageUri = string.Empty;

			SiteData sd = SiteData.CurrentSite;

			if (sd != null) {
				pageUri = sd.DefaultCanonicalURL;
				if (this.ContentPage == null) {
					this.ContentPage = SiteData.GetCurrentPage();
				}

				if (ContentPage != null) {
					if (ContentPage.NavOrder == 0) {
						pageUri = sd.MainCanonicalURL;
					} else {
						pageUri = sd.DefaultCanonicalURL;
					}
				}
			} else {
				pageUri = SiteData.DefaultDirectoryFilename;
			}

			string lnk = String.Format("<link rel=\"canonical\" href=\"{0}\" />\r\n", pageUri);

			if (this.Enable301Redirect) {
				HttpContext ctx = HttpContext.Current;

				if (!SiteData.CurrentSite.MainCanonicalURL.ToLower().Contains(@"://" + CMSConfigHelper.DomainName.ToLower())) {
					ctx.Response.Status = "301 Moved Permanently";
					ctx.Response.AddHeader("Location", pageUri);
				}
			}

			return lnk;
		}
	}
}