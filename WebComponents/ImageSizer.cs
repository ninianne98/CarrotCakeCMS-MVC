using System;
using System.Collections.Generic;
using System.Web;
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

namespace Carrotware.Web.UI.Components {

	public class ImageSizer : IHtmlString {

		public ImageSizer() {
			this.ThumbSize = 150;
			this.ScaleImage = true;
			this.ImageUrl = String.Empty;
			this.Title = String.Empty;
		}

		public string HandlerURL {
			get {
				return "/carrotwarethumb.ashx";
			}
		}

		public string ImageUrl { get; set; }

		public string ImageThumbUrl {
			get {
				string imgURL = this.ImageUrl;
				if (!imgURL.StartsWith(this.HandlerURL)) {
					imgURL = String.Format("{0}?thumb={1}&square={2}&scale={3}", this.HandlerURL, HttpUtility.UrlEncode(this.ImageUrl), this.ThumbSize, this.ScaleImage);
				}
				return imgURL;
			}
		}

		public string ThumbUrl {
			get {
				string imgURL = this.ImageUrl;
				if (!imgURL.StartsWith(this.HandlerURL)) {
					imgURL = String.Format("{0}?square={1}&scale={2}&thumb=", this.HandlerURL, this.ThumbSize, this.ScaleImage);
				}
				return imgURL;
			}
		}

		public string Title { get; set; }
		public int ThumbSize { get; set; }
		public bool ScaleImage { get; set; }
		public object imageAttributes { get; set; }

		public string ToHtmlString() {
			var imgBuilder = new TagBuilder("img");
			imgBuilder.MergeAttribute("src", this.ImageThumbUrl);
			imgBuilder.MergeAttribute("alt", this.Title);
			imgBuilder.MergeAttribute("title", this.Title);

			var imgAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(imageAttributes);
			imgBuilder.MergeAttributes(imgAttribs);

			return imgBuilder.ToString(TagRenderMode.SelfClosing);
		}
	}
}