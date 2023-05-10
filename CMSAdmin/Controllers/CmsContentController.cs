using Carrotware.CMS.Core;
using Carrotware.CMS.DBUpdater;
using Carrotware.CMS.Security;
using Carrotware.CMS.Security.Models;
using Carrotware.CMS.UI.Components;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Text;
using System.Threading.Tasks;
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

namespace Carrotware.CMS.Mvc.UI.Admin.Controllers {

	public class CmsContentController : Controller, IContentController {
		protected SecurityHelper securityHelper = new SecurityHelper();
		protected CMSConfigHelper cmsHelper = new CMSConfigHelper();
		private PagePayload _page = null;

		public CmsContentController()
			: base() {
			this.TemplateFile = string.Empty;
			this.WidgetCount = 0;
		}

		public string TemplateFile { get; set; }

		private int _widgetCount = 0;

		public int WidgetCount {
			get {
				return _widgetCount++;
			}
			set {
				_widgetCount = value;
			}
		}

		[HttpGet]
		public ActionResult Default() {
			if (DatabaseUpdate.TablesIncomplete) {
				if (DatabaseUpdate.LastSQLError != null) {
					SiteData.WriteDebugException("cmscontentcontroller_default_inc", DatabaseUpdate.LastSQLError);
				} else {
					SiteData.WriteDebugException("cmscontentcontroller_default_inc", new Exception(string.Format("Requesting: {0} {1}", Request.Path, this.DisplayTemplateFile)));
				}

				return View("_EmptyHome");
			}

			try {
				return DefaultView();
			} catch (Exception ex) {
				//assumption is database is probably empty / needs updating, so trigger the under construction view
				if (DatabaseUpdate.SystemNeedsChecking(ex) || DatabaseUpdate.AreCMSTablesIncomplete()) {
					SiteData.WriteDebugException("cmscontentcontroller_defaultview", ex);

					return View("_EmptyHome");
				} else {
					//something bad has gone down, toss back the error
					SiteData.WriteDebugException("cmscontentcontroller_defaultview throw", ex);

					throw;
				}
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Default(FormCollection model) {
			_page = PagePayload.GetCurrentContent();

			object frm = null;

			if (Request.Form["form_type"] != null) {
				string formMode = Request.Form["form_type"].ToString().ToLowerInvariant();

				if (formMode == "searchform") {
					frm = new SiteSearch();
					frm = FormHelper.ParseRequest(frm, Request);
					this.ViewData["CMS_searchform"] = frm;
					if (frm != null) {
						this.TryValidateModel(frm);
					}
				}

				if (formMode == "contactform") {
					frm = new ContactInfo();
					frm = FormHelper.ParseRequest(frm, Request);
					var cmt = (ContactInfo)frm;
					cmt.Root_ContentID = _page.ThePage.Root_ContentID;
					cmt.CreateDate = SiteData.CurrentSite.Now;
					cmt.CommenterIP = Request.ServerVariables["REMOTE_ADDR"];
					this.ViewData[ContactInfo.Key] = frm;
					if (cmt != null) {
						this.TryValidateModel(cmt);
					}
				}
			}

			return DefaultView();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public PartialViewResult Default2(ContactInfo model, bool contact) {
			return PartialView();
		}

		public ActionResult DefaultView() {
			LoadPage();

			if (_page != null && _page.ThePage.Root_ContentID != Guid.Empty) {
				DateTime dtModified = _page.TheSite.ConvertSiteTimeToLocalServer(_page.ThePage.EditDate);
				string strModifed = dtModified.ToString("r");
				Response.AppendHeader("Last-Modified", strModifed);
				Response.Cache.SetLastModified(dtModified);

				DateTime dtExpire = DateTime.Now.AddSeconds(15);

				if (User.Identity.IsAuthenticated) {
					Response.Cache.SetNoServerCaching();
					Response.Cache.SetCacheability(HttpCacheability.NoCache);
					dtExpire = DateTime.Now.AddMinutes(-10);
					Response.Cache.SetExpires(dtExpire);
				} else {
					Response.Cache.SetExpires(dtExpire);
				}

				SiteData.WriteDebugException("cmscontentcontroller_defaultview _page != null", new Exception(string.Format("Loading: {0} {1} {2}", _page.ThePage.FileName, _page.ThePage.TemplateFile, this.DisplayTemplateFile)));

				return View(this.DisplayTemplateFile);
			} else {
				string sFileRequested = Request.Path;

				SiteData.WriteDebugException("cmscontentcontroller_defaultview _page == null", new Exception(string.Format("Requesting: {0} {1}", sFileRequested, this.DisplayTemplateFile)));

				DateTime dtModified = DateTime.Now.Date;
				string strModifed = dtModified.ToString("r");
				Response.AppendHeader("Last-Modified", strModifed);
				Response.Cache.SetLastModified(dtModified);
				Response.Cache.SetExpires(DateTime.Now.AddSeconds(30));

				if (SiteData.IsLikelyHomePage(sFileRequested)) {
					SiteData.WriteDebugException("cmscontentcontroller_defaultview", new Exception("Empty _page"));
					return View("_EmptyHome");
				} else {
					Response.StatusCode = 404;
					Response.AppendHeader("Status", "HTTP/1.1 404 object Not Found");
					SiteData.WriteDebugException("cmscontentcontroller_httpnotfound", new Exception("HttpNotFound"));
					return HttpNotFound();
				}
			}
		}

		public ActionResult PageNotFound() {
			//SiteData.Perform404Redirect(Request.Path);

			Response.StatusCode = 404;
			Response.AppendHeader("Status", "HTTP/1.1 404 object Not Found");
			SiteData.WriteDebugException("cmscontentcontroller_pagenotfound", new Exception(string.Format("HttpNotFound: {0}", Request.Path)));

			throw new HttpException(404, string.Format("HTTP/1.1 404 object Not Found: {0}", Request.Path));

			return HttpNotFound();
		}

		protected void LoadPage() {
			if (_page == null) {
				if (this.ViewData[PagePayload.ViewDataKey] == null) {
					_page = PagePayload.GetCurrentContent();
					this.ViewData[PagePayload.ViewDataKey] = _page;
				} else {
					_page = (PagePayload)this.ViewData[PagePayload.ViewDataKey];
				}
			}

			this.TemplateFile = this.DisplayTemplateFile;
		}

		protected void LoadPage(string Uri) {
			_page = PagePayload.GetContent(Uri);

			this.ViewData[PagePayload.ViewDataKey] = _page;

			this.TemplateFile = this.DisplayTemplateFile;
		}

		protected string DisplayTemplateFile {
			get {
				if (_page != null && _page.ThePage != null && !string.IsNullOrEmpty(_page.ThePage.TemplateFile)
					&& System.IO.File.Exists(Server.MapPath(_page.ThePage.TemplateFile))) {
					return _page.ThePage.TemplateFile;
				} else {
					return SiteData.DefaultTemplateFilename;
				}
			}
		}

		[HttpGet]
		public ActionResult RSSFeed(string type) {
			return new ContentResult {
				ContentType = "text/xml",
				Content = SiteData.CurrentSite.GetRSSFeed(type).ToHtmlString(),
				ContentEncoding = Encoding.UTF8
			};
		}

		[HttpGet]
		public ActionResult SiteMap() {
			return new ContentResult {
				ContentType = "text/xml",
				Content = SiteMapHelper.GetSiteMap().ToHtmlString(),
				ContentEncoding = Encoding.UTF8
			};
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		public PartialViewResult Contact(ContactInfo model) {
			model.ReconstructSettings();
			this.ViewData[ContactInfo.Key] = model;
			model.IsSaved = false;

			LoadPage(model.Settings.Uri);

			var settings = model.Settings;

			if (settings.UseValidateHuman) {
				bool IsValidated = model.ValidateHuman.ValidateValue(model.ValidationValue);
				if (!IsValidated) {
					ModelState.AddModelError("ValidationValue", model.ValidateHuman.AltValidationFailText);
					model.ValidationValue = string.Empty;
				}
			}

			//TODO: log the comment and B64 encode some of the settings (TBD)
			if (ModelState.IsValid) {
				string sIP = Request.ServerVariables["REMOTE_ADDR"].ToString();

				PostComment pc = new PostComment();
				pc.ContentCommentID = Guid.NewGuid();
				pc.Root_ContentID = _page.ThePage.Root_ContentID;
				pc.CreateDate = SiteData.CurrentSite.Now;
				pc.IsApproved = false;
				pc.IsSpam = false;
				pc.CommenterIP = sIP;
				pc.CommenterName = Server.HtmlEncode(model.CommenterName);
				pc.CommenterEmail = Server.HtmlEncode(model.CommenterEmail ?? string.Empty);
				pc.PostCommentText = Server.HtmlEncode(model.PostCommentText); //.Replace("<", "&lt;").Replace(">", "&gt;");
				pc.CommenterURL = Server.HtmlEncode(model.CommenterURL ?? string.Empty);

				pc.Save();

				model.IsSaved = true;

				model.CommenterName = string.Empty;
				model.CommenterEmail = string.Empty;
				model.PostCommentText = string.Empty;
				model.CommenterURL = string.Empty;
				model.ValidationValue = string.Empty;

				this.ViewData[ContactInfo.Key] = model;
				model.SendMail(pc, _page.ThePage);

				ModelState.Clear();
			}

			return PartialView(settings.PostPartialName);
		}

		//====================================
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			if (securityHelper != null) {
				securityHelper.Dispose();
			}

			if (cmsHelper != null) {
				cmsHelper.Dispose();
			}
		}

		protected void AddErrors(IdentityResult result) {
			Helper.AddErrors(ModelState, result);
		}

		//====================================

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ForgotPassword(ForgotPasswordInfo model) {
			model.ReconstructSettings();
			this.ViewData[ForgotPasswordInfo.Key] = model;

			LoadPage(model.Settings.Uri);

			var settings = model.Settings;

			if (settings.UseValidateHuman) {
				bool IsValidated = model.ValidateHuman.ValidateValue(model.ValidationValue);
				if (!IsValidated) {
					ModelState.AddModelError("ValidationValue", model.ValidateHuman.AltValidationFailText);
					model.ValidationValue = string.Empty;
				}
			}

			string confirmView = settings.PostPartialName;
			if (!string.IsNullOrEmpty(settings.PostPartialName)) {
				confirmView = settings.PostPartialConfirmation;
			}

			string confirmUri = settings.Uri;
			if (!string.IsNullOrEmpty(settings.ConfirmUri)) {
				confirmUri = settings.ConfirmUri;
			}

			if (ModelState.IsValid) {
				var user = await securityHelper.UserManager.FindByEmailAsync(model.Email);
				if (user != null) {
					SecurityData sd = new SecurityData();
					sd.ResetPassword(confirmUri, model.Email);
				}

				return PartialView(confirmView, model);
			}

			Helper.HandleErrorDict(ModelState);

			return PartialView(settings.PostPartialName, model);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ResetPassword(ResetPasswordInfo model) {
			model.ReconstructSettings();
			this.ViewData[ResetPasswordInfo.Key] = model;

			LoadPage(model.Settings.Uri);

			var settings = model.Settings;

			if (settings.UseValidateHuman) {
				bool IsValidated = model.ValidateHuman.ValidateValue(model.ValidationValue);
				if (!IsValidated) {
					ModelState.AddModelError("ValidationValue", model.ValidateHuman.AltValidationFailText);
					model.ValidationValue = string.Empty;
				}
			}

			if (string.IsNullOrEmpty(settings.UserCode)) {
				ModelState.AddModelError(string.Empty, "Reset code not provided.");
			}

			if (ModelState.IsValid) {
				string confirmView = settings.PostPartialName;
				if (!string.IsNullOrEmpty(settings.PostPartialName)) {
					confirmView = settings.PostPartialConfirmation;
				}

				var user = await securityHelper.UserManager.FindByEmailAsync(model.Email);
				if (user == null) {
					return PartialView(confirmView, model);
				} else {
					SecurityData sd = new SecurityData();
					var result = sd.ResetPassword(user, settings.UserCode, model.Password);
					model.CreationResult = result;

					if (result.Succeeded) {
						return PartialView(confirmView, model);
					}

					AddErrors(result);
				}
			}

			Helper.HandleErrorDict(ModelState);

			return PartialView(settings.PostPartialName, model);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangePassword(ChangePasswordInfo model) {
			model.ReconstructSettings();
			this.ViewData[ChangePasswordInfo.Key] = model;

			LoadPage(model.Settings.Uri);

			var settings = model.Settings;
			if (!SecurityData.IsAuthenticated) {
				ModelState.AddModelError("", "User is not authenticated");
			}

			if (settings.UseValidateHuman) {
				bool IsValidated = model.ValidateHuman.ValidateValue(model.ValidationValue);
				if (!IsValidated) {
					ModelState.AddModelError("ValidationValue", model.ValidateHuman.AltValidationFailText);
					model.ValidationValue = string.Empty;
				}
			}

			if (ModelState.IsValid && SecurityData.IsAuthenticated) {
				string successView = settings.PostPartialName;
				if (!string.IsNullOrEmpty(settings.PostPartialName)) {
					successView = settings.PostPartialSuccess;
				}

				var result = await securityHelper.UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);

				if (result.Succeeded) {
					var user = await securityHelper.UserManager.FindByIdAsync(User.Identity.GetUserId());
					if (user != null) {
						await securityHelper.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
					}

					return PartialView(successView, model);
				}

				AddErrors(result);
			}

			Helper.HandleErrorDict(ModelState);

			return PartialView(settings.PostPartialName, model);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult ChangeProfile(ChangeProfileInfo model) {
			model.ReconstructSettings();
			this.ViewData[ChangeProfileInfo.Key] = model;

			LoadPage(model.Settings.Uri);

			var settings = model.Settings;

			if (settings.UseValidateHuman) {
				bool IsValidated = model.ValidateHuman.ValidateValue(model.ValidationValue);
				if (!IsValidated) {
					ModelState.AddModelError("ValidationValue", model.ValidateHuman.AltValidationFailText);
					model.ValidationValue = string.Empty;
				}
			}

			if (ModelState.IsValid && SecurityData.IsAuthenticated) {
				string successView = settings.PostPartialName;
				if (!string.IsNullOrEmpty(settings.PostPartialName)) {
					successView = settings.PostPartialSuccess;
				}

				ExtendedUserData exUsr = SecurityData.CurrentExUser;

				IdentityResult result = securityHelper.UserManager.SetEmail(exUsr.UserKey, model.Email);

				exUsr.UserNickName = model.UserNickName;
				exUsr.FirstName = model.FirstName;
				exUsr.LastName = model.LastName;

				exUsr.Save();

				if (result.Succeeded) {
					return PartialView(successView, model);
				}
			}

			Helper.HandleErrorDict(ModelState);

			return PartialView(settings.PostPartialName, model);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult Logout(LogoutInfo model) {
			model.ReconstructSettings();
			this.ViewData[LogoutInfo.Key] = model;
			LoadPage(model.Settings.Uri);

			if (ModelState.IsValid) {
				ModelState.Clear();
			}

			securityHelper.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

			return PartialView(model.Settings.PostPartialName);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Login(LoginInfo model) {
			bool rememberme = false;

			model.ReconstructSettings();
			this.ViewData[LoginInfo.Key] = model;

			LoadPage(model.Settings.Uri);

			var settings = model.Settings;

			string partialName = settings.PostPartialName;

			if (settings.UseValidateHuman) {
				bool IsValidated = model.ValidateHuman.ValidateValue(model.ValidationValue);
				if (!IsValidated) {
					ModelState.AddModelError("ValidationValue", model.ValidateHuman.AltValidationFailText);
					model.ValidationValue = string.Empty;
				}
			}

			if (ModelState.IsValid) {
				ModelState.Clear();

				ApplicationUser user = await securityHelper.UserManager.FindByNameAsync(model.UserName);
				SignInStatus result = await securityHelper.SignInManager.PasswordSignInAsync(model.UserName, model.Password, rememberme, shouldLockout: true);

				model.LogInStatus = result;

				switch (result) {
					case SignInStatus.Success:
						await securityHelper.UserManager.ResetAccessFailedCountAsync(user.Id);
						break;

					case SignInStatus.RequiresVerification:
						if (!string.IsNullOrEmpty(settings.PostPartialNameVerification)) {
							partialName = settings.PostPartialNameVerification;
						}
						break;

					case SignInStatus.LockedOut:

						ModelState.AddModelError(string.Empty, "User locked out.");

						if (!string.IsNullOrEmpty(settings.PostPartialNameLockout)) {
							partialName = settings.PostPartialNameLockout;
						}
						break;

					case SignInStatus.Failure:
					default:
						ModelState.AddModelError(string.Empty, "Invalid login attempt.");

						if (!string.IsNullOrEmpty(settings.PostPartialNameFailure)) {
							partialName = settings.PostPartialNameFailure;
						}

						if (user.LockoutEndDateUtc.HasValue && user.LockoutEndDateUtc.Value < DateTime.UtcNow) {
							user.LockoutEndDateUtc = null;
							user.AccessFailedCount = 1;
							securityHelper.UserManager.Update(user);
						}

						break;
				}
			}

			return PartialView(partialName, model);
		}
	}
}