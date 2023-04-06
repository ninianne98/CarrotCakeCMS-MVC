using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.WebPages;
using System.Web;
using System;

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

		public static string ShortDateFormatPattern {
			get {
				return "{0:" + ShortDatePattern + "}";
			}
		}

		public static string ShortDateTimeFormatPattern {
			get {
				return "{0:" + ShortDatePattern + "} {0:" + ShortTimePattern + "}";
			}
		}

		private static string _shortDatePattern = null;

		public static string ShortDatePattern {
			get {
				if (_shortDatePattern == null) {
					DateTimeFormatInfo _dtf = CultureInfo.CurrentCulture.DateTimeFormat;
					if (_dtf == null) {
						_dtf = CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat;
					}

					_shortDatePattern = _dtf.ShortDatePattern ?? "M/d/yyyy";
					_shortDatePattern = _shortDatePattern.Replace("MM", "M").Replace("dd", "d");
				}

				return _shortDatePattern;
			}
		}

		private static string _shortTimePattern = null;

		public static string ShortTimePattern {
			get {
				if (_shortTimePattern == null) {
					DateTimeFormatInfo _dtf = CultureInfo.CurrentCulture.DateTimeFormat;
					if (_dtf == null) {
						_dtf = CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat;
					}
					_shortTimePattern = _dtf.ShortTimePattern ?? "hh:mm tt";
				}

				return _shortTimePattern;
			}
		}

		public static string DateKey() {
			return GenerateTick(DateTime.UtcNow).ToString();
			//return DateKey(15);
		}

		public static string DateKey(int interval) {
			DateTime now = DateTime.UtcNow;
			TimeSpan d = TimeSpan.FromMinutes(interval);
			DateTime dt = new DateTime(((now.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
			byte[] dateStringBytes = Encoding.ASCII.GetBytes(dt.ToString("U"));

			return Convert.ToBase64String(dateStringBytes);
		}

		private static string _resourceBase = "carrotwarewebresource.ashx";

		private static long GenerateTick(DateTime dateIn) {
			int roundTo = 12;
			dateIn = dateIn.AddMinutes(-2);
			int iMin = roundTo * (dateIn.Minute / roundTo);

			DateTime dateOut1 = dateIn.AddMinutes(0 - dateIn.Minute).AddMinutes(iMin);

			var dateOut = new DateTime(dateOut1.Year, dateOut1.Month, dateOut1.Day, dateOut1.Hour, dateOut1.Minute, dateOut1.Minute, (2 * dateOut1.DayOfYear), DateTimeKind.Utc);

			return dateOut.Ticks;
		}

		private static string GetInternalResourceName(string resource) {
			if (resource.ToLowerInvariant().StartsWith("carrotware.web.ui")) {
				return resource;
			}

			return string.Format("Carrotware.Web.UI.Components.{0}", resource);
		}

		internal static string GetWebResourceUrl(string resource) {
			return GetWebResourceUrl(typeof(CarrotWeb), GetInternalResourceName(resource));
		}

		public static string GetWebResourceUrl(Type type, string resource) {
			string sUri = string.Empty;

			var asmb = type.Assembly.ManifestModule.Name;

			try {
				sUri = string.Format("/{0}?r={1}&t={2}", _resourceBase, HttpUtility.HtmlEncode(Utils.EncodeBase64(resource + ":" + asmb)), DateKey());
			} catch { }

			return sUri;
		}

		private static Assembly GetAssembly(Type type, string[] res) {
			var _assembly = Assembly.GetAssembly(type);
			if (res.Length > 1) {
				var dir = AppDomain.CurrentDomain.RelativeSearchPath;
				_assembly = Assembly.LoadFrom(Path.Combine(dir, res[1]));
			}

			return _assembly;
		}

		public static string GetManifestResourceText(Type type, string resource) {
			string returnText = null;
			var res = resource.Split(':');

			var _assembly = GetAssembly(type, res);

			using (var stream = new StreamReader(_assembly.GetManifestResourceStream(res[0]))) {
				returnText = stream.ReadToEnd();
			}

			return returnText;
		}

		public static byte[] GetManifestResourceBytes(Type type, string resource) {
			byte[] returnBytes = null;
			var res = resource.Split(':');

			var _assembly = GetAssembly(type, res);

			using (var stream = _assembly.GetManifestResourceStream(res[0])) {
				returnBytes = new byte[stream.Length];
				stream.Read(returnBytes, 0, returnBytes.Length);
			}

			return returnBytes;
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
			string propertyName = string.Empty;
			PropertyInfo propInfo = null;
			Type type = null;

			MemberExpression memberExpression = expression.Body as MemberExpression ??
												((UnaryExpression)expression.Body).Operand as MemberExpression;
			if (memberExpression != null) {
				propertyName = memberExpression.Member.Name;
				type = memberExpression.Member.DeclaringType;
				propInfo = type.GetProperty(propertyName);
			}

			if (!string.IsNullOrEmpty(propertyName) && type != null) {
				DisplayAttribute attribute1 = propInfo.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute;
				if (attribute1 != null) {
					return attribute1.Name;
				}

				DisplayNameAttribute attribute2 = propInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;
				if (attribute2 != null) {
					return attribute2.DisplayName;
				}

				MetadataTypeAttribute metadataType = (MetadataTypeAttribute)type.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
				if (metadataType != null) {
					PropertyInfo metaProp = metadataType.MetadataClassType.GetProperty(propInfo.Name);
					if (metaProp != null) {
						DisplayAttribute attribute3 = (DisplayAttribute)metaProp.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();
						if (attribute3 != null) {
							return attribute3.Name;
						}

						DisplayNameAttribute attribute4 = (DisplayNameAttribute)metaProp.GetCustomAttributes(typeof(DisplayNameAttribute), true).SingleOrDefault();
						if (attribute4 != null) {
							return attribute4.DisplayName;
						}
					}
				}
			}

			return string.Empty;
		}

		public static HtmlString ValidationMultiMessageFor<T>(this HtmlHelper<T> htmlHelper,
			Expression<Func<T, object>> property, object listAttributes = null, bool messageAsSpan = false) {
			MemberExpression memberExpression = property.Body as MemberExpression ??
									((UnaryExpression)property.Body).Operand as MemberExpression;

			// Static prop Html vs HtmlHelper<T>
			if (memberExpression != null) {
				string propertyName = propertyName = ReflectionUtilities.BuildProp<T>(property);

				ModelStateDictionary stateDictionary = htmlHelper.ViewData.ModelState;

				if (stateDictionary[propertyName] != null) {
					StringBuilder sb = new StringBuilder();
					sb.Append(string.Empty);
					string validationClass = "field-validation-valid";

					foreach (var err in stateDictionary[propertyName].Errors) {
						if (!string.IsNullOrEmpty(err.ErrorMessage.Trim())) {
							if (messageAsSpan) {
								sb.AppendLine(string.Format("<span>{0}</span> ", err.ErrorMessage.Trim()));
							} else {
								sb.AppendLine(string.Format("<li>{0}</li>", err.ErrorMessage.Trim()));
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
						listAttribs["class"] = string.Format("{0} {1}", validationClass, origCss.Trim());
					}

					msgBuilder.MergeAttributes(listAttribs);

					// force the data-valmsg-for value to match the property name
					msgBuilder.MergeAttribute("data-valmsg-for", propertyName);

					msgBuilder.InnerHtml = sb.ToString();

					return new HtmlString(msgBuilder.ToString());
				}
			}

			return new HtmlString(string.Empty);
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