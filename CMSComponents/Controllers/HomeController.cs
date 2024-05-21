using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using Carrotware.Web.UI.Components.Controllers;
using System;
using System.IO;
using System.Text;
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

namespace Carrotware.CMS.UI.Components.Controllers {

	public class HomeController : BaseController {
		protected MemoryStream _stream = new MemoryStream();

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			_stream.Dispose();
		}

		public ActionResult GetAdminScriptValues(string ts) {
			var context = System.Web.HttpContext.Current;
			CarrotWeb.VaryCacheByQuery(new string[] { "ts", "cms", "a" }, 2);
			DoCacheMagic(context, 2);

			var adminFolder = SiteData.AdminFolderPath;

			var sb = new StringBuilder();
			sb.Append(CarrotWeb.GetManifestResourceText(this.GetType(), "Carrotware.CMS.UI.Components.adminHelp.js"));

			sb.Replace("[[TIMESTAMP]]", DateTime.UtcNow.ToString("u"));

			if (SecurityData.UserPrincipal.Identity.IsAuthenticated) {
				if (SecurityData.IsAdmin || SecurityData.IsEditor) {
					sb.Replace("[[ADMIN_PATH]]", adminFolder.FixFolderSlashes());
					sb.Replace("[[API_PATH]]", CarrotCakeHtml.WebServiceAddress.FixPathSlashes());
					sb.Replace("[[TEMPLATE_PATH]]", SiteData.PreviewTemplateFilePage);
					sb.Replace("[[TEMPLATE_QS]]", SiteData.TemplatePreviewParameter);
				}
			}

			string sBody = sb.ToString();
			var byteArray = Encoding.UTF8.GetBytes(sBody);
			_stream = new MemoryStream(byteArray);

			return File(_stream, "text/javascript");
		}

		public ActionResult GetNavigationCss(string el, string sel, string tbg, string f, string bg, string ubg,
				string fc, string bc, string hbc, string hfc, string uf,
				string sbg, string sfg, string bc2, string fc2) {
			var context = System.Web.HttpContext.Current;

			CarrotWeb.VaryCacheByQuery(new string[] { "el", "ts", "f", "bg", "fc", "bc" }, 5);

			DoCacheMagic(context, 7);

			var nav = new TwoLevelNavigation(f, bg, ubg, fc, bc,
									hbc, hfc, uf, sbg, sfg, bc2, fc2);

			nav.ElementId = el.DecodeBase64().ScrubQueryElement();
			nav.CssSelected = sel.DecodeBase64().ScrubQueryElement();
			nav.CssTopBackground = tbg.DecodeBase64().ScrubQueryElement();

			var txt = nav.GenerateCSS();
			var byteArray = Encoding.UTF8.GetBytes(txt);

			_stream = new MemoryStream(byteArray);

			return File(_stream, "text/css");
		}
	}
}