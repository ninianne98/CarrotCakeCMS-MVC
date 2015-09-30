using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
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

namespace Carrotware.Web.UI.Components.Controllers {

	public class HomeController : Controller {

		[OutputCache(Duration = 360)]
		public ActionResult GetImageThumb(string thumb, bool? scale, int? square) {
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

							using (MemoryStream memStream = new MemoryStream()) {
								bmpThumb.Save(memStream, ImageFormat.Png);
							}

							textBrush.Dispose();
							font.Dispose();
						}
					}
				}
			}

			if (bmpThumb == null) {
				return null;
			}

			using (MemoryStream imgStream = new MemoryStream()) {
				bmpThumb.Save(imgStream, ImageFormat.Png);
				bmpThumb.Dispose();

				return File(imgStream.ToArray(), "image/png");
			}
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

			using (MemoryStream imgStream = new MemoryStream()) {
				bmpCaptcha.Save(imgStream, ImageFormat.Png);
				bmpCaptcha.Dispose();

				Response.StatusCode = 200;
				Response.StatusDescription = "OK";

				return File(imgStream.ToArray(), "image/png");
			}
		}
	}
}