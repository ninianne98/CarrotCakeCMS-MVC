using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

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

	public class ContentSnippetText : BaseToolboxComponent {

		public ContentSnippetText() {
			this.SnippetSlug = string.Empty;
			this.SnippetID = Guid.Empty;
		}

		public string SnippetSlug { get; set; }

		[Description("Select a content snippet to display in the widget area")]
		[Widget(WidgetAttribute.FieldMode.DropDownList, "lstSnippetID")]
		public Guid SnippetID { get; set; }

		[Widget(WidgetAttribute.FieldMode.DictionaryList)]
		public Dictionary<string, string> lstSnippetID {
			get {
				if (SiteID == Guid.Empty) {
					SiteID = SiteData.CurrentSiteID;
				}
				Dictionary<string, string> _dict = (from c in SiteData.CurrentSite.GetContentSnippetList()
													orderby c.ContentSnippetName
													where c.SiteID == SiteID
													select c).ToList().ToDictionary(k => k.Root_ContentSnippetID.ToString(),
													v => String.Format("{0} - {1} ({2})", v.ContentSnippetSlug, v.ContentSnippetName, (v.ContentSnippetActive ? "active" : "inactive")));
				return _dict;
			}
		}

		public override void LoadData() {
			base.LoadData();

			if (this.PublicParmValues.Any()) {
				this.SnippetID = new Guid(GetParmValue("SnippetID", Guid.Empty.ToString()));
			}
		}

		public override bool EnableEdit {
			get {
				return true;
			}
		}

		public override string ToHtmlString() {
			LoadData();

			StringBuilder sb = new StringBuilder();

			string sBody = string.Empty;

			ContentSnippet cs = null;

			try {
				bool bIsEditor = SecurityData.IsAdmin || SecurityData.IsSiteEditor;

				if (this.SnippetID != Guid.Empty) {
					cs = ContentSnippet.GetSnippetByID(SiteData.CurrentSiteID, this.SnippetID, !bIsEditor);
				} else {
					cs = ContentSnippet.GetSnippetBySlug(SiteData.CurrentSiteID, this.SnippetSlug, !bIsEditor);
				}

				string sBodyNote = string.Empty;
				string sIdent = string.Empty;

				if (cs != null) {
					if (bIsEditor && (cs.IsRetired || cs.IsUnReleased || !cs.ContentSnippetActive)) {
						string sBodyFlags = string.Empty;
						if (!cs.ContentSnippetActive) {
							sBodyFlags += CMSConfigHelper.InactivePagePrefix + " - Status : " + cs.ContentSnippetActive.ToString() + " ";
						}
						if (cs.IsRetired) {
							sBodyFlags += CMSConfigHelper.RetiredPagePrefix + " - Retired : " + cs.RetireDate.ToString() + " ";
						}
						if (cs.IsUnReleased) {
							sBodyFlags += CMSConfigHelper.UnreleasedPagePrefix + " - Unreleased : " + cs.GoLiveDate.ToString() + " ";
						}

						if (SecurityData.AdvancedEditMode) {
							sBodyNote = "<div class=\"cmsSnippetOuter\"> <div class=\"cmsSnippetInner\">\r\n" + cs.ContentSnippetSlug + ": " + sBodyFlags.Trim() + "\r\n<br style=\"clear: both;\" /></div></div>";
						} else {
							sBodyNote = "<div>\r\n" + cs.ContentSnippetSlug + ": " + sBodyFlags.Trim() + "\r\n<br style=\"clear: both;\" /></div>";
						}
					}

					if (SecurityData.AdvancedEditMode) {
						sIdent = "<div class=\"cmsSnippetOuter\"> <div class=\"cmsSnippetInner\">\r\n" + cs.ContentSnippetSlug + ": " + cs.ContentSnippetName + "\r\n<br style=\"clear: both;\" /></div></div>";
					}

					sBody = String.Format("{0}\r\n{1}\r\n{2}", sIdent, cs.ContentBody, sBodyNote);
				}
			} catch {
				if (!SiteData.IsWebView) {
					if (this.SnippetID != Guid.Empty) {
						sBody = this.SnippetID.ToString();
					} else {
						sBody = this.SnippetSlug;
					}
				}
			}

			sBody = SiteData.CurrentSite.UpdateContentSnippet(sBody);

			sb.AppendLine();
			sb.Append(sBody);
			sb.AppendLine();

			return sb.ToString();
		}
	}
}