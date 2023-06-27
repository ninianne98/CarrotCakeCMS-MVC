using System.Web.Mvc;
using System.Web;
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

namespace Carrotware.Web.UI.Components.Controllers {

	public class BaseController : Controller {

		protected void DoCacheMagic(HttpContext context, double minutes) {
			DateTime now = DateTime.Now;

			DateTime dtModified = GetFauxModDate(10);
			DateTime? dtM = GetModDate(context);

			string strModifed = dtModified.ToUniversalTime().ToString("r");
			context.Response.AppendHeader("Last-Modified", strModifed);
			context.Response.AppendHeader("Date", strModifed);
			context.Response.Cache.SetLastModified(dtModified);

			DateTime dtExpire = now.ToUniversalTime().AddMinutes(minutes);
			context.Response.Cache.SetExpires(dtExpire.AddSeconds(-10));
			context.Response.Cache.SetValidUntilExpires(true);
			context.Response.Cache.SetCacheability(HttpCacheability.Private);

			if (dtM == null || dtM.Value != dtModified) {
				context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
				context.Response.StatusDescription = "OK";
			} else {
				context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotModified;
				context.Response.SuppressContent = true;
			}
		}

		protected DateTime GetFauxModDate(double minutes) {
			DateTime now = DateTime.Now;

			DateTime dtMod = now.AddMinutes(-90);
			TimeSpan ts = TimeSpan.FromMinutes(minutes);
			DateTime dtModified = new DateTime(((dtMod.Ticks + ts.Ticks - 1) / ts.Ticks) * ts.Ticks);

			return dtModified;
		}

		protected DateTime? GetModDate(HttpContext context) {
			DateTime? dtModSince = null;
			string modSince = context.Request.Headers.Get("If-Modified-Since");

			if (!string.IsNullOrEmpty(modSince)) {
				dtModSince = DateTime.Parse(modSince);
				dtModSince = dtModSince.Value.ToUniversalTime();
			}

			return dtModSince;
		}
	}
}