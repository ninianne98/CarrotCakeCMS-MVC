using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
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
				using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
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
								sFieldValue = string.Format("{0}", objData);
							}

							this.NavigateText = sFieldValue;
						}

						this.NavigateUrl = navNext.FileName;
					} else {
						this.NavigateUrl = string.Empty;
					}
				}
			} else {
				this.NavigateUrl = string.Empty;
			}

			if (!string.IsNullOrEmpty(this.NavigateUrl)) {
				var lnkBuilder = new HtmlTag("a");
				lnkBuilder.Uri = this.NavigateUrl;
				lnkBuilder.InnerHtml = this.NavigateUrl;

				if (!string.IsNullOrEmpty(this.NavigateText)) {
					lnkBuilder.InnerHtml = this.NavigateText;
				}

				lnkBuilder.MergeAttributes(linkAttributes);

				return lnkBuilder.RenderTag();
			} else {
				return string.Empty;
			}
		}
	}

	//========================================
	public class ContentPageImageThumb : BaseCmsComponent {

		public ContentPageImageThumb() {
			this.ThumbSize = 100;
			this.ScaleImage = true;
			this.PerformURLResize = false;
			this.ImageUrl = string.Empty;
			this.ImgSrc = string.Empty;
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

		public int ThumbSize { get; set; } = 100;

		public bool ScaleImage { get; set; } = true;

		public bool PerformURLResize { get; set; }

		public object imageAttributes { get; set; }

		public ContentPage ContentPage { get; set; }

		public override string GetHtml() {
			if (this.ContentPage == null) {
				this.ContentPage = SiteData.GetCurrentPage();
			}

			this.ImgSrc = this.ContentPage.Thumbnail;
			this.Title = this.ContentPage.NavMenuText;

			if (string.IsNullOrEmpty(this.ImgSrc)) {
				// if page itself has no image, see if the image had been specified directly
				this.ImgSrc = this.ImageUrl;
			}

			if (!string.IsNullOrEmpty(this.ImgSrc)) {
				if (this.PerformURLResize) {
					ImageSizer imgSzr = new ImageSizer();
					imgSzr.ImageUrl = this.ImgSrc;
					imgSzr.ThumbSize = this.ThumbSize;
					imgSzr.ScaleImage = this.ScaleImage;

					this.ImgSrc = imgSzr.ImageThumbUrl;
				}
			} else {
				this.ImgSrc = string.Empty;
			}

			if (!string.IsNullOrEmpty(this.ImgSrc)) {
				var imgBuilder = new HtmlTag("img");
				imgBuilder.Uri = this.ImgSrc;
				imgBuilder.MergeAttributes(imageAttributes);

				if (!string.IsNullOrEmpty(this.Title)) {
					imgBuilder.MergeAttribute("alt", this.Title);
					imgBuilder.MergeAttribute("title", this.Title);
				}

				return imgBuilder.RenderSelfClosingTag();
			} else {
				return string.Empty;
			}
		}
	}

	//========================================
	public class SiteCanonicalURL : BaseCmsComponent {

		public SiteCanonicalURL() {
			this.Enable301Redirect = false;
			this.ContentPage = null;
		}

		public SiteCanonicalURL(ContentPage page) {
			this.Enable301Redirect = false;
			this.ContentPage = page;
		}

		public bool Enable301Redirect { get; set; } = false;

		public ContentPage ContentPage { get; set; }

		public override string GetHtml() {
			var site = SiteData.CurrentSite;
			var blogIndexId = site.Blog_Root_ContentID.HasValue ? site.Blog_Root_ContentID.Value : Guid.Empty;
			var pageUri = string.Empty;
			var pageisIndex = false;

			if (site != null) {
				pageisIndex = this.ContentPage.Root_ContentID == blogIndexId;
				pageUri = site.DefaultCanonicalURL;

				if (this.ContentPage == null) {
					this.ContentPage = SiteData.GetCurrentPage();
				}

				if (this.ContentPage != null) {
					if (this.ContentPage.NavOrder == 0) {
						pageUri = site.MainCanonicalURL;
					} else {
						if (pageisIndex && SiteData.CurrentScriptName.Length > 1
								&& this.ContentPage.FileName.ToLowerInvariant() != SiteData.CurrentScriptName.ToLowerInvariant()) {
							// if blog index, use whatever the url is as the valid url
							pageUri = site.MainCanonicalURL + SiteData.CurrentScriptName.Substring(1);
						} else {
							pageUri = site.DefaultCanonicalURL;
						}
					}
				}
			} else {
				pageUri = SiteData.DefaultDirectoryFilename;
			}

			string lnk = string.Format("<link rel=\"canonical\" href=\"{0}\" />", pageUri);

			// do not do a redirect on index because its URLs can vary legitimately
			if (this.Enable301Redirect && pageisIndex == false) {
				HttpContext ctx = HttpContext.Current;

				if (!site.MainCanonicalURL.ToLowerInvariant().Contains(@"://" + CMSConfigHelper.DomainName.ToLowerInvariant())) {
					ctx.Response.Status = "301 Moved Permanently";
					ctx.Response.AddHeader("Location", pageUri);
				}
			}

			return lnk;
		}
	}
}