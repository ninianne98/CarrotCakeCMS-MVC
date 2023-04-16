using System.Collections.Generic;
using System.Web.Mvc;
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

namespace Carrotware.Web.UI.Components {

	public class ImageSizer : IHtmlString {

		public ImageSizer() {
			this.ThumbSize = 150;
			this.ScaleImage = true;
			this.ImageUrl = string.Empty;
			this.Title = string.Empty;
		}

		public string HandlerURL {
			get {
				return UrlPaths.ThumbnailPath;
			}
		}

		public string ImageUrl { get; set; }

		public string ImageThumbUrl {
			get {
				string imgURL = this.ImageUrl;
				if (!imgURL.StartsWith(this.HandlerURL)) {
					imgURL = string.Format("{0}?thumb={1}&square={2}&scale={3}", this.HandlerURL, HttpUtility.UrlEncode(this.ImageUrl), this.ThumbSize, this.ScaleImage);
				}
				return imgURL;
			}
		}

		public string ThumbUrl {
			get {
				string imgURL = this.ImageUrl;
				if (!imgURL.StartsWith(this.HandlerURL)) {
					imgURL = string.Format("{0}?square={1}&scale={2}&thumb=", this.HandlerURL, this.ThumbSize, this.ScaleImage);
				}
				return imgURL;
			}
		}

		public string Title { get; set; }

		// allow alt to be different from Title, but set alt to title if not directly set
		private string _alt = null;
		public string Alt {
			get {
				if (string.IsNullOrWhiteSpace(_alt)) {
					_alt = this.Title;
				}
				return _alt;
			}
			set {
				_alt = value;
			}
		}

		public int ThumbSize { get; set; }
		public bool ScaleImage { get; set; }
		public object imageAttributes { get; set; }

		public string ToHtmlString() {
			var imgBuilder = new TagBuilder("img");
			imgBuilder.MergeAttribute("src", this.ImageThumbUrl);
			imgBuilder.MergeAttribute("alt", HttpUtility.UrlEncode(this.Alt));
			imgBuilder.MergeAttribute("title", HttpUtility.UrlEncode(this.Title));

			var imgAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(imageAttributes);
			imgBuilder.MergeAttributes(imgAttribs);

			return imgBuilder.ToString(TagRenderMode.SelfClosing);
		}
	}
}