using Carrotware.CMS.Core;
using Carrotware.CMS.DBUpdater;
using Carrotware.CMS.Security;
using Carrotware.CMS.Security.Models;
using Carrotware.CMS.UI.Components;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Mvc.UI.Admin.Controllers {

	public class CmsContentController : Controller {
		protected SecurityHelper securityHelper = new SecurityHelper();
		private PagePayload _page = null;

		[HttpGet]
		public ActionResult Default() {
			if (DatabaseUpdate.TablesIncomplete) {
				return View("_EmptyHome");
			}

			try {
				return DefaultView();
			} catch (Exception ex) {
				//assumption is database is probably empty / needs updating, so trigger the under construction view
				if (DatabaseUpdate.SystemNeedsChecking(ex) || DatabaseUpdate.AreCMSTablesIncomplete()) {
					return View("_EmptyHome");
				} else {
					//something bad has gone down, toss back the error
					throw;
				}
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Default(FormCollection model) {
			_page = PagePayload.GetCurrentContent();

			Object frm = null;

			if (Request.Form["form_type"] != null) {
				string formMode = Request.Form["form_type"].ToString().ToLower();

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
					this.ViewData["CMS_contactform"] = frm;
					if (cmt != null) {
						this.TryValidateModel(cmt);
					}
				}
			}

			return DefaultView();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public PartialViewResult Default(ContactInfo model, bool contact) {
			return PartialView();
		}

		public ActionResult DefaultView() {
			if (_page == null) {
				LoadPage();
			}

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

				return View(DisplayTemplateFile);
			} else {
				string sFileRequested = Request.Path;

				DateTime dtModified = DateTime.Now.Date;
				string strModifed = dtModified.ToString("r");
				Response.AppendHeader("Last-Modified", strModifed);
				Response.Cache.SetLastModified(dtModified);

				if (sFileRequested.Length < 2 || sFileRequested.ToLower() == SiteData.DefaultDirectoryFilename) {
					return View("_EmptyHome");
				} else {
					Response.StatusCode = 404;
					Response.AppendHeader("Status", "HTTP/1.1 404 Object Not Found");

					return HttpNotFound();
				}
			}
		}

		protected void LoadPage() {
			if (this.ViewData[PagePayload.ViewDataKey] == null) {
				_page = PagePayload.GetCurrentContent();
				this.ViewData[PagePayload.ViewDataKey] = _page;
			} else {
				_page = (PagePayload)this.ViewData[PagePayload.ViewDataKey];
			}

			if (_page != null && _page.ThePage.ContentID != Guid.Empty) {
				_page.HandleTemplatePath(this);
			}
		}

		protected void LoadPage(string Uri) {
			_page = PagePayload.GetContent(Uri);
			this.ViewData[PagePayload.ViewDataKey] = _page;

			if (_page != null && _page.ThePage.ContentID != Guid.Empty) {
				_page.HandleTemplatePath(this);
			}
		}

		protected string DisplayTemplateFile {
			get {
				if (!String.IsNullOrEmpty(_page.ThePage.TemplateFile) && System.IO.File.Exists(Server.MapPath(_page.ThePage.TemplateFile))) {
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
			this.ViewData["CMS_contactform"] = model;
			model.IsSaved = false;

			LoadPage(model.Settings.Uri);

			var settings = model.Settings;

			if (settings.UseValidateHuman) {
				bool IsValidated = model.ValidateHuman.ValidateValue(model.ValidationValue);
				if (!IsValidated) {
					ModelState.AddModelError("ValidationValue", model.ValidateHuman.AltValidationFailText);
					model.ValidationValue = String.Empty;
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
				pc.CommenterEmail = Server.HtmlEncode(model.CommenterEmail ?? String.Empty);
				pc.PostCommentText = Server.HtmlEncode(model.PostCommentText); //.Replace("<", "&lt;").Replace(">", "&gt;");
				pc.CommenterURL = Server.HtmlEncode(model.CommenterURL ?? String.Empty);

				pc.Save();

				model.IsSaved = true;

				model.CommenterName = String.Empty;
				model.CommenterEmail = String.Empty;
				model.PostCommentText = String.Empty;
				model.CommenterURL = String.Empty;
				model.ValidationValue = String.Empty;

				this.ViewData["CMS_contactform"] = model;
				model.SendMail(pc, _page.ThePage);

				ModelState.Clear();
			}

			return PartialView(model.Settings.PostPartialName);
		}

		//====================================
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			if (securityHelper != null) {
				securityHelper.Dispose();
			}

			// only add the xtra lookup paths so long as needed to render the relative path partials from the template
			List<CmsTemplateViewEngine> lst = this.ViewEngineCollection
					.Where(x => x is CmsTemplateViewEngine).Cast<CmsTemplateViewEngine>().ToList();

			if (lst.Any()) {
				for (int j = (lst.Count - 1); j >= 0; j--) {
					this.ViewEngineCollection.Remove(lst[j]);
				}
			}
		}

		//====================================

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LogOff() {
			securityHelper.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

			return RedirectToAction("Default");
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Login(LoginViewModel model, string returnUrl) {
			if (!ModelState.IsValid) {
				return View(model);
			}

			//TODO: make configurable
			//manage.UserManager.UserLockoutEnabledByDefault = true;
			//manage.UserManager.MaxFailedAccessAttemptsBeforeLockout = 5;
			//manage.UserManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(15);

			// This doesn't count login failures towards account lockout
			// To enable password failures to trigger account lockout, change to shouldLockout: true
			var user = await securityHelper.UserManager.FindByNameAsync(model.UserName);

			var result = await securityHelper.SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: true);

			switch (result) {
				case SignInStatus.Success:
					await securityHelper.UserManager.ResetAccessFailedCountAsync(user.Id);
					if (String.IsNullOrEmpty(returnUrl)) {
						Response.Redirect(SiteData.RefererScriptName);
					}

					return RedirectToLocal(returnUrl);

				case SignInStatus.LockedOut:
					return View("Lockout");

				case SignInStatus.RequiresVerification:
					return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

				case SignInStatus.Failure:
				default:
					ModelState.AddModelError(String.Empty, "Invalid login attempt.");

					if (user.LockoutEndDateUtc.HasValue && user.LockoutEndDateUtc.Value < DateTime.UtcNow) {
						user.LockoutEndDateUtc = null;
						user.AccessFailedCount = 1;
						securityHelper.UserManager.Update(user);
					}

					return View(model);
			}
		}

		private ActionResult RedirectToLocal(string returnUrl) {
			if (Url.IsLocalUrl(returnUrl)) {
				return Redirect(returnUrl);
			}

			return RedirectToAction("Default");
		}
	}
}