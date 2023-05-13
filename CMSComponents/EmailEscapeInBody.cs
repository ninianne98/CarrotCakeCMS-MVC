using Carrotware.CMS.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

	public class EmailEscapeInBody : ITextBodyUpdate {

		#region ITextBodyUpdate Members

		public string UpdateContent(string TextContent) {
			if (!string.IsNullOrEmpty(TextContent)) {
				List<EmailReplace> _mail = FindEmails(TextContent);

				if (_mail.Any()) {
					StringBuilder sb = new StringBuilder(TextContent);
					foreach (EmailReplace m in _mail.OrderByDescending(x => x.Length)) {
						sb.Replace(m.Email, m.MungedEmail);
					}
					TextContent = sb.ToString();
				}
			}

			return TextContent;
		}

		public string UpdateContentPlainText(string TextContent) {
			return UpdateContent(TextContent);
		}

		public string UpdateContentRichText(string TextContent) {
			return UpdateContent(TextContent);
		}

		public string UpdateContentComment(string TextContent) {
			return UpdateContent(TextContent);
		}

		public string UpdateContentSnippet(string TextContent) {
			return UpdateContent(TextContent);
		}

		protected string MungeEmail(string s) {
			string retVal = "";

			if (!string.IsNullOrEmpty(s)) {
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < s.Length; i++) {
					sb.Append("&#" + Convert.ToByte(s[i]).ToString() + ";");
				}
				retVal = sb.ToString();
			}

			return retVal;
		}

		protected List<EmailReplace> FindEmails(string strIn) {
			string MailRegExString = @"([a-zA-Z_0-9.-]+\@[a-zA-Z_0-9.-]+\.\w+)";

			Regex mailRegEx = new Regex(MailRegExString, RegexOptions.Compiled);

			List<EmailReplace> _mail = (from c in mailRegEx.Matches(strIn).Cast<Match>().Select(x => x.Value).Distinct().ToList()
										select new EmailReplace {
											Email = c,
											Length = c.Length,
											MungedEmail = MungeEmail(c),
										}).ToList();
			return _mail;
		}

		//==================
		public class EmailReplace {
			public string Email { get; set; }
			public string MungedEmail { get; set; }
			public int Length { get; set; }
		}

		#endregion ITextBodyUpdate Members
	}
}