using CarrotCake.CMS.Plugins.LoremIpsum.Models;
using Carrotware.CMS.Core;
using Carrotware.CMS.Interface.Controllers;
using Carrotware.CMS.Security;
using Carrotware.CMS.Security.Models;
using Carrotware.Web.UI.Components;
using System.Threading.Tasks;
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

namespace CarrotCake.CMS.Plugins.LoremIpsum.Controllers {

	public class AdminController : BaseAdminWidgetController {
		protected SecurityHelper securityHelper = new SecurityHelper();

		public ActionResult Index() {
			return View();
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			if (securityHelper != null) {
				securityHelper.Dispose();
			}
		}

		[HttpGet]
		[AllowAnonymous]
		public ActionResult LogOut() {
			securityHelper.LogoutSession();
			Session.Clear();

			return RedirectToAction(this.GetActionName(x => x.Index()));
		}

		[HttpGet]
		[AllowAnonymous]
		public ActionResult Login() {
			var model = new LoginViewModel();
			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<ActionResult> Login(LoginViewModel model) {
			if (ModelState.IsValid) {
				var result = await securityHelper.SimpleLogInAsync(model.UserName, model.Password, true);

				if (result) {
					return RedirectToAction(this.GetActionName(x => x.Index()));
				}
			}

			ModelState.AddModelError("message", "Invalid login attempt");
			return View(model);
		}

		[HttpGet]
		public ActionResult Pages() {
			ViewBag.Title = "Pages";
			var model = new ContentCreator(ContentPageType.PageType.ContentEntry);

			return View("View", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Pages(ContentCreator model) {
			ViewBag.Title = "Pages";
			model.ContentType = ContentPageType.PageType.ContentEntry;

			if (ModelState.IsValid) {
				model.BuildPages();
			}

			return View("View", model);
		}

		[HttpGet]
		public ActionResult Posts() {
			ViewBag.Title = "Posts";
			var model = new ContentCreator(ContentPageType.PageType.BlogEntry);

			return View("View", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Posts(ContentCreator model) {
			ViewBag.Title = "Posts";
			model.ContentType = ContentPageType.PageType.BlogEntry;

			if (ModelState.IsValid) {
				model.BuildPosts();
			}

			return View("View", model);
		}
	}
}