using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Web;

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

	public static class EmailHelper {

		private static Version CurrentDLLVersion {
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}

		public static bool SendMail(string fromEmail, string emailTo, string subjectLine, string bodyText, bool isHTML) {
			var lst = new List<string>();
			lst.Add(emailTo);

			return SendMail(fromEmail, lst, null, subjectLine, bodyText, isHTML, null);
		}

		public static bool SendMail(string fromEmail, List<string> emailTo, List<string> emailCC, string subjectLine,
							string bodyText, bool isHTML, List<string> attachments) {
			HttpContext context = HttpContext.Current;
			EMailSettings mailSettings = EMailSettings.GetEMailSettings();

			if (String.IsNullOrEmpty(fromEmail)) {
				fromEmail = mailSettings.ReturnAddress;
			}

			if (emailTo != null && emailTo.Any()) {
				MailMessage message = new MailMessage {
					From = new MailAddress(fromEmail),
					Subject = subjectLine,
					Body = bodyText,
					IsBodyHtml = isHTML
				};

				message.Headers.Add("X-Computer", Environment.MachineName);
				message.Headers.Add("X-Originating-IP", context.Request.ServerVariables["REMOTE_ADDR"].ToString());
				message.Headers.Add("X-Application", "Carrotware Web " + CurrentDLLVersion);
				message.Headers.Add("User-Agent", "Carrotware Web " + CurrentDLLVersion);
				message.Headers.Add("Message-ID", "<" + Guid.NewGuid().ToString().ToLower() + "@" + mailSettings.MailDomainName + ">");

				foreach (var t in emailTo) {
					message.To.Add(new MailAddress(t));
				}

				if (emailCC != null) {
					foreach (var t in emailCC) {
						message.CC.Add(new MailAddress(t));
					}
				}

				if (attachments != null) {
					foreach (var f in attachments) {
						Attachment a = new Attachment(f, MediaTypeNames.Application.Octet);
						ContentDisposition disp = a.ContentDisposition;
						disp.CreationDate = System.IO.File.GetCreationTime(f);
						disp.ModificationDate = System.IO.File.GetLastWriteTime(f);
						disp.ReadDate = System.IO.File.GetLastAccessTime(f);
						message.Attachments.Add(a);
					}
				}

				using (SmtpClient client = new SmtpClient()) {
					if (mailSettings.DeliveryMethod == SmtpDeliveryMethod.Network
							&& !String.IsNullOrEmpty(mailSettings.MailUserName)
							&& !String.IsNullOrEmpty(mailSettings.MailPassword)) {
						client.Host = mailSettings.MailDomainName;
						client.Credentials = new NetworkCredential(mailSettings.MailUserName, mailSettings.MailPassword);
					} else {
						client.Credentials = new NetworkCredential();
					}

					client.Send(message);
				}
			}

			return true;
		}
	}
}