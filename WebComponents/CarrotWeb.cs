using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.WebPages;

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

	public static class CarrotWeb {

		public static HtmlHelper Html {
			get { return ((WebViewPage)WebPageContext.Current.Page).Html; }
		}

		public static AjaxHelper Ajax {
			get { return ((WebViewPage)WebPageContext.Current.Page).Ajax; }
		}

		public static UrlHelper Url {
			get { return ((WebViewPage)WebPageContext.Current.Page).Url; }
		}

		private static Page _Page;

		private static Page WebPage {
			get {
				if (_Page == null) {
					_Page = new Page();
					_Page.AppRelativeVirtualPath = "~/";
				}
				return _Page;
			}
		}

		public static string GetWebResourceUrl(Type type, string resource) {
			string sPath = String.Empty;

			try {
				sPath = WebPage.ClientScript.GetWebResourceUrl(type, resource);
				sPath = HttpUtility.HtmlEncode(sPath);
			} catch { }

			return sPath;
		}

		public static CarrotWebGrid<T> CarrotWebGrid<T>() where T : class {
			return new CarrotWebGrid<T>(Html);
		}

		public static CarrotWebGrid<T> CarrotWebGrid<T>(PagedData<T> dp) where T : class {
			return new CarrotWebGrid<T>(Html, dp);
		}

		public static CarrotWebDataTable CarrotWebDataTable() {
			return new CarrotWebDataTable(Html);
		}

		public static CarrotWebDataTable CarrotWebDataTable(PagedDataTable dp) {
			return new CarrotWebDataTable(Html, dp);
		}

		public static CarrotWebGrid<T> CarrotWebGrid<T>(List<T> lst) where T : class {
			PagedData<T> dp = new PagedData<T>();
			dp.DataSource = lst;
			dp.PageNumber = 1;
			dp.TotalRecords = lst.Count();

			var grid = new CarrotWebGrid<T>(Html, dp);
			grid.UseDataPage = false;

			return grid;
		}

		public static string DisplayNameFor<T>(Expression<Func<T, object>> expression) {
			string propertyName = String.Empty;
			PropertyInfo propInfo = null;

			MemberExpression memberExpression = expression.Body as MemberExpression ??
												((UnaryExpression)expression.Body).Operand as MemberExpression;
			if (memberExpression != null) {
				propertyName = memberExpression.Member.Name;
				Type t = memberExpression.Member.DeclaringType;
				propInfo = t.GetProperty(propertyName);
			}

			if (!String.IsNullOrEmpty(propertyName)) {
				DisplayAttribute attribute1 = propInfo.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute;
				if (attribute1 != null) {
					return attribute1.Name;
				}

				DisplayNameAttribute attribute2 = propInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;
				if (attribute2 != null) {
					return attribute2.DisplayName;
				}
			}

			return String.Empty;
		}

		public static HtmlString ValidationMultiMessageFor<T>(this HtmlHelper<T> htmlHelper,
			Expression<Func<T, Object>> property, object listAttributes = null, bool messageAsSpan = false) {
			MemberExpression memberExpression = property.Body as MemberExpression ??
									((UnaryExpression)property.Body).Operand as MemberExpression;

			// Static prop Html vs HtmlHelper<T>
			if (memberExpression != null) {
				string propertyName = propertyName = ReflectionUtilities.BuildProp<T>(property);

				ModelStateDictionary stateDictionary = htmlHelper.ViewData.ModelState;

				if (stateDictionary[propertyName] != null) {
					StringBuilder sb = new StringBuilder();
					sb.Append(String.Empty);
					string validationClass = "field-validation-valid";

					foreach (var err in stateDictionary[propertyName].Errors) {
						if (!String.IsNullOrEmpty(err.ErrorMessage.Trim())) {
							if (messageAsSpan) {
								sb.AppendLine(String.Format("<span>{0}</span> ", err.ErrorMessage.Trim()));
							} else {
								sb.AppendLine(String.Format("<li>{0}</li>", err.ErrorMessage.Trim()));
							}
							validationClass = "field-validation-error";
						}
					}

					TagBuilder msgBuilder = new TagBuilder("ul");
					if (messageAsSpan) {
						msgBuilder = new TagBuilder("span");
					}

					var listAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(listAttributes);
					// can be overwritten
					msgBuilder.MergeAttribute("data-valmsg-replace", "true");

					// append if present, provide if none
					if (!listAttribs.ContainsKey("class")) {
						listAttribs.Add("class", validationClass);
					} else {
						string origCss = listAttribs["class"].ToString();
						listAttribs["class"] = String.Format("{0} {1}", validationClass, origCss.Trim());
					}

					msgBuilder.MergeAttributes(listAttribs);

					// force the data-valmsg-for value to match the property name
					msgBuilder.MergeAttribute("data-valmsg-for", propertyName);

					msgBuilder.InnerHtml = sb.ToString();

					return new HtmlString(msgBuilder.ToString());
				}
			}

			return new HtmlString(String.Empty);
		}

		public static MvcHtmlString MetaTag(string Name, string Content) {
			var metaTag = new TagBuilder("meta");
			metaTag.MergeAttribute("name", Name);
			metaTag.MergeAttribute("content", Content);

			return MvcHtmlString.Create(metaTag.ToString(TagRenderMode.SelfClosing));
		}

		public static MvcHtmlString ActionImage(string actionName,
													string controllerName,
													object routeValues,
													string imagePath,
													string imageAltText = "",
													object imageAttributes = null,
													object linkAttributes = null) {
			var url = new UrlHelper(Html.ViewContext.RequestContext);

			var anchorBuilder = new TagBuilder("a");
			anchorBuilder.MergeAttribute("href", url.Action(actionName, controllerName, routeValues));

			var lnkAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(linkAttributes);
			anchorBuilder.MergeAttributes(lnkAttribs);

			var imgBuilder = new TagBuilder("img");
			imgBuilder.MergeAttribute("src", url.Content(imagePath));
			imgBuilder.MergeAttribute("alt", imageAltText);
			imgBuilder.MergeAttribute("title", imageAltText);

			var imgAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(imageAttributes);
			imgBuilder.MergeAttributes(imgAttribs);

			string imgHtml = imgBuilder.ToString(TagRenderMode.SelfClosing);

			anchorBuilder.InnerHtml = imgHtml;

			return MvcHtmlString.Create(anchorBuilder.ToString());
		}

		public static WrappedItem BeginWrappedItem(string tag,
					string actionName, string controllerName,
					object activeAttributes = null, object inactiveAttributes = null) {
			return new WrappedItem(Html, tag, actionName, controllerName, activeAttributes, inactiveAttributes);
		}

		public static WrappedItem BeginWrappedItem(string tag,
							int currentPage, int selectedPage,
							object activeAttributes = null, object inactiveAttributes = null) {
			return new WrappedItem(Html, tag, currentPage, selectedPage, activeAttributes, inactiveAttributes);
		}

		public static WrappedItem BeginWrappedItem(string tag, object htmlAttributes = null) {
			return new WrappedItem(Html, tag, htmlAttributes);
		}

		public static MvcHtmlString ImageSizer(string ImageUrl, string Title, int ThumbSize, bool ScaleImage, object imageAttributes = null) {
			ImageSizer img = new ImageSizer();
			img.ImageUrl = ImageUrl;
			img.Title = Title;
			img.ThumbSize = ThumbSize;
			img.ScaleImage = ScaleImage;
			img.imageAttributes = imageAttributes;

			return new MvcHtmlString(img.ToHtmlString());
		}

		//================================
		public static HtmlString RenderControlToHtml(IWebComponent ctrl) {
			return new HtmlString(ctrl.GetHtml());
		}

		public static HtmlString RenderTwoPartControlBody(ITwoPartWebComponent ctrl) {
			return new HtmlString(ctrl.GetBody());
		}

		public static HtmlString RenderTwoPartControlBodyCss(ITwoPartWebComponent ctrl) {
			return new HtmlString(ctrl.GetHead());
		}
	}
}