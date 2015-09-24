using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Routing;
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

	public static class CarrotCakeHtml {

		public static HtmlHelper Html {
			get { return Page.Html; }
		}

		public static AjaxHelper Ajax {
			get { return Page.Ajax; }
		}

		public static UrlHelper Url {
			get { return Page.Url; }
		}

		public static WebViewPage Page {
			get { return ((WebViewPage)WebPageContext.Current.Page); }
		}

		public static PagePayload CmsPage {
			get {
				if (Page.ViewData[PagePayload.ViewDataKey] != null) {
					return (PagePayload)Page.ViewData[PagePayload.ViewDataKey];
				}
				//if (Page.Model is PagePayload) {
				//	return (PagePayload)Page.Model;
				//}
				if (Page is CmsWebViewPage) {
					return ((CmsWebViewPage)Page).CmsPage;
				}

				Page.ViewData[PagePayload.ViewDataKey] = PagePayload.GetCurrentContent();

				return (PagePayload)Page.ViewData[PagePayload.ViewDataKey];
			}
		}

		public static HtmlString RenderPartialFromController(string partialViewName, string controllerClass, Object model) {
			Type type = Type.GetType(controllerClass);
			Object obj = Activator.CreateInstance(type);

			if (obj is Controller) {
				Controller controller = (Controller)obj;
				controller.ControllerContext = Html.ViewContext.Controller.ControllerContext;

				if (model != null) {
					controller.ViewData.Model = model;
				}

				using (var sw = new StringWriter()) {
					ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, partialViewName);
					ViewContext viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);

					// copy model state items to the html helper
					foreach (var item in viewContext.Controller.ViewData.ModelState)
						if (!viewContext.ViewData.ModelState.Keys.Contains(item.Key)) {
							viewContext.ViewData.ModelState.Add(item);
						}

					viewResult.View.Render(viewContext, sw);

					return new HtmlString(sw.GetStringBuilder().ToString());
				}
			}

			return new HtmlString(String.Empty);
		}

		public static HtmlString RenderResultViewFromController(string actionName, string controllerClass) {
			Type type = Type.GetType(controllerClass);
			Object obj = Activator.CreateInstance(type);

			return new HtmlString(GetResultViewStringFromController(actionName, type, obj));
		}

		private static void AddUpdateRouting(RouteData routeData, string key, string value) {
			string keyLower = key.ToLower();
			if (routeData.Values.ContainsKey(keyLower)) {
				routeData.Values[keyLower] = value;
			} else {
				routeData.Values.Add(keyLower, value);
			}
		}

		private static string GetResultViewStringFromController(string actionName, Type type, Object obj) {
			bool IsPost = Html.ViewContext.HttpContext.Request.HttpMethod.ToUpper() == "POST";

			if (obj is Controller) {
				MethodInfo methodInfo = null;
				Controller controller = (Controller)obj;
				controller.ControllerContext = Html.ViewContext.Controller.ControllerContext;
				RouteData routeData = controller.ControllerContext.RouteData;
				string areaName = type.Assembly.ManifestModule.Name;
				areaName = areaName.Substring(0, areaName.Length - 4);

				AddUpdateRouting(routeData, "Controller", type.Name.ToLower().Replace("controller", String.Empty));
				AddUpdateRouting(routeData, "Action", actionName);
				AddUpdateRouting(routeData, "Area", areaName);

				List<MethodInfo> mthds = type.GetMethods().Where(x => x.Name == actionName).ToList();
				if (mthds.Count <= 1) {
					methodInfo = mthds.FirstOrDefault();
				} else {
					if (!IsPost) {
						methodInfo = mthds.Where(x => x.GetCustomAttributes(typeof(HttpGetAttribute), true).Any()).FirstOrDefault();
						if (methodInfo == null) {
							methodInfo = mthds.Where(x => !x.GetCustomAttributes(typeof(HttpPostAttribute), true).Any()).FirstOrDefault();
						}
					} else {
						methodInfo = mthds.Where(x => x.GetCustomAttributes(typeof(HttpPostAttribute), true).Any()).FirstOrDefault();
						if (methodInfo == null) {
							methodInfo = mthds.Where(x => !x.GetCustomAttributes(typeof(HttpGetAttribute), true).Any()).FirstOrDefault();
						}
					}
				}

				string currentAction = routeData.GetRequiredString("action");
				string currentController = routeData.GetRequiredString("controller");
				var collect = controller.ViewEngineCollection;

				if (methodInfo != null) {
					Object result = null;
					ParameterInfo[] parameters = methodInfo.GetParameters();

					if (parameters.Length == 0) {
						result = methodInfo.Invoke(controller, null);
					} else {
						List<object> parametersArray = new List<object>();

						if (!IsPost || parameters.Length > 1) {
							foreach (ParameterInfo parm in parameters) {
								object val = null;

								if (routeData.Values[parm.Name] != null) {
									val = routeData.Values[parm.Name];
								}
								if (val == null && controller.Request.QueryString[parm.Name] != null) {
									val = controller.Request.QueryString[parm.Name];
								}

								if (val != null) {
									object o = null;
									Type tp = parm.ParameterType;
									tp = Nullable.GetUnderlyingType(tp) ?? tp;

									if (tp == typeof(Guid)) {
										o = new Guid(val.ToString());
									} else {
										o = Convert.ChangeType(val, tp);
									}

									parametersArray.Add(o);
								} else {
									parametersArray.Add(null);
								}
							}
						} else {
							if (parameters.Length == 1) {
								var o = controller.ViewData.Model;
								parametersArray.Add(o);
							}
						}

						result = methodInfo.Invoke(controller, parametersArray.ToArray());
					}

					if (result is PartialViewResult) {
						PartialViewResult res = (PartialViewResult)result;

						Html.ViewContext.Controller.ViewData[actionName] = res.ViewData;
						Html.ViewContext.Controller.TempData[actionName] = res.TempData;

						if (String.IsNullOrEmpty(res.ViewName)) {
							res.ViewName = actionName;
						}

						string resultString = RenderView(controller.ControllerContext, res);
						controller.Dispose();

						return resultString;
					}
				}
			}

			return String.Empty;
		}

		private static string RenderView(ControllerContext ctrlCtx, PartialViewResult result) {
			string currentAction = ctrlCtx.RouteData.GetRequiredString("action");
			string currentController = ctrlCtx.RouteData.GetRequiredString("controller");

			using (var sw = new StringWriter()) {
				result.View = ViewEngines.Engines.FindPartialView(ctrlCtx, result.ViewName).View;
				ViewContext vc = new ViewContext(ctrlCtx, result.View, result.ViewData, result.TempData, sw);
				result.View.Render(vc, sw);
				return sw.GetStringBuilder().ToString();
			}
		}

		private static string RenderPartialToString(string partialViewName) {
			return RenderPartialToString(partialViewName, null);
		}

		private static string RenderPartialToString(string partialViewName, Object model) {
			var controller = Html.ViewContext.Controller;
			var viewData = Html.ViewData;
			var tempData = Html.ViewContext.TempData;

			if (model != null) {
				controller.ViewData.Model = model;
			}

			using (var sw = new StringWriter()) {
				var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, partialViewName);
				var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, viewData, tempData, sw);
				viewResult.View.Render(viewContext, sw);
				return sw.GetStringBuilder().ToString();
			}
		}

		public static HtmlString MetaTags() {
			StringBuilder sb = new StringBuilder();

			if (CmsPage.TheSite.BlockIndex || CmsPage.ThePage.BlockIndex) {
				sb.AppendLine(CarrotWeb.MetaTag("robots", "noindex,nofollow,noarchive").ToString());
				sb.AppendLine(String.Empty);
			}

			if (!String.IsNullOrEmpty(CmsPage.ThePage.MetaKeyword)) {
				sb.AppendLine(CarrotWeb.MetaTag("keywords", CmsPage.ThePage.MetaKeyword).ToString());
				sb.AppendLine(String.Empty);
			}
			if (!String.IsNullOrEmpty(CmsPage.ThePage.MetaDescription)) {
				sb.AppendLine(CarrotWeb.MetaTag("description", CmsPage.ThePage.MetaDescription).ToString());
				sb.AppendLine(String.Empty);
			}

			sb.AppendLine(CarrotWeb.MetaTag("generator", SiteData.CarrotCakeCMSVersion).ToString());
			sb.AppendLine(String.Empty);

			return new HtmlString(sb.ToString());
		}

		public static string CurrentViewName {
			get {
				//return System.IO.Path.GetFileNameWithoutExtension(((RazorView)Html.ViewContext.View).ViewPath);
				return ((RazorView)Html.ViewContext.View).ViewPath;
			}
		}

		public static void HandleTemplatePath(Controller controller, string templateFile) {
			string folderPath = templateFile.Substring(0, templateFile.LastIndexOf("/"));

			List<CmsTemplateViewEngine> lst = controller.ViewEngineCollection
				.Where(x => x is CmsTemplateViewEngine).Cast<CmsTemplateViewEngine>()
				.Where(x => x.ThemeFile.ToLower() == templateFile.ToLower()
					|| x.ThemeFile.ToLower().StartsWith(folderPath.ToLower())).ToList();

			if (!lst.Any()) {
				CmsTemplateViewEngine ve = new CmsTemplateViewEngine(templateFile);
				controller.ViewEngineCollection.Add(ve);
			}
		}

		public static string SiteMapUri {
			get { return "/sitemap.ashx"; }
		}

		public static string RssUri {
			get { return "/rss.ashx"; }
		}

		public static HtmlString Rss(SiteData.RSSFeedInclude mode) {
			return new HtmlString(String.Format("<!-- RSS Header Feed --> <link rel=\"alternate\" type=\"application/rss+xml\" title=\"RSS Feed\" href=\"{0}?type={1}\" /> ", CarrotCakeHtml.RssUri, mode));
		}

		public static HtmlString Rss() {
			return Rss(SiteData.RSSFeedInclude.BlogAndPages);
		}

		public static MvcHtmlString RssLink(string imagePath = "",
									string imageAltText = "RSS",
									object imageAttributes = null,
									object linkAttributes = null) {
			return RssLink(SiteData.RSSFeedInclude.BlogAndPages, imagePath, imageAltText, imageAttributes, linkAttributes);
		}

		public static MvcHtmlString RssLink(SiteData.RSSFeedInclude mode,
											string imagePath = "",
											string imageAltText = "RSS",
											object imageAttributes = null,
											object linkAttributes = null) {
			var url = new UrlHelper(Html.ViewContext.RequestContext);

			var anchorBuilder = new TagBuilder("a");
			anchorBuilder.MergeAttribute("href", String.Format("{0}?type={1}", CarrotCakeHtml.RssUri, mode));

			var lnkAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(linkAttributes);
			anchorBuilder.MergeAttributes(lnkAttribs);

			if (String.IsNullOrEmpty(imagePath)) {
				imagePath = ControlUtilities.GetWebResourceUrl("Carrotware.CMS.UI.Components.feed.png");
			}

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

		public static MvcHtmlString RssTextLink(string linkText = "RSS", object linkAttributes = null) {
			return RssTextLink(SiteData.RSSFeedInclude.BlogAndPages, linkText, linkAttributes);
		}

		public static MvcHtmlString RssTextLink(SiteData.RSSFeedInclude mode, string linkText = "RSS", object linkAttributes = null) {
			var anchorBuilder = new TagBuilder("a");
			anchorBuilder.MergeAttribute("href", String.Format("{0}?type={1}", CarrotCakeHtml.RssUri, mode));

			var lnkAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(linkAttributes);
			anchorBuilder.MergeAttributes(lnkAttribs);

			anchorBuilder.InnerHtml = linkText;

			return MvcHtmlString.Create(anchorBuilder.ToString());
		}

		public static HtmlString IncludeHead() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(String.Empty);

			if (SecurityData.IsAdmin || SecurityData.IsSiteEditor) {
				if (SecurityData.AdvancedEditMode) {
					sb.AppendLine(RenderPartialToString(SiteFilename.AdvancedEditHeadControlPath));
				}
			}

			return new HtmlString(sb.ToString());
		}

		public static HtmlString IncludeFooter() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(String.Empty);
			bool IsPageTemplate = false;

			if (SecurityData.IsAdmin || SecurityData.IsSiteEditor) {
				if (SecurityData.AdvancedEditMode) {
					sb.AppendLine(RenderPartialToString(SiteFilename.AdvancedEditControlPath));
				} else {
					if (CmsPage.ThePage.Root_ContentID == SiteData.CurrentSiteID && SiteData.IsPageReal) {
						IsPageTemplate = true;
					}

					if (!SiteData.IsPageSampler && !IsPageTemplate) {
						sb.AppendLine(RenderPartialToString(SiteFilename.EditNotifierControlPath));
					}
				}
			}

			return new HtmlString(sb.ToString());
		}

		public static string GenerateUrl() {
			ViewContext viewContext = Html.ViewContext;
			Uri url = Html.ViewContext.HttpContext.Request.Url;

			if (viewContext.RouteData.Values["id"] != null) {
				return String.Join("", url.Segments.Take(url.Segments.Length - 1));
			} else {
				return String.Join("", url.Segments) + @"/";
			}
		}

		public static SearchForm BeginSearchForm(object formAttributes = null) {
			return new SearchForm(Html, CmsPage, formAttributes);
		}

		//public static ContactForm BeginContactForm2(object formAttributes = null) {
		//	return new ContactForm(Html, CmsPage, formAttributes);
		//}

		public static AjaxContactForm BeginContactForm(AjaxOptions ajaxOptions, object formAttributes = null) {
			return new AjaxContactForm(Ajax, CmsPage, ajaxOptions, formAttributes);
		}

		public enum TextFieldZone {
			TextLeft,
			TextCenter,
			TextRight,
		}

		public static HtmlString RenderBody(TextFieldZone zone) {
			string bodyText = String.Empty;

			switch (zone) {
				case TextFieldZone.TextLeft:
					bodyText = CmsPage.ThePage.LeftPageText;
					break;

				case TextFieldZone.TextCenter:
					bodyText = CmsPage.ThePage.PageText;
					break;

				case TextFieldZone.TextRight:
					bodyText = CmsPage.ThePage.RightPageText;
					break;

				default:
					break;
			}

			bodyText = SiteData.CurrentSite.UpdateContent(bodyText);

			if (SecurityData.AdvancedEditMode) {
				AdvContentModel m = new AdvContentModel();
				m.Content = bodyText;
				m.AreaName = zone;
				switch (zone) {
					case TextFieldZone.TextLeft:
						m.Zone = "l";
						break;

					case TextFieldZone.TextCenter:
						m.Zone = "c";
						break;

					case TextFieldZone.TextRight:
						m.Zone = "r";
						break;
				}

				string sTextZone = ControlUtilities.GetManifestResourceStream("Carrotware.CMS.UI.Components._TextZone.cshtml");
				sTextZone = sTextZone.Replace("[[CONTENT]]", m.Content);
				sTextZone = sTextZone.Replace("[[AREA_NAME]]", m.AreaName.ToString());
				sTextZone = sTextZone.Replace("[[zone]]", m.Zone);

				bodyText = sTextZone;
			}

			return new HtmlString(bodyText);
		}

		public static HtmlString RenderWidget(string placeHolderName) {
			StringBuilder sb = new StringBuilder();
			string sWidgetZone = String.Empty;
			string masterWidgetWrapper = String.Empty;
			string widgetMenuTemplate = String.Empty;
			string sStatusTemplate = String.Empty;

			if (SecurityData.AdvancedEditMode) {
				widgetMenuTemplate = "<li id=\"liMenu\"><a href=\"javascript:[[JS_CALL]]\" id=\"cmsMenuEditLink\" class=\"cmsWidgetBarLink cmsWidgetBarIconPencil\" alt=\"[[CAP]]\" title=\"[[CAP]]\"> [[CAP]]</a></li>";
				sWidgetZone = ControlUtilities.GetManifestResourceStream("Carrotware.CMS.UI.Components._WidgetZone.cshtml");
				masterWidgetWrapper = ControlUtilities.GetManifestResourceStream("Carrotware.CMS.UI.Components._WidgetWrapper.cshtml");

				sWidgetZone = sWidgetZone.Replace("[[PLACEHOLDER]]", placeHolderName);
				masterWidgetWrapper = masterWidgetWrapper.Replace("[[PLACEHOLDER]]", placeHolderName);
			}

			int iWidgetCount = 0;
			foreach (Widget widget in CmsPage.TheWidgets
				.Where(x => x.PlaceholderName == placeHolderName)
				.OrderBy(x => x.WidgetOrder)) {
				bool IsWidgetClass = false;

				string widgetKey = String.Format("Widget_{0}_{1}", placeHolderName, iWidgetCount);
				iWidgetCount++;

				if (widget.ControlPath.Contains(":")) {
					string[] path = widget.ControlPath.Split(':');
					string objectPrefix = path[0];
					string objectClass = path[1];
					string altView = path.Length >= 3 ? path[2] : String.Empty;

					string widgetText = String.Empty;
					string widgetWrapper = String.Empty;

					Object settings = null;
					Object obj = null;
					Dictionary<string, string> lstMenus = new Dictionary<string, string>();

					try {
						Type objType = Type.GetType(objectClass);
						obj = Activator.CreateInstance(objType);

						if (objectPrefix.ToUpper() != "CLASS") {
							IsWidgetClass = false;
							// assumed to be a controller action/method
							Object attrib = ReflectionUtilities.GetAttribute<WidgetActionSettingModelAttribute>(objType, objectPrefix);

							if (attrib != null) {
								Type s = Type.GetType(((WidgetActionSettingModelAttribute)attrib).ClassName);
								settings = Activator.CreateInstance(s);
							}
						} else {
							IsWidgetClass = true;
							// a class widget is its own setting object
							settings = obj;
						}

						if (settings != null) {
							if (settings is IWidget) {
								IWidget w = (IWidget)settings;
								w.SiteID = CmsPage.TheSite.SiteID;
								w.RootContentID = widget.Root_ContentID;
								w.PageWidgetID = widget.Root_WidgetID;
								w.IsDynamicInserted = true;
								w.IsBeingEdited = SecurityData.AdvancedEditMode;
								w.WidgetClientID = widgetKey;
								if (!String.IsNullOrEmpty(altView)) {
									w.AlternateViewFile = altView;
								}

								List<WidgetProps> lstProp = widget.ParseDefaultControlProperties();
								w.PublicParmValues = lstProp.ToDictionary(t => t.KeyName, t => t.KeyValue);

								lstMenus = w.JSEditFunctions;

								if (!lstMenus.Any() && w.EnableEdit) {
									lstMenus.Add("Edit", "cmsGenericEdit('" + widget.Root_ContentID.ToString() + "','" + widget.Root_WidgetID.ToString() + "')");
								}
							}

							if (settings is IWidgetRawData) {
								IWidgetRawData w = settings as IWidgetRawData;
								w.RawWidgetData = widget.ControlProperties;
							}
						}

						if (obj != null && settings != null && obj is IWidgetDataObject) {
							((IWidgetDataObject)obj).WidgetPayload = settings;
						}

						if (IsWidgetClass && obj is IHtmlString) {
							widgetText = ((IHtmlString)obj).ToHtmlString();
						} else {
							widgetText = GetResultViewStringFromController(objectPrefix, objType, obj);
						}
					} catch (Exception ex) {
						LiteralMessage msg = new LiteralMessage(ex, widgetKey, widget.ControlPath);
						obj = msg;
						widgetText = msg.ToHtmlString();
					}

					if (SecurityData.AdvancedEditMode) {
						if (widget.IsWidgetActive) {
							sStatusTemplate = "<a href=\"javascript:cmsRemoveWidgetLink('[[ITEM_ID]]');\" id=\"cmsContentRemoveLink\" class=\"cmsWidgetBarLink cmsWidgetBarIconCross\" alt=\"Remove\" title=\"Remove\">  Disable</a>";
						} else {
							sStatusTemplate = "<a href=\"javascript:cmsActivateWidgetLink('[[ITEM_ID]]');\" id=\"cmsActivateWidgetLink\" class=\"cmsWidgetBarLink cmsWidgetBarIconActive\" alt=\"Activate\" title=\"Activate\">  Enable</a>";
						}

						widgetWrapper = masterWidgetWrapper;
						widgetWrapper = widgetWrapper.Replace("[[STATUS_LINK]]", sStatusTemplate);
						widgetWrapper = widgetWrapper.Replace("[[WIDGET_PATH]]", widget.ControlPath);
						widgetWrapper = widgetWrapper.Replace("[[sequence]]", widget.WidgetOrder.ToString());
						widgetWrapper = widgetWrapper.Replace("[[ITEM_ID]]", widget.Root_WidgetID.ToString());

						CMSPlugin plug = (from p in CmsPage.Plugins
										  where p.FilePath.ToLower() == widget.ControlPath.ToLower()
										  select p).FirstOrDefault();

						string captionPrefix = String.Empty;

						if (!widget.IsWidgetActive) {
							captionPrefix = String.Format("{0} {1}", CMSConfigHelper.InactivePagePrefix, captionPrefix);
						}
						if (widget.IsRetired) {
							captionPrefix = String.Format("{0} {1}", CMSConfigHelper.RetiredPagePrefix, captionPrefix);
						}
						if (widget.IsUnReleased) {
							captionPrefix = String.Format("{0} {1}", CMSConfigHelper.UnreleasedPagePrefix, captionPrefix);
						}
						if (widget.IsWidgetPendingDelete) {
							captionPrefix = String.Format("{0} {1}", CMSConfigHelper.PendingDeletePrefix, captionPrefix);
						}

						if (plug != null) {
							string sysControl = (plug.SystemPlugin ? "[CMS]" : String.Empty);
							widgetWrapper = widgetWrapper.Replace("[[WIDGET_CAPTION]]", String.Format("{0}  {1}  {2}", captionPrefix, plug.Caption, sysControl).Trim());
						} else {
							widgetWrapper = widgetWrapper.Replace("[[WIDGET_CAPTION]]", String.Format("{0}  UNTITLED", captionPrefix).Trim());
						}

						StringBuilder sbMenu = new StringBuilder();
						sbMenu.AppendLine();
						if (lstMenus != null) {
							foreach (var d in lstMenus) {
								sbMenu.AppendLine(widgetMenuTemplate.Replace("[[JS_CALL]]", d.Value).Replace("[[CAP]]", d.Key));
							}
						}

						widgetWrapper = widgetWrapper.Replace("[[MENU_ITEMS]]", sbMenu.ToString().Trim());
						widgetWrapper = widgetWrapper.Replace("[[WIDGET_CAPTION]]", widget.ControlPath + captionPrefix);

						widgetWrapper = widgetWrapper.Replace("[[CONTENT]]", widgetText);
					} else {
						widgetWrapper = widgetText;
					}

					sb.AppendLine(widgetWrapper);
				}
			}

			string bodyText = String.Empty;

			if (SecurityData.AdvancedEditMode) {
				bodyText = sWidgetZone.Replace("[[CONTENT]]", sb.ToString());
			} else {
				bodyText = sb.ToString();
			}

			return new HtmlString(bodyText);
		}
	}
}