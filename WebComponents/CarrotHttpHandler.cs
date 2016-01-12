using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.SessionState;

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

	internal class CarrotHttpHandler : IHttpHandler, IRequiresSessionState {

		public bool IsReusable {
			get { return true; }
		}

		public void ProcessRequest(HttpContext context) {
			if (context.Request.Path.ToLowerInvariant() == "/carrotwarecaptcha.axd") {
				DoCaptcha(context);
			}
		}

		private void DoCaptcha(HttpContext context) {
			Color f = ColorTranslator.FromHtml(CaptchaImage.FGColorDef);
			Color b = ColorTranslator.FromHtml(CaptchaImage.BGColorDef);
			Color n = ColorTranslator.FromHtml(CaptchaImage.NColorDef);

			Bitmap captchaImg = CaptchaImage.GetCaptchaImage(f, b, n);

			if (captchaImg == null) {
				context.Response.StatusCode = 404;
				context.Response.StatusDescription = "Not Found";
				context.ApplicationInstance.CompleteRequest();
				return;
			}

			context.Response.ContentType = "image/x-png";

			using (MemoryStream memStream = new MemoryStream()) {
				captchaImg.Save(memStream, ImageFormat.Png);
				memStream.WriteTo(context.Response.OutputStream);
			}
			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.ApplicationInstance.CompleteRequest();

			captchaImg.Dispose();
			context.Response.End();
		}
	}
}