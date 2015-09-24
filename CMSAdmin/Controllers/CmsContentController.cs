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
		private PagePayload _page = null;

		[HttpGet]
		public ActionResult Default() {
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

			if (_page != null && _page.ThePage.ContentID != Guid.Empty) {
				return View(DisplayTemplateFile);
			} else {
				string sFileRequested = HttpContext.Request.Path;

				if (sFileRequested.Length < 2 || sFileRequested.ToLower() == SiteData.DefaultDirectoryFilename) {
					return View("_EmptyHome");
				} else {
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
				if (System.IO.File.Exists(Server.MapPath(_page.ThePage.TemplateFile))) {
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
		[ValidateAntiForgeryToken]
		public PartialViewResult Contact(ContactInfo model) {
			this.ViewData["CMS_contactform"] = model;

			LoadPage(model.Uri);

			//TODO: log the comment

			return PartialView(model.PostPartialName);
		}

		//====================================
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

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
			var manage = new ManageSecurity(this);
			manage.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

			return RedirectToAction("Default");
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Login(LoginViewModel model, string returnUrl) {
			if (!ModelState.IsValid) {
				return View(model);
			}

			var manage = new ManageSecurity(this);

			// This doesn't count login failures towards account lockout
			// To enable password failures to trigger account lockout, change to shouldLockout: true
			var result = await manage.SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: true);

			switch (result) {
				case SignInStatus.Success:
					if (String.IsNullOrEmpty(returnUrl)) {
						Response.Redirect(SiteData.RefererScriptName);
					}
					return RedirectToLocal(returnUrl);
					break;

				case SignInStatus.LockedOut:
					return View("Lockout");

				case SignInStatus.RequiresVerification:
					return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

				case SignInStatus.Failure:
				default:
					ModelState.AddModelError("", "Invalid login attempt.");
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