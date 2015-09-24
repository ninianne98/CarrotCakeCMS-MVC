using Carrotware.CMS.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;

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

	public class SearchForm : IDisposable {
		protected HtmlHelper _helper;

		public SearchForm(HtmlHelper helper, PagePayload page, object formAttributes = null) {
			_helper = helper;

			var frmID = new TagBuilder("input");
			frmID.MergeAttribute("type", "hidden");
			frmID.MergeAttribute("name", "form_type");
			frmID.MergeAttribute("value", "SearchForm");

			var frmBuilder = new TagBuilder("form");
			frmBuilder.MergeAttribute("action", page.TheSite.SiteSearchPath);
			frmBuilder.MergeAttribute("method", "GET");

			//frmBuilder.MergeAttribute("action", page.ThePage.FileName);
			//frmBuilder.MergeAttribute("method", "POST");

			var frmAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(formAttributes);
			frmBuilder.MergeAttributes(frmAttribs);

			string frmTag = frmBuilder.ToString(TagRenderMode.StartTag)
				//+ Environment.NewLine
				//+ helper.AntiForgeryToken().ToString()  // only put in if using a post
				//+ Environment.NewLine
				//+ frmID.ToString(TagRenderMode.SelfClosing)
							+ Environment.NewLine;

			_helper.ViewContext.Writer.Write(frmTag);
		}

		public HtmlHelper<SiteSearch> GetModelHelper() {
			SiteSearch model = new SiteSearch();
			if (_helper.ViewData["CMS_searchform"] != null) {
				model = _helper.ViewData["CMS_searchform"] as SiteSearch;
			} else {
				model = new SiteSearch();
			}

			return new HtmlHelper<SiteSearch>(_helper.ViewContext, new WrapperForHtmlHelper<SiteSearch>(model));
		}

		public void Dispose() {
			_helper.ViewContext.Writer.Write("</form>");
		}
	}

	//=======
	public class SiteSearch {

		[StringLength(128)]
		public string query { get; set; }
	}

	//======================

	//public class ContactForm : IDisposable {
	//	protected HtmlHelper _helper;

	//	public ContactForm(HtmlHelper helper, PagePayload page, object formAttributes = null) {
	//		_helper = helper;

	//		var frmID = new TagBuilder("input");
	//		frmID.MergeAttribute("type", "hidden");
	//		frmID.MergeAttribute("name", "form_type");
	//		frmID.MergeAttribute("value", "ContactForm");

	//		var frmBuilder = new TagBuilder("form");
	//		frmBuilder.MergeAttribute("action", page.ThePage.FileName);
	//		frmBuilder.MergeAttribute("method", "POST");

	//		var frmAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(formAttributes);
	//		frmBuilder.MergeAttributes(frmAttribs);

	//		string frmTag = frmBuilder.ToString(TagRenderMode.StartTag)
	//						+ Environment.NewLine
	//						+ _helper.AntiForgeryToken().ToString()
	//						+ Environment.NewLine
	//						+ frmID.ToString(TagRenderMode.SelfClosing);

	//		_helper.ViewContext.Writer.Write(frmTag);
	//	}

	//	public HtmlHelper<ContactInfo> GetModelHelper() {
	//		ContactInfo model = new ContactInfo();

	//		//var lst = ModelBinders.Binders.Values.Where(x => x is GenericModelBinder).Cast<GenericModelBinder>().ToList();
	//		////.Where(x => x.BinderType is ContactInfo).ToList();

	//		//if (!lst.Any()) {
	//		//	var bind = new GenericModelBinder();
	//		//	bind.BinderType = typeof(ContactInfo);
	//		//	ModelBinders.Binders.Add(bind.BinderType, bind);
	//		//}

	//		if (_helper.ViewData["CMS_contactform"] != null) {
	//			model = _helper.ViewData["CMS_contactform"] as ContactInfo;

	//		} else {
	//			model = new ContactInfo();
	//		}

	//		return new HtmlHelper<ContactInfo>(_helper.ViewContext, new WrapperForHtmlHelper<ContactInfo>(model, _helper.ViewData));
	//	}

	//	public void Dispose() {
	//		_helper.ViewContext.Writer.Write("</form>");
	//	}
	//}

	//======================

	public class AjaxContactForm : IDisposable {
		protected AjaxHelper _helper;
		protected MvcForm frm = null;

		public AjaxContactForm(AjaxHelper ajaxHelper, PagePayload page, AjaxOptions ajaxOptions, object formAttributes = null) {
			_helper = ajaxHelper;

			if (page == null) {
				page = PagePayload.GetCurrentContent();
			}

			if (ajaxOptions == null) {
				ajaxOptions = new AjaxOptions();
				ajaxOptions.InsertionMode = InsertionMode.Replace;
			}
			if (String.IsNullOrEmpty(ajaxOptions.HttpMethod)) {
				ajaxOptions.HttpMethod = "POST";
			}
			if (String.IsNullOrEmpty(ajaxOptions.UpdateTargetId)) {
				ajaxOptions.UpdateTargetId = "frmContact";
			}
			if (String.IsNullOrEmpty(ajaxOptions.OnFailure)) {
				ajaxOptions.OnFailure = "__OnAjaxRequestFailure";
			}

			frm = ajaxHelper.BeginRouteForm("Default", new { controller = "CmsAjaxForms", action = "Contact.ashx" }, ajaxOptions, formAttributes);
		}

		public HtmlHelper<ContactInfo> GetModelHelper(string partialName) {
			ContactInfo model = new ContactInfo();

			if (_helper.ViewData["CMS_contactform"] != null) {
				model = _helper.ViewData["CMS_contactform"] as ContactInfo;
			} else {
				model = new ContactInfo();
			}

			var hlp = new HtmlHelper<ContactInfo>(_helper.ViewContext, new WrapperForHtmlHelper<ContactInfo>(model, _helper.ViewData));

			//TODO: figure out a way to binhex and include/parse
			model.Uri = CarrotCakeHtml.CmsPage.ThePage.FileName;
			model.PostPartialName = partialName;

			string frmTag = Environment.NewLine
						+ hlp.AntiForgeryToken().ToString()
						+ Environment.NewLine
						+ hlp.HiddenFor(x => x.Uri).ToString()
						+ Environment.NewLine
						+ hlp.HiddenFor(x => x.PostPartialName).ToString()
						+ Environment.NewLine;

			_helper.ViewContext.Writer.Write(frmTag);

			return hlp;
		}

		public void Dispose() {
			//_helper.ViewContext.Writer.Write("</form>");
			if (frm != null) {
				frm.Dispose();
			}
		}
	}

	//=======

	public class ContactInfo {

		public ContactInfo() { }

		public Guid Root_ContentID { get; set; }
		public DateTime CreateDate { get; set; }

		[StringLength(32)]
		[Display(Name = "IP")]
		public string CommenterIP { get; set; }

		[StringLength(256)]
		[Required]
		[Display(Name = "Name")]
		public string CommenterName { get; set; }

		public string PostPartialName { get; set; }
		public string Uri { get; set; }

		[StringLength(256)]
		[Required]
		[Display(Name = "Email")]
		public string CommenterEmail { get; set; }

		[StringLength(256)]
		[Required]
		[Display(Name = "URL")]
		public string CommenterURL { get; set; }

		[StringLength(4096)]
		[Required]
		[Display(Name = "Comment")]
		public string PostCommentText { get; set; }
	}

	//======================

	public class FormHelper {

		public static Object ParseRequest(Object obj, HttpRequestBase request) {
			Type type = obj.GetType();

			PropertyInfo[] props = type.GetProperties();

			foreach (var p in props) {
				if (request.Form[p.Name] != null) {
					string val = request.Form[p.Name];
					object o = null;

					if (val != null) {
						Type tp = p.PropertyType;
						tp = Nullable.GetUnderlyingType(tp) ?? tp;

						if (tp == typeof(Guid)) {
							o = new Guid(val.ToString());
						} else {
							o = Convert.ChangeType(val, tp);
						}

						p.SetValue(obj, o);
					}
				}
			}

			return obj;
		}

		//========================

		public static T ParseRequest<T>(T obj, HttpRequestBase request) {
			Type type = typeof(T);

			PropertyInfo[] props = type.GetProperties();

			foreach (var p in props) {
				if (request.Form[p.Name] != null) {
					string val = request.Form[p.Name];
					object o = null;

					if (val != null) {
						Type tp = p.PropertyType;
						tp = Nullable.GetUnderlyingType(tp) ?? tp;

						if (tp == typeof(Guid)) {
							o = new Guid(val.ToString());
						} else {
							o = Convert.ChangeType(val, tp);
						}

						p.SetValue(obj, o);
					}
				}
			}

			return obj;
		}
	}

	//==================================================

	public class GenericModelBinder : IModelBinder {
		//public T BindModel<T>(ControllerContext controllerContext, ModelBindingContext bindingContext) {
		//	var request = controllerContext.HttpContext.Request;

		//	T obj = Activator.CreateInstance<T>();

		//	obj = FormHelper.ParseRequest<T>(obj, request);

		//	return obj;
		//}

		public Type BinderType { get; set; }

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
			this.BinderType = bindingContext.ModelType;
			object model = Activator.CreateInstance(this.BinderType);
			var request = controllerContext.HttpContext.Request;

			model = FormHelper.ParseRequest(model, request);

			return model;
		}
	}
}