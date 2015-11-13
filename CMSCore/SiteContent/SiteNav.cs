using Carrotware.CMS.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Web.Mvc;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Core {

	public class SiteNav : ISiteContent {

		public SiteNav() { }

		internal SiteNav(vw_carrot_Content c) {
			if (c != null) {
				SiteData site = SiteData.GetSiteFromCache(c.SiteID);

				this.Root_ContentID = c.Root_ContentID;
				this.SiteID = c.SiteID;
				this.FileName = c.FileName;
				this.Thumbnail = c.PageThumbnail;
				this.ShowInSiteMap = c.ShowInSiteMap;
				this.BlockIndex = c.BlockIndex;
				this.PageActive = c.PageActive;
				this.CreateDate = site.ConvertUTCToSiteTime(c.CreateDate);
				this.GoLiveDate = site.ConvertUTCToSiteTime(c.GoLiveDate);
				this.RetireDate = site.ConvertUTCToSiteTime(c.RetireDate);
				this.EditDate = site.ConvertUTCToSiteTime(c.EditDate);
				this.EditUserId = c.EditUserId;
				this.CreditUserId = c.CreditUserId;
				this.ShowInSiteNav = c.ShowInSiteNav;
				this.CreateUserId = c.CreateUserId;
				this.ContentType = ContentPageType.GetTypeByID(c.ContentTypeID);
				this.ContentID = c.ContentID;
				this.Parent_ContentID = c.Parent_ContentID;
				this.TitleBar = c.TitleBar;
				this.NavMenuText = c.NavMenuText;
				this.PageHead = c.PageHead;
				this.PageText = c.PageText;
				this.NavOrder = c.NavOrder;
				this.TemplateFile = c.TemplateFile;
			}
		}

		public ContentPage GetContentPage() {
			ContentPage cp = null;
			if (SiteData.IsPageSampler) {
				cp = ContentPageHelper.GetSamplerView();
			} else {
				using (ContentPageHelper cph = new ContentPageHelper()) {
					cp = cph.FindContentByID(this.SiteID, this.Root_ContentID);
				}
			}
			return cp;
		}

		public MvcHtmlString PageTextPlainSummaryMedium {
			get {
				string txt = this.PageText ?? String.Empty;
				txt = txt.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("&nbsp;", " ").Replace('\u00A0', ' '); //.Replace(".", "&#46;").Replace("@", " &#40;&#97;&#116;&#41; ");

				txt = Regex.Replace(txt, @"<!--(\n|.)*-->", " ");
				txt = Regex.Replace(txt, @"<(.|\n)*?>", " ");
				txt = txt.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ").Replace("  ", " ");

				txt = SiteData.CurrentSite.UpdateContent(txt);

				if (txt.Length > 4096) {
					txt = txt.Substring(0, 4096);
				}

				if (txt.Length > 800) {
					txt = txt.Substring(0, 768).Trim() + "[.....]";
				}

				return new MvcHtmlString(txt);
			}
		}

		public MvcHtmlString PageTextPlainSummary {
			get {
				string txt = this.PageTextPlainSummaryMedium.ToString();

				if (txt.Length > 300) {
					txt = txt.Substring(0, 256).Trim() + "[.....]";
				}

				return new MvcHtmlString(txt);
			}
		}

		public MvcHtmlString NavigationText { get { return new MvcHtmlString(this.NavMenuText); } }
		public MvcHtmlString HeadingText { get { return new MvcHtmlString(this.PageHead); } }

		public Guid ContentID { get; set; }
		public DateTime EditDate { get; set; }
		public Guid? EditUserId { get; set; }
		public Guid? CreditUserId { get; set; }
		public Guid CreateUserId { get; set; }
		public string TemplateFile { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime GoLiveDate { get; set; }
		public DateTime RetireDate { get; set; }
		public int NavOrder { get; set; }

		[Display(Name = "File Name")]
		public string FileName { get; set; }

		[Display(Name = "Heading")]
		public string PageHead { get; set; }

		[Display(Name = "Title")]
		public string TitleBar { get; set; }

		[Display(Name = "Nav Text")]
		public string NavMenuText { get; set; }

		[Display(Name = "Body")]
		public string PageText { get; set; }

		public Guid? Parent_ContentID { get; set; }
		public Guid Root_ContentID { get; set; }
		public string Thumbnail { get; set; }

		[Display(Name = "In Site Map")]
		public bool ShowInSiteMap { get; set; }

		[Display(Name = "Block")]
		public bool BlockIndex { get; set; }

		[Display(Name = "Active")]
		public bool PageActive { get; set; }

		[Display(Name = "In Site Nav")]
		public bool ShowInSiteNav { get; set; }

		public Guid SiteID { get; set; }

		public ContentPageType.PageType ContentType { get; set; }

		[Display(Name = "Retired")]
		public bool IsRetired {
			get {
				if (SiteData.IsWebView && SiteData.CurretSiteExists) {
					return this.RetireDate <= SiteData.CurrentSite.Now;
				} else {
					return this.RetireDate <= DateTime.UtcNow;
				}
			}
		}

		[Display(Name = "Unreleased")]
		public bool IsUnReleased {
			get {
				if (SiteData.IsWebView && SiteData.CurretSiteExists) {
					return this.GoLiveDate >= SiteData.CurrentSite.Now;
				} else {
					return this.GoLiveDate >= DateTime.UtcNow;
				}
			}
		}

		private int _commentCount = -1;

		public int CommentCount {
			get {
				if (_commentCount < 0) {
					_commentCount = PostComment.GetCommentCountByContent(this.Root_ContentID, !SecurityData.IsAuthEditor);
				}
				return _commentCount;
			}
			set {
				_commentCount = value;
			}
		}

		private List<ContentTag> _contentTags = null;

		public List<ContentTag> ContentTags {
			get {
				if (_contentTags == null) {
					_contentTags = ContentTag.BuildTagList(this.Root_ContentID);
				}
				return _contentTags;
			}
			set {
				_contentTags = value;
			}
		}

		private List<ContentCategory> _ContentCategories = null;

		public List<ContentCategory> ContentCategories {
			get {
				if (_ContentCategories == null) {
					_ContentCategories = ContentCategory.BuildCategoryList(this.Root_ContentID);
				}
				return _ContentCategories;
			}
			set {
				_ContentCategories = value;
			}
		}

		private ExtendedUserData _user = null;

		public ExtendedUserData GetUserInfo() {
			if (_user == null && this.EditUserId.HasValue) {
				_user = new ExtendedUserData(this.EditUserId.Value);
			}
			return _user;
		}

		private ExtendedUserData _creditUser = null;

		public ExtendedUserData GetCreditUserInfo() {
			if (_creditUser == null && this.CreditUserId.HasValue) {
				_creditUser = new ExtendedUserData(this.CreditUserId.Value);
			}
			return _creditUser;
		}

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is SiteNav) {
				SiteNav p = (SiteNav)obj;
				return (this.ContentID == p.ContentID)
						&& (this.SiteID == p.SiteID)
						&& (this.Root_ContentID == p.Root_ContentID);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return this.ContentID.GetHashCode() ^ this.SiteID.GetHashCode() ^ this.Root_ContentID.GetHashCode();
		}
	}
}