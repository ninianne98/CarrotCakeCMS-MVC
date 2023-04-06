using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
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

			if (!string.IsNullOrEmpty(sImageUri)) {
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

		public ActionResult GetWebResource(string r, string t) {
			var context = System.Web.HttpContext.Current;
			context.Response.Cache.VaryByParams["r"] = true;
			context.Response.Cache.VaryByParams["t"] = true;

			DoCacheMagic(context, 5);

			var mime = "text/plain";
			string resource = Utils.DecodeBase64(r);

			var res = resource.Split(':');
			var ext = Path.GetExtension(res[0]);

			if (FileDataHelper.MimeTypes.ContainsKey(ext)) {
				mime = FileDataHelper.MimeTypes[ext];
			}

			if (mime.ToLowerInvariant().StartsWith("text")) {
				var txt = CarrotWeb.GetManifestResourceText(this.GetType(), resource);
				var sb = new StringBuilder(txt);

				try {
					Regex _webResourceRegEx = new Regex(@"<%\s*=\s*(?<rt>WebResource)\(""(?<rn>[^""]*)""\)\s*%>", RegexOptions.Singleline);
					MatchCollection matches = _webResourceRegEx.Matches(txt);

					foreach (Match m in matches) {
						var orig = m.Value;
						Group g = m.Groups["rn"];
						string resourceName = g.Value;

						sb.Replace(orig, CarrotWeb.GetWebResourceUrl(resourceName));
					}
				} catch (Exception ex) { }

				txt = sb.ToString();

				var byteArray = Encoding.UTF8.GetBytes(txt);
				stream = new MemoryStream(byteArray);
			} else {
				var bytes = CarrotWeb.GetManifestResourceBytes(this.GetType(), resource);
				stream = new MemoryStream(bytes);
			}

			return File(stream, mime);
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

			var sb = new StringBuilder();
			sb.Append(CarrotWeb.GetManifestResourceText(this.GetType(), "Carrotware.Web.UI.Components.carrotHelp.js"));

			DateTime timeAM = DateTime.Now.Date.AddHours(7);  // 7AM
			DateTime timePM = DateTime.Now.Date.AddHours(17);  // 5PM

			sb.Replace("[[TIMESTAMP]]", DateTime.UtcNow.ToString("u"));

			sb.Replace("[[SHORTDATEPATTERN]]", CarrotWeb.ShortDatePattern);
			sb.Replace("[[SHORTTIMEPATTERN]]", CarrotWeb.ShortTimePattern);

			sb.Replace("[[SHORTDATEFORMATPATTERN]]", CarrotWeb.ShortDateFormatPattern);
			sb.Replace("[[SHORTDATETIMEFORMATPATTERN]]", CarrotWeb.ShortDateTimeFormatPattern);

			sb.Replace("[[AM_TIMEPATTERN]]", timeAM.ToString("tt"));
			sb.Replace("[[PM_TIMEPATTERN]]", timePM.ToString("tt"));

			string sBody = sb.ToString();

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

			if (!string.IsNullOrEmpty(modSince)) {
				dtModSince = DateTime.Parse(modSince);
				dtModSince = dtModSince.Value.ToUniversalTime();
			}

			return dtModSince;
		}
	}
}