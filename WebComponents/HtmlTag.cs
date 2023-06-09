using System.Collections.Generic;
using System.Linq;
using System.Text;
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

	public class HtmlTag {
		private Dictionary<string, string> _attrs;
		private string _default = "span";

		public enum EasyTag {
			Css,
			Stylesheet,
			JavaScript,
			Image,
			Link,
			AnchorTag,
			BulletList,
			UnorderedList,
			OrderedList,
			ListItem,
		}

		public HtmlTag() {
			this.Tag = _default;
			_attrs = new Dictionary<string, string>();
		}

		public HtmlTag(string tag) : this() {
			this.Tag = (tag ?? _default).ToLowerInvariant().Trim();
		}

		public HtmlTag(EasyTag tag) : this() {
			string tagHtml = string.Empty;

			switch (tag) {
				case EasyTag.Css:
				case EasyTag.Stylesheet:
					tagHtml = "link";
					this.SetAttribute("rel", "stylesheet");
					this.SetAttribute("type", "text/css");
					break;

				case EasyTag.JavaScript:
					tagHtml = "script";
					this.SetAttribute("type", "text/javascript");
					break;

				case EasyTag.Image:
					tagHtml = "img";
					break;

				case EasyTag.Link:
				case EasyTag.AnchorTag:
					tagHtml = "a";
					break;

				case EasyTag.BulletList:
				case EasyTag.UnorderedList:
					tagHtml = "ul";
					break;

				case EasyTag.OrderedList:
					tagHtml = "ol";
					break;

				case EasyTag.ListItem:
					tagHtml = "li";
					break;
			}

			this.Tag = (tagHtml ?? _default).ToLowerInvariant().Trim();
		}

		public HtmlTag(string tag, string uri) : this(tag) {
			this.Uri = uri;
		}

		public void SetAttribute(string attr, string val) {
			if (val != null) {
				if (_attrs.ContainsKey(attr)) {
					_attrs[attr] = val;
				} else {
					_attrs.Add(attr, val);
				}
			}
		}

		public void MergeAttribute(string attr, string val) {
			if (val != null) {
				if (_attrs.ContainsKey(attr)) {
					_attrs[attr] = string.Format("{0} {1}", _attrs[attr], val).Trim();
				} else {
					_attrs.Add(attr, val);
				}
			}
		}

		public void MergeAttributes(IDictionary<string, string> attrs) {
			if (attrs != null) {
				foreach (var k in attrs) {
					MergeAttribute(k.Key, k.Value);
				}
			}
		}

		public void MergeAttributes(IDictionary<string, object> attrs) {
			if (attrs != null) {
				foreach (var k in attrs) {
					try {
						MergeAttribute(k.Key, k.Value.ToString());
					} catch { }
				}
			}
		}

		public void MergeAttributes(object attrs) {
			if (attrs != null) {
				try {
					var dict = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(attrs);
					MergeAttributes(dict);
				} catch { }
			}
		}

		public string Tag { get; set; }
		public string Uri { get; set; }
		public string InnerHtml { get; set; }

		protected StringBuilder CreateOutput() {
			string[] uris = new string[] { "src", "href" };

			var uri = _attrs.Where(x => uris.Contains(x.Key.ToLowerInvariant())).FirstOrDefault();
			if (uri.Key != null && string.IsNullOrWhiteSpace(this.Uri)) {
				this.Uri = uri.Value;
			}

			var sb = new StringBuilder();
			string[] hrefs = new string[] { "a", "link" };
			string[] srcs = new string[] { "audio", "embed", "iframe", "img", "input", "script", "source", "track", "video" };

			if (hrefs.Contains(this.Tag)) {
				sb.Append(string.Format(" href=\"{0}\" ", this.Uri));
			}

			if (srcs.Contains(this.Tag)) {
				sb.Append(string.Format(" src=\"{0}\" ", this.Uri));
			}

			foreach (var kvp in _attrs.Where(x => x.Value.Trim().Length > 0)) {
				sb.Append(string.Format(" {0}=\"{1}\" ", HttpUtility.HtmlEncode(kvp.Key), HttpUtility.HtmlEncode((kvp.Value))));
			}

			sb.Replace("  ", " ").Replace("  ", " ");

			return sb;
		}

		public enum TagMode {
			Open,
			Close,
			Normal,
			SelfClosing,
		}

		public string ToString(TagMode renderMode) {
			switch (renderMode) {
				case TagMode.Open:
					return OpenTag();

				case TagMode.Close:
					return CloseTag();

				case TagMode.Normal:
					return RenderTag();

				default:
				case TagMode.SelfClosing:
					return RenderSelfClosingTag();
			}
		}

		public override string ToString() {
			return this.RenderTag();
		}

		public string OpenTag() {
			var sb = CreateOutput();

			return string.Format("<{0} {1}>", this.Tag, sb.ToString().Trim());
		}

		public string CloseTag() {
			return string.Format("</{0}>", this.Tag);
		}

		public string RenderTag() {
			var sb = CreateOutput();

			return string.Format("<{0} {1}>{2}</{0}>", this.Tag, sb.ToString().Trim(), this.InnerHtml);
		}

		public string RenderSelfClosingTag() {
			var sb = CreateOutput();

			return string.Format("<{0} {1} />", this.Tag, sb.ToString().Trim());
		}

		public HtmlString EmitHtmlTag() {
			return new HtmlString(RenderTag());
		}

		public HtmlString EmitHtmlSelfClosingTag() {
			return new HtmlString(RenderSelfClosingTag());
		}
	}
}