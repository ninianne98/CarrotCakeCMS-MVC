using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace Carrotware.Web.UI.Components {

	public class Captcha : BaseWebComponent, IValidateHuman {

		public Captcha() {
			this.NoiseColor = ColorTranslator.FromHtml(CaptchaImage.NColorDef);
			this.ForeColor = ColorTranslator.FromHtml(CaptchaImage.FGColorDef);
			this.BackColor = ColorTranslator.FromHtml(CaptchaImage.BGColorDef);

			this.Instructions = "Please enter the code from the image above in the box below.";
			this.IsValidMessage = "Code correct!";
			this.IsNotValidMessage = "Code incorrect, try again!";

			this.AltValidationFailText = "Failed to validate CAPTCHA.";
		}

		public void SetNoiseColor(string colorCode) {
			this.NoiseColor = ColorTranslator.FromHtml(colorCode);
		}

		public void SetForeColor(string colorCode) {
			this.ForeColor = ColorTranslator.FromHtml(colorCode);
		}

		public void SetBackColor(string colorCode) {
			this.BackColor = ColorTranslator.FromHtml(colorCode);
		}

		public string CaptchaText { get; set; }

		public override string ToString() {
			return this.CaptchaText;
		}

		public string ValidationGroup { get; set; }

		public string ValidationMessage { get; set; }

		private bool IsValid { get; set; }

		public string Instructions { get; set; }

		public string IsValidMessage { get; set; }

		public string IsNotValidMessage { get; set; }

		public Color NoiseColor { get; set; }
		public Color ForeColor { get; set; }
		public Color BackColor { get; set; }

		public bool Validate() {
			this.IsValid = CaptchaImage.Validate(this.CaptchaText);

			if (!this.IsValid) {
				this.ValidationMessage = this.IsNotValidMessage;
			} else {
				this.ValidationMessage = this.IsValidMessage;
			}

			return this.IsValid;
		}

		public object imageAttributes { get; set; }

		public override string GetHtml() {
			var key = CaptchaImage.GetKey();

			var imgBuilder = new TagBuilder("img");
			imgBuilder.MergeAttribute("src", GetCaptchaImageURI());
			imgBuilder.MergeAttribute("alt", key);
			imgBuilder.MergeAttribute("title", key);

			var imgAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(imageAttributes);
			imgBuilder.MergeAttributes(imgAttribs);

			return imgBuilder.ToString(TagRenderMode.SelfClosing);
		}

		private string GetCaptchaImageURI() {
			if (this.IsWebView) {
				return "/CarrotwareCaptcha.ashx?t=" + DateTime.Now.Ticks +
						"&fgcolor=" + CaptchaImage.EncodeColor(ColorTranslator.ToHtml(this.ForeColor)) +
						"&bgcolor=" + CaptchaImage.EncodeColor(ColorTranslator.ToHtml(this.BackColor)) +
						"&ncolor=" + CaptchaImage.EncodeColor(ColorTranslator.ToHtml(this.NoiseColor));
			} else {
				return "/CarrotwareCaptcha.ashx?t=" + DateTime.Now.Ticks;
			}
		}

		public bool ValidateValue(string testValue) {
			this.CaptchaText = testValue;
			return Validate();
		}

		public string AltValidationFailText { get; set; }
	}
}