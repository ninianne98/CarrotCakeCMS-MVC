using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

	public class WrappedItem : IDisposable {
		protected HtmlHelper _helper = null;
		protected StringBuilder _stringbldr = null;

		protected string _tag = "li";
		private string DefaultActionName = "Index";

		public WrappedItem(HtmlHelper htmlHelper, string tag,
							string actionName, string controllerName,
							object activeAttributes = null, object inactiveAttributes = null) {
			_helper = htmlHelper;

			StringBuilder sb = new StringBuilder();

			_helper.ViewContext.Writer.Write(OpenTag(htmlHelper, sb, tag,
						actionName, controllerName, activeAttributes, inactiveAttributes));

			sb.Clear();
		}

		public WrappedItem(HtmlHelper htmlHelper, string tag,
							int currentPage, int selectedPage,
							object activeAttributes = null, object inactiveAttributes = null) {
			_helper = htmlHelper;
			StringBuilder sb = new StringBuilder();

			_helper.ViewContext.Writer.Write(OpenTag(htmlHelper, sb, tag, currentPage, selectedPage,
						activeAttributes, inactiveAttributes));

			sb.Clear();
		}

		public WrappedItem(HtmlHelper htmlHelper, string tag, object htmlAttributes = null) {
			_helper = htmlHelper;
			StringBuilder sb = new StringBuilder();

			_helper.ViewContext.Writer.Write(OpenTag(sb, tag, htmlAttributes));

			sb.Clear();
		}

		//=====================

		public WrappedItem(HtmlHelper htmlHelper, StringBuilder sb, string tag,
						string actionName, string controllerName,
						object activeAttributes = null, object inactiveAttributes = null) {
			_stringbldr = sb;

			OpenTag(htmlHelper, sb, tag, actionName, controllerName, activeAttributes, inactiveAttributes);
		}

		public WrappedItem(HtmlHelper htmlHelper, StringBuilder sb, string tag,
							int currentPage, int selectedPage,
							object activeAttributes = null, object inactiveAttributes = null) {
			_stringbldr = sb;

			OpenTag(htmlHelper, sb, tag, currentPage, selectedPage, activeAttributes, inactiveAttributes);
		}

		public WrappedItem(StringBuilder sb, string tag, object htmlAttributes = null) {
			_stringbldr = sb;

			OpenTag(sb, tag, htmlAttributes);
		}

		//=====================

		private string OpenTag(HtmlHelper htmlHelper, StringBuilder sb, string tag,
				string actionName, string controllerName,
				object activeAttributes = null, object inactiveAttributes = null) {
			_stringbldr = sb;
			_tag = String.IsNullOrEmpty(tag) ? "li" : tag;

			actionName = String.IsNullOrEmpty(actionName) ? DefaultActionName : actionName.Replace(" ", "");

			string[] actionNames = actionName.Contains(";") ? actionName.Split(';') : new string[] { actionName };

			// shortcut for when using the default action rather than having to hardcode to a string
			if (actionNames == null || !actionNames.Any()) {
				actionNames = new string[] { DefaultActionName };
			}

			string currentAction = htmlHelper.ViewContext.RouteData.GetRequiredString("action");
			string currentController = htmlHelper.ViewContext.RouteData.GetRequiredString("controller");

			string theAction = actionNames.Where(x => x.Trim().Equals(currentAction, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

			if (actionName == "*") {
				theAction = currentAction;
			}

			if (!String.IsNullOrEmpty(theAction)) {
				actionName = theAction.Trim();
			}

			var tagBuilder = new TagBuilder(_tag);
			IDictionary<string, object> tagAttrib = null;

			if ((actionName.Equals(currentAction, StringComparison.InvariantCultureIgnoreCase))
				&& controllerName.Equals(currentController, StringComparison.InvariantCultureIgnoreCase)) {
				if (activeAttributes != null) {
					tagAttrib = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(activeAttributes);
				}
			} else {
				if (inactiveAttributes != null) {
					tagAttrib = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(inactiveAttributes);
				}
			}

			if (tagAttrib != null) {
				tagBuilder.MergeAttributes(tagAttrib);
			}

			_stringbldr.Append(tagBuilder.ToString(TagRenderMode.StartTag) + Environment.NewLine);
			return _stringbldr.ToString();
		}

		private string OpenTag(HtmlHelper htmlHelper, StringBuilder sb, string tag,
							int currentPage, int selectedPage,
							object activeAttributes = null, object inactiveAttributes = null) {
			_stringbldr = sb;
			_tag = String.IsNullOrEmpty(tag) ? "li" : tag;

			var tagBuilder = new TagBuilder(_tag);
			IDictionary<string, object> tagAttrib = null;

			if (currentPage == selectedPage) {
				if (activeAttributes != null) {
					tagAttrib = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(activeAttributes);
				}
			} else {
				if (inactiveAttributes != null) {
					tagAttrib = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(inactiveAttributes);
				}
			}

			if (tagAttrib != null) {
				tagBuilder.MergeAttributes(tagAttrib);
			}

			_stringbldr.Append(tagBuilder.ToString(TagRenderMode.StartTag) + Environment.NewLine);
			return _stringbldr.ToString();
		}

		private string OpenTag(StringBuilder sb, string tag, object htmlAttributes = null) {
			_stringbldr = sb;
			_tag = String.IsNullOrEmpty(tag) ? "li" : tag;

			var tagBuilder = new TagBuilder(_tag);
			IDictionary<string, object> tagAttrib = null;

			if (htmlAttributes != null) {
				if ((htmlAttributes is IDictionary<string, object>)
							|| (htmlAttributes is Dictionary<string, object>)) {
					tagAttrib = (IDictionary<string, object>)htmlAttributes;
				} else {
					tagAttrib = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
				}
			}

			if (tagAttrib != null) {
				tagBuilder.MergeAttributes(tagAttrib);
			}

			_stringbldr.Append(tagBuilder.ToString(TagRenderMode.StartTag));

			return _stringbldr.ToString();
		}

		//=====================
		public void Dispose() {
			if (_stringbldr != null && _stringbldr.Length > 1) {
				_stringbldr.Append(Environment.NewLine + String.Format("</{0}>", _tag));
			} else {
				_helper.ViewContext.Writer.Write(Environment.NewLine + String.Format("</{0}>", _tag));
			}
		}
	}
}