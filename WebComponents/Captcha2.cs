using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Carrotware.Web.UI.Components {
	public class Captcha2 : BaseWebComponent, IValidateHuman {

		public Captcha2() {
			this.AltValidationFailText = "Failed to validate image";
			this.Instructions = "Select the name of the item shown in the image above from the list below.";

			this.ImageOptions = new Dictionary<string, string>();
			string imgPfx = "Carrotware.Web.UI.Components.captcha2";

			this.ImageOptions.Add(CarrotWeb.GetWebResourceUrl(this.GetType(), String.Format("{0}.bouquet.png", imgPfx)), "Bouquet");
			this.ImageOptions.Add(CarrotWeb.GetWebResourceUrl(this.GetType(), String.Format("{0}.pen.png", imgPfx)), "Pen");
			this.ImageOptions.Add(CarrotWeb.GetWebResourceUrl(this.GetType(), String.Format("{0}.pepper.png", imgPfx)), "Pepper");
			this.ImageOptions.Add(CarrotWeb.GetWebResourceUrl(this.GetType(), String.Format("{0}.scissors.png", imgPfx)), "Scissors");
			this.ImageOptions.Add(CarrotWeb.GetWebResourceUrl(this.GetType(), String.Format("{0}.snowflake.png", imgPfx)), "Snowflake");
			this.ImageOptions.Add(CarrotWeb.GetWebResourceUrl(this.GetType(), String.Format("{0}.web.png", imgPfx)), "Web");
		}

		public Dictionary<string, string> ImageOptions { get; set; }

		public object imageAttributes { get; set; }

		public override string GetHtml() {
			string val = this.SessionKeyValue.Value;

			var imgBuilder = new TagBuilder("img");
			imgBuilder.MergeAttribute("src", GetCaptchaImageURI());
			imgBuilder.MergeAttribute("alt", val);
			imgBuilder.MergeAttribute("title", val);

			var imgAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(imageAttributes);
			imgBuilder.MergeAttributes(imgAttribs);

			return imgBuilder.ToString(TagRenderMode.SelfClosing);
		}

		private string GetCaptchaImageURI() {
			if (this.IsWebView) {
				return this.SessionKeyValue.Key;
			} else {
				return String.Empty;
			}
		}

		public static string SessionKey {
			get {
				return "carrot_captcha2_key";
			}
		}

		public KeyValuePair<string, string> SessionKeyValue {
			get {
				string imageName = String.Empty;
				KeyValuePair<string, string> randImg = new KeyValuePair<string, string>("Fake", "Value");
				if (this.IsWebView && this.ImageOptions.Any()) {
					var rand = new Random();
					randImg = this.ImageOptions.ElementAt(rand.Next(0, this.ImageOptions.Count));

					try {
						if (HttpContext.Current.Session[SessionKey] != null) {
							imageName = HttpContext.Current.Session[SessionKey].ToString();
						} else {
							imageName = String.Format("{0}|{1}", randImg.Key, randImg.Value);
							HttpContext.Current.Session[SessionKey] = imageName;
						}
					} catch {
						imageName = String.Format("{0}|{1}", randImg.Key, randImg.Value);
						HttpContext.Current.Session[SessionKey] = imageName;
					}
				}

				string[] kvp = imageName.Split('|');
				randImg = new KeyValuePair<string, string>(kvp[0], kvp[1]);
				return randImg;
			}
		}


		//=============================
		public bool ValidateValue(string testValue) {
			if (String.IsNullOrEmpty(testValue)) {
				return false;
			}

			bool valid = this.SessionKeyValue.Value.ToLowerInvariant().Trim() == testValue.ToLowerInvariant().Trim();

			if (valid) {
				HttpContext.Current.Session[SessionKey] = null;
			}

			return valid;
		}

		public string Instructions { get; set; }

		public string AltValidationFailText { get; set; }
	}
}
