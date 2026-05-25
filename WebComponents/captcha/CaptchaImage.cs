using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.Web.UI.Components {

	public static class CaptchaImage {

		public static string BGColorDef {
			get {
				string d = "#EEEEEE";
				var v = HttpContext.Current.SafeQueryString("bgcolor", d);
				return (string.IsNullOrWhiteSpace(v) ? d : CarrotWeb.DecodeColorString(v));
			}
		}

		public static string NColorDef {
			get {
				string d = "#C46314";
				var v = HttpContext.Current.SafeQueryString("ncolor", d);
				return (string.IsNullOrWhiteSpace(v) ? d : CarrotWeb.DecodeColorString(v));
			}
		}

		public static string FGColorDef {
			get {
				string d = "#69785F";
				var v = HttpContext.Current.SafeQueryString("fgcolor", d);
				return (string.IsNullOrWhiteSpace(v) ? d : CarrotWeb.DecodeColorString(v));
			}
		}

		public static string SessionKey {
			get {
				return "carrot_captcha_key";
			}
		}

		public static bool Validate(string testValue) {
			if (string.IsNullOrEmpty(testValue)) {
				return false;
			}

			bool bValid = false;
			string guid = SessionKeyValue;

			if (testValue.ToLowerInvariant() == guid.ToLowerInvariant()) {
				bValid = true;
			}

			if (HttpContext.Current != null) {
				guid = Guid.NewGuid().ToString().Substring(0, 6);
				HttpContext.Current.Session[SessionKey] = guid;
			}
			return bValid;
		}

		public static Bitmap GetCachedCaptcha() {
			Color medGreen = ColorTranslator.FromHtml("#69785F");
			Color medOrange = ColorTranslator.FromHtml("#C46314");
			return GetCaptchaImage(medGreen, Color.White, medOrange);
		}

		public static string SessionKeyValue {
			get {
				string guid = "ABCXYZ";
				if (HttpContext.Current != null) {
					try {
						if (HttpContext.Current.Session[SessionKey] != null) {
							guid = HttpContext.Current.Session[SessionKey].ToString();
						} else {
							guid = Guid.NewGuid().ToString().Substring(0, 6);
							HttpContext.Current.Session[SessionKey] = guid;
						}
					} catch {
						guid = Guid.NewGuid().ToString().Substring(0, 6);
						HttpContext.Current.Session[SessionKey] = guid;
					}
				}
				return guid.ToUpperInvariant();
			}
		}

		public static Bitmap GetCaptchaImage(Color fg, Color bg, Color n) {
			int topPadding = 2; // top and bottom padding in pixels
			int sidePadding = 3; // side padding in pixels

			SolidBrush textBrush = new SolidBrush(fg);
			Font font = new Font(FontFamily.GenericSansSerif, 32, FontStyle.Bold);

			string guid = SessionKeyValue;

			Bitmap bmpCaptcha = new Bitmap(500, 500);
			Graphics graphics = Graphics.FromImage(bmpCaptcha);
			SizeF textSize = graphics.MeasureString(guid, font);

			bmpCaptcha.Dispose();
			graphics.Dispose();

			int bitmapWidth = sidePadding * 2 + (int)textSize.Width;
			int bitmapHeight = topPadding * 2 + (int)textSize.Height;
			bmpCaptcha = new Bitmap(bitmapWidth, bitmapHeight);
			graphics = Graphics.FromImage(bmpCaptcha);

			Rectangle rect = new Rectangle(0, 0, bmpCaptcha.Width, bmpCaptcha.Height);

			HatchBrush hatch1 = new HatchBrush(HatchStyle.SmallGrid, n, bg);

			HatchBrush hatch2 = new HatchBrush(HatchStyle.DiagonalCross, bg, Color.Transparent);

			graphics.FillRectangle(hatch1, rect);
			graphics.DrawString(guid, font, textBrush, sidePadding, topPadding);
			graphics.FillRectangle(hatch2, rect);

			HttpContext.Current.Response.ContentType = "image/x-png";

			using (MemoryStream memStream = new MemoryStream()) {
				bmpCaptcha.Save(memStream, ImageFormat.Png);
			}

			textBrush.Dispose();
			font.Dispose();
			hatch1.Dispose();
			hatch2.Dispose();
			graphics.Dispose();

			return bmpCaptcha;
		}
	}
}