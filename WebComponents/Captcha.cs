using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Web;
using System.Web.UI;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.Web.UI.Components {

	[DefaultProperty("CaptchaText")]
	[ToolboxData("<{0}:Captcha runat=server></{0}:Captcha>")]
	[ValidationPropertyAttribute("CaptchaText")]
	public class Captcha : BaseWebComponent, ITextControl {

		public Captcha() {
			this.CaptchaImageBoxStyle = new SimpleStyle();
			this.CaptchaTextStyle = new SimpleStyle();
			this.CaptchaInstructionStyle = new SimpleStyle();
			this.CaptchaImageStyle = new SimpleStyle();

			this.CaptchaIsValidStyle = new SimpleStyle();
			this.CaptchaIsNotValidStyle = new SimpleStyle();

			this.NoiseColor = ColorTranslator.FromHtml(CaptchaImage.NColorDef);
			this.ForeColor = ColorTranslator.FromHtml(CaptchaImage.FGColorDef);
			this.BackColor = ColorTranslator.FromHtml(CaptchaImage.BGColorDef);
		}

		[DefaultValue("")]
		public string Text {
			get;
			set;
		}

		public string CaptchaText {
			get {
				return this.Text;
			}
			set {
				this.Text = value;
			}
		}

		public override string ToString() {
			return this.CaptchaText;
		}

		public string ValidationGroup {
			get;
			set;
		}

		public string ValidationMessage {
			get;
			set;
		}

		private bool IsValid {
			get;
			set;
		}

		[DefaultValue("Please enter the code from the image above in the box below.")]
		public string Instructions { get; set; }

		[DefaultValue("Code correct!")]
		public string IsValidMessage { get; set; }

		[DefaultValue("Code incorrect, try again!")]
		public string IsNotValidMessage { get; set; }

		public string FormFieldName { get; set; }

		public Color NoiseColor { get; set; }
		public Color ForeColor { get; set; }
		public Color BackColor { get; set; }

		public SimpleStyle CaptchaImageBoxStyle { get; set; }

		public SimpleStyle CaptchaTextStyle { get; set; }

		public SimpleStyle CaptchaInstructionStyle { get; set; }

		public SimpleStyle CaptchaImageStyle { get; set; }

		public SimpleStyle CaptchaIsValidStyle { get; set; }

		public SimpleStyle CaptchaIsNotValidStyle { get; set; }

		public bool Validate() {
			this.IsValid = CaptchaImage.Validate(this.CaptchaText);

			if (!this.IsValid) {
				this.ValidationMessage = this.IsNotValidMessage;
			} else {
				this.ValidationMessage = this.IsValidMessage;
			}

			return this.IsValid;
		}

		public override string GetHtml() {
			StringBuilder sb = new StringBuilder();
			var key = CaptchaImage.GetKey();

			sb.AppendLine("<div style=\"clear: both;\" id=\"" + this.FormFieldName + "_wrapper\">\r\n");

			if (!String.IsNullOrEmpty(this.ValidationMessage)) {
				sb.AppendLine("<div");

				if (this.IsValid) {
					sb.AppendLine(this.CaptchaIsValidStyle.ToString());
				} else {
					sb.AppendLine(this.CaptchaIsNotValidStyle.ToString());
				}

				sb.AppendLine(" id=\"" + this.FormFieldName + "_msg\">\r\n");
				sb.AppendLine(this.ValidationMessage);
				sb.AppendLine("\r\n</div>\r\n");
			}

			string sJSFuncName = "Show_" + this.FormFieldName;

			sb.AppendLine("<div" + this.CaptchaImageBoxStyle.ToString() + "> ");
			sb.AppendLine("<a href=\"javascript:" + sJSFuncName + "();\"> <img" + this.CaptchaImageStyle.ToString() + " title=\"" + key + "\" alt=\"" + key + "\" border=\"0\" id=\""
				+ this.FormFieldName + "_img\" src=\"" + GetCaptchaImageURI() + "\" /> </a> \r\n");

			sb.AppendLine("</div>\r\n");
			sb.AppendLine("<div" + this.CaptchaInstructionStyle.ToString() + ">" + this.Instructions + " </div>\r\n");
			sb.AppendLine("<div" + this.CaptchaTextStyle.ToString() + "><input type=\"text\" id=\"" + this.FormFieldName + "\" name=\"" + this.FormFieldName + "\" value=\"" + HttpUtility.HtmlEncode(this.CaptchaText) + "\" /> </div>\r\n");

			sb.AppendLine("\r\n<script  type=\"text/javascript\">\r\n");
			sb.AppendLine("\r\nfunction " + sJSFuncName + "(){\r\n");
			if (!String.IsNullOrEmpty(key)) {
				sb.AppendLine("alert('" + key.Substring(0, 3) + "' + '" + key.Substring(3) + "');\r\n");
			} else {
				sb.AppendLine("alert('no code');\r\n");
			}
			sb.AppendLine("}\r\n");
			sb.AppendLine("</script>\r\n");

			sb.AppendLine("\r\n</div>\r\n");

			return sb.ToString();
		}

		private string GetCaptchaImageURI() {
			if (this.IsWebView) {
				return "/CarrotwareCaptcha.axd?t=" + DateTime.Now.Ticks +
						"&fgcolor=" + CaptchaImage.EncodeColor(ColorTranslator.ToHtml(this.ForeColor)) +
						"&bgcolor=" + CaptchaImage.EncodeColor(ColorTranslator.ToHtml(this.BackColor)) +
						"&ncolor=" + CaptchaImage.EncodeColor(ColorTranslator.ToHtml(this.NoiseColor));
			} else {
				return "/CarrotwareCaptcha.axd?t=" + DateTime.Now.Ticks;
			}
		}

		public void LoadField() {
			if (this.IsWebView) {
				if (HttpContext.Current.Request.Form.Count > 0) {
					var s = HttpContext.Current.Request.Form[this.FormFieldName];
					this.CaptchaText = s;
				}
			}
		}
	}
}