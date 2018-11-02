using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
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

namespace Carrotware.Web.UI.Components.Controllers {

	public class HomeController : Controller {
		protected MemoryStream stream = new MemoryStream();

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			stream.Dispose();
		}

		public ActionResult GetImageThumb(string thumb, bool? scale, int? square) {
			var context = System.Web.HttpContext.Current;
			context.Response.Cache.VaryByParams["thumb"] = true;
			context.Response.Cache.VaryByParams["scale"] = true;
			context.Response.Cache.VaryByParams["square"] = true;

			DoCacheMagic(context, 3);

			string sImageUri = thumb;
			string sImgPath = thumb;

			bool scaleImage = scale.HasValue ? scale.Value : false;
			int iThumb = square.HasValue ? square.Value : 150;

			if (sImageUri.Contains("../") || sImageUri.Contains(@"..\")) {
				throw new Exception("Cannot use relative paths.");
			}
			if (sImageUri.Contains(":")) {
				throw new Exception("Cannot specify drive letters.");
			}
			if (sImageUri.Contains("//") || sImageUri.Contains(@"\\")) {
				throw new Exception("Cannot use UNC paths.");
			}

			if (iThumb < 5 || iThumb > 500) {
				iThumb = 100;
			}

			Bitmap bmpThumb = new Bitmap(iThumb, iThumb);

			int iHeight = iThumb;
			int iWidth = iThumb;

			if (!String.IsNullOrEmpty(sImageUri)) {
				sImgPath = Server.MapPath(sImageUri);
				if (System.IO.File.Exists(sImgPath)) {
					using (Bitmap bmpOriginal = new Bitmap(sImgPath)) {
						if (scaleImage) {
							int h = bmpOriginal.Height;
							int w = bmpOriginal.Width;

							if (iHeight > 0) {
								iWidth = (int)(((float)w / (float)h) * (float)iHeight);
							} else {
								iHeight = h;
								iWidth = w;
							}
						}

						bmpThumb = ResizeBitmap(bmpOriginal, iWidth, iHeight);
					}
				} else {
					using (Graphics graphics = Graphics.FromImage(bmpThumb)) {
						Rectangle rect = new Rectangle(0, 0, bmpThumb.Width, bmpThumb.Height);
						using (HatchBrush hatch = new HatchBrush(HatchStyle.Weave, Color.BurlyWood, Color.AntiqueWhite)) {
							graphics.FillRectangle(hatch, rect);
							int topPadding = 2; // top and bottom padding in pixels
							int sidePadding = 2; // side padding in pixels
							Font font = new Font(FontFamily.GenericSerif, 10, FontStyle.Bold);
							SolidBrush textBrush = new SolidBrush(Color.Black);

							if (sImageUri.Contains("/")) {
								sImageUri = sImageUri.Substring(sImageUri.LastIndexOf("/") + 1);
							}

							sImageUri = "404 \r\n" + sImageUri;

							graphics.DrawString(sImageUri, font, textBrush, sidePadding, topPadding);

							bmpThumb.Save(stream, ImageFormat.Png);

							textBrush.Dispose();
							font.Dispose();
						}
					}
				}
			}

			if (bmpThumb == null) {
				return null;
			}

			bmpThumb.Save(stream, ImageFormat.Png);
			bmpThumb.Dispose();

			return File(stream.ToArray(), "image/png");
		}

		private Bitmap ResizeBitmap(Bitmap bmpIn, int w, int h) {
			if (w < 1) {
				w = 1;
			}
			if (h < 1) {
				h = 1;
			}

			Bitmap bmpNew = new Bitmap(w, h);
			using (Graphics g = Graphics.FromImage(bmpNew)) {
				g.DrawImage(bmpIn, 0, 0, w, h);
			}

			return bmpNew;
		}

		public ActionResult GetCaptchaImage(string fgcolor, string bgcolor, string ncolor) {
			var context = System.Web.HttpContext.Current;
			context.Response.Cache.VaryByParams["fgcolor"] = true;
			context.Response.Cache.VaryByParams["bgcolor"] = true;
			context.Response.Cache.VaryByParams["ncolor"] = true;

			DoCacheMagic(context, 3);

			Color f = ColorTranslator.FromHtml(CaptchaImage.FGColorDef);
			Color b = ColorTranslator.FromHtml(CaptchaImage.BGColorDef);
			Color n = ColorTranslator.FromHtml(CaptchaImage.NColorDef);

			Bitmap bmpCaptcha = CaptchaImage.GetCaptchaImage(f, b, n);

			if (bmpCaptcha == null) {
				Response.StatusCode = 404;
				Response.StatusDescription = "Not Found";
				byte[] bb = new byte[0];

				return File(bb, "image/png");
			}

			bmpCaptcha.Save(stream, ImageFormat.Png);
			bmpCaptcha.Dispose();

			return File(stream.ToArray(), "image/png");
		}

		public ActionResult GetCarrotHelp(string id) {
			var context = System.Web.HttpContext.Current;
			context.Response.Cache.VaryByParams["ts"] = true;

			DoCacheMagic(context, 10);

			string sBody = CarrotWeb.GetManifestResourceStream("Carrotware.Web.UI.Components.carrotHelp.js");
			DateTime timeAM = DateTime.Now.Date.AddHours(6);  // 6AM
			DateTime timePM = DateTime.Now.Date.AddHours(12);  // 6PM

			sBody = sBody.Replace("[[TIMESTAMP]]", DateTime.UtcNow.ToString("u"));

			sBody = sBody.Replace("[[SHORTDATEPATTERN]]", CarrotWeb.ShortDatePattern);
			sBody = sBody.Replace("[[SHORTTIMEPATTERN]]", CarrotWeb.ShortTimePattern);
			sBody = sBody.Replace("[[SHORTDATEFORMATPATTERN]]", CarrotWeb.ShortDateFormatPattern);
			sBody = sBody.Replace("[[SHORTDATETIMEFORMATPATTERN]]", CarrotWeb.ShortDateTimeFormatPattern);

			sBody = sBody.Replace("[[AM_TIMEPATTERN]]", timeAM.ToString("tt"));
			sBody = sBody.Replace("[[PM_TIMEPATTERN]]", timePM.ToString("tt"));

			var byteArray = Encoding.UTF8.GetBytes(sBody);
			stream = new MemoryStream(byteArray);

			return File(stream, "text/javascript");
		}

		protected void DoCacheMagic(HttpContext context, int interval) {
			DateTime now = DateTime.Now;

			DateTime dtModified = GetFauxModDate(10);
			DateTime? dtM = GetModDate(context);

			string strModifed = dtModified.ToUniversalTime().ToString("r");
			context.Response.AppendHeader("Last-Modified", strModifed);
			context.Response.AppendHeader("Date", strModifed);
			context.Response.Cache.SetLastModified(dtModified);

			DateTime dtExpire = now.ToUniversalTime().AddMinutes(interval);
			context.Response.Cache.SetExpires(dtExpire);
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

		protected DateTime GetFauxModDate(int interval) {
			DateTime now = DateTime.Now;

			DateTime dtMod = now.AddMinutes(-90);
			TimeSpan ts = TimeSpan.FromMinutes(interval);
			DateTime dtModified = new DateTime(((dtMod.Ticks + ts.Ticks - 1) / ts.Ticks) * ts.Ticks);

			return dtModified;
		}

		protected DateTime? GetModDate(HttpContext context) {
			DateTime? dtModSince = null;
			string modSince = context.Request.Headers.Get("If-Modified-Since");

			if (!String.IsNullOrEmpty(modSince)) {
				dtModSince = DateTime.Parse(modSince);
				dtModSince = dtModSince.Value.ToUniversalTime();
			}

			return dtModSince;
		}
	}
}