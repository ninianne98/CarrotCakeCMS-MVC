using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using System.Xml.Linq;

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

		public static HttpContext Current {
			get {
				return HttpContext.Current;
			}
		}

		public static HttpRequest Request { get { return Current.Request; } }
		public static HttpResponse Response { get { return Current.Response; } }

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

		//================================
		public static string EncodeColor(Color color) {
			var colorCode = ColorTranslator.ToHtml(color);

			string sColor = string.Empty;
			if (!string.IsNullOrEmpty(colorCode)) {
				sColor = colorCode.ToLowerInvariant();
				sColor = sColor.Replace("#", string.Empty);
				sColor = sColor.Replace("HEX-", string.Empty);
				sColor = HttpUtility.HtmlEncode(sColor);
			}
			return sColor;
		}

		public static string DecodeColorString(string colorCode) {
			string sColor = string.Empty;
			if (!string.IsNullOrEmpty(colorCode)) {
				sColor = colorCode;
				sColor = HttpUtility.HtmlDecode(sColor);
				sColor = sColor.Replace("HEX-", string.Empty);
				if (!sColor.StartsWith("#")) {
					sColor = string.Format("#{0}", sColor);
				}
			}

			return sColor;
		}

		public static Color DecodeColor(string colorCode) {
			string sColor = DecodeColorString(colorCode);

			if (sColor.ToLowerInvariant().EndsWith("transparent")) {
				return Color.Transparent;
			}
			if (sColor == "#" || string.IsNullOrWhiteSpace(sColor)
					|| sColor.ToLowerInvariant().EndsWith("empty")) {
				return Color.Empty;
			}

			return ColorTranslator.FromHtml(sColor);
		}

		public static string HtmlFormat(StringBuilder input) {
			if (input != null) {
				return HtmlFormat(input.ToString());
			}

			return string.Empty;
		}

		public static string HtmlFormat(string input) {
			if (!string.IsNullOrWhiteSpace(input)) {
				bool autoAddTypes = false;
				var subs = new Dictionary<string, int>();
				subs.Add("ndash", 150);
				subs.Add("mdash", 151);
				subs.Add("nbsp", 153);
				subs.Add("trade", 153);
				subs.Add("copy", 169);
				subs.Add("reg", 174);
				subs.Add("laquo", 171);
				subs.Add("raquo", 187);
				subs.Add("lsquo", 145);
				subs.Add("rsquo", 146);
				subs.Add("ldquo", 147);
				subs.Add("rdquo", 148);
				subs.Add("bull", 149);
				subs.Add("amp", 38);
				subs.Add("quot", 34);

				var subs2 = new Dictionary<string, int>();
				subs2.Add("ndash", 150);
				subs2.Add("mdash", 151);
				subs2.Add("nbsp", 153);
				subs2.Add("trade", 153);
				subs2.Add("copy", 169);
				subs2.Add("reg", 174);
				subs2.Add("laquo", 171);
				subs2.Add("raquo", 187);
				subs2.Add("bull", 149);

				string docType = string.Empty;

				if (!input.ToLowerInvariant().StartsWith("<!doctype")) {
					autoAddTypes = true;

					docType = "<!DOCTYPE html [ ";
					foreach (var s in subs) {
						docType += string.Format(" <!ENTITY {0} \"&#{1};\"> ", s.Key, s.Value);
					}
					docType += " ]>".Replace("  ", " ");

					input = docType + Environment.NewLine + input;
				}

				var doc = XDocument.Parse(input);

				if (autoAddTypes) {
					var sb = new StringBuilder();
					sb.Append(doc.ToString().Replace(docType, string.Empty));

					foreach (var s in subs2) {
						sb.Replace(Convert.ToChar(s.Value).ToString(), string.Format("&{0};", s.Key));
					}

					return sb.ToString();
				}

				return doc.ToString();
			}

			return string.Empty;
		}

		//================================
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
			var asmb = type.Assembly;

			return GetWebResourceUrl(asmb, resource);
		}

		public static string GetWebResourceUrl(Assembly assembly, string resource) {
			string sUri = string.Empty;

			var asmb = assembly.ManifestModule.Name;
			var resName = HttpUtility.HtmlEncode(Utils.EncodeBase64(string.Format("{0}:{1}", resource, asmb)));

			try {
				var ver = assembly.GetName().Version.ToString().Replace(".", string.Empty);
				sUri = string.Format("{0}?r={1}&ts={2}-{3}", UrlPaths.ResourcePath, resName, ver, DateKey());
			} catch {
				sUri = string.Format("{0}?r={1}&ts={2}", UrlPaths.ResourcePath, resName, DateKey());
			}

			return sUri;
		}

		internal static Assembly GetAssembly(Type type, string resource) {
			return GetAssembly(type, resource.Split(':'));
		}

		internal static Assembly GetAssembly(Type type, string[] res) {
			if (res.Length > 1) {
				var dir = AppDomain.CurrentDomain.RelativeSearchPath;
				return Assembly.LoadFrom(Path.Combine(dir, res[1]));
			}

			return Assembly.GetAssembly(type);
		}

		internal static Assembly GetAssembly(string[] res) {
			return GetAssembly(typeof(CarrotWeb), res);
		}

		internal static Assembly GetAssembly(string resource) {
			return GetAssembly(typeof(CarrotWeb), resource);
		}

		internal static string GetManifestResourceText(string resource) {
			return GetManifestResourceText(typeof(CarrotWeb), GetInternalResourceName(resource));
		}

		internal static byte[] GetManifestResourceBytes(string resource) {
			return GetManifestResourceBytes(typeof(CarrotWeb), GetInternalResourceName(resource));
		}

		internal static string TrimAssemblyName(Assembly assembly) {
			var asmb = assembly.ManifestModule.Name;
			return asmb.Substring(0, asmb.Length - 4);
		}

		internal static string[] FixResourceName(Assembly assembly, string[] res) {
			if (res.Length > 1) {
				var asmbName = TrimAssemblyName(assembly);

				if (!res[0].StartsWith(asmbName)) {
					res[0] = string.Format("{0}.{1}", asmbName, res[0]);
				}
			}

			return res;
		}

		public static string GetManifestResourceText(Type type, string resource) {
			string returnText = null;
			var res = resource.Split(':');

			var assembly = GetAssembly(type, res);
			res = FixResourceName(assembly, res);

			using (var stream = new StreamReader(assembly.GetManifestResourceStream(res[0]))) {
				returnText = stream.ReadToEnd();
			}

			return returnText;
		}

		public static byte[] GetManifestResourceBytes(Type type, string resource) {
			byte[] returnBytes = null;
			var res = resource.Split(':');

			var assembly = GetAssembly(type, res);
			res = FixResourceName(assembly, res);

			using (var stream = assembly.GetManifestResourceStream(res[0])) {
				returnBytes = new byte[stream.Length];
				stream.Read(returnBytes, 0, returnBytes.Length);
			}

			return returnBytes;
		}

		//================================
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

		public static string GetActionName<T>(this T instance, Expression<Action<T>> expression) {
			var expressionBody = expression.Body;

			if (expressionBody == null) {
				throw new ArgumentException("Cannot be null.");
			}

			if ((expressionBody is MethodCallExpression) != true) {
				throw new ArgumentException("Methods only!");
			}

			if (expressionBody is MethodCallExpression) {
				var methodCallExpression = (MethodCallExpression)expressionBody;
				return methodCallExpression.Method.Name;
			}

			return string.Empty;
		}

		public static string GetPropertyName<T, P>(this T instance, Expression<Func<T, P>> expression) {
			var expressionBody = expression.Body;

			if (expressionBody == null) {
				throw new ArgumentException("Cannot be null.");
			}

			if ((expressionBody is MemberExpression) != true) {
				throw new ArgumentException("Properties only!");
			}

			if (expressionBody is MemberExpression) {
				// Reference type property or field
				var memberExpression = (MemberExpression)expressionBody;
				return memberExpression.Member.Name;
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

					var msgBuilder = new HtmlTag("ul");
					if (messageAsSpan) {
						msgBuilder = new HtmlTag("span");
					}

					// can be overwritten
					msgBuilder.MergeAttribute("data-valmsg-replace", "true");

					msgBuilder.MergeAttribute("class", validationClass);

					msgBuilder.MergeAttributes(listAttributes);

					// force the data-valmsg-for value to match the property name
					msgBuilder.MergeAttribute("data-valmsg-for", propertyName);

					msgBuilder.InnerHtml = sb.ToString();

					return new HtmlString(msgBuilder.ToString());
				}
			}

			return new HtmlString(string.Empty);
		}

		public static MvcHtmlString MetaTag(string Name, string Content) {
			var metaTag = new HtmlTag("meta");
			metaTag.MergeAttribute("name", Name);
			metaTag.MergeAttribute("content", Content);

			return MvcHtmlString.Create(metaTag.RenderSelfClosingTag());
		}

		public static MvcHtmlString ActionImage(string actionName,
													string controllerName,
													object routeValues,
													string imagePath,
													string imageAltText = "",
													object imageAttributes = null,
													object linkAttributes = null) {
			var url = new UrlHelper(Html.ViewContext.RequestContext);

			var anchorBuilder = new HtmlTag("a");
			anchorBuilder.Uri = url.Action(actionName, controllerName, routeValues);
			anchorBuilder.MergeAttributes(linkAttributes);

			var imgBuilder = new HtmlTag("img");
			imgBuilder.Uri = url.Content(imagePath);
			imgBuilder.MergeAttribute("alt", imageAltText);
			imgBuilder.MergeAttribute("title", imageAltText);
			imgBuilder.MergeAttributes(imageAttributes);

			string imgHtml = imgBuilder.RenderSelfClosingTag();

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
			img.ImageAttributes = imageAttributes;

			return new MvcHtmlString(img.ToHtmlString());
		}

		//================================

		public static string ToKebabCase(this string input) {
			return string.Concat(input.Select((c, i) => (char.IsUpper(c) && i > 0 ? "-" : string.Empty) + char.ToLower(c)));
		}

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