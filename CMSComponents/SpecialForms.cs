using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Mvc;
using System.Web;
using System.Xml.Serialization;
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
			return GetModelHelper(false);
		}

		public HtmlHelper<SiteSearch> GetModelHelper(bool restoreQuery) {
			var model = new SiteSearch();

			if (_helper.ViewData["CMS_searchform"] != null) {
				model = _helper.ViewData["CMS_searchform"] as SiteSearch;
			} else {
				model = new SiteSearch();
			}

			if (restoreQuery) {
				model.RestoreQueryString();
			}

			return new HtmlHelper<SiteSearch>(_helper.ViewContext, new WrapperForHtmlHelper<SiteSearch>(model));
		}

		public void Dispose() {
			_helper.ViewContext.Writer.Write("</form>");
		}
	}

	//==================================================

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

	//		if (_helper.ViewData[ContactInfo.Key] != null) {
	//			model = _helper.ViewData[ContactInfo.Key] as ContactInfo;

	//		} else {
	//			model = new ContactInfo();
	//		}

	//		return new HtmlHelper<ContactInfo>(_helper.ViewContext, new WrapperForHtmlHelper<ContactInfo>(model, _helper.ViewData));
	//	}

	//	public void Dispose() {
	//		_helper.ViewContext.Writer.Write("</form>");
	//	}
	//}

	//==================================================

	public class AjaxContactForm : IDisposable {
		protected AjaxHelper _helper;
		protected MvcForm frm = null;
		protected ContactInfo _model = null;
		protected ContactInfoSettings _settings = null;

		public AjaxContactForm(AjaxHelper ajaxHelper, PagePayload page, AjaxOptions ajaxOptions, object formAttributes = null) {
			_helper = ajaxHelper;

			if (page == null) {
				page = PagePayload.GetCurrentContent();
			}

			if (ajaxOptions == null) {
				ajaxOptions = new AjaxOptions();
				ajaxOptions.InsertionMode = InsertionMode.Replace;
			}
			if (string.IsNullOrEmpty(ajaxOptions.HttpMethod)) {
				ajaxOptions.HttpMethod = "POST";
			}
			if (string.IsNullOrEmpty(ajaxOptions.UpdateTargetId)) {
				ajaxOptions.UpdateTargetId = "frmContact";
			}
			if (string.IsNullOrEmpty(ajaxOptions.OnFailure)) {
				ajaxOptions.OnFailure = "__OnAjaxRequestFailure";
			}

			string formAction = "Contact.ashx";

			//try #3
			//RouteValueDictionary dic = new RouteValueDictionary();
			//dic.Add("controller", CmsRouteConstants.Controller.AjaxForms);
			//dic.Add("action", formAction);
			//if (SecurityData.AdvancedEditMode) {
			//	dic.Add(SiteData.AdvancedEditParameter, true);
			//}
			//frm = ajaxHelper.BeginRouteForm("Default", dic, ajaxOptions, formAttributes);

			//try #2
			if (SecurityData.AdvancedEditMode) {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = CmsRouteConstants.Controller.AjaxForms, action = formAction, carrotedit = true }, ajaxOptions, formAttributes);
			} else {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = CmsRouteConstants.Controller.AjaxForms, action = formAction }, ajaxOptions, formAttributes);
			}

			/*
			//try #1
			string formAction = "Contact.ashx";
			FormRouteValue frv = new FormRouteValue(CmsRouteConstants.Controller.AjaxForms, formAction);

			if (SecurityData.AdvancedEditMode) {
				frv = new FormRouteValue(CmsRouteConstants.Controller.AjaxForms, formAction, true);
			}

			frm = ajaxHelper.BeginRouteForm("Default", frv, ajaxOptions, formAttributes);
			 */
		}

		public HtmlHelper<ContactInfo> GetModelHelper(string partialName, IValidateHuman validateHuman) {
			_model = InitContactInfo(partialName);

			_settings.UseValidateHuman = true;
			_settings.ValidateHumanClass = validateHuman.GetType().AssemblyQualifiedName;
			if (!string.IsNullOrEmpty(validateHuman.AltValidationFailText)) {
				_settings.ValidationFailText = validateHuman.AltValidationFailText;
			}

			return InitHelp();
		}

		public HtmlHelper<ContactInfo> GetModelHelper(ContactInfoConfig config) {
			_model = InitContactInfo(config.PostPartialName);

			_settings.GetSettingFromConfig(config);

			_settings.DirectEmailKeyName = config.DirectEmailKeyName;
			_settings.NotifyEditors = config.NotifyEditors;

			return InitHelp();
		}

		public HtmlHelper<ContactInfo> GetModelHelper(string partialName) {
			_model = InitContactInfo(partialName);

			return InitHelp();
		}

		protected ContactInfo InitContactInfo(string partialName) {
			ContactInfo model = new ContactInfo();
			_settings = new ContactInfoSettings();

			if (_helper.ViewData[ContactInfo.Key] != null) {
				model = _helper.ViewData[ContactInfo.Key] as ContactInfo;
			} else {
				model = new ContactInfo();
			}

			_settings.Uri = CarrotCakeHtml.CmsPage.ThePage.FileName;
			_settings.PostPartialName = partialName;
			model.Settings = _settings;

			return model;
		}

		protected HtmlHelper<ContactInfo> InitHelp() {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ContactInfoSettings));
			string sXML = string.Empty;
			using (StringWriter stringWriter = new StringWriter()) {
				xmlSerializer.Serialize(stringWriter, _settings);
				sXML = stringWriter.ToString();
				sXML = CMSConfigHelper.EncodeBase64(sXML);
			}

			_model.Settings = _settings;
			_model.EncodedSettings = sXML;

			var hlp = new HtmlHelper<ContactInfo>(_helper.ViewContext, new WrapperForHtmlHelper<ContactInfo>(_model, _helper.ViewData));

			string frmTag = Environment.NewLine
						+ hlp.AntiForgeryToken().ToString()
						+ Environment.NewLine
						+ hlp.HiddenFor(x => x.EncodedSettings).ToString()
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

	//==================================================

	public class ContactInfoSettings : FormSettingBase {

		public ContactInfoSettings() : base() { }

		public string DirectEmailKeyName { get; set; }
		public bool NotifyEditors { get; set; }
	}

	//==================================================

	public class ContactInfoConfig : FormConfigBase {

		public ContactInfoConfig()
			: base() {
			InitStuff();
		}

		public ContactInfoConfig(string partialName)
			: base(partialName) {
			InitStuff();
		}

		public ContactInfoConfig(string partialName, IValidateHuman validateHuman)
			: base(partialName, validateHuman) {
			InitStuff();
		}

		protected void InitStuff() {
			this.NotifyEditors = false;
			this.DirectEmailKeyName = string.Empty;
		}

		public string DirectEmailKeyName { get; set; }
		public bool NotifyEditors { get; set; }
	}

	//==================================================

	public class AjaxLoginForm : IDisposable {
		protected AjaxHelper _helper;
		protected MvcForm frm = null;
		protected LoginInfo _model = null;
		protected LoginInfoSettings _settings = null;

		public AjaxLoginForm(AjaxHelper ajaxHelper, PagePayload page, AjaxOptions ajaxOptions, object formAttributes = null) {
			_helper = ajaxHelper;

			if (page == null) {
				page = PagePayload.GetCurrentContent();
			}

			if (ajaxOptions == null) {
				ajaxOptions = new AjaxOptions();
				ajaxOptions.InsertionMode = InsertionMode.Replace;
			}
			if (string.IsNullOrEmpty(ajaxOptions.HttpMethod)) {
				ajaxOptions.HttpMethod = "POST";
			}
			if (string.IsNullOrEmpty(ajaxOptions.UpdateTargetId)) {
				ajaxOptions.UpdateTargetId = "frmLogin";
			}
			if (string.IsNullOrEmpty(ajaxOptions.OnFailure)) {
				ajaxOptions.OnFailure = "__OnAjaxRequestFailure";
			}

			string formAction = "Login.ashx";

			if (SecurityData.AdvancedEditMode) {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = CmsRouteConstants.Controller.AjaxForms, action = formAction, carrotedit = true }, ajaxOptions, formAttributes);
			} else {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = CmsRouteConstants.Controller.AjaxForms, action = formAction }, ajaxOptions, formAttributes);
			}
		}

		public HtmlHelper<LoginInfo> GetModelHelper(string partialName, IValidateHuman validateHuman = null) {
			_model = InitLoginInfo(partialName);

			_settings.SetHuman(validateHuman);

			return InitHelp();
		}

		public HtmlHelper<LoginInfo> GetModelHelper(LoginInfoConfig config) {
			_model = InitLoginInfo(config.PostPartialName);

			_settings.GetSettingFromConfig(config);

			_settings.RedirectUri = config.RedirectUri;
			_settings.CodeRedirectUri = config.CodeRedirectUri;

			_settings.PostPartialNameLockout = config.PostPartialNameLockout;
			_settings.PostPartialNameVerification = config.PostPartialNameVerification;
			_settings.PostPartialNameFailure = config.PostPartialNameFailure;

			return InitHelp();
		}

		public HtmlHelper<LoginInfo> GetModelHelper(string partialName) {
			_model = InitLoginInfo(partialName);

			return InitHelp();
		}

		protected LoginInfo InitLoginInfo(string partialName) {
			LoginInfo model = new LoginInfo();
			_settings = new LoginInfoSettings();

			if (_helper.ViewData[LoginInfo.Key] != null) {
				model = _helper.ViewData[LoginInfo.Key] as LoginInfo;
			} else {
				model = new LoginInfo();
			}

			_settings.Uri = CarrotCakeHtml.CmsPage.ThePage.FileName;
			_settings.PostPartialName = partialName;
			model.Settings = _settings;

			return model;
		}

		protected HtmlHelper<LoginInfo> InitHelp() {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(LoginInfoSettings));
			string sXML = string.Empty;
			using (StringWriter stringWriter = new StringWriter()) {
				xmlSerializer.Serialize(stringWriter, _settings);
				sXML = stringWriter.ToString();
				sXML = CMSConfigHelper.EncodeBase64(sXML);
			}

			_model.Settings = _settings;
			_model.EncodedSettings = sXML;

			var hlp = new HtmlHelper<LoginInfo>(_helper.ViewContext, new WrapperForHtmlHelper<LoginInfo>(_model, _helper.ViewData));

			string frmTag = Environment.NewLine
						+ hlp.AntiForgeryToken().ToString()
						+ Environment.NewLine
						+ hlp.HiddenFor(x => x.EncodedSettings).ToString()
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

	//==================================================

	public class LoginInfoSettings : FormSettingBase {

		public LoginInfoSettings() : base() { }

		public string PostPartialNameLockout { get; set; }
		public string PostPartialNameVerification { get; set; }
		public string PostPartialNameFailure { get; set; }
		public string CodeRedirectUri { get; set; }
		public string RedirectUri { get; set; }
	}

	//==================================================

	public class LoginInfoConfig : FormConfigBase {

		public LoginInfoConfig() : base() { }

		public LoginInfoConfig(string partialName)
			: base(partialName) {
		}

		public LoginInfoConfig(string partialName, IValidateHuman validateHuman)
			: base(partialName, validateHuman) {
		}

		public string PostPartialNameLockout { get; set; }
		public string PostPartialNameVerification { get; set; }
		public string PostPartialNameFailure { get; set; }

		public string CodeRedirectUri { get; set; }
		public string RedirectUri { get; set; }
	}

	//==================================================

	public class AjaxLogoutForm : IDisposable {
		protected AjaxHelper _helper;
		protected MvcForm frm = null;
		protected LogoutInfo _model = null;
		protected LogoutInfoSettings _settings = null;

		public AjaxLogoutForm(AjaxHelper ajaxHelper, PagePayload page, AjaxOptions ajaxOptions, object formAttributes = null) {
			_helper = ajaxHelper;

			if (page == null) {
				page = PagePayload.GetCurrentContent();
			}

			if (ajaxOptions == null) {
				ajaxOptions = new AjaxOptions();
				ajaxOptions.InsertionMode = InsertionMode.Replace;
			}
			if (string.IsNullOrEmpty(ajaxOptions.HttpMethod)) {
				ajaxOptions.HttpMethod = "POST";
			}
			if (string.IsNullOrEmpty(ajaxOptions.UpdateTargetId)) {
				ajaxOptions.UpdateTargetId = "frmLogout";
			}
			if (string.IsNullOrEmpty(ajaxOptions.OnFailure)) {
				ajaxOptions.OnFailure = "__OnAjaxRequestFailure";
			}

			string formAction = "Logout.ashx";

			if (SecurityData.AdvancedEditMode) {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = CmsRouteConstants.Controller.AjaxForms, action = formAction, carrotedit = true }, ajaxOptions, formAttributes);
			} else {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = CmsRouteConstants.Controller.AjaxForms, action = formAction }, ajaxOptions, formAttributes);
			}
		}

		public HtmlHelper<LogoutInfo> GetModelHelper(LogoutInfoConfig config) {
			_model = InitLogoutInfo(config.PostPartialName);

			_settings.RedirectUri = config.RedirectUri;

			return InitHelp();
		}

		public HtmlHelper<LogoutInfo> GetModelHelper(string partialName) {
			_model = InitLogoutInfo(partialName);

			return InitHelp();
		}

		protected LogoutInfo InitLogoutInfo(string partialName) {
			LogoutInfo model = new LogoutInfo();
			_settings = new LogoutInfoSettings();

			if (_helper.ViewData[LogoutInfo.Key] != null) {
				model = _helper.ViewData[LogoutInfo.Key] as LogoutInfo;
			} else {
				model = new LogoutInfo();
			}

			_settings.Uri = CarrotCakeHtml.CmsPage.ThePage.FileName;
			_settings.PostPartialName = partialName;
			model.Settings = _settings;

			return model;
		}

		protected HtmlHelper<LogoutInfo> InitHelp() {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(LogoutInfoSettings));
			string sXML = string.Empty;
			using (StringWriter stringWriter = new StringWriter()) {
				xmlSerializer.Serialize(stringWriter, _settings);
				sXML = stringWriter.ToString();
				sXML = CMSConfigHelper.EncodeBase64(sXML);
			}

			_model.Settings = _settings;
			_model.EncodedSettings = sXML;

			var hlp = new HtmlHelper<LogoutInfo>(_helper.ViewContext, new WrapperForHtmlHelper<LogoutInfo>(_model, _helper.ViewData));

			string frmTag = Environment.NewLine
						+ hlp.AntiForgeryToken().ToString()
						+ Environment.NewLine
						+ hlp.HiddenFor(x => x.EncodedSettings).ToString()
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

	//==================================================

	public class LogoutInfoSettings : FormSettingRootBase {

		public LogoutInfoSettings() : base() { }

		public string RedirectUri { get; set; }
	}

	//==================================================

	public class LogoutInfoConfig : FormConfigRootBase {

		public LogoutInfoConfig() : base() { }

		public LogoutInfoConfig(string partialName)
			: base(partialName) {
		}

		public string RedirectUri { get; set; }
	}

	//==================================================

	public class AjaxForgotPasswordForm : IDisposable {
		protected AjaxHelper _helper;
		protected MvcForm frm = null;
		protected ForgotPasswordInfo _model = null;
		protected ForgotPasswordInfoSettings _settings = null;

		public AjaxForgotPasswordForm(AjaxHelper ajaxHelper, PagePayload page, AjaxOptions ajaxOptions, object formAttributes = null) {
			_helper = ajaxHelper;

			if (page == null) {
				page = PagePayload.GetCurrentContent();
			}

			if (ajaxOptions == null) {
				ajaxOptions = new AjaxOptions();
				ajaxOptions.InsertionMode = InsertionMode.Replace;
			}
			if (string.IsNullOrEmpty(ajaxOptions.HttpMethod)) {
				ajaxOptions.HttpMethod = "POST";
			}
			if (string.IsNullOrEmpty(ajaxOptions.UpdateTargetId)) {
				ajaxOptions.UpdateTargetId = "frmForgotPassword";
			}
			if (string.IsNullOrEmpty(ajaxOptions.OnFailure)) {
				ajaxOptions.OnFailure = "__OnAjaxRequestFailure";
			}

			string formAction = "ForgotPassword.ashx";

			if (SecurityData.AdvancedEditMode) {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = CmsRouteConstants.Controller.AjaxForms, action = formAction, carrotedit = true }, ajaxOptions, formAttributes);
			} else {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = CmsRouteConstants.Controller.AjaxForms, action = formAction }, ajaxOptions, formAttributes);
			}
		}

		public HtmlHelper<ForgotPasswordInfo> GetModelHelper(string partialName, IValidateHuman validateHuman = null) {
			_model = InitForgotPasswordInfo(partialName);

			_settings.SetHuman(validateHuman);

			return InitHelp();
		}

		public HtmlHelper<ForgotPasswordInfo> GetModelHelper(ForgotPasswordInfoConfig config) {
			_model = InitForgotPasswordInfo(config.PostPartialName);

			_settings.GetSettingFromConfig(config);

			_settings.PostPartialConfirmation = config.PostPartialConfirmation;
			_settings.ConfirmUri = config.ConfirmUri;

			return InitHelp();
		}

		public HtmlHelper<ForgotPasswordInfo> GetModelHelper(string partialName) {
			_model = InitForgotPasswordInfo(partialName);

			return InitHelp();
		}

		protected ForgotPasswordInfo InitForgotPasswordInfo(string partialName) {
			ForgotPasswordInfo model = new ForgotPasswordInfo();
			_settings = new ForgotPasswordInfoSettings();

			if (_helper.ViewData[ForgotPasswordInfo.Key] != null) {
				model = _helper.ViewData[ForgotPasswordInfo.Key] as ForgotPasswordInfo;
			} else {
				model = new ForgotPasswordInfo();
			}

			_settings.Uri = CarrotCakeHtml.CmsPage.ThePage.FileName;
			_settings.PostPartialName = partialName;
			model.Settings = _settings;

			return model;
		}

		protected HtmlHelper<ForgotPasswordInfo> InitHelp() {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ForgotPasswordInfoSettings));
			string sXML = string.Empty;
			using (StringWriter stringWriter = new StringWriter()) {
				xmlSerializer.Serialize(stringWriter, _settings);
				sXML = stringWriter.ToString();
				sXML = CMSConfigHelper.EncodeBase64(sXML);
			}

			_model.Settings = _settings;
			_model.EncodedSettings = sXML;

			var hlp = new HtmlHelper<ForgotPasswordInfo>(_helper.ViewContext, new WrapperForHtmlHelper<ForgotPasswordInfo>(_model, _helper.ViewData));

			string frmTag = Environment.NewLine
						+ hlp.AntiForgeryToken().ToString()
						+ Environment.NewLine
						+ hlp.HiddenFor(x => x.EncodedSettings).ToString()
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

	//==================================================

	public class ForgotPasswordInfoSettings : FormSettingBase {

		public ForgotPasswordInfoSettings() : base() { }

		public string PostPartialConfirmation { get; set; }
		public string ConfirmUri { get; set; }
	}

	//==================================================

	public class ForgotPasswordInfoConfig : FormConfigBase {

		public ForgotPasswordInfoConfig() : base() { }

		public ForgotPasswordInfoConfig(string partialName)
			: base(partialName) {
		}

		public ForgotPasswordInfoConfig(string partialName, IValidateHuman validateHuman)
			: base(partialName, validateHuman) {
		}

		public string PostPartialConfirmation { get; set; }
		public string ConfirmUri { get; set; }
	}

	//==================================================

	public class AjaxResetPasswordForm : IDisposable {
		protected AjaxHelper _helper;
		protected MvcForm frm = null;
		protected ResetPasswordInfo _model = null;
		protected ResetPasswordInfoSettings _settings = null;

		public AjaxResetPasswordForm(AjaxHelper ajaxHelper, PagePayload page, AjaxOptions ajaxOptions, object formAttributes = null) {
			_helper = ajaxHelper;

			if (page == null) {
				page = PagePayload.GetCurrentContent();
			}

			if (ajaxOptions == null) {
				ajaxOptions = new AjaxOptions();
				ajaxOptions.InsertionMode = InsertionMode.Replace;
			}
			if (string.IsNullOrEmpty(ajaxOptions.HttpMethod)) {
				ajaxOptions.HttpMethod = "POST";
			}
			if (string.IsNullOrEmpty(ajaxOptions.UpdateTargetId)) {
				ajaxOptions.UpdateTargetId = "frmResetPassword";
			}
			if (string.IsNullOrEmpty(ajaxOptions.OnFailure)) {
				ajaxOptions.OnFailure = "__OnAjaxRequestFailure";
			}

			string code = ResetPasswordInfoSettings.CodeUrl;

			string formAction = "ResetPassword.ashx";

			if (SecurityData.AdvancedEditMode) {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = CmsRouteConstants.Controller.AjaxForms, action = formAction, @code = code, @carrotedit = true }, ajaxOptions, formAttributes);
			} else {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = CmsRouteConstants.Controller.AjaxForms, action = formAction, @code = code }, ajaxOptions, formAttributes);
			}
		}

		public HtmlHelper<ResetPasswordInfo> GetModelHelper(string partialName, IValidateHuman validateHuman = null) {
			_model = InitResetPasswordInfo(partialName);

			_settings.SetHuman(validateHuman);

			return InitHelp();
		}

		public HtmlHelper<ResetPasswordInfo> GetModelHelper(ResetPasswordInfoConfig config) {
			_model = InitResetPasswordInfo(config.PostPartialName);

			_settings.GetSettingFromConfig(config);

			_settings.PostPartialConfirmation = config.PostPartialConfirmation;

			return InitHelp();
		}

		public HtmlHelper<ResetPasswordInfo> GetModelHelper(string partialName) {
			_model = InitResetPasswordInfo(partialName);

			return InitHelp();
		}

		protected ResetPasswordInfo InitResetPasswordInfo(string partialName) {
			ResetPasswordInfo model = new ResetPasswordInfo();
			_settings = new ResetPasswordInfoSettings();

			if (_helper.ViewData[ResetPasswordInfo.Key] != null) {
				model = _helper.ViewData[ResetPasswordInfo.Key] as ResetPasswordInfo;
			} else {
				model = new ResetPasswordInfo();
			}

			_settings.Uri = CarrotCakeHtml.CmsPage.ThePage.FileName;
			_settings.PostPartialName = partialName;
			_settings.UserCode = ResetPasswordInfoSettings.CodeUrl;

			model.Settings = _settings;

			return model;
		}

		protected HtmlHelper<ResetPasswordInfo> InitHelp() {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ResetPasswordInfoSettings));
			string sXML = string.Empty;
			using (StringWriter stringWriter = new StringWriter()) {
				xmlSerializer.Serialize(stringWriter, _settings);
				sXML = stringWriter.ToString();
				sXML = CMSConfigHelper.EncodeBase64(sXML);
			}

			_model.Settings = _settings;
			_model.EncodedSettings = sXML;

			var hlp = new HtmlHelper<ResetPasswordInfo>(_helper.ViewContext, new WrapperForHtmlHelper<ResetPasswordInfo>(_model, _helper.ViewData));

			string frmTag = Environment.NewLine
						+ hlp.AntiForgeryToken().ToString()
						+ Environment.NewLine
						+ hlp.HiddenFor(x => x.EncodedSettings).ToString()
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

	//==================================================

	public class ResetPasswordInfoSettings : FormSettingBase {

		public ResetPasswordInfoSettings()
			: base() {
			this.UserCode = ResetPasswordInfoSettings.CodeUrl;
		}

		public string PostPartialConfirmation { get; set; }
		public string UserCode { get; set; }

		public static string CodeUrl {
			get {
				return HttpContext.Current.Request.QueryString["code"] != null
					? HttpContext.Current.Request.QueryString["code"].ToString() : string.Empty;
			}
		}
	}

	//==================================================

	public class ResetPasswordInfoConfig : FormConfigBase {

		public ResetPasswordInfoConfig() : base() { }

		public ResetPasswordInfoConfig(string partialName)
			: base(partialName) {
		}

		public ResetPasswordInfoConfig(string partialName, IValidateHuman validateHuman)
			: base(partialName, validateHuman) {
		}

		public string PostPartialConfirmation { get; set; }
	}

	//==================================================

	public class AjaxChangePasswordForm : IDisposable {
		protected AjaxHelper _helper;
		protected MvcForm frm = null;
		protected ChangePasswordInfo _model = null;
		protected ChangePasswordInfoSettings _settings = null;

		public AjaxChangePasswordForm(AjaxHelper ajaxHelper, PagePayload page, AjaxOptions ajaxOptions, object formAttributes = null) {
			_helper = ajaxHelper;

			if (page == null) {
				page = PagePayload.GetCurrentContent();
			}

			if (ajaxOptions == null) {
				ajaxOptions = new AjaxOptions();
				ajaxOptions.InsertionMode = InsertionMode.Replace;
			}
			if (string.IsNullOrEmpty(ajaxOptions.HttpMethod)) {
				ajaxOptions.HttpMethod = "POST";
			}
			if (string.IsNullOrEmpty(ajaxOptions.UpdateTargetId)) {
				ajaxOptions.UpdateTargetId = "frmChangePassword";
			}
			if (string.IsNullOrEmpty(ajaxOptions.OnFailure)) {
				ajaxOptions.OnFailure = "__OnAjaxRequestFailure";
			}

			string formAction = "ChangePassword.ashx";

			if (SecurityData.AdvancedEditMode) {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = CmsRouteConstants.Controller.AjaxForms, action = formAction, @carrotedit = true }, ajaxOptions, formAttributes);
			} else {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = CmsRouteConstants.Controller.AjaxForms, action = formAction }, ajaxOptions, formAttributes);
			}
		}

		public HtmlHelper<ChangePasswordInfo> GetModelHelper(string partialName, IValidateHuman validateHuman = null) {
			_model = InitChangePasswordInfo(partialName);

			_settings.SetHuman(validateHuman);

			return InitHelp();
		}

		public HtmlHelper<ChangePasswordInfo> GetModelHelper(ChangePasswordInfoConfig config) {
			_model = InitChangePasswordInfo(config.PostPartialName);

			_settings.GetSettingFromConfig(config);

			_settings.PostPartialSuccess = config.PostPartialSuccess;

			return InitHelp();
		}

		public HtmlHelper<ChangePasswordInfo> GetModelHelper(string partialName) {
			_model = InitChangePasswordInfo(partialName);

			return InitHelp();
		}

		protected ChangePasswordInfo InitChangePasswordInfo(string partialName) {
			ChangePasswordInfo model = new ChangePasswordInfo();
			_settings = new ChangePasswordInfoSettings();

			if (_helper.ViewData[ChangePasswordInfo.Key] != null) {
				model = _helper.ViewData[ChangePasswordInfo.Key] as ChangePasswordInfo;
			} else {
				model = new ChangePasswordInfo();
			}

			_settings.Uri = CarrotCakeHtml.CmsPage.ThePage.FileName;
			_settings.PostPartialName = partialName;

			model.Settings = _settings;

			return model;
		}

		protected HtmlHelper<ChangePasswordInfo> InitHelp() {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ChangePasswordInfoSettings));
			string sXML = string.Empty;
			using (StringWriter stringWriter = new StringWriter()) {
				xmlSerializer.Serialize(stringWriter, _settings);
				sXML = stringWriter.ToString();
				sXML = CMSConfigHelper.EncodeBase64(sXML);
			}

			_model.Settings = _settings;
			_model.EncodedSettings = sXML;

			var hlp = new HtmlHelper<ChangePasswordInfo>(_helper.ViewContext, new WrapperForHtmlHelper<ChangePasswordInfo>(_model, _helper.ViewData));

			string frmTag = Environment.NewLine
						+ hlp.AntiForgeryToken().ToString()
						+ Environment.NewLine
						+ hlp.HiddenFor(x => x.EncodedSettings).ToString()
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

	//==================================================

	public class ChangePasswordInfoSettings : FormSettingBase {

		public ChangePasswordInfoSettings() : base() { }

		public string PostPartialSuccess { get; set; }

		public static string CodeUrl {
			get {
				return HttpContext.Current.Request.QueryString["code"] != null
					? HttpContext.Current.Request.QueryString["code"].ToString() : string.Empty;
			}
		}
	}

	//==================================================

	public class ChangePasswordInfoConfig : FormConfigBase {

		public ChangePasswordInfoConfig() : base() { }

		public ChangePasswordInfoConfig(string partialName)
			: base(partialName) {
		}

		public ChangePasswordInfoConfig(string partialName, IValidateHuman validateHuman)
			: base(partialName, validateHuman) {
		}

		public string PostPartialSuccess { get; set; }
	}

	//==================================================

	public class AjaxChangeProfileForm : IDisposable {
		protected AjaxHelper _helper;
		protected MvcForm frm = null;
		protected ChangeProfileInfo _model = null;
		protected ChangeProfileInfoSettings _settings = null;

		public AjaxChangeProfileForm(AjaxHelper ajaxHelper, PagePayload page, AjaxOptions ajaxOptions, object formAttributes = null) {
			_helper = ajaxHelper;

			if (page == null) {
				page = PagePayload.GetCurrentContent();
			}

			if (ajaxOptions == null) {
				ajaxOptions = new AjaxOptions();
				ajaxOptions.InsertionMode = InsertionMode.Replace;
			}
			if (string.IsNullOrEmpty(ajaxOptions.HttpMethod)) {
				ajaxOptions.HttpMethod = "POST";
			}
			if (string.IsNullOrEmpty(ajaxOptions.UpdateTargetId)) {
				ajaxOptions.UpdateTargetId = "frmChangeProfile";
			}
			if (string.IsNullOrEmpty(ajaxOptions.OnFailure)) {
				ajaxOptions.OnFailure = "__OnAjaxRequestFailure";
			}

			string formAction = "ChangeProfile.ashx";

			if (SecurityData.AdvancedEditMode) {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = CmsRouteConstants.Controller.AjaxForms, action = formAction, @carrotedit = true }, ajaxOptions, formAttributes);
			} else {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = CmsRouteConstants.Controller.AjaxForms, action = formAction }, ajaxOptions, formAttributes);
			}
		}

		public HtmlHelper<ChangeProfileInfo> GetModelHelper(string partialName, IValidateHuman validateHuman = null) {
			_model = InitChangeProfileInfo(partialName);

			_settings.SetHuman(validateHuman);

			return InitHelp();
		}

		public HtmlHelper<ChangeProfileInfo> GetModelHelper(ChangeProfileInfoConfig config) {
			_model = InitChangeProfileInfo(config.PostPartialName);

			_settings.GetSettingFromConfig(config);

			_settings.PostPartialSuccess = config.PostPartialSuccess;

			return InitHelp();
		}

		public HtmlHelper<ChangeProfileInfo> GetModelHelper(string partialName) {
			_model = InitChangeProfileInfo(partialName);

			return InitHelp();
		}

		protected ChangeProfileInfo InitChangeProfileInfo(string partialName) {
			ChangeProfileInfo model = new ChangeProfileInfo();

			_settings = new ChangeProfileInfoSettings();

			if (_helper.ViewData[ChangeProfileInfo.Key] != null) {
				model = _helper.ViewData[ChangeProfileInfo.Key] as ChangeProfileInfo;
			} else {
				if (SecurityData.IsAuthenticated) {
					var usr = SecurityData.CurrentExUser;
					if (usr != null) {
						model.Email = usr.Email;
						model.UserNickName = usr.UserNickName;
						model.FirstName = usr.FirstName;
						model.LastName = usr.LastName;
					}
				}
			}

			_settings.Uri = CarrotCakeHtml.CmsPage.ThePage.FileName;
			_settings.PostPartialName = partialName;

			model.Settings = _settings;

			return model;
		}

		protected HtmlHelper<ChangeProfileInfo> InitHelp() {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ChangeProfileInfoSettings));
			string sXML = string.Empty;
			using (StringWriter stringWriter = new StringWriter()) {
				xmlSerializer.Serialize(stringWriter, _settings);
				sXML = stringWriter.ToString();
				sXML = CMSConfigHelper.EncodeBase64(sXML);
			}

			_model.Settings = _settings;
			_model.EncodedSettings = sXML;

			var hlp = new HtmlHelper<ChangeProfileInfo>(_helper.ViewContext, new WrapperForHtmlHelper<ChangeProfileInfo>(_model, _helper.ViewData));

			string frmTag = Environment.NewLine
						+ hlp.AntiForgeryToken().ToString()
						+ Environment.NewLine
						+ hlp.HiddenFor(x => x.EncodedSettings).ToString()
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

	//==================================================

	public class ChangeProfileInfoSettings : FormSettingBase {

		public ChangeProfileInfoSettings() : base() { }

		public string PostPartialSuccess { get; set; }
	}

	//==================================================

	public class ChangeProfileInfoConfig : FormConfigBase {

		public ChangeProfileInfoConfig() : base() { }

		public ChangeProfileInfoConfig(string partialName)
			: base(partialName) {
		}

		public ChangeProfileInfoConfig(string partialName, IValidateHuman validateHuman)
			: base(partialName, validateHuman) {
		}

		public string PostPartialSuccess { get; set; }
	}

	//==================================================

	internal class FormRouteValue {

		internal FormRouteValue() {
			this.controller = string.Empty;
			this.action = string.Empty;
			this.carrotedit = null;
		}

		internal FormRouteValue(string c, string a)
			: this() {
			this.controller = c;
			this.action = a;
		}

		internal FormRouteValue(string c, string a, bool ce)
			: this() {
			this.controller = c;
			this.action = a;
			this.carrotedit = ce;
		}

		internal string controller { get; set; }
		internal string action { get; set; }
		internal bool? carrotedit { get; set; }
	}

	//==================================================

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