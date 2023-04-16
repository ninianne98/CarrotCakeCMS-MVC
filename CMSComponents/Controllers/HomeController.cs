using Carrotware.Web.UI.Components.Controllers;
using Carrotware.Web.UI.Components;
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

		public ActionResult GetNavigationCss(string el, string sel, string tbg, string f, string bg, string ubg,
				string fc, string bc, string hbc, string hfc, string uf,
				string sbg, string sfg, string bc2, string fc2) {

			var context = System.Web.HttpContext.Current;
			context.Response.Cache.VaryByParams["el"] = true;
			context.Response.Cache.VaryByParams["ts"] = true;
			DoCacheMagic(context, 7);

			var nav = new TwoLevelNavigation(f, bg, ubg, fc, bc,
									hbc, hfc, uf, sbg, sfg, bc2, fc2);

			nav.ElementId = Utils.DecodeBase64(el).Replace("{", "").Replace(">", "").Replace("<", "").Replace(">", "")
									.Replace("'", "").Replace("\\", "").Replace("//", "").Replace(":", "");
			nav.CssSelected = Utils.DecodeBase64(sel).Replace("{", "").Replace(">", "").Replace("<", "").Replace(">", "")
									.Replace("'", "").Replace("\\", "").Replace("//", "").Replace(":", "");
			nav.CssTopBackground = Utils.DecodeBase64(tbg).Replace("{", "").Replace(">", "").Replace("<", "").Replace(">", "")
									.Replace("'", "").Replace("\\", "").Replace("//", "").Replace(":", "");

			var txt = nav.GenerateCSS();
			var byteArray = Encoding.UTF8.GetBytes(txt);

			_stream = new MemoryStream(byteArray);

			return File(_stream, "text/css");
		}
	}
}