using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

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

	public class MostRecentUpdated : SimpleList {

		public MostRecentUpdated()
			: base() {
			this.ContentType = ListContentType.Blog;
			this.TakeTop = 5;
			this.ShowUpdateDate = false;
			this.DateFormat = "({0:d})";

			this.CategoryGuidList = new List<Guid>();
			this.CategorySlugList = new List<string>();
		}

		public enum ListContentType {
			Unknown,
			Blog,
			ContentPage,
			SpecifiedCategories
		}

		[Widget(WidgetAttribute.FieldMode.DictionaryList)]
		public Dictionary<string, string> lstContentType {
			get {
				Dictionary<string, string> _dict = new Dictionary<string, string>();
				_dict.Add("Blog", "Blog");
				_dict.Add("ContentPage", "Content Page");
				_dict.Add("SpecifiedCategories", "Specified Categories");
				return _dict;
			}
		}

		[Widget(WidgetAttribute.FieldMode.DropDownList, "lstContentType")]
		public ListContentType ContentType { get; set; }

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public int TakeTop { get; set; }

		public List<Guid> CategoryGuidList { get; set; }

		public List<string> CategorySlugList { get; set; }

		private List<Guid> _guids = null;

		[Widget(WidgetAttribute.FieldMode.CheckBoxList, "lstCategories")]
		public List<Guid> SelectedCategories {
			get {
				if (_guids == null) {
					if (CategoryGuidList.Any()) {
						_guids = (from n in CategoryGuidList select n).ToList();
					} else {
						_guids = new List<Guid>();
					}
				}
				return _guids;
			}
			set {
				_guids = value;
			}
		}

		private List<string> _slugs = null;

		public List<string> SelectedCategorySlugs {
			get {
				if (_slugs == null) {
					if (CategorySlugList.Any()) {
						_slugs = (from n in CategorySlugList select n).ToList();
					} else {
						_slugs = new List<string>();
					}
				}
				return _slugs;
			}
			set {
				_slugs = value;
			}
		}

		[Widget(WidgetAttribute.FieldMode.DictionaryList)]
		public Dictionary<string, string> lstCategories {
			get {
				Dictionary<string, string> _dict = (from c in SiteData.CurrentSite.GetCategoryList()
													orderby c.CategoryText
													where c.SiteID == SiteData.CurrentSiteID
													select c).ToList().ToDictionary(k => k.ContentCategoryID.ToString(), v => v.CategoryText + " (" + v.CategorySlug + ")");

				return _dict;
			}
		}

		public bool ShowUpdateDate { get; set; }

		public string DateFormat { get; set; }

		protected List<SiteNav> GetUpdates() {
			List<SiteNav> lst = new List<SiteNav>();

			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				switch (this.ContentType) {
					case ListContentType.Blog:
						lst = navHelper.GetLatestPosts(SiteData.CurrentSiteID, this.TakeTop, !SecurityData.IsAuthEditor);
						break;

					case ListContentType.ContentPage:
						lst = navHelper.GetLatest(SiteData.CurrentSiteID, this.TakeTop, !SecurityData.IsAuthEditor);
						break;

					case ListContentType.SpecifiedCategories:
						if (this.TakeTop > 0) {
							lst = navHelper.GetFilteredContentByIDPagedList(SiteData.CurrentSite, this.SelectedCategories, this.SelectedCategorySlugs, !SecurityData.IsAuthEditor, this.TakeTop, 0, "GoLiveDate", "DESC");
						} else {
							lst = navHelper.GetFilteredContentByIDPagedList(SiteData.CurrentSite, this.SelectedCategories, this.SelectedCategorySlugs, !SecurityData.IsAuthEditor, 3200000, 0, "NavMenuText", "ASC");
						}
						break;
				}

				if (this.ShowUpdateDate && !String.IsNullOrEmpty(this.DateFormat)) {
					lst.ForEach(x => x.NavMenuText = String.Format("{0}  {1}", x.NavMenuText, String.Format(this.DateFormat, x.GoLiveDate)));
				}
			}

			return lst;
		}

		public override void LoadData() {
			base.LoadData();

			try {
				if (this.PublicParmValues.Any()) {
					this.TakeTop = int.Parse(base.GetParmValue("TakeTop", "5"));

					this.ContentType = (ListContentType)Enum.Parse(typeof(ListContentType), base.GetParmValue("ContentType", "Blog"), true);

					this.SelectedCategories = new List<Guid>();

					List<string> lstCategories = base.GetParmValueList("SelectedCategories");
					foreach (string sCat in lstCategories) {
						if (!String.IsNullOrEmpty(sCat)) {
							this.SelectedCategories.Add(new Guid(sCat));
						}
					}
				}

				if (this.SelectedCategories.Any()) {
					this.ContentType = ListContentType.SpecifiedCategories;
				}
			} catch (Exception ex) { }

			this.NavigationData = GetUpdates();
		}
	}
}