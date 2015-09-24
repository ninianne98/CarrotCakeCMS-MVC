using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Carrotware.Web.UI.Components.test {
	public partial class CAPTCHA2 : System.Web.UI.Page {
		protected override void OnInit(EventArgs e) {

			int imageHeight = 50;
			int topPadding = 10; // top and bottom padding in pixels
			int sidePadding = 10; // side padding in pixels
			SolidBrush textBrush = new SolidBrush(ColorTranslator.FromHtml("#97AC88"));
			Font font = new Font("Verdana", 18);

			string text = Guid.NewGuid().ToString().Substring(0, 6);

			Bitmap bitmap = new Bitmap(500, 500);
			Graphics graphics = Graphics.FromImage(bitmap);
			SizeF textSize = graphics.MeasureString(text, font);

			bitmap.Dispose();
			graphics.Dispose();

			int bitmapWidth = sidePadding * 2 + (int)textSize.Width;
			bitmap = new Bitmap(bitmapWidth, imageHeight);
			graphics = Graphics.FromImage(bitmap);

			graphics.DrawString(text, font, textBrush, sidePadding, topPadding);

			this.Response.ContentType = "image/x-png";

			using (MemoryStream memStream = new MemoryStream()) {
				bitmap.Save(memStream, ImageFormat.Png);
				memStream.WriteTo(this.Response.OutputStream);
			}

			graphics.Dispose();
			bitmap.Dispose();

			this.Response.End();

		}
	}
}
