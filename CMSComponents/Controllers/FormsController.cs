using Carrotware.CMS.Interface;
using Carrotware.CMS.Interface.Controllers;
using Carrotware.Web.UI.Components;
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

namespace Carrotware.CMS.UI.Components.Controllers {

	public class FormsController : BaseDataWidgetController {
		protected WidgetActionSettingModel _setting = new WidgetActionSettingModel();
		protected PagedCommentSettings _comment_settings = new PagedCommentSettings();
		protected PagedDataSettings _paged_settings = new PagedDataSettings();

		protected override void Initialize(RequestContext requestContext) {
			base.Initialize(requestContext);

			var path = requestContext.HttpContext.Request.Path;

			var routeInfo = requestContext.RouteData.GetRouteInfo();

			string action = routeInfo.Action.ToLowerInvariant();
			string controller = routeInfo.Controller.ToLowerInvariant();
			string area = routeInfo.Area.ToLowerInvariant();

			if (requestContext.RouteData != null) {
				requestContext.RouteData.Values.Remove(RouteInfo.Keys.Area);
			}

			if (this.WidgetPayload != null) {
				if (this.WidgetPayload is PagedCommentSettings) {
					_comment_settings = (PagedCommentSettings)this.WidgetPayload;
					_comment_settings.LoadData();
				}
				if (this.WidgetPayload is PagedDataSettings) {
					_paged_settings = (PagedDataSettings)this.WidgetPayload;
					_paged_settings.LoadData();
				}
				if (this.WidgetPayload is WidgetActionSettingModel) {
					_setting = (WidgetActionSettingModel)this.WidgetPayload;
				}
			}
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext) {
			base.OnActionExecuting(filterContext);

			var routeInfo = filterContext.RouteData.GetRouteInfo();
		}

		protected override void OnActionExecuted(ActionExecutedContext filterContext) {
			base.OnActionExecuted(filterContext);

			var routeInfo = filterContext.RouteData.GetRouteInfo();
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
		}

		internal PartialViewResult RenderPartial(object model) {
			ViewBag.CmsWidgetClientID = _setting.WidgetClientID;
			ViewBag.CmsPageWidgetID = _setting.PageWidgetID.ToString("N");

			if (model == null) {
				return PartialView(_setting.AlternateViewFile);
			} else {
				this.ViewData.Model = model;
				return PartialView(_setting.AlternateViewFile, model);
			}
		}

		[WidgetActionSettingModel(typeof(WidgetActionSettingModel))]
		public PartialViewResult ContentCommentForm() {
			_setting.AlternateViewFile = VirtualViewFileProvider.PartialViews.Contact;
			_setting.AlternateViewFile = VirtualViewFileProvider.RegisterView(_setting.AlternateViewFile);

			return RenderPartial(null);
		}

		[WidgetActionSettingModel(typeof(WidgetActionSettingModel))]
		public PartialViewResult LoginOutForm() {
			_setting.AlternateViewFile = VirtualViewFileProvider.PartialViews.Login;
			_setting.AlternateViewFile = VirtualViewFileProvider.RegisterView(_setting.AlternateViewFile);

			return RenderPartial(null);
		}

		[WidgetActionSettingModel(typeof(PagedDataSettings))]
		public PartialViewResult ShowPagedData() {
			_setting.AlternateViewFile = VirtualViewFileProvider.PartialViews.PagedData;
			_setting.AlternateViewFile = VirtualViewFileProvider.RegisterView(_setting.AlternateViewFile);
			_paged_settings.AlternateViewFile = _setting.AlternateViewFile;

			var model = new PagedDataModel(_paged_settings);

			return RenderPartial(model);
		}

		[WidgetActionSettingModel(typeof(PagedCommentSettings))]
		public PartialViewResult ShowPagedComments() {
			_setting.AlternateViewFile = VirtualViewFileProvider.PartialViews.PagedComments;
			_setting.AlternateViewFile = VirtualViewFileProvider.RegisterView(_setting.AlternateViewFile);
			_comment_settings.AlternateViewFile = _setting.AlternateViewFile;

			var model = new PagedCommentModel(_comment_settings);

			return RenderPartial(model);
		}
	}
}