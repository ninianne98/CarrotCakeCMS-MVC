using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.UI.Components {

	//==================================================
	public class SiteSearch {

		[StringLength(128)]
		public string query { get; set; }
	}

	//==================================================

	public class ContactInfo {

		public ContactInfo() {
			ReconstructSettings();
		}

		public static string Key {
			get {
				return "CMS_contactform";
			}
		}

		public void ReconstructSettings() {
			this.Settings = null;

			if (!String.IsNullOrEmpty(this.EncodedSettings)) {
				string sXML = CMSConfigHelper.DecodeBase64(this.EncodedSettings);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(ContactInfoSettings));
				using (StringReader stringReader = new StringReader(sXML)) {
					this.Settings = (ContactInfoSettings)xmlSerializer.Deserialize(stringReader);
				}

				if (this.Settings != null && !String.IsNullOrEmpty(this.Settings.ValidateHumanClass)) {
					Type objType = ReflectionUtilities.GetTypeFromString(this.Settings.ValidateHumanClass);
					Object obj = Activator.CreateInstance(objType);
					this.ValidateHuman = (IValidateHuman)obj;
					this.ValidateHuman.AltValidationFailText = this.Settings.ValidationFailText;
				}
			}
		}

		public Guid Root_ContentID { get; set; }
		public DateTime CreateDate { get; set; }

		[StringLength(32)]
		[Display(Name = "IP")]
		public string CommenterIP { get; set; }

		[StringLength(256)]
		[Required]
		[Display(Name = "Name")]
		public string CommenterName { get; set; }

		[StringLength(256)]
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string CommenterEmail { get; set; }

		[StringLength(256)]
		//[Required]
		[Display(Name = "URL")]
		public string CommenterURL { get; set; }

		[StringLength(4096)]
		[Required]
		[Display(Name = "Comment")]
		public string PostCommentText { get; set; }

		public string EncodedSettings { get; set; }
		public ContactInfoSettings Settings { get; set; }
		public IValidateHuman ValidateHuman { get; set; }
		public string ValidationValue { get; set; }
		public bool IsSaved { get; set; }

		public void SendMail(PostComment pc, ContentPage page) {
			HttpRequest request = HttpContext.Current.Request;

			if (this.Settings.NotifyEditors || !String.IsNullOrEmpty(this.Settings.DirectEmailKeyName)) {
				List<string> emails = new List<string>();

				if (this.Settings.NotifyEditors && page != null) {
					emails.Add(page.CreateUser.Email);

					if (page.EditUser.UserId != page.CreateUser.UserId) {
						emails.Add(page.EditUser.Email);
					}

					if (page.CreditUserId.HasValue) {
						emails.Add(page.CreditUser.Email);
					}
				}

				if (!String.IsNullOrEmpty(this.Settings.DirectEmailKeyName)) {
					emails.Add(ConfigurationManager.AppSettings[this.Settings.DirectEmailKeyName].ToString());
				}

				string strHTTPHost = String.Empty;
				try { strHTTPHost = request.ServerVariables["HTTP_HOST"].ToString().Trim(); } catch { strHTTPHost = String.Empty; }

				string hostName = strHTTPHost.ToLower();

				string strHTTPPrefix = "http://";
				try {
					strHTTPPrefix = request.ServerVariables["SERVER_PORT_SECURE"] == "1" ? "https://" : "http://";
				} catch { strHTTPPrefix = "http://"; }

				strHTTPHost = String.Format("{0}{1}", strHTTPPrefix, strHTTPHost).ToLower();

				string mailSubject = String.Format("Comment Form From {0}", hostName);

				string sBody = "Name:   " + pc.CommenterName
					+ "\r\nEmail:   " + pc.CommenterEmail
					+ "\r\nURL:   " + pc.CommenterURL
					+ "\r\n-----------------"
					+ "\r\nComment:\r\n" + HttpUtility.HtmlEncode(pc.PostCommentText)
					+ "\r\n=================\r\n"
					+ "\r\nIP:   " + pc.CommenterIP
					+ "\r\nSite URL:   " + String.Format("{0}{1}", strHTTPHost, page.FileName)
					+ "\r\nSite Time:   " + SiteData.CurrentSite.Now.ToString()
					+ "\r\nUTC Time:   " + DateTime.UtcNow.ToString();

				string sEmail = String.Join(";", emails);

				EmailHelper.SendMail(null, sEmail, mailSubject, sBody, false);
			}
		}
	}

	//==================================================

	public class LogoutInfo {

		public LogoutInfo() {
			ReconstructSettings();
		}

		public static string Key {
			get {
				return "CMS_logoutform";
			}
		}

		public void ReconstructSettings() {
			this.Settings = null;

			if (!String.IsNullOrEmpty(this.EncodedSettings)) {
				string sXML = CMSConfigHelper.DecodeBase64(this.EncodedSettings);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(LogoutInfoSettings));
				using (StringReader stringReader = new StringReader(sXML)) {
					this.Settings = (LogoutInfoSettings)xmlSerializer.Deserialize(stringReader);
				}
			}

			this.IsLoggedIn = SecurityData.IsAuthenticated;
		}

		public string EncodedSettings { get; set; }
		public LogoutInfoSettings Settings { get; set; }
		public bool IsLoggedIn { get; set; }
	}

	//==================================================

	public class LoginInfo {

		public LoginInfo() {
			ReconstructSettings();
		}

		public static string Key {
			get {
				return "CMS_loginform";
			}
		}

		public void ReconstructSettings() {
			this.Settings = null;

			if (!String.IsNullOrEmpty(this.EncodedSettings)) {
				string sXML = CMSConfigHelper.DecodeBase64(this.EncodedSettings);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(LoginInfoSettings));
				using (StringReader stringReader = new StringReader(sXML)) {
					this.Settings = (LoginInfoSettings)xmlSerializer.Deserialize(stringReader);
				}

				if (this.Settings != null && !String.IsNullOrEmpty(this.Settings.ValidateHumanClass)) {
					Type objType = ReflectionUtilities.GetTypeFromString(this.Settings.ValidateHumanClass);
					Object obj = Activator.CreateInstance(objType);
					this.ValidateHuman = (IValidateHuman)obj;
					this.ValidateHuman.AltValidationFailText = this.Settings.ValidationFailText;
				}
			}

			this.IsLoggedIn = SecurityData.IsAuthenticated;

			this.LogInStatus = this.IsLoggedIn ? SignInStatus.Success : SignInStatus.Failure;
		}

		[Required]
		[Display(Name = "Username")]
		[StringLength(128)]
		public string UserName { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		public string EncodedSettings { get; set; }
		public LoginInfoSettings Settings { get; set; }
		public IValidateHuman ValidateHuman { get; set; }
		public string ValidationValue { get; set; }
		public SignInStatus LogInStatus { get; set; }
		public bool IsLoggedIn { get; set; }
	}

	//==================================================

	public class ForgotPasswordInfo {

		public ForgotPasswordInfo() {
			ReconstructSettings();
		}

		public static string Key {
			get {
				return "CMS_forgotform";
			}
		}

		public void ReconstructSettings() {
			this.Settings = null;

			if (!String.IsNullOrEmpty(this.EncodedSettings)) {
				string sXML = CMSConfigHelper.DecodeBase64(this.EncodedSettings);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(ForgotPasswordInfoSettings));
				using (StringReader stringReader = new StringReader(sXML)) {
					this.Settings = (ForgotPasswordInfoSettings)xmlSerializer.Deserialize(stringReader);
				}

				if (this.Settings != null && !String.IsNullOrEmpty(this.Settings.ValidateHumanClass)) {
					Type objType = ReflectionUtilities.GetTypeFromString(this.Settings.ValidateHumanClass);
					Object obj = Activator.CreateInstance(objType);
					this.ValidateHuman = (IValidateHuman)obj;
					this.ValidateHuman.AltValidationFailText = this.Settings.ValidationFailText;
				}
			}
		}

		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		[StringLength(128)]
		public string Email { get; set; }

		public string EncodedSettings { get; set; }
		public ForgotPasswordInfoSettings Settings { get; set; }
		public IValidateHuman ValidateHuman { get; set; }
		public string ValidationValue { get; set; }
	}

	//==================================================

	public class ResetPasswordInfo {

		public ResetPasswordInfo() {
			this.CreationResult = null;

			ReconstructSettings();
		}

		public static string Key {
			get {
				return "CMS_resetform";
			}
		}

		public void ReconstructSettings() {
			this.Settings = null;

			if (!String.IsNullOrEmpty(this.EncodedSettings)) {
				string sXML = CMSConfigHelper.DecodeBase64(this.EncodedSettings);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(ResetPasswordInfoSettings));
				using (StringReader stringReader = new StringReader(sXML)) {
					this.Settings = (ResetPasswordInfoSettings)xmlSerializer.Deserialize(stringReader);
				}

				if (this.Settings != null && !String.IsNullOrEmpty(this.Settings.ValidateHumanClass)) {
					Type objType = ReflectionUtilities.GetTypeFromString(this.Settings.ValidateHumanClass);
					Object obj = Activator.CreateInstance(objType);
					this.ValidateHuman = (IValidateHuman)obj;
					this.ValidateHuman.AltValidationFailText = this.Settings.ValidationFailText;
				}
			}
		}

		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		[StringLength(128)]
		public string Email { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		public string EncodedSettings { get; set; }
		public ResetPasswordInfoSettings Settings { get; set; }
		public IValidateHuman ValidateHuman { get; set; }
		public string ValidationValue { get; set; }

		public IdentityResult CreationResult { get; set; }
	}

	//==================================================

	public class ChangePasswordInfo {

		public ChangePasswordInfo() {
			ReconstructSettings();
		}

		public static string Key {
			get {
				return "CMS_chngpassform";
			}
		}

		public void ReconstructSettings() {
			this.Settings = null;

			if (!String.IsNullOrEmpty(this.EncodedSettings)) {
				string sXML = CMSConfigHelper.DecodeBase64(this.EncodedSettings);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(ChangePasswordInfoSettings));
				using (StringReader stringReader = new StringReader(sXML)) {
					this.Settings = (ChangePasswordInfoSettings)xmlSerializer.Deserialize(stringReader);
				}

				if (this.Settings != null && !String.IsNullOrEmpty(this.Settings.ValidateHumanClass)) {
					Type objType = ReflectionUtilities.GetTypeFromString(this.Settings.ValidateHumanClass);
					Object obj = Activator.CreateInstance(objType);
					this.ValidateHuman = (IValidateHuman)obj;
					this.ValidateHuman.AltValidationFailText = this.Settings.ValidationFailText;
				}
			}
		}

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Current password")]
		public string OldPassword { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "New password")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password")]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		public string EncodedSettings { get; set; }
		public ChangePasswordInfoSettings Settings { get; set; }
		public IValidateHuman ValidateHuman { get; set; }
		public string ValidationValue { get; set; }
	}

	//==================================================

	public class ChangeProfileInfo {

		public ChangeProfileInfo() {
			ReconstructSettings();
		}

		public static string Key {
			get {
				return "CMS_chngproform";
			}
		}

		public void ReconstructSettings() {
			this.Settings = null;

			if (!String.IsNullOrEmpty(this.EncodedSettings)) {
				string sXML = CMSConfigHelper.DecodeBase64(this.EncodedSettings);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(ChangeProfileInfoSettings));
				using (StringReader stringReader = new StringReader(sXML)) {
					this.Settings = (ChangeProfileInfoSettings)xmlSerializer.Deserialize(stringReader);
				}

				if (this.Settings != null && !String.IsNullOrEmpty(this.Settings.ValidateHumanClass)) {
					Type objType = ReflectionUtilities.GetTypeFromString(this.Settings.ValidateHumanClass);
					Object obj = Activator.CreateInstance(objType);
					this.ValidateHuman = (IValidateHuman)obj;
					this.ValidateHuman.AltValidationFailText = this.Settings.ValidationFailText;
				}
			}
		}

		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		[StringLength(128)]
		public string Email { get; set; }

		[Display(Name = "User nickname")]
		[StringLength(64)]
		public string UserNickName { get; set; }

		[Display(Name = "First name")]
		[StringLength(64)]
		public string FirstName { get; set; }

		[Display(Name = "Last name")]
		[StringLength(64)]
		public string LastName { get; set; }

		public string EncodedSettings { get; set; }
		public ChangeProfileInfoSettings Settings { get; set; }
		public IValidateHuman ValidateHuman { get; set; }
		public string ValidationValue { get; set; }
	}
}