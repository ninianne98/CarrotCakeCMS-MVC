using Carrotware.CMS.Interface.Controllers;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Interface {

	public static class RenderWidgetHelper {

		public static Controller CreateController<T>(this Controller source, string actionName, string areaName, object widgetPayload) where T : Controller, new() {
			Controller controller = null;

			T ctrl = new T();
			if (ctrl is Controller) {
				controller = ctrl;
			}

			var type = controller.GetType();
			var routeData = new RouteData();

			var wrapper = new HttpContextWrapper(HttpContext.Current);
			string controlerName = controller.GetControllerName();

			//if (string.IsNullOrWhiteSpace(areaName)) {
			//	routeData.Values[RouteInfo.Keys.Area] = string.Empty;
			//} else {
			//	routeData.Values[RouteInfo.Keys.Area] = areaName;
			//}

			//routeData.Values[RouteInfo.Keys.Action] = actionName;
			//routeData.Values[RouteInfo.Keys.Controller] = controlerName;

			routeData.SetRouteValues(areaName, controlerName, actionName, null);

			foreach (var r in source.RouteData.Values.Where(x => RouteInfo.Keys.GetStandardKeys().Contains(x.Key.ToLowerInvariant()) == false)) {
				routeData.Values[r.Key] = r.Value;
			}

			var context = new ControllerContext(wrapper, routeData, controller);
			controller.ControllerContext = context;

			if (controller is BaseDataWidgetController) {
				((BaseDataWidgetController)controller).WidgetPayload = widgetPayload;
			}

			return controller;
		}

		public static PartialViewResult ExecuteAction(this Controller controller) {
			var type = controller.GetType();
			var routeData = controller.RouteData;
			var routeInfo = routeData.GetRouteInfo();
			var actionName = routeInfo.Action;

			MethodInfo methodInfo = null;
			List<MethodInfo> mthds = type.GetMethods().Where(x => x.Name.ToLowerInvariant() == actionName.ToLowerInvariant()).ToList();

			// because there might be an overload, get the GET version if there is more than one
			if (mthds.Count <= 1) {
				methodInfo = mthds.FirstOrDefault();
			} else {
				methodInfo = mthds.Where(x => x.GetCustomAttributes(typeof(HttpGetAttribute), true).Any()).FirstOrDefault();
			}

			PartialViewResult partialResult = null;

			if (methodInfo != null) {
				object result = null;
				ParameterInfo[] parameters = methodInfo.GetParameters();

				if (parameters.Length == 0) {
					result = methodInfo.Invoke(controller, null);
					partialResult = (PartialViewResult)result;
				} else {
					List<object> parametersArray = new List<object>();

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

					result = methodInfo.Invoke(controller, parametersArray.ToArray());
					partialResult = (PartialViewResult)result;
				}
			}

			return partialResult;
		}

		public static string ResultToString(this ViewResultBase partialResult, Controller controller, string viewName = null) {
			return ResultToString(controller, partialResult, viewName);
		}

		public static HtmlString ResultToHtmlString(this Controller controller, ViewResultBase partialResult, string viewName = null) {
			var result = ResultToString(controller, partialResult, viewName);
			return new HtmlString(result);
		}

		public static string ResultToString(this Controller controller, ViewResultBase partialResult, string viewName = null) {
			var context = controller.ControllerContext;
			var sb = new StringBuilder();

			if (string.IsNullOrEmpty(viewName)) {
				var routeInfo = context.RouteData.GetRouteInfo();
				viewName = routeInfo.Action;
			}

			if (partialResult != null) {
				var model = partialResult.Model;
				if (model != null) {
					context.Controller.ViewData.Model = model;
				}

				var actualViewName = string.IsNullOrWhiteSpace(partialResult.ViewName) ? viewName : partialResult.ViewName;
				var viewEngineResult = ViewEngines.Engines.FindPartialView(context, actualViewName);
				var view = viewEngineResult.View;

				using (var sw = new StringWriter(sb)) {
					var ctx = new ViewContext(context, view, context.Controller.ViewData, context.Controller.TempData, sw);
					view.Render(ctx, sw);
				}
			}

			return sb.ToString();
		}

		public static string RenderViewToString(this Controller controller, object result, string viewName = null) {
			object model = new object();
			bool partialView = false;

			if (string.IsNullOrEmpty(viewName)) {
				var routeInfo = controller.ControllerContext.RouteData.GetRouteInfo();
				viewName = routeInfo.Action;
			}

			if (result is PartialViewResult) {
				partialView = true;
				model = ((PartialViewResult)result).Model;
			} else {
				// also covers ViewResult
				if (result is ViewResultBase) {
					model = ((ViewResultBase)result).Model;
				}
			}

			return RenderViewToString(controller.ControllerContext, model, viewName, partialView);
		}

		public static string RenderViewToString(this ControllerContext context, object model, string viewName = null, bool partialView = false) {
			// first find the ViewEngine for this view
			ViewEngineResult viewEngineResult = null;
			var sb = new StringBuilder();

			if (string.IsNullOrEmpty(viewName)) {
				var routeInfo = context.RouteData.GetRouteInfo();
				viewName = routeInfo.Action;
			}

			if (partialView) {
				viewEngineResult = ViewEngines.Engines.FindPartialView(context, viewName);
			} else {
				viewEngineResult = ViewEngines.Engines.FindView(context, viewName, null);
			}

			if (model != null) {
				context.Controller.ViewData.Model = model;
			}

			// get the view and attach the model to view data
			var view = viewEngineResult.View;

			using (var sw = new StringWriter(sb)) {
				var ctx = new ViewContext(context, view, context.Controller.ViewData, context.Controller.TempData, sw);
				view.Render(ctx, sw);
			}

			return sb.ToString();
		}
	}
}